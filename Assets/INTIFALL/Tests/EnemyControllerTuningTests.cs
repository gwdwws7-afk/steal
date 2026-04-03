using System.Reflection;
using INTIFALL.AI;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class EnemyControllerTuningTests
    {
        private GameObject _enemyGo;
        private EnemyController _controller;

        [SetUp]
        public void SetUp()
        {
            _enemyGo = new GameObject("EnemyController_Tuning");
            _enemyGo.AddComponent<CharacterController>();
            _enemyGo.AddComponent<EnemyStateMachine>();
            _enemyGo.AddComponent<PerceptionModule>();
            _controller = _enemyGo.AddComponent<EnemyController>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_enemyGo != null)
                Object.DestroyImmediate(_enemyGo);
        }

        [Test]
        public void ConfigureDetectionProfile_AppliesValuesWithClamping()
        {
            _controller.ConfigureDetectionProfile(0.6f, 0.72f, 0.5f, 0.95f);

            Assert.AreEqual(0.6f, _controller.DetectionPulseInterval, 0.001f);
            Assert.AreEqual(0.72f, _controller.SearchingPulseMultiplier, 0.001f);
            Assert.AreEqual(0.5f, _controller.AlertedPulseMultiplier, 0.001f);
        }

        [Test]
        public void HandleFullAlert_TargetLost_DoesNotDropToSearching()
        {
            EnemyStateMachine stateMachine = _enemyGo.GetComponent<EnemyStateMachine>();
            stateMachine.TransitionTo(EEnemyState.FullAlert);

            MethodInfo method = typeof(EnemyController).GetMethod(
                "HandleFullAlert",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "Failed to reflect EnemyController.HandleFullAlert.");
            method.Invoke(_controller, null);

            Assert.AreEqual(EEnemyState.FullAlert, stateMachine.CurrentState);
        }
    }
}
