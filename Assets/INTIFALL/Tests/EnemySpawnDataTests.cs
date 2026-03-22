using NUnit.Framework;
using INTIFALL.Data;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class EnemySpawnDataTests
    {
        private EnemySpawnData _spawnData;

        [SetUp]
        public void Setup()
        {
            _spawnData = ScriptableObject.CreateInstance<EnemySpawnData>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_spawnData);
        }

        [Test]
        public void GetTotalSpawnCount_WithNull_ReturnsZero()
        {
            Assert.AreEqual(0, _spawnData.GetTotalSpawnCount());
        }

        [Test]
        public void GetSpawnType_WithNullSpawnPoints_ReturnsNormal()
        {
            Assert.AreEqual(EEnemySpawnType.Normal, _spawnData.GetSpawnType(0));
        }

        [Test]
        public void GetSpawnPointsByType_WithNull_ReturnsEmpty()
        {
            Assert.AreEqual(0, _spawnData.GetSpawnPointsByType(EEnemySpawnType.Normal).Length);
        }

        [Test]
        public void SetLevelIndex_SetsCorrectly()
        {
            _spawnData.levelIndex = 3;
            Assert.AreEqual(3, _spawnData.levelIndex);
        }
    }
}