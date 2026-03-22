using NUnit.Framework;
using INTIFALL.Narrative;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class WillaCommTests
    {
        private WillaComm _willa;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("WillaComm");
            _willa = _go.AddComponent<WillaComm>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void IsDisplaying_InitiallyFalse()
        {
            Assert.IsFalse(_willa.IsDisplaying);
        }

        [Test]
        public void CloseComm_DoesNotCrash()
        {
            _willa.CloseComm();
        }

        [Test]
        public void SkipTyping_DoesNotCrash()
        {
            _willa.SkipTyping();
        }
    }
}