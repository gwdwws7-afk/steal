using NUnit.Framework;
using INTIFALL.Growth;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class ProgressionTreeTests
    {
        private ProgressionTree _progression;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("ProgressionTree");
            _progression = _go.AddComponent<ProgressionTree>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void CurrentCompletedLevel_InitiallyZero()
        {
            Assert.AreEqual(0, _progression.CurrentCompletedLevel);
        }

        [Test]
        public void CompleteLevel_Level1_UnlocksTools()
        {
            _progression.CompleteLevel(1);
            Assert.AreEqual(1, _progression.CurrentCompletedLevel);
            Assert.IsTrue(_progression.IsToolUnlocked("SmokeBomb"));
            Assert.IsTrue(_progression.IsToolUnlocked("FlashBang"));
        }

        [Test]
        public void CompleteLevel_Level2_UnlocksMoreTools()
        {
            _progression.CompleteLevel(2);
            Assert.IsTrue(_progression.IsToolUnlocked("TimedNoise"));
            Assert.IsTrue(_progression.IsToolUnlocked("WallBreak"));
        }

        [Test]
        public void CompleteLevel_Duplicate_DoesNothing()
        {
            _progression.CompleteLevel(1);
            _progression.CompleteLevel(1);
            Assert.AreEqual(1, _progression.CurrentCompletedLevel);
        }

        [Test]
        public void CompleteLevel_OutOfOrder_DoesNothing()
        {
            _progression.CompleteLevel(2);
            Assert.AreEqual(0, _progression.CurrentCompletedLevel);
        }

        [Test]
        public void GetUpgradeLevel_UnupgradedTool_ReturnsZero()
        {
            Assert.AreEqual(0, _progression.GetUpgradeLevel("SmokeBomb"));
        }

        [Test]
        public void CanUpgrade_WithoutUnlock_ReturnsFalse()
        {
            Assert.IsFalse(_progression.CanUpgrade("SmokeBomb"));
        }

        [Test]
        public void GetMaxUpgradeLevel_ReturnsCorrectMax()
        {
            Assert.AreEqual(2, _progression.GetMaxUpgradeLevel("SmokeBomb"));
            Assert.AreEqual(2, _progression.GetMaxUpgradeLevel("FlashBang"));
            Assert.AreEqual(2, _progression.GetMaxUpgradeLevel("EMP"));
            Assert.AreEqual(1, _progression.GetMaxUpgradeLevel("UnknownTool"));
        }

        [Test]
        public void GetToolStatBonus_ReturnsZeroForUnknownTool()
        {
            Assert.AreEqual(0f, _progression.GetToolStatBonus("UnknownTool", "Radius"));
        }

        [Test]
        public void ResetProgression_ResetsLevel()
        {
            _progression.CompleteLevel(3);
            _progression.ResetProgression();
            Assert.AreEqual(0, _progression.CurrentCompletedLevel);
            Assert.IsFalse(_progression.IsToolUnlocked("EMP"));
        }
    }
}