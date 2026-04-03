using System.Text;
using INTIFALL.Level;
using INTIFALL.System;
using UnityEngine;
using UnityEngine.UI;

namespace INTIFALL.UI
{
    public struct MissionDebriefShownEvent
    {
        public int levelIndex;
        public string rank;
        public int creditsEarned;
        public string summary;
    }

    public class MissionDebriefUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject debriefPanel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text summaryText;

        [Header("Behavior")]
        [SerializeField] private bool autoShowOnMissionOutcome = true;
        [SerializeField] private bool autoHide = false;
        [SerializeField] private float autoHideDelay = 8f;
        [SerializeField] private float transitionHoldSeconds = 2.5f;

        private bool _isVisible;
        private float _visibleTime;
        private MissionOutcomeEvaluatedEvent _lastOutcome;
        private string _lastSummary = string.Empty;

        public bool IsVisible => _isVisible;
        public string LastSummary => _lastSummary;
        public MissionOutcomeEvaluatedEvent LastOutcome => _lastOutcome;
        public float TransitionHoldSeconds => Mathf.Max(0f, transitionHoldSeconds);

        private void Awake()
        {
            HideDebrief();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<MissionOutcomeEvaluatedEvent>(OnMissionOutcomeEvaluated);
            EventBus.Subscribe<LevelLoadedEvent>(OnLevelLoaded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<MissionOutcomeEvaluatedEvent>(OnMissionOutcomeEvaluated);
            EventBus.Unsubscribe<LevelLoadedEvent>(OnLevelLoaded);
        }

        private void Update()
        {
            if (!_isVisible || !autoHide)
                return;

            _visibleTime += Time.unscaledDeltaTime;
            if (_visibleTime >= Mathf.Max(0.5f, autoHideDelay))
                HideDebrief();
        }

        public void ShowDebrief(MissionOutcomeEvaluatedEvent outcome)
        {
            _lastOutcome = outcome;
            _lastSummary = BuildSummary(outcome);

            _isVisible = true;
            _visibleTime = 0f;

            if (debriefPanel != null)
                debriefPanel.SetActive(true);

            if (titleText != null)
                titleText.text = string.Format(
                    LocalizationService.Get(
                        "debrief.title",
                        fallbackEnglish: "Mission Debrief - Level {0}",
                        fallbackChinese: string.Empty),
                    outcome.levelIndex + 1);

            if (summaryText != null)
                summaryText.text = _lastSummary;

            EventBus.Publish(new MissionDebriefShownEvent
            {
                levelIndex = outcome.levelIndex,
                rank = outcome.rank,
                creditsEarned = outcome.creditsEarned,
                summary = _lastSummary
            });
        }

        public void HideDebrief()
        {
            _isVisible = false;
            _visibleTime = 0f;

            if (debriefPanel != null)
                debriefPanel.SetActive(false);
        }

        private void OnMissionOutcomeEvaluated(MissionOutcomeEvaluatedEvent evt)
        {
            if (!autoShowOnMissionOutcome)
                return;

            ShowDebrief(evt);
        }

        private void OnLevelLoaded(LevelLoadedEvent evt)
        {
            HideDebrief();
        }

        private static string BuildSummary(MissionOutcomeEvaluatedEvent outcome)
        {
            string stealthState = ResolveStealthState(outcome.wasDiscovered, outcome.fullAlertTriggered);
            string combatStyle = outcome.zeroKill
                ? LocalizationService.Get("debrief.combat.zero_kill", fallbackEnglish: "Zero-Kill", fallbackChinese: string.Empty)
                : LocalizationService.Get("debrief.combat.lethal", fallbackEnglish: "Lethal", fallbackChinese: string.Empty);
            string damageState = outcome.noDamage
                ? LocalizationService.Get("debrief.damage.no_damage", fallbackEnglish: "No Damage", fallbackChinese: string.Empty)
                : LocalizationService.Get("debrief.damage.took_damage", fallbackEnglish: "Took Damage", fallbackChinese: string.Empty);
            string routeLabel = string.IsNullOrWhiteSpace(outcome.extractionRouteLabel)
                ? LocalizationService.Get("route.main", fallbackEnglish: "Main Extraction", fallbackChinese: string.Empty)
                : outcome.extractionRouteLabel;
            string routeType = outcome.usedOptionalExit
                ? LocalizationService.Get("debrief.route.optional", fallbackEnglish: "Optional", fallbackChinese: string.Empty)
                : LocalizationService.Get("debrief.route.main", fallbackEnglish: "Main", fallbackChinese: string.Empty);
            int secondaryTotal = outcome.secondaryObjectivesTotal <= 0 ? 2 : outcome.secondaryObjectivesTotal;
            int secondaryCompleted = Mathf.Clamp(outcome.secondaryObjectivesCompleted, 0, secondaryTotal);
            int secondaryEvaluated = outcome.secondaryObjectivesEvaluated > 0
                ? Mathf.Clamp(outcome.secondaryObjectivesEvaluated, 0, secondaryTotal)
                : secondaryCompleted;

            var sb = new StringBuilder();
            sb.AppendLine(string.Format(
                LocalizationService.Get("debrief.line.rank", fallbackEnglish: "Rank: {0} ({1})", fallbackChinese: string.Empty),
                outcome.rank,
                outcome.rankScore));
            sb.AppendLine(string.Format(
                LocalizationService.Get("debrief.line.credits", fallbackEnglish: "Credits: {0}", fallbackChinese: string.Empty),
                outcome.creditsEarned));
            sb.AppendLine(string.Format(
                LocalizationService.Get("debrief.line.intel", fallbackEnglish: "Intel: {0}/{1}", fallbackChinese: string.Empty),
                outcome.intelCollected,
                outcome.intelRequired));
            sb.AppendLine(string.Format(
                LocalizationService.Get("debrief.line.secondary", fallbackEnglish: "Secondary Objectives: {0}/{1}", fallbackChinese: string.Empty),
                secondaryCompleted,
                secondaryTotal));
            if (secondaryEvaluated != secondaryCompleted)
            {
                sb.AppendLine(string.Format(
                    LocalizationService.Get(
                        "debrief.line.secondary_evaluated",
                        fallbackEnglish: "Secondary (evaluated): {0}/{1}",
                        fallbackChinese: string.Empty),
                    secondaryEvaluated,
                    secondaryTotal));
            }
            sb.AppendLine(string.Format(
                LocalizationService.Get("debrief.line.stealth", fallbackEnglish: "Stealth: {0}", fallbackChinese: string.Empty),
                stealthState));
            sb.AppendLine(string.Format(
                LocalizationService.Get("debrief.line.combat", fallbackEnglish: "Combat: {0}", fallbackChinese: string.Empty),
                combatStyle));
            sb.AppendLine(string.Format(
                LocalizationService.Get("debrief.line.damage", fallbackEnglish: "Damage: {0}", fallbackChinese: string.Empty),
                damageState));
            sb.AppendLine(string.Format(
                LocalizationService.Get(
                    "debrief.line.extraction",
                    fallbackEnglish: "Extraction: {0} ({1}, risk {2}, x{3})",
                    fallbackChinese: string.Empty),
                routeLabel,
                routeType,
                outcome.routeRiskTier,
                outcome.routeCreditMultiplier.ToString("0.00")));
            sb.AppendLine(string.Format(
                LocalizationService.Get(
                    "debrief.line.pressure",
                    fallbackEnglish: "Pressure: alerts {0}, tools used {1}",
                    fallbackChinese: string.Empty),
                outcome.alertsTriggered,
                outcome.toolsUsed));
            sb.AppendLine(string.Format(
                LocalizationService.Get(
                    "debrief.line.tool_window",
                    fallbackEnglish: "Tool Window: {0} ({1})",
                    fallbackChinese: string.Empty),
                FormatSignedInt(outcome.toolRiskWindowAdjustment),
                ResolveToolWindowAssessment(outcome.toolRiskWindowAdjustment)));
            sb.AppendLine(string.Format(
                LocalizationService.Get(
                    "debrief.line.tool_mix",
                    fallbackEnglish: "Tool Mix: rope {0}, smoke {1}, bait {2}, cooldown load {3:0.0}s",
                    fallbackChinese: string.Empty),
                Mathf.Max(0, outcome.ropeToolUses),
                Mathf.Max(0, outcome.smokeToolUses),
                Mathf.Max(0, outcome.soundBaitToolUses),
                Mathf.Max(0f, outcome.toolCooldownLoad)));
            return sb.ToString().TrimEnd();
        }

        private static string FormatSignedInt(int value)
        {
            return value.ToString("+0;-0;0");
        }

        private static string ResolveToolWindowAssessment(int adjustment)
        {
            if (adjustment > 0)
            {
                return LocalizationService.Get(
                    "debrief.tool_window.positive",
                    fallbackEnglish: "Balanced window",
                    fallbackChinese: string.Empty);
            }

            if (adjustment < 0)
            {
                return LocalizationService.Get(
                    "debrief.tool_window.negative",
                    fallbackEnglish: "Overused / mistimed",
                    fallbackChinese: string.Empty);
            }

            return LocalizationService.Get(
                "debrief.tool_window.neutral",
                fallbackEnglish: "Neutral",
                fallbackChinese: string.Empty);
        }

        private static string ResolveStealthState(bool wasDiscovered, bool fullAlertTriggered)
        {
            if (!wasDiscovered && !fullAlertTriggered)
                return LocalizationService.Get("debrief.stealth.undetected", fallbackEnglish: "Undetected", fallbackChinese: string.Empty);
            if (fullAlertTriggered)
                return LocalizationService.Get("debrief.stealth.full_alert", fallbackEnglish: "Full Alert", fallbackChinese: string.Empty);
            return LocalizationService.Get("debrief.stealth.spotted", fallbackEnglish: "Spotted", fallbackChinese: string.Empty);
        }
    }
}
