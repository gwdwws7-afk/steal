using NUnit.Framework;
using INTIFALL.Tools;
using INTIFALL.System;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class ToolManagerTests
    {
        private ToolManager _manager;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("ToolManager");
            _manager = _go.AddComponent<ToolManager>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void MaxToolSlots_DefaultValue_Is4()
        {
            Assert.AreEqual(4, _manager.MaxToolSlots);
        }

        [Test]
        public void ActiveToolIndex_InitiallyNegative()
        {
            Assert.AreEqual(-1, _manager.ActiveToolIndex);
        }

        [Test]
        public void ActiveTool_InitiallyNull()
        {
            Assert.IsNull(_manager.ActiveTool);
        }

        [Test]
        public void EquippedTools_InitiallyEmpty()
        {
            Assert.AreEqual(4, _manager.EquippedTools.Length);
            for (int i = 0; i < _manager.EquippedTools.Length; i++)
            {
                Assert.IsNull(_manager.EquippedTools[i]);
            }
        }

        [Test]
        public void SelectTool_InvalidIndex_DoesNothing()
        {
            _manager.SelectTool(-1);
            Assert.AreEqual(-1, _manager.ActiveToolIndex);

            _manager.SelectTool(99);
            Assert.AreEqual(-1, _manager.ActiveToolIndex);
        }

        [Test]
        public void GetEquippedToolCount_InitiallyZero()
        {
            Assert.AreEqual(0, _manager.GetEquippedToolCount());
        }

        [Test]
        public void IsToolUnlocked_MissingTool_ReturnsFalse()
        {
            Assert.IsFalse(_manager.IsToolUnlocked("NonExistentTool"));
        }

        [Test]
        public void GetToolData_MissingTool_ReturnsNull()
        {
            Assert.IsNull(_manager.GetToolData("NonExistentTool"));
        }

        [Test]
        public void GetAvailableTools_InitiallyEmpty()
        {
            Assert.AreEqual(0, _manager.GetAvailableTools().Count);
        }

        [Test]
        public void GetToolsByCategory_InitiallyEmpty()
        {
            Assert.AreEqual(0, _manager.GetToolsByCategory(EToolCategory.PerceptionDisrupt).Count);
        }
    }
}