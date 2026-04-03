using UnityEngine;
using UnityEngine.UI;
using INTIFALL.System;
using INTIFALL.Tools;

namespace INTIFALL.UI
{
    public class ToolHUD : MonoBehaviour
    {
        [Header("Tool Slots")]
        [SerializeField] private Text[] slotTexts = new Text[4];
        [SerializeField] private Text[] slotAmmoTexts = new Text[4];
        [SerializeField] private Image[] slotImages = new Image[4];
        [SerializeField] private GameObject[] slotHighlights = new GameObject[4];

        [Header("Active Tool Display")]
        [SerializeField] private Text activeToolNameText;
        [SerializeField] private Text activeToolDescText;
        [SerializeField] private Image activeToolIcon;
        [SerializeField] private Image cooldownFill;
        [SerializeField] private Text loadoutCapacityText;
        [SerializeField] private float rejectionMessageHoldSeconds = 3f;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color equippedColor = Color.yellow;
        [SerializeField] private Color cooldownColor = Color.gray;
        [SerializeField] private Color emptyColor = Color.red;

        private ToolManager _toolManager;
        private string _temporaryFeedbackMessage = string.Empty;
        private float _temporaryFeedbackExpireAt = -1f;

        private void Start()
        {
            UpdateAllSlots();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ToolEquipRejectedEvent>(OnToolEquipRejected);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ToolEquipRejectedEvent>(OnToolEquipRejected);
        }

        private void Update()
        {
            UpdateCooldownDisplay();
            UpdateSlotHighlights();
            UpdateCapacityAndFeedback();
        }

        public void SetToolManager(ToolManager manager)
        {
            _toolManager = manager;
            UpdateAllSlots();
        }

        private void UpdateAllSlots()
        {
            if (_toolManager == null) return;

            for (int i = 0; i < 4; i++)
            {
                var tool = _toolManager.EquippedTools[i];
                if (tool != null)
                {
                    int slotCost = (_toolManager.EquippedToolSlotCosts != null && i < _toolManager.EquippedToolSlotCosts.Length)
                        ? Mathf.Max(1, _toolManager.EquippedToolSlotCosts[i])
                        : 1;
                    SetText(slotTexts, i, FormatSlotLabel(tool.toolName, slotCost));
                    SetTextColor(slotTexts, i, normalColor);

                    int ammo = tool.CurrentAmmo;
                    int maxAmmo = tool.maxAmmo;
                    SetText(slotAmmoTexts, i, $"{ammo}/{maxAmmo}");
                    SetTextColor(slotAmmoTexts, i, ammo > 0 ? normalColor : emptyColor);

                    SetImageColor(slotImages, i, normalColor);
                }
                else
                {
                    SetText(slotTexts, i, "-");
                    SetTextColor(slotTexts, i, cooldownColor);
                    SetText(slotAmmoTexts, i, string.Empty);
                    SetImageColor(slotImages, i, cooldownColor);
                }
            }

            UpdateCapacityAndFeedback();
        }

        private void UpdateCooldownDisplay()
        {
            if (_toolManager == null) return;

            var active = _toolManager.ActiveTool;
            if (active != null)
            {
                if (activeToolNameText != null)
                    activeToolNameText.text = active.toolName;

                if (active.IsOnCooldown)
                {
                    float progress = active.CooldownProgress;
                    if (cooldownFill != null)
                    {
                        cooldownFill.fillAmount = 1f - progress;
                        cooldownFill.color = cooldownColor;
                    }
                }
                else
                {
                    if (cooldownFill != null)
                    {
                        cooldownFill.fillAmount = 0f;
                        cooldownFill.color = equippedColor;
                    }
                }
            }
            else
            {
                if (activeToolNameText != null)
                {
                    activeToolNameText.text = LocalizationService.Get(
                        "toolhud.no_tool",
                        fallbackEnglish: "No Tool",
                        fallbackChinese: string.Empty);
                }

                if (cooldownFill != null)
                    cooldownFill.fillAmount = 0f;
            }
        }

        private void UpdateSlotHighlights()
        {
            if (_toolManager == null) return;

            int activeIndex = _toolManager.ActiveToolIndex;
            for (int i = 0; i < 4; i++)
            {
                if (slotHighlights != null && i < slotHighlights.Length && slotHighlights[i] != null)
                    slotHighlights[i].SetActive(i == activeIndex);
            }
        }

        private void UpdateCapacityAndFeedback()
        {
            if (_toolManager == null)
                return;

            int used = _toolManager.GetTotalEquippedSlotCost();
            int max = _toolManager.MaxToolSlots;
            if (loadoutCapacityText != null)
                loadoutCapacityText.text = FormatCapacityLabel(used, max);

            if (activeToolDescText == null)
                return;

            string feedbackMessage = ResolveFeedbackMessage(used, max);
            activeToolDescText.text = feedbackMessage;
        }

        private string ResolveFeedbackMessage(int used, int max)
        {
            if (_temporaryFeedbackExpireAt >= 0f && Time.unscaledTime <= _temporaryFeedbackExpireAt && !string.IsNullOrWhiteSpace(_temporaryFeedbackMessage))
                return _temporaryFeedbackMessage;

            _temporaryFeedbackExpireAt = -1f;
            _temporaryFeedbackMessage = string.Empty;
            return FormatCapacityLabel(used, max);
        }

        private void OnToolEquipRejected(ToolEquipRejectedEvent evt)
        {
            _temporaryFeedbackMessage = FormatEquipRejectedMessage(evt);
            _temporaryFeedbackExpireAt = Time.unscaledTime + Mathf.Max(0.5f, rejectionMessageHoldSeconds);
            UpdateCapacityAndFeedback();
        }

        private static string FormatSlotLabel(string toolName, int slotCost)
        {
            string safeToolName = string.IsNullOrWhiteSpace(toolName) ? "Tool" : toolName.Trim();
            int normalizedCost = Mathf.Max(1, slotCost);
            return normalizedCost <= 1
                ? safeToolName
                : $"{safeToolName} [{normalizedCost}]";
        }

        private static string FormatCapacityLabel(int used, int max)
        {
            int normalizedMax = Mathf.Max(1, max);
            int normalizedUsed = Mathf.Clamp(used, 0, normalizedMax);
            int remaining = Mathf.Max(0, normalizedMax - normalizedUsed);
            return $"Loadout {normalizedUsed}/{normalizedMax}  Remaining {remaining}";
        }

        private static string FormatEquipRejectedMessage(ToolEquipRejectedEvent evt)
        {
            string toolName = string.IsNullOrWhiteSpace(evt.toolName) ? "Tool" : evt.toolName.Trim();
            int slotCost = Mathf.Max(1, evt.slotCost);
            int remaining = Mathf.Max(0, evt.remainingCapacity);
            return $"Cannot equip {toolName}: need {slotCost}, have {remaining} free.";
        }

        private static void SetText(Text[] array, int index, string value)
        {
            if (array == null || index < 0 || index >= array.Length || array[index] == null)
                return;
            array[index].text = value;
        }

        private static void SetTextColor(Text[] array, int index, Color value)
        {
            if (array == null || index < 0 || index >= array.Length || array[index] == null)
                return;
            array[index].color = value;
        }

        private static void SetImageColor(Image[] array, int index, Color value)
        {
            if (array == null || index < 0 || index >= array.Length || array[index] == null)
                return;
            array[index].color = value;
        }

        public void RefreshHUD()
        {
            UpdateAllSlots();
        }
    }
}
