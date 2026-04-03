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

        [Header("Detection")]
        [SerializeField] private float detectionPulseInterval = 0.75f;
        [SerializeField] private float searchingPulseMultiplier = 0.75f;
        [SerializeField] private float alertedPulseMultiplier = 0.5f;

        [Header("Squad Coordination")]
        [SerializeField] private float squadBroadcastRange = 26f;
        [SerializeField] private float squadBroadcastCooldown = 1.2f;
        [SerializeField] private float searchRadius = 6f;
        [SerializeField] private int searchSectorCount = 6;
        [SerializeField] private int maxSearchSteps = 6;
        [SerializeField] private float searchPointTolerance = 1.1f;
        [SerializeField] private float searchPointHoldDuration = 0.45f;
        [SerializeField] private float searchRetargetInterval = 0.6f;

        private EnemyStateMachine _stateMachine;
        private CharacterController _cc;
        private Vector3 _moveTarget;
        private Vector3 _lookTarget;
        private float _lastAttackTime;
        private int _currentHp;
        private bool _isDead;
        private float _currentSpeed;
        private bool _wasDetectingTarget;
        private float _lastDetectionPulseTime;
        private int _currentDetectionWaveId;
        private float _lastSquadBroadcastTime;
        private int _activeSearchWaveId = -1;
        private int _searchStep;
        private Vector3 _searchTarget;
        private bool _hasSearchTarget;
        private float _searchHoldUntil;
        private float _lastSearchRetargetTime;

        public EEnemyType EnemyType => enemyType;
        public EnemyStateMachine StateMachine => _stateMachine;
        public bool IsDead => _isDead;
        public float DetectionPulseInterval => detectionPulseInterval;
        public float SearchingPulseMultiplier => searchingPulseMultiplier;
        public float AlertedPulseMultiplier => alertedPulseMultiplier;

        private void Awake()
        {
            _stateMachine = GetComponent<EnemyStateMachine>();
            _cc = GetComponent<CharacterController>();
            _currentHp = hp;
            _currentSpeed = walkSpeed;
            _lastDetectionPulseTime = -999f;
            _lastSquadBroadcastTime = -999f;
        }

        private void Start()
        {
            if (eyes == null)
                eyes = transform;
        }

        private void OnEnable()
        {
            EnemySquadCoordinator.Register(this);
        }

        private void OnDisable()
        {
            EnemySquadCoordinator.Unregister(this);
        }

        private void OnDestroy()
        {
            EnemySquadCoordinator.Unregister(this);
        }

        private void Update()
        {
            if (_isDead) return;

            EvaluatePerception();
            UpdateBehavior();
        }

        private void EvaluatePerception()
        {
            if (perception == null || _stateMachine == null) return;

            bool canSee = perception.CanSeeTarget();
            bool canHear = !canSee && perception.CanHearCurrentTarget();
            bool detecting = canSee || canHear;
            float pulseInterval = GetDetectionPulseInterval();

            if (detecting)
            {
                bool shouldPulse = !_wasDetectingTarget || Time.time - _lastDetectionPulseTime >= pulseInterval;
                if (!_wasDetectingTarget)
                    _currentDetectionWaveId = EnemySquadCoordinator.NextWaveId();

                if (shouldPulse)
                {
                    Vector3 targetPosition = perception.GetTargetPosition();
                    _stateMachine.OnPlayerDetected(targetPosition);
                    BroadcastSquadAlert(targetPosition, canSee, _currentDetectionWaveId);
                    _lastDetectionPulseTime = Time.time;
                }

                _wasDetectingTarget = true;
                return;
            }

            if (_wasDetectingTarget)
            {
                _stateMachine.OnPlayerLost();
                _currentDetectionWaveId = 0;
            }

            _wasDetectingTarget = false;
        }

        private void BroadcastSquadAlert(Vector3 alertPosition, bool highPriority, int waveId, bool bypassCooldown = false)
        {
            if (waveId <= 0 || _isDead)
                return;

            float cooldown = Mathf.Max(0.1f, squadBroadcastCooldown);
            if (!bypassCooldown && Time.time - _lastSquadBroadcastTime < cooldown)
                return;

            EnemySquadCoordinator.BroadcastAlert(
                this,
                alertPosition,
                highPriority,
                ResolveBroadcastRange(),
                waveId);

            _lastSquadBroadcastTime = Time.time;
        }

        private float ResolveBroadcastRange()
        {
            float configuredRange = Mathf.Max(1f, squadBroadcastRange);
            if (perception == null)
                return configuredRange;

            return Mathf.Max(configuredRange, perception.CommunicationRange);
        }

        private float GetDetectionPulseInterval()
        {
            if (_stateMachine == null)
                return Mathf.Max(0.1f, detectionPulseInterval);

            float multiplier = 1f;
            switch (_stateMachine.CurrentState)
            {
                case EEnemyState.Searching:
                    multiplier = searchingPulseMultiplier;
                    break;
                case EEnemyState.Alert:
                case EEnemyState.FullAlert:
                    multiplier = alertedPulseMultiplier;
                    break;
            }

            return Mathf.Max(0.1f, detectionPulseInterval * Mathf.Max(0.1f, multiplier));
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
            RefreshSearchPlan(false);
            Vector3 target = _hasSearchTarget ? _searchTarget : _stateMachine.LastKnownPlayerPos;
            float distanceToTarget = Vector3.Distance(transform.position, target);

            if (distanceToTarget <= Mathf.Max(0.25f, searchPointTolerance))
            {
                LookAt(target);
                if (Time.time >= _searchHoldUntil)
                {
                    AdvanceSearchStep();
                }
                return;
            }

            MoveTo(target);
            LookAt(target);
        }

        private void HandleAlert()
        {
            _currentSpeed = runSpeed;

            if (perception != null && perception.CanSeeTarget())
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

            if (perception != null && perception.CanSeeTarget())
            {
                _moveTarget = perception.GetTargetPosition();
                MoveTo(_moveTarget);
                LookAt(_moveTarget);

                TryAttack();
            }
            else
            {
                Vector3 fallbackTarget = _stateMachine != null
                    ? _stateMachine.LastKnownPlayerPos
                    : transform.position;
                MoveTo(fallbackTarget);
                LookAt(fallbackTarget);
            }
        }

        private void RefreshSearchPlan(bool forceRetarget)
        {
            if (_stateMachine == null)
                return;

            int waveId = Mathf.Max(1, _stateMachine.SearchWaveId);
            if (waveId != _activeSearchWaveId)
            {
                _activeSearchWaveId = waveId;
                _searchStep = 0;
                forceRetarget = true;
            }

            float retargetInterval = Mathf.Max(0.1f, searchRetargetInterval);
            if (!forceRetarget &&
                _hasSearchTarget &&
                Time.time - _lastSearchRetargetTime < retargetInterval)
            {
                return;
            }

            Vector3 anchor = _stateMachine.SearchAnchor;
            if (anchor == Vector3.zero)
                anchor = _stateMachine.LastKnownPlayerPos;
            if (anchor == Vector3.zero)
                anchor = transform.position;

            int steps = Mathf.Max(1, maxSearchSteps);
            int boundedStep = Mathf.Clamp(_searchStep, 0, steps - 1);
            int seed = gameObject.GetInstanceID() + (_activeSearchWaveId * 17);

            _searchTarget = EnemySquadCoordinator.ComputeSearchPoint(
                anchor,
                seed,
                Mathf.Max(3, searchSectorCount),
                Mathf.Max(1f, searchRadius),
                boundedStep);

            _hasSearchTarget = true;
            _searchHoldUntil = Time.time + Mathf.Max(0f, searchPointHoldDuration);
            _lastSearchRetargetTime = Time.time;
        }

        private void AdvanceSearchStep()
        {
            int steps = Mathf.Max(1, maxSearchSteps);
            _searchStep = (_searchStep + 1) % steps;
            RefreshSearchPlan(true);
        }

        private void ClearSearchPlan()
        {
            _activeSearchWaveId = -1;
            _searchStep = 0;
            _hasSearchTarget = false;
            _searchTarget = Vector3.zero;
            _searchHoldUntil = 0f;
            _lastSearchRetargetTime = -999f;
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
            if (_cc == null || !_cc.enabled)
                return;

            Vector3 direction = target - transform.position;
            direction.y = 0;
            if (direction.sqrMagnitude <= 0.01f)
                return;

            direction.Normalize();
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
            if (perception == null || !perception.CanSeeTarget()) return;

            _lastAttackTime = Time.time;
            Attack();
        }

        private void Attack()
        {
            EventBus.Publish(new EnemyAttackedEvent
            {
                enemyId = gameObject.GetInstanceID(),
                damage = damage,
                targetPosition = perception != null ? perception.GetTargetPosition() : _stateMachine.LastKnownPlayerPos
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
            EnemySquadCoordinator.Unregister(this);

            EventBus.Publish(new EnemyKilledEvent
            {
                enemyId = gameObject.GetInstanceID(),
                enemyType = enemyType
            });
        }

        public void OnEnterUnaware()
        {
            _currentDetectionWaveId = 0;
            ClearSearchPlan();
        }

        public void OnEnterSuspicious(Vector3 lookTarget)
        {
            _lookTarget = lookTarget;
            ClearSearchPlan();
        }

        public void OnEnterSearching(Vector3 searchPos)
        {
            _moveTarget = searchPos;
            _activeSearchWaveId = -1;
            _searchStep = 0;
            _hasSearchTarget = false;
            _searchHoldUntil = 0f;
            RefreshSearchPlan(true);
        }

        public void OnEnterAlert()
        {
            ClearSearchPlan();
        }

        public void OnEnterFullAlert()
        {
            ClearSearchPlan();
        }

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
            if (_stateMachine == null) return;

            int waveId = EnemySquadCoordinator.NextWaveId();
            _stateMachine.OnSquadAlert(soundPosition, waveId, false);
            BroadcastSquadAlert(soundPosition, false, waveId, true);
        }

        public void ReceiveSquadAlert(Vector3 alertPosition, int waveId, bool highPriority)
        {
            if (_isDead || _stateMachine == null)
                return;

            _stateMachine.OnSquadAlert(alertPosition, waveId, highPriority);
        }

        public void ConfigureDetectionProfile(
            float pulseInterval,
            float searchingMultiplier,
            float alertedMultiplier,
            float broadcastCooldown = -1f)
        {
            detectionPulseInterval = Mathf.Max(0.1f, pulseInterval);
            searchingPulseMultiplier = Mathf.Max(0.1f, searchingMultiplier);
            alertedPulseMultiplier = Mathf.Max(0.1f, alertedMultiplier);

            if (broadcastCooldown >= 0f)
                squadBroadcastCooldown = Mathf.Max(0.1f, broadcastCooldown);
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
