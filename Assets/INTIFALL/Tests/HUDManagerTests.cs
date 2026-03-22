using NUnit.Framework;
using INTIFALL.UI;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class HUDManagerTests
    {
        private HUDManager _hud;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("HUDManager");
            _hud = _go.AddComponent<HUDManager>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void IsVisible_InitiallyTrue()
        {
            Assert.IsTrue(_hud.IsVisible);
        }

        [Test]
        public void ToggleHUD_SwitchesVisibility()
        {
            bool initial = _hud.IsVisible;
            _hud.ToggleHUD();
            Assert.AreNotEqual(initial, _hud.IsVisible);
        }

        [Test]
        public void ShowHUD_SetsVisibleTrue()
        {
            _hud.HideHUD();
            _hud.ShowHUD();
            Assert.IsTrue(_hud.IsVisible);
        }

        [Test]
        public void HideHUD_SetsVisibleFalse()
        {
            _hud.ShowHUD();
            _hud.HideHUD();
            Assert.IsFalse(_hud.IsVisible);
        }
    }
}