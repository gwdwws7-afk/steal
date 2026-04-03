using System.Collections.Generic;
using INTIFALL.AI;
using INTIFALL.Growth;
using INTIFALL.Input;
using INTIFALL.Player;
using INTIFALL.Tools;
using UnityEngine;

namespace INTIFALL.System
{
    public enum EGameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        LevelComplete
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private EGameState _currentState = EGameState.MainMenu;
        [SerializeField] private int _currentLevelIndex;
        [SerializeField] private string _currentLevelName = string.Empty;

        [Header("Player Stats")]
        [SerializeField] private int _playerCredits;
        [SerializeField] private float _playTime;

        [Header("Mission Stats")]
        [SerializeField] private int _enemiesKilled;
        [SerializeField] private int _enemiesKnockedOut;
        [SerializeField] private bool _wasDiscovered;
        [SerializeField] private bool _fullAlertTriggered;
        [SerializeField] private bool _tookDamage;
        [SerializeField] private int _toolsUsed;
        [SerializeField] private int _alertsTriggered;
        [SerializeField] private float _toolCooldownLoad;
        [SerializeField] private int _ropeToolUses;
        [SerializeField] private int _smokeToolUses;
        [SerializeField] private int _soundBaitToolUses;
        [SerializeField] private MissionResult _lastMissionResult;
        [SerializeField] private bool _hasLastMissionResult;

        private readonly Dictionary<int, EAlertState> _enemyAlertStateById = new();

        public EGameState CurrentState => _currentState;
        public int CurrentLevelIndex => _currentLevelIndex;
        public string CurrentLevelName => _currentLevelName;
        public int PlayerCredits => _playerCredits;
        public float PlayTime => _playTime;
        public int EnemiesKilled => _enemiesKilled;
        public int EnemiesKnockedOut => _enemiesKnockedOut;
        public bool WasDiscovered => _wasDiscovered;
        public bool FullAlertTriggered => _fullAlertTriggered;
        public bool TookDamage => _tookDamage;
        public int ToolsUsed => _toolsUsed;
        public int AlertsTriggered => _alertsTriggered;
        public float ToolCooldownLoad => _toolCooldownLoad;
        public int RopeToolUses => _ropeToolUses;
        public int SmokeToolUses => _smokeToolUses;
        public int SoundBaitToolUses => _soundBaitToolUses;
        public bool HasLastMissionResult => _hasLastMissionResult;
        public MissionResult LastMissionResult => _lastMissionResult;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (transform.parent != null)
                transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Subscribe<AlertStateChangedEvent>(OnAlertStateChanged);
            EventBus.Subscribe<HPChangedEvent>(OnHPChanged);
            EventBus.Subscribe<ToolUsedEvent>(OnToolUsed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Unsubscribe<AlertStateChangedEvent>(OnAlertStateChanged);
            EventBus.Unsubscribe<HPChangedEvent>(OnHPChanged);
            EventBus.Unsubscribe<ToolUsedEvent>(OnToolUsed);
        }

        private void Update()
        {
            if (_currentState == EGameState.Playing)
                _playTime += Time.deltaTime;

            if (InputCompat.GetKeyDown(KeyCode.Escape))
                TogglePause();
        }

        public void StartGame()
        {
            _currentState = EGameState.Playing;
            _playTime = 0f;
            ResetMissionStats();
        }

        public void LoadLevel(int levelIndex, string levelName)
        {
            _currentLevelIndex = levelIndex;
            _currentLevelName = levelName;
            _currentState = EGameState.Playing;
            _playTime = 0f;
            ResetMissionStats();
        }

        public void PauseGame()
        {
            if (_currentState != EGameState.Playing)
                return;

            _currentState = EGameState.Paused;
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            if (_currentState != EGameState.Paused)
                return;

            _currentState = EGameState.Playing;
            Time.timeScale = 1f;
        }

        public void TogglePause()
        {
            if (_currentState == EGameState.Playing)
                PauseGame();
            else if (_currentState == EGameState.Paused)
                ResumeGame();
        }

        public void GameOver()
        {
            _currentState = EGameState.GameOver;
        }

        public void LevelComplete()
        {
            _currentState = EGameState.LevelComplete;
        }

        public void AddCredits(int amount)
        {
            _playerCredits += amount;
        }

        public void SpendCredits(int amount)
        {
            _playerCredits = Mathf.Max(0, _playerCredits - amount);
        }

        public void RecordEnemyKill()
        {
            _enemiesKilled++;
        }

        public void RecordEnemyKnockout()
        {
            _enemiesKnockedOut++;
        }

        public void RecordDiscovery()
        {
            _wasDiscovered = true;
        }

        public void RecordFullAlert()
        {
            _fullAlertTriggered = true;
        }

        public MissionResult CalculateMissionResult()
        {
            bool zeroKill = _enemiesKilled == 0;
            bool noDamage = !_tookDamage;
            bool isSRank = !_wasDiscovered && !_fullAlertTriggered &&
                           zeroKill && _enemiesKnockedOut > 0 && noDamage;

            int rankScore = isSRank ? 5 : (_fullAlertTriggered ? 2 : 3);
            int creditsEarned = LevelUpReward.EvaluateCredits(
                rankScore,
                zeroKill,
                noDamage,
                0,
                0,
                0f);

            return BuildMissionResult(
                zeroKill,
                noDamage,
                rankScore,
                creditsEarned,
                intelCollected: 0,
                intelRequired: 0,
                secondaryObjectivesCompleted: 0,
                secondaryObjectivesEvaluated: 0,
                secondaryObjectivesTotal: 2,
                extractionRouteId: "main",
                extractionRouteLabel: "Main Extraction",
                isMainRoute: true,
                routeRiskTier: 0,
                routeCreditMultiplier: 1f,
                toolRiskWindowAdjustment: 0);
        }

        public MissionResult CalculateMissionResult(
            int secondaryObjectivesCompleted,
            int intelCollected,
            int intelRequired,
            float timeBudgetSeconds)
        {
            return CalculateMissionResult(
                secondaryObjectivesCompleted,
                intelCollected,
                intelRequired,
                timeBudgetSeconds,
                extractionRouteId: "main",
                extractionRouteLabel: "Main Extraction",
                isMainRoute: true,
                routeRiskTier: 0,
                routeCreditMultiplier: 1f,
                routeSecondaryObjectiveBonus: 0,
                secondaryObjectivesTotal: 2);
        }

        public MissionResult CalculateMissionResult(
            int secondaryObjectivesCompleted,
            int intelCollected,
            int intelRequired,
            float timeBudgetSeconds,
            string extractionRouteId,
            string extractionRouteLabel,
            bool isMainRoute,
            int routeRiskTier,
            float routeCreditMultiplier,
            int routeSecondaryObjectiveBonus,
            int secondaryObjectivesTotal = 2)
        {
            bool zeroKill = _enemiesKilled == 0;
            bool noDamage = !_tookDamage;
            float timeRemaining = timeBudgetSeconds > 0f
                ? Mathf.Max(0f, timeBudgetSeconds - _playTime)
                : 0f;

            int normalizedRisk = Mathf.Clamp(routeRiskTier, 0, 3);
            float normalizedMultiplier = Mathf.Clamp(routeCreditMultiplier, 0.5f, 2f);
            int secondaryBaseTotal = secondaryObjectivesTotal > 0 ? secondaryObjectivesTotal : 2;
            int secondaryBase = Mathf.Clamp(secondaryObjectivesCompleted, 0, secondaryBaseTotal);
            int secondaryBonus = Mathf.Max(0, routeSecondaryObjectiveBonus);
            int secondaryTotal = secondaryBaseTotal + secondaryBonus;
            int secondaryEvaluated = Mathf.Clamp(secondaryBase + secondaryBonus, 0, secondaryTotal);

            int rankScore = LevelUpReward.EvaluateRankScore(
                zeroKill,
                noDamage,
                secondaryEvaluated,
                intelCollected,
                timeRemaining);

            if (!isMainRoute && normalizedRisk >= 2 && !_wasDiscovered && !_fullAlertTriggered)
                rankScore = Mathf.Min(5, rankScore + 1);

            if (_wasDiscovered)
                rankScore = Mathf.Min(rankScore, 4);
            if (_fullAlertTriggered)
                rankScore = Mathf.Min(rankScore, 2);

            int creditsEarned = LevelUpReward.EvaluateCredits(
                rankScore,
                zeroKill,
                noDamage,
                secondaryEvaluated,
                intelCollected,
                timeRemaining);

            float pressureFactor = Mathf.Clamp(1f - (_alertsTriggered * 0.06f), 0.7f, 1f);
            creditsEarned = Mathf.RoundToInt(creditsEarned * normalizedMultiplier * pressureFactor);

            int routeRiskBonus = normalizedRisk * 25;
            int optionalDisciplineBonus = 0;
            if (!isMainRoute)
            {
                optionalDisciplineBonus = Mathf.Max(20, 70 - (_alertsTriggered * 15));
            }

            int pressurePenalty = Mathf.Max(0, _alertsTriggered - 1) * 25;
            pressurePenalty += Mathf.Max(0, _toolsUsed - 4) * 8;
            if (!isMainRoute && _fullAlertTriggered)
                pressurePenalty += 90;

            int toolWindowAdjustment = EvaluateToolRiskWindowAdjustment(normalizedRisk, isMainRoute);
            creditsEarned += routeRiskBonus + optionalDisciplineBonus + toolWindowAdjustment - pressurePenalty;
            creditsEarned += EvaluateRankBandCreditOffset(rankScore);
            creditsEarned = Mathf.Max(0, creditsEarned);

            return BuildMissionResult(
                zeroKill,
                noDamage,
                rankScore,
                creditsEarned,
                intelCollected,
                intelRequired,
                secondaryBase,
                secondaryEvaluated,
                secondaryTotal,
                extractionRouteId,
                extractionRouteLabel,
                isMainRoute,
                normalizedRisk,
                normalizedMultiplier,
                toolWindowAdjustment);
        }

        private MissionResult BuildMissionResult(
            bool zeroKill,
            bool noDamage,
            int rankScore,
            int creditsEarned,
            int intelCollected,
            int intelRequired,
            int secondaryObjectivesCompleted,
            int secondaryObjectivesEvaluated,
            int secondaryObjectivesTotal,
            string extractionRouteId,
            string extractionRouteLabel,
            bool isMainRoute,
            int routeRiskTier,
            float routeCreditMultiplier,
            int toolRiskWindowAdjustment)
        {
            int normalizedSecondaryTotal = secondaryObjectivesTotal > 0 ? secondaryObjectivesTotal : 2;
            int normalizedSecondaryCompleted = Mathf.Clamp(secondaryObjectivesCompleted, 0, normalizedSecondaryTotal);
            int normalizedSecondaryEvaluated = Mathf.Clamp(secondaryObjectivesEvaluated, 0, normalizedSecondaryTotal);
            int normalizedRouteRisk = Mathf.Clamp(routeRiskTier, 0, 3);
            float normalizedRouteMultiplier = Mathf.Clamp(routeCreditMultiplier, 0.5f, 2f);

            return new MissionResult
            {
                LevelName = _currentLevelName,
                PlayTime = _playTime,
                WasDiscovered = _wasDiscovered,
                FullAlertTriggered = _fullAlertTriggered,
                EnemiesKilled = _enemiesKilled,
                EnemiesKnockedOut = _enemiesKnockedOut,
                Rank = LevelUpReward.GetRankNameForScore(rankScore),
                RankScore = rankScore,
                CreditsEarned = Mathf.Max(0, creditsEarned),
                ZeroKill = zeroKill,
                NoDamage = noDamage,
                IntelCollected = Mathf.Max(0, intelCollected),
                IntelRequired = Mathf.Max(0, intelRequired),
                SecondaryObjectivesCompleted = normalizedSecondaryCompleted,
                SecondaryObjectivesEvaluated = normalizedSecondaryEvaluated,
                SecondaryObjectivesTotal = normalizedSecondaryTotal,
                ExtractionRouteId = string.IsNullOrWhiteSpace(extractionRouteId) ? "main" : extractionRouteId,
                ExtractionRouteLabel = string.IsNullOrWhiteSpace(extractionRouteLabel) ? "Main Extraction" : extractionRouteLabel,
                UsedOptionalExit = !isMainRoute,
                RouteRiskTier = normalizedRouteRisk,
                RouteCreditMultiplier = normalizedRouteMultiplier,
                ToolsUsed = _toolsUsed,
                AlertsTriggered = _alertsTriggered,
                ToolRiskWindowAdjustment = toolRiskWindowAdjustment,
                ToolCooldownLoad = _toolCooldownLoad,
                RopeToolUses = _ropeToolUses,
                SmokeToolUses = _smokeToolUses,
                SoundBaitToolUses = _soundBaitToolUses
            };
        }

        private static int EvaluateRankBandCreditOffset(int rankScore)
        {
            return rankScore switch
            {
                5 => 120,
                4 => 60,
                3 => 0,
                2 => -55,
                _ => -120
            };
        }

        private int EvaluateToolRiskWindowAdjustment(int normalizedRisk, bool isMainRoute)
        {
            if (_toolsUsed <= 0)
                return 0;

            float averageCooldown = _toolCooldownLoad / Mathf.Max(1, _toolsUsed);
            bool hasRope = _ropeToolUses > 0;
            bool hasSmoke = _smokeToolUses > 0;
            bool hasSoundBait = _soundBaitToolUses > 0;
            int adjustment = 0;

            switch (normalizedRisk)
            {
                case 0:
                    if (!isMainRoute && hasSoundBait && averageCooldown >= 4f && averageCooldown <= 10f && _toolsUsed <= 4)
                        adjustment += 12;
                    if (averageCooldown < 2.5f)
                        adjustment -= 10;
                    break;

                case 1:
                    if (hasSoundBait && averageCooldown >= 5f && averageCooldown <= 10f && _toolsUsed <= 4)
                        adjustment += 18;
                    if (averageCooldown < 3f || _toolsUsed > 6)
                        adjustment -= 20;
                    break;

                case 2:
                    if (hasSmoke && hasSoundBait && averageCooldown >= 6f && averageCooldown <= 13f && _toolsUsed >= 2 && _toolsUsed <= 6)
                        adjustment += 35;
                    else if (hasSmoke && averageCooldown >= 6f && averageCooldown <= 14f && _toolsUsed <= 6)
                        adjustment += 15;

                    if (averageCooldown < 3f || _toolsUsed > 7)
                        adjustment -= 28;
                    break;

                default:
                    if (hasRope && hasSmoke && averageCooldown >= 7f && averageCooldown <= 16f && _toolsUsed >= 2 && _toolsUsed <= 6)
                        adjustment += 60;
                    else if (hasRope && (hasSmoke || hasSoundBait) && averageCooldown >= 6f && averageCooldown <= 16f && _toolsUsed <= 7)
                        adjustment += 30;

                    if (averageCooldown < 3f || _toolsUsed > 8)
                        adjustment -= 40;
                    break;
            }

            int overuse = Mathf.Max(0, _toolsUsed - 6);
            adjustment -= overuse * 6;
            return adjustment;
        }

        public void ApplyMissionResult(MissionResult result)
        {
            _lastMissionResult = result;
            _hasLastMissionResult = true;

            if (result.CreditsEarned > 0)
                AddCredits(result.CreditsEarned);

            LevelComplete();
        }

        private void ResetMissionStats()
        {
            _enemiesKilled = 0;
            _enemiesKnockedOut = 0;
            _wasDiscovered = false;
            _fullAlertTriggered = false;
            _tookDamage = false;
            _toolsUsed = 0;
            _alertsTriggered = 0;
            _toolCooldownLoad = 0f;
            _ropeToolUses = 0;
            _smokeToolUses = 0;
            _soundBaitToolUses = 0;
            _enemyAlertStateById.Clear();
        }

        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            if (_currentState != EGameState.Playing)
                return;

            RecordEnemyKill();
        }

        private void OnAlertStateChanged(AlertStateChangedEvent evt)
        {
            if (_currentState != EGameState.Playing)
                return;

            EAlertState previousState = EAlertState.Unaware;
            if (_enemyAlertStateById.TryGetValue(evt.enemyId, out EAlertState knownState))
                previousState = knownState;
            _enemyAlertStateById[evt.enemyId] = evt.newState;

            bool wasAlertedBefore = previousState == EAlertState.Alert || previousState == EAlertState.FullAlert;
            bool isAlertedNow = evt.newState == EAlertState.Alert || evt.newState == EAlertState.FullAlert;

            if (isAlertedNow && !wasAlertedBefore)
                _alertsTriggered++;

            if (isAlertedNow)
                RecordDiscovery();

            if (evt.newState == EAlertState.FullAlert)
                RecordFullAlert();
        }

        private void OnHPChanged(HPChangedEvent evt)
        {
            if (_currentState != EGameState.Playing)
                return;

            if (!evt.isHealing && evt.changeAmount > 0)
                _tookDamage = true;
        }

        private void OnToolUsed(ToolUsedEvent evt)
        {
            if (_currentState != EGameState.Playing)
                return;

            _toolsUsed++;
            _toolCooldownLoad += Mathf.Max(0f, evt.cooldownSeconds);

            string normalizedTool = string.IsNullOrWhiteSpace(evt.toolName)
                ? string.Empty
                : evt.toolName.Trim().ToLowerInvariant();

            if (normalizedTool.Contains("rope"))
                _ropeToolUses++;

            if (normalizedTool.Contains("smoke"))
                _smokeToolUses++;

            if (normalizedTool.Contains("soundbait") || normalizedTool.Contains("sound bait"))
                _soundBaitToolUses++;
        }
    }

    [global::System.Serializable]
    public struct MissionResult
    {
        public string LevelName;
        public float PlayTime;
        public bool WasDiscovered;
        public bool FullAlertTriggered;
        public int EnemiesKilled;
        public int EnemiesKnockedOut;
        public string Rank;
        public int RankScore;
        public int CreditsEarned;
        public bool ZeroKill;
        public bool NoDamage;
        public int IntelCollected;
        public int IntelRequired;
        public int SecondaryObjectivesCompleted;
        public int SecondaryObjectivesEvaluated;
        public int SecondaryObjectivesTotal;
        public string ExtractionRouteId;
        public string ExtractionRouteLabel;
        public bool UsedOptionalExit;
        public int RouteRiskTier;
        public float RouteCreditMultiplier;
        public int ToolsUsed;
        public int AlertsTriggered;
        public int ToolRiskWindowAdjustment;
        public float ToolCooldownLoad;
        public int RopeToolUses;
        public int SmokeToolUses;
        public int SoundBaitToolUses;
    }
}
