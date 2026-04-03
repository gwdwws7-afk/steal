using INTIFALL.Level;
using INTIFALL.System;
using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace INTIFALL.Tests
{
    public class SecondaryObjectiveTrackerTests
    {
        private GameObject _trackerGo;
        private SecondaryObjectiveTracker _tracker;

        [SetUp]
        public void SetUp()
        {
            EventBus.ClearAllSubscribers();

            _trackerGo = new GameObject("SecondaryObjectiveTracker");
            _tracker = _trackerGo.AddComponent<SecondaryObjectiveTracker>();
            InvokeLifecycle(_tracker, "OnEnable");
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.ClearAllSubscribers();

            if (_tracker != null)
                InvokeLifecycle(_tracker, "OnDisable");
            if (_trackerGo != null)
                Object.DestroyImmediate(_trackerGo);
        }

        [Test]
        public void DefaultTrackerState_StartsWithTwoCompletedStealthObjectives()
        {
            SecondaryObjectiveSummary summary = _tracker.GetSummary();

            Assert.AreEqual(2, summary.total);
            Assert.AreEqual(2, summary.completed);
        }

        [Test]
        public void AlertEvents_DegradeStealthObjectivesInOrder()
        {
            EventBus.Publish(new AlertStateChangedEvent
            {
                enemyId = 1,
                newState = EAlertState.Alert
            });

            SecondaryObjectiveSummary afterAlert = _tracker.GetSummary();
            Assert.AreEqual(2, afterAlert.total);
            Assert.AreEqual(1, afterAlert.completed);

            EventBus.Publish(new AlertStateChangedEvent
            {
                enemyId = 1,
                newState = EAlertState.FullAlert
            });

            SecondaryObjectiveSummary afterFullAlert = _tracker.GetSummary();
            Assert.AreEqual(2, afterFullAlert.total);
            Assert.AreEqual(0, afterFullAlert.completed);
        }

        [Test]
        public void CustomObjectiveEvents_UpdateSummaryWithoutHardcodedRules()
        {
            EventBus.Publish(new SecondaryObjectiveRegisteredEvent
            {
                objectiveId = "collect_data_core",
                startsCompleted = false
            });

            SecondaryObjectiveSummary afterRegister = _tracker.GetSummary();
            Assert.AreEqual(3, afterRegister.total);
            Assert.AreEqual(2, afterRegister.completed);

            EventBus.Publish(new SecondaryObjectiveCompletedEvent
            {
                objectiveId = "collect_data_core"
            });

            SecondaryObjectiveSummary afterComplete = _tracker.GetSummary();
            Assert.AreEqual(3, afterComplete.total);
            Assert.AreEqual(3, afterComplete.completed);

            EventBus.Publish(new SecondaryObjectiveFailedEvent
            {
                objectiveId = "collect_data_core"
            });

            SecondaryObjectiveSummary afterFail = _tracker.GetSummary();
            Assert.AreEqual(3, afterFail.total);
            Assert.AreEqual(2, afterFail.completed);
        }

        private static void InvokeLifecycle(SecondaryObjectiveTracker tracker, string methodName)
        {
            MethodInfo method = typeof(SecondaryObjectiveTracker).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Missing lifecycle method {methodName}.");
            method.Invoke(tracker, null);
        }
    }
}
