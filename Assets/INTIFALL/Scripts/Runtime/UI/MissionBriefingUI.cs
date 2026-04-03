using INTIFALL.Level;
using INTIFALL.System;
using UnityEngine;
using UnityEngine.UI;

namespace INTIFALL.UI
{
    public class MissionBriefingUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject briefingPanel;
        [SerializeField] private Text missionIdText;
        [SerializeField] private Text missionNameText;
        [SerializeField] private Text missionLevelText;
        [SerializeField] private Text backgroundText;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text intelText;
        [SerializeField] private Text warningText;
        [SerializeField] private Text contactText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button cancelButton;

        [Header("Level Data")]
        [SerializeField] private LevelFlowManager levelFlowManager;

        [Header("Briefing Content")]
        [SerializeField] private string[] missionIds =
        {
            "OP-2247-001",
            "OP-2247-002",
            "OP-2247-003",
            "OP-2247-004",
            "OP-2247-005"
        };

        [SerializeField] private string[] missionNames =
        {
            "Golden Rain Warehouse",
            "Temple Archive Complex",
            "Underground Bio-Labs",
            "Qhipu Core Facility",
            "General Taki Villa"
        };

        [SerializeField] private string[] missionLevels =
        {
            "Confidential",
            "Confidential",
            "Top Secret",
            "Top Secret",
            "Top Secret"
        };

        [SerializeField] private string[] backgrounds =
        {
            "Ayllu Astral activity has intensified around the logistics district. Command authorized a stealth breach and intel sweep.",
            "Recovered routing logs indicate bloodline archives are stored inside this temple complex. Retrieve records and stay untracked.",
            "Aya-Tech is running illegal augmentation trials in subterranean labs. Secure evidence and break the command chain.",
            "The Qhipu core node controls strategic forecasts. Infiltrate the core ring and recover the final shards.",
            "General Taki is preparing a final contingency protocol. This operation determines the end-state of the campaign."
        };

        [SerializeField] private string[] objectives =
        {
            "Primary: Reach the warehouse control sector and secure intel.\nSecondary: Preserve stealth route integrity.",
            "Primary: Extract archive files from the upper sanctum.\nSecondary: Bypass security without full alert.",
            "Primary: Capture experiment logs from the lab network.\nSecondary: Avoid sustained direct firefights.",
            "Primary: Enter the Qhipu core chamber and recover route keys.\nSecondary: Keep priest patrols disrupted.",
            "Primary: Complete final objective and extract.\nSecondary: Execute the chosen end-state path."
        };

        [SerializeField] private string[] warnings =
        {
            "Multiple patrol loops overlap near loading bays. Vent bypass remains the safest flank.",
            "Archive floors are layered with reinforced sentries. Expect rapid escalation if spotted.",
            "Saqueos units are active in lower corridors. Maintain distance and break line-of-sight quickly.",
            "Core perimeter rotates heavy guards and priest controllers. Noise discipline is critical.",
            "Final sector has no safe fallback. Every alarm state increases extraction risk."
        };

        private int _currentLevelIndex = -1;
        private bool _isDisplayed;

        public bool IsDisplayed => _isDisplayed;

        private void Awake()
        {
            if (levelFlowManager == null)
                levelFlowManager = FindObjectOfType<LevelFlowManager>();

            if (briefingPanel != null)
                briefingPanel.SetActive(false);
        }

        private void Start()
        {
            if (startButton != null)
                startButton.onClick.AddListener(OnStartMission);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancel);
        }

        public void ShowBriefing(int levelIndex)
        {
            if (_isDisplayed)
                return;

            if (levelIndex < 0 || levelIndex >= missionIds.Length)
                return;

            _currentLevelIndex = levelIndex;
            _isDisplayed = true;

            UpdateBriefingContent();

            if (briefingPanel != null)
                briefingPanel.SetActive(true);

            Time.timeScale = 0f;
        }

        private void UpdateBriefingContent()
        {
            string missionName = GetLocalizedEntry("briefing.mission_name", missionNames);
            string missionLevel = GetLocalizedEntry("briefing.mission_level", missionLevels);
            string background = GetLocalizedEntry("briefing.background", backgrounds);
            string objective = GetLocalizedEntry("briefing.objective", objectives);
            string warning = GetLocalizedEntry("briefing.warning", warnings);
            string intelHint = GetIntelHint(_currentLevelIndex);

            if (missionIdText != null)
            {
                string template = LocalizationService.Get(
                    "briefing.line.mission_id",
                    fallbackEnglish: "[Mission ID] {0}",
                    fallbackChinese: string.Empty);
                missionIdText.text = string.Format(template, missionIds[_currentLevelIndex]);
            }

            if (missionNameText != null)
            {
                string template = LocalizationService.Get(
                    "briefing.line.mission_name",
                    fallbackEnglish: "[Mission Name] {0}",
                    fallbackChinese: string.Empty);
                missionNameText.text = string.Format(template, missionName);
            }

            if (missionLevelText != null)
            {
                string template = LocalizationService.Get(
                    "briefing.line.classification",
                    fallbackEnglish: "[Classification] {0}",
                    fallbackChinese: string.Empty);
                missionLevelText.text = string.Format(template, missionLevel);
            }

            if (backgroundText != null)
            {
                string template = LocalizationService.Get(
                    "briefing.line.background",
                    fallbackEnglish: "[Background]\n{0}",
                    fallbackChinese: string.Empty);
                backgroundText.text = string.Format(template, background);
            }

            if (objectiveText != null)
            {
                string template = LocalizationService.Get(
                    "briefing.line.objectives",
                    fallbackEnglish: "[Objectives]\n{0}",
                    fallbackChinese: string.Empty);
                objectiveText.text = string.Format(template, objective);
            }

            if (intelText != null)
            {
                string template = LocalizationService.Get(
                    "briefing.line.intel_hint",
                    fallbackEnglish: "[Intel Hint]\n{0}",
                    fallbackChinese: string.Empty);
                intelText.text = string.Format(template, intelHint);
            }

            if (warningText != null)
            {
                string template = LocalizationService.Get(
                    "briefing.line.warnings",
                    fallbackEnglish: "[Warnings]\n{0}",
                    fallbackChinese: string.Empty);
                warningText.text = string.Format(template, warning);
            }

            if (contactText != null)
            {
                contactText.text = LocalizationService.Get(
                    "briefing.contact",
                    fallbackEnglish: "[Contact] Willa\n--------------------------",
                    fallbackChinese: string.Empty);
            }
        }

        private string GetLocalizedEntry(string keyPrefix, string[] fallbackValues)
        {
            if (fallbackValues == null || _currentLevelIndex < 0 || _currentLevelIndex >= fallbackValues.Length)
                return string.Empty;

            string key = $"{keyPrefix}.{_currentLevelIndex + 1}";
            return LocalizationService.Get(
                key,
                fallbackEnglish: fallbackValues[_currentLevelIndex],
                fallbackChinese: string.Empty);
        }

        private static string GetIntelHint(int levelIndex)
        {
            string fallback = levelIndex switch
            {
                0 => "Vent channels connect cargo intake to rear extraction lanes.",
                1 => "Temple terminals contain bloodline archive breadcrumbs.",
                2 => "Primary evidence cluster is mirrored to the central lab terminal.",
                3 => "Qhipu fragments are distributed across ring corridors and upper control decks.",
                4 => "Final extraction route depends on the evidence chain you carry.",
                _ => string.Empty
            };

            string key = $"briefing.intel_hint.{levelIndex + 1}";
            return LocalizationService.Get(
                key,
                fallbackEnglish: fallback,
                fallbackChinese: string.Empty);
        }

        private void OnStartMission()
        {
            CloseBriefing();

            if (levelFlowManager != null)
            {
                levelFlowManager.SelectLevel(_currentLevelIndex);
                levelFlowManager.LoadSelectedLevel();
            }
            else
            {
                EventBus.Publish(new LevelSelectedEvent { levelIndex = _currentLevelIndex });
            }
        }

        private void OnCancel()
        {
            CloseBriefing();
        }

        public void CloseBriefing()
        {
            _isDisplayed = false;

            if (briefingPanel != null)
                briefingPanel.SetActive(false);

            Time.timeScale = 1f;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<LevelSelectedEvent>(OnLevelSelected);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LevelSelectedEvent>(OnLevelSelected);
        }

        private void OnLevelSelected(LevelSelectedEvent evt)
        {
            ShowBriefing(evt.levelIndex);
        }
    }
}
