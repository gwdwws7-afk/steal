using UnityEngine;
using INTIFALL.System;

namespace INTIFALL.AI
{
    public enum EEnemyType
    {
        Normal,
        Reinforced,
        Heavy,
        Quipucamayoc,
        Saqueos
    }

    [RequireComponent(typeof(EnemyStateMachine))]
    [RequireComponent(typeof(CharacterController))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Enemy Type")]
        [SerializeField] private EEnemyType enemyType = EEnemyType.Normal;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 2.5f;
        [SerializeField] private float runSpeed = 4.5f;

        [Header("Combat")]
        [SerializeField] private int hp = 1;
        [SerializeField] private int damage = 1;
        [SerializeField] private float attackRange = 3f;
        [SerializeField] private float attackCooldown = 1.5f;

        [Header("Patrol")]
        [SerializeField] private PatrolRoute patrolRoute;
        [SerializeField] private bool autoPatrol = true;

        [Header("References")]
        [SerializeField] private PerceptionModule perception;
        [SerializeField] private Transform eyes;

        private EnemyStateMachine _stateMachine;
        private CharacterController _cc;
        private Vector3 _moveTarget;
        private Vector3 _lookTarget;
        private float _lastAttackTime;
        private int _currentHp;
        private bool _isDead;
        private float _currentSpeed;

        public EEnemyType EnemyType => enemyType;
        public EnemyStateMachine StateMachine => _stateMachine;
        public bool IsDead => _isDead;

        private void Awake()
        {
            _stateMachine = GetComponent<EnemyStateMachine>();
            _cc = GetComponent<CharacterController>();
            _currentHp = hp;
            _currentSpeed = walkSpeed;
        }

        private void Start()
        {
            if (eyes == null)
                eyes = transform;
        }

        private void Update()
        {
            if (_isDead) return;

            UpdateBehavior();
        }

        private void UpdateBehavior()
        {
            switch (_stateMachine.CurrentState)
            {
                case EEnemyState.Unaware:
                    HandleUnaware();
                    break;
                case EEnemyState.Suspicious:
                    HandleSuspicious();
                    break;
                case EEnemyState.Searching:
                    HandleSearching();
                    break;
                case EEnemyState.Alert:
                    HandleAlert();
                    break;
                case EEnemyState.FullAlert:
                    HandleFullAlert();
                    break;
            }
        }

        private void HandleUnaware()
        {
            if (autoPatrol && patrolRoute != null)
            {
                Patrol();
            }
        }

        private void HandleSuspicious()
        {
            _currentSpeed = walkSpeed;
            LookAt(_stateMachine.LastKnownPlayerPos);
        }

        private void HandleSearching()
        {
            _currentSpeed = walkSpeed;
            MoveTo(_stateMachine.LastKnownPlayerPos);
            LookAt(_stateMachine.LastKnownPlayerPos);
        }

        private void HandleAlert()
        {
            _currentSpeed = runSpeed;

            if (perception.CanSeeTarget())
            {
                _moveTarget = perception.GetTargetPosition();
                MoveTo(_moveTarget);
                LookAt(_moveTarget);

                if (Vector3.Distance(transform.position, _moveTarget) <= attackRange)
                {
                    TryAttack();
                }
            }
            else
            {
                MoveTo(_stateMachine.LastKnownPlayerPos);
            }
        }

        private void HandleFullAlert()
        {
            _currentSpeed = runSpeed;

            if (perception.CanSeeTarget())
            {
                _moveTarget = perception.GetTargetPosition();
                MoveTo(_moveTarget);
                LookAt(_moveTarget);

                TryAttack();

                EventBus.Publish(new AlertStateChangedEvent
                {
                    enemyId = gameObject.GetInstanceID(),
                    newState = EAlertState.FullAlert
                });
            }
            else
            {
                _stateMachine.TransitionTo(EEnemyState.Searching);
            }
        }

        private void Patrol()
        {
            if (patrolRoute == null || !patrolRoute.HasWaypoints()) return;

            Vector3 nextWaypoint = patrolRoute.GetCurrentWaypoint();
            float distanceToWaypoint = Vector3.Distance(transform.position, nextWaypoint);

            if (distanceToWaypoint < 1f)
            {
                patrolRoute.AdvanceToNext();
                return;
            }

            MoveTo(nextWaypoint);
            LookAt(nextWaypoint);
        }

        private void MoveTo(Vector3 target)
        {
            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0;

            _cc.Move(direction * _currentSpeed * Time.deltaTime);
        }

        private void LookAt(Vector3 target)
        {
            Vector3 direction = target - transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
            }
        }

        private void TryAttack()
        {
            if (Time.time - _lastAttackTime < attackCooldown) return;
            if (!perception.CanSeeTarget()) return;

            _lastAttackTime = Time.time;
            Attack();
        }

        private void Attack()
        {
            EventBus.Publish(new EnemyAttackedEvent
            {
                enemyId = gameObject.GetInstanceID(),
                damage = damage,
                targetPosition = perception.GetTargetPosition()
            });
        }

        public void TakeDamage(int amount)
        {
            if (_isDead) return;

            _currentHp -= amount;

            if (_currentHp <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            _isDead = true;
            _cc.enabled = false;

            EventBus.Publish(new EnemyKilledEvent
            {
                enemyId = gameObject.GetInstanceID(),
                enemyType = enemyType
            });
        }

        public void OnEnterUnaware() { }
        public void OnEnterSuspicious(Vector3 lookTarget) { _lookTarget = lookTarget; }
        public void OnEnterSearching(Vector3 searchPos) { _moveTarget = searchPos; }
        public void OnEnterAlert() { }
        public void OnEnterFullAlert() { }

        public void SetPatrolRoute(PatrolRoute route)
        {
            patrolRoute = route;
        }

        public void ApplyBlindEffect(float duration)
        {
            StartCoroutine(BlindCoroutine(duration));
        }

        private global::System.Collections.IEnumerator BlindCoroutine(float duration)
        {
            _stateMachine.TransitionTo(EEnemyState.Suspicious);
            yield return new WaitForSeconds(duration);
        }

        public void ApplySleepEffect(float duration)
        {
            if (_isDead) return;
            StartCoroutine(SleepCoroutine(duration));
        }

        private global::System.Collections.IEnumerator SleepCoroutine(float duration)
        {
            _stateMachine.TransitionTo(EEnemyState.Unaware);
            _currentSpeed = 0;
            yield return new WaitForSeconds(duration);
            _currentSpeed = walkSpeed;
        }

        public void ApplyEMPEffect(float duration)
        {
            StartCoroutine(EMPCoroutine(duration));
        }

        private global::System.Collections.IEnumerator EMPCoroutine(float duration)
        {
            if (perception != null)
                perception.enabled = false;
            yield return new WaitForSeconds(duration);
            if (perception != null)
                perception.enabled = true;
        }

        public void InvestigateSound(Vector3 soundPosition)
        {
            if (_isDead) return;
            if (_stateMachine.CurrentState == EEnemyState.Unaware)
            {
                _stateMachine.TransitionTo(EEnemyState.Suspicious);
            }
        }
    }

    public struct EnemyAttackedEvent
    {
        public int enemyId;
        public int damage;
        public Vector3 targetPosition;
    }

    public struct EnemyKilledEvent
    {
        public int enemyId;
        public EEnemyType enemyType;
    }
}
