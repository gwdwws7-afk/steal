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
    public class Iteration22NarrativeAdvancedTriggerPlayModeTests
    {
        [UnityTest]
        public IEnumerator FinalScene_AdvancedNarrativeTriggers_AreEmittedFromRuntimeEvents()
        {
            SceneManager.LoadScene("Level05_General_Taki_Villa");
            yield return null;
            yield return null;

            var loader = Object.FindFirstObjectByType<LevelLoader>();
            Assert.IsNotNull(loader, "LevelLoader missing in Level05_General_Taki_Villa.");

            var willa = Object.FindFirstObjectByType<WillaComm>();
            Assert.IsNotNull(willa, "WillaComm missing in Level05_General_Taki_Villa.");

            SetPrivateField(willa, "typingSpeed", 0f);
            willa.SkipTyping();
            willa.CloseComm();
            yield return null;

            int levelIndex = loader.GetLevelData() != null ? loader.GetLevelData().levelIndex : 4;
            int warningCount = 0;
            int storyRevealCount = 0;
            int betrayalCount = 0;

            global::System.Action<WillaMessageEvent> onMessage = evt =>
            {
                if (evt.levelIndex != levelIndex)
                    return;

                if (evt.trigger == EWillaTrigger.Warning)
                    warningCount++;
                if (evt.trigger == EWillaTrigger.StoryReveal)
                    storyRevealCount++;
                if (evt.trigger == EWillaTrigger.Betrayal)
                    betrayalCount++;
            };

            EventBus.Subscribe(onMessage);
            try
            {
                EventBus.Publish(new AlertStateChangedEvent
                {
                    enemyId = 9001,
                    newState = EAlertState.FullAlert
                });
                yield return null;

                Assert.GreaterOrEqual(warningCount, 1, "Expected warning trigger from threat spike event.");
                willa.CloseComm();
                yield return null;

                EventBus.Publish(new MissionOutcomeEvaluatedEvent
                {
                    levelIndex = levelIndex,
                    rank = "A",
                    rankScore = 4,
                    creditsEarned = 320,
                    intelCollected = 3,
                    intelRequired = 3,
                    secondaryObjectivesCompleted = 2,
                    secondaryObjectivesTotal = 2,
                    zeroKill = true,
                    noDamage = true,
                    wasDiscovered = false,
                    fullAlertTriggered = false,
                    extractionRouteLabel = "Main Extraction",
                    usedOptionalExit = false,
                    routeRiskTier = 1,
                    routeCreditMultiplier = 1f,
                    toolsUsed = 2,
                    alertsTriggered = 0
                });
                yield return null;

                willa.CloseComm();
                yield return null;

                Assert.GreaterOrEqual(storyRevealCount, 1, "Expected story reveal trigger from non-betrayal final outcome.");

                EventBus.Publish(new MissionOutcomeEvaluatedEvent
                {
                    levelIndex = levelIndex,
                    rank = "C",
                    rankScore = 2,
                    creditsEarned = 120,
                    intelCollected = 2,
                    intelRequired = 3,
                    secondaryObjectivesCompleted = 1,
                    secondaryObjectivesTotal = 2,
                    zeroKill = false,
                    noDamage = false,
                    wasDiscovered = true,
                    fullAlertTriggered = true,
                    extractionRouteLabel = "Main Extraction",
                    usedOptionalExit = false,
                    routeRiskTier = 2,
                    routeCreditMultiplier = 1f,
                    toolsUsed = 5,
                    alertsTriggered = 2
                });
                yield return null;

                willa.CloseComm();
                yield return null;

                Assert.GreaterOrEqual(betrayalCount, 1, "Expected betrayal trigger from degraded final outcome branch.");
            }
            finally
            {
                EventBus.Unsubscribe(onMessage);
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
