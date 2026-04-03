using UnityEngine;
using INTIFALL.Audio;
using INTIFALL.System;

namespace INTIFALL.Environment
{
    public class BreakableWall : MonoBehaviour
    {
        [Header("Break Settings")]
        [SerializeField] private float breakTime = 3f;
        [SerializeField] private int hitCount = 3;
        [SerializeField] private float hitCooldown = 1f;

        [Header("Visual")]
        [SerializeField] private GameObject crackOverlay;
        [SerializeField] private GameObject breakParticles;
        [SerializeField] private AudioClip[] breakSounds;
        [SerializeField] private AudioClip hitSound;

        [Header("After Break")]
        [SerializeField] private GameObject debrisPrefab;
        [SerializeField] private float debrisLifetime = 5f;

        private int _currentHits;
        private float _hitTimer;
        private bool _isBreaking;
        private bool _isBroken;
        private float _breakProgress;
        private Renderer _wallRenderer;

        public float BreakProgress => _breakProgress;
        public bool IsBroken => _isBroken;
        public bool IsBreaking => _isBreaking;

        private void Awake()
        {
            _wallRenderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            if (_isBreaking && !_isBroken)
            {
                _breakProgress += Time.deltaTime / breakTime;

                if (_breakProgress >= 1f)
                {
                    BreakWall();
                }
            }

            if (_hitTimer > 0)
                _hitTimer -= Time.deltaTime;
        }

        public void Hit()
        {
            if (_isBroken) return;
            if (_hitTimer > 0) return;

            _currentHits++;
            _hitTimer = hitCooldown;

            AudioManager.Instance?.PlaySFX(hitSound);

            if (_currentHits >= hitCount)
            {
                StartBreaking();
            }
            else
            {
                ShowCracks();
            }
        }

        public void StartBreaking()
        {
            if (_isBroken || _isBreaking) return;

            _isBreaking = true;
            _breakProgress = 0f;

            EventBus.Publish(new WallBreakStartedEvent
            {
                wallPosition = transform.position
            });
        }

        private void ShowCracks()
        {
            if (crackOverlay != null)
            {
                crackOverlay.SetActive(true);
                float crackLevel = (float)_currentHits / hitCount;
                crackOverlay.transform.localScale = Vector3.one * crackLevel;
            }
        }

        private void BreakWall()
        {
            _isBroken = true;
            _isBreaking = false;

            if (crackOverlay != null)
                crackOverlay.SetActive(false);

            if (breakParticles != null)
                breakParticles.SetActive(true);

            AudioClip breakSound = breakSounds != null && breakSounds.Length > 0
                ? breakSounds[Random.Range(0, breakSounds.Length)]
                : null;
            AudioManager.Instance?.PlaySFX(breakSound);

            SpawnDebris();

            gameObject.SetActive(false);

            EventBus.Publish(new WallBrokenEvent
            {
                wallPosition = transform.position
            });
        }

        private void SpawnDebris()
        {
            if (debrisPrefab == null) return;

            GameObject debris = Instantiate(debrisPrefab, transform.position, transform.rotation);
            Destroy(debris, debrisLifetime);
        }

        public void ResetWall()
        {
            _isBroken = false;
            _isBreaking = false;
            _currentHits = 0;
            _breakProgress = 0f;
            _hitTimer = 0f;

            if (crackOverlay != null)
                crackOverlay.SetActive(false);

            if (breakParticles != null)
                breakParticles.SetActive(false);

            gameObject.SetActive(true);
        }

        public struct WallBreakStartedEvent
        {
            public Vector3 wallPosition;
        }

        public struct WallBrokenEvent
        {
            public Vector3 wallPosition;
        }
    }
}
