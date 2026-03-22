using NUnit.Framework;
using INTIFALL.Environment;
using UnityEngine;

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
    }
}