using NUnit.Framework;
using INTIFALL.Economy;
using INTIFALL.System;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class CreditSystemTests
    {
        private CreditSystem _credit;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("CreditSystem");
            _credit = _go.AddComponent<CreditSystem>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void CurrentCredits_InitiallyZero()
        {
            Assert.AreEqual(0, _credit.CurrentCredits);
        }

        [Test]
        public void CanAfford_WithEnoughCredits_ReturnsTrue()
        {
            _credit.EarnCredits(100, "test");
            Assert.IsTrue(_credit.CanAfford(50));
        }

        [Test]
        public void CanAfford_WithInsufficientCredits_ReturnsFalse()
        {
            Assert.IsFalse(_credit.CanAfford(100));
        }

        [Test]
        public void EarnCredits_IncreasesBalance()
        {
            _credit.EarnCredits(100, "test");
            Assert.AreEqual(100, _credit.CurrentCredits);
        }

        [Test]
        public void EarnCredits_IncreasesTotalEarned()
        {
            _credit.EarnCredits(100, "test");
            Assert.AreEqual(100, _credit.TotalEarned);
        }

        [Test]
        public void SpendCredits_WithEnoughCredits_ReturnsTrue()
        {
            _credit.EarnCredits(100, "test");
            bool result = _credit.SpendCredits(50, "Purchase", "TestTool");
            Assert.IsTrue(result);
            Assert.AreEqual(50, _credit.CurrentCredits);
        }

        [Test]
        public void SpendCredits_WithInsufficientCredits_ReturnsFalse()
        {
            bool result = _credit.SpendCredits(100, "Purchase", "TestTool");
            Assert.IsFalse(result);
            Assert.AreEqual(0, _credit.CurrentCredits);
        }

        [Test]
        public void SpendCredits_DecreasesTotalSpent()
        {
            _credit.EarnCredits(100, "test");
            _credit.SpendCredits(30, "Purchase", "TestTool");
            Assert.AreEqual(30, _credit.TotalSpent);
        }

        [Test]
        public void EarnCredits_WithNegativeAmount_DoesNothing()
        {
            _credit.EarnCredits(-100, "test");
            Assert.AreEqual(0, _credit.CurrentCredits);
        }

        [Test]
        public void SpendCredits_WithNegativeAmount_DoesNothing()
        {
            _credit.EarnCredits(100, "test");
            _credit.SpendCredits(-50, "Purchase", "TestTool");
            Assert.AreEqual(100, _credit.CurrentCredits);
        }

        [Test]
        public void ResetForNewGame_ResetsAllValues()
        {
            _credit.EarnCredits(500, "test");
            _credit.SpendCredits(100, "Purchase", "TestTool");

            _credit.ResetForNewGame();

            Assert.AreEqual(0, _credit.CurrentCredits);
            Assert.AreEqual(0, _credit.TotalEarned);
            Assert.AreEqual(0, _credit.TotalSpent);
        }

        [Test]
        public void AddMissionReward_SRank_Adds500()
        {
            _credit.AddMissionReward(5, 0, 0, false, false, 0f);
            Assert.AreEqual(500, _credit.CurrentCredits);
        }

        [Test]
        public void AddMissionReward_ARank_Adds350()
        {
            _credit.AddMissionReward(4, 0, 0, false, false, 0f);
            Assert.AreEqual(350, _credit.CurrentCredits);
        }

        [Test]
        public void AddMissionReward_BRank_Adds200()
        {
            _credit.AddMissionReward(3, 0, 0, false, false, 0f);
            Assert.AreEqual(200, _credit.CurrentCredits);
        }

        [Test]
        public void AddMissionReward_ZeroKill_Adds150()
        {
            _credit.AddMissionReward(5, 0, 0, true, false, 0f);
            Assert.AreEqual(650, _credit.CurrentCredits);
        }

        [Test]
        public void AddMissionReward_NoDamage_Adds200()
        {
            _credit.AddMissionReward(5, 0, 0, false, true, 0f);
            Assert.AreEqual(700, _credit.CurrentCredits);
        }

        [Test]
        public void AddMissionReward_IntelCount_Adds50Each()
        {
            _credit.AddMissionReward(5, 0, 3, false, false, 0f);
            Assert.AreEqual(650, _credit.CurrentCredits);
        }
    }
}