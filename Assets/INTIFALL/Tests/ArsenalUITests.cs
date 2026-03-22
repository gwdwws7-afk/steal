using NUnit.Framework;
using INTIFALL.Economy;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class ArsenalUITests
    {
        private ArsenalUI _arsenal;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("ArsenalUI");
            _arsenal = _go.AddComponent<ArsenalUI>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void IsOpen_InitiallyFalse()
        {
            Assert.IsFalse(_arsenal.IsOpen);
        }

        [Test]
        public void OpenArsenal_SetsIsOpenTrue()
        {
            _arsenal.OpenArsenal();
            Assert.IsTrue(_arsenal.IsOpen);
        }

        [Test]
        public void CloseArsenal_SetsIsOpenFalse()
        {
            _arsenal.OpenArsenal();
            _arsenal.CloseArsenal();
            Assert.IsFalse(_arsenal.IsOpen);
        }

        [Test]
        public void ToggleArsenal_OpensWhenClosed()
        {
            _arsenal.ToggleArsenal();
            Assert.IsTrue(_arsenal.IsOpen);
        }

        [Test]
        public void ToggleArsenal_ClosesWhenOpen()
        {
            _arsenal.OpenArsenal();
            _arsenal.ToggleArsenal();
            Assert.IsFalse(_arsenal.IsOpen);
        }

        [Test]
        public void OpenArsenal_Twice_DoesNothing()
        {
            _arsenal.OpenArsenal();
            _arsenal.OpenArsenal();
            Assert.IsTrue(_arsenal.IsOpen);
        }
    }
}