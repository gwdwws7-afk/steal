using NUnit.Framework;
using INTIFALL.AI;
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
    }
}
