using UnityEngine;
using INTIFALL.Input;
using System.Collections.Generic;
using INTIFALL.Environment;
using INTIFALL.System;

namespace INTIFALL.Tools
{
    public class ToolManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int maxToolSlots = 4;
        [SerializeField] private List<ToolData> availableTools = new();
        [SerializeField] private float wheelSwitchDeadZone = 0.1f;
        [SerializeField] private float cancelToolUseHoldSeconds = 0.35f;

        [Header("Current Loadout")]
        [SerializeField] private ToolBase[] equippedTools = new ToolBase[4];
        [SerializeField] private int[] equippedToolSlotCosts = new int[4];
        [SerializeField] private int activeToolIndex = -1;

        [Header("References")]
        [SerializeField] private Transform toolAnchor;

        private Dictionary<string, ToolData> _toolDatabase;
        private HashSet<string> _unlockedTools;
        private ToolBase _activeToolInstance;
        private bool _isInsideVent;
        private bool _toolUseButtonHeld;
        private float _toolUseButtonPressedAtUnscaledTime;

        public int MaxToolSlots => maxToolSlots;
        public ToolBase[] EquippedTools => equippedTools;
        public int[] EquippedToolSlotCosts => equippedToolSlotCosts;
        public int ActiveToolIndex => activeToolIndex;
        public ToolBase ActiveTool => _activeToolInstance;
        public int TotalEquippedSlotCost => GetTotalEquippedSlotCost();
        public int RemainingSlotCapacity => Mathf.Max(0, maxToolSlots - GetTotalEquippedSlotCost());

        private void EnsureInitialized()
        {
            if (_toolDatabase == null || _unlockedTools == null)
            {
                _toolDatabase = new Dictionary<string, ToolData>();
                _unlockedTools = new HashSet<string>();
                foreach (var tool in availableTools)
                {
                    if (tool != null)
                    {
                        _toolDatabase[tool.toolName] = tool;
                        if (tool.unlockedByDefault)
                            _unlockedTools.Add(tool.toolName);
                    }
                }
            }

            EnsureLoadoutArrays();
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<VentEntrance.VentEnteredEvent>(OnVentEntered);
            EventBus.Subscribe<VentEntrance.VentExitedEvent>(OnVentExited);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<VentEntrance.VentEnteredEvent>(OnVentEntered);
            EventBus.Unsubscribe<VentEntrance.VentExitedEvent>(OnVentExited);
            _toolUseButtonHeld = false;
        }

        private void Update()
        {
            HandleToolInput();
        }

        private void HandleToolInput()
        {
            if (InputCompat.GetKeyDown(KeyCode.Alpha1)) SelectTool(0);
            else if (InputCompat.GetKeyDown(KeyCode.Alpha2)) SelectTool(1);
            else if (InputCompat.GetKeyDown(KeyCode.Alpha3)) SelectTool(2);
            else if (InputCompat.GetKeyDown(KeyCode.Alpha4)) SelectTool(3);

            float wheelDelta = InputCompat.GetAxis("Mouse ScrollWheel");
            if (wheelDelta > wheelSwitchDeadZone)
                SelectNextTool();
            else if (wheelDelta < -wheelSwitchDeadZone)
                SelectPreviousTool();

            if (InputCompat.GetKeyDown(KeyCode.Escape))
                CancelActiveToolSelection();

            if (InputCompat.GetKeyDown(KeyCode.Mouse1))
            {
                _toolUseButtonHeld = true;
                _toolUseButtonPressedAtUnscaledTime = Time.unscaledTime;
            }

            if (_toolUseButtonHeld && InputCompat.GetKeyUp(KeyCode.Mouse1))
            {
                _toolUseButtonHeld = false;
                float heldSeconds = Time.unscaledTime - _toolUseButtonPressedAtUnscaledTime;
                if (heldSeconds >= cancelToolUseHoldSeconds)
                {
                    CancelActiveToolSelection();
                    return;
                }

                if (!_isInsideVent)
                    UseActiveTool();
            }
        }

        public void SelectTool(int slotIndex)
        {
            EnsureInitialized();
            if (slotIndex < 0 || slotIndex >= equippedTools.Length) return;
            if (equippedTools[slotIndex] == null) return;

            activeToolIndex = slotIndex;
            _activeToolInstance = equippedTools[slotIndex];

            EventBus.Publish(new ToolEquippedEvent
            {
                toolName = _activeToolInstance.toolName,
                slot = _activeToolInstance.defaultSlot
            });
        }

        public bool SelectNextTool()
        {
            EnsureInitialized();
            return SelectAdjacentEquippedTool(1);
        }

        public bool SelectPreviousTool()
        {
            EnsureInitialized();
            return SelectAdjacentEquippedTool(-1);
        }

        public bool CancelActiveToolSelection()
        {
            EnsureInitialized();
            if (activeToolIndex < 0 || _activeToolInstance == null)
                return false;

            activeToolIndex = -1;
            _activeToolInstance = null;
            return true;
        }

        public void EquipTool(int slotIndex, ToolData toolData)
        {
            EnsureInitialized();
            if (slotIndex < 0 || slotIndex >= equippedTools.Length) return;
            if (toolData == null) return;
            if (!CanEquipTool(toolData, slotIndex))
            {
                int slotCost = ResolveSlotCost(toolData);
                int replacingCost = equippedTools[slotIndex] != null
                    ? Mathf.Max(1, equippedToolSlotCosts[slotIndex])
                    : 0;
                int remainingCapacity = Mathf.Max(0, maxToolSlots - (GetTotalEquippedSlotCost() - replacingCost));
                Debug.LogWarning($"ToolManager: cannot equip '{toolData.toolName}' to slot {slotIndex}. Cost {slotCost} exceeds remaining capacity.");
                EventBus.Publish(new ToolEquipRejectedEvent
                {
                    toolName = string.IsNullOrWhiteSpace(toolData.toolName) ? "UnknownTool" : toolData.toolName,
                    requestedSlotIndex = slotIndex,
                    slotCost = slotCost,
                    remainingCapacity = remainingCapacity,
                    maxCapacity = maxToolSlots,
                    reason = "slot_capacity_exceeded"
                });
                return;
            }

            if (equippedTools[slotIndex] != null)
            {
                Destroy(equippedTools[slotIndex].gameObject);
                equippedTools[slotIndex] = null;
                equippedToolSlotCosts[slotIndex] = 0;
            }

            ToolBase toolInstance = CreateToolInstance(toolData);
            if (toolInstance != null)
            {
                toolInstance.transform.SetParent(toolAnchor);
                toolInstance.Initialize(this);
                equippedTools[slotIndex] = toolInstance;
                equippedToolSlotCosts[slotIndex] = ResolveSlotCost(toolData);

                if (activeToolIndex == -1 || activeToolIndex == slotIndex)
                    SelectTool(slotIndex);
            }
        }

        public void UnequipTool(int slotIndex)
        {
            EnsureInitialized();
            if (slotIndex < 0 || slotIndex >= equippedTools.Length) return;

            if (equippedTools[slotIndex] != null)
            {
                Destroy(equippedTools[slotIndex].gameObject);
                equippedTools[slotIndex] = null;
            }

            equippedToolSlotCosts[slotIndex] = 0;

            if (activeToolIndex == slotIndex)
            {
                activeToolIndex = -1;
                _activeToolInstance = null;
            }
        }

        public void UseActiveTool()
        {
            EnsureInitialized();
            if (_isInsideVent) return;
            if (_activeToolInstance == null) return;
            if (!_activeToolInstance.CanUse()) return;

            _activeToolInstance.Use();
        }

        public void UseTool(int slotIndex)
        {
            EnsureInitialized();
            if (_isInsideVent) return;
            if (slotIndex < 0 || slotIndex >= equippedTools.Length) return;
            if (equippedTools[slotIndex] == null) return;
            if (!equippedTools[slotIndex].CanUse()) return;

            equippedTools[slotIndex].Use();
        }

        public bool IsToolUnlocked(string toolName)
        {
            EnsureInitialized();
            return _unlockedTools.Contains(toolName);
        }

        public void UnlockTool(string toolName)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(toolName)) return;
            _unlockedTools.Add(toolName);
        }

        public string[] GetUnlockedToolNames()
        {
            EnsureInitialized();
            string[] result = new string[_unlockedTools.Count];
            _unlockedTools.CopyTo(result);
            return result;
        }

        public ToolData GetToolData(string toolName)
        {
            EnsureInitialized();
            _toolDatabase.TryGetValue(toolName, out var data);
            return data;
        }

        public List<ToolData> GetAvailableTools()
        {
            EnsureInitialized();
            return new List<ToolData>(_toolDatabase.Values);
        }

        public List<ToolData> GetToolsByCategory(EToolCategory category)
        {
            EnsureInitialized();
            List<ToolData> result = new();
            foreach (var tool in _toolDatabase.Values)
            {
                if (tool.category == category)
                    result.Add(tool);
            }
            return result;
        }

        public int GetTotalEquippedSlotCost()
        {
            EnsureInitialized();
            int total = 0;
            for (int i = 0; i < equippedTools.Length; i++)
            {
                if (equippedTools[i] != null)
                    total += Mathf.Max(1, equippedToolSlotCosts[i]);
            }

            return total;
        }

        public int GetRemainingSlotCapacity()
        {
            EnsureInitialized();
            return Mathf.Max(0, maxToolSlots - GetTotalEquippedSlotCost());
        }

        public bool CanEquipTool(ToolData toolData, int replacingSlotIndex = -1)
        {
            EnsureInitialized();
            if (toolData == null)
                return false;

            if (replacingSlotIndex >= equippedTools.Length)
                return false;

            int replacingCost = 0;
            if (replacingSlotIndex >= 0 && replacingSlotIndex < equippedTools.Length && equippedTools[replacingSlotIndex] != null)
                replacingCost = Mathf.Max(1, equippedToolSlotCosts[replacingSlotIndex]);

            int projectedTotal = GetTotalEquippedSlotCost() - replacingCost + ResolveSlotCost(toolData);
            return projectedTotal <= maxToolSlots;
        }

        private void EnsureLoadoutArrays()
        {
            maxToolSlots = Mathf.Max(1, maxToolSlots);

            if (equippedTools == null || equippedTools.Length != maxToolSlots)
            {
                ToolBase[] resized = new ToolBase[maxToolSlots];
                if (equippedTools != null)
                {
                    int copyLength = Mathf.Min(equippedTools.Length, resized.Length);
                    for (int i = 0; i < copyLength; i++)
                        resized[i] = equippedTools[i];
                }
                equippedTools = resized;
            }

            if (equippedToolSlotCosts == null || equippedToolSlotCosts.Length != maxToolSlots)
            {
                int[] resizedCosts = new int[maxToolSlots];
                if (equippedToolSlotCosts != null)
                {
                    int copyLength = Mathf.Min(equippedToolSlotCosts.Length, resizedCosts.Length);
                    for (int i = 0; i < copyLength; i++)
                        resizedCosts[i] = equippedToolSlotCosts[i];
                }
                equippedToolSlotCosts = resizedCosts;
            }

            for (int i = 0; i < maxToolSlots; i++)
            {
                if (equippedTools[i] == null)
                {
                    equippedToolSlotCosts[i] = 0;
                    continue;
                }

                if (equippedToolSlotCosts[i] < 1)
                    equippedToolSlotCosts[i] = 1;
            }
        }

        private bool SelectAdjacentEquippedTool(int direction)
        {
            if (equippedTools == null || equippedTools.Length == 0)
                return false;

            int slotCount = equippedTools.Length;
            int current = activeToolIndex;
            if (current < 0 || current >= slotCount)
                current = direction > 0 ? -1 : 0;

            for (int i = 0; i < slotCount; i++)
            {
                current = (current + direction + slotCount) % slotCount;
                if (equippedTools[current] == null)
                    continue;

                SelectTool(current);
                return true;
            }

            return false;
        }

        private static int ResolveSlotCost(ToolData toolData)
        {
            if (toolData == null)
                return 0;

            return Mathf.Max(1, toolData.slotCost);
        }

        private ToolBase CreateToolInstance(ToolData data)
        {
            GameObject prefab = data.runtimePrefab;
            if (prefab == null)
                prefab = Resources.Load<GameObject>($"Prefabs/Tools/{data.toolName}");

            if (prefab == null)
            {
                Debug.LogWarning($"ToolManager: missing prefab for tool '{data.toolName}'.");
                return null;
            }

            ToolBase tool = Instantiate(prefab).GetComponent<ToolBase>();
            if (tool == null)
            {
                Debug.LogWarning($"ToolManager: prefab '{prefab.name}' has no ToolBase component.");
                return null;
            }

            ApplyRuntimeOverrides(tool, data);
            return tool;
        }

        private static void ApplyRuntimeOverrides(ToolBase tool, ToolData data)
        {
            if (tool == null || data == null)
                return;

            tool.ApplyToolData(data);
        }

        public int GetEquippedToolCount()
        {
            EnsureInitialized();
            int count = 0;
            for (int i = 0; i < equippedTools.Length; i++)
            {
                if (equippedTools[i] != null)
                    count++;
            }
            return count;
        }

        public void SetToolAnchor(Transform anchor)
        {
            toolAnchor = anchor;
        }

        private void OnVentEntered(VentEntrance.VentEnteredEvent evt)
        {
            _isInsideVent = true;
        }

        private void OnVentExited(VentEntrance.VentExitedEvent evt)
        {
            _isInsideVent = false;
        }
    }
}

