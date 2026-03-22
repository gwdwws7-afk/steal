using UnityEngine;
using UnityEngine.SceneManagement;
using INTIFALL.System;

namespace INTIFALL.Level
{
    public struct LevelLoadedEvent
    {
        public int levelIndex;
        public string levelName;
    }

    public struct LevelCompletedEvent
    {
        public int levelIndex;
        public string rank;
        public int creditsEarned;
    }

    public struct LevelSelectedEvent
    {
        public int levelIndex;
    }

    public class LevelFlowManager : MonoBehaviour
    {
        [Header("Level Configuration")]
        [SerializeField] private string[] levelSceneNames = new string[]
        {
            "Level_01",
            "Level_02",
            "Level_03",
            "Level_04",
            "Level_05"
        };

        [SerializeField] private string[] levelDisplayNames = new string[]
        {
            "黄金雨",
            "档案迷宫",
            "黄金血脉",
            "Qhipu 核心",
            "太阳陨落"
        };

        [Header("Current State")]
        [SerializeField] private int _currentLevelIndex = 0;
        [SerializeField] private int _highestUnlockedLevel = 1;

        private bool _isTransitioning;

        public int CurrentLevelIndex => _currentLevelIndex;
        public int HighestUnlockedLevel => _highestUnlockedLevel;
        public bool IsTransitioning => _isTransitioning;

        public int TotalLevelCount => levelSceneNames.Length;

        private void Awake()
        {
            LoadProgress();
        }

        private void LoadProgress()
        {
            _highestUnlockedLevel = PlayerPrefs.GetInt("INTIFALL_HighestLevel", 1);
        }

        public void SaveProgress()
        {
            PlayerPrefs.SetInt("INTIFALL_HighestLevel", _highestUnlockedLevel);
            PlayerPrefs.Save();
        }

        public string GetLevelSceneName(int index)
        {
            if (index < 0 || index >= levelSceneNames.Length)
                return "";
            return levelSceneNames[index];
        }

        public string GetLevelDisplayName(int index)
        {
            if (index < 0 || index >= levelDisplayNames.Length)
                return "";
            return levelDisplayNames[index];
        }

        public bool IsLevelUnlocked(int index)
        {
            return index < _highestUnlockedLevel;
        }

        public void SelectLevel(int index)
        {
            if (_isTransitioning) return;
            if (!IsLevelUnlocked(index)) return;

            _currentLevelIndex = index;

            EventBus.Publish(new LevelSelectedEvent
            {
                levelIndex = index
            });
        }

        public void LoadSelectedLevel()
        {
            if (_isTransitioning) return;
            if (_currentLevelIndex < 0 || _currentLevelIndex >= levelSceneNames.Length) return;

            _isTransitioning = true;
            string sceneName = levelSceneNames[_currentLevelIndex];

            EventBus.Publish(new LevelLoadedEvent
            {
                levelIndex = _currentLevelIndex,
                levelName = sceneName
            });

            SceneManager.LoadScene(sceneName);
            _isTransitioning = false;
        }

        public void CompleteLevel(string rank, int creditsEarned)
        {
            EventBus.Publish(new LevelCompletedEvent
            {
                levelIndex = _currentLevelIndex,
                rank = rank,
                creditsEarned = creditsEarned
            });

            if (_currentLevelIndex + 1 < levelSceneNames.Length)
            {
                int nextLevel = _currentLevelIndex + 1;
                if (nextLevel > _highestUnlockedLevel)
                {
                    _highestUnlockedLevel = nextLevel;
                    SaveProgress();
                }
            }
        }

        public void LoadMainMenu()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;
            SceneManager.LoadScene("MainMenu");
            _isTransitioning = false;
        }

        public void LoadNextLevel()
        {
            if (_currentLevelIndex + 1 >= levelSceneNames.Length)
                return;

            _currentLevelIndex++;
            LoadSelectedLevel();
        }

        public void RestartCurrentLevel()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;
            SceneManager.LoadScene(levelSceneNames[_currentLevelIndex]);
            _isTransitioning = false;
        }

        public void UnlockLevel(int index)
        {
            if (index > _highestUnlockedLevel)
            {
                _highestUnlockedLevel = index;
                SaveProgress();
            }
        }

        public void ResetProgress()
        {
            _highestUnlockedLevel = 1;
            _currentLevelIndex = 0;
            SaveProgress();
        }

        public int GetLevelIndexFromSceneName(string sceneName)
        {
            for (int i = 0; i < levelSceneNames.Length; i++)
            {
                if (levelSceneNames[i] == sceneName)
                    return i;
            }
            return -1;
        }
    }
}