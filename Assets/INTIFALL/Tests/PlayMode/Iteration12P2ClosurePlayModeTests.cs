using System.Collections;
using System.Reflection;
using INTIFALL.Core;
using INTIFALL.Level;
using INTIFALL.Narrative;
using INTIFALL.System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class Iteration12P2ClosurePlayModeTests
    {
        private static readonly string[] CoreSceneNames =
        {
            "Level01_Qhapaq_Passage",
            "Level02_Temple_Complex",
            "Level03_Underground_Labs",
            "Level04_Qhipu_Core",
            "Level05_General_Taki_Villa"
        };

        [UnityTest]
        public IEnumerator SaveSlotWorkflow_MultiSlotDeleteAndBackupRecovery_WorksInRuntime()
        {
            yield return CleanupSingletonManagers();

            var gameManagerGo = new GameObject("I12_GameManager");
            var saveLoadManagerGo = new GameObject("I12_SaveLoadManager");
            GameManager gameManager = gameManagerGo.AddComponent<GameManager>();
            SaveLoadManager saveLoadManager = saveLoadManagerGo.AddComponent<SaveLoadManager>();
            yield return null;

            try
            {
                ClearAllSaveSlots(saveLoadManager);

                gameManager.LoadLevel(0, "Level01_Qhapaq_Passage");
                gameManager.AddCredits(120);
                Assert.IsTrue(saveLoadManager.SaveGame(0), "Saving slot 0 should succeed.");
                Assert.AreEqual(120, saveLoadManager.GetCurrentSave().credits);

                gameManager.LoadLevel(3, "Level04_Qhipu_Core");
                gameManager.AddCredits(350);
                Assert.IsTrue(saveLoadManager.SaveGame(1), "Saving slot 1 should succeed.");
                Assert.AreEqual(470, saveLoadManager.GetCurrentSave().credits);

                Assert.IsTrue(saveLoadManager.LoadGame(0), "Loading slot 0 should succeed.");
                Assert.AreEqual(0, saveLoadManager.CurrentLoadedSlotIndex);
                Assert.AreEqual(120, saveLoadManager.GetCurrentSave().credits);
                Assert.AreEqual(1, saveLoadManager.GetCurrentSave().currentLevel);

                Assert.IsTrue(saveLoadManager.LoadGame(1), "Loading slot 1 should succeed.");
                Assert.AreEqual(1, saveLoadManager.CurrentLoadedSlotIndex);
                Assert.AreEqual(470, saveLoadManager.GetCurrentSave().credits);
                Assert.AreEqual(4, saveLoadManager.GetCurrentSave().currentLevel);

                const int deleteSlot = 2;
                gameManager.LoadLevel(4, "Level05_General_Taki_Villa");
                Assert.IsTrue(saveLoadManager.SaveGame(deleteSlot), "Initial save for delete-slot should succeed.");
                gameManager.AddCredits(10);
                Assert.IsTrue(saveLoadManager.SaveGame(deleteSlot), "Second save should create backup snapshot.");

                string deletePrimaryKey = SaveLoadManager.GetSaveKeyForSlot(deleteSlot);
                string deleteBackupKey = SaveLoadManager.GetBackupKeyForSlot(deleteSlot);
                Assert.IsTrue(PlayerPrefs.HasKey(deletePrimaryKey), "Delete test primary key should exist before delete.");
                Assert.IsTrue(PlayerPrefs.HasKey(deleteBackupKey), "Delete test backup key should exist before delete.");

                saveLoadManager.DeleteSave(deleteSlot);
                Assert.IsFalse(PlayerPrefs.HasKey(deletePrimaryKey), "Delete should remove primary key.");
                Assert.IsFalse(PlayerPrefs.HasKey(deleteBackupKey), "Delete should remove backup key.");

                const int recoverySlot = 2;
                string recoveryPrimaryKey = SaveLoadManager.GetSaveKeyForSlot(recoverySlot);
                string recoveryBackupKey = SaveLoadManager.GetBackupKeyForSlot(recoverySlot);
                string recoveryBackupJson = BuildSaveJson(333, 5, 5, recoverySlot);
                PlayerPrefs.SetString(recoveryPrimaryKey, "corrupted-primary");
                PlayerPrefs.SetString(recoveryBackupKey, recoveryBackupJson);
                PlayerPrefs.Save();

                Assert.IsTrue(saveLoadManager.LoadGame(recoverySlot), "Corrupted primary should recover from backup.");
                Assert.AreEqual(333, saveLoadManager.GetCurrentSave().credits);
                Assert.AreEqual(recoveryBackupJson, PlayerPrefs.GetString(recoveryPrimaryKey, string.Empty));
            }
            finally
            {
                ClearAllSaveSlots(saveLoadManager);
                Object.Destroy(saveLoadManagerGo);
                Object.Destroy(gameManagerGo);
            }
        }

        [Test]
        public void LocalizationWorkflow_EnglishChineseFallback_ResolvesDeterministically()
        {
            LocalizationService.ResetForTests();

            string english = LocalizationService.Get(
                "menu.new_game",
                languageOverride: SystemLanguage.English);
            string chinese = LocalizationService.Get(
                "menu.new_game",
                languageOverride: SystemLanguage.ChineseSimplified);

            Assert.AreEqual("New Game", english);
            Assert.AreEqual("\u65B0\u6E38\u620F", chinese);

            string chineseFallback = LocalizationService.Get(
                "missing.key",
                fallbackEnglish: "Fallback EN",
                fallbackChinese: "\u56DE\u9000\u4E2D\u6587",
                languageOverride: SystemLanguage.ChineseSimplified);
            string englishFallback = LocalizationService.Get(
                "missing.key",
                fallbackEnglish: "Fallback EN",
                fallbackChinese: "\u56DE\u9000\u4E2D\u6587",
                languageOverride: SystemLanguage.English);

            Assert.AreEqual("\u56DE\u9000\u4E2D\u6587", chineseFallback);
            Assert.AreEqual("Fallback EN", englishFallback);
            Assert.AreEqual("missing.no_fallback", LocalizationService.Get("missing.no_fallback"));
        }

        [UnityTest]
        public IEnumerator CoreScenes_FlowProfileAndMissionLoop_RemainGateCompliant()
        {
            int firstPressureTier = -1;
            int lastPressureTier = -1;

            for (int i = 0; i < CoreSceneNames.Length; i++)
            {
                string sceneName = CoreSceneNames[i];
                Assert.IsTrue(Application.CanStreamedLevelBeLoaded(sceneName), $"Scene not loadable: {sceneName}");

                SceneManager.LoadScene(sceneName);
                yield return null;
                yield return null;

                LevelFlowManager flow = Object.FindFirstObjectByType<LevelFlowManager>();
                if (flow != null)
                    Object.Destroy(flow.gameObject);

                LevelLoader loader = Object.FindFirstObjectByType<LevelLoader>();
                Assert.IsNotNull(loader, $"LevelLoader missing in {sceneName}");

                var levelData = loader.GetLevelData();
                Assert.IsNotNull(levelData, $"LevelData missing in {sceneName}");
                Assert.GreaterOrEqual(levelData.plannedMainRoutes, 2, $"Main route count below design floor in {sceneName}");
                Assert.GreaterOrEqual(levelData.plannedStealthRoutes, 1, $"Stealth route count below design floor in {sceneName}");
                Assert.Greater(levelData.completionWindowMinMinutes, 0f, $"Completion window min must be > 0 in {sceneName}");
                Assert.GreaterOrEqual(levelData.completionWindowMaxMinutes, levelData.completionWindowMinMinutes, $"Completion window max < min in {sceneName}");
                Assert.That(
                    levelData.designedCompletionMinutes,
                    Is.InRange(levelData.completionWindowMinMinutes, levelData.completionWindowMaxMinutes),
                    $"Designed completion out of configured window in {sceneName}");
                Assert.That(levelData.patrolPressureTier, Is.InRange(1, 5), $"Patrol pressure tier out of range in {sceneName}");

                if (firstPressureTier < 0)
                    firstPressureTier = levelData.patrolPressureTier;
                lastPressureTier = levelData.patrolPressureTier;

                int encounterBudget = levelData.infiltrationEnemyBudget +
                                      levelData.objectiveEnemyBudget +
                                      levelData.extractionEnemyBudget;
                Assert.Greater(encounterBudget, 0, $"Encounter budget should be positive in {sceneName}");

                MissionExitPoint exitPoint = Object.FindFirstObjectByType<MissionExitPoint>();
                Assert.IsNotNull(exitPoint, $"MissionExitPoint missing in {sceneName}");

                IntelPickup[] intelPickups = Object.FindObjectsByType<IntelPickup>(FindObjectsSortMode.None);
                Assert.Greater(intelPickups.Length, 0, $"IntelPickup missing in {sceneName}");

                GameObject player = TryFindPlayer();
                Assert.IsNotNull(player, $"Player missing in {sceneName}");
                Collider playerCollider = player.GetComponent<Collider>();
                Assert.IsNotNull(playerCollider, $"Player collider missing in {sceneName}");

                int levelIndex = levelData.levelIndex;
                exitPoint.Configure(levelIndex, true, 1);

                int missionExitEvents = 0;
                global::System.Action<MissionExitTriggeredEvent> onMissionExit = evt =>
                {
                    if (evt.levelIndex == levelIndex)
                        missionExitEvents++;
                };
                EventBus.Subscribe(onMissionExit);

                try
                {
                    InvokeExitTrigger(exitPoint, playerCollider);
                    Assert.AreEqual(0, missionExitEvents, $"Exit should remain locked before intel in {sceneName}");

                    for (int intelIndex = 0; intelIndex < intelPickups.Length; intelIndex++)
                    {
                        if (intelPickups[intelIndex] != null)
                            intelPickups[intelIndex].Collect();
                    }

                    yield return null;

                    InvokeExitTrigger(exitPoint, playerCollider);
                    Assert.AreEqual(1, missionExitEvents, $"Exit should trigger after intel collection in {sceneName}");
                }
                finally
                {
                    EventBus.Unsubscribe(onMissionExit);
                }
            }

            Assert.GreaterOrEqual(
                lastPressureTier,
                firstPressureTier,
                "Level pressure tier should not regress from first level to final level.");
        }

        private static IEnumerator CleanupSingletonManagers()
        {
            GameManager[] gameManagers = Object.FindObjectsByType<GameManager>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < gameManagers.Length; i++)
            {
                if (gameManagers[i] != null)
                    Object.Destroy(gameManagers[i].gameObject);
            }

            SaveLoadManager[] saveManagers = Object.FindObjectsByType<SaveLoadManager>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < saveManagers.Length; i++)
            {
                if (saveManagers[i] != null)
                    Object.Destroy(saveManagers[i].gameObject);
            }

            yield return null;
        }

        private static void ClearAllSaveSlots(SaveLoadManager saveLoadManager)
        {
            for (int slot = 0; slot < SaveLoadManager.MaxSaveSlots; slot++)
                saveLoadManager.DeleteSave(slot);
        }

        private static string BuildSaveJson(int credits, int highestLevel, int currentLevel, int slotIndex)
        {
            SaveLoadManager.SaveData data = new SaveLoadManager.SaveData
            {
                schemaVersion = SaveLoadManager.CurrentSaveSchemaVersion,
                slotIndex = slotIndex,
                saveId = "iteration12-p2-recovery",
                saveTimestampUtc = "2026-04-02T00:00:00.0000000Z",
                credits = credits,
                highestLevel = highestLevel,
                currentLevel = currentLevel,
                bloodlineLevel = 2,
                totalPlayTime = 256f,
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

        private static void InvokeExitTrigger(MissionExitPoint exitPoint, Collider playerCollider)
        {
            MethodInfo method = typeof(MissionExitPoint).GetMethod(
                "OnTriggerEnter",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "Failed to reflect MissionExitPoint.OnTriggerEnter.");
            method.Invoke(exitPoint, new object[] { playerCollider });
        }

        private static GameObject TryFindPlayer()
        {
            try
            {
                return GameObject.FindGameObjectWithTag("Player");
            }
            catch (UnityException)
            {
                return null;
            }
        }
    }
}
