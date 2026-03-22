using NUnit.Framework;
using INTIFALL.Data;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class LevelDataTests
    {
        private LevelData _levelData;

        [SetUp]
        public void Setup()
        {
            _levelData = ScriptableObject.CreateInstance<LevelData>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_levelData);
        }

        [Test]
        public void DefaultValues_AreCorrect()
        {
            Assert.AreEqual(3, _levelData.qhipuFragmentCount);
            Assert.AreEqual(3, _levelData.terminalCount);
            Assert.AreEqual(2, _levelData.supplyPointCount);
        }

        [Test]
        public void SetLevelName_SetsCorrectly()
        {
            _levelData.levelName = "Level_01";
            Assert.AreEqual("Level_01", _levelData.levelName);
        }

        [Test]
        public void SetLevelIndex_SetsCorrectly()
        {
            _levelData.levelIndex = 2;
            Assert.AreEqual(2, _levelData.levelIndex);
        }

        [Test]
        public void SetSceneName_SetsCorrectly()
        {
            _levelData.sceneName = "Level_03";
            Assert.AreEqual("Level_03", _levelData.sceneName);
        }

        [Test]
        public void DefaultRewards_AreCorrect()
        {
            Assert.AreEqual(200, _levelData.baseCreditReward);
            Assert.AreEqual(150, _levelData.zeroKillBonus);
            Assert.AreEqual(200, _levelData.noDamageBonus);
        }
    }
}