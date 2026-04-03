using UnityEngine;
using INTIFALL.AI;
using INTIFALL.Environment;
using INTIFALL.Input;
using INTIFALL.System;

namespace INTIFALL.Player
{
    public enum ECQCAction
    {
        None,
        MeleeAttack,
        Backstab,
        RopeKill,
        SleepDart
    }

    public struct CQCActionExecutedEvent
    {
        public ECQCAction actionType;
        public bool success;
        public int targetId;
    }

    public class CQCSystem : MonoBehaviour
    {
        [Header("Melee Settings")]
        [SerializeField] private float meleeRange = 2f;
        [SerializeField] private float meleeCastTime = 0.5f;
        [SerializeField] private int meleeDamage = 1;

        [Header("Backstab Settings")]
        [SerializeField] private float backstabRange = 2f;
        [SerializeField] private float backstabCastTime = 1.0f;
        [SerializeField] private float backstabStunDuration = 30f;
        [SerializeField] private float backstabAngleThreshold = 135f;
        [SerializeField] private bool interruptOnMove = true;

        [Header("Rope Kill Settings")]
        [SerializeField] private float ropeKillRange = 2f;
        [SerializeField] private float ropeKillCastTime = 0.3f;
        [SerializeField] private float ropeKillSleepDuration = 20f;

        [Header("Sleep Dart Settings")]
        [SerializeField] private float sleepDartRange = 15f;
        [SerializeField] private float sleepDartCastTime = 0.3f;
        [SerializeField] private float sleepDartSleepDuration = 20f;

        [Header("References")]
        [SerializeField] private Transform eyes;
        [SerializeField] private LayerMask enemyLayer;

        private float _currentActionTimer;
        private ECQCAction _currentAction;
        private bool _isExecutingAction;
        private Transform _currentTarget;
        private bool _isInsideVent;
        private PlayerController _playerController;

        public bool IsExecutingAction => _isExecutingAction;
        public ECQCAction CurrentAction => _currentAction;
        public float ActionProgress => _currentActionTimer;

        private void EnsureReferences()
        {
            if (eyes == null)
                eyes = transform;
        }

        private void Awake()
        {
            EnsureReferences();
            _playerController = GetComponent<PlayerController>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<VentEntrance.VentEnteredEvent>(OnVentEntered);
            EventBus.Subscribe<VentEntrance.VentExitedEvent>(OnVentExited);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<VentEntrance.VentEnteredEvent>(OnVentEntered);
            EventBus.Unsubscribe<VentEntrance.VentExitedEvent>(OnVentExited);
        }

        private void Update()
        {
            if (_isExecutingAction)
            {
                UpdateActionExecution();
                return;
            }

            if (_isInsideVent)
                return;
            if (_playerController != null && _playerController.IsOnRope)
                return;

            if (InputCompat.GetKeyDown(KeyCode.Mouse0))
            {
                TryMeleeAttack();
            }
            else if (InputCompat.GetKeyDown(KeyCode.Q))
            {
                TryBackstab();
            }
        }

        private void UpdateActionExecution()
        {
            _currentActionTimer += Time.deltaTime;

            float castTime = GetCastTimeForAction(_currentAction);
            if (_currentActionTimer >= castTime)
            {
                CompleteAction();
            }
        }

        private float GetCastTimeForAction(ECQCAction action)
        {
            return action switch
            {
                ECQCAction.MeleeAttack => meleeCastTime,
                ECQCAction.Backstab => backstabCastTime,
                ECQCAction.RopeKill => ropeKillCastTime,
                ECQCAction.SleepDart => sleepDartCastTime,
                _ => 0f
            };
        }

        public bool TryMeleeAttack()
        {
            EnsureReferences();
            if (_isInsideVent) return false;
            if (_playerController != null && _playerController.IsOnRope) return false;
            if (_isExecutingAction) return false;

            Collider[] hits = Physics.OverlapSphere(eyes.position, meleeRange, enemyLayer);
            if (hits.Length == 0) return false;

            Transform nearest = GetNearestEnemy(hits);
            if (nearest == null) return false;

            StartAction(ECQCAction.MeleeAttack, nearest);
            return true;
        }

        public bool TryBackstab()
        {
            EnsureReferences();
            if (_isInsideVent) return false;
            if (_playerController != null && _playerController.IsOnRope) return false;
            if (_isExecutingAction) return false;

            Collider[] hits = Physics.OverlapSphere(eyes.position, backstabRange, enemyLayer);
            if (hits.Length == 0) return false;

            foreach (var hit in hits)
            {
                if (IsBehindTarget(hit.transform))
                {
                    StartAction(ECQCAction.Backstab, hit.transform);
                    return true;
                }
            }

            return false;
        }

        public bool TryRopeKill(Transform ropePoint)
        {
            if (_isInsideVent) return false;
            if (_isExecutingAction) return false;

            Collider[] hits = Physics.OverlapSphere(ropePoint.position, ropeKillRange, enemyLayer);
            if (hits.Length == 0) return false;

            Transform nearest = GetNearestEnemy(hits);
            if (nearest == null) return false;

            StartAction(ECQCAction.RopeKill, nearest);
            return true;
        }

        public bool TrySleepDart()
        {
            EnsureReferences();
            if (_isInsideVent) return false;
            if (_playerController != null && _playerController.IsOnRope) return false;
            if (_isExecutingAction) return false;

            Ray ray = new Ray(eyes.position, eyes.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, sleepDartRange, enemyLayer))
            {
                StartAction(ECQCAction.SleepDart, hit.transform);
                return true;
            }

            return false;
        }

        private void StartAction(ECQCAction action, Transform target)
        {
            _currentAction = action;
            _currentTarget = target;
            _isExecutingAction = true;
            _currentActionTimer = 0f;
        }

        private void CompleteAction()
        {
            if (_currentTarget == null)
            {
                CancelAction();
                return;
            }

            bool success = false;

            switch (_currentAction)
            {
                case ECQCAction.MeleeAttack:
                    success = ExecuteMeleeAttack();
                    break;
                case ECQCAction.Backstab:
                    success = ExecuteBackstab();
                    break;
                case ECQCAction.RopeKill:
                    success = ExecuteRopeKill();
                    break;
                case ECQCAction.SleepDart:
                    success = ExecuteSleepDart();
                    break;
            }

            EventBus.Publish(new CQCActionExecutedEvent
            {
                actionType = _currentAction,
                success = success,
                targetId = _currentTarget != null ? _currentTarget.gameObject.GetInstanceID() : 0
            });

            EndAction();
        }

        private bool ExecuteMeleeAttack()
        {
            if (_currentTarget == null) return false;

            var enemy = _currentTarget.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(meleeDamage);
                return true;
            }
            return false;
        }

        private bool ExecuteBackstab()
        {
            if (_currentTarget == null) return false;

            var enemy = _currentTarget.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(0);
                enemy.ApplySleepEffect(backstabStunDuration);
                return true;
            }
            return false;
        }

        private bool ExecuteRopeKill()
        {
            if (_currentTarget == null) return false;

            var enemy = _currentTarget.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(0);
                enemy.ApplySleepEffect(ropeKillSleepDuration);
                return true;
            }
            return false;
        }

        private bool ExecuteSleepDart()
        {
            if (_currentTarget == null) return false;

            var enemy = _currentTarget.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.ApplySleepEffect(sleepDartSleepDuration);
                return true;
            }
            return false;
        }

        private void CancelAction()
        {
            EndAction();

            EventBus.Publish(new CQCActionExecutedEvent
            {
                actionType = _currentAction,
                success = false,
                targetId = 0
            });
        }

        private void EndAction()
        {
            _isExecutingAction = false;
            _currentAction = ECQCAction.None;
            _currentTarget = null;
            _currentActionTimer = 0f;
        }

        private Transform GetNearestEnemy(Collider[] hits)
        {
            Transform nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                float dist = Vector3.Distance(eyes.position, hit.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = hit.transform;
                }
            }

            return nearest;
        }

        private bool IsBehindTarget(Transform target)
        {
            Vector3 toTarget = target.position - transform.position;
            float angle = Vector3.Angle(transform.forward, toTarget);
            return angle > backstabAngleThreshold;
        }

        public void OnPlayerMove()
        {
            if (_isExecutingAction && _currentAction == ECQCAction.Backstab && interruptOnMove)
            {
                CancelAction();
            }
        }

        private void OnVentEntered(VentEntrance.VentEnteredEvent evt)
        {
            _isInsideVent = true;
            if (_isExecutingAction)
                CancelAction();
        }

        private void OnVentExited(VentEntrance.VentExitedEvent evt)
        {
            _isInsideVent = false;
        }
    }
}
