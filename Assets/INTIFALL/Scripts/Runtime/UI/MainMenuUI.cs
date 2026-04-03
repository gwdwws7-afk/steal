using INTIFALL.Core;
using INTIFALL.Level;
using INTIFALL.System;
using UnityEngine;
using UnityEngine.UI;

namespace INTIFALL.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        private const string ActiveSlotPrefsKey = "INTIFALL_MainMenu_ActiveSlot";
        private const int MissionSnapshotMaxLines = 4;
        private const int MissionSnapshotLineMaxCharacters = 96;
        private const int MissionSnapshotCompactMaxCharacters = 48;
        private const string SnapshotEllipsis = "...";

        [Header("Menu Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject levelSelectPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Level Select")]
        [SerializeField] private Button[] levelButtons;
        [SerializeField] private Text[] levelLockTexts;

        [Header("Settings")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Toggle invertYToggle;
        [SerializeField] private Slider sensitivitySlider;

        [Header("Save Slots")]
        [Range(0, SaveLoadManager.MaxSaveSlots - 1)]
        [SerializeField] private int defaultSaveSlot;
        [SerializeField] private Button[] saveSlotButtons;
        [SerializeField] private Text[] saveSlotStatusTexts;
        [SerializeField] private Text activeSlotText;
        [SerializeField] private Text activeSlotMissionSnapshotText;
        [SerializeField] private Text slotActionFeedbackText;
        [SerializeField] private Button restoreBackupButton;
        [SerializeField] private Button deleteSlotButton;
        [SerializeField] private bool autoContinueFromFirstAvailableSlot = true;

        [Header("References")]
        [SerializeField] private LevelFlowManager levelFlowManager;
        [SerializeField] private SaveLoadManager saveLoadManager;

        private int _activeSaveSlotIndex;

        public int ActiveSaveSlotIndex => _activeSaveSlotIndex;

        private void Start()
        {
            ResolveReferences();
            SetupButtons();
            SetupSettings();
            InitializeSaveSlots();
            RefreshMenuState();
        }

        private void ResolveReferences()
        {
            if (saveLoadManager == null)
                saveLoadManager = Object.FindFirstObjectByType<SaveLoadManager>();

            if (levelFlowManager == null)
                levelFlowManager = Object.FindFirstObjectByType<LevelFlowManager>();
        }

        private void SetupButtons()
        {
            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGame);

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinue);

            if (levelSelectButton != null)
                levelSelectButton.onClick.AddListener(OnLevelSelect);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettings);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuit);

            if (saveSlotButtons != null)
            {
                for (int i = 0; i < saveSlotButtons.Length; i++)
                {
                    if (saveSlotButtons[i] == null)
                        continue;

                    int slot = i;
                    saveSlotButtons[i].onClick.AddListener(() => OnSaveSlotButtonClicked(slot));
                }
            }

            if (restoreBackupButton != null)
                restoreBackupButton.onClick.AddListener(OnRestoreBackupClicked);

            if (deleteSlotButton != null)
                deleteSlotButton.onClick.AddListener(OnDeleteSlotClicked);
        }

        private void SetupSettings()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

            if (invertYToggle != null)
                invertYToggle.onValueChanged.AddListener(OnInvertYToggled);

            if (sensitivitySlider != null)
                sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }

        private void InitializeSaveSlots()
        {
            int preferredSlot = PlayerPrefs.GetInt(ActiveSlotPrefsKey, defaultSaveSlot);
            int initialSlot = Mathf.Clamp(preferredSlot, 0, SaveLoadManager.MaxSaveSlots - 1);

            if (autoContinueFromFirstAvailableSlot && !IsSlotLoadable(initialSlot))
            {
                int fallbackSlot = FindFirstLoadableSlot(initialSlot);
                if (fallbackSlot >= 0)
                    initialSlot = fallbackSlot;
            }

            SetActiveSaveSlot(initialSlot, false);
            ClearSlotActionFeedback();
        }

        private void SetActiveSaveSlot(int slotIndex, bool persist)
        {
            _activeSaveSlotIndex = Mathf.Clamp(slotIndex, 0, SaveLoadManager.MaxSaveSlots - 1);
            if (saveLoadManager != null)
                saveLoadManager.SetActiveSlot(_activeSaveSlotIndex);

            if (persist)
            {
                PlayerPrefs.SetInt(ActiveSlotPrefsKey, _activeSaveSlotIndex);
                PlayerPrefs.Save();
            }
        }

        private static bool IsSlotLoadable(int slotIndex)
        {
            bool hasPrimary = PlayerPrefs.HasKey(SaveLoadManager.GetSaveKeyForSlot(slotIndex));
            if (hasPrimary && TryReadSlotData(slotIndex, false, out _))
                return true;

            bool hasBackup = PlayerPrefs.HasKey(SaveLoadManager.GetBackupKeyForSlot(slotIndex));
            if (hasBackup && TryReadSlotData(slotIndex, true, out _))
                return true;

            return false;
        }

        private bool HasAnyLoadableSaveData()
        {
            for (int slot = 0; slot < SaveLoadManager.MaxSaveSlots; slot++)
            {
                if (IsSlotLoadable(slot))
                    return true;
            }

            return false;
        }

        private int FindFirstLoadableSlot(int excludedSlot = -1)
        {
            for (int slot = 0; slot < SaveLoadManager.MaxSaveSlots; slot++)
            {
                if (slot == excludedSlot)
                    continue;

                if (IsSlotLoadable(slot))
                    return slot;
            }

            return -1;
        }

        private static bool TryReadSlotData(int slotIndex, bool preferBackup, out SaveLoadManager.SaveData data)
        {
            string key = preferBackup
                ? SaveLoadManager.GetBackupKeyForSlot(slotIndex)
                : SaveLoadManager.GetSaveKeyForSlot(slotIndex);
            data = SaveLoadManager.DeserializeWithMigration(PlayerPrefs.GetString(key, string.Empty));
            return data != null;
        }

        private void CheckSaveData()
        {
            bool hasAnySave = HasAnyLoadableSaveData();

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(hasAnySave);
                continueButton.interactable = hasAnySave;
            }

            if (newGameButton != null)
            {
                Text buttonText = newGameButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    string template = hasAnySave
                        ? LocalizationService.Get(
                            "menu.new_game_slot",
                            fallbackEnglish: "New Game (Slot {0})",
                            fallbackChinese: string.Empty)
                        : LocalizationService.Get(
                            "menu.new_game",
                            fallbackEnglish: "New Game",
                            fallbackChinese: string.Empty);

                    buttonText.text = hasAnySave
                        ? string.Format(template, _activeSaveSlotIndex + 1)
                        : template;
                }
            }

            if (restoreBackupButton != null)
                restoreBackupButton.interactable = saveLoadManager != null && saveLoadManager.HasBackupDataInSlot(_activeSaveSlotIndex);

            if (deleteSlotButton != null)
                deleteSlotButton.interactable = saveLoadManager != null && saveLoadManager.HasSaveDataInSlot(_activeSaveSlotIndex);
        }

        private static void SetButtonLabel(Button button, string localizationKey, string fallbackEnglish)
        {
            if (button == null)
                return;

            Text label = button.GetComponentInChildren<Text>();
            if (label == null)
                return;

            label.text = LocalizationService.Get(
                localizationKey,
                fallbackEnglish: fallbackEnglish,
                fallbackChinese: string.Empty);
        }

        private void UpdateMenuButtonTexts()
        {
            SetButtonLabel(continueButton, "menu.continue", "Continue");
            SetButtonLabel(levelSelectButton, "menu.level_select", "Level Select");
            SetButtonLabel(settingsButton, "menu.settings", "Settings");
            SetButtonLabel(quitButton, "menu.quit", "Quit");
            SetButtonLabel(restoreBackupButton, "menu.restore_backup", "Restore Backup");
            SetButtonLabel(deleteSlotButton, "menu.delete_slot", "Delete Slot");
        }

        private void SetSlotActionFeedback(string key, string fallbackEnglish, params object[] formatArgs)
        {
            if (slotActionFeedbackText == null)
                return;

            string template = LocalizationService.Get(
                key,
                fallbackEnglish: fallbackEnglish,
                fallbackChinese: string.Empty);

            slotActionFeedbackText.text = formatArgs != null && formatArgs.Length > 0
                ? string.Format(template, formatArgs)
                : template;
        }

        private void ClearSlotActionFeedback()
        {
            if (slotActionFeedbackText != null)
                slotActionFeedbackText.text = string.Empty;
        }

        private void UpdateSaveSlotUI()
        {
            if (activeSlotText != null)
            {
                activeSlotText.text = string.Format(
                    LocalizationService.Get(
                        "menu.active_slot",
                        fallbackEnglish: "Active Slot: {0}",
                        fallbackChinese: string.Empty),
                    _activeSaveSlotIndex + 1);
            }

            if (saveSlotButtons != null)
            {
                for (int i = 0; i < saveSlotButtons.Length; i++)
                {
                    Button button = saveSlotButtons[i];
                    if (button == null)
                        continue;

                    Text label = button.GetComponentInChildren<Text>();
                    if (label != null)
                    {
                        bool isActive = i == _activeSaveSlotIndex;
                        string template = isActive
                            ? LocalizationService.Get(
                                "menu.slot_label_active",
                                fallbackEnglish: "Slot {0} *",
                                fallbackChinese: string.Empty)
                            : LocalizationService.Get(
                                "menu.slot_label",
                                fallbackEnglish: "Slot {0}",
                                fallbackChinese: string.Empty);

                        label.text = string.Format(template, i + 1);
                    }
                }
            }

            if (saveSlotStatusTexts != null)
            {
                for (int i = 0; i < saveSlotStatusTexts.Length; i++)
                {
                    if (saveSlotStatusTexts[i] == null)
                        continue;

                    saveSlotStatusTexts[i].text = BuildSlotStatusText(i);
                }
            }

            UpdateActiveSlotMissionSnapshot();
        }

        private string BuildSlotStatusText(int slotIndex)
        {
            bool hasPrimary = PlayerPrefs.HasKey(SaveLoadManager.GetSaveKeyForSlot(slotIndex));
            bool hasBackup = PlayerPrefs.HasKey(SaveLoadManager.GetBackupKeyForSlot(slotIndex));
            SaveLoadManager.SaveData primaryData = null;
            SaveLoadManager.SaveData backupData = null;
            bool hasPrimaryData = hasPrimary && TryReadSlotData(slotIndex, false, out primaryData);
            bool hasBackupData = hasBackup && TryReadSlotData(slotIndex, true, out backupData);

            if (!hasPrimary && !hasBackup)
            {
                return LocalizationService.Get(
                    "menu.slot.empty",
                    fallbackEnglish: "EMPTY",
                    fallbackChinese: string.Empty);
            }

            if (hasPrimaryData)
            {
                string backupTag = hasBackupData
                    ? LocalizationService.Get(
                        "menu.slot.backup_tag",
                        fallbackEnglish: " +BKP",
                        fallbackChinese: string.Empty)
                    : string.Empty;
                string template = LocalizationService.Get(
                    "menu.slot.status",
                    fallbackEnglish: "L{0} C{1}{2}",
                    fallbackChinese: string.Empty);

                return string.Format(
                    template,
                    Mathf.Max(1, primaryData.currentLevel),
                    Mathf.Max(0, primaryData.credits),
                    backupTag);
            }

            if (hasPrimary && !hasPrimaryData && hasBackupData)
            {
                string template = LocalizationService.Get(
                    "menu.slot.corrupted_backup_status",
                    fallbackEnglish: "CORRUPTED -> BKP L{0} C{1}",
                    fallbackChinese: string.Empty);

                return string.Format(
                    template,
                    Mathf.Max(1, backupData.currentLevel),
                    Mathf.Max(0, backupData.credits));
            }

            if (hasBackupData)
            {
                string template = LocalizationService.Get(
                    "menu.slot.backup_status",
                    fallbackEnglish: "BACKUP L{0} C{1}",
                    fallbackChinese: string.Empty);

                return string.Format(
                    template,
                    Mathf.Max(1, backupData.currentLevel),
                    Mathf.Max(0, backupData.credits));
            }

            return LocalizationService.Get(
                "menu.slot.corrupted",
                fallbackEnglish: "CORRUPTED",
                fallbackChinese: string.Empty);
        }

        private void UpdateActiveSlotMissionSnapshot()
        {
            if (!TryResolveActiveSlotData(out SaveLoadManager.SaveData data) ||
                data == null ||
                !data.hasMissionSnapshot)
            {
                if (activeSlotMissionSnapshotText != null)
                {
                    activeSlotMissionSnapshotText.text = LocalizationService.Get(
                        "menu.mission_snapshot.empty",
                        fallbackEnglish: "Last Mission: N/A",
                        fallbackChinese: string.Empty);
                }
                return;
            }

            string routeLabel = string.IsNullOrWhiteSpace(data.lastMissionRouteLabel)
                ? LocalizationService.Get("route.main", fallbackEnglish: "Main Extraction", fallbackChinese: string.Empty)
                : data.lastMissionRouteLabel;

            string toolWindowAssessment = ResolveToolWindowAssessment(data.lastMissionToolRiskWindowAdjustment);
            string toolWindowValue = data.lastMissionToolRiskWindowAdjustment.ToString("+0;-0;0");
            string rankLabel = string.IsNullOrWhiteSpace(data.lastMissionRank) ? "N/A" : data.lastMissionRank;

            if (activeSlotMissionSnapshotText == null)
            {
                if (saveSlotStatusTexts != null &&
                    _activeSaveSlotIndex >= 0 &&
                    _activeSaveSlotIndex < saveSlotStatusTexts.Length &&
                    saveSlotStatusTexts[_activeSaveSlotIndex] != null)
                {
                    string compact = string.Format(
                        LocalizationService.Get(
                            "menu.mission_snapshot.compact",
                            fallbackEnglish: "Last {0} C{1} TW{2}",
                            fallbackChinese: string.Empty),
                        rankLabel,
                        Mathf.Max(0, data.lastMissionCredits),
                        toolWindowValue);
                    compact = EllipsizeSingleLine(compact, MissionSnapshotCompactMaxCharacters);

                    saveSlotStatusTexts[_activeSaveSlotIndex].text = string.Concat(
                        BuildSlotStatusText(_activeSaveSlotIndex),
                        "\n",
                        compact);
                }
                return;
            }

            string line1 = string.Format(
                LocalizationService.Get(
                    "menu.mission_snapshot.line1",
                    fallbackEnglish: "Last Mission: Rank {0} ({1}), Credits {2}",
                    fallbackChinese: string.Empty),
                rankLabel,
                Mathf.Max(0, data.lastMissionRankScore),
                Mathf.Max(0, data.lastMissionCredits));

            string line2 = string.Format(
                LocalizationService.Get(
                    "menu.mission_snapshot.line2",
                    fallbackEnglish: "Route: {0} (risk {1}, x{2})",
                    fallbackChinese: string.Empty),
                routeLabel,
                Mathf.Clamp(data.lastMissionRouteRiskTier, 0, 3),
                Mathf.Clamp(data.lastMissionRouteMultiplier, 0.5f, 2f).ToString("0.00"));

            string line3 = string.Format(
                LocalizationService.Get(
                    "menu.mission_snapshot.line3",
                    fallbackEnglish: "Tool Window: {0} ({1}) | Alerts {2}, Tools {3}",
                    fallbackChinese: string.Empty),
                toolWindowValue,
                toolWindowAssessment,
                Mathf.Max(0, data.lastMissionAlertsTriggered),
                Mathf.Max(0, data.lastMissionToolsUsed));

            string line4 = string.Format(
                LocalizationService.Get(
                    "menu.mission_snapshot.line4",
                    fallbackEnglish: "Tool Mix: rope {0}, smoke {1}, bait {2}, cooldown load {3:0.0}s",
                    fallbackChinese: string.Empty),
                Mathf.Max(0, data.lastMissionRopeToolUses),
                Mathf.Max(0, data.lastMissionSmokeToolUses),
                Mathf.Max(0, data.lastMissionSoundBaitToolUses),
                Mathf.Max(0f, data.lastMissionToolCooldownLoad));

            activeSlotMissionSnapshotText.text = BuildMissionSnapshotBlock(line1, line2, line3, line4);
        }

        private bool TryResolveActiveSlotData(out SaveLoadManager.SaveData data)
        {
            data = null;

            if (TryReadSlotData(_activeSaveSlotIndex, false, out SaveLoadManager.SaveData primary))
            {
                data = primary;
                return true;
            }

            if (TryReadSlotData(_activeSaveSlotIndex, true, out SaveLoadManager.SaveData backup))
            {
                data = backup;
                return true;
            }

            return false;
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

        private static string BuildMissionSnapshotBlock(params string[] lines)
        {
            if (lines == null || lines.Length == 0)
                return string.Empty;

            int lineCount = Mathf.Min(MissionSnapshotMaxLines, lines.Length);
            if (lineCount <= 0)
                return string.Empty;

            string[] normalized = new string[lineCount];
            for (int i = 0; i < lineCount; i++)
                normalized[i] = EllipsizeSingleLine(lines[i], MissionSnapshotLineMaxCharacters);

            return string.Join("\n", normalized);
        }

        private static string EllipsizeSingleLine(string value, int maxCharacters)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string normalized = value.Replace('\n', ' ').Replace('\r', ' ').Trim();
            if (maxCharacters <= SnapshotEllipsis.Length || normalized.Length <= maxCharacters)
                return normalized;

            int cutLength = maxCharacters - SnapshotEllipsis.Length;
            if (cutLength <= 0)
                return SnapshotEllipsis;

            return string.Concat(normalized.Substring(0, cutLength).TrimEnd(), SnapshotEllipsis);
        }

        private void UpdateLevelButtons()
        {
            if (levelButtons == null || levelFlowManager == null)
                return;

            for (int i = 0; i < levelButtons.Length; i++)
            {
                bool unlocked = levelFlowManager.IsLevelUnlocked(i);

                if (levelButtons[i] != null)
                    levelButtons[i].interactable = unlocked;

                if (levelLockTexts != null && i < levelLockTexts.Length && levelLockTexts[i] != null)
                {
                    levelLockTexts[i].text = unlocked
                        ? string.Empty
                        : LocalizationService.Get(
                            "menu.level.locked",
                            fallbackEnglish: "LOCKED",
                            fallbackChinese: string.Empty);
                }
            }
        }

        private void OnNewGame()
        {
            saveLoadManager?.DeleteSave(_activeSaveSlotIndex);
            levelFlowManager?.ResetProgress();
            ClearSlotActionFeedback();
            StartNewGame();
            RefreshMenuState();
        }

        private void OnContinue()
        {
            int startLevelIndex = 0;
            bool loaded = false;

            if (saveLoadManager != null)
            {
                loaded = saveLoadManager.LoadGame(_activeSaveSlotIndex);
                if (!loaded && autoContinueFromFirstAvailableSlot)
                {
                    int fallbackSlot = FindFirstLoadableSlot(_activeSaveSlotIndex);
                    if (fallbackSlot >= 0)
                    {
                        SetActiveSaveSlot(fallbackSlot, true);
                        loaded = saveLoadManager.LoadGame(fallbackSlot);
                    }
                }

                if (loaded)
                {
                    SaveLoadManager.SaveData save = saveLoadManager.GetCurrentSave();
                    if (save != null)
                        startLevelIndex = Mathf.Max(0, save.currentLevel - 1);
                }
            }

            if (!loaded)
            {
                SetSlotActionFeedback(
                    "menu.feedback.continue_failed",
                    "No valid save found in selected slot.");
                RefreshMenuState();
                return;
            }

            ClearSlotActionFeedback();
            StartNewGame(startLevelIndex);
            RefreshMenuState();
        }

        private void StartNewGame(int levelIndex = 0)
        {
            if (levelFlowManager != null)
            {
                levelFlowManager.SelectLevel(levelIndex);
                levelFlowManager.LoadSelectedLevel();
            }
        }

        private void OnLevelSelect()
        {
            ShowPanel(levelSelectPanel);
            UpdateLevelButtons();
        }

        private void OnSettings()
        {
            ShowPanel(settingsPanel);
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ShowPanel(GameObject panelToShow)
        {
            if (mainPanel != null) mainPanel.SetActive(panelToShow == mainPanel);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(panelToShow == levelSelectPanel);
            if (settingsPanel != null) settingsPanel.SetActive(panelToShow == settingsPanel);
        }

        private void OnMasterVolumeChanged(float value)
        {
            Audio.AudioManager.Instance?.SetMasterVolume(value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            Audio.AudioManager.Instance?.SetSFXVolume(value);
        }

        private void OnMusicVolumeChanged(float value)
        {
            Audio.AudioManager.Instance?.SetMusicVolume(value);
        }

        private void OnInvertYToggled(bool value)
        {
            Input.InputManager.Instance?.SetInvertY(value);
        }

        private void OnSensitivityChanged(float value)
        {
            Input.InputManager.Instance?.SetMouseSensitivity(value);
        }

        public void OnLevelButtonClicked(int levelIndex)
        {
            if (levelFlowManager != null)
            {
                levelFlowManager.SelectLevel(levelIndex);
                levelFlowManager.LoadSelectedLevel();
            }
        }

        public void OnSaveSlotButtonClicked(int slotIndex)
        {
            SetActiveSaveSlot(slotIndex, true);
            ClearSlotActionFeedback();
            RefreshMenuState();
        }

        public void OnRestoreBackupClicked()
        {
            bool restored = saveLoadManager != null && saveLoadManager.RestoreBackupToPrimary(_activeSaveSlotIndex);
            SetSlotActionFeedback(
                restored ? "menu.feedback.restore_success" : "menu.feedback.restore_failed",
                restored ? "Backup restored to active slot." : "No valid backup available for active slot.");
            RefreshMenuState();
        }

        public void OnDeleteSlotClicked()
        {
            bool hadSave = saveLoadManager != null && saveLoadManager.HasSaveDataInSlot(_activeSaveSlotIndex);
            saveLoadManager?.DeleteSave(_activeSaveSlotIndex);
            SetSlotActionFeedback(
                hadSave ? "menu.feedback.delete_success" : "menu.feedback.delete_missing",
                hadSave ? "Active slot save deleted." : "Active slot has no save data.");
            RefreshMenuState();
        }

        public void RefreshMenuState()
        {
            UpdateMenuButtonTexts();
            CheckSaveData();
            UpdateSaveSlotUI();
            UpdateLevelButtons();
        }
    }
}
