using UnityEngine;
using INTIFALL.AI;
using INTIFALL.Environment;
using INTIFALL.System;

namespace INTIFALL.Tools
{
    public class EMP : ToolBase
    {
        [Header("EMP Specific")]
        [SerializeField] private float disableDuration = 10f;
        [SerializeField] private float effectRadius = 6f;
        [SerializeField] private LayerMask affectedLayers;

        private void Awake()
        {
            toolName = "EMP";
            toolNameCN = "EMP";
            category = EToolCategory.PerceptionDisrupt;
            defaultSlot = EToolSlot.Slot2;
            cooldown = 32f;
            maxAmmo = 1;
            _currentAmmo = maxAmmo;
            range = 6f;
            duration = 10f;
        }

        protected override void OnToolUsed()
        {
            float effectiveRadius = range > 0f ? range : effectRadius;
            float effectiveDisableDuration = duration > 0f ? duration : disableDuration;
            Collider[] hits = Physics.OverlapSphere(transform.position, effectiveRadius, affectedLayers);

            foreach (Collider hit in hits)
            {
                ElectronicDoor door = hit.GetComponent<ElectronicDoor>();
                if (door == null)
                    door = hit.GetComponentInParent<ElectronicDoor>();
                if (door != null)
                    door.ApplyEMPDisruption(effectiveDisableDuration);

                if (hit.TryGetComponent<ElectronicDevice>(out var device))
                {
                    device.Disable(effectiveDisableDuration);
                }

                if (hit.TryGetComponent<EnemyController>(out var enemy))
                {
                    enemy.ApplyEMPEffect(effectiveDisableDuration);
                }

                if (hit.TryGetComponent<PerceptionModule>(out var perception))
                {
                    perception.ApplyEMPEffect(effectiveDisableDuration);
                }
            }

            EventBus.Publish(new EMPUsedEvent
            {
                position = transform.position,
                radius = effectiveRadius,
                disableDuration = effectiveDisableDuration
            });
        }

        protected override void OnApplyToolData(ToolData data)
        {
            if (data.range > 0f)
                effectRadius = data.range;
            if (data.duration > 0f)
                disableDuration = data.duration;
        }
    }

    public interface IElectronic
    {
        void Disable(float duration);
        void Enable();
        bool IsDisabled { get; }
    }

    public class ElectronicDevice : MonoBehaviour, IElectronic
    {
        [SerializeField] private bool _isDisabled;
        private float _disableEndTime;

        public bool IsDisabled => _isDisabled;

        public void Disable(float duration)
        {
            _isDisabled = true;
            _disableEndTime = Time.time + duration;
            OnDisabled();
        }

        public void Enable()
        {
            _isDisabled = false;
            OnEnabled();
        }

        private void Update()
        {
            if (_isDisabled && Time.time >= _disableEndTime)
            {
                Enable();
            }
        }

        protected virtual void OnDisabled() { }
        protected virtual void OnEnabled() { }
    }

    public struct EMPUsedEvent
    {
        public Vector3 position;
        public float radius;
        public float disableDuration;
    }
}
