using NUnit.Framework;
using INTIFALL.Player;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class PlayerStateMachineTests
    {
        private PlayerStateMachine _sm;

        [SetUp]
        public void Setup()
        {
            var go = new GameObject("StateMachine");
            _sm = go.AddComponent<PlayerStateMachine>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_sm.gameObject);
        }

        [Test]
        public void InitialState_ShouldBeIdle()
        {
            Assert.AreEqual(EPlayerState.Idle, _sm.CurrentState);
        }

        [Test]
        public void TransitionTo_ShouldChangeState()
        {
            _sm.TransitionTo(EPlayerState.Walk);
            Assert.AreEqual(EPlayerState.Walk, _sm.CurrentState);
        }

        [Test]
        public void TransitionTo_SameState_ShouldNotChange()
        {
            _sm.TransitionTo(EPlayerState.Walk);
            _sm.TransitionTo(EPlayerState.Walk);
            Assert.AreEqual(EPlayerState.Walk, _sm.CurrentState);
        }

        [Test]
        public void TransitionTo_Crouch_ShouldBeAccessible()
        {
            _sm.TransitionTo(EPlayerState.Crouch);
            Assert.AreEqual(EPlayerState.Crouch, _sm.CurrentState);
        }

        [Test]
        public void TransitionTo_Cover_ShouldBeAccessible()
        {
            _sm.TransitionTo(EPlayerState.Cover);
            Assert.AreEqual(EPlayerState.Cover, _sm.CurrentState);
        }

        [Test]
        public void TransitionTo_Roll_ShouldBeAccessible()
        {
            _sm.TransitionTo(EPlayerState.Roll);
            Assert.AreEqual(EPlayerState.Roll, _sm.CurrentState);
        }

        [Test]
        public void TransitionTo_Rope_ShouldBeAccessible()
        {
            _sm.TransitionTo(EPlayerState.Rope);
            Assert.AreEqual(EPlayerState.Rope, _sm.CurrentState);
        }

        [Test]
        public void StateTimer_ShouldIncrement()
        {
            _sm.TransitionTo(EPlayerState.Walk);
            _sm.Update();
            Assert.Greater(_sm.StateTimer, 0f);
        }

        [Test]
        public void Update_ShouldIncrementTimer()
        {
            float initial = _sm.StateTimer;
            _sm.Update();
            _sm.Update();
            Assert.Greater(_sm.StateTimer, initial);
        }

        [Test]
        public void CrouchTimer_ShouldStartAtZero()
        {
            Assert.AreEqual(0f, _sm.CrouchTimer);
        }

        [Test]
        public void IncrementCrouchTimer_ShouldIncrease()
        {
            _sm.IncrementCrouchTimer(1.5f);
            Assert.AreEqual(1.5f, _sm.CrouchTimer);
        }

        [Test]
        public void ResetCrouchTimer_ShouldSetToZero()
        {
            _sm.IncrementCrouchTimer(2f);
            _sm.ResetCrouchTimer();
            Assert.AreEqual(0f, _sm.CrouchTimer);
        }
    }
}
