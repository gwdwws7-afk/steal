using NUnit.Framework;
using INTIFALL.UI;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class AlertIndicatorTests
    {
        private AlertIndicator _alert;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("AlertIndicator");
            _alert = _go.AddComponent<AlertIndicator>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void IsAlerted_InitiallyFalse()
        {
            Assert.IsFalse(_alert.IsAlerted);
        }

        [Test]
        public void AlertLevel_InitiallyZero()
        {
            Assert.AreEqual(0f, _alert.AlertLevel);
        }

        [Test]
        public void SetAlertState_True_SetsIsAlertedTrue()
        {
            _alert.SetAlertState(true);
            Assert.IsTrue(_alert.IsAlerted);
        }

        [Test]
        public void SetAlertState_False_SetsIsAlertedFalse()
        {
            _alert.SetAlertState(true);
            _alert.SetAlertState(false);
            Assert.IsFalse(_alert.IsAlerted);
        }

        [Test]
        public void SetAlertLevel_ClampsBetweenZeroAndOne()
        {
            _alert.SetAlertLevel(1.5f);
            Assert.AreEqual(1f, _alert.AlertLevel);

            _alert.SetAlertLevel(-0.5f);
            Assert.AreEqual(0f, _alert.AlertLevel);
        }
    }
}