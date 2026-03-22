using UnityEngine;
using INTIFALL.AI;
using INTIFALL.System;

namespace INTIFALL.Tools
{
    public class SoundBait : ToolBase
    {
        [Header("SoundBait Specific")]
        [SerializeField] private float attractRadius = 5f;
        [SerializeField] private float soundDuration = 3f;
        [SerializeField] private AudioClip baitSound;

        private void Awake()
        {
            toolName = "SoundBait";
            toolNameCN = "鸦鸣石";
            category = EToolCategory.AttentionShift;
            defaultSlot = EToolSlot.Slot3;
            cooldown = 10f;
            maxAmmo = 3;
            _currentAmmo = maxAmmo;
            range = 5f;
        }

        protected override void OnToolUsed()
        {
            GameObject bait = new GameObject("SoundBait");
            bait.transform.position = transform.position;

            SoundBaitComponent component = bait.AddComponent<SoundBaitComponent>();
            component.Initialize(attractRadius, soundDuration, baitSound);

            Destroy(bait, soundDuration + 1f);

            EventBus.Publish(new SoundBaitUsedEvent
            {
                position = transform.position,
                attractRadius = attractRadius,
                duration = soundDuration
            });
        }
    }

    public class SoundBaitComponent : MonoBehaviour
    {
        private float _attractRadius;
        private float _soundDuration;
        private AudioClip _sound;
        private float _timer;
        private AudioSource _audioSource;

        public void Initialize(float radius, float duration, AudioClip sound)
        {
            _attractRadius = radius;
            _soundDuration = duration;
            _sound = sound;
            _timer = 0f;

            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = sound;
            _audioSource.spatialBlend = 1f;
            _audioSource.Play();
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            if (_timer >= _soundDuration)
            {
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

    public class DroneInterference : ToolBase
    {
        [Header("DroneInterference Specific")]
        [SerializeField] private float controlRadius = 15f;
        [SerializeField] private float energyDuration = 30f;
        [SerializeField] private GameObject dronePrefab;

        private void Awake()
        {
            toolName = "DroneInterference";
            toolNameCN = "蜂群碎片";
            category = EToolCategory.AttentionShift;
            defaultSlot = EToolSlot.Slot4;
            cooldown = 60f;
            maxAmmo = 1;
            _currentAmmo = maxAmmo;
            range = 15f;
        }

        public override void Use()
        {
            if (!CanUse()) return;

            if (dronePrefab != null)
            {
                GameObject drone = Instantiate(dronePrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
                InterferenceDrone droneComponent = drone.GetComponent<InterferenceDrone>();
                if (droneComponent != null)
                {
                    droneComponent.Initialize(controlRadius, energyDuration);
                }
            }

            _currentAmmo--;
            _currentCooldown = cooldown;
            _isOnCooldown = true;

            EventBus.Publish(new ToolUsedEvent
            {
                toolName = toolName,
                category = category
            });
        }
    }

    public class InterferenceDrone : MonoBehaviour
    {
        private float _controlRadius;
        private float _energyDuration;
        private float _timer;
        private Vector3 _startPos;

        public void Initialize(float radius, float duration)
        {
            _controlRadius = radius;
            _energyDuration = duration;
            _timer = 0f;
            _startPos = transform.position;
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            transform.position = _startPos + Vector3.up * 2f + Vector3.right * Mathf.Sin(_timer * 2f) * 2f;

            if (_timer >= _energyDuration)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, _controlRadius);
            foreach (Collider hit in hits)
            {
                if (hit.TryGetComponent<EnemyController>(out var enemy))
                {
                    enemy.InvestigateSound(transform.position);
                }
            }
        }
    }

    public struct DroneUsedEvent
    {
        public Vector3 position;
        public float controlRadius;
        public float energyDuration;
    }
}
