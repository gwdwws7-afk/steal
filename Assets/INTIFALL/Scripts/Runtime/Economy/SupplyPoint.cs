using UnityEngine;
using UnityEngine.UI;
using INTIFALL.Input;
using INTIFALL.Player;
using INTIFALL.Tools;
using INTIFALL.System;

namespace INTIFALL.Economy
{
    public struct SupplyPointUsedEvent
    {
        public Vector3 position;
        public string supplyType;
    }

    public class SupplyPoint : MonoBehaviour
    {
        [Header("Supply Settings")]
        [SerializeField] private bool providesFirstAid = true;
        [SerializeField] private bool providesTools = true;
        [SerializeField] private int firstAidRestoreAmount = 5;
        [SerializeField] private int toolAmmoRefillAmount = 1;
        [SerializeField] private bool resetToolCooldownOnSupply;
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private float cooldownDuration = 30f;

        [Header("Visual")]
        [SerializeField] private Text uiPromptText;
        [SerializeField] private GameObject visualIndicator;
        [SerializeField] private Material activeMaterial;
        [SerializeField] private Material cooldownMaterial;

        [Header("References")]
        [SerializeField] private MeshRenderer indicatorMesh;

        private float _cooldownTimer;
        private bool _isOnCooldown;
        private bool _playerInRange;
        private PlayerHealthSystem _playerHealth;
        private ToolManager _playerToolManager;

        public bool IsOnCooldown => _isOnCooldown;
        public float CooldownProgress => _isOnCooldown ? 1f - (_cooldownTimer / cooldownDuration) : 1f;

        private void Awake()
        {
            _cooldownTimer = 0f;
            _isOnCooldown = false;
            _playerInRange = false;
            UpdateVisualState();
        }

        private void Update()
        {
            if (_isOnCooldown)
            {
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer <= 0f)
                {
                    _cooldownTimer = 0f;
                    _isOnCooldown = false;
                    UpdateVisualState();
                }
            }

            CheckPlayerProximity();
            UpdatePrompt();
        }

        private void CheckPlayerProximity()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                _playerInRange = false;
                return;
            }

            float distance = Vector3.Distance(transform.position, player.transform.position);
            _playerInRange = distance <= interactionRange;

            if (_playerInRange)
            {
                if (_playerHealth == null)
                    _playerHealth = player.GetComponent<PlayerHealthSystem>();
                if (_playerToolManager == null)
                    _playerToolManager = player.GetComponent<ToolManager>();
            }
        }

        private void UpdatePrompt()
        {
            if (uiPromptText == null)
                return;

            if (!_playerInRange)
            {
                uiPromptText.text = string.Empty;
                return;
            }

            if (_isOnCooldown)
            {
                string template = LocalizationService.Get(
                    "supply.prompt.cooldown",
                    fallbackEnglish: "Cooling down... {0}",
                    fallbackChinese: string.Empty);
                uiPromptText.text = string.Format(template, CooldownProgress.ToString("P0"));
            }
            else
            {
                uiPromptText.text = LocalizationService.Get(
                    "supply.prompt.ready",
                    fallbackEnglish: "Press E to resupply",
                    fallbackChinese: string.Empty);
            }
        }

        private void UpdateVisualState()
        {
            if (indicatorMesh == null)
                return;

            indicatorMesh.material = _isOnCooldown ? cooldownMaterial : activeMaterial;

            if (visualIndicator != null)
                visualIndicator.SetActive(!_isOnCooldown);
        }

        public void TrySupply()
        {
            if (_isOnCooldown)
                return;
            if (!_playerInRange)
                return;

            bool supplied = false;

            if (providesFirstAid && _playerHealth != null)
            {
                int beforeHp = _playerHealth.CurrentHP;
                if (beforeHp < _playerHealth.MaxHP)
                {
                    _playerHealth.Heal(firstAidRestoreAmount);
                    if (_playerHealth.CurrentHP > beforeHp)
                    {
                        EventBus.Publish(new SupplyPointUsedEvent
                        {
                            position = transform.position,
                            supplyType = "FirstAid"
                        });
                        supplied = true;
                    }
                }
            }

            if (providesTools && _playerToolManager != null)
            {
                for (int i = 0; i < _playerToolManager.EquippedTools.Length; i++)
                {
                    ToolBase tool = _playerToolManager.EquippedTools[i];
                    if (tool == null || tool.CurrentAmmo >= tool.maxAmmo)
                        continue;

                    int beforeAmmo = tool.CurrentAmmo;
                    tool.Reload(Mathf.Max(1, toolAmmoRefillAmount));

                    if (tool.CurrentAmmo > beforeAmmo)
                    {
                        if (resetToolCooldownOnSupply)
                            tool.ResetCooldown();

                        EventBus.Publish(new SupplyPointUsedEvent
                        {
                            position = transform.position,
                            supplyType = "Tool"
                        });
                        supplied = true;
                    }
                }
            }

            if (supplied)
            {
                StartCooldown();
            }
        }

        private void StartCooldown()
        {
            _isOnCooldown = true;
            _cooldownTimer = cooldownDuration;
            UpdateVisualState();
        }

        private void OnInteract()
        {
            TrySupply();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<InputManager.InteractEvent>(OnPlayerInteract);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InputManager.InteractEvent>(OnPlayerInteract);
        }

        private void OnPlayerInteract(InputManager.InteractEvent evt)
        {
            if (_playerInRange)
                TrySupply();
        }

        public void Configure(bool firstAidEnabled, bool toolsEnabled, float cooldownSeconds)
        {
            providesFirstAid = firstAidEnabled;
            providesTools = toolsEnabled;
            cooldownDuration = Mathf.Max(0f, cooldownSeconds);
        }
    }
}
