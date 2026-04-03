using System.Collections;
using INTIFALL.Audio;
using INTIFALL.Core;
using INTIFALL.System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class Iteration10PersistenceAndRecoveryPlayModeTests
    {
        [UnityTest]
        public IEnumerator PersistentManagers_DetachToRoot_BeforeDontDestroyOnLoad()
        {
            GameObject parent = new GameObject("Parent");
            GameObject child = new GameObject("Child");
            child.transform.SetParent(parent.transform, false);

            GameManager gameManager = child.AddComponent<GameManager>();
            AudioManager audioManager = child.AddComponent<AudioManager>();

            yield return null;

            Assert.IsNotNull(gameManager);
            Assert.IsNotNull(audioManager);
            Assert.IsNull(gameManager.transform.parent, "GameManager should detach to root before DontDestroyOnLoad.");
            Assert.IsNull(audioManager.transform.parent, "AudioManager should detach to root before DontDestroyOnLoad.");

            Object.Destroy(child);
            Object.Destroy(parent);
        }

        [UnityTest]
        public IEnumerator SaveRecovery_CorruptedPrimary_FallsBackToBackup()
        {
            GameObject go = new GameObject("SaveLoadManager");
            SaveLoadManager manager = go.AddComponent<SaveLoadManager>();
            const int slot = 1;

            manager.DeleteSave(slot);

            string saveKey = SaveLoadManager.GetSaveKeyForSlot(slot);
            string backupKey = SaveLoadManager.GetBackupKeyForSlot(slot);
            string backupJson = BuildSaveJson(390, 4, 4, slot);

            PlayerPrefs.SetString(saveKey, "corrupted-primary");
            PlayerPrefs.SetString(backupKey, backupJson);
            PlayerPrefs.Save();

            yield return null;

            Assert.IsTrue(manager.LoadGame(slot));
            Assert.IsNotNull(manager.GetCurrentSave());
            Assert.AreEqual(390, manager.GetCurrentSave().credits);
            Assert.AreEqual(backupJson, PlayerPrefs.GetString(saveKey, string.Empty));

            manager.DeleteSave(slot);
            Object.Destroy(go);
        }

        private static string BuildSaveJson(int credits, int highestLevel, int currentLevel, int slotIndex)
        {
            SaveLoadManager.SaveData data = new SaveLoadManager.SaveData
            {
                schemaVersion = SaveLoadManager.CurrentSaveSchemaVersion,
                slotIndex = slotIndex,
                saveId = "playmode-recovery-test",
                saveTimestampUtc = "2026-04-01T00:00:00.0000000Z",
                credits = credits,
                highestLevel = highestLevel,
                currentLevel = currentLevel,
                bloodlineLevel = 1,
                totalPlayTime = 120f,
                unlockedTools = global::System.Array.Empty<string>(),
                hasMissionSnapshot = false,
                lastMissionRank = "A",
                lastMissionRouteId = "main",
                lastMissionRouteLabel = "Main Extraction",
                lastMissionRouteMultiplier = 1f,
                lastMissionSecondaryTotal = 2
            };

            return JsonUtility.ToJson(data);
        }
    }
}
