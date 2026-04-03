using NUnit.Framework;
using INTIFALL.UI;
using UnityEngine;
using System.Reflection;

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

        [Test]
        public void FormatObjectiveCounter_FormatsProgressTemplate()
        {
            string formatted = InvokeFormatObjectiveCounter("Secondary: Objectives {0}/{1}", 1, 3, "Secondary: {0}/{1}");
            Assert.AreEqual("Secondary: Objectives 1/3", formatted);
        }

        [Test]
        public void FormatObjectiveCounter_InvalidTemplate_FallsBack()
        {
            string formatted = InvokeFormatObjectiveCounter("Secondary: {2}", 2, 4, "Secondary: {0}/{1}");
            Assert.AreEqual("Secondary: 2/4", formatted);
        }

        private static string InvokeFormatObjectiveCounter(string template, int completed, int total, string fallback)
        {
            MethodInfo method = typeof(HUDManager).GetMethod(
                "FormatObjectiveCounter",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "Missing HUDManager.FormatObjectiveCounter.");

            return (string)method.Invoke(null, new object[] { template, completed, total, fallback });
        }
    }
}
