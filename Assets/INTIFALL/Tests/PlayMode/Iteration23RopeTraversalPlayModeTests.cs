using System.Collections;
using INTIFALL.Player;
using INTIFALL.System;
using INTIFALL.Tools;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class Iteration23RopeTraversalPlayModeTests
    {
        private GameObject _player;
        private PlayerController _controller;

        [SetUp]
        public void Setup()
        {
            EventBus.ClearAllSubscribers();

            _player = new GameObject("Iteration23_Player");
            _player.transform.position = Vector3.zero;
            _player.AddComponent<PlayerStateMachine>();
            _player.AddComponent<CharacterController>();
            _controller = _player.AddComponent<PlayerController>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.ClearAllSubscribers();
            if (_player != null)
                Object.Destroy(_player);
        }

        [UnityTest]
        public IEnumerator RopeAttach_WithDuration_AutoDetachesAfterTimeout()
        {
            yield return null;

            EventBus.Publish(new RopeUsedEvent
            {
                position = _player.transform.position,
                range = 8f,
                duration = 0.15f
            });

            yield return null;
            Assert.IsTrue(_controller.IsOnRope, "Player should attach to rope after tool event.");

            yield return new WaitForSeconds(0.3f);
            Assert.IsFalse(_controller.IsOnRope, "Rope should auto-detach when duration expires.");
            Assert.AreEqual(EPlayerState.Idle, _controller.State, "Player should return to idle after auto-detach.");
        }

        [UnityTest]
        public IEnumerator RopeRuntime_ConstrainsHorizontalAndVerticalOffsetAroundAnchor()
        {
            yield return null;

            Vector3 anchor = new Vector3(0f, 1f, 0f);
            _controller.AttachToRope(anchor, 1f);
            _player.transform.position = new Vector3(6f, 9f, 0f);

            yield return null;

            Vector3 clamped = _player.transform.position;
            float horizontalDistance = Vector2.Distance(
                new Vector2(clamped.x, clamped.z),
                new Vector2(anchor.x, anchor.z));

            Assert.LessOrEqual(horizontalDistance, 0.85f, "Horizontal rope drift exceeded snap radius.");
            Assert.GreaterOrEqual(clamped.y, anchor.y - 1.30f, "Rope lower Y clamp was not applied.");
            Assert.LessOrEqual(clamped.y, anchor.y + 2.05f, "Rope upper Y clamp was not applied.");
            Assert.IsTrue(_controller.IsOnRope, "Player should remain on rope while duration is active.");
        }
    }
}
