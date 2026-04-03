using INTIFALL.Economy;
using INTIFALL.Growth;
using INTIFALL.Level;
using INTIFALL.System;
using INTIFALL.Tools;
using UnityEngine;

namespace INTIFALL.Core
{
    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance { get; private set; }

        public const int CurrentSaveSchemaVersion = 2;
        public const int MaxSaveSlots = 3;

        private const string SaveKeyPrefix = "INTIFALL_SaveData_Slot_";
        private const string BackupSuffix = "_Backup";

        [Header("References")]
        [SerializeField] private CreditSystem creditSystem;
        [SerializeField] private ProgressionTree progressionTree;
        [SerializeField] private BloodlineSystem bloodlineSystem;
        [SerializeField] private LevelFlowManager levelFlow;
        [SerializeField] private ToolManager toolManager;
        [Range(0, MaxSaveSlots - 1)]
        [SerializeField] private int activeSlotIndex;

        private SaveData _currentSave;
        private int _currentLoadedSlotIndex = -1;

        public int ActiveSlotIndex => activeSlotIndex;
        public int CurrentLoadedSlotIndex => _currentLoadedSlotIndex;
        public bool HasSaveData => HasSaveDataInSlot(activeSlotIndex);
        public bool HasBackupData => HasBackupDataInSlot(activeSlotIndex);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void SetActiveSlot(int slotIndex)
        {
            activeSlotIndex = NormalizeSlot(slotIndex);
        }

        public bool SaveGame()
        {
            return SaveGame(activeSlotIndex);
        }

        public bool SaveGame(int slotIndex)
        {
            if (toolManager == null)
                toolManager = Object.FindFirstObjectByType<ToolManager>();

            int normalizedSlot = NormalizeSlot(slotIndex);
            GameManager gm = GameManager.Instance;
            SaveData data = BuildSaveData(gm, toolManager, normalizedSlot);

            string saveKey = GetPrimaryKey(normalizedSlot);
            string backupKey = GetBackupKey(normalizedSlot);

            bool backupUpdated = false;
            if (PlayerPrefs.HasKey(saveKey))
            {
                string previousJson = PlayerPrefs.GetString(saveKey);
                if (!string.IsNullOrWhiteSpace(previousJson))
                {
                    PlayerPrefs.SetString(backupKey, previousJson);
                    backupUpdated = true;
                }
            }

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(saveKey, json);
            PlayerPrefs.Save();

            _currentSave = data;
            _currentLoadedSlotIndex = normalizedSlot;

            EventBus.Publish(new GameSavedEvent
            {
                saveData = data,
                slotIndex = normalizedSlot,
                backupUpdated = backupUpdated
            });

            return true;
        }

        public bool LoadGame()
        {
            return LoadGame(activeSlotIndex);
        }

        public bool LoadGame(int slotIndex)
        {
            int normalizedSlot = NormalizeSlot(slotIndex);
            if (!HasSaveDataInSlot(normalizedSlot))
                return false;

            if (toolManager == null)
                toolManager = Object.FindFirstObjectByType<ToolManager>();

            string saveKey = GetPrimaryKey(normalizedSlot);
            string backupKey = GetBackupKey(normalizedSlot);

            bool loadedFromBackup = false;
            bool restoredPrimaryFromBackup = false;

            string json = PlayerPrefs.GetString(saveKey, string.Empty);
            SaveData data = DeserializeWithMigrationInternal(json, out int originalVersion, out bool migrated);
            if (data == null && PlayerPrefs.HasKey(backupKey))
            {
                string backupJson = PlayerPrefs.GetString(backupKey, string.Empty);
                data = DeserializeWithMigrationInternal(backupJson, out originalVersion, out migrated);
                if (data != null)
                {
                    loadedFromBackup = true;
                    PlayerPrefs.SetString(saveKey, backupJson);
                    PlayerPrefs.Save();
                    restoredPrimaryFromBackup = true;
                }
            }

            if (data == null)
                return false;

            ApplyLoadedData(data);

            _currentSave = data;
            _currentLoadedSlotIndex = normalizedSlot;

            EventBus.Publish(new GameLoadedEvent
            {
                saveData = data,
                slotIndex = normalizedSlot,
                loadedFromBackup = loadedFromBackup
            });

            if (restoredPrimaryFromBackup)
            {
                EventBus.Publish(new SaveRecoveredFromBackupEvent
                {
                    slotIndex = normalizedSlot
                });
            }

            if (migrated)
            {
                EventBus.Publish(new SaveMigrationAppliedEvent
                {
                    fromVersion = originalVersion,
                    toVersion = data.schemaVersion,
                    slotIndex = normalizedSlot,
                    loadedFromBackup = loadedFromBackup
                });
            }

            return true;
        }

        public void DeleteSave()
        {
            DeleteSave(activeSlotIndex);
        }

        public void DeleteSave(int slotIndex)
        {
            int normalizedSlot = NormalizeSlot(slotIndex);
            string saveKey = GetPrimaryKey(normalizedSlot);
            string backupKey = GetBackupKey(normalizedSlot);

            PlayerPrefs.DeleteKey(saveKey);
            PlayerPrefs.DeleteKey(backupKey);
            PlayerPrefs.Save();

            if (_currentLoadedSlotIndex == normalizedSlot)
            {
                _currentSave = null;
                _currentLoadedSlotIndex = -1;
            }
        }

        public SaveData GetCurrentSave()
        {
            return _currentSave;
        }

        public bool HasSaveDataInSlot(int slotIndex)
        {
            int normalizedSlot = NormalizeSlot(slotIndex);
            return PlayerPrefs.HasKey(GetPrimaryKey(normalizedSlot)) ||
                   PlayerPrefs.HasKey(GetBackupKey(normalizedSlot));
        }

        public bool HasBackupDataInSlot(int slotIndex)
        {
            int normalizedSlot = NormalizeSlot(slotIndex);
            return PlayerPrefs.HasKey(GetBackupKey(normalizedSlot));
        }

        public bool RestoreBackupToPrimary(int slotIndex)
        {
            int normalizedSlot = NormalizeSlot(slotIndex);
            string backupKey = GetBackupKey(normalizedSlot);
            string primaryKey = GetPrimaryKey(normalizedSlot);
            if (!PlayerPrefs.HasKey(backupKey))
                return false;

            string backupJson = PlayerPrefs.GetString(backupKey, string.Empty);
            SaveData data = DeserializeWithMigrationInternal(backupJson, out _, out _);
            if (data == null)
                return false;

            PlayerPrefs.SetString(primaryKey, backupJson);
            PlayerPrefs.Save();

            _currentSave = data;
            _currentLoadedSlotIndex = normalizedSlot;

            EventBus.Publish(new SaveRecoveredFromBackupEvent
            {
                slotIndex = normalizedSlot
            });

            return true;
        }

        public static string GetSaveKeyForSlot(int slotIndex)
        {
            return GetPrimaryKey(NormalizeSlot(slotIndex));
        }

        public static string GetBackupKeyForSlot(int slotIndex)
        {
            return GetBackupKey(NormalizeSlot(slotIndex));
        }

        public static SaveData DeserializeWithMigration(string json)
        {
            return DeserializeWithMigrationInternal(json, out _, out _);
        }

        private static SaveData DeserializeWithMigrationInternal(string json, out int originalVersion, out bool migrated)
        {
            originalVersion = 0;
            migrated = false;

            if (string.IsNullOrWhiteSpace(json))
                return null;
            if (!ContainsKnownSaveToken(json))
                return null;

            SaveData data;
            try
            {
                data = JsonUtility.FromJson<SaveData>(json);
            }
            catch
            {
                return null;
            }

            if (data == null)
                return null;

            originalVersion = Mathf.Max(0, data.schemaVersion);
            int normalizedVersion = originalVersion == 0 ? 1 : originalVersion;

            if (normalizedVersion < CurrentSaveSchemaVersion)
            {
                MigrateToCurrentSchema(data, normalizedVersion);
                data.schemaVersion = CurrentSaveSchemaVersion;
                migrated = true;
            }
            else
            {
                NormalizeSchema(data);
            }

            return data;
        }

        private static SaveData BuildSaveData(GameManager gm, ToolManager manager, int slotIndex)
        {
            int normalizedSlot = NormalizeSlot(slotIndex);
            SaveData data = new SaveData
            {
                schemaVersion = CurrentSaveSchemaVersion,
                slotIndex = normalizedSlot,
                saveId = global::System.Guid.NewGuid().ToString("N"),
                saveTimestampUtc = global::System.DateTime.UtcNow.ToString("o"),
                credits = gm != null ? gm.PlayerCredits : 0,
                highestLevel = 1,
                currentLevel = (gm?.CurrentLevelIndex ?? 0) + 1,
                bloodlineLevel = 0,
                totalPlayTime = gm != null ? gm.PlayTime : 0f,
                unlockedTools = manager != null ? manager.GetUnlockedToolNames() : global::System.Array.Empty<string>(),
                hasMissionSnapshot = gm != null && gm.HasLastMissionResult,
                lastMissionRank = "N/A",
                lastMissionRouteId = "main",
                lastMissionRouteLabel = "Main Extraction",
                lastMissionRouteMultiplier = 1f
            };

            if (Instance != null)
            {
                if (Instance.levelFlow != null)
                    data.highestLevel = Instance.levelFlow.HighestUnlockedLevel;
                if (Instance.bloodlineSystem != null)
                    data.bloodlineLevel = Instance.bloodlineSystem.CurrentLevel;
            }

            if (gm != null && gm.HasLastMissionResult)
                ApplyMissionSnapshot(data, gm.LastMissionResult);

            NormalizeSchema(data);
            return data;
        }

        private static void ApplyMissionSnapshot(SaveData data, MissionResult result)
        {
            data.lastMissionRank = result.Rank;
            data.lastMissionRankScore = result.RankScore;
            data.lastMissionCredits = result.CreditsEarned;
            data.lastMissionIntelCollected = result.IntelCollected;
            data.lastMissionIntelRequired = result.IntelRequired;
            data.lastMissionSecondaryCompleted = result.SecondaryObjectivesEvaluated;
            data.lastMissionSecondaryTotal = result.SecondaryObjectivesTotal;
            data.lastMissionUsedOptionalExit = result.UsedOptionalExit;
            data.lastMissionRouteId = result.ExtractionRouteId;
            data.lastMissionRouteLabel = result.ExtractionRouteLabel;
            data.lastMissionRouteRiskTier = result.RouteRiskTier;
            data.lastMissionRouteMultiplier = result.RouteCreditMultiplier;
            data.lastMissionToolsUsed = result.ToolsUsed;
            data.lastMissionAlertsTriggered = result.AlertsTriggered;
            data.lastMissionToolRiskWindowAdjustment = result.ToolRiskWindowAdjustment;
            data.lastMissionToolCooldownLoad = result.ToolCooldownLoad;
            data.lastMissionRopeToolUses = result.RopeToolUses;
            data.lastMissionSmokeToolUses = result.SmokeToolUses;
            data.lastMissionSoundBaitToolUses = result.SoundBaitToolUses;
        }

        private static void MigrateToCurrentSchema(SaveData data, int sourceVersion)
        {
            if (sourceVersion <= 1)
            {
                if (data.currentLevel <= 0)
                    data.currentLevel = Mathf.Max(1, data.highestLevel);

                data.slotIndex = NormalizeSlot(data.slotIndex);
                data.saveId = string.IsNullOrWhiteSpace(data.saveId) ? global::System.Guid.NewGuid().ToString("N") : data.saveId;
                data.hasMissionSnapshot = false;
                data.lastMissionRank = string.IsNullOrWhiteSpace(data.lastMissionRank) ? "N/A" : data.lastMissionRank;
                data.lastMissionRouteId = "main";
                data.lastMissionRouteLabel = "Main Extraction";
                data.lastMissionRouteMultiplier = 1f;
            }

            NormalizeSchema(data);
        }

        private static void NormalizeSchema(SaveData data)
        {
            if (data == null)
                return;

            data.schemaVersion = Mathf.Max(1, data.schemaVersion);
            data.slotIndex = NormalizeSlot(data.slotIndex);
            if (string.IsNullOrWhiteSpace(data.saveId))
                data.saveId = global::System.Guid.NewGuid().ToString("N");
            data.credits = Mathf.Max(0, data.credits);
            data.highestLevel = Mathf.Max(1, data.highestLevel);
            data.currentLevel = Mathf.Max(1, data.currentLevel);
            data.bloodlineLevel = Mathf.Max(0, data.bloodlineLevel);
            data.totalPlayTime = Mathf.Max(0f, data.totalPlayTime);
            data.unlockedTools ??= global::System.Array.Empty<string>();

            data.lastMissionRank = string.IsNullOrWhiteSpace(data.lastMissionRank) ? "N/A" : data.lastMissionRank;
            data.lastMissionRankScore = Mathf.Max(0, data.lastMissionRankScore);
            data.lastMissionCredits = Mathf.Max(0, data.lastMissionCredits);
            data.lastMissionIntelCollected = Mathf.Max(0, data.lastMissionIntelCollected);
            data.lastMissionIntelRequired = Mathf.Max(0, data.lastMissionIntelRequired);
            data.lastMissionSecondaryCompleted = Mathf.Max(0, data.lastMissionSecondaryCompleted);
            data.lastMissionSecondaryTotal = Mathf.Max(2, data.lastMissionSecondaryTotal);
            data.lastMissionRouteId = string.IsNullOrWhiteSpace(data.lastMissionRouteId) ? "main" : data.lastMissionRouteId;
            data.lastMissionRouteLabel = string.IsNullOrWhiteSpace(data.lastMissionRouteLabel) ? "Main Extraction" : data.lastMissionRouteLabel;
            data.lastMissionRouteRiskTier = Mathf.Clamp(data.lastMissionRouteRiskTier, 0, 3);
            data.lastMissionRouteMultiplier = Mathf.Clamp(data.lastMissionRouteMultiplier <= 0f ? 1f : data.lastMissionRouteMultiplier, 0.5f, 2f);
            data.lastMissionToolsUsed = Mathf.Max(0, data.lastMissionToolsUsed);
            data.lastMissionAlertsTriggered = Mathf.Max(0, data.lastMissionAlertsTriggered);
            data.lastMissionToolCooldownLoad = Mathf.Max(0f, data.lastMissionToolCooldownLoad);
            data.lastMissionRopeToolUses = Mathf.Max(0, data.lastMissionRopeToolUses);
            data.lastMissionSmokeToolUses = Mathf.Max(0, data.lastMissionSmokeToolUses);
            data.lastMissionSoundBaitToolUses = Mathf.Max(0, data.lastMissionSoundBaitToolUses);
        }

        private static int NormalizeSlot(int slotIndex)
        {
            return Mathf.Clamp(slotIndex, 0, MaxSaveSlots - 1);
        }

        private static string GetPrimaryKey(int slotIndex)
        {
            return $"{SaveKeyPrefix}{NormalizeSlot(slotIndex)}";
        }

        private static string GetBackupKey(int slotIndex)
        {
            return $"{GetPrimaryKey(slotIndex)}{BackupSuffix}";
        }

        private static bool ContainsKnownSaveToken(string json)
        {
            string[] knownFields =
            {
                "\"schemaVersion\"",
                "\"credits\"",
                "\"highestLevel\"",
                "\"currentLevel\"",
                "\"totalPlayTime\"",
                "\"unlockedTools\""
            };

            for (int i = 0; i < knownFields.Length; i++)
            {
                if (json.Contains(knownFields[i]))
                    return true;
            }

            return false;
        }

        private void ApplyLoadedData(SaveData data)
        {
            if (creditSystem != null)
                creditSystem.SetCredits(data.credits);

            if (levelFlow != null)
            {
                int highestLevel = Mathf.Clamp(data.highestLevel, 1, levelFlow.TotalLevelCount);
                levelFlow.ResetProgress();
                levelFlow.UnlockLevel(highestLevel);

                int targetLevelIndex = Mathf.Clamp(data.currentLevel - 1, 0, levelFlow.TotalLevelCount - 1);
                levelFlow.SelectLevel(targetLevelIndex);
            }

            if (progressionTree != null)
            {
                progressionTree.ResetProgression();
                int completedLevel = levelFlow != null
                    ? Mathf.Clamp(data.highestLevel, 0, levelFlow.TotalLevelCount)
                    : Mathf.Max(0, data.highestLevel);

                for (int i = 1; i <= completedLevel; i++)
                    progressionTree.CompleteLevel(i);
            }

            if (bloodlineSystem != null)
            {
                int maxBloodlineLevel = 5;
                int bloodlineLevel = Mathf.Clamp(data.bloodlineLevel, 0, maxBloodlineLevel);
                bloodlineSystem.ResetBloodline();
                for (int i = 1; i <= bloodlineLevel; i++)
                    bloodlineSystem.UnlockPassiveForLevel(i);
            }

            if (toolManager != null && data.unlockedTools != null)
            {
                for (int i = 0; i < data.unlockedTools.Length; i++)
                {
                    string toolName = data.unlockedTools[i];
                    if (!string.IsNullOrWhiteSpace(toolName))
                        toolManager.UnlockTool(toolName);
                }
            }
        }

        [global::System.Serializable]
        public class SaveData
        {
            public int schemaVersion;
            public int slotIndex;
            public string saveId;
            public string saveTimestampUtc;

            public int credits;
            public int highestLevel;
            public int currentLevel;
            public int bloodlineLevel;
            public float totalPlayTime;
            public string[] unlockedTools;

            public bool hasMissionSnapshot;
            public string lastMissionRank;
            public int lastMissionRankScore;
            public int lastMissionCredits;
            public int lastMissionIntelCollected;
            public int lastMissionIntelRequired;
            public int lastMissionSecondaryCompleted;
            public int lastMissionSecondaryTotal;
            public bool lastMissionUsedOptionalExit;
            public string lastMissionRouteId;
            public string lastMissionRouteLabel;
            public int lastMissionRouteRiskTier;
            public float lastMissionRouteMultiplier;
            public int lastMissionToolsUsed;
            public int lastMissionAlertsTriggered;
            public int lastMissionToolRiskWindowAdjustment;
            public float lastMissionToolCooldownLoad;
            public int lastMissionRopeToolUses;
            public int lastMissionSmokeToolUses;
            public int lastMissionSoundBaitToolUses;
        }

        public struct GameSavedEvent
        {
            public SaveData saveData;
            public int slotIndex;
            public bool backupUpdated;
        }

        public struct GameLoadedEvent
        {
            public SaveData saveData;
            public int slotIndex;
            public bool loadedFromBackup;
        }

        public struct SaveMigrationAppliedEvent
        {
            public int fromVersion;
            public int toVersion;
            public int slotIndex;
            public bool loadedFromBackup;
        }

        public struct SaveRecoveredFromBackupEvent
        {
            public int slotIndex;
        }
    }
}
