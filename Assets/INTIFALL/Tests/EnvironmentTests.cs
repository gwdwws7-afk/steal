using NUnit.Framework;
using INTIFALL.Environment;
using INTIFALL.Input;
using INTIFALL.Player;
using INTIFALL.System;
using UnityEngine;
using System.Reflection;

namespace INTIFALL.Tests
{
    public class EnvironmentTests
    {
        [Test]
        public void BreakableWall_Defaults()
        {
            var wall = new GameObject("BreakableWall").AddComponent<BreakableWall>();
            Assert.IsFalse(wall.IsBroken);
            Assert.IsFalse(wall.IsBreaking);
            Assert.AreEqual(0f, wall.BreakProgress);
            Object.DestroyImmediate(wall.gameObject);
        }

        [Test]
        public void HangingPoint_Defaults()
        {
            var point = new GameObject("HangingPoint").AddComponent<HangingPoint>();
            Assert.IsFalse(point.IsOccupied);
            Assert.IsFalse(point.PlayerAttached);
            Object.DestroyImmediate(point.gameObject);
        }

        [Test]
        public void ElectronicDoor_Defaults()
        {
            var door = new GameObject("ElectronicDoor").AddComponent<ElectronicDoor>();
            Assert.IsFalse(door.IsOpen);
            Assert.IsTrue(door.IsLocked);
            Object.DestroyImmediate(door.gameObject);
        }

        [Test]
        public void ElectronicDoor_ApplyEMPDisruption_UnlocksAndOpensDoor()
        {
            var doorGo = new GameObject("ElectronicDoor");
            doorGo.AddComponent<BoxCollider>();
            var door = doorGo.AddComponent<ElectronicDoor>();

            Assert.IsTrue(door.IsLocked);
            Assert.IsFalse(door.IsOpen);

            door.ApplyEMPDisruption(2f);

            Assert.IsTrue(door.IsEMPDisabled);
            Assert.IsFalse(door.IsLocked);
            Assert.IsTrue(door.IsOpen);

            Object.DestroyImmediate(doorGo);
        }

        [Test]
        public void ElectronicDoor_InteractEvent_WhenPlayerInRange_OpensUnlockedDoor()
        {
            EventBus.ClearAllSubscribers();

            var doorGo = new GameObject("ElectronicDoor");
            doorGo.AddComponent<BoxCollider>();
            var door = doorGo.AddComponent<ElectronicDoor>();
            InvokePrivate(door, "OnEnable");

            try
            {
                door.Unlock();
                SetPrivateField(door, "_playerInRange", true);

                EventBus.Publish(new InputManager.InteractEvent());

                Assert.IsTrue(door.IsOpen, "Door should open on interact event when player is in range and unlocked.");
            }
            finally
            {
                InvokePrivate(door, "OnDisable");
                Object.DestroyImmediate(doorGo);
                EventBus.ClearAllSubscribers();
            }
        }

        [Test]
        public void SurveillanceCamera_Defaults()
        {
            var cam = new GameObject("SurveillanceCamera").AddComponent<SurveillanceCamera>();
            Assert.IsFalse(cam.PlayerDetected);
            Object.DestroyImmediate(cam.gameObject);
        }

        [Test]
        public void VentEntrance_Defaults()
        {
            var vent = new GameObject("VentEntrance").AddComponent<VentEntrance>();
            Assert.IsFalse(vent.IsInside);
            Object.DestroyImmediate(vent.gameObject);
        }

        [Test]
        public void HangingPoint_PlayerDetachedExternally_ReleasesOccupiedState()
        {
            var player = new GameObject("Player");
            player.AddComponent<PlayerStateMachine>();
            player.AddComponent<CharacterController>();
            var playerController = player.AddComponent<PlayerController>();
            InvokePrivate(playerController, "Awake");
            InvokePrivate(playerController, "OnEnable");

            var pointGo = new GameObject("HangingPoint");
            var point = pointGo.AddComponent<HangingPoint>();

            SetPrivateField(point, "_player", player);
            InvokePrivate(point, "Attach");

            Assert.IsTrue(point.IsOccupied);
            Assert.IsTrue(point.PlayerAttached);
            Assert.IsTrue(playerController.IsOnRope);

            playerController.DetachFromRope();
            InvokePrivate(point, "Update");

            Assert.IsFalse(point.IsOccupied, "Hanging point should release occupied state after external rope detach.");
            Assert.IsFalse(point.PlayerAttached, "Hanging point should clear player attachment after external rope detach.");

            InvokePrivate(playerController, "OnDisable");
            Object.DestroyImmediate(pointGo);
            Object.DestroyImmediate(player);
        }

        private static void InvokePrivate(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Missing private method: {methodName}");
            method.Invoke(target, null);
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing private field: {fieldName}");
            field.SetValue(target, value);
        }
    }
}
