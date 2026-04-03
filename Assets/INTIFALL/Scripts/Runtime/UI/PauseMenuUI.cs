using UnityEngine;
using INTIFALL.Input;
using UnityEngine.UI;
using INTIFALL.System;

namespace INTIFALL.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("Pause Panel")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private bool isPaused;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button restartButton;

        [Header("Settings")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;

        private void Start()
        {
            SetupButtons();

            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        private void SetupButtons()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(Resume);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettings);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenu);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestart);
        }

        private void Update()
        {
            if (InputCompat.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }

        public void Pause()
        {
            isPaused = true;
            Time.timeScale = 0f;

            if (pausePanel != null)
                pausePanel.SetActive(true);

            EventBus.Publish(new GamePausedEvent());
        }

        public void Resume()
        {
            isPaused = false;
            Time.timeScale = 1f;

            if (pausePanel != null)
                pausePanel.SetActive(false);

            EventBus.Publish(new GameResumedEvent());
        }

        private void OnSettings()
        {
        }

        private void OnMainMenu()
        {
            Time.timeScale = 1f;

            var levelFlow = FindObjectOfType<Level.LevelFlowManager>();
            if (levelFlow != null)
                levelFlow.LoadMainMenu();
        }

        private void OnRestart()
        {
            Time.timeScale = 1f;

            var levelFlow = FindObjectOfType<Level.LevelFlowManager>();
            if (levelFlow != null)
                levelFlow.RestartCurrentLevel();
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

        public bool IsPaused => isPaused;

        public struct GamePausedEvent { }
        public struct GameResumedEvent { }
    }
}
