using NUnit.Framework;
using INTIFALL.Data;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class GameConfigTests
    {
        private GameConfig _config;

        [SetUp]
        public void Setup()
        {
            _config = ScriptableObject.CreateInstance<GameConfig>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void DefaultPlayerSettings_AreCorrect()
        {
            Assert.AreEqual(5, _config.playerMaxHP);
            Assert.AreEqual(5, _config.playerMaxFirstAid);
            Assert.AreEqual(0.5f, _config.playerInvincibilityDuration);
        }

        [Test]
        public void DefaultMovementSpeeds_AreCorrect()
        {
            Assert.AreEqual(4.5f, _config.walkSpeed);
            Assert.AreEqual(7.0f, _config.sprintSpeed);
            Assert.AreEqual(1.5f, _config.crouchSpeed);
        }

        [Test]
        public void DefaultEconomySettings_AreCorrect()
        {
            Assert.AreEqual(0, _config.startingCredits);
            Assert.AreEqual(500, _config.sRankBaseCredit);
            Assert.AreEqual(350, _config.aRankBaseCredit);
            Assert.AreEqual(200, _config.bRankBaseCredit);
        }

        [Test]
        public void DefaultEnemySettings_AreCorrect()
        {
            Assert.AreEqual(15f, _config.enemyVisionDistance);
            Assert.AreEqual(60f, _config.enemyVisionAngle);
            Assert.AreEqual(30f, _config.enemyCommRange);
        }

        [Test]
        public void DefaultConfig_ReturnsValidConfig()
        {
            var defaultConfig = GameConfig.DefaultConfig();
            Assert.IsNotNull(defaultConfig);
            Object.DestroyImmediate(defaultConfig);
        }
    }
}