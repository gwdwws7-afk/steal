using UnityEngine;
using UnityEngine.UI;
using INTIFALL.Tools;
using INTIFALL.System;

namespace INTIFALL.Economy
{
    public enum EPurchaseResult
    {
        Success,
        InsufficientCredits,
        AlreadyOwned,
        Locked,
        Failed
    }

    public struct ToolPurchasedEvent
    {
        public string toolName;
        public int cost;
    }

    public struct ToolUpgradedEvent
    {
        public string toolName;
        public int cost;
    }

    public class ArsenalUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CreditSystem creditSystem;
        [SerializeField] private ToolManager toolManager;

        [Header("UI Elements")]
        [SerializeField] private Text creditDisplay;
        [SerializeField] private GameObject arsenalPanel;
        [SerializeField] private Button closeButton;

        [Header("Tool Button Prefabs")]
        [SerializeField] private GameObject toolButtonPrefab;
        [SerializeField] private Transform toolListContent;

        [Header("Tool Database")]
        [SerializeField] private ToolData[] availableTools;

        private bool _isOpen;

        public bool IsOpen => _isOpen;

        private void Awake()
        {
            if (creditSystem == null)
                creditSystem = FindObjectOfType<CreditSystem>();
            if (toolManager == null)
                toolManager = FindObjectOfType<ToolManager>();
        }

        private void Start()
        {
            if (arsenalPanel != null)
                arsenalPanel.SetActive(false);

            if (closeButton != null)
                closeButton.onClick.AddListener(CloseArsenal);
        }

        private void Update()
        {
            UpdateCreditDisplay();

            if (Input.GetKeyDown(KeyCode.U) || Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleArsenal();
            }
        }

        private void UpdateCreditDisplay()
        {
            if (creditDisplay != null && creditSystem != null)
            {
                creditDisplay.text = $"{creditSystem.CurrentCredits} Credit";
            }
        }

        public void OpenArsenal()
        {
            if (_isOpen) return;
            _isOpen = true;

            if (arsenalPanel != null)
                arsenalPanel.SetActive(true);

            RefreshToolList();
            Time.timeScale = 0f;
        }

        public void CloseArsenal()
        {
            if (!_isOpen) return;
            _isOpen = false;

            if (arsenalPanel != null)
                arsenalPanel.SetActive(false);

            Time.timeScale = 1f;
        }

        public void ToggleArsenal()
        {
            if (_isOpen)
                CloseArsenal();
            else
                OpenArsenal();
        }

        private void RefreshToolList()
        {
            if (toolListContent == null) return;
            if (toolButtonPrefab == null) return;

            foreach (Transform child in toolListContent)
            {
                Destroy(child.gameObject);
            }

            if (availableTools == null) return;

            foreach (var toolData in availableTools)
            {
                if (toolData == null) continue;
                CreateToolButton(toolData);
            }
        }

        private void CreateToolButton(ToolData toolData)
        {
            GameObject buttonObj = Instantiate(toolButtonPrefab, toolListContent);

            var toolButton = buttonObj.GetComponent<ToolPurchaseButton>();
            if (toolButton != null)
            {
                toolButton.Initialize(toolData, this);
            }
            else
            {
                Text[] texts = buttonObj.GetComponentsInChildren<Text>();
                if (texts.Length > 0)
                    texts[0].text = toolData.toolName;
            }
        }

        public EPurchaseResult TryPurchaseTool(ToolData toolData)
        {
            if (toolData == null) return EPurchaseResult.Failed;
            if (creditSystem == null) return EPurchaseResult.Failed;

            if (toolManager != null && toolManager.IsToolUnlocked(toolData.toolName))
            {
                return EPurchaseResult.AlreadyOwned;
            }

            if (!CanAffordTool(toolData))
            {
                return EPurchaseResult.InsufficientCredits;
            }

            if (!IsToolAvailable(toolData))
            {
                return EPurchaseResult.Locked;
            }

            int price = GetToolPrice(toolData);
            bool success = creditSystem.SpendCredits(price, "Purchase", toolData.toolName);

            if (success)
            {
                EventBus.Publish(new ToolPurchasedEvent
                {
                    toolName = toolData.toolName,
                    cost = price
                });

                return EPurchaseResult.Success;
            }

            return EPurchaseResult.Failed;
        }

        public EPurchaseResult TryUpgradeTool(ToolData toolData)
        {
            if (toolData == null) return EPurchaseResult.Failed;
            if (toolData.upgradedVersion == null) return EPurchaseResult.Failed;
            if (creditSystem == null) return EPurchaseResult.Failed;

            int upgradePrice = toolData.upgradePrice;
            if (!creditSystem.CanAfford(upgradePrice))
            {
                return EPurchaseResult.InsufficientCredits;
            }

            bool success = creditSystem.SpendCredits(upgradePrice, "Upgrade", toolData.toolName);

            if (success)
            {
                EventBus.Publish(new ToolUpgradedEvent
                {
                    toolName = toolData.toolName,
                    cost = upgradePrice
                });

                return EPurchaseResult.Success;
            }

            return EPurchaseResult.Failed;
        }

        public bool CanAffordTool(ToolData toolData)
        {
            if (toolData == null) return false;
            if (creditSystem == null) return false;

            int price = GetToolPrice(toolData);
            return creditSystem.CanAfford(price);
        }

        public int GetToolPrice(ToolData toolData)
        {
            if (toolData == null) return 0;

            if (toolManager != null && toolManager.IsToolUnlocked(toolData.toolName))
            {
                return toolData.upgradePrice;
            }

            return toolData.unlockPrice;
        }

        public bool IsToolAvailable(ToolData toolData)
        {
            if (toolData == null) return false;
            return toolData.unlockedByDefault || toolData.unlockLevel <= GetCurrentProgressionLevel();
        }

        private int GetCurrentProgressionLevel()
        {
            return GameManager.Instance?.CurrentLevel ?? 1;
        }
    }

    public class ToolPurchaseButton : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text priceText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button purchaseButton;

        private ToolData _toolData;
        private ArsenalUI _arsenalUI;

        public void Initialize(ToolData data, ArsenalUI arsenal)
        {
            _toolData = data;
            _arsenalUI = arsenal;

            if (nameText != null)
                nameText.text = data.toolName;

            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            if (_toolData == null || _arsenalUI == null) return;

            bool owned = _arsenalUI.TryGetComponent<ToolManager>()?.IsToolUnlocked(_toolData.toolName) ?? false;
            bool available = _arsenalUI.IsToolAvailable(_toolData);
            bool canAfford = _arsenalUI.CanAffordTool(_toolData);

            if (priceText != null)
            {
                int price = _arsenalUI.GetToolPrice(_toolData);
                priceText.text = owned ? "OWNED" : $"{price}";
            }

            if (statusText != null)
            {
                if (owned)
                    statusText.text = "UPGRADE";
                else if (!available)
                    statusText.text = "LOCKED";
                else
                    statusText.text = "";
            }

            if (purchaseButton != null)
            {
                purchaseButton.interactable = available && canAfford;
            }
        }

        public void OnPurchaseClicked()
        {
            if (_arsenalUI == null || _toolData == null) return;

            EPurchaseResult result = _arsenalUI.TryPurchaseTool(_toolData);

            if (result == EPurchaseResult.Success)
            {
                UpdateButtonState();
            }
        }
    }
}