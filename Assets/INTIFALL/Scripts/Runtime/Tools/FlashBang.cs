using UnityEngine;
using INTIFALL.AI;
using INTIFALL.System;

namespace INTIFALL.Tools
{
    public class FlashBang : ToolBase
    {
        [Header("FlashBang Specific")]
        [SerializeField] private float blindDuration = 3f;
        [SerializeField] private float effectRadius = 9f;

        private void Awake()
        {
            toolName = "FlashBang";
            toolNameCN = "Flash Bang";
            category = EToolCategory.PerceptionDisrupt;
            defaultSlot = EToolSlot.Slot1;
            cooldown = 24f;
            maxAmmo = 2;
            _currentAmmo = maxAmmo;
            range = 9f;
            duration = 3f;
        }

        protected override void OnToolUsed()
        {
            float effectiveRadius = range > 0f ? range : effectRadius;
            float effectiveBlindDuration = duration > 0f ? duration : blindDuration;
            Collider[] hits = Physics.OverlapSphere(transform.position, effectiveRadius);

            foreach (Collider hit in hits)
            {
                if (hit.TryGetComponent<EnemyController>(out var enemy))
                {
                    enemy.ApplyBlindEffect(effectiveBlindDuration);
                }
            }

            EventBus.Publish(new FlashBangUsedEvent
            {
                position = transform.position,
                radius = effectiveRadius,
                blindDuration = effectiveBlindDuration
            });
        }

        protected override void OnApplyToolData(ToolData data)
        {
            if (data.range > 0f)
                effectRadius = data.range;
            if (data.duration > 0f)
                blindDuration = data.duration;
        }
    }

    public struct FlashBangUsedEvent
    {
        public Vector3 position;
        public float radius;
        public float blindDuration;
    }
}
