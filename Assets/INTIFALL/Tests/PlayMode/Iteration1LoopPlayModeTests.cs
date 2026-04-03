using System.Collections;
using System.Reflection;
using INTIFALL.AI;
using INTIFALL.Data;
using INTIFALL.Economy;
using INTIFALL.Level;
using INTIFALL.Narrative;
using INTIFALL.System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class Iteration1LoopPlayModeTests
    {
        private LevelData _runtimeLevelData;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            CleanupRuntimeObjects();
            yield break;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            CleanupRuntimeObjects();

            if (_runtimeLevelData != null)
            {
                Object.DestroyImmediate(_runtimeLevelData);
                _runtimeLevelData = null;
            }

            yield break;
        }

        [UnityTest]
        public IEnumerator LevelLoader_LoadLevel_SpawnsDefaultLoopActors()
        {
            var loaderObject = new GameObject("Test_LevelLoader");
            var loader = loaderObject.AddComponent<LevelLoader>();

            _runtimeLevelData = ScriptableObject.CreateInstance<LevelData>();
            _runtimeLevelData.levelIndex = 0;
            _runtimeLevelData.levelName = "TestLevel";
            _runtimeLevelData.sceneName = "TestLevel";

            SetPrivateField(loader, "levelData", _runtimeLevelData);
            SetPrivateField(loader, "autoResolveDataBySceneName", false);
            SetPrivateField(loader, "autoCreatePlaceholderPlayer", true);
            SetPrivateField(loader, "spawnMissionExit", true);

            loader.LoadLevel();
            GameObject player = TryFindPlayer();
            Assert.IsNotNull(player, "LevelLoader should provide a Player object for Iteration 1.");

            var playerController = player.GetComponent<INTIFALL.Player.PlayerController>();
            if (playerController != null)
                playerController.enabled = false;

            Assert.AreEqual(1, loader.EnemiesSpawned, "Expected default single enemy spawn.");
            Assert.AreEqual(3, loader.IntelSpawned, "Expected default three intel spawns.");

            var exits = Object.FindObjectsByType<MissionExitPoint>(FindObjectsSortMode.None);
            Assert.AreEqual(1, exits.Length, "Expected one default mission exit.");
            yield break;
        }

        [UnityTest]
        public IEnumerator MissionExit_StaysLockedUntilIntelCollected()
        {
            var narrativeObject = new GameObject("Test_NarrativeManager");
            var narrative = narrativeObject.AddComponent<NarrativeManager>();

            var player = new GameObject("Test_Player");
            player.tag = "Player";
            var playerCollider = player.AddComponent<CapsuleCollider>();

            var exitObject = new GameObject("Test_Exit");
            var exitCollider = exitObject.AddComponent<BoxCollider>();
            exitCollider.isTrigger = true;
            var exitPoint = exitObject.AddComponent<MissionExitPoint>();
            exitPoint.Configure(0, true, 1);

            int missionExitEventCount = 0;
            global::System.Action<MissionExitTriggeredEvent> onMissionExit = _ => missionExitEventCount++;
            EventBus.Subscribe(onMissionExit);

            try
            {
                InvokeOnTriggerEnter(exitPoint, playerCollider);
                Assert.AreEqual(0, missionExitEventCount, "Exit must stay locked before intel is collected.");

                var intelObject = new GameObject("Test_Intel");
                intelObject.AddComponent<SphereCollider>().isTrigger = true;
                var intelPickup = intelObject.AddComponent<IntelPickup>();
                intelPickup.Configure("iteration1_test_intel", 0, EIntelType.QhipuFragment, "Test Intel");
                intelPickup.Collect();
                yield return null;

                Assert.GreaterOrEqual(
                    narrative.GetIntelCollectedForLevel(0),
                    1,
                    "Narrative should track at least one collected intel after pickup.");

                InvokeOnTriggerEnter(exitPoint, playerCollider);
                Assert.AreEqual(1, missionExitEventCount, "Exit should trigger after intel requirement is met.");
            }
            finally
            {
                EventBus.Unsubscribe(onMissionExit);
            }
        }

        private static void InvokeOnTriggerEnter(MissionExitPoint missionExitPoint, Collider playerCollider)
        {
            MethodInfo method = typeof(MissionExitPoint).GetMethod("OnTriggerEnter", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "Failed to reflect MissionExitPoint.OnTriggerEnter.");
            method.Invoke(missionExitPoint, new object[] { playerCollider });
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing private field '{fieldName}' on {target.GetType().Name}.");
            field.SetValue(target, value);
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

        private static void CleanupRuntimeObjects()
        {
            DestroyByComponent<LevelLoader>();
            DestroyByComponent<MissionExitPoint>();
            DestroyByComponent<IntelPickup>();
            DestroyByComponent<NarrativeManager>();
            DestroyByComponent<WillaComm>();
            DestroyByComponent<GameManager>();
            DestroyByComponent<LevelFlowManager>();
            DestroyByComponent<EnemyController>();
            DestroyByComponent<SupplyPoint>();

            GameObject player = TryFindPlayer();
            if (player != null)
                Object.DestroyImmediate(player);

            DestroyByName("Enemies");
            DestroyByName("Intel");
            DestroyByName("PlayerSpawnPoint");
            DestroyByName("INTIFALL_Runtime");
            DestroyByName("INTIFALL_Runtime_UI");
            DestroyByName("INTIFALL_Runtime_Narrative");
            DestroyByName("Test_LevelLoader");
            DestroyByName("Test_NarrativeManager");
            DestroyByName("Test_Player");
            DestroyByName("Test_Exit");
            DestroyByName("Test_Intel");
        }

        private static void DestroyByComponent<T>() where T : Component
        {
            T[] components = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] != null)
                    Object.DestroyImmediate(components[i].gameObject);
            }
        }

        private static void DestroyByName(string objectName)
        {
            GameObject found = GameObject.Find(objectName);
            if (found != null)
                Object.DestroyImmediate(found);
        }
    }
}
