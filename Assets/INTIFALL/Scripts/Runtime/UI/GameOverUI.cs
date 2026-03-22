using UnityEngine;
using UnityEngine.UI;
using INTIFALL.System;
using INTIFALL.Level;

namespace INTIFALL.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("Game Over Panel")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text messageText;

        [Header("Stats")]
        [SerializeField] private Text playTimeText;
        [SerializeField] private Text killsText;
        [SerializeField] private Text intelText;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Level Complete")]
        [SerializeField] private GameObject levelCompletePanel;
        [SerializeField] private Text rankText;
        [SerializeField] private Text creditsEarnedText;
        [SerializeField] private Text bonusText;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button levelSelectButton;

        private void Start()
        {
            SetupButtons();

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            if (levelCompletePanel != null)
                levelCompletePanel.SetActive(false);
        }

        private void SetupButtons()
        {
            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetry);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenu);

            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(OnNextLevel);

            if (levelSelectButton != null)
                levelSelectButton.onClick.AddListener(OnMainMenu);
        }

        public void ShowGameOver()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            if (levelCompletePanel != null)
                levelCompletePanel.SetActive(false);

            if (titleText != null)
                titleText.text = "任务失败";

            if (messageText != null)
                messageText.text = "你被发现了...";

            UpdateStats();
        }

        public void ShowLevelComplete(int rank, int credits, bool[] bonuses)
        {
            if (levelCompletePanel != null)
                levelCompletePanel.SetActive(true);

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            string rankName = rank switch
            {
                5 => "S",
                4 => "A",
                3 => "B",
                2 => "C",
                _ => "D"
            };

            if (rankText != null)
                rankText.text = $"Rank: {rankName}";

            if (creditsEarnedText != null)
                creditsEarnedText.text = $"+{credits} Credit";

            if (bonusText != null)
            {
                string bonusStr = "";
                if (bonuses != null)
                {
                    if (bonuses.Length > 0 && bonuses[0]) bonusStr += "零击杀 ";
                    if (bonuses.Length > 1 && bonuses[1]) bonusStr += "无伤 ";
                    if (bonuses.Length > 2 && bonuses[2]) bonusStr += "全情报 ";
                }
                bonusText.text = bonusStr;
            }

            UpdateStats();
        }

        private void UpdateStats()
        {
            var gm = GameManager.Instance;

            if (playTimeText != null && gm != null)
                playTimeText.text = $"时间: {gm.PlayTime:F1}s";

            if (killsText != null && gm != null)
                killsText.text = $"击杀: {gm.EnemiesKilled}";

            if (intelText != null)
            {
                var narrative = FindObjectOfType<Narrative.NarrativeManager>();
                intelText.text = $"情报: {narrative?.IntelCollected ?? 0}";
            }
        }

        private void OnRetry()
        {
            Time.timeScale = 1f;

            var levelFlow = FindObjectOfType<LevelFlowManager>();
            if (levelFlow != null)
                levelFlow.RestartCurrentLevel();
        }

        private void OnMainMenu()
        {
            Time.timeScale = 1f;

            var levelFlow = FindObjectOfType<LevelFlowManager>();
            if (levelFlow != null)
                levelFlow.LoadMainMenu();
        }

        private void OnNextLevel()
        {
            Time.timeScale = 1f;

            var levelFlow = FindObjectOfType<LevelFlowManager>();
            if (levelFlow != null)
                levelFlow.LoadNextLevel();
        }
    }
}