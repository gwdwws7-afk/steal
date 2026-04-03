using System;
using System.Collections.Generic;
using INTIFALL.System;
using UnityEngine;

namespace INTIFALL.Level
{
    public enum ESecondaryObjectiveState
    {
        InProgress,
        Completed,
        Failed
    }

    public struct SecondaryObjectiveRegisteredEvent
    {
        public string objectiveId;
        public bool startsCompleted;
    }

    public struct SecondaryObjectiveCompletedEvent
    {
        public string objectiveId;
    }

    public struct SecondaryObjectiveFailedEvent
    {
        public string objectiveId;
    }

    public struct SecondaryObjectiveProgressEvent
    {
        public int levelIndex;
        public string objectiveId;
        public ESecondaryObjectiveState state;
        public int completedCount;
        public int totalCount;
    }

    public struct SecondaryObjectiveSummary
    {
        public int levelIndex;
        public int completed;
        public int total;
    }

    public class SecondaryObjectiveTracker : MonoBehaviour
    {
        [Header("Behavior")]
        [SerializeField] private bool autoRegisterStealthObjectives = true;
        [SerializeField] private bool autoResetOnLevelLoaded = true;

        [Header("Objective IDs")]
        [SerializeField] private string undetectedObjectiveId = "stealth_undetected";
        [SerializeField] private string noFullAlertObjectiveId = "stealth_no_full_alert";

        private readonly Dictionary<string, ESecondaryObjectiveState> _objectiveStates = new(StringComparer.OrdinalIgnoreCase);
        private int _completedCount;
        private int _currentLevelIndex;
        private bool _initialized;

        public int CurrentLevelIndex => _currentLevelIndex;
        public int CompletedCount => _completedCount;
        public int TotalCount => _objectiveStates.Count;

        private void OnEnable()
        {
            EventBus.Subscribe<LevelLoadedEvent>(OnLevelLoaded);
            EventBus.Subscribe<AlertStateChangedEvent>(OnAlertStateChanged);
            EventBus.Subscribe<SecondaryObjectiveRegisteredEvent>(OnSecondaryObjectiveRegistered);
            EventBus.Subscribe<SecondaryObjectiveCompletedEvent>(OnSecondaryObjectiveCompleted);
            EventBus.Subscribe<SecondaryObjectiveFailedEvent>(OnSecondaryObjectiveFailed);

            EnsureInitialized();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LevelLoadedEvent>(OnLevelLoaded);
            EventBus.Unsubscribe<AlertStateChangedEvent>(OnAlertStateChanged);
            EventBus.Unsubscribe<SecondaryObjectiveRegisteredEvent>(OnSecondaryObjectiveRegistered);
            EventBus.Unsubscribe<SecondaryObjectiveCompletedEvent>(OnSecondaryObjectiveCompleted);
            EventBus.Unsubscribe<SecondaryObjectiveFailedEvent>(OnSecondaryObjectiveFailed);
        }

        public SecondaryObjectiveSummary GetSummary()
        {
            EnsureInitialized();
            SyncStealthObjectivesFromGameManager();

            return new SecondaryObjectiveSummary
            {
                levelIndex = _currentLevelIndex,
                completed = Mathf.Clamp(_completedCount, 0, _objectiveStates.Count),
                total = Mathf.Max(0, _objectiveStates.Count)
            };
        }

        public void ResetObjectivesForLevel(int levelIndex)
        {
            _currentLevelIndex = Mathf.Max(0, levelIndex);
            _objectiveStates.Clear();
            _completedCount = 0;

            if (autoRegisterStealthObjectives)
            {
                RegisterObjective(undetectedObjectiveId, startsCompleted: true);
                RegisterObjective(noFullAlertObjectiveId, startsCompleted: true);
            }

            SyncStealthObjectivesFromGameManager();
        }

        public void RegisterObjective(string objectiveId, bool startsCompleted = false)
        {
            string normalizedId = NormalizeObjectiveId(objectiveId);
            if (string.IsNullOrEmpty(normalizedId))
                return;

            SetObjectiveState(
                normalizedId,
                startsCompleted ? ESecondaryObjectiveState.Completed : ESecondaryObjectiveState.InProgress);
        }

        public void MarkObjectiveCompleted(string objectiveId)
        {
            string normalizedId = NormalizeObjectiveId(objectiveId);
            if (string.IsNullOrEmpty(normalizedId))
                return;

            SetObjectiveState(normalizedId, ESecondaryObjectiveState.Completed);
        }

        public void MarkObjectiveFailed(string objectiveId)
        {
            string normalizedId = NormalizeObjectiveId(objectiveId);
            if (string.IsNullOrEmpty(normalizedId))
                return;

            SetObjectiveState(normalizedId, ESecondaryObjectiveState.Failed);
        }

        public static bool TryGetActiveSummary(out SecondaryObjectiveSummary summary)
        {
            SecondaryObjectiveTracker tracker = UnityEngine.Object.FindFirstObjectByType<SecondaryObjectiveTracker>();
            if (tracker == null)
            {
                summary = default;
                return false;
            }

            summary = tracker.GetSummary();
            return true;
        }

        private void EnsureInitialized()
        {
            if (_initialized)
                return;

            int defaultLevelIndex = 0;
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
                gameManager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
            if (gameManager != null)
                defaultLevelIndex = gameManager.CurrentLevelIndex;

            ResetObjectivesForLevel(defaultLevelIndex);
            _initialized = true;
        }

        private void OnLevelLoaded(LevelLoadedEvent evt)
        {
            if (!autoResetOnLevelLoaded)
                return;

            ResetObjectivesForLevel(evt.levelIndex);
        }

        private void OnAlertStateChanged(AlertStateChangedEvent evt)
        {
            if (!autoRegisterStealthObjectives)
                return;

            if (evt.newState == EAlertState.Alert || evt.newState == EAlertState.FullAlert)
                MarkObjectiveFailed(undetectedObjectiveId);

            if (evt.newState == EAlertState.FullAlert)
                MarkObjectiveFailed(noFullAlertObjectiveId);
        }

        private void OnSecondaryObjectiveRegistered(SecondaryObjectiveRegisteredEvent evt)
        {
            RegisterObjective(evt.objectiveId, evt.startsCompleted);
        }

        private void OnSecondaryObjectiveCompleted(SecondaryObjectiveCompletedEvent evt)
        {
            MarkObjectiveCompleted(evt.objectiveId);
        }

        private void OnSecondaryObjectiveFailed(SecondaryObjectiveFailedEvent evt)
        {
            MarkObjectiveFailed(evt.objectiveId);
        }

        private void SyncStealthObjectivesFromGameManager()
        {
            if (!autoRegisterStealthObjectives)
                return;

            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
                gameManager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
            if (gameManager == null)
                return;

            if (gameManager.WasDiscovered)
                MarkObjectiveFailed(undetectedObjectiveId);

            if (gameManager.FullAlertTriggered)
            {
                MarkObjectiveFailed(undetectedObjectiveId);
                MarkObjectiveFailed(noFullAlertObjectiveId);
            }
        }

        private void SetObjectiveState(string objectiveId, ESecondaryObjectiveState nextState)
        {
            if (_objectiveStates.TryGetValue(objectiveId, out ESecondaryObjectiveState currentState))
            {
                if (currentState == nextState)
                    return;

                if (currentState == ESecondaryObjectiveState.Completed)
                    _completedCount = Mathf.Max(0, _completedCount - 1);
            }

            _objectiveStates[objectiveId] = nextState;

            if (nextState == ESecondaryObjectiveState.Completed)
                _completedCount++;

            EventBus.Publish(new SecondaryObjectiveProgressEvent
            {
                levelIndex = _currentLevelIndex,
                objectiveId = objectiveId,
                state = nextState,
                completedCount = Mathf.Clamp(_completedCount, 0, _objectiveStates.Count),
                totalCount = _objectiveStates.Count
            });
        }

        private static string NormalizeObjectiveId(string objectiveId)
        {
            return string.IsNullOrWhiteSpace(objectiveId) ? string.Empty : objectiveId.Trim();
        }
    }
}
