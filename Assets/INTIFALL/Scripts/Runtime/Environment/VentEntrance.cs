using UnityEngine;
using UnityEngine.UI;
using INTIFALL.Audio;
using INTIFALL.Input;
using INTIFALL.Player;
using INTIFALL.System;

namespace INTIFALL.Environment
{
    public class VentEntrance : MonoBehaviour
    {
        [Header("Vent Settings")]
        [SerializeField] private float enterRange = 1.5f;
        [SerializeField] private float moveSpeed = 1.5f;
        [SerializeField] private Vector3 exitPosition;

        [Header("Visual")]
        [SerializeField] private Text uiPrompt;
        [SerializeField] private GameObject ventCover;

        [Header("Audio")]
        [SerializeField] private AudioClip enterSound;
        [SerializeField] private AudioClip exitSound;

        private bool _playerInRange;
        private bool _isInside;
        private GameObject _player;
        private PlayerController _playerController;

        public bool IsInside => _isInside;
        public Vector3 ExitPosition => exitPosition;

        private void Update()
        {
            CheckPlayerProximity();

            if (_playerInRange && !_isInside)
            {
                ShowPrompt(LocalizationService.Get(
                    "vent.prompt.enter",
                    fallbackEnglish: "Press E to enter vent",
                    fallbackChinese: string.Empty));
            }
            else if (_isInside)
            {
                ShowPrompt(LocalizationService.Get(
                    "vent.prompt.exit",
                    fallbackEnglish: "Press E to exit vent",
                    fallbackChinese: string.Empty));
            }
            else
                HidePrompt();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<InputManager.InteractEvent>(OnPlayerInteract);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InputManager.InteractEvent>(OnPlayerInteract);
        }

        private void CheckPlayerProximity()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            if (_player == null)
            {
                _playerInRange = false;
                return;
            }

            float distance = Vector3.Distance(transform.position, _player.transform.position);
            _playerInRange = distance <= enterRange;

            if (_playerInRange && _playerController == null)
                _playerController = _player.GetComponent<PlayerController>();
        }

        private void ShowPrompt(string message)
        {
            if (uiPrompt != null)
                uiPrompt.text = message;
        }

        private void HidePrompt()
        {
            if (uiPrompt != null)
                uiPrompt.text = string.Empty;
        }

        public void TryEnter()
        {
            if (!_playerInRange)
                return;

            if (_isInside)
                ExitVent();
            else
                EnterVent();
        }

        private void EnterVent()
        {
            if (_player == null || _playerController == null)
                return;

            _isInside = true;
            _playerController.AttachToRope(transform.position);

            if (ventCover != null)
                ventCover.SetActive(false);

            AudioManager.Instance?.PlaySFX(enterSound);

            EventBus.Publish(new VentEnteredEvent
            {
                ventPosition = transform.position
            });
        }

        private void ExitVent()
        {
            if (_player == null || _playerController == null)
                return;

            _isInside = false;
            _playerController.DetachFromRope();
            _player.transform.position = exitPosition;

            if (ventCover != null)
                ventCover.SetActive(true);

            AudioManager.Instance?.PlaySFX(exitSound);

            EventBus.Publish(new VentExitedEvent
            {
                exitPosition = exitPosition
            });
        }

        private void OnInteract()
        {
            TryEnter();
        }

        private void OnPlayerInteract(InputManager.InteractEvent evt)
        {
            if (_playerInRange)
                TryEnter();
        }

        public struct VentEnteredEvent
        {
            public Vector3 ventPosition;
        }

        public struct VentExitedEvent
        {
            public Vector3 exitPosition;
        }
    }
}
