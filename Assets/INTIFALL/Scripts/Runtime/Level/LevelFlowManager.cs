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
            "Level01_Qhapaq_Passage",
            "Level02_Temple_Complex",
            "Level03_Underground_Labs",
            "Level04_Qhipu_Core",
            "Level05_General_Taki_Villa"
        };

        [SerializeField] private string[] levelDisplayNames = new string[]
        {
            "Golden Ruins",
            "Archive Maze",
            "Golden Bloodline",
            "Qhipu Core",
            "Solar Fall"
        };

        [Header("Current State")]
        [SerializeField] private int _currentLevelIndex = 0;
        [SerializeField] private int _highestUnlockedLevel = 1;

        private const string MainMenuSceneName = "MainMenu";
        private const string MainMenuFallbackSceneName = "TerminalScene";
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
            _highestUnlockedLevel = Mathf.Clamp(_highestUnlockedLevel, 1, levelSceneNames.Length);
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
            if (index < 0 || index >= levelSceneNames.Length) return false;
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
            if (!CanLoadScene(sceneName))
            {
                Debug.LogError($"LevelFlowManager: scene '{sceneName}' cannot be loaded.");
                _isTransitioning = false;
                return;
            }

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
                int unlockedCount = nextLevel + 1;
                if (unlockedCount > _highestUnlockedLevel)
                {
                    _highestUnlockedLevel = unlockedCount;
                    SaveProgress();
                }
            }
        }

        public void LoadMainMenu()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            if (CanLoadScene(MainMenuSceneName))
            {
                SceneManager.LoadScene(MainMenuSceneName);
            }
            else if (CanLoadScene(MainMenuFallbackSceneName))
            {
                SceneManager.LoadScene(MainMenuFallbackSceneName);
            }
            else if (SceneManager.sceneCountInBuildSettings > 0)
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                Debug.LogError("LevelFlowManager: no loadable scene found for main menu.");
            }

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
            if (_currentLevelIndex < 0 || _currentLevelIndex >= levelSceneNames.Length) return;

            string sceneName = levelSceneNames[_currentLevelIndex];
            if (!CanLoadScene(sceneName))
            {
                Debug.LogError($"LevelFlowManager: cannot restart missing scene '{sceneName}'.");
                return;
            }

            _isTransitioning = true;
            SceneManager.LoadScene(sceneName);
            _isTransitioning = false;
        }

        public void UnlockLevel(int index)
        {
            int clamped = Mathf.Clamp(index, 1, levelSceneNames.Length);
            if (clamped > _highestUnlockedLevel)
            {
                _highestUnlockedLevel = clamped;
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

        private static bool CanLoadScene(string sceneName)
        {
            return !string.IsNullOrEmpty(sceneName) &&
                   Application.CanStreamedLevelBeLoaded(sceneName);
        }
    }
}
