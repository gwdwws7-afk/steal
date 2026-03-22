using UnityEngine;

namespace INTIFALL.Tools
{
    public class SmokeBomb : ToolBase
    {
        [Header("Smoke Bomb Specific")]
        [SerializeField] private float radius = 4f;
        [SerializeField] private float duration = 8f;
        [SerializeField] private GameObject smokeVFX;

        private void Awake()
        {
            toolName = "SmokeBomb";
            toolNameCN = "云雾祭礼";
            category = EToolCategory.PerceptionDisrupt;
            defaultSlot = EToolSlot.Slot1;
            cooldown = 15f;
            maxAmmo = 3;
            _currentAmmo = maxAmmo;
            range = 4f;
        }

        protected override void OnToolUsed()
        {
            if (smokeVFX != null)
            {
                GameObject vfx = Instantiate(smokeVFX, transform.position, Quaternion.identity);
                SmokeEffect effect = vfx.GetComponent<SmokeEffect>();
                if (effect != null)
                {
                    effect.Initialize(radius, duration);
                }
            }

            EventBus.Publish(new SmokeBombUsedEvent
            {
                position = transform.position,
                radius = radius,
                duration = duration
            });
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
            transform.localScale = Vector3.one * (_radius * (_timer / _duration));
        }
    }

    public struct SmokeBombUsedEvent
    {
        public Vector3 position;
        public float radius;
        public float duration;
    }
}
