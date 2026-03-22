using NUnit.Framework;
using INTIFALL.Economy;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class SupplyPointTests
    {
        private SupplyPoint _supply;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("SupplyPoint");
            _supply = _go.AddComponent<SupplyPoint>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void IsOnCooldown_InitiallyFalse()
        {
            Assert.IsFalse(_supply.IsOnCooldown);
        }

        [Test]
        public void CooldownProgress_InitiallyFull()
        {
            Assert.AreEqual(1f, _supply.CooldownProgress);
        }

        [Test]
        public void TrySupply_WithNoPlayer_DoesNothing()
        {
            _supply.TrySupply();
            Assert.IsFalse(_supply.IsOnCooldown);
        }
    }
}