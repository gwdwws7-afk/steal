using UnityEngine;
using INTIFALL.System;

namespace INTIFALL.Environment
{
    public class ElectronicDoor : MonoBehaviour
    {
        [Header("Door Settings")]
        [SerializeField] private float openSpeed = 2f;
        [SerializeField] private float closeSpeed = 1f;
        [SerializeField] private float openHeight = 3f;
        [SerializeField] private bool autoClose = true;
        [SerializeField] private float autoCloseDelay = 3f;

        [Header("Lock Settings")]
        [SerializeField] private bool isLocked = true;
        [SerializeField] private int requiredKeycardLevel = 0;
        [SerializeField] private string[] unlockCodes;

        [Header("Visual")]
        [SerializeField] private Text statusText;
        [SerializeField] private Color lockedColor = Color.red;
        [SerializeField] private Color unlockedColor = Color.green;
        [SerializeField] private Color openingColor = Color.yellow;
        [SerializeField] private Renderer statusLight;

        [Header("Audio")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private AudioClip deniedSound;
        [SerializeField] private AudioClip unlockSound;

        private Vector3 _closedPosition;
        private Vector3 _openPosition;
        private bool _isOpen;
        private bool _isMoving;
        private float _autoCloseTimer;
        private bool _playerInRange;

        public bool IsOpen => _isOpen;
        public bool IsLocked => isLocked;

        private void Awake()
        {
            _closedPosition = transform.position;
            _openPosition = _closedPosition + Vector3.up * openHeight;
        }

        private void Update()
        {
            if (_isMoving)
            {
                MoveDoor();
            }

            if (autoClose && _isOpen && !_isMoving)
            {
                _autoCloseTimer -= Time.deltaTime;
                if (_autoCloseTimer <= 0)
                {
                    Close();
                }
            }

            if (_playerInRange)
            {
                if (isLocked)
                    ShowStatus("门已锁定 - 需要钥匙卡");
                else if (!_isOpen)
                    ShowStatus("按 E 开门");
            }
            else
            {
                HideStatus();
            }
        }

        private void MoveDoor()
        {
            Vector3 target = _isOpen ? _openPosition : _closedPosition;
            float speed = _isOpen ? openSpeed : closeSpeed;

            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.01f)
            {
                transform.position = target;
                _isMoving = false;

                AudioSource.PlayClipAtPoint(_isOpen ? openSound : closeSound, transform.position);
            }
        }

        public void TryOpen()
        {
            if (_isOpen || _isMoving) return;

            if (isLocked)
            {
                AudioSource.PlayClipAtPoint(deniedSound, transform.position);
                return;
            }

            Open();
        }

        public void Open()
        {
            if (_isOpen) return;

            _isOpen = true;
            _isMoving = true;
            _autoCloseTimer = autoCloseDelay;

            SetStatusLight(openingColor);
        }

        public void Close()
        {
            if (!_isOpen || _isMoving) return;

            _isOpen = false;
            _isMoving = true;

            SetStatusLight(unlockedColor);
        }

        public bool TryUnlock(int keycardLevel)
        {
            if (keycardLevel >= requiredKeycardLevel)
            {
                Unlock();
                return true;
            }

            AudioSource.PlayClipAtPoint(deniedSound, transform.position);
            return false;
        }

        public bool TryUnlockWithCode(string code)
        {
            if (unlockCodes == null) return false;

            foreach (var unlockCode in unlockCodes)
            {
                if (unlockCode == code)
                {
                    Unlock();
                    return true;
                }
            }

            AudioSource.PlayClipAtPoint(deniedSound, transform.position);
            return false;
        }

        public void Unlock()
        {
            if (!isLocked) return;

            isLocked = false;
            SetStatusLight(unlockedColor);

            AudioSource.PlayClipAtPoint(unlockSound, transform.position);

            EventBus.Publish(new DoorUnlockedEvent
            {
                doorPosition = transform.position
            });
        }

        public void Lock()
        {
            isLocked = true;
            SetStatusLight(lockedColor);
        }

        public void SetKeycardLevel(int level)
        {
            requiredKeycardLevel = level;
        }

        private void OnInteract()
        {
            if (_playerInRange)
                TryOpen();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = false;
            }
        }

        private void SetStatusLight(Color color)
        {
            if (statusLight != null)
                statusLight.material.color = color;
        }

        private void ShowStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        private void HideStatus()
        {
            if (statusText != null)
                statusText.text = "";
        }

        public struct DoorUnlockedEvent
        {
            public Vector3 doorPosition;
        }
    }
}