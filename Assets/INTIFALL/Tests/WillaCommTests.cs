using NUnit.Framework;
using INTIFALL.Level;
using INTIFALL.Narrative;
using INTIFALL.System;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class WillaCommTests
    {
        private WillaComm _willa;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            LocalizationService.SetLanguageOverride(SystemLanguage.English);
            _go = new GameObject("WillaComm");
            _willa = _go.AddComponent<WillaComm>();
        }

        [TearDown]
        public void Teardown()
        {
            LocalizationService.ClearLanguageOverride();
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void IsDisplaying_InitiallyFalse()
        {
            Assert.IsFalse(_willa.IsDisplaying);
        }

        [Test]
        public void CloseComm_DoesNotCrash()
        {
            _willa.CloseComm();
        }

        [Test]
        public void SkipTyping_DoesNotCrash()
        {
            _willa.SkipTyping();
        }

        [Test]
        public void LevelLoadedEvent_TriggersMissionStartComm()
        {
            InvokePrivateHandler("OnLevelLoaded", new LevelLoadedEvent
            {
                levelIndex = 0,
                levelName = "Level01_Qhapaq_Passage"
            });

            Assert.IsTrue(_willa.IsDisplaying);
        }

        [Test]
        public void IntelCollectedEvent_OnlyTriggersOncePerLevel()
        {
            InvokePrivateHandler("OnIntelCollectedInScene", new IntelCollectedInSceneEvent
            {
                intelId = "intel_a",
                levelIndex = 1
            });

            Assert.IsTrue(_willa.IsDisplaying);

            _willa.CloseComm();
            Assert.IsFalse(_willa.IsDisplaying);

            InvokePrivateHandler("OnIntelCollectedInScene", new IntelCollectedInSceneEvent
            {
                intelId = "intel_b",
                levelIndex = 1
            });

            Assert.IsFalse(_willa.IsDisplaying);
        }

        [Test]
        public void LevelReload_ResetsPerLevelTriggerFlags()
        {
            InvokePrivateHandler("OnIntelCollectedInScene", new IntelCollectedInSceneEvent
            {
                intelId = "intel_a",
                levelIndex = 2
            });
            Assert.IsTrue(_willa.IsDisplaying);
            _willa.CloseComm();

            InvokePrivateHandler("OnIntelCollectedInScene", new IntelCollectedInSceneEvent
            {
                intelId = "intel_b",
                levelIndex = 2
            });
            Assert.IsFalse(_willa.IsDisplaying);

            InvokePrivateHandler("OnLevelLoaded", new LevelLoadedEvent
            {
                levelIndex = 2,
                levelName = "Level03_Underground_Labs"
            });
            Assert.IsTrue(_willa.IsDisplaying);
            _willa.CloseComm();

            InvokePrivateHandler("OnIntelCollectedInScene", new IntelCollectedInSceneEvent
            {
                intelId = "intel_c",
                levelIndex = 2
            });
            Assert.IsTrue(_willa.IsDisplaying);
        }

        [Test]
        public void MissionOutcome_WhenCommBusy_IsQueuedAndDisplayedAfterClose()
        {
            InvokePrivateHandler("OnLevelLoaded", new LevelLoadedEvent
            {
                levelIndex = 2,
                levelName = "Level03_Underground_Labs"
            });
            Assert.IsTrue(_willa.IsDisplaying);

            InvokePrivateHandler("OnMissionOutcomeEvaluated", new MissionOutcomeEvaluatedEvent
            {
                levelIndex = 2,
                rank = "A",
                rankScore = 4,
                creditsEarned = 200
            });

            Assert.AreEqual(1, GetPendingMessageCount(_willa));

            _willa.CloseComm();
            Assert.IsTrue(_willa.IsDisplaying);
        }

        [Test]
        public void AlertStateChanged_FullAlert_TriggersWarningOncePerLevel()
        {
            InvokePrivateHandler("OnAlertStateChanged", new AlertStateChangedEvent
            {
                enemyId = 101,
                newState = EAlertState.FullAlert
            });

            Assert.IsTrue(_willa.IsDisplaying);
            _willa.CloseComm();
            Assert.IsFalse(_willa.IsDisplaying);

            InvokePrivateHandler("OnAlertStateChanged", new AlertStateChangedEvent
            {
                enemyId = 102,
                newState = EAlertState.FullAlert
            });

            Assert.IsFalse(_willa.IsDisplaying, "Warning trigger should be idempotent per level.");
        }

        [Test]
        public void MissionOutcome_FinalLevel_NoFullAlert_QueuesStoryReveal()
        {
            InvokePrivateHandler("OnMissionOutcomeEvaluated", new MissionOutcomeEvaluatedEvent
            {
                levelIndex = 4,
                rank = "S",
                rankScore = 5,
                creditsEarned = 500,
                fullAlertTriggered = false
            });

            Assert.IsTrue(_willa.IsDisplaying);
            Assert.AreEqual(1, GetPendingMessageCount(_willa), "Expected MissionComplete + queued StoryReveal.");

            _willa.CloseComm();
            Assert.IsTrue(_willa.IsDisplaying, "Queued StoryReveal should display after MissionComplete closes.");
        }

        [Test]
        public void MissionOutcome_FinalLevel_FullAlert_QueuesBetrayal()
        {
            InvokePrivateHandler("OnMissionOutcomeEvaluated", new MissionOutcomeEvaluatedEvent
            {
                levelIndex = 4,
                rank = "C",
                rankScore = 2,
                creditsEarned = 120,
                fullAlertTriggered = true
            });

            Assert.IsTrue(_willa.IsDisplaying);
            Assert.AreEqual(1, GetPendingMessageCount(_willa), "Expected MissionComplete + queued Betrayal.");

            _willa.CloseComm();
            Assert.IsTrue(_willa.IsDisplaying, "Queued Betrayal should display after MissionComplete closes.");
        }

        [Test]
        public void MissionOutcomeTemplate_ReplacesRuntimeTokens()
        {
            InvokePrivateHandler("OnMissionOutcomeEvaluated", new MissionOutcomeEvaluatedEvent
            {
                levelIndex = 3,
                rank = "S",
                rankScore = 5,
                creditsEarned = 420,
                intelCollected = 3,
                intelRequired = 3,
                secondaryObjectivesCompleted = 2,
                secondaryObjectivesTotal = 3,
                zeroKill = true,
                noDamage = true,
                wasDiscovered = false,
                fullAlertTriggered = false,
                extractionRouteLabel = "Upper Ring Catwalk",
                usedOptionalExit = true,
                routeRiskTier = 3,
                routeCreditMultiplier = 1.3f,
                toolsUsed = 4,
                alertsTriggered = 1
            });

            string resolved = InvokePrivateMessageResolver(
                EWillaTrigger.MissionComplete,
                3,
                "Rank {rank} ({rank_score}) Credits {credits} Intel {intel_collected}/{intel_required} Missing {intel_missing} Secondary {secondary_completed}/{secondary_total} Stealth {stealth_status} Combat {combat_style} Damage {damage_status} Route {route_label} {route_type} Risk {route_risk} Mult {route_multiplier} Tools {tools_used} Alerts {alerts_triggered}");

            Assert.AreEqual(
                "Rank S (5) Credits 420 Intel 3/3 Missing 0 Secondary 2/3 Stealth Undetected Combat Zero-Kill Damage No-Damage Route Upper Ring Catwalk Optional Risk 3 Mult 1.30 Tools 4 Alerts 1",
                resolved);
        }

        [Test]
        public void MissionOutcomeTemplate_WithoutSnapshot_UsesFallbackValues()
        {
            string resolved = InvokePrivateMessageResolver(
                EWillaTrigger.MissionComplete,
                99,
                "Rank {rank} ({rank_score}) Credits {credits}");

            Assert.AreEqual("Rank N/A (0) Credits 0", resolved);
        }

        [Test]
        public void NarrativeTriggered_ScriptedWarningToken_TriggersWarning()
        {
            InvokePrivateHandler("OnNarrativeTriggered", new NarrativeTriggeredEvent
            {
                eventType = ENarrativeEventType.ScriptedTrigger,
                eventId = "warning",
                levelIndex = 1
            });

            Assert.IsTrue(_willa.IsDisplaying);
        }

        [Test]
        public void NarrativeTriggered_ScriptedStoryToken_TriggersStoryReveal()
        {
            InvokePrivateHandler("OnNarrativeTriggered", new NarrativeTriggeredEvent
            {
                eventType = ENarrativeEventType.ScriptedTrigger,
                eventId = "story_reveal",
                levelIndex = 2
            });

            Assert.IsTrue(_willa.IsDisplaying);
        }

        [Test]
        public void NarrativeTriggered_ScriptedBetrayalToken_TriggersBetrayal()
        {
            InvokePrivateHandler("OnNarrativeTriggered", new NarrativeTriggeredEvent
            {
                eventType = ENarrativeEventType.ScriptedTrigger,
                eventId = "betrayal",
                levelIndex = 3
            });

            Assert.IsTrue(_willa.IsDisplaying);
        }

        private void InvokePrivateHandler<T>(string methodName, T eventValue)
        {
            var method = typeof(WillaComm).GetMethod(methodName, global::System.Reflection.BindingFlags.Instance | global::System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Expected private handler {methodName}.");
            method.Invoke(_willa, new object[] { eventValue });
        }

        private static int GetPendingMessageCount(WillaComm willa)
        {
            var field = typeof(WillaComm).GetField("_pendingMessages", global::System.Reflection.BindingFlags.Instance | global::System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(field, "Expected _pendingMessages queue field.");

            object queueObject = field.GetValue(willa);
            Assert.IsNotNull(queueObject, "Expected _pendingMessages queue instance.");

            var countProperty = queueObject.GetType().GetProperty("Count");
            Assert.IsNotNull(countProperty, "Expected queue Count property.");

            return (int)countProperty.GetValue(queueObject);
        }

        private string InvokePrivateMessageResolver(EWillaTrigger trigger, int levelIndex, string template)
        {
            var method = typeof(WillaComm).GetMethod("ResolveRuntimeMessage", global::System.Reflection.BindingFlags.Instance | global::System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(method, "Expected private resolver ResolveRuntimeMessage.");

            object result = method.Invoke(_willa, new object[] { trigger, levelIndex, template });
            Assert.IsNotNull(result);
            return (string)result;
        }
    }
}
