using UnityEngine;

namespace INTIFALL.Player
{
    public enum EPlayerState
    {
        Idle,
        Walk,
        Sprint,
        Crouch,
        Cover,
        Roll,
        Rope
    }

    public class PlayerStateMachine : MonoBehaviour
    {
        public EPlayerState CurrentState { get; private set; }

        private float _stateTimer;
        private float _crouchTimer;
        private bool _isTransitioning;

        public float StateTimer => _stateTimer;
        public float CrouchTimer => _crouchTimer;

        public void TransitionTo(EPlayerState newState)
        {
            if (_isTransitioning) return;
            if (CurrentState == newState) return;

            _isTransitioning = true;
            _stateTimer = 0f;
            CurrentState = newState;
            _isTransitioning = false;
        }

        public void Update()
        {
            _stateTimer += Time.deltaTime;
        }

        public void IncrementCrouchTimer(float delta)
        {
            _crouchTimer += delta;
        }

        public void ResetCrouchTimer()
        {
            _crouchTimer = 0f;
        }
    }
}