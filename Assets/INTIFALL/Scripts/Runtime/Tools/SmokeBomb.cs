using UnityEngine;
using INTIFALL.System;

namespace INTIFALL.Tools
{
    public class SmokeBomb : ToolBase
    {
        [Header("Smoke Bomb Specific")]
        [SerializeField] private float smokeRadius = 5f;
        [SerializeField] private float smokeDuration = 7f;
        [SerializeField] private GameObject smokeVFX;

        private void Awake()
        {
            toolName = "SmokeBomb";
            toolNameCN = "Smoke Bomb";
            category = EToolCategory.PerceptionDisrupt;
            defaultSlot = EToolSlot.Slot1;
            cooldown = 16f;
            maxAmmo = 3;
            _currentAmmo = maxAmmo;
            range = 6f;
            duration = 7f;
        }

        protected override void OnToolUsed()
        {
            float effectiveRadius = range > 0f ? range : smokeRadius;
            float effectiveDuration = duration > 0f ? duration : smokeDuration;

            if (smokeVFX != null)
            {
                GameObject vfx = Instantiate(smokeVFX, transform.position, Quaternion.identity);
                SmokeEffect effect = vfx.GetComponent<SmokeEffect>();
                if (effect != null)
                {
                    effect.Initialize(effectiveRadius, effectiveDuration);
                }
            }

            EventBus.Publish(new SmokeBombUsedEvent
            {
                position = transform.position,
                radius = effectiveRadius,
                duration = effectiveDuration
            });
        }

        protected override void OnApplyToolData(ToolData data)
        {
            if (data.range > 0f)
                smokeRadius = data.range;
            if (data.duration > 0f)
                smokeDuration = data.duration;
        }
    }

    public class SmokeEffect : MonoBehaviour
    {
        private float _radius;
        private float _duration;
        private float _timer;
        private SphereCollider _collider;

        public void Initialize(float radius, float duration)
        {
            _radius = radius;
            _duration = duration;
            _timer = 0f;

            if (_collider == null)
                _collider = gameObject.AddComponent<SphereCollider>();

            _collider.radius = radius;
            _collider.isTrigger = true;

            Destroy(gameObject, duration);
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            float normalized = _duration <= 0f ? 1f : Mathf.Clamp01(_timer / _duration);
            transform.localScale = Vector3.one * (_radius * normalized);
        }
    }

    public struct SmokeBombUsedEvent
    {
        public Vector3 position;
        public float radius;
        public float duration;
    }
}
