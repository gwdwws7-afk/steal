using UnityEngine;
using INTIFALL.System;
using INTIFALL.Economy;
using INTIFALL.Growth;

namespace INTIFALL.Core
{
    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance { get; private set; }

        private const string SAVE_KEY = "INTIFALL_SaveData";

        [Header("References")]
        [SerializeField] private CreditSystem creditSystem;
        [SerializeField] private ProgressionTree progressionTree;
        [SerializeField] private BloodlineSystem bloodlineSystem;
        [SerializeField] private LevelFlowManager levelFlow;

        private SaveData _currentSave;

        public bool HasSaveData => PlayerPrefs.HasKey(SAVE_KEY);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SaveGame()
        {
            SaveData data = new SaveData
            {
                credits = creditSystem?.CurrentCredits ?? 0,
                highestLevel = levelFlow?.HighestUnlockedLevel ?? 1,
                currentLevel = GameManager.Instance?.CurrentLevel ?? 1,
                bloodlineLevel = bloodlineSystem?.CurrentLevel ?? 0,
                totalPlayTime = GameManager.Instance?.PlayTime ?? 0f
            };

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();

            EventBus.Publish(new GameSavedEvent { saveData = data });
        }

        public bool LoadGame()
        {
            if (!HasSaveData) return false;

            string json = PlayerPrefs.GetString(SAVE_KEY);
            SaveData data = JsonImpl(json);

            if (data == null) return false;

            if (creditSystem != null)
            {
            }

            if (levelFlow != null)
            {
                for (int i = 1; i <= data.highestLevel; i++)
                {
                    levelFlow.UnlockLevel(i);
                }
            }

            if (bloodlineSystem != null)
            {
                for (int i = 1; i <= data.bloodlineLevel; i++)
                {
                    bloodlineSystem.UnlockPassiveForLevel(i);
                }
            }

            _currentSave = data;

            EventBus.Publish(new GameLoadedEvent { saveData = data });

            return true;
        }

        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
        }

        private SaveData JsonImpl(string json)
        {
            try
            {
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch
            {
                return null;
            }
        }

        public SaveData GetCurrentSave()
        {
            return _currentSave;
        }

        [System.Serializable]
        public class SaveData
        {
            public int credits;
            public int highestLevel;
            public int currentLevel;
            public int bloodlineLevel;
            public float totalPlayTime;
        }

        public struct GameSavedEvent
        {
            public SaveData saveData;
        }

        public struct GameLoadedEvent
        {
            public SaveData saveData;
        }
    }
}