using System.Collections.Generic;
using INTIFALL.Level;
using INTIFALL.System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace INTIFALL.Narrative
{
    public enum EWillaTrigger
    {
        MissionStart,
        IntelFound,
        MissionComplete,
        StoryReveal,
        Warning,
        Betrayal
    }

    public struct WillaMessageEvent
    {
        public EWillaTrigger trigger;
        public int levelIndex;
        public string messageKey;
    }

    public class WillaComm : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject commPanel;
        [SerializeField] private Text speakerNameText;
        [SerializeField] private Text messageText;
        [SerializeField] private Image commIcon;
        [SerializeField] private float typingSpeed = 0.05f;
        [SerializeField] private float autoCloseDelay = 5f;

        [Header("Audio")]
        [SerializeField] private AudioClip incomingCommSound;
        [SerializeField] private AudioClip commEndSound;

        [Header("Message Catalog")]
        [SerializeField] private TextAsset messageCatalogJson;
        [SerializeField] private bool preferResourceCatalog = true;
        [SerializeField] private string messageCatalogResourcePath = "INTIFALL/Narrative/WillaMessages";

        [Header("Auto Trigger")]
        [SerializeField] private bool autoBindEventBus = true;
        [SerializeField] private bool autoMissionStartOnLevelLoad = true;
        [SerializeField] private bool autoIntelFoundOnFirstCollect = true;
        [SerializeField] private bool autoMissionCompleteOnOutcome = true;
        [SerializeField] private bool autoWarningOnThreatSpike = true;
        [SerializeField] private bool autoStoryRevealOnNarrativeEvent = true;
        [SerializeField] private bool autoEndingBranchOnFinalOutcome = true;

        private bool _isDisplaying;
        private bool _isTyping;
        private float _displayTimer;
        private AudioSource _audioSource;
        private EWillaTrigger _currentTrigger;
        private int _currentLevel;
        private Dictionary<WillaMessageCatalog.MessageKey, string[]> _messageCatalog;
        private readonly Queue<WillaCommRequest> _pendingMessages = new();
        private readonly HashSet<int> _missionStartAnnouncedLevels = new();
        private readonly HashSet<int> _intelFoundAnnouncedLevels = new();
        private readonly HashSet<int> _missionCompleteAnnouncedLevels = new();
        private readonly HashSet<int> _warningAnnouncedLevels = new();
        private readonly HashSet<int> _storyRevealAnnouncedLevels = new();
        private readonly HashSet<int> _betrayalAnnouncedLevels = new();
        private readonly Dictionary<int, MissionOutcomeSnapshot> _latestMissionOutcomeByLevel = new();

        private struct WillaCommRequest
        {
            public EWillaTrigger Trigger;
            public int LevelIndex;
        }

        private struct MissionOutcomeSnapshot
        {
            public string Rank;
            public int RankScore;
            public int CreditsEarned;
            public int IntelCollected;
            public int IntelRequired;
            public int SecondaryObjectivesCompleted;
            public int SecondaryObjectivesTotal;
            public bool ZeroKill;
            public bool NoDamage;
            public bool WasDiscovered;
            public bool FullAlertTriggered;
            public string ExtractionRouteLabel;
            public bool UsedOptionalExit;
            public int RouteRiskTier;
            public float RouteCreditMultiplier;
            public int ToolsUsed;
            public int AlertsTriggered;
        }

        public bool IsDisplaying => _isDisplaying;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();

            if (commPanel != null)
                commPanel.SetActive(false);

            ReloadMessageCatalog();
        }

        private void Start()
        {
            if (!autoMissionStartOnLevelLoad)
                return;

            if (autoBindEventBus)
                return;

            int levelIndex = ResolveCurrentLevelIndex();
            TryTriggerMissionStart(levelIndex);
        }

        private void OnEnable()
        {
            if (!autoBindEventBus)
                return;

            EventBus.Subscribe<LevelLoadedEvent>(OnLevelLoaded);
            EventBus.Subscribe<IntelCollectedInSceneEvent>(OnIntelCollectedInScene);
            EventBus.Subscribe<MissionOutcomeEvaluatedEvent>(OnMissionOutcomeEvaluated);
            EventBus.Subscribe<AlertStateChangedEvent>(OnAlertStateChanged);
            EventBus.Subscribe<NarrativeTriggeredEvent>(OnNarrativeTriggered);
        }

        private void OnDisable()
        {
            if (!autoBindEventBus)
                return;

            EventBus.Unsubscribe<LevelLoadedEvent>(OnLevelLoaded);
            EventBus.Unsubscribe<IntelCollectedInSceneEvent>(OnIntelCollectedInScene);
            EventBus.Unsubscribe<MissionOutcomeEvaluatedEvent>(OnMissionOutcomeEvaluated);
            EventBus.Unsubscribe<AlertStateChangedEvent>(OnAlertStateChanged);
            EventBus.Unsubscribe<NarrativeTriggeredEvent>(OnNarrativeTriggered);
        }

        private void Update()
        {
            if (_isDisplaying && !_isTyping)
            {
                _displayTimer += Time.deltaTime;
                if (_displayTimer >= autoCloseDelay)
                    CloseComm();
            }
        }

        public void ReloadMessageCatalog()
        {
            TextAsset selectedCatalog = messageCatalogJson;
            if (preferResourceCatalog && !string.IsNullOrEmpty(messageCatalogResourcePath))
            {
                TextAsset loaded = Resources.Load<TextAsset>(messageCatalogResourcePath);
                if (loaded != null)
                    selectedCatalog = loaded;
            }

            string json = selectedCatalog != null ? selectedCatalog.text : string.Empty;
            _messageCatalog = WillaMessageCatalog.BuildEffectiveCatalog(json, out int importedEntries, out string warning);

            if (!string.IsNullOrEmpty(warning))
                Debug.LogWarning($"WillaComm: message catalog warning: {warning}");

            if (importedEntries > 0)
                Debug.Log($"WillaComm: loaded {importedEntries} custom message entries.");
        }

        public void TriggerComm(EWillaTrigger trigger, int levelIndex)
        {
            if (_isDisplaying)
            {
                _pendingMessages.Enqueue(new WillaCommRequest
                {
                    Trigger = trigger,
                    LevelIndex = levelIndex
                });
                return;
            }

            if (!TryDisplayMessage(trigger, levelIndex))
                return;
        }

        private bool TryDisplayMessage(EWillaTrigger trigger, int levelIndex)
        {
            string[] messages = GetMessagesForTrigger(trigger, levelIndex);
            if (messages == null || messages.Length == 0)
                return false;

            string template = messages[Random.Range(0, messages.Length)];
            string message = ResolveRuntimeMessage(trigger, levelIndex, template);
            DisplayMessage(trigger, levelIndex, message);
            return true;
        }

        private string[] GetMessagesForTrigger(EWillaTrigger trigger, int levelIndex)
        {
            if (_messageCatalog == null)
                ReloadMessageCatalog();

            int normalizedLevel = Mathf.Max(0, levelIndex);
            if (_messageCatalog != null &&
                _messageCatalog.TryGetValue(new WillaMessageCatalog.MessageKey(normalizedLevel, trigger), out string[] levelSpecific))
            {
                return levelSpecific;
            }

            if (_messageCatalog != null &&
                _messageCatalog.TryGetValue(new WillaMessageCatalog.MessageKey(-1, trigger), out string[] fallback))
            {
                return fallback;
            }

            return global::System.Array.Empty<string>();
        }

        private string ResolveRuntimeMessage(EWillaTrigger trigger, int levelIndex, string template)
        {
            if (string.IsNullOrEmpty(template))
                return string.Empty;

            if (trigger != EWillaTrigger.MissionComplete)
                return template;

            MissionOutcomeSnapshot snapshot = GetMissionOutcomeSnapshot(levelIndex);
            return ApplyMissionOutcomeTokens(template, snapshot);
        }

        private MissionOutcomeSnapshot GetMissionOutcomeSnapshot(int levelIndex)
        {
            int normalizedLevel = Mathf.Max(0, levelIndex);
            if (_latestMissionOutcomeByLevel.TryGetValue(normalizedLevel, out MissionOutcomeSnapshot snapshot))
                return snapshot;

            return new MissionOutcomeSnapshot
            {
                Rank = "N/A",
                RankScore = 0,
                CreditsEarned = 0,
                IntelCollected = 0,
                IntelRequired = 0,
                SecondaryObjectivesCompleted = 0,
                SecondaryObjectivesTotal = 2,
                ZeroKill = false,
                NoDamage = false,
                WasDiscovered = true,
                FullAlertTriggered = false,
                ExtractionRouteLabel = LocalizationService.Get("route.main", fallbackEnglish: "Main Extraction", fallbackChinese: string.Empty),
                UsedOptionalExit = false,
                RouteRiskTier = 0,
                RouteCreditMultiplier = 1f,
                ToolsUsed = 0,
                AlertsTriggered = 0
            };
        }

        private static string ApplyMissionOutcomeTokens(string template, MissionOutcomeSnapshot snapshot)
        {
            int intelMissing = Mathf.Max(0, snapshot.IntelRequired - snapshot.IntelCollected);
            int secondaryTotal = snapshot.SecondaryObjectivesTotal <= 0 ? 2 : snapshot.SecondaryObjectivesTotal;
            string stealthStatus = GetStealthStatus(snapshot.WasDiscovered, snapshot.FullAlertTriggered);
            string combatStyle = snapshot.ZeroKill
                ? LocalizationService.Get("debrief.combat.zero_kill", fallbackEnglish: "Zero-Kill", fallbackChinese: string.Empty)
                : LocalizationService.Get("debrief.combat.lethal", fallbackEnglish: "Lethal", fallbackChinese: string.Empty);
            string damageStatus = snapshot.NoDamage
                ? LocalizationService.Get("willa.damage.no_damage", fallbackEnglish: "No-Damage", fallbackChinese: string.Empty)
                : LocalizationService.Get("willa.damage.took_damage", fallbackEnglish: "Took Damage", fallbackChinese: string.Empty);
            string routeLabel = string.IsNullOrWhiteSpace(snapshot.ExtractionRouteLabel)
                ? LocalizationService.Get("route.main", fallbackEnglish: "Main Extraction", fallbackChinese: string.Empty)
                : snapshot.ExtractionRouteLabel;
            string routeType = snapshot.UsedOptionalExit
                ? LocalizationService.Get("debrief.route.optional", fallbackEnglish: "Optional", fallbackChinese: string.Empty)
                : LocalizationService.Get("debrief.route.main", fallbackEnglish: "Main", fallbackChinese: string.Empty);

            return template
                .Replace("{rank}", string.IsNullOrEmpty(snapshot.Rank) ? LocalizationService.Get("common.na", fallbackEnglish: "N/A", fallbackChinese: string.Empty) : snapshot.Rank)
                .Replace("{rank_score}", snapshot.RankScore.ToString())
                .Replace("{credits}", snapshot.CreditsEarned.ToString())
                .Replace("{intel_collected}", snapshot.IntelCollected.ToString())
                .Replace("{intel_required}", snapshot.IntelRequired.ToString())
                .Replace("{intel_missing}", intelMissing.ToString())
                .Replace("{secondary_completed}", snapshot.SecondaryObjectivesCompleted.ToString())
                .Replace("{secondary_total}", secondaryTotal.ToString())
                .Replace("{stealth_status}", stealthStatus)
                .Replace("{combat_style}", combatStyle)
                .Replace("{damage_status}", damageStatus)
                .Replace("{route_label}", routeLabel)
                .Replace("{route_type}", routeType)
                .Replace("{route_risk}", snapshot.RouteRiskTier.ToString())
                .Replace("{route_multiplier}", snapshot.RouteCreditMultiplier.ToString("0.00"))
                .Replace("{tools_used}", snapshot.ToolsUsed.ToString())
                .Replace("{alerts_triggered}", snapshot.AlertsTriggered.ToString());
        }

        private static string GetStealthStatus(bool wasDiscovered, bool fullAlertTriggered)
        {
            if (!wasDiscovered && !fullAlertTriggered)
                return LocalizationService.Get("debrief.stealth.undetected", fallbackEnglish: "Undetected", fallbackChinese: string.Empty);
            if (fullAlertTriggered)
                return LocalizationService.Get("debrief.stealth.full_alert", fallbackEnglish: "Full Alert", fallbackChinese: string.Empty);
            return LocalizationService.Get("debrief.stealth.spotted", fallbackEnglish: "Spotted", fallbackChinese: string.Empty);
        }

        private void DisplayMessage(EWillaTrigger trigger, int levelIndex, string message)
        {
            if (_isDisplaying)
                return;

            _isDisplaying = true;
            _isTyping = true;
            _displayTimer = 0f;
            _currentTrigger = trigger;
            _currentLevel = levelIndex;

            if (commPanel != null)
                commPanel.SetActive(true);

            if (speakerNameText != null)
            {
                speakerNameText.text = LocalizationService.Get(
                    "willa.speaker.secure",
                    fallbackEnglish: "[Willa - Secure Channel]",
                    fallbackChinese: string.Empty);
            }

            if (messageText != null)
                messageText.text = string.Empty;

            if (incomingCommSound != null)
                _audioSource.PlayOneShot(incomingCommSound);

            StartCoroutine(TypeMessage(message));
        }

        private global::System.Collections.IEnumerator TypeMessage(string message)
        {
            _isTyping = true;

            if (messageText != null)
            {
                float interval = Mathf.Max(0f, typingSpeed);
                for (int i = 0; i < message.Length; i++)
                {
                    messageText.text += message[i];
                    if (interval > 0f)
                        yield return new WaitForSeconds(interval);
                }
            }

            _isTyping = false;

            EventBus.Publish(new WillaMessageEvent
            {
                trigger = _currentTrigger,
                levelIndex = _currentLevel,
                messageKey = message
            });
        }

        private void OnLevelLoaded(LevelLoadedEvent evt)
        {
            _currentLevel = Mathf.Max(0, evt.levelIndex);
            ResetPerLevelTriggerState(_currentLevel);
            TryTriggerMissionStart(_currentLevel);
        }

        private void OnIntelCollectedInScene(IntelCollectedInSceneEvent evt)
        {
            if (!autoIntelFoundOnFirstCollect)
                return;

            int levelIndex = Mathf.Max(0, evt.levelIndex);
            if (_intelFoundAnnouncedLevels.Contains(levelIndex))
                return;

            _intelFoundAnnouncedLevels.Add(levelIndex);
            TriggerComm(EWillaTrigger.IntelFound, levelIndex);
        }

        private void OnMissionOutcomeEvaluated(MissionOutcomeEvaluatedEvent evt)
        {
            int levelIndex = Mathf.Max(0, evt.levelIndex);
            _latestMissionOutcomeByLevel[levelIndex] = new MissionOutcomeSnapshot
            {
                Rank = evt.rank,
                RankScore = evt.rankScore,
                CreditsEarned = evt.creditsEarned,
                IntelCollected = evt.intelCollected,
                IntelRequired = evt.intelRequired,
                SecondaryObjectivesCompleted = evt.secondaryObjectivesCompleted,
                SecondaryObjectivesTotal = evt.secondaryObjectivesTotal,
                ZeroKill = evt.zeroKill,
                NoDamage = evt.noDamage,
                WasDiscovered = evt.wasDiscovered,
                FullAlertTriggered = evt.fullAlertTriggered,
                ExtractionRouteLabel = evt.extractionRouteLabel,
                UsedOptionalExit = evt.usedOptionalExit,
                RouteRiskTier = evt.routeRiskTier,
                RouteCreditMultiplier = evt.routeCreditMultiplier,
                ToolsUsed = evt.toolsUsed,
                AlertsTriggered = evt.alertsTriggered
            };

            if (autoMissionCompleteOnOutcome && !_missionCompleteAnnouncedLevels.Contains(levelIndex))
            {
                _missionCompleteAnnouncedLevels.Add(levelIndex);
                TriggerComm(EWillaTrigger.MissionComplete, levelIndex);
            }

            if (!autoEndingBranchOnFinalOutcome || levelIndex < 4)
                return;

            bool betrayalBranch = evt.fullAlertTriggered || evt.rankScore <= 2;
            if (betrayalBranch)
                TryTriggerBetrayal(levelIndex);
            else
                TryTriggerStoryReveal(levelIndex);
        }

        private void OnAlertStateChanged(AlertStateChangedEvent evt)
        {
            if (!autoWarningOnThreatSpike)
                return;
            if (evt.newState != EAlertState.FullAlert)
                return;

            int levelIndex = ResolveActiveLevelIndex();
            TryTriggerWarning(levelIndex);
        }

        private void OnNarrativeTriggered(NarrativeTriggeredEvent evt)
        {
            if (!autoStoryRevealOnNarrativeEvent)
                return;

            int levelIndex = Mathf.Max(0, evt.levelIndex);
            switch (evt.eventType)
            {
                case ENarrativeEventType.BloodlineResonance:
                    TryTriggerStoryReveal(levelIndex);
                    break;
                case ENarrativeEventType.EndingChoice:
                    if (!string.IsNullOrWhiteSpace(evt.eventId) &&
                        evt.eventId.IndexOf("betray", global::System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        TryTriggerBetrayal(levelIndex);
                    }
                    else
                    {
                        TryTriggerStoryReveal(levelIndex);
                    }
                    break;
                case ENarrativeEventType.ScriptedTrigger:
                    TryTriggerFromScriptToken(levelIndex, evt.eventId);
                    break;
            }
        }

        private void TryTriggerFromScriptToken(int levelIndex, string tokenList)
        {
            if (string.IsNullOrWhiteSpace(tokenList))
                return;

            string[] tokens = tokenList.Split(
                new[] { ',', ';', '|' },
                global::System.StringSplitOptions.RemoveEmptyEntries);
            if (tokens == null || tokens.Length == 0)
                return;

            for (int i = 0; i < tokens.Length; i++)
            {
                string rawToken = tokens[i];
                if (string.IsNullOrWhiteSpace(rawToken))
                    continue;

                string token = rawToken.Trim().ToLowerInvariant();
                if (token.Contains("betray"))
                {
                    TryTriggerBetrayal(levelIndex);
                    continue;
                }

                if (token.Contains("warning") || token.Contains("alert") || token.Contains("threat"))
                {
                    TryTriggerWarning(levelIndex);
                    continue;
                }

                if (token.Contains("story") || token.Contains("reveal") || token.Contains("truth"))
                {
                    TryTriggerStoryReveal(levelIndex);
                }
            }
        }

        private void TryTriggerWarning(int levelIndex)
        {
            int normalizedLevel = Mathf.Max(0, levelIndex);
            if (_warningAnnouncedLevels.Contains(normalizedLevel))
                return;

            _warningAnnouncedLevels.Add(normalizedLevel);
            TriggerComm(EWillaTrigger.Warning, normalizedLevel);
        }

        private void TryTriggerStoryReveal(int levelIndex)
        {
            int normalizedLevel = Mathf.Max(0, levelIndex);
            if (_storyRevealAnnouncedLevels.Contains(normalizedLevel))
                return;

            _storyRevealAnnouncedLevels.Add(normalizedLevel);
            TriggerComm(EWillaTrigger.StoryReveal, normalizedLevel);
        }

        private void TryTriggerBetrayal(int levelIndex)
        {
            int normalizedLevel = Mathf.Max(0, levelIndex);
            if (_betrayalAnnouncedLevels.Contains(normalizedLevel))
                return;

            _betrayalAnnouncedLevels.Add(normalizedLevel);
            TriggerComm(EWillaTrigger.Betrayal, normalizedLevel);
        }

        private void TryTriggerMissionStart(int levelIndex)
        {
            if (!autoMissionStartOnLevelLoad)
                return;

            int normalizedLevel = Mathf.Max(0, levelIndex);
            if (_missionStartAnnouncedLevels.Contains(normalizedLevel))
                return;

            _missionStartAnnouncedLevels.Add(normalizedLevel);
            TriggerComm(EWillaTrigger.MissionStart, normalizedLevel);
        }

        private void ResetPerLevelTriggerState(int levelIndex)
        {
            int normalizedLevel = Mathf.Max(0, levelIndex);
            _missionStartAnnouncedLevels.Remove(normalizedLevel);
            _intelFoundAnnouncedLevels.Remove(normalizedLevel);
            _missionCompleteAnnouncedLevels.Remove(normalizedLevel);
            _warningAnnouncedLevels.Remove(normalizedLevel);
            _storyRevealAnnouncedLevels.Remove(normalizedLevel);
            _betrayalAnnouncedLevels.Remove(normalizedLevel);
            _latestMissionOutcomeByLevel.Remove(normalizedLevel);
        }

        private int ResolveActiveLevelIndex()
        {
            return Mathf.Max(0, _currentLevel);
        }

        private int ResolveCurrentLevelIndex()
        {
            LevelLoader loader = Object.FindFirstObjectByType<LevelLoader>();
            if (loader != null)
            {
                var levelData = loader.GetLevelData();
                if (levelData != null)
                    return Mathf.Max(0, levelData.levelIndex);
            }

            LevelFlowManager flow = Object.FindFirstObjectByType<LevelFlowManager>();
            if (flow != null)
                return Mathf.Max(0, flow.CurrentLevelIndex);

            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName.StartsWith("Level0") && sceneName.Length >= 7)
            {
                string indexText = sceneName.Substring(6, 1);
                if (int.TryParse(indexText, out int indexFromScene))
                    return Mathf.Max(0, indexFromScene - 1);
            }

            return 0;
        }

        private void TryDisplayNextPendingMessage()
        {
            if (_isDisplaying)
                return;

            while (_pendingMessages.Count > 0)
            {
                WillaCommRequest next = _pendingMessages.Dequeue();
                if (TryDisplayMessage(next.Trigger, next.LevelIndex))
                    break;
            }
        }

        public void CloseComm()
        {
            if (!_isDisplaying)
                return;

            _isDisplaying = false;
            _isTyping = false;
            _displayTimer = 0f;

            if (commPanel != null)
                commPanel.SetActive(false);

            if (commEndSound != null)
                _audioSource.PlayOneShot(commEndSound);

            TryDisplayNextPendingMessage();
        }

        public void SkipTyping()
        {
            if (!_isTyping)
                return;

            StopAllCoroutines();
            _isTyping = false;
        }
    }
}

