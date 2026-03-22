using UnityEngine;
using UnityEngine.UI;
using INTIFALL.Level;

namespace INTIFALL.UI
{
    public class MainMenuUI : MonoBehaviour
    {
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

        [Header("References")]
        [SerializeField] private LevelFlowManager levelFlowManager;
        [SerializeField] private SaveLoadManager saveLoadManager;

        private void Start()
        {
            SetupButtons();
            SetupSettings();
            CheckSaveData();
            UpdateLevelButtons();
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

        private void CheckSaveData()
        {
            bool hasSave = saveLoadManager != null && saveLoadManager.HasSaveData;

            if (continueButton != null)
                continueButton.gameObject.SetActive(hasSave);

            if (newGameButton != null && hasSave)
                newGameButton.GetComponentInChildren<Text>().text = "新游戏";
        }

        private void UpdateLevelButtons()
        {
            if (levelButtons == null || levelFlowManager == null) return;

            for (int i = 0; i < levelButtons.Length; i++)
            {
                bool unlocked = levelFlowManager.IsLevelUnlocked(i);

                if (levelButtons[i] != null)
                    levelButtons[i].interactable = unlocked;

                if (levelLockTexts != null && levelLockTexts[i] != null)
                    levelLockTexts[i].text = unlocked ? "" : "🔒";
            }
        }

        private void OnNewGame()
        {
            if (saveLoadManager != null)
                saveLoadManager.DeleteSave();

            StartNewGame();
        }

        private void OnContinue()
        {
            if (saveLoadManager != null)
                saveLoadManager.LoadGame();

            StartNewGame();
        }

        private void StartNewGame()
        {
            if (levelFlowManager != null)
            {
                levelFlowManager.SelectLevel(0);
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
    }
}