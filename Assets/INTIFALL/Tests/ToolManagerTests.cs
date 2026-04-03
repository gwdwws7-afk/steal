using System.Collections.Generic;
using INTIFALL.Tools;
using INTIFALL.System;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class ToolManagerTests
    {
        private ToolManager _manager;
        private GameObject _go;
        private readonly List<Object> _cleanupObjects = new();

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("ToolManager");
            _manager = _go.AddComponent<ToolManager>();
            _manager.SetToolAnchor(_go.transform);
        }

        [TearDown]
        public void Teardown()
        {
            for (int i = 0; i < _cleanupObjects.Count; i++)
            {
                if (_cleanupObjects[i] != null)
                    Object.DestroyImmediate(_cleanupObjects[i]);
            }

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
                Assert.AreEqual(0, _manager.EquippedToolSlotCosts[i]);
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
            Assert.AreEqual(0, _manager.GetTotalEquippedSlotCost());
            Assert.AreEqual(4, _manager.GetRemainingSlotCapacity());
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

        [Test]
        public void EquipTool_RejectsWhenSlotCostExceedsCapacity()
        {
            ToolData costTwoA = CreateToolData("CostTwoA", slotCost: 2);
            ToolData costTwoB = CreateToolData("CostTwoB", slotCost: 2);
            ToolData costOneC = CreateToolData("CostOneC", slotCost: 1);

            _manager.EquipTool(0, costTwoA);
            _manager.EquipTool(1, costTwoB);

            Assert.AreEqual(4, _manager.GetTotalEquippedSlotCost());
            Assert.AreEqual(0, _manager.GetRemainingSlotCapacity());

            _manager.EquipTool(2, costOneC);

            Assert.IsNull(_manager.EquippedTools[2], "Equip should be rejected when projected cost exceeds capacity.");
            Assert.AreEqual(4, _manager.GetTotalEquippedSlotCost());
        }

        [Test]
        public void EquipTool_RejectPath_PublishesToolEquipRejectedEvent()
        {
            ToolData costTwoA = CreateToolData("RejectA", slotCost: 2);
            ToolData costTwoB = CreateToolData("RejectB", slotCost: 2);
            ToolData costOneC = CreateToolData("RejectC", slotCost: 1);
            ToolEquipRejectedEvent received = default;
            bool receivedEvent = false;

            global::System.Action<ToolEquipRejectedEvent> handler = evt =>
            {
                received = evt;
                receivedEvent = true;
            };
            EventBus.Subscribe(handler);

            try
            {
                _manager.EquipTool(0, costTwoA);
                _manager.EquipTool(1, costTwoB);
                _manager.EquipTool(2, costOneC);
            }
            finally
            {
                EventBus.Unsubscribe(handler);
            }

            Assert.IsTrue(receivedEvent, "Expected ToolEquipRejectedEvent when capacity is exceeded.");
            Assert.AreEqual("RejectC", received.toolName);
            Assert.AreEqual(2, received.requestedSlotIndex);
            Assert.AreEqual(1, received.slotCost);
            Assert.AreEqual(0, received.remainingCapacity);
            Assert.AreEqual(4, received.maxCapacity);
            Assert.AreEqual("slot_capacity_exceeded", received.reason);
        }

        [Test]
        public void CanEquipTool_WhenReplacingSlot_UsesReplacementCost()
        {
            ToolData costTwoA = CreateToolData("ReplaceCostA", slotCost: 2);
            ToolData costTwoB = CreateToolData("ReplaceCostB", slotCost: 2);
            ToolData costTwoC = CreateToolData("ReplaceCostC", slotCost: 2);

            _manager.EquipTool(0, costTwoA);
            _manager.EquipTool(1, costTwoB);
            Assert.AreEqual(4, _manager.GetTotalEquippedSlotCost());

            Assert.IsFalse(_manager.CanEquipTool(costTwoC), "Cannot add a new cost-2 tool when capacity is full.");
            Assert.IsTrue(_manager.CanEquipTool(costTwoC, replacingSlotIndex: 0), "Replacing a cost-2 slot should keep projected total legal.");
        }

        [Test]
        public void ResolveSlotCost_ZeroOrNegativeDataDefaultsToOne()
        {
            ToolData zeroCostTool = CreateToolData("ZeroCostData", slotCost: 0);
            _manager.EquipTool(0, zeroCostTool);

            Assert.IsNotNull(_manager.EquippedTools[0]);
            Assert.AreEqual(1, _manager.EquippedToolSlotCosts[0]);
            Assert.AreEqual(1, _manager.GetTotalEquippedSlotCost());
        }

        [Test]
        public void SelectNextAndPreviousTool_SkipsEmptySlotsAndWraps()
        {
            ToolData toolA = CreateToolData("CycleA", slotCost: 1);
            ToolData toolB = CreateToolData("CycleB", slotCost: 1);
            _manager.EquipTool(0, toolA);
            _manager.EquipTool(2, toolB);

            _manager.SelectTool(0);
            Assert.AreEqual(0, _manager.ActiveToolIndex);

            Assert.IsTrue(_manager.SelectNextTool());
            Assert.AreEqual(2, _manager.ActiveToolIndex);

            Assert.IsTrue(_manager.SelectNextTool());
            Assert.AreEqual(0, _manager.ActiveToolIndex);

            Assert.IsTrue(_manager.SelectPreviousTool());
            Assert.AreEqual(2, _manager.ActiveToolIndex);
        }

        [Test]
        public void CancelActiveToolSelection_ClearsActiveState()
        {
            ToolData toolA = CreateToolData("CancelableTool", slotCost: 1);
            _manager.EquipTool(0, toolA);
            _manager.SelectTool(0);

            Assert.IsTrue(_manager.CancelActiveToolSelection());
            Assert.AreEqual(-1, _manager.ActiveToolIndex);
            Assert.IsNull(_manager.ActiveTool);
            Assert.IsFalse(_manager.CancelActiveToolSelection());
        }

        private ToolData CreateToolData(string name, int slotCost)
        {
            GameObject prefab = new GameObject(name + "_Prefab");
            ToolManagerMockTool tool = prefab.AddComponent<ToolManagerMockTool>();
            tool.toolName = name;
            tool.defaultSlot = EToolSlot.Slot1;
            tool.category = EToolCategory.AttentionShift;
            tool.maxAmmo = 1;
            tool.ammo = 1;
            tool.cooldown = 0f;
            _cleanupObjects.Add(prefab);

            ToolData data = ScriptableObject.CreateInstance<ToolData>();
            data.toolName = name;
            data.toolNameCN = name;
            data.category = EToolCategory.AttentionShift;
            data.defaultSlot = EToolSlot.Slot1;
            data.slotCost = slotCost;
            data.maxAmmo = 1;
            data.runtimePrefab = prefab;
            _cleanupObjects.Add(data);
            return data;
        }
    }

    public class ToolManagerMockTool : ToolBase
    {
        protected override void OnToolUsed()
        {
        }
    }
}
