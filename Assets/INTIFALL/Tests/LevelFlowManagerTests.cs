using NUnit.Framework;
using INTIFALL.Level;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class LevelFlowManagerTests
    {
        private LevelFlowManager _flow;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("LevelFlowManager");
            _flow = _go.AddComponent<LevelFlowManager>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void TotalLevelCount_Is5()
        {
            Assert.AreEqual(5, _flow.TotalLevelCount);
        }

        [Test]
        public void GetLevelSceneName_ValidIndex_ReturnsName()
        {
            Assert.AreEqual("Level01_Qhapaq_Passage", _flow.GetLevelSceneName(0));
            Assert.AreEqual("Level05_General_Taki_Villa", _flow.GetLevelSceneName(4));
        }

        [Test]
        public void GetLevelSceneName_InvalidIndex_ReturnsEmpty()
        {
            Assert.AreEqual("", _flow.GetLevelSceneName(-1));
            Assert.AreEqual("", _flow.GetLevelSceneName(99));
        }

        [Test]
        public void GetLevelDisplayName_ValidIndex_ReturnsName()
        {
            Assert.AreEqual("Golden Ruins", _flow.GetLevelDisplayName(0));
        }

        [Test]
        public void GetLevelDisplayName_InvalidIndex_ReturnsEmpty()
        {
            Assert.AreEqual("", _flow.GetLevelDisplayName(-1));
        }

        [Test]
        public void IsLevelUnlocked_Level1_IsTrue()
        {
            Assert.IsTrue(_flow.IsLevelUnlocked(0));
        }

        [Test]
        public void IsTransitioning_InitiallyFalse()
        {
            Assert.IsFalse(_flow.IsTransitioning);
        }

        [Test]
        public void GetLevelIndexFromSceneName_Valid_ReturnsIndex()
        {
            Assert.AreEqual(0, _flow.GetLevelIndexFromSceneName("Level01_Qhapaq_Passage"));
            Assert.AreEqual(4, _flow.GetLevelIndexFromSceneName("Level05_General_Taki_Villa"));
        }

        [Test]
        public void GetLevelIndexFromSceneName_Invalid_ReturnsNegative()
        {
            Assert.AreEqual(-1, _flow.GetLevelIndexFromSceneName("InvalidScene"));
        }

        [Test]
        public void HighestUnlockedLevel_InitiallyAtLeast1()
        {
            Assert.GreaterOrEqual(_flow.HighestUnlockedLevel, 1);
        }
    }
}
