using NUnit.Framework;
using INTIFALL.Level;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class LevelLoaderTests
    {
        private LevelLoader _loader;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("LevelLoader");
            _loader = _go.AddComponent<LevelLoader>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void EnemiesSpawned_InitiallyZero()
        {
            Assert.AreEqual(0, _loader.EnemiesSpawned);
        }

        [Test]
        public void IntelSpawned_InitiallyZero()
        {
            Assert.AreEqual(0, _loader.IntelSpawned);
        }

        [Test]
        public void GetLevelData_ReturnsData()
        {
            var data = _loader.GetLevelData();
        }

        [Test]
        public void GetEnemySpawnData_ReturnsData()
        {
            var data = _loader.GetEnemySpawnData();
        }

        [Test]
        public void GetIntelSpawnData_ReturnsData()
        {
            var data = _loader.GetIntelSpawnData();
        }
    }
}