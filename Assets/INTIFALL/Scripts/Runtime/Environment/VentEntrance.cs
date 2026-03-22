using UnityEngine;
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
                ShowPrompt("按 E 进入通风管");
            }
            else if (_isInside)
            {
                ShowPrompt("按 E 离开通风管");
            }
            else
            {
                HidePrompt();
            }
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
                uiPrompt.text = "";
        }

        public void TryEnter()
        {
            if (!_playerInRange) return;

            if (_isInside)
            {
                ExitVent();
            }
            else
            {
                EnterVent();
            }
        }

        private void EnterVent()
        {
            if (_player == null || _playerController == null) return;

            _isInside = true;

            if (_playerController != null)
            {
                _playerController.AttachToRope(transform.position);
            }

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
            if (_player == null || _playerController == null) return;

            _isInside = false;

            if (_playerController != null)
            {
                _playerController.DetachFromRope();
                _player.transform.position = exitPosition;
            }

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