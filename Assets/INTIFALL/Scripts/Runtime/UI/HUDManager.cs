using UnityEngine;
using INTIFALL.Player;
using INTIFALL.System;

namespace INTIFALL.UI
{
    public class HUDManager : MonoBehaviour
    {
        [Header("HUD Components")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private HPHUD hpHUD;
        [SerializeField] private AlertIndicator alertIndicator;
        [SerializeField] private EagleEyeUI eagleEyeUI;
        [SerializeField] private ToolHUD toolHUD;

        [Header("Settings")]
        [SerializeField] private bool hudEnabled = true;
        [SerializeField] private float fadeSpeed = 0.3f;

        private PlayerHealthSystem _playerHealth;
        private PlayerCombatStateMachine _combatState;
        private bool _isVisible;

        public bool IsVisible => _isVisible;

        private void Awake()
        {
            _isVisible = hudEnabled;

            if (hudPanel != null)
                hudPanel.SetActive(hudEnabled);
        }

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerHealth = player.GetComponent<PlayerHealthSystem>();
                _combatState = player.GetComponent<PlayerCombatStateMachine>();
            }

            if (hpHUD != null && _playerHealth != null)
                hpHUD.Initialize(_playerHealth);

            if (toolHUD != null)
            {
                var toolManager = player?.GetComponent<INTIFALL.Tools.ToolManager>();
                if (toolManager != null)
                    toolHUD.SetToolManager(toolManager);
            }
        }

        private void Update()
        {
            UpdateAlertState();
            UpdateHUDVisibility();
        }

        private void UpdateAlertState()
        {
            if (alertIndicator == null) return;
            if (_combatState == null) return;

            alertIndicator.SetAlertState(_combatState.IsInCombat);
        }

        private void UpdateHUDVisibility()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                ToggleHUD();
            }
        }

        public void ToggleHUD()
        {
            _isVisible = !_isVisible;

            if (hudPanel != null)
                hudPanel.SetActive(_isVisible);
        }

        public void ShowHUD()
        {
            if (_isVisible) return;
            _isVisible = true;

            if (hudPanel != null)
                hudPanel.SetActive(true);
        }

        public void HideHUD()
        {
            if (!_isVisible) return;
            _isVisible = false;

            if (hudPanel != null)
                hudPanel.SetActive(false);
        }

        public void ShowIntelPopup(string intelName)
        {
            if (eagleEyeUI != null)
                eagleEyeUI.ShowIntelPickup(intelName);
        }

        public void UpdateHPRecovery()
        {
            if (hpHUD != null)
                hpHUD.UpdateHPRecoveryState();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<HPChangedEvent>(OnHPChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<HPChangedEvent>(OnHPChanged);
        }

        private void OnHPChanged(HPChangedEvent evt)
        {
            if (hpHUD != null)
                hpHUD.UpdateHPDisplay(evt.currentHP, evt.maxHP);
        }
    }
}