using INTIFALL.System;
using UnityEngine;

namespace INTIFALL.Tools
{
    public class RopeTool : ToolBase
    {
        [Header("Rope Tool")]
        [SerializeField] private float deploymentRange = 12f;
        [SerializeField] private float climbAssistDuration = 8f;

        private void Awake()
        {
            toolName = "Rope";
            toolNameCN = "Rope";
            category = EToolCategory.Environmental;
            defaultSlot = EToolSlot.Slot4;
            cooldown = 6f;
            maxAmmo = 3;
            _currentAmmo = maxAmmo;
            range = deploymentRange;
            duration = climbAssistDuration;
        }

        protected override void OnToolUsed()
        {
            float effectiveRange = range > 0f ? range : deploymentRange;
            float effectiveDuration = duration > 0f ? duration : climbAssistDuration;

            EventBus.Publish(new RopeUsedEvent
            {
                position = transform.position,
                range = effectiveRange,
                duration = effectiveDuration
            });
        }

        protected override void OnApplyToolData(ToolData data)
        {
            if (data.range > 0f)
                deploymentRange = data.range;
            if (data.duration > 0f)
                climbAssistDuration = data.duration;
        }
    }

    public struct RopeUsedEvent
    {
        public Vector3 position;
        public float range;
        public float duration;
    }
}
