using NUnit.Framework;
using INTIFALL.AI;
using INTIFALL.Environment;
using INTIFALL.System;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class EnemyStateMachineTests
    {
        private EnemyStateMachine _sm;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("EnemyStateMachine");
            _sm = _go.AddComponent<EnemyStateMachine>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void InitialState_ShouldBeUnaware()
        {
            Assert.AreEqual(EEnemyState.Unaware, _sm.CurrentState);
        }

        [Test]
        public void TransitionTo_SameState_DoesNothing()
        {
            _sm.TransitionTo(EEnemyState.Unaware);
            Assert.AreEqual(EEnemyState.Unaware, _sm.CurrentState);
        }

        [Test]
        public void TransitionTo_Suspicious_ChangesState()
        {
            _sm.TransitionTo(EEnemyState.Suspicious);
            Assert.AreEqual(EEnemyState.Suspicious, _sm.CurrentState);
        }

        [Test]
        public void TransitionTo_Searching_ChangesState()
        {
            _sm.TransitionTo(EEnemyState.Searching);
            Assert.AreEqual(EEnemyState.Searching, _sm.CurrentState);
        }

        [Test]
        public void TransitionTo_Alert_ChangesState()
        {
            _sm.TransitionTo(EEnemyState.Alert);
            Assert.AreEqual(EEnemyState.Alert, _sm.CurrentState);
        }

        [Test]
        public void TransitionTo_FullAlert_ChangesState()
        {
            _sm.TransitionTo(EEnemyState.FullAlert);
            Assert.AreEqual(EEnemyState.FullAlert, _sm.CurrentState);
        }

        [Test]
        public void CanSeePlayer_AlertOrAbove_ReturnsTrue()
        {
            _sm.TransitionTo(EEnemyState.Alert);
            Assert.IsTrue(_sm.CanSeePlayer());
        }

        [Test]
        public void CanSeePlayer_Unaware_ReturnsFalse()
        {
            Assert.IsFalse(_sm.CanSeePlayer());
        }

        [Test]
        public void IsInvestigating_SuspiciousOrSearching_ReturnsTrue()
        {
            _sm.TransitionTo(EEnemyState.Suspicious);
            Assert.IsTrue(_sm.IsInvestigating());
        }

        [Test]
        public void IsInvestigating_Unaware_ReturnsFalse()
        {
            Assert.IsFalse(_sm.IsInvestigating());
        }

        [Test]
        public void IsAlerted_AlertOrFullAlert_ReturnsTrue()
        {
            _sm.TransitionTo(EEnemyState.FullAlert);
            Assert.IsTrue(_sm.IsAlerted());
        }

        [Test]
        public void IsAlerted_Unaware_ReturnsFalse()
        {
            Assert.IsFalse(_sm.IsAlerted());
        }

        [Test]
        public void OnPlayerLost_FromAlert_KeepsAlertUntilDropDelay()
        {
            _sm.TransitionTo(EEnemyState.Alert);
            _sm.OnPlayerLost();
            Assert.AreEqual(EEnemyState.Alert, _sm.CurrentState);
        }

        [Test]
        public void OnPlayerLost_FromFullAlert_RemainsFullAlert()
        {
            _sm.TransitionTo(EEnemyState.FullAlert);
            _sm.OnPlayerLost();
            Assert.AreEqual(EEnemyState.FullAlert, _sm.CurrentState);
        }

        [Test]
        public void OnPlayerDetected_FromSearching_TransitionsToAlert()
        {
            _sm.TransitionTo(EEnemyState.Searching);
            _sm.OnPlayerDetected(Vector3.one);
            Assert.AreEqual(EEnemyState.Alert, _sm.CurrentState);
        }

        [Test]
        public void OnSquadAlert_LowPriority_FromUnaware_TransitionsToSuspicious()
        {
            _sm.TransitionTo(EEnemyState.Unaware);
            _sm.OnSquadAlert(new Vector3(3f, 0f, 2f), 1, false);

            Assert.AreEqual(EEnemyState.Suspicious, _sm.CurrentState);
        }

        [Test]
        public void OnSquadAlert_HighPriority_FromSearching_TransitionsToAlert()
        {
            _sm.TransitionTo(EEnemyState.Searching);
            _sm.OnSquadAlert(new Vector3(5f, 0f, 5f), 3, true);

            Assert.AreEqual(EEnemyState.Alert, _sm.CurrentState);
        }

        [Test]
        public void OnSquadAlert_UpdatesSearchAnchorAndWave()
        {
            Vector3 alertPos = new Vector3(7f, 0f, -4f);
            _sm.OnSquadAlert(alertPos, 12, false);

            Assert.AreEqual(alertPos, _sm.SearchAnchor);
            Assert.AreEqual(12, _sm.SearchWaveId);
        }

        [Test]
        public void ConfigureTimingProfile_AppliesAndNormalizesDurations()
        {
            _sm.ConfigureTimingProfile(
                searchDurationSeconds: 9.5f,
                alertDurationSeconds: 5.8f,
                fullAlertMissionFailDelaySeconds: 34f,
                alertDropToSearchDelaySeconds: 1.4f,
                suspiciousDurationSeconds: 2.6f);

            Assert.AreEqual(9.5f, _sm.SearchDuration, 0.001f);
            Assert.AreEqual(5.8f, _sm.AlertDuration, 0.001f);
            Assert.AreEqual(34f, _sm.FullAlertMissionFailDelay, 0.001f);
            Assert.AreEqual(1.4f, _sm.AlertDropToSearchDelay, 0.001f);
            Assert.AreEqual(2.6f, _sm.SuspiciousDuration, 0.001f);
        }

        [Test]
        public void TerminalAlertSuppressedEvent_FromFullAlert_TransitionsToSearching()
        {
            InvokeLifecycle(_sm, "OnEnable");
            try
            {
                _sm.TransitionTo(EEnemyState.FullAlert);
                Assert.AreEqual(EEnemyState.FullAlert, _sm.CurrentState);

                EventBus.Publish(new TerminalAlertSuppressedEvent
                {
                    sourceTerminalId = "terminal_qa",
                    levelIndex = 0,
                    durationSeconds = 8f,
                    sourcePosition = Vector3.zero,
                    effectRadiusMeters = 20f,
                    waveId = 7
                });

                Assert.AreEqual(EEnemyState.Searching, _sm.CurrentState);
            }
            finally
            {
                InvokeLifecycle(_sm, "OnDisable");
            }
        }

        [Test]
        public void TerminalAlertSuppressedEvent_OutOfRange_DoesNotChangeState()
        {
            InvokeLifecycle(_sm, "OnEnable");
            try
            {
                _sm.transform.position = new Vector3(50f, 0f, 0f);
                _sm.TransitionTo(EEnemyState.Alert);

                EventBus.Publish(new TerminalAlertSuppressedEvent
                {
                    sourceTerminalId = "terminal_far",
                    levelIndex = 0,
                    durationSeconds = 8f,
                    sourcePosition = Vector3.zero,
                    effectRadiusMeters = 10f,
                    waveId = 2
                });

                Assert.AreEqual(EEnemyState.Alert, _sm.CurrentState);
            }
            finally
            {
                InvokeLifecycle(_sm, "OnDisable");
            }
        }

        private static void InvokeLifecycle(EnemyStateMachine target, string methodName)
        {
            var method = typeof(EnemyStateMachine).GetMethod(
                methodName,
                global::System.Reflection.BindingFlags.Instance | global::System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Missing lifecycle method {methodName}.");
            method.Invoke(target, null);
        }
    }
}
