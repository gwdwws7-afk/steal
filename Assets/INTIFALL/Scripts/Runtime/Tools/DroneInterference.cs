using UnityEngine;
using INTIFALL.AI;
using INTIFALL.System;

namespace INTIFALL.Tools
{
    public class DroneInterference : ToolBase
    {
        [Header("DroneInterference Specific")]
        [SerializeField] private float controlRadius = 15f;
        [SerializeField] private float energyDuration = 24f;
        [SerializeField] private GameObject dronePrefab;

        private void Awake()
        {
            toolName = "DroneInterference";
            toolNameCN = "Drone Interference";
            category = EToolCategory.AttentionShift;
            defaultSlot = EToolSlot.Slot4;
            cooldown = 50f;
            maxAmmo = 1;
            _currentAmmo = maxAmmo;
            range = 15f;
            duration = 24f;
        }

        public override void Use()
        {
            if (!CanUse())
                return;

            float effectiveRadius = range > 0f ? range : controlRadius;
            float effectiveDuration = duration > 0f ? duration : energyDuration;

            if (dronePrefab != null)
            {
                GameObject drone = Instantiate(dronePrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
                InterferenceDrone droneComponent = drone.GetComponent<InterferenceDrone>();
                if (droneComponent != null)
                {
                    droneComponent.Initialize(effectiveRadius, effectiveDuration);
                }
            }

            if (HasLimitedAmmo)
                _currentAmmo = Mathf.Max(0, _currentAmmo - 1);

            _currentCooldown = Mathf.Max(0f, cooldown);
            _isOnCooldown = _currentCooldown > 0f;

            EventBus.Publish(new ToolUsedEvent
            {
                toolName = toolName,
                category = category
            });

            EventBus.Publish(new DroneUsedEvent
            {
                position = transform.position,
                controlRadius = effectiveRadius,
                energyDuration = effectiveDuration
            });
        }

        protected override void OnApplyToolData(ToolData data)
        {
            if (data.range > 0f)
                controlRadius = data.range;
            if (data.duration > 0f)
                energyDuration = data.duration;
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
