using INTIFALL.Core;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class SaveLoadManagerReliabilityTests
    {
        private GameObject _managerGo;
        private SaveLoadManager _manager;

        [SetUp]
        public void SetUp()
        {
            _managerGo = new GameObject("SaveLoadManager");
            _manager = _managerGo.AddComponent<SaveLoadManager>();

            for (int slot = 0; slot < SaveLoadManager.MaxSaveSlots; slot++)
                _manager.DeleteSave(slot);
        }

        [TearDown]
        public void TearDown()
        {
            if (_manager != null)
            {
                for (int slot = 0; slot < SaveLoadManager.MaxSaveSlots; slot++)
                    _manager.DeleteSave(slot);
            }

            if (_managerGo != null)
                Object.DestroyImmediate(_managerGo);
        }

        [Test]
        public void LoadGame_MultiSlotData_RemainsIsolatedBySlot()
        {
            int slot0 = 0;
            int slot1 = 1;

            PlayerPrefs.SetString(SaveLoadManager.GetSaveKeyForSlot(slot0), BuildSaveJson(120, 2, 2, slot0));
            PlayerPrefs.SetString(SaveLoadManager.GetSaveKeyForSlot(slot1), BuildSaveJson(480, 5, 5, slot1));
            PlayerPrefs.Save();

            Assert.IsTrue(_manager.LoadGame(slot0));
            Assert.AreEqual(120, _manager.GetCurrentSave().credits);
            Assert.AreEqual(slot0, _manager.CurrentLoadedSlotIndex);

            Assert.IsTrue(_manager.LoadGame(slot1));
            Assert.AreEqual(480, _manager.GetCurrentSave().credits);
            Assert.AreEqual(slot1, _manager.CurrentLoadedSlotIndex);
        }

        [Test]
        public void LoadGame_CorruptedPrimary_UsesBackupAndRestoresPrimary()
        {
            int slot = 1;
            string saveKey = SaveLoadManager.GetSaveKeyForSlot(slot);
            string backupKey = SaveLoadManager.GetBackupKeyForSlot(slot);
            string backupJson = BuildSaveJson(360, 4, 4, slot);

            PlayerPrefs.SetString(saveKey, "corrupted-primary");
            PlayerPrefs.SetString(backupKey, backupJson);
            PlayerPrefs.Save();

            Assert.IsTrue(_manager.LoadGame(slot));
            Assert.AreEqual(360, _manager.GetCurrentSave().credits);
            Assert.AreEqual(backupJson, PlayerPrefs.GetString(saveKey, string.Empty));
        }

        [Test]
        public void SaveGame_WhenPrimaryExists_CreatesBackupSnapshot()
        {
            int slot = 2;
            string saveKey = SaveLoadManager.GetSaveKeyForSlot(slot);
            string backupKey = SaveLoadManager.GetBackupKeyForSlot(slot);
            string previousPrimary = BuildSaveJson(77, 1, 1, slot);

            PlayerPrefs.SetString(saveKey, previousPrimary);
            PlayerPrefs.DeleteKey(backupKey);
            PlayerPrefs.Save();

            Assert.IsTrue(_manager.SaveGame(slot));
            Assert.IsTrue(PlayerPrefs.HasKey(backupKey));
            Assert.AreEqual(previousPrimary, PlayerPrefs.GetString(backupKey, string.Empty));
        }

        [Test]
        public void RestoreBackupToPrimary_ValidBackup_RehydratesPrimary()
        {
            int slot = 0;
            string saveKey = SaveLoadManager.GetSaveKeyForSlot(slot);
            string backupKey = SaveLoadManager.GetBackupKeyForSlot(slot);
            string backupJson = BuildSaveJson(205, 3, 3, slot);

            PlayerPrefs.SetString(saveKey, "broken");
            PlayerPrefs.SetString(backupKey, backupJson);
            PlayerPrefs.Save();

            Assert.IsTrue(_manager.RestoreBackupToPrimary(slot));
            Assert.AreEqual(backupJson, PlayerPrefs.GetString(saveKey, string.Empty));
            Assert.AreEqual(205, _manager.GetCurrentSave().credits);
        }

        [Test]
        public void DeleteSave_RemovesPrimaryAndBackup()
        {
            int slot = 1;
            string saveKey = SaveLoadManager.GetSaveKeyForSlot(slot);
            string backupKey = SaveLoadManager.GetBackupKeyForSlot(slot);

            PlayerPrefs.SetString(saveKey, BuildSaveJson(100, 2, 2, slot));
            PlayerPrefs.SetString(backupKey, BuildSaveJson(90, 1, 1, slot));
            PlayerPrefs.Save();

            _manager.DeleteSave(slot);

            Assert.IsFalse(PlayerPrefs.HasKey(saveKey));
            Assert.IsFalse(PlayerPrefs.HasKey(backupKey));
        }

        private static string BuildSaveJson(int credits, int highestLevel, int currentLevel, int slotIndex)
        {
            SaveLoadManager.SaveData data = new SaveLoadManager.SaveData
            {
                schemaVersion = SaveLoadManager.CurrentSaveSchemaVersion,
                slotIndex = slotIndex,
                saveId = "test-save-id",
                saveTimestampUtc = "2026-04-01T00:00:00.0000000Z",
                credits = credits,
                highestLevel = highestLevel,
                currentLevel = currentLevel,
                bloodlineLevel = 1,
                totalPlayTime = 120f,
                unlockedTools = global::System.Array.Empty<string>(),
                hasMissionSnapshot = false,
                lastMissionRank = "B",
                lastMissionRouteId = "main",
                lastMissionRouteLabel = "Main Extraction",
                lastMissionRouteMultiplier = 1f,
                lastMissionSecondaryTotal = 2
            };

            return JsonUtility.ToJson(data);
        }
    }
}
