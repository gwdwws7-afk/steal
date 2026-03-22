using UnityEngine;
using System.Collections.Generic;
using INTIFALL.System;

namespace INTIFALL.Tools
{
    public class ToolManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int maxToolSlots = 4;
        [SerializeField] private List<ToolData> availableTools = new();

        [Header("Current Loadout")]
        [SerializeField] private ToolBase[] equippedTools = new ToolBase[4];
        [SerializeField] private int activeToolIndex = -1;

        [Header("References")]
        [SerializeField] private Transform toolAnchor;

        private Dictionary<string, ToolData> _toolDatabase;
        private ToolBase _activeToolInstance;

        public int MaxToolSlots => maxToolSlots;
        public ToolBase[] EquippedTools => equippedTools;
        public int ActiveToolIndex => activeToolIndex;
        public ToolBase ActiveTool => _activeToolInstance;

        private void Awake()
        {
            _toolDatabase = new Dictionary<string, ToolData>();
            foreach (var tool in availableTools)
            {
                if (tool != null)
                    _toolDatabase[tool.toolName] = tool;
            }
        }

        private void Update()
        {
            UpdateToolCooldowns();
            HandleToolInput();
        }

        private void UpdateToolCooldowns()
        {
            for (int i = 0; i < equippedTools.Length; i++)
            {
                if (equippedTools[i] != null)
                    equippedTools[i].Update();
            }
        }

        private void HandleToolInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectTool(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectTool(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectTool(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) SelectTool(3);

            if (Input.GetKeyDown(KeyCode.Q))
            {
                UseActiveTool();
            }
        }

        public void SelectTool(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxToolSlots) return;
            if (equippedTools[slotIndex] == null) return;

            activeToolIndex = slotIndex;
            _activeToolInstance = equippedTools[slotIndex];

            EventBus.Publish(new ToolEquippedEvent
            {
                toolName = _activeToolInstance.toolName,
                slot = _activeToolInstance.defaultSlot
            });
        }

        public void EquipTool(int slotIndex, ToolData toolData)
        {
            if (slotIndex < 0 || slotIndex >= maxToolSlots) return;
            if (toolData == null) return;

            if (equippedTools[slotIndex] != null)
            {
                Destroy(equippedTools[slotIndex].gameObject);
            }

            ToolBase toolInstance = CreateToolInstance(toolData);
            if (toolInstance != null)
            {
                toolInstance.transform.SetParent(toolAnchor);
                toolInstance.Initialize(this);
                equippedTools[slotIndex] = toolInstance;

                if (activeToolIndex == -1)
                    SelectTool(slotIndex);
            }
        }

        public void UnequipTool(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxToolSlots) return;

            if (equippedTools[slotIndex] != null)
            {
                Destroy(equippedTools[slotIndex].gameObject);
                equippedTools[slotIndex] = null;
            }

            if (activeToolIndex == slotIndex)
            {
                activeToolIndex = -1;
                _activeToolInstance = null;
            }
        }

        public void UseActiveTool()
        {
            if (_activeToolInstance == null) return;
            if (!_activeToolInstance.CanUse()) return;

            _activeToolInstance.Use();
        }

        public void UseTool(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxToolSlots) return;
            if (equippedTools[slotIndex] == null) return;
            if (!equippedTools[slotIndex].CanUse()) return;

            equippedTools[slotIndex].Use();
        }

        public bool IsToolUnlocked(string toolName)
        {
            if (_toolDatabase.TryGetValue(toolName, out var data))
                return data.unlockedByDefault;
            return false;
        }

        public ToolData GetToolData(string toolName)
        {
            _toolDatabase.TryGetValue(toolName, out var data);
            return data;
        }

        public List<ToolData> GetAvailableTools()
        {
            return new List<ToolData>(_toolDatabase.Values);
        }

        public List<ToolData> GetToolsByCategory(EToolCategory category)
        {
            List<ToolData> result = new();
            foreach (var tool in _toolDatabase.Values)
            {
                if (tool.category == category)
                    result.Add(tool);
            }
            return result;
        }

        private ToolBase CreateToolInstance(ToolData data)
        {
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/Tools/{data.toolName}");
            if (prefab != null)
            {
                return Instantiate(prefab).GetComponent<ToolBase>();
            }

            return null;
        }

        public int GetEquippedToolCount()
        {
            int count = 0;
            for (int i = 0; i < equippedTools.Length; i++)
            {
                if (equippedTools[i] != null)
                    count++;
            }
            return count;
        }
    }
}
