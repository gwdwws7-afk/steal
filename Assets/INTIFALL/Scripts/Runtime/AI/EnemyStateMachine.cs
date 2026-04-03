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
        [SerializeField] private float suspiciousDuration = 2.2f;
        [SerializeField] private float searchDuration = 7.2f;
        [SerializeField] private float alertDuration = 3.5f;
        [SerializeField] private float alertDropToSearchDelay = 1.1f;
        [SerializeField] private float fullAlertMissionFailDelay = 30f;
        [SerializeField] private float minimumStateDuration = 0.2f;

        [Header("References")]
        [SerializeField] private EnemyController controller;

        private Vector3 _lastKnownPlayerPos;
        private Vector3 _suspiciousLookTarget;
        private Vector3 _searchAnchor;
        private int _searchWaveId;
        private bool _hasDetectionInState;

        public Vector3 LastKnownPlayerPos => _lastKnownPlayerPos;
        public Vector3 SearchAnchor => _searchAnchor;
        public int SearchWaveId => _searchWaveId;
        public bool HasDetectionInState => _hasDetectionInState;
        public float SuspiciousDuration => suspiciousDuration;
        public float SearchDuration => searchDuration;
        public float AlertDuration => alertDuration;
        public float AlertDropToSearchDelay => alertDropToSearchDelay;
        public float FullAlertMissionFailDelay => fullAlertMissionFailDelay;

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
            NormalizeDurations();
            CurrentState = EEnemyState.Unaware;
            StateEnterTime = Time.time;
        }

        private void OnValidate()
        {
            NormalizeDurations();
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
                        if (_hasDetectionInState)
                            TransitionTo(EEnemyState.Searching);
                        else
                            ReturnToUnaware();
                    }
                    break;

                case EEnemyState.Searching:
                    if (TimeInCurrentState >= searchDuration)
                    {
                        ReturnToUnaware();
                    }
                    break;

                case EEnemyState.Alert:
                    if (_hasDetectionInState && TimeInCurrentState >= alertDuration)
                    {
                        TransitionTo(EEnemyState.FullAlert);
                    }
                    else if (!_hasDetectionInState && TimeInCurrentState >= alertDropToSearchDelay)
                    {
                        TransitionTo(EEnemyState.Searching);
                    }
                    break;

                case EEnemyState.FullAlert:
                    if (TimeInCurrentState >= fullAlertMissionFailDelay)
                    {
                        GameManager.Instance?.GameOver();
                    }
                    break;
            }
        }

        private void OnStateEntered(EEnemyState from, EEnemyState to)
        {
            _hasDetectionInState = false;
            if (to == EEnemyState.Searching)
                _searchWaveId++;

            switch (to)
            {
                case EEnemyState.Unaware:
                    controller?.OnEnterUnaware();
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

            EventBus.Publish(new AlertStateChangedEvent
            {
                enemyId = gameObject.GetInstanceID(),
                newState = ConvertAlertState(to)
            });
        }

        public void OnPlayerDetected(Vector3 playerPos)
        {
            _lastKnownPlayerPos = playerPos;
            _suspiciousLookTarget = playerPos;
            _searchAnchor = playerPos;
            _hasDetectionInState = true;

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
                    break;
                case EEnemyState.FullAlert:
                    break;
            }
        }

        public void OnSquadAlert(Vector3 alertPosition, int waveId, bool highPriority)
        {
            _lastKnownPlayerPos = alertPosition;
            _suspiciousLookTarget = alertPosition;
            _searchAnchor = alertPosition;
            _searchWaveId = Mathf.Max(_searchWaveId, waveId);

            if (highPriority)
            {
                _hasDetectionInState = true;

                switch (CurrentState)
                {
                    case EEnemyState.Unaware:
                    case EEnemyState.Suspicious:
                    case EEnemyState.Searching:
                        TransitionTo(EEnemyState.Alert);
                        break;
                }

                return;
            }

            switch (CurrentState)
            {
                case EEnemyState.Unaware:
                    TransitionTo(EEnemyState.Suspicious);
                    break;
                case EEnemyState.Suspicious:
                    TransitionTo(EEnemyState.Searching);
                    break;
            }
        }

        public void OnPlayerLost()
        {
            switch (CurrentState)
            {
                case EEnemyState.Suspicious:
                    ReturnToUnaware();
                    break;
                case EEnemyState.Alert:
                case EEnemyState.FullAlert:
                    _hasDetectionInState = false;
                    break;
            }
        }

        public void ConfigureTimingProfile(
            float searchDurationSeconds,
            float alertDurationSeconds,
            float fullAlertMissionFailDelaySeconds,
            float? alertDropToSearchDelaySeconds = null,
            float? suspiciousDurationSeconds = null)
        {
            searchDuration = searchDurationSeconds;
            alertDuration = alertDurationSeconds;
            fullAlertMissionFailDelay = fullAlertMissionFailDelaySeconds;

            if (alertDropToSearchDelaySeconds.HasValue)
                alertDropToSearchDelay = alertDropToSearchDelaySeconds.Value;

            if (suspiciousDurationSeconds.HasValue)
                suspiciousDuration = suspiciousDurationSeconds.Value;

            NormalizeDurations();
        }

        private void ReturnToUnaware()
        {
            TransitionTo(EEnemyState.Unaware);
        }

        private void NormalizeDurations()
        {
            float min = Mathf.Max(0.05f, minimumStateDuration);
            suspiciousDuration = Mathf.Max(min, suspiciousDuration);
            searchDuration = Mathf.Max(min, searchDuration);
            alertDuration = Mathf.Max(min, alertDuration);
            alertDropToSearchDelay = Mathf.Max(min, alertDropToSearchDelay);
            fullAlertMissionFailDelay = Mathf.Max(min, fullAlertMissionFailDelay);
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

        private static EAlertState ConvertAlertState(EEnemyState state)
        {
            switch (state)
            {
                case EEnemyState.Suspicious:
                    return EAlertState.Suspicious;
                case EEnemyState.Searching:
                    return EAlertState.Searching;
                case EEnemyState.Alert:
                    return EAlertState.Alert;
                case EEnemyState.FullAlert:
                    return EAlertState.FullAlert;
                default:
                    return EAlertState.Unaware;
            }
        }
    }
}
