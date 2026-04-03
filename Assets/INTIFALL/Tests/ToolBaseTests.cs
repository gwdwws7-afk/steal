using NUnit.Framework;
using INTIFALL.Tools;
using INTIFALL.System;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class TestTool : ToolBase
    {
        public bool onToolUsedCalled = false;

        protected override void OnToolUsed()
        {
            onToolUsedCalled = true;
        }
    }

    public class ToolBaseTests
    {
        private TestTool _tool;
        private ToolManager _manager;
        private GameObject _toolGo;
        private GameObject _managerGo;

        [SetUp]
        public void Setup()
        {
            _managerGo = new GameObject("ToolManager");
            _manager = _managerGo.AddComponent<ToolManager>();

            _toolGo = new GameObject("TestTool");
            _tool = _toolGo.AddComponent<TestTool>();

            _tool.toolName = "TestTool";
            _tool.category = EToolCategory.PerceptionDisrupt;
            _tool.defaultSlot = EToolSlot.Slot1;
            _tool.maxAmmo = 3;
            _tool.cooldown = 2f;
            _tool.damage = 0;
            _tool.range = 5f;
            _tool.duration = 3f;
            _tool.ammo = 3;
            _tool.energyCost = 10f;

            _tool.Initialize(_manager);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_toolGo);
            Object.DestroyImmediate(_managerGo);
        }

        [Test]
        public void Initialize_SetsMaxAmmo()
        {
            Assert.AreEqual(3, _tool.CurrentAmmo);
        }

        [Test]
        public void Initialize_SetsCooldownToZero()
        {
            Assert.AreEqual(0f, _tool.CurrentCooldown);
        }

        [Test]
        public void Initialize_SetsIsOnCooldownFalse()
        {
            Assert.IsFalse(_tool.IsOnCooldown);
        }

        [Test]
        public void CanUse_NoCooldownNoAmmo_ReturnsTrue()
        {
            Assert.IsTrue(_tool.CanUse());
        }

        [Test]
        public void CanUse_OnCooldown_ReturnsFalse()
        {
            _tool.Use();
            Assert.IsFalse(_tool.CanUse());
        }

        [Test]
        public void CanUse_ZeroAmmo_ReturnsFalse()
        {
            _tool.toolName = "UnlimitedTool";
            _tool.ammo = 0;
            _tool.maxAmmo = 0;
            _tool.Initialize(_manager);

            Assert.IsTrue(_tool.CanUse());
        }

        [Test]
        public void Use_UnlimitedAmmo_DoesNotGoNegative()
        {
            _tool.toolName = "UnlimitedTool";
            _tool.ammo = 0;
            _tool.maxAmmo = 0;
            _tool.cooldown = 0f;
            _tool.Initialize(_manager);

            _tool.Use();
            _tool.Use();

            Assert.AreEqual(0, _tool.CurrentAmmo);
            Assert.IsTrue(_tool.CanUse());
            Assert.IsFalse(_tool.IsOnCooldown);
        }

        [Test]
        public void CanUse_OutOfAmmo_ReturnsFalse()
        {
            _tool.Use();
            _tool.Use();
            _tool.Use();

            Assert.IsFalse(_tool.CanUse());
        }

        [Test]
        public void Use_DecreasesAmmo()
        {
            _tool.Use();
            Assert.AreEqual(2, _tool.CurrentAmmo);
        }

        [Test]
        public void Use_SetsCooldown()
        {
            _tool.Use();
            Assert.AreEqual(2f, _tool.CurrentCooldown);
            Assert.IsTrue(_tool.IsOnCooldown);
        }

        [Test]
        public void Use_CallsOnToolUsed()
        {
            _tool.Use();
            Assert.IsTrue(_tool.onToolUsedCalled);
        }

        [Test]
        public void Update_CooldownDecreases()
        {
            _tool.Use();
            float initialCooldown = _tool.CurrentCooldown;

            _tool.Update();

            Assert.Less(_tool.CurrentCooldown, initialCooldown);
        }

        [Test]
        public void Update_CooldownEnds_SetsIsOnCooldownFalse()
        {
            _tool.Use();
            Assert.IsTrue(_tool.IsOnCooldown);

            for (int i = 0; i < 100; i++)
                _tool.Update();

            Assert.IsFalse(_tool.IsOnCooldown);
        }

        [Test]
        public void CooldownProgress_OnCooldown_ReturnsRatio()
        {
            _tool.Use();
            _tool.Update();

            float progress = _tool.CooldownProgress;
            Assert.Greater(progress, 0f);
            Assert.Less(progress, 1f);
        }

        [Test]
        public void CooldownProgress_NotOnCooldown_ReturnsOne()
        {
            Assert.AreEqual(1f, _tool.CooldownProgress);
        }

        [Test]
        public void Reload_AddsAmmo()
        {
            _tool.Use();
            _tool.ResetCooldown();
            _tool.Use();
            Assert.AreEqual(1, _tool.CurrentAmmo);

            _tool.Reload(2);

            Assert.AreEqual(3, _tool.CurrentAmmo);
        }

        [Test]
        public void Reload_DoesNotExceedMaxAmmo()
        {
            _tool.Reload(10);
            Assert.AreEqual(3, _tool.CurrentAmmo);
        }

        [Test]
        public void ResetCooldown_ClearsCooldown()
        {
            _tool.Use();
            Assert.IsTrue(_tool.IsOnCooldown);

            _tool.ResetCooldown();

            Assert.AreEqual(0f, _tool.CurrentCooldown);
            Assert.IsFalse(_tool.IsOnCooldown);
        }
    }
}
