using System.Reflection;
using INTIFALL.Level;
using INTIFALL.System;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class MissionExitPointTests
    {
        private GameObject _gmGo;
        private GameManager _gm;
        private GameObject _trackerGo;
        private SecondaryObjectiveTracker _tracker;
        private GameObject _exitGo;
        private MissionExitPoint _exitPoint;

        [SetUp]
        public void SetUp()
        {
            EventBus.ClearAllSubscribers();

            _gmGo = new GameObject("GameManager");
            _gm = _gmGo.AddComponent<GameManager>();
            _gm.LoadLevel(0, "Level01");
            _gm.StartGame();

            _trackerGo = new GameObject("SecondaryObjectiveTracker");
            _tracker = _trackerGo.AddComponent<SecondaryObjectiveTracker>();
            InvokeLifecycle(_tracker, "OnEnable");

            _exitGo = new GameObject("MissionExit");
            _exitGo.AddComponent<BoxCollider>();
            _exitPoint = _exitGo.AddComponent<MissionExitPoint>();
            _exitPoint.Configure(
                index: 0,
                requireAllIntel: false,
                requiredIntel: 0,
                routeId: "main",
                routeLabel: "Main Extraction",
                riskTier: 0,
                creditMultiplier: 1f,
                secondaryBonus: 0,
                mainRoute: true);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.ClearAllSubscribers();

            if (_exitGo != null)
                Object.DestroyImmediate(_exitGo);
            if (_tracker != null)
                InvokeLifecycle(_tracker, "OnDisable");
            if (_trackerGo != null)
                Object.DestroyImmediate(_trackerGo);
            if (_gmGo != null)
                Object.DestroyImmediate(_gmGo);
        }

        [Test]
        public void CompleteMission_UsesTrackerSummaryForSecondaryTotals()
        {
            EventBus.Publish(new SecondaryObjectiveRegisteredEvent
            {
                objectiveId = "optional_data_core",
                startsCompleted = false
            });

            bool receivedOutcome = false;
            MissionOutcomeEvaluatedEvent captured = default;
            global::System.Action<MissionOutcomeEvaluatedEvent> onOutcome = evt =>
            {
                receivedOutcome = true;
                captured = evt;
            };

            EventBus.Subscribe(onOutcome);
            try
            {
                MethodInfo completeMethod = typeof(MissionExitPoint).GetMethod(
                    "CompleteMission",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                Assert.IsNotNull(completeMethod, "Missing MissionExitPoint.CompleteMission.");
                completeMethod.Invoke(_exitPoint, null);
            }
            finally
            {
                EventBus.Unsubscribe(onOutcome);
            }

            Assert.IsTrue(receivedOutcome);
            Assert.AreEqual(3, captured.secondaryObjectivesTotal);
            Assert.AreEqual(2, captured.secondaryObjectivesCompleted);
            Assert.AreEqual(2, captured.secondaryObjectivesEvaluated);
            Assert.AreEqual(0, captured.toolRiskWindowAdjustment);
            Assert.AreEqual(0f, captured.toolCooldownLoad, 0.001f);
            Assert.AreEqual(0, captured.ropeToolUses);
            Assert.AreEqual(0, captured.smokeToolUses);
            Assert.AreEqual(0, captured.soundBaitToolUses);
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
