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
        [SerializeField] private int _currentLevelIndex = 0;
        [SerializeField] private string _currentLevelName = "";

        [Header("Player Stats")]
        [SerializeField] private int _playerCredits = 0;
        [SerializeField] private float _playTime = 0f;

        [Header("Mission Stats")]
        [SerializeField] private int _enemiesKilled = 0;
        [SerializeField] private int _enemiesKnockedOut = 0;
        [SerializeField] private bool _wasDiscovered = false;
        [SerializeField] private bool _fullAlertTriggered = false;

        public EGameState CurrentState => _currentState;
        public int CurrentLevelIndex => _currentLevelIndex;
        public string CurrentLevelName => _currentLevelName;
        public int PlayerCredits => _playerCredits;
        public float PlayTime => _playTime;
        public int EnemiesKilled => _enemiesKilled;
        public int EnemiesKnockedOut => _enemiesKnockedOut;
        public bool WasDiscovered => _wasDiscovered;
        public bool FullAlertTriggered => _fullAlertTriggered;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (_currentState == EGameState.Playing)
            {
                _playTime += Time.deltaTime;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        public void StartGame()
        {
            _currentState = EGameState.Playing;
            _playTime = 0f;
            _enemiesKilled = 0;
            _enemiesKnockedOut = 0;
            _wasDiscovered = false;
            _fullAlertTriggered = false;
        }

        public void LoadLevel(int levelIndex, string levelName)
        {
            _currentLevelIndex = levelIndex;
            _currentLevelName = levelName;
            _currentState = EGameState.Playing;
        }

        public void PauseGame()
        {
            if (_currentState == EGameState.Playing)
            {
                _currentState = EGameState.Paused;
                Time.timeScale = 0f;
            }
        }

        public void ResumeGame()
        {
            if (_currentState == EGameState.Paused)
            {
                _currentState = EGameState.Playing;
                Time.timeScale = 1f;
            }
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
            bool isSRank = !_wasDiscovered && !_fullAlertTriggered && 
                            _enemiesKilled == 0 && _enemiesKnockedOut > 0;

            return new MissionResult
            {
                LevelName = _currentLevelName,
                PlayTime = _playTime,
                WasDiscovered = _wasDiscovered,
                FullAlertTriggered = _fullAlertTriggered,
                EnemiesKilled = _enemiesKilled,
                EnemiesKnockedOut = _enemiesKnockedOut,
                Rank = isSRank ? "S" : (_fullAlertTriggered ? "C" : "B")
            };
        }
    }

    public struct MissionResult
    {
        public string LevelName;
        public float PlayTime;
        public bool WasDiscovered;
        public bool FullAlertTriggered;
        public int EnemiesKilled;
        public int EnemiesKnockedOut;
        public string Rank;
    }
}
