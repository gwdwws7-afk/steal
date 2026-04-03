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
    public class Iteration3SceneNarrativeAuditPlayModeTests
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
        public IEnumerator CoreScenes_NarrativeTriggerCoverage_IsAudited()
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

                var narrative = Object.FindFirstObjectByType<NarrativeManager>();
                Assert.IsNotNull(narrative, $"NarrativeManager missing in {sceneName}");

                var willa = Object.FindFirstObjectByType<WillaComm>();
                Assert.IsNotNull(willa, $"WillaComm missing in {sceneName}");

                SetPrivateField(willa, "typingSpeed", 0f);
                willa.CloseComm();

                int levelIndex = loader.GetLevelData() != null ? loader.GetLevelData().levelIndex : i;
                int messageCount = 0;
                WillaMessageEvent lastMessage = default;

                global::System.Action<WillaMessageEvent> onMessage = evt =>
                {
                    if (evt.levelIndex != levelIndex)
                        return;

                    messageCount++;
                    lastMessage = evt;
                };

                EventBus.Subscribe(onMessage);
                try
                {
                    willa.TriggerComm(EWillaTrigger.MissionStart, levelIndex);
                    yield return null;
                    Assert.GreaterOrEqual(messageCount, 1, $"No mission-start comm in {sceneName}");
                    Assert.AreEqual(EWillaTrigger.MissionStart, lastMessage.trigger, $"Unexpected mission-start trigger in {sceneName}");
                    Assert.IsFalse(string.IsNullOrEmpty(lastMessage.messageKey), $"Mission-start message empty in {sceneName}");

                    willa.CloseComm();
                    yield return null;

                    EventBus.Publish(new IntelCollectedInSceneEvent
                    {
                        intelId = $"audit_intel_{i}",
                        levelIndex = levelIndex
                    });
                    yield return null;
                    Assert.AreEqual(EWillaTrigger.IntelFound, lastMessage.trigger, $"Intel-found comm missing in {sceneName}");

                    willa.CloseComm();
                    yield return null;

                    EventBus.Publish(new MissionOutcomeEvaluatedEvent
                    {
                        levelIndex = levelIndex,
                        rank = "A",
                        rankScore = 4,
                        creditsEarned = 100 + i,
                        intelCollected = 2,
                        intelRequired = 3,
                        secondaryObjectivesCompleted = 1,
                        zeroKill = false,
                        noDamage = true,
                        wasDiscovered = true,
                        fullAlertTriggered = false
                    });
                    yield return null;

                    Assert.AreEqual(EWillaTrigger.MissionComplete, lastMessage.trigger, $"Mission-complete comm missing in {sceneName}");
                    Assert.IsFalse(
                        lastMessage.messageKey.Contains("{rank}") ||
                        lastMessage.messageKey.Contains("{credits}") ||
                        lastMessage.messageKey.Contains("{intel_collected}"),
                        $"Mission-complete template not resolved in {sceneName}: {lastMessage.messageKey}");
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
