using UnityEngine;
using INTIFALL.AI;
using INTIFALL.System;

namespace INTIFALL.Player
{
    public class CombatTrigger : MonoBehaviour
    {
        [Header("Trigger Distances")]
        [SerializeField] private float immediateCombatDistance = 3f;
        [SerializeField] private float nearbyEnemyCheckRadius = 10f;

        [Header("References")]
        [SerializeField] private LayerMask enemyLayer;

        private PlayerCombatStateMachine _combatState;
        private EnemyStateMachine[] _trackedEnemies;
        private float[] _enemyAlertTimers;
        private bool[] _enemyWasAlerted;
        private int _trackedEnemyCount;

        private void Awake()
        {
            _combatState = GetComponent<PlayerCombatStateMachine>();
        }

        private void Update()
        {
            CheckImmediateCombatTriggers();
            CheckAlertedEnemyTimers();
        }

        private void CheckImmediateCombatTriggers()
        {
            if (_combatState.IsInCombat) return;

            Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, immediateCombatDistance, enemyLayer);
            foreach (var enemy in nearbyEnemies)
            {
                var perception = enemy.GetComponent<PerceptionModule>();
                if (perception != null && perception.CanSeeTarget())
                {
                    _combatState.OnImmediateCombat();
                    return;
                }
            }
        }

        private void CheckAlertedEnemyTimers()
        {
            if (_combatState.IsInCombat) return;

            for (int i = 0; i < _trackedEnemyCount; i++)
            {
                if (_trackedEnemies[i] == null) continue;

                bool isCurrentlyAlerted = _trackedEnemies[i].CurrentState == EEnemyState.Alert ||
                                          _trackedEnemies[i].CurrentState == EEnemyState.FullAlert;

                if (isCurrentlyAlerted)
                {
                    if (!_enemyWasAlerted[i])
                    {
                        _enemyAlertTimers[i] = 0f;
                        _enemyWasAlerted[i] = true;
                    }

                    _enemyAlertTimers[i] += Time.deltaTime;

                    if (_enemyAlertTimers[i] >= 5f)
                    {
                        _combatState.OnEnemyAlerted(_enemyAlertTimers[i]);
                        return;
                    }
                }
                else
                {
                    _enemyWasAlerted[i] = false;
                    _enemyAlertTimers[i] = 0f;
                }
            }
        }

        public void TrackEnemy(EnemyStateMachine enemy)
        {
            if (enemy == null) return;

            int newIndex = _trackedEnemyCount;
            System.Array.Resize(ref _trackedEnemies, newIndex + 1);
            System.Array.Resize(ref _enemyAlertTimers, newIndex + 1);
            System.Array.Resize(ref _enemyWasAlerted, newIndex + 1);

            _trackedEnemies[newIndex] = enemy;
            _enemyAlertTimers[newIndex] = 0f;
            _enemyWasAlerted[newIndex] = false;
            _trackedEnemyCount++;
        }

        public void UntrackEnemy(EnemyStateMachine enemy)
        {
            if (enemy == null) return;

            for (int i = 0; i < _trackedEnemyCount; i++)
            {
                if (_trackedEnemies[i] == enemy)
                {
                    for (int j = i; j < _trackedEnemyCount - 1; j++)
                    {
                        _trackedEnemies[j] = _trackedEnemies[j + 1];
                        _enemyAlertTimers[j] = _enemyAlertTimers[j + 1];
                        _enemyWasAlerted[j] = _enemyWasAlerted[j + 1];
                    }
                    _trackedEnemyCount--;
                    break;
                }
            }
        }

        public void OnPlayerFiredWeapon()
        {
            if (_combatState == null) return;
            _combatState.OnPlayerFiredWeapon();
        }

        public void OnSaqueosAttacked()
        {
            if (_combatState == null) return;
            _combatState.OnImmediateCombat();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<AlertStateChangedEvent>(OnAlertStateChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<AlertStateChangedEvent>(OnAlertStateChanged);
        }

        private void OnAlertStateChanged(AlertStateChangedEvent evt)
        {
            if (_combatState == null || _combatState.IsInCombat) return;

            if (evt.newState == EAlertState.Alert || evt.newState == EAlertState.FullAlert)
            {
                for (int i = 0; i < _trackedEnemyCount; i++)
                {
                    if (_trackedEnemies[i] != null && _trackedEnemies[i].gameObject.GetInstanceID() == evt.enemyId)
                    {
                        _enemyWasAlerted[i] = true;
                        return;
                    }
                }
            }
        }
    }
}