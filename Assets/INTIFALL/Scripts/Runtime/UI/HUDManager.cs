using INTIFALL.Input;
using INTIFALL.Level;
using INTIFALL.Narrative;
using INTIFALL.Player;
using INTIFALL.System;
using UnityEngine;

namespace INTIFALL.UI
{
    public class HUDManager : MonoBehaviour
    {
        private const string DefaultPrimaryObjectiveTemplate = "Primary: Collect intel {0}/{1} and extract";
        private const string DefaultSecondaryObjective = "Secondary: Stay undetected (optional)";
        private const string DefaultSecondaryObjectiveProgressTemplate = "Secondary: Objectives {0}/{1} (optional)";
        private const string DefaultSecondaryObjectiveCompleteTemplate = "Secondary complete: {0}/{1}";
        private const string MissionEvaluatingSecondary = "Secondary: Evaluation in progress...";
        private const string MissionCompletePrimary = "Primary complete: Successful extraction";
        private const string ReachExtractionPrimary = "Primary: Reach extraction point";

        [Header("HUD Components")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private HPHUD hpHUD;
        [SerializeField] private AlertIndicator alertIndicator;
        [SerializeField] private EagleEyeUI eagleEyeUI;
        [SerializeField] private ToolHUD toolHUD;

        [Header("Settings")]
        [SerializeField] private bool hudEnabled = true;
        [SerializeField] private float fadeSpeed = 0.3f;
        [SerializeField] private string primaryObjectiveTemplate = DefaultPrimaryObjectiveTemplate;
        [SerializeField] private string secondaryObjectiveDefault = DefaultSecondaryObjective;
        [SerializeField] private string secondaryObjectiveProgressTemplate = DefaultSecondaryObjectiveProgressTemplate;
        [SerializeField] private string secondaryObjectiveCompleteTemplate = DefaultSecondaryObjectiveCompleteTemplate;
        [SerializeField] private string secondaryObjectiveEvaluating = MissionEvaluatingSecondary;

        private PlayerHealthSystem _playerHealth;
        private PlayerCombatStateMachine _combatState;
        private bool _isVisible;
        private bool _initialized;
        private int _currentObjectiveLevelIndex;

        public bool IsVisible
        {
            get
            {
                EnsureInitialized();
                return _isVisible;
            }
        }

        private void EnsureInitialized()
        {
            if (_initialized)
                return;

            _isVisible = hudEnabled;
            if (hudPanel != null)
                hudPanel.SetActive(hudEnabled);

            _initialized = true;
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void Start()
        {
            ApplyLocalizedTemplates();

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerHealth = player.GetComponent<PlayerHealthSystem>();
                _combatState = player.GetComponent<PlayerCombatStateMachine>();
            }

            if (hpHUD != null && _playerHealth != null)
                hpHUD.Initialize(_playerHealth);

            if (toolHUD != null)
            {
                var toolManager = player?.GetComponent<INTIFALL.Tools.ToolManager>();
                if (toolManager != null)
                    toolHUD.SetToolManager(toolManager);
            }

            _currentObjectiveLevelIndex = GameManager.Instance != null ? GameManager.Instance.CurrentLevelIndex : 0;
            RefreshObjectiveState();
        }

        private void Update()
        {
            UpdateAlertState();
            UpdateHUDVisibility();
        }

        private void UpdateAlertState()
        {
            if (alertIndicator == null || _combatState == null)
                return;

            alertIndicator.SetAlertState(_combatState.IsInCombat);
        }

        private void UpdateHUDVisibility()
        {
            if (InputCompat.GetKeyDown(KeyCode.H))
                ToggleHUD();
        }

        public void ToggleHUD()
        {
            EnsureInitialized();
            _isVisible = !_isVisible;

            if (hudPanel != null)
                hudPanel.SetActive(_isVisible);
        }

        public void ShowHUD()
        {
            EnsureInitialized();
            if (_isVisible)
                return;

            _isVisible = true;
            if (hudPanel != null)
                hudPanel.SetActive(true);
        }

        public void HideHUD()
        {
            EnsureInitialized();
            if (!_isVisible)
                return;

            _isVisible = false;
            if (hudPanel != null)
                hudPanel.SetActive(false);
        }

        public void ShowIntelPopup(string intelName)
        {
            if (eagleEyeUI != null)
                eagleEyeUI.ShowIntelPickup(intelName);
        }

        public void UpdateHPRecovery()
        {
            if (hpHUD != null)
                hpHUD.UpdateHPRecoveryState();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<HPChangedEvent>(OnHPChanged);
            EventBus.Subscribe<LevelLoadedEvent>(OnLevelLoaded);
            EventBus.Subscribe<IntelCollectedInSceneEvent>(OnIntelCollectedInScene);
            EventBus.Subscribe<SecondaryObjectiveProgressEvent>(OnSecondaryObjectiveProgress);
            EventBus.Subscribe<MissionExitTriggeredEvent>(OnMissionExitTriggered);
            EventBus.Subscribe<MissionOutcomeEvaluatedEvent>(OnMissionOutcomeEvaluated);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<HPChangedEvent>(OnHPChanged);
            EventBus.Unsubscribe<LevelLoadedEvent>(OnLevelLoaded);
            EventBus.Unsubscribe<IntelCollectedInSceneEvent>(OnIntelCollectedInScene);
            EventBus.Unsubscribe<SecondaryObjectiveProgressEvent>(OnSecondaryObjectiveProgress);
            EventBus.Unsubscribe<MissionExitTriggeredEvent>(OnMissionExitTriggered);
            EventBus.Unsubscribe<MissionOutcomeEvaluatedEvent>(OnMissionOutcomeEvaluated);
        }

        private void OnHPChanged(HPChangedEvent evt)
        {
            if (hpHUD != null)
                hpHUD.UpdateHPDisplay(evt.currentHP, evt.maxHP);
        }

        private void OnLevelLoaded(LevelLoadedEvent evt)
        {
            _currentObjectiveLevelIndex = evt.levelIndex;
            RefreshObjectiveState();
        }

        private void OnIntelCollectedInScene(IntelCollectedInSceneEvent evt)
        {
            if (evt.levelIndex != _currentObjectiveLevelIndex)
                return;

            RefreshObjectiveState();
        }

        private void OnMissionExitTriggered(MissionExitTriggeredEvent evt)
        {
            if (evt.levelIndex != _currentObjectiveLevelIndex || eagleEyeUI == null)
                return;

            eagleEyeUI.SetPrimaryObjective(LocalizationService.Get(
                "hud.primary.complete",
                fallbackEnglish: MissionCompletePrimary,
                fallbackChinese: string.Empty));
            eagleEyeUI.SetSecondaryObjective(secondaryObjectiveEvaluating);
        }

        private void OnMissionOutcomeEvaluated(MissionOutcomeEvaluatedEvent evt)
        {
            if (evt.levelIndex != _currentObjectiveLevelIndex || eagleEyeUI == null)
                return;

            int total = Mathf.Max(1, evt.secondaryObjectivesTotal);
            int evaluated = evt.secondaryObjectivesEvaluated > 0
                ? evt.secondaryObjectivesEvaluated
                : evt.secondaryObjectivesCompleted;

            eagleEyeUI.SetSecondaryObjective(FormatObjectiveCounter(
                secondaryObjectiveCompleteTemplate,
                evaluated,
                total,
                DefaultSecondaryObjectiveCompleteTemplate));
        }

        private void OnSecondaryObjectiveProgress(SecondaryObjectiveProgressEvent evt)
        {
            if (evt.levelIndex != _currentObjectiveLevelIndex || eagleEyeUI == null)
                return;

            int completed = Mathf.Clamp(evt.completedCount, 0, Mathf.Max(1, evt.totalCount));
            int total = Mathf.Max(1, evt.totalCount);
            eagleEyeUI.SetSecondaryObjective(FormatObjectiveCounter(
                secondaryObjectiveProgressTemplate,
                completed,
                total,
                DefaultSecondaryObjectiveProgressTemplate));
        }

        private void RefreshObjectiveState()
        {
            if (eagleEyeUI == null)
                return;

            int requiredIntel = ResolveRequiredIntelCount();
            int collectedIntel = ResolveCollectedIntelCount(_currentObjectiveLevelIndex);

            eagleEyeUI.SetIntelTarget(requiredIntel);
            eagleEyeUI.SetIntelCount(collectedIntel);

            if (requiredIntel > 0)
                eagleEyeUI.SetPrimaryObjective(FormatPrimaryObjective(collectedIntel, requiredIntel));
            else
                eagleEyeUI.SetPrimaryObjective(LocalizationService.Get(
                    "hud.primary.reach_extract",
                    fallbackEnglish: ReachExtractionPrimary,
                    fallbackChinese: string.Empty));

            if (SecondaryObjectiveTracker.TryGetActiveSummary(out SecondaryObjectiveSummary summary) && summary.total > 0)
            {
                int completed = Mathf.Clamp(summary.completed, 0, summary.total);
                eagleEyeUI.SetSecondaryObjective(FormatObjectiveCounter(
                    secondaryObjectiveProgressTemplate,
                    completed,
                    summary.total,
                    DefaultSecondaryObjectiveProgressTemplate));
            }
            else if (!string.IsNullOrEmpty(secondaryObjectiveDefault))
            {
                eagleEyeUI.SetSecondaryObjective(secondaryObjectiveDefault);
            }
        }

        private string FormatPrimaryObjective(int collectedIntel, int requiredIntel)
        {
            string template = string.IsNullOrWhiteSpace(primaryObjectiveTemplate)
                ? DefaultPrimaryObjectiveTemplate
                : primaryObjectiveTemplate;

            try
            {
                return string.Format(template, collectedIntel, requiredIntel);
            }
            catch (global::System.FormatException)
            {
                return $"{DefaultPrimaryObjectiveTemplate.Replace("{0}", collectedIntel.ToString()).Replace("{1}", requiredIntel.ToString())}";
            }
        }

        private void ApplyLocalizedTemplates()
        {
            primaryObjectiveTemplate = LocalizationService.Get(
                "hud.primary.collect_extract",
                fallbackEnglish: DefaultPrimaryObjectiveTemplate,
                fallbackChinese: string.Empty);
            secondaryObjectiveDefault = LocalizationService.Get(
                "hud.secondary.undetected",
                fallbackEnglish: DefaultSecondaryObjective,
                fallbackChinese: string.Empty);
            secondaryObjectiveProgressTemplate = LocalizationService.Get(
                "hud.secondary.progress",
                fallbackEnglish: DefaultSecondaryObjectiveProgressTemplate,
                fallbackChinese: string.Empty);
            secondaryObjectiveCompleteTemplate = LocalizationService.Get(
                "hud.secondary.complete_with_count",
                fallbackEnglish: DefaultSecondaryObjectiveCompleteTemplate,
                fallbackChinese: string.Empty);
            secondaryObjectiveEvaluating = LocalizationService.Get(
                "hud.secondary.evaluating",
                fallbackEnglish: MissionEvaluatingSecondary,
                fallbackChinese: string.Empty);
        }

        private static string FormatObjectiveCounter(
            string template,
            int completed,
            int total,
            string fallbackTemplate)
        {
            string format = string.IsNullOrWhiteSpace(template) ? fallbackTemplate : template;
            string fallback = string.IsNullOrWhiteSpace(fallbackTemplate)
                ? DefaultSecondaryObjectiveProgressTemplate
                : fallbackTemplate;

            try
            {
                return string.Format(format, completed, total);
            }
            catch (global::System.FormatException)
            {
                return string.Format(fallback, completed, total);
            }
        }

        private static int ResolveRequiredIntelCount()
        {
            LevelLoader loader = Object.FindFirstObjectByType<LevelLoader>();
            if (loader != null)
            {
                var intelData = loader.GetIntelSpawnData();
                if (intelData != null && intelData.intelPoints != null && intelData.intelPoints.Length > 0)
                    return intelData.intelPoints.Length;

                if (loader.IntelSpawned > 0)
                    return loader.IntelSpawned;
            }

            return 3;
        }

        private static int ResolveCollectedIntelCount(int levelIndex)
        {
            NarrativeManager narrative = NarrativeManager.Instance;
            if (narrative == null)
                narrative = Object.FindFirstObjectByType<NarrativeManager>();

            return narrative != null ? narrative.GetIntelCollectedForLevel(levelIndex) : 0;
        }
    }
}
