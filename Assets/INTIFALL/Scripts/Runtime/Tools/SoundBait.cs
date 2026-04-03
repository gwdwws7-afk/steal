using UnityEngine;
using INTIFALL.AI;
using INTIFALL.System;

namespace INTIFALL.Tools
{
    public class SoundBait : ToolBase
    {
        [Header("SoundBait Specific")]
        [SerializeField] private float attractRadius = 7f;
        [SerializeField] private float soundDuration = 2.5f;
        [SerializeField] private AudioClip baitSound;

        private void Awake()
        {
            toolName = "SoundBait";
            toolNameCN = "Sound Bait";
            category = EToolCategory.AttentionShift;
            defaultSlot = EToolSlot.Slot3;
            cooldown = 8f;
            maxAmmo = 4;
            _currentAmmo = maxAmmo;
            range = 7f;
            duration = 2.5f;
        }

        protected override void OnToolUsed()
        {
            float effectiveRadius = range > 0f ? range : attractRadius;
            float effectiveDuration = duration > 0f ? duration : soundDuration;

            GameObject bait = new GameObject("SoundBait");
            bait.transform.position = transform.position;

            SoundBaitComponent component = bait.AddComponent<SoundBaitComponent>();
            component.Initialize(effectiveRadius, effectiveDuration, baitSound);

            Destroy(bait, effectiveDuration + 1f);

            EventBus.Publish(new SoundBaitUsedEvent
            {
                position = transform.position,
                attractRadius = effectiveRadius,
                duration = effectiveDuration
            });
        }

        protected override void OnApplyToolData(ToolData data)
        {
            if (data.range > 0f)
                attractRadius = data.range;
            if (data.duration > 0f)
                soundDuration = data.duration;
        }
    }

    public class SoundBaitComponent : MonoBehaviour
    {
        private float _attractRadius;
        private float _soundDuration;
        private AudioClip _sound;
        private float _timer;
        private bool _triggered;
        private AudioSource _audioSource;

        public void Initialize(float radius, float duration, AudioClip sound)
        {
            _attractRadius = radius;
            _soundDuration = duration;
            _sound = sound;
            _timer = 0f;
            _triggered = false;

            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = sound;
            _audioSource.spatialBlend = 1f;
            _audioSource.Play();
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            if (!_triggered && _timer >= _soundDuration)
            {
                _triggered = true;
                Collider[] hits = Physics.OverlapSphere(transform.position, _attractRadius);
                foreach (Collider hit in hits)
                {
                    if (hit.TryGetComponent<EnemyController>(out var enemy))
                    {
                        enemy.InvestigateSound(transform.position);
                    }
                }
            }
        }
    }

    public struct SoundBaitUsedEvent
    {
        public Vector3 position;
        public float attractRadius;
        public float duration;
    }
}
