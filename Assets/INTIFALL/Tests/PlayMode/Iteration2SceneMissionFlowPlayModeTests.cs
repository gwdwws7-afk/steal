using System.Collections;
using System.Reflection;
using INTIFALL.Economy;
using INTIFALL.Level;
using INTIFALL.Narrative;
using INTIFALL.System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class Iteration2SceneMissionFlowPlayModeTests
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
        public IEnumerator CoreScenes_IntelAndExitFlow_WorksForSmoke()
        {
            for (int i = 0; i < CoreSceneNames.Length; i++)
            {
                string sceneName = CoreSceneNames[i];
                SceneManager.LoadScene(sceneName);
                yield return null;
                yield return null;

                var loader = Object.FindFirstObjectByType<LevelLoader>();
                Assert.IsNotNull(loader, $"LevelLoader missing in {sceneName}");

                var flow = Object.FindFirstObjectByType<LevelFlowManager>();
                if (flow != null)
                    Object.Destroy(flow.gameObject);

                var supplyPoints = Object.FindObjectsByType<SupplyPoint>(FindObjectsSortMode.None);
                Assert.Greater(supplyPoints.Length, 0, $"SupplyPoint missing in {sceneName}");

                var intelPickups = Object.FindObjectsByType<IntelPickup>(FindObjectsSortMode.None);
                Assert.Greater(intelPickups.Length, 0, $"IntelPickup missing in {sceneName}");

                var exit = Object.FindFirstObjectByType<MissionExitPoint>();
                Assert.IsNotNull(exit, $"MissionExitPoint missing in {sceneName}");

                var player = TryFindPlayer();
                Assert.IsNotNull(player, $"Player missing in {sceneName}");
                var playerCollider = player.GetComponent<Collider>();
                Assert.IsNotNull(playerCollider, $"Player collider missing in {sceneName}");

                int levelIndex = loader.GetLevelData() != null ? loader.GetLevelData().levelIndex : i;
                exit.Configure(levelIndex, true, 1);

                int intelCollectedEvents = 0;
                int missionExitEvents = 0;
                int missionOutcomeEvents = 0;

                global::System.Action<IntelCollectedInSceneEvent> onIntel = evt =>
                {
                    if (evt.levelIndex == levelIndex)
                        intelCollectedEvents++;
                };
                global::System.Action<MissionExitTriggeredEvent> onExit = evt =>
                {
                    if (evt.levelIndex == levelIndex)
                        missionExitEvents++;
                };
                global::System.Action<MissionOutcomeEvaluatedEvent> onOutcome = evt =>
                {
                    if (evt.levelIndex == levelIndex)
                        missionOutcomeEvents++;
                };

                EventBus.Subscribe(onIntel);
                EventBus.Subscribe(onExit);
                EventBus.Subscribe(onOutcome);

                try
                {
                    InvokeExitTrigger(exit, playerCollider);
                    Assert.AreEqual(0, missionExitEvents, $"Exit should stay locked before intel in {sceneName}");

                    for (int intelIndex = 0; intelIndex < intelPickups.Length; intelIndex++)
                    {
                        if (intelPickups[intelIndex] != null)
                            intelPickups[intelIndex].Collect();
                    }

                    yield return null;
                    Assert.GreaterOrEqual(
                        intelCollectedEvents,
                        intelPickups.Length,
                        $"Not all intel events fired in {sceneName}");

                    InvokeExitTrigger(exit, playerCollider);
                    Assert.AreEqual(1, missionExitEvents, $"Exit should trigger after intel in {sceneName}");
                    Assert.AreEqual(1, missionOutcomeEvents, $"Mission outcome should be evaluated in {sceneName}");
                }
                finally
                {
                    EventBus.Unsubscribe(onIntel);
                    EventBus.Unsubscribe(onExit);
                    EventBus.Unsubscribe(onOutcome);
                }
            }
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
