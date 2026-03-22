using UnityEngine;
using UnityEngine.UI;
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

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color equippedColor = Color.yellow;
        [SerializeField] private Color cooldownColor = Color.gray;
        [SerializeField] private Color emptyColor = Color.red;

        private ToolManager _toolManager;

        private void Start()
        {
            UpdateAllSlots();
        }

        private void Update()
        {
            UpdateCooldownDisplay();
            UpdateSlotHighlights();
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
                    slotTexts[i].text = tool.toolName;
                    slotTexts[i].color = normalColor;

                    int ammo = tool.CurrentAmmo;
                    int maxAmmo = tool.maxAmmo;
                    slotAmmoTexts[i].text = $"{ammo}/{maxAmmo}";
                    slotAmmoTexts[i].color = ammo > 0 ? normalColor : emptyColor;

                    slotImages[i].color = normalColor;
                }
                else
                {
                    slotTexts[i].text = "-";
                    slotTexts[i].color = cooldownColor;
                    slotAmmoTexts[i].text = "";
                    slotImages[i].color = cooldownColor;
                }
            }
        }

        private void UpdateCooldownDisplay()
        {
            if (_toolManager == null) return;

            var active = _toolManager.ActiveTool;
            if (active != null)
            {
                activeToolNameText.text = active.toolName;

                if (active.IsOnCooldown)
                {
                    float progress = active.CooldownProgress;
                    cooldownFill.fillAmount = 1f - progress;
                    cooldownFill.color = cooldownColor;
                }
                else
                {
                    cooldownFill.fillAmount = 0f;
                    cooldownFill.color = equippedColor;
                }
            }
            else
            {
                activeToolNameText.text = "No Tool";
                cooldownFill.fillAmount = 0f;
            }
        }

        private void UpdateSlotHighlights()
        {
            if (_toolManager == null) return;

            int activeIndex = _toolManager.ActiveToolIndex;
            for (int i = 0; i < 4; i++)
            {
                slotHighlights[i].SetActive(i == activeIndex);
            }
        }

        public void RefreshHUD()
        {
            UpdateAllSlots();
        }
    }
}