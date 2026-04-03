using NUnit.Framework;
using INTIFALL.UI;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class EagleEyeUITests
    {
        private EagleEyeUI _eagle;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("EagleEyeUI");
            _eagle = _go.AddComponent<EagleEyeUI>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void CurrentIntel_InitiallyZero()
        {
            Assert.AreEqual(0, _eagle.CurrentIntel);
        }

        [Test]
        public void SetIntelCount_SetsCorrectValue()
        {
            _eagle.SetIntelCount(2);
            Assert.AreEqual(2, _eagle.CurrentIntel);
        }

        [Test]
        public void SetIntelCount_ClampsToMax()
        {
            _eagle.SetIntelCount(10);
            Assert.LessOrEqual(_eagle.CurrentIntel, 3);
        }

        [Test]
        public void ResetIntel_SetsToZero()
        {
            _eagle.SetIntelCount(2);
            _eagle.ResetIntel();
            Assert.AreEqual(0, _eagle.CurrentIntel);
        }

        [Test]
        public void SetIntelTarget_UpdatesTotalTarget()
        {
            _eagle.SetIntelTarget(5);
            Assert.AreEqual(5, _eagle.TotalIntelTarget);
        }

        [Test]
        public void SetIntelTarget_ClampsCurrentIntelToNewLimit()
        {
            _eagle.SetIntelCount(3);
            _eagle.SetIntelTarget(2);
            Assert.AreEqual(2, _eagle.CurrentIntel);
        }
    }
}
