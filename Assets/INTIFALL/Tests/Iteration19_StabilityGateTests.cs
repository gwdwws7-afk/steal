using INTIFALL.AI;
using INTIFALL.Level;
using INTIFALL.Narrative;
using INTIFALL.Player;
using INTIFALL.System;
using INTIFALL.Tools;
using INTIFALL.UI;
using NUnit.Framework;
using System.Reflection;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class Iteration19_StabilityGateTests
    {
        [SetUp]
        public void SetUp()
        {
            EventBus.ClearAllSubscribers();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.ClearAllSubscribers();
        }

        [Test]
        public void RuntimeCoreComponents_RepeatedEnableDisable_DoesNotLeakEventSubscriptions()
        {
            var hudGo = new GameObject("HUD");
            var hud = hudGo.AddComponent<HUDManager>();

            var willaGo = new GameObject("Willa");
            var willa = willaGo.AddComponent<WillaComm>();

            var trackerGo = new GameObject("Tracker");
            var tracker = trackerGo.AddComponent<SecondaryObjectiveTracker>();

            try
            {
                InvokeLifecycle(hud, "OnEnable");
                InvokeLifecycle(willa, "OnEnable");
                InvokeLifecycle(tracker, "OnEnable");
                AssertSubscriberSnapshot(0, 2, 1, 0, 3, 2, 1, 2, 1, 1, 1, 1, 1);

                for (int i = 0; i < 15; i++)
                {
                    InvokeLifecycle(hud, "OnDisable");
                    InvokeLifecycle(willa, "OnDisable");
                    InvokeLifecycle(tracker, "OnDisable");
                    AssertSubscriberSnapshot(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

                    InvokeLifecycle(hud, "OnEnable");
                    InvokeLifecycle(willa, "OnEnable");
                    InvokeLifecycle(tracker, "OnEnable");
                    AssertSubscriberSnapshot(0, 2, 1, 0, 3, 2, 1, 2, 1, 1, 1, 1, 1);
                }
            }
            finally
            {
                InvokeLifecycle(tracker, "OnDisable");
                InvokeLifecycle(willa, "OnDisable");
                InvokeLifecycle(hud, "OnDisable");
                Object.DestroyImmediate(trackerGo);
                Object.DestroyImmediate(willaGo);
                Object.DestroyImmediate(hudGo);
            }
        }

        [Test]
        public void EventBus_IdempotentSubscribe_DoesNotDuplicateHandlersAfterLoop()
        {
            int invokeCount = 0;
            global::System.Action<PlayerMovedEvent> handler = _ => invokeCount++;

            for (int i = 0; i < 30; i++)
            {
                EventBus.Subscribe(handler);
                EventBus.Subscribe(handler);
            }

            Assert.AreEqual(1, EventBus.GetSubscriberCount<PlayerMovedEvent>());
            EventBus.Publish(new PlayerMovedEvent());
            Assert.AreEqual(1, invokeCount);
        }

        private static void AssertSubscriberSnapshot(
            int enemyKilled,
            int alertStateChanged,
            int hpChanged,
            int toolUsed,
            int levelLoaded,
            int intelCollected,
            int missionExitTriggered,
            int missionOutcomeEvaluated,
            int secondaryRegistered,
            int secondaryCompleted,
            int secondaryFailed,
            int secondaryProgress,
            int narrativeTriggered)
        {
            Assert.AreEqual(enemyKilled, EventBus.GetSubscriberCount<EnemyKilledEvent>(), "EnemyKilledEvent subscriber count mismatch.");
            Assert.AreEqual(alertStateChanged, EventBus.GetSubscriberCount<AlertStateChangedEvent>(), "AlertStateChangedEvent subscriber count mismatch.");
            Assert.AreEqual(hpChanged, EventBus.GetSubscriberCount<HPChangedEvent>(), "HPChangedEvent subscriber count mismatch.");
            Assert.AreEqual(toolUsed, EventBus.GetSubscriberCount<ToolUsedEvent>(), "ToolUsedEvent subscriber count mismatch.");
            Assert.AreEqual(levelLoaded, EventBus.GetSubscriberCount<LevelLoadedEvent>(), "LevelLoadedEvent subscriber count mismatch.");
            Assert.AreEqual(intelCollected, EventBus.GetSubscriberCount<IntelCollectedInSceneEvent>(), "IntelCollectedInSceneEvent subscriber count mismatch.");
            Assert.AreEqual(missionExitTriggered, EventBus.GetSubscriberCount<MissionExitTriggeredEvent>(), "MissionExitTriggeredEvent subscriber count mismatch.");
            Assert.AreEqual(missionOutcomeEvaluated, EventBus.GetSubscriberCount<MissionOutcomeEvaluatedEvent>(), "MissionOutcomeEvaluatedEvent subscriber count mismatch.");

            Assert.AreEqual(secondaryRegistered, EventBus.GetSubscriberCount<SecondaryObjectiveRegisteredEvent>(), "SecondaryObjectiveRegisteredEvent subscriber count mismatch.");
            Assert.AreEqual(secondaryCompleted, EventBus.GetSubscriberCount<SecondaryObjectiveCompletedEvent>(), "SecondaryObjectiveCompletedEvent subscriber count mismatch.");
            Assert.AreEqual(secondaryFailed, EventBus.GetSubscriberCount<SecondaryObjectiveFailedEvent>(), "SecondaryObjectiveFailedEvent subscriber count mismatch.");
            Assert.AreEqual(secondaryProgress, EventBus.GetSubscriberCount<SecondaryObjectiveProgressEvent>(), "SecondaryObjectiveProgressEvent subscriber count mismatch.");
            Assert.AreEqual(narrativeTriggered, EventBus.GetSubscriberCount<NarrativeTriggeredEvent>(), "NarrativeTriggeredEvent subscriber count mismatch.");
        }

        private static void InvokeLifecycle(object target, string methodName)
        {
            if (target == null)
                return;

            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Missing lifecycle method {methodName} on {target.GetType().Name}.");
            method.Invoke(target, null);
        }
    }
}
