using UnityEngine;
using INTIFALL.AI;
using INTIFALL.System;

namespace INTIFALL.Tools
{
    public class EMP : ToolBase
    {
        [Header("EMP Specific")]
        [SerializeField] private float disableDuration = 12f;
        [SerializeField] private float radius = 5f;
        [SerializeField] private LayerMask affectedLayers;

        private void Awake()
        {
            toolName = "EMP";
            toolNameCN = "震脉器";
            category = EToolCategory.PerceptionDisrupt;
            defaultSlot = EToolSlot.Slot2;
            cooldown = 30f;
            maxAmmo = 2;
            _currentAmmo = maxAmmo;
            range = 5f;
        }

        protected override void OnToolUsed()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, affectedLayers);

            foreach (Collider hit in hits)
            {
                if (hit.TryGetComponent<ElectronicDevice>(out var device))
                {
                    device.Disable(disableDuration);
                }

                if (hit.TryGetComponent<EnemyController>(out var enemy))
                {
                    enemy.ApplyEMPEffect(disableDuration);
                }

                if (hit.TryGetComponent<PerceptionModule>(out var perception))
                {
                    perception.ApplyEMPEffect(disableDuration);
                }
            }

            EventBus.Publish(new EMPUsedEvent
            {
                position = transform.position,
                radius = radius,
                disableDuration = disableDuration
            });
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
