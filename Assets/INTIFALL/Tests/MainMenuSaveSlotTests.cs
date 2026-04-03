using System.Reflection;
using INTIFALL.Core;
using INTIFALL.System;
using INTIFALL.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace INTIFALL.Tests
{
    public class MainMenuSaveSlotTests
    {
        private const string ActiveSlotPrefsKey = "INTIFALL_MainMenu_ActiveSlot";

        private GameObject _saveManagerGo;
        private SaveLoadManager _saveManager;
        private GameObject _menuGo;
        private MainMenuUI _menu;

        [SetUp]
        public void SetUp()
        {
            LocalizationService.SetLanguageOverride(SystemLanguage.English);

            _saveManagerGo = new GameObject("SaveLoadManager");
            _saveManager = _saveManagerGo.AddComponent<SaveLoadManager>();

            _menuGo = new GameObject("MainMenu");
            _menu = _menuGo.AddComponent<MainMenuUI>();

            SetPrivateField(_menu, "saveLoadManager", _saveManager);
            for (int slot = 0; slot < SaveLoadManager.MaxSaveSlots; slot++)
                _saveManager.DeleteSave(slot);
            PlayerPrefs.DeleteKey(ActiveSlotPrefsKey);
            PlayerPrefs.Save();
        }

        [TearDown]
        public void TearDown()
        {
            if (_saveManager != null)
            {
                for (int slot = 0; slot < SaveLoadManager.MaxSaveSlots; slot++)
                    _saveManager.DeleteSave(slot);
            }

            LocalizationService.ClearLanguageOverride();
            PlayerPrefs.DeleteKey(ActiveSlotPrefsKey);
            PlayerPrefs.Save();

            if (_menuGo != null)
                Object.DestroyImmediate(_menuGo);
            if (_saveManagerGo != null)
                Object.DestroyImmediate(_saveManagerGo);
        }

        [Test]
        public void SaveSlotSelection_UpdatesMenuAndSaveManagerActiveSlot()
        {
            _menu.OnSaveSlotButtonClicked(2);

            Assert.AreEqual(2, _menu.ActiveSaveSlotIndex);
            Assert.AreEqual(2, _saveManager.ActiveSlotIndex);
            Assert.AreEqual(2, PlayerPrefs.GetInt(ActiveSlotPrefsKey, -1));
        }

        [Test]
        public void DeleteActiveSlot_ClearsOnlySelectedSlot()
        {
            int slot0 = 0;
            int slot1 = 1;

            PlayerPrefs.SetString(SaveLoadManager.GetSaveKeyForSlot(slot0), BuildSaveJson(100, 2, 2, slot0));
            PlayerPrefs.SetString(SaveLoadManager.GetSaveKeyForSlot(slot1), BuildSaveJson(300, 4, 4, slot1));
            PlayerPrefs.Save();

            _menu.OnSaveSlotButtonClicked(slot1);
            _menu.OnDeleteSlotClicked();

            Assert.IsTrue(_saveManager.HasSaveDataInSlot(slot0));
            Assert.IsFalse(_saveManager.HasSaveDataInSlot(slot1));
        }

        [Test]
        public void RestoreBackupClicked_RehydratesCorruptedPrimary()
        {
            int slot = 1;
            string primaryKey = SaveLoadManager.GetSaveKeyForSlot(slot);
            string backupKey = SaveLoadManager.GetBackupKeyForSlot(slot);
            string backupJson = BuildSaveJson(420, 5, 5, slot);

            PlayerPrefs.SetString(primaryKey, "broken-primary");
            PlayerPrefs.SetString(backupKey, backupJson);
            PlayerPrefs.Save();

            _menu.OnSaveSlotButtonClicked(slot);
            _menu.OnRestoreBackupClicked();

            Assert.AreEqual(backupJson, PlayerPrefs.GetString(primaryKey, string.Empty));
            Assert.IsTrue(_saveManager.HasSaveDataInSlot(slot));
        }

        [Test]
        public void ContinueButtonVisibility_UsesOnlyLoadableSlotData()
        {
            Button continueButton = CreateButton("ContinueButton");
            continueButton.transform.SetParent(_menuGo.transform, false);
            SetPrivateField(_menu, "continueButton", continueButton);

            int corruptedSlot = 0;
            PlayerPrefs.SetString(SaveLoadManager.GetSaveKeyForSlot(corruptedSlot), "broken-json");
            PlayerPrefs.Save();

            _menu.RefreshMenuState();
            Assert.IsFalse(continueButton.gameObject.activeSelf);

            PlayerPrefs.SetString(SaveLoadManager.GetBackupKeyForSlot(1), BuildSaveJson(260, 3, 3, 1));
            PlayerPrefs.Save();

            _menu.RefreshMenuState();
            Assert.IsTrue(continueButton.gameObject.activeSelf);
        }

        [Test]
        public void Continue_NoLoadableSave_ShowsFailureFeedback()
        {
            Text feedback = CreateText("SlotFeedback");
            feedback.transform.SetParent(_menuGo.transform, false);
            SetPrivateField(_menu, "slotActionFeedbackText", feedback);

            PlayerPrefs.SetString(SaveLoadManager.GetSaveKeyForSlot(0), "broken-json");
            PlayerPrefs.Save();

            InvokePrivateMethod(_menu, "OnContinue");

            Assert.AreEqual(-1, _saveManager.CurrentLoadedSlotIndex);
            Assert.IsFalse(string.IsNullOrWhiteSpace(feedback.text));
        }

        [Test]
        public void RefreshMenuState_WithMissionSnapshot_ShowsActiveSlotCard()
        {
            Text snapshotText = CreateText("MissionSnapshot");
            snapshotText.transform.SetParent(_menuGo.transform, false);
            SetPrivateField(_menu, "activeSlotMissionSnapshotText", snapshotText);

            int slot = 0;
            PlayerPrefs.SetString(SaveLoadManager.GetSaveKeyForSlot(slot), BuildSaveJson(420, 5, 5, slot, hasMissionSnapshot: true));
            PlayerPrefs.Save();

            _menu.OnSaveSlotButtonClicked(slot);

            StringAssert.Contains("Last Mission: Rank A (4), Credits 640", snapshotText.text);
            StringAssert.Contains("Route: Upper Ring Catwalk (risk 3, x1.30)", snapshotText.text);
            StringAssert.Contains("Tool Window: +35 (Balanced window) | Alerts 2, Tools 5", snapshotText.text);
            StringAssert.Contains("Tool Mix: rope 1, smoke 2, bait 1, cooldown load 30.0s", snapshotText.text);
        }

        [Test]
        public void RefreshMenuState_WithoutMissionSnapshot_ShowsEmptyCard()
        {
            Text snapshotText = CreateText("MissionSnapshotEmpty");
            snapshotText.transform.SetParent(_menuGo.transform, false);
            SetPrivateField(_menu, "activeSlotMissionSnapshotText", snapshotText);

            int slot = 0;
            PlayerPrefs.SetString(SaveLoadManager.GetSaveKeyForSlot(slot), BuildSaveJson(300, 3, 3, slot, hasMissionSnapshot: false));
            PlayerPrefs.Save();

            _menu.OnSaveSlotButtonClicked(slot);

            Assert.AreEqual("Last Mission: N/A", snapshotText.text);
        }

        [Test]
        public void RefreshMenuState_WithoutDedicatedCard_AppendsCompactSnapshotToActiveSlotStatus()
        {
            Text[] statusTexts = new Text[SaveLoadManager.MaxSaveSlots];
            for (int i = 0; i < statusTexts.Length; i++)
            {
                statusTexts[i] = CreateText($"SlotStatus_{i}");
                statusTexts[i].transform.SetParent(_menuGo.transform, false);
            }

            SetPrivateField(_menu, "saveSlotStatusTexts", statusTexts);

            int slot = 1;
            PlayerPrefs.SetString(SaveLoadManager.GetSaveKeyForSlot(slot), BuildSaveJson(420, 5, 5, slot, hasMissionSnapshot: true));
            PlayerPrefs.Save();

            _menu.OnSaveSlotButtonClicked(slot);

            StringAssert.Contains("L5 C420", statusTexts[slot].text);
            StringAssert.Contains("Last A C640 TW+35", statusTexts[slot].text);
        }

        [Test]
        public void RefreshMenuState_WithLongRouteLabel_ClampsSnapshotToFourEllipsizedLines()
        {
            Text snapshotText = CreateText("MissionSnapshotLong");
            snapshotText.transform.SetParent(_menuGo.transform, false);
            SetPrivateField(_menu, "activeSlotMissionSnapshotText", snapshotText);

            int slot = 0;
            string longRouteLabel = "Upper Ring Catwalk Segment " + new string('X', 180);
            PlayerPrefs.SetString(
                SaveLoadManager.GetSaveKeyForSlot(slot),
                BuildSaveJson(420, 5, 5, slot, hasMissionSnapshot: true, routeLabelOverride: longRouteLabel));
            PlayerPrefs.Save();

            _menu.OnSaveSlotButtonClicked(slot);

            string[] lines = snapshotText.text.Split('\n');
            Assert.AreEqual(4, lines.Length);
            Assert.IsTrue(lines[1].StartsWith("Route:"));
            StringAssert.EndsWith("...", lines[1]);

            for (int i = 0; i < lines.Length; i++)
                Assert.LessOrEqual(lines[i].Length, 96, $"Snapshot line {i} exceeded max character budget.");
        }

        private static Button CreateButton(string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            return go.GetComponent<Button>();
        }

        private static Text CreateText(string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            return go.GetComponent<Text>();
        }

        private static string BuildSaveJson(
            int credits,
            int highestLevel,
            int currentLevel,
            int slotIndex,
            bool hasMissionSnapshot = false,
            string routeLabelOverride = null)
        {
            string routeLabel = hasMissionSnapshot
                ? (string.IsNullOrWhiteSpace(routeLabelOverride) ? "Upper Ring Catwalk" : routeLabelOverride)
                : "Main Extraction";

            SaveLoadManager.SaveData data = new SaveLoadManager.SaveData
            {
                schemaVersion = SaveLoadManager.CurrentSaveSchemaVersion,
                slotIndex = slotIndex,
                saveId = "main-menu-test-save",
                saveTimestampUtc = "2026-04-01T00:00:00.0000000Z",
                credits = credits,
                highestLevel = highestLevel,
                currentLevel = currentLevel,
                bloodlineLevel = 1,
                totalPlayTime = 120f,
                unlockedTools = global::System.Array.Empty<string>(),
                hasMissionSnapshot = hasMissionSnapshot,
                lastMissionRank = hasMissionSnapshot ? "A" : "B",
                lastMissionRankScore = hasMissionSnapshot ? 4 : 3,
                lastMissionCredits = hasMissionSnapshot ? 640 : 0,
                lastMissionRouteId = hasMissionSnapshot ? "upper_ring" : "main",
                lastMissionRouteLabel = routeLabel,
                lastMissionRouteRiskTier = hasMissionSnapshot ? 3 : 0,
                lastMissionRouteMultiplier = hasMissionSnapshot ? 1.3f : 1f,
                lastMissionToolsUsed = hasMissionSnapshot ? 5 : 0,
                lastMissionAlertsTriggered = hasMissionSnapshot ? 2 : 0,
                lastMissionToolRiskWindowAdjustment = hasMissionSnapshot ? 35 : 0,
                lastMissionToolCooldownLoad = hasMissionSnapshot ? 30f : 0f,
                lastMissionRopeToolUses = hasMissionSnapshot ? 1 : 0,
                lastMissionSmokeToolUses = hasMissionSnapshot ? 2 : 0,
                lastMissionSoundBaitToolUses = hasMissionSnapshot ? 1 : 0,
                lastMissionSecondaryTotal = 2
            };

            return JsonUtility.ToJson(data);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Failed to reflect field '{fieldName}'.");
            field.SetValue(target, value);
        }

        private static void InvokePrivateMethod(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Failed to reflect method '{methodName}'.");
            method.Invoke(target, null);
        }
    }
}
