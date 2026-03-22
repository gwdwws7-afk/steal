using NUnit.Framework;
using INTIFALL.Data;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class IntelSpawnDataTests
    {
        private IntelSpawnData _intelData;

        [SetUp]
        public void Setup()
        {
            _intelData = ScriptableObject.CreateInstance<IntelSpawnData>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_intelData);
        }

        [Test]
        public void GetTotalIntelCount_WithNull_ReturnsZero()
        {
            Assert.AreEqual(0, _intelData.GetTotalIntelCount());
        }

        [Test]
        public void GetIntel_WithNull_ReturnsNull()
        {
            Assert.IsNull(_intelData.GetIntel("test"));
        }

        [Test]
        public void GetSupplyPoint_WithNull_ReturnsNull()
        {
            Assert.IsNull(_intelData.GetSupplyPoint("test"));
        }

        [Test]
        public void GetExitPoint_WithNull_ReturnsNull()
        {
            Assert.IsNull(_intelData.GetExitPoint("test"));
        }

        [Test]
        public void GetIntelByType_WithNull_ReturnsEmpty()
        {
            Assert.AreEqual(0, _intelData.GetIntelByType(EIntelType.QhipuFragment).Length);
        }

        [Test]
        public void SetLevelIndex_SetsCorrectly()
        {
            _intelData.levelIndex = 2;
            Assert.AreEqual(2, _intelData.levelIndex);
        }
    }
}