using NUnit.Framework;
using INTIFALL.Growth;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class LevelUpRewardTests
    {
        private LevelUpReward _reward;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("LevelUpReward");
            _reward = _go.AddComponent<LevelUpReward>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void CalculateRank_PerfectStats_ReturnsSRank()
        {
            _reward.SetMissionStats(true, true, 3, 3, 120f);
            Assert.AreEqual(5, _reward.CalculateRank());
        }

        [Test]
        public void CalculateRank_GoodStats_ReturnsARank()
        {
            _reward.SetMissionStats(false, true, 2, 2, 60f);
            Assert.AreEqual(4, _reward.CalculateRank());
        }

        [Test]
        public void CalculateRank_NormalStats_ReturnsBRank()
        {
            _reward.SetMissionStats(false, false, 1, 0, 30f);
            Assert.AreEqual(3, _reward.CalculateRank());
        }

        [Test]
        public void CalculateRank_PoorStats_ReturnsCRank()
        {
            _reward.SetMissionStats(false, false, 1, 0, 0f);
            Assert.AreEqual(2, _reward.CalculateRank());
        }

        [Test]
        public void GetRankName_ReturnsCorrectName()
        {
            Assert.AreEqual("S", _reward.GetRankName(5));
            Assert.AreEqual("A", _reward.GetRankName(4));
            Assert.AreEqual("B", _reward.GetRankName(3));
            Assert.AreEqual("C", _reward.GetRankName(2));
            Assert.AreEqual("D", _reward.GetRankName(1));
        }

        [Test]
        public void SetCurrentLevel_SetsLevel()
        {
            _reward.SetCurrentLevel(3);
            Assert.AreEqual(3, _reward.GetCurrentLevel());
        }

        [Test]
        public void GetCurrentLevel_InitiallyReturns1()
        {
            Assert.AreEqual(1, _reward.GetCurrentLevel());
        }
    }
}