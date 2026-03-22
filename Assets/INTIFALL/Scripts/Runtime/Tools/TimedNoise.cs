using UnityEngine;
using INTIFALL.AI;
using INTIFALL.System;

namespace INTIFALL.Tools
{
    public class TimedNoise : ToolBase
    {
        [Header("TimedNoise Specific")]
        [SerializeField] private float minDelay = 1f;
        [SerializeField] private float maxDelay = 5f;
        [SerializeField] private float noiseRadius = 15f;
        [SerializeField] private AudioClip noiseSound;

        private float _currentDelay;
        private bool _isArmed;
        private GameObject _noiseSource;

        private void Awake()
        {
            toolName = "TimedNoise";
            toolNameCN = "定时噪音";
            category = EToolCategory.AttentionShift;
            defaultSlot = EToolSlot.Slot2;
            cooldown = 30f;
            maxAmmo = 2;
            _currentAmmo = maxAmmo;
            range = 15f;
        }

        protected override void OnToolUsed()
        {
            _currentDelay = Random.Range(minDelay, maxDelay);
            _isArmed = true;

            _noiseSource = new GameObject("TimedNoiseSource");
            _noiseSource.transform.position = transform.position;

            AudioSource source = _noiseSource.AddComponent<AudioSource>();
            source.clip = noiseSound;
            source.spatialBlend = 1f;
            source.playOnAwake = false;

            TimedNoiseComponent component = _noiseSource.AddComponent<TimedNoiseComponent>();
            component.Initialize(_currentDelay, noiseRadius, source);

            Destroy(_noiseSource, _currentDelay + 5f);

            EventBus.Publish(new TimedNoisePlacedEvent
            {
                position = transform.position,
                delay = _currentDelay,
                radius = noiseRadius
            });
        }

        public void SetDelay(float delay)
        {
            _currentDelay = Mathf.Clamp(delay, minDelay, maxDelay);
        }

        public float GetCurrentDelay()
        {
            return _currentDelay;
        }
    }

    public class TimedNoiseComponent : MonoBehaviour
    {
        private float _delay;
        private float _noiseRadius;
        private AudioSource _audioSource;
        private float _timer;
        private bool _exploded;

        public void Initialize(float delay, float radius, AudioSource source)
        {
            _delay = delay;
            _noiseRadius = radius;
            _audioSource = source;
            _timer = 0f;
            _exploded = false;
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            if (_timer >= _delay && !_exploded)
            {
                Explode();
            }
        }

        private void Explode()
        {
            _exploded = true;

            if (_audioSource != null)
                _audioSource.Play();

            Collider[] hits = Physics.OverlapSphere(transform.position, _noiseRadius);
            foreach (Collider hit in hits)
            {
                if (hit.TryGetComponent<EnemyController>(out var enemy))
                {
                    enemy.InvestigateSound(transform.position);
                }
            }

            EventBus.Publish(new TimedNoiseExplodedEvent
            {
                position = transform.position,
                radius = _noiseRadius
            });
        }
    }

    public struct TimedNoisePlacedEvent
    {
        public Vector3 position;
        public float delay;
        public float radius;
    }

    public struct TimedNoiseExplodedEvent
    {
        public Vector3 position;
        public float radius;
    }
}