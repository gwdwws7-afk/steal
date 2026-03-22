using UnityEngine;
using INTIFALL.System;

namespace INTIFALL.Player
{
    public enum ECombatMode
    {
        Normal,
        Combat
    }

    public struct CombatModeChangedEvent
    {
        public ECombatMode newMode;
        public ECombatMode previousMode;
    }

    public class PlayerCombatStateMachine : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private float alertToCombatDelay = 5f;
        [SerializeField] private float combatExitDistance = 30f;

        private ECombatMode _currentMode;
        private float _combatTimer;
        private bool _isInCombat;
        private Vector3 _combatOrigin;

        public ECombatMode CurrentMode => _currentMode;
        public bool IsInCombat => _currentMode == ECombatMode.Combat;
        public float CombatTimer => _combatTimer;

        private void Update()
        {
            if (_currentMode == ECombatMode.Combat)
            {
                _combatTimer += Time.deltaTime;
                CheckCombatExit();
            }
        }

        public void TransitionTo(ECombatMode newMode)
        {
            if (_currentMode == newMode) return;

            ECombatMode previousMode = _currentMode;
            _currentMode = newMode;

            if (newMode == ECombatMode.Combat)
            {
                _combatOrigin = transform.position;
                _combatTimer = 0f;
            }

            EventBus.Publish(new CombatModeChangedEvent
            {
                newMode = newMode,
                previousMode = previousMode
            });
        }

        public void OnEnemyAlerted(float alertDuration)
        {
            if (_currentMode == ECombatMode.Combat) return;

            if (alertDuration >= alertToCombatDelay)
            {
                TransitionTo(ECombatMode.Combat);
            }
        }

        public void OnImmediateCombat()
        {
            if (_currentMode == ECombatMode.Combat) return;
            TransitionTo(ECombatMode.Combat);
        }

        public void OnPlayerFiredWeapon()
        {
            if (_currentMode == ECombatMode.Combat) return;
            TransitionTo(ECombatMode.Combat);
        }

        private void CheckCombatExit()
        {
            float distanceFromOrigin = Vector3.Distance(transform.position, _combatOrigin);
            if (distanceFromOrigin > combatExitDistance)
            {
                TransitionTo(ECombatMode.Normal);
            }
        }

        public void ForceExitCombat()
        {
            if (_currentMode == ECombatMode.Normal) return;
            TransitionTo(ECombatMode.Normal);
        }
    }
}