using INTIFALL.Level;
using INTIFALL.System;
using UnityEngine;
using UnityEngine.UI;

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
            {
                titleText.text = LocalizationService.Get(
                    "gameover.title.failed",
                    fallbackEnglish: "Mission Failed",
                    fallbackChinese: string.Empty);
            }

            if (messageText != null)
            {
                messageText.text = LocalizationService.Get(
                    "gameover.message.compromised",
                    fallbackEnglish: "You were compromised.",
                    fallbackChinese: string.Empty);
            }

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
            {
                string template = LocalizationService.Get(
                    "gameover.rank.line",
                    fallbackEnglish: "Rank: {0}",
                    fallbackChinese: string.Empty);
                rankText.text = string.Format(template, rankName);
            }

            if (creditsEarnedText != null)
            {
                string template = LocalizationService.Get(
                    "gameover.credits.line",
                    fallbackEnglish: "+{0} Credits",
                    fallbackChinese: string.Empty);
                creditsEarnedText.text = string.Format(template, credits);
            }

            if (bonusText != null)
                bonusText.text = BuildBonusText(bonuses);

            UpdateStats();
        }

        private static string BuildBonusText(bool[] bonuses)
        {
            if (bonuses == null)
                return string.Empty;

            string[] tokens = new string[3];
            int count = 0;

            if (bonuses.Length > 0 && bonuses[0])
            {
                tokens[count++] = LocalizationService.Get(
                    "gameover.bonus.zero_kill",
                    fallbackEnglish: "Zero-Kill",
                    fallbackChinese: string.Empty);
            }

            if (bonuses.Length > 1 && bonuses[1])
            {
                tokens[count++] = LocalizationService.Get(
                    "gameover.bonus.no_damage",
                    fallbackEnglish: "No-Damage",
                    fallbackChinese: string.Empty);
            }

            if (bonuses.Length > 2 && bonuses[2])
            {
                tokens[count++] = LocalizationService.Get(
                    "gameover.bonus.full_intel",
                    fallbackEnglish: "Full-Intel",
                    fallbackChinese: string.Empty);
            }

            if (count == 0)
                return string.Empty;

            return string.Join(" ", tokens, 0, count);
        }

        private void UpdateStats()
        {
            GameManager gm = GameManager.Instance;

            if (playTimeText != null && gm != null)
            {
                string template = LocalizationService.Get(
                    "gameover.stats.time",
                    fallbackEnglish: "Time: {0}s",
                    fallbackChinese: string.Empty);
                playTimeText.text = string.Format(template, gm.PlayTime.ToString("F1"));
            }

            if (killsText != null && gm != null)
            {
                string template = LocalizationService.Get(
                    "gameover.stats.kills",
                    fallbackEnglish: "Kills: {0}",
                    fallbackChinese: string.Empty);
                killsText.text = string.Format(template, gm.EnemiesKilled);
            }

            if (intelText != null)
            {
                var narrative = FindObjectOfType<Narrative.NarrativeManager>();
                string template = LocalizationService.Get(
                    "gameover.stats.intel",
                    fallbackEnglish: "Intel: {0}",
                    fallbackChinese: string.Empty);
                intelText.text = string.Format(template, narrative?.IntelCollected ?? 0);
            }
        }

        private void OnRetry()
        {
            Time.timeScale = 1f;

            LevelFlowManager levelFlow = FindObjectOfType<LevelFlowManager>();
            if (levelFlow != null)
                levelFlow.RestartCurrentLevel();
        }

        private void OnMainMenu()
        {
            Time.timeScale = 1f;

            LevelFlowManager levelFlow = FindObjectOfType<LevelFlowManager>();
            if (levelFlow != null)
                levelFlow.LoadMainMenu();
        }

        private void OnNextLevel()
        {
            Time.timeScale = 1f;

            LevelFlowManager levelFlow = FindObjectOfType<LevelFlowManager>();
            if (levelFlow != null)
                levelFlow.LoadNextLevel();
        }
    }
}
