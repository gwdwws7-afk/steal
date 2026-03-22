using UnityEngine;
using INTIFALL.System;

namespace INTIFALL.AI
{
    public enum EEnemyState
    {
        Unaware,
        Suspicious,
        Searching,
        Alert,
        FullAlert
    }

    public class EnemyStateMachine : MonoBehaviour
    {
        public EEnemyState CurrentState { get; private set; }
        public float StateEnterTime { get; private set; }
        public float TimeInCurrentState => Time.time - StateEnterTime;

        [Header("Timing")]
        [SerializeField] private float suspiciousDuration = 3f;
        [SerializeField] private float searchDuration = 5f;
        [SerializeField] private float alertDuration = 5f;

        [Header("References")]
        [SerializeField] private EnemyController controller;

        private Vector3 _lastKnownPlayerPos;
        private Vector3 _suspiciousLookTarget;

        public Vector3 LastKnownPlayerPos => _lastKnownPlayerPos;

        public void TransitionTo(EEnemyState newState)
        {
            if (CurrentState == newState) return;

            EEnemyState previousState = CurrentState;
            CurrentState = newState;
            StateEnterTime = Time.time;

            OnStateEntered(previousState, newState);
        }

        private void Awake()
        {
            CurrentState = EEnemyState.Unaware;
            StateEnterTime = Time.time;
        }

        private void Update()
        {
            UpdateStateLogic();
        }

        private void UpdateStateLogic()
        {
            switch (CurrentState)
            {
                case EEnemyState.Unaware:
                    break;

                case EEnemyState.Suspicious:
                    if (TimeInCurrentState >= suspiciousDuration)
                    {
                        TransitionTo(EEnemyState.Searching);
                    }
                    break;

                case EEnemyState.Searching:
                    if (TimeInCurrentState >= searchDuration)
                    {
                        ReturnToUnaware();
                    }
                    break;

                case EEnemyState.Alert:
                    if (TimeInCurrentState >= alertDuration)
                    {
                        TransitionTo(EEnemyState.FullAlert);
                    }
                    break;

                case EEnemyState.FullAlert:
                    break;
            }
        }

        private void OnStateEntered(EEnemyState from, EEnemyState to)
        {
            switch (to)
            {
                case EEnemyState.Unaware:
                    controller.OnEnterUnaware();
                    break;
                case EEnemyState.Suspicious:
                    controller?.OnEnterSuspicious(_suspiciousLookTarget);
                    break;
                case EEnemyState.Searching:
                    controller?.OnEnterSearching(_lastKnownPlayerPos);
                    break;
                case EEnemyState.Alert:
                    controller?.OnEnterAlert();
                    break;
                case EEnemyState.FullAlert:
                    controller?.OnEnterFullAlert();
                    break;
            }
        }

        public void OnPlayerDetected(Vector3 playerPos)
        {
            _lastKnownPlayerPos = playerPos;
            _suspiciousLookTarget = playerPos;

            switch (CurrentState)
            {
                case EEnemyState.Unaware:
                    TransitionTo(EEnemyState.Suspicious);
                    break;
                case EEnemyState.Suspicious:
                    TransitionTo(EEnemyState.Searching);
                    break;
                case EEnemyState.Searching:
                    TransitionTo(EEnemyState.Alert);
                    break;
                case EEnemyState.Alert:
                    TransitionTo(EEnemyState.Alert);
                    break;
                case EEnemyState.FullAlert:
                    break;
            }
        }

        public void OnPlayerLost()
        {
            if (CurrentState == EEnemyState.Suspicious)
            {
                ReturnToUnaware();
            }
        }

        private void ReturnToUnaware()
        {
            TransitionTo(EEnemyState.Unaware);
        }

        public bool CanSeePlayer()
        {
            return CurrentState == EEnemyState.Suspicious ||
                   CurrentState == EEnemyState.Searching ||
                   CurrentState == EEnemyState.Alert ||
                   CurrentState == EEnemyState.FullAlert;
        }

        public bool IsInvestigating()
        {
            return CurrentState == EEnemyState.Suspicious ||
                   CurrentState == EEnemyState.Searching;
        }

        public bool IsAlerted()
        {
            return CurrentState == EEnemyState.Alert ||
                   CurrentState == EEnemyState.FullAlert;
        }
    }
}
