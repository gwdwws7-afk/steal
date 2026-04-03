using System.Collections;
using INTIFALL.Level;
using INTIFALL.Narrative;
using INTIFALL.System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class Iteration7NarrativeContinuityPlayModeTests
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
        public IEnumerator CoreScenes_NarrativeChain_ResolvesExtendedOutcomeTokens()
        {
            for (int i = 0; i < CoreSceneNames.Length; i++)
            {
                string sceneName = CoreSceneNames[i];
                SceneManager.LoadScene(sceneName);
                yield return null;
                yield return null;

                var flow = Object.FindFirstObjectByType<LevelFlowManager>();
                if (flow != null)
                    Object.Destroy(flow.gameObject);

                var loader = Object.FindFirstObjectByType<LevelLoader>();
                Assert.IsNotNull(loader, $"LevelLoader missing in {sceneName}");

                var willa = Object.FindFirstObjectByType<WillaComm>();
                Assert.IsNotNull(willa, $"WillaComm missing in {sceneName}");

                SetPrivateField(willa, "typingSpeed", 0f);
                willa.CloseComm();

                int levelIndex = loader.GetLevelData() != null ? loader.GetLevelData().levelIndex : i;
                WillaMessageEvent lastMessage = default;
                int missionStartCount = 0;
                int intelFoundCount = 0;
                int missionCompleteCount = 0;

                global::System.Action<WillaMessageEvent> onMessage = evt =>
                {
                    if (evt.levelIndex != levelIndex)
                        return;

                    lastMessage = evt;
                    if (evt.trigger == EWillaTrigger.MissionStart) missionStartCount++;
                    if (evt.trigger == EWillaTrigger.IntelFound) intelFoundCount++;
                    if (evt.trigger == EWillaTrigger.MissionComplete) missionCompleteCount++;
                };

                EventBus.Subscribe(onMessage);
                try
                {
                    willa.TriggerComm(EWillaTrigger.MissionStart, levelIndex);
                    yield return null;
                    Assert.GreaterOrEqual(missionStartCount, 1, $"Missing MissionStart in {sceneName}");

                    willa.CloseComm();
                    yield return null;

                    EventBus.Publish(new IntelCollectedInSceneEvent
                    {
                        intelId = $"i7_intel_{i}",
                        levelIndex = levelIndex
                    });
                    yield return null;
                    Assert.GreaterOrEqual(intelFoundCount, 1, $"Missing IntelFound in {sceneName}");

                    willa.CloseComm();
                    yield return null;

                    EventBus.Publish(new MissionOutcomeEvaluatedEvent
                    {
                        levelIndex = levelIndex,
                        rank = "A",
                        rankScore = 4,
                        creditsEarned = 420,
                        intelCollected = 2,
                        intelRequired = 3,
                        secondaryObjectivesCompleted = 2,
                        secondaryObjectivesTotal = 3,
                        zeroKill = false,
                        noDamage = true,
                        wasDiscovered = true,
                        fullAlertTriggered = false,
                        extractionRouteId = "coolant_tunnel",
                        extractionRouteLabel = "Coolant Tunnel",
                        usedOptionalExit = true,
                        routeRiskTier = 3,
                        routeCreditMultiplier = 1.25f,
                        toolsUsed = 4,
                        alertsTriggered = 1
                    });
                    yield return null;

                    Assert.GreaterOrEqual(missionCompleteCount, 1, $"Missing MissionComplete in {sceneName}");
                    Assert.AreEqual(EWillaTrigger.MissionComplete, lastMessage.trigger, $"Unexpected final trigger in {sceneName}");
                    Assert.IsFalse(lastMessage.messageKey.Contains("{"), $"Unresolved token remains in {sceneName}: {lastMessage.messageKey}");
                    Assert.IsFalse(lastMessage.messageKey.ToUpperInvariant().Contains("TODO"), $"Placeholder text in {sceneName}: {lastMessage.messageKey}");
                }
                finally
                {
                    EventBus.Unsubscribe(onMessage);
                }
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, global::System.Reflection.BindingFlags.Instance | global::System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing private field '{fieldName}' on {target.GetType().Name}.");
            field.SetValue(target, value);
        }
    }
}
