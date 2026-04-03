using INTIFALL.Economy;
using INTIFALL.Growth;
using INTIFALL.Narrative;
using INTIFALL.System;
using INTIFALL.UI;
using UnityEngine;

namespace INTIFALL.Level
{
    public struct MissionExitTriggeredEvent
    {
        public int levelIndex;
        public bool requiresAllIntel;
        public int requiredIntelCount;
        public string extractionRouteId;
        public string extractionRouteLabel;
        public bool isMainRoute;
        public int routeRiskTier;
        public float routeCreditMultiplier;
    }

    public struct MissionOutcomeEvaluatedEvent
    {
        public int levelIndex;
        public string rank;
        public int rankScore;
        public int creditsEarned;
        public int intelCollected;
        public int intelRequired;
        public int secondaryObjectivesCompleted;
        public int secondaryObjectivesEvaluated;
        public int secondaryObjectivesTotal;
        public bool zeroKill;
        public bool noDamage;
        public bool wasDiscovered;
        public bool fullAlertTriggered;
        public string extractionRouteId;
        public string extractionRouteLabel;
        public bool usedOptionalExit;
        public int routeRiskTier;
        public float routeCreditMultiplier;
        public int toolsUsed;
        public int alertsTriggered;
        public int toolRiskWindowAdjustment;
        public float toolCooldownLoad;
        public int ropeToolUses;
        public int smokeToolUses;
        public int soundBaitToolUses;
    }

    [RequireComponent(typeof(Collider))]
    public class MissionExitPoint : MonoBehaviour
    {
        [SerializeField] private int levelIndex;
        [SerializeField] private bool requiresAllIntel = true;
        [SerializeField] private int requiredIntelCount = 3;
        [SerializeField] private string completionRank = "B";
        [SerializeField] private int completionCredits;
        [SerializeField] private bool loadNextLevel = true;
        [SerializeField] private bool showDebriefBeforeTransition = true;
        [SerializeField] private float fallbackDebriefDelaySeconds = 2.5f;

        [Header("Extraction Route")]
        [SerializeField] private string extractionRouteId = "main";
        [SerializeField] private string extractionRouteLabel = "Main Extraction";
        [SerializeField] private bool isMainRoute = true;
        [SerializeField, Range(0, 3)] private int routeRiskTier;
        [SerializeField, Range(0.5f, 2f)] private float routeCreditMultiplier = 1f;
        [SerializeField] private int routeSecondaryObjectiveBonus;

        private bool _triggered;
        private GameObject _triggeringPlayer;

        public string ExtractionRouteId => extractionRouteId;
        public string ExtractionRouteLabel => extractionRouteLabel;
        public bool IsMainRoute => isMainRoute;
        public int RouteRiskTier => routeRiskTier;
        public float RouteCreditMultiplier => routeCreditMultiplier;

        private void Awake()
        {
            EnsureTriggerCollider();
        }

        private void Reset()
        {
            EnsureTriggerCollider();
        }

        public void Configure(
            int index,
            bool requireAllIntel,
            int requiredIntel,
            string routeId = "main",
            string routeLabel = "Main Extraction",
            int riskTier = 0,
            float creditMultiplier = 1f,
            int secondaryBonus = 0,
            bool mainRoute = true)
        {
            levelIndex = Mathf.Max(0, index);
            requiresAllIntel = requireAllIntel;
            requiredIntelCount = Mathf.Max(0, requiredIntel);
            extractionRouteId = string.IsNullOrWhiteSpace(routeId) ? "main" : routeId.Trim();
            extractionRouteLabel = string.IsNullOrWhiteSpace(routeLabel) ? "Main Extraction" : routeLabel.Trim();
            routeRiskTier = Mathf.Clamp(riskTier, 0, 3);
            routeCreditMultiplier = Mathf.Clamp(creditMultiplier, 0.5f, 2f);
            routeSecondaryObjectiveBonus = Mathf.Max(0, secondaryBonus);
            isMainRoute = mainRoute;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered)
                return;
            if (!other.CompareTag("Player"))
                return;
            if (!CanExitMission())
                return;

            _triggeringPlayer = other.gameObject;
            CompleteMission();
        }

        private bool CanExitMission()
        {
            if (!requiresAllIntel)
                return true;

            NarrativeManager narrative = NarrativeManager.Instance;
            if (narrative == null)
                narrative = Object.FindFirstObjectByType<NarrativeManager>();
            if (narrative == null)
                return false;

            return narrative.GetIntelCollectedForLevel(levelIndex) >= requiredIntelCount;
        }

        private void CompleteMission()
        {
            _triggered = true;

            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
                gameManager = Object.FindFirstObjectByType<GameManager>();

            int intelCollected = GetCollectedIntelCount();
            int intelRequired = Mathf.Max(0, requiredIntelCount);
            SecondaryObjectiveSummary secondarySummary = ResolveSecondaryObjectiveSummary(gameManager);
            int secondaryObjectives = secondarySummary.completed;
            int secondaryObjectivesTotal = secondarySummary.total;
            float missionTimeBudget = ResolveMissionTimeBudget();

            MissionResult missionResult;
            if (gameManager != null)
            {
                missionResult = gameManager.CalculateMissionResult(
                    secondaryObjectives,
                    intelCollected,
                    intelRequired,
                    missionTimeBudget,
                    extractionRouteId,
                    extractionRouteLabel,
                    isMainRoute,
                    routeRiskTier,
                    routeCreditMultiplier,
                    routeSecondaryObjectiveBonus,
                    secondaryObjectivesTotal);
                gameManager.ApplyMissionResult(missionResult);
            }
            else
            {
                int fallbackRankScore = RankToScore(completionRank);
                missionResult = new MissionResult
                {
                    LevelName = string.Empty,
                    PlayTime = 0f,
                    WasDiscovered = false,
                    FullAlertTriggered = false,
                    EnemiesKilled = 0,
                    EnemiesKnockedOut = 0,
                    Rank = completionRank,
                    RankScore = fallbackRankScore,
                    CreditsEarned = completionCredits,
                    ZeroKill = true,
                    NoDamage = true,
                    IntelCollected = intelCollected,
                    IntelRequired = intelRequired,
                    SecondaryObjectivesCompleted = secondaryObjectives,
                    SecondaryObjectivesTotal = secondaryObjectivesTotal,
                    SecondaryObjectivesEvaluated = secondaryObjectives,
                    ExtractionRouteId = extractionRouteId,
                    ExtractionRouteLabel = extractionRouteLabel,
                    UsedOptionalExit = !isMainRoute,
                    RouteRiskTier = routeRiskTier,
                    RouteCreditMultiplier = routeCreditMultiplier,
                    AlertsTriggered = 0,
                    ToolsUsed = 0
                };
            }

            EventBus.Publish(new MissionExitTriggeredEvent
            {
                levelIndex = levelIndex,
                requiresAllIntel = requiresAllIntel,
                requiredIntelCount = requiredIntelCount,
                extractionRouteId = extractionRouteId,
                extractionRouteLabel = extractionRouteLabel,
                isMainRoute = isMainRoute,
                routeRiskTier = routeRiskTier,
                routeCreditMultiplier = routeCreditMultiplier
            });

            EventBus.Publish(new MissionOutcomeEvaluatedEvent
            {
                levelIndex = levelIndex,
                rank = missionResult.Rank,
                rankScore = missionResult.RankScore,
                creditsEarned = missionResult.CreditsEarned,
                intelCollected = missionResult.IntelCollected,
                intelRequired = missionResult.IntelRequired,
                secondaryObjectivesCompleted = missionResult.SecondaryObjectivesCompleted,
                secondaryObjectivesEvaluated = missionResult.SecondaryObjectivesEvaluated,
                secondaryObjectivesTotal = missionResult.SecondaryObjectivesTotal,
                zeroKill = missionResult.ZeroKill,
                noDamage = missionResult.NoDamage,
                wasDiscovered = missionResult.WasDiscovered,
                fullAlertTriggered = missionResult.FullAlertTriggered,
                extractionRouteId = missionResult.ExtractionRouteId,
                extractionRouteLabel = missionResult.ExtractionRouteLabel,
                usedOptionalExit = missionResult.UsedOptionalExit,
                routeRiskTier = missionResult.RouteRiskTier,
                routeCreditMultiplier = missionResult.RouteCreditMultiplier,
                toolsUsed = missionResult.ToolsUsed,
                alertsTriggered = missionResult.AlertsTriggered,
                toolRiskWindowAdjustment = missionResult.ToolRiskWindowAdjustment,
                toolCooldownLoad = missionResult.ToolCooldownLoad,
                ropeToolUses = missionResult.RopeToolUses,
                smokeToolUses = missionResult.SmokeToolUses,
                soundBaitToolUses = missionResult.SoundBaitToolUses
            });

            ApplyMetaProgression(gameManager, missionResult);

            LevelFlowManager flow = Object.FindFirstObjectByType<LevelFlowManager>();
            if (flow == null)
                return;

            flow.SelectLevel(levelIndex);
            flow.CompleteLevel(missionResult.Rank, missionResult.CreditsEarned);

            float transitionDelay = ResolveTransitionDelaySeconds();
            if (transitionDelay > 0f)
                StartCoroutine(TransitionAfterDebrief(flow, transitionDelay));
            else
                ExecuteFlowTransition(flow);
        }

        private void ApplyMetaProgression(GameManager gameManager, MissionResult missionResult)
        {
            int completedLevel = Mathf.Max(1, levelIndex + 1);

            ProgressionTree progressionTree = Object.FindFirstObjectByType<ProgressionTree>();
            progressionTree?.CompleteLevel(completedLevel);

            BloodlineSystem bloodlineSystem = Object.FindFirstObjectByType<BloodlineSystem>();
            bloodlineSystem?.UnlockPassiveForLevel(completedLevel);

            CreditSystem creditSystem = Object.FindFirstObjectByType<CreditSystem>();
            if (creditSystem == null)
                return;

            if (gameManager != null)
            {
                creditSystem.SetCredits(gameManager.PlayerCredits, "MissionResultSync");
                return;
            }

            if (missionResult.CreditsEarned > 0)
                creditSystem.EarnCredits(missionResult.CreditsEarned, "MissionCompleteFallback");
        }

        private int GetCollectedIntelCount()
        {
            NarrativeManager narrative = NarrativeManager.Instance;
            if (narrative == null)
                narrative = Object.FindFirstObjectByType<NarrativeManager>();
            if (narrative == null)
                return 0;

            return narrative.GetIntelCollectedForLevel(levelIndex);
        }

        private static SecondaryObjectiveSummary ResolveSecondaryObjectiveSummary(GameManager gameManager)
        {
            if (SecondaryObjectiveTracker.TryGetActiveSummary(out SecondaryObjectiveSummary summary))
            {
                if (summary.total <= 0)
                {
                    summary.total = 2;
                    summary.completed = Mathf.Clamp(summary.completed, 0, summary.total);
                }
                return summary;
            }

            int count = 0;
            if (gameManager == null)
            {
                return new SecondaryObjectiveSummary
                {
                    levelIndex = 0,
                    completed = 0,
                    total = 2
                };
            }

            if (!gameManager.WasDiscovered)
                count++;
            if (!gameManager.FullAlertTriggered)
                count++;

            return new SecondaryObjectiveSummary
            {
                levelIndex = gameManager != null ? gameManager.CurrentLevelIndex : 0,
                completed = count,
                total = 2
            };
        }

        private float ResolveMissionTimeBudget()
        {
            LevelLoader loader = Object.FindFirstObjectByType<LevelLoader>();
            if (loader == null)
                return 0f;

            var levelData = loader.GetLevelData();
            if (levelData == null)
                return 0f;

            if (levelData.timeLimit > 0f)
                return levelData.timeLimit;
            if (levelData.standardTime > 0f)
                return levelData.standardTime;

            return 0f;
        }

        private static int RankToScore(string rank)
        {
            if (string.IsNullOrEmpty(rank))
                return 1;

            switch (rank.ToUpperInvariant())
            {
                case "S": return 5;
                case "A": return 4;
                case "B": return 3;
                case "C": return 2;
                default: return 1;
            }
        }

        private void EnsureTriggerCollider()
        {
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                BoxCollider box = gameObject.AddComponent<BoxCollider>();
                box.size = new Vector3(2f, 2f, 2f);
                box.isTrigger = true;
                return;
            }

            col.isTrigger = true;
        }

        private float ResolveTransitionDelaySeconds()
        {
            if (!showDebriefBeforeTransition)
                return 0f;

            MissionDebriefUI debrief = Object.FindFirstObjectByType<MissionDebriefUI>();
            if (debrief != null)
                return Mathf.Max(0f, debrief.TransitionHoldSeconds);

            return Mathf.Max(0f, fallbackDebriefDelaySeconds);
        }

        private global::System.Collections.IEnumerator TransitionAfterDebrief(LevelFlowManager flow, float delaySeconds)
        {
            if (delaySeconds > 0f)
                yield return new WaitForSecondsRealtime(delaySeconds);

            ExecuteFlowTransition(flow);
        }

        private void ExecuteFlowTransition(LevelFlowManager flow)
        {
            if (flow == null)
                return;

            if (loadNextLevel)
                flow.LoadNextLevel();
            else
                flow.LoadMainMenu();
        }
    }
}
