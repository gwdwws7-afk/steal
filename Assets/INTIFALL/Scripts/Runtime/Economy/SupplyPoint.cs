using UnityEngine;
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
            if (uiPromptText == null) return;

            if (!_playerInRange)
            {
                uiPromptText.text = "";
                return;
            }

            if (_isOnCooldown)
            {
                uiPromptText.text = $"冷却中... {CooldownProgress:P0}";
            }
            else
            {
                uiPromptText.text = "按 E 补给";
            }
        }

        private void UpdateVisualState()
        {
            if (indicatorMesh == null) return;

            indicatorMesh.material = _isOnCooldown ? cooldownMaterial : activeMaterial;

            if (visualIndicator != null)
                visualIndicator.SetActive(!_isOnCooldown);
        }

        public void TrySupply()
        {
            if (_isOnCooldown) return;
            if (!_playerInRange) return;

            bool supplied = false;

            if (providesFirstAid && _playerHealth != null)
            {
                if (_playerHealth.FirstAidCount < _playerHealth.FirstAidCount)
                {
                    _playerHealth.TakeDamage(0);
                    _playerHealth.Heal(firstAidRestoreAmount);
                    EventBus.Publish(new SupplyPointUsedEvent
                    {
                        position = transform.position,
                        supplyType = "FirstAid"
                    });
                    supplied = true;
                }
            }

            if (providesTools && _playerToolManager != null)
            {
                for (int i = 0; i < _playerToolManager.EquippedTools.Length; i++)
                {
                    var tool = _playerToolManager.EquippedTools[i];
                    if (tool != null && tool.CurrentAmmo < tool.maxAmmo)
                    {
                        tool.Reload(tool.maxAmmo);
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
            EventBus.Subscribe<PlayerInteractEvent>(OnPlayerInteract);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerInteractEvent>(OnPlayerInteract);
        }

        private void OnPlayerInteract(PlayerInteractEvent evt)
        {
            if (_playerInRange && Vector3.Distance(transform.position, evt.position) <= interactionRange)
            {
                TrySupply();
            }
        }

        public struct PlayerInteractEvent
        {
            public Vector3 position;
        }
    }
}