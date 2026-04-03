using INTIFALL.Player;
using INTIFALL.System;
using INTIFALL.Tools;
using NUnit.Framework;
using System.Reflection;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class PlayerControllerRopeTests
    {
        private GameObject _go;
        private PlayerController _controller;

        [SetUp]
        public void Setup()
        {
            EventBus.ClearAllSubscribers();

            _go = new GameObject("PlayerControllerRopeTests_Player");
            _go.AddComponent<PlayerStateMachine>();
            _go.AddComponent<CharacterController>();
            _controller = _go.AddComponent<PlayerController>();

            InvokePrivateLifecycle(_controller, "Awake");
            InvokePrivateLifecycle(_controller, "OnEnable");
        }

        [TearDown]
        public void TearDown()
        {
            if (_controller != null)
                InvokePrivateLifecycle(_controller, "OnDisable");

            EventBus.ClearAllSubscribers();
            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        [Test]
        public void RopeEventSubscription_EnableDisable_IsLeakSafe()
        {
            Assert.AreEqual(1, EventBus.GetSubscriberCount<RopeUsedEvent>());

            InvokePrivateLifecycle(_controller, "OnDisable");
            _controller.enabled = false;
            Assert.AreEqual(0, EventBus.GetSubscriberCount<RopeUsedEvent>());

            _controller.enabled = true;
            InvokePrivateLifecycle(_controller, "OnEnable");
            Assert.AreEqual(1, EventBus.GetSubscriberCount<RopeUsedEvent>());
        }

        [Test]
        public void RopeUsedEvent_AttachesPlayerToRopeState()
        {
            EventBus.Publish(new RopeUsedEvent
            {
                position = _go.transform.position,
                range = 8f,
                duration = 2f
            });

            Assert.IsTrue(_controller.IsOnRope, "Player should enter rope mode after RopeUsedEvent.");
            Assert.AreEqual(EPlayerState.Rope, _controller.State, "State machine should transition to Rope.");
        }

        [Test]
        public void DetachFromRope_TransitionsBackToIdle()
        {
            _controller.AttachToRope(_go.transform.position + Vector3.up, 1f);
            Assert.IsTrue(_controller.IsOnRope);

            _controller.DetachFromRope();

            Assert.IsFalse(_controller.IsOnRope);
            Assert.AreEqual(EPlayerState.Idle, _controller.State);
        }

        private static void InvokePrivateLifecycle(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method != null)
                method.Invoke(target, null);
        }
    }
}
