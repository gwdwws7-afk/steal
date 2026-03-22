using UnityEngine;
using INTIFALL.AI;
using INTIFALL.System;

namespace INTIFALL.Tools
{
    public class FlashBang : ToolBase
    {
        [Header("FlashBang Specific")]
        [SerializeField] private float blindDuration = 2.5f;
        [SerializeField] private float radius = 8f;

        private void Awake()
        {
            toolName = "FlashBang";
            toolNameCN = "太阳刺";
            category = EToolCategory.PerceptionDisrupt;
            defaultSlot = EToolSlot.Slot1;
            cooldown = 20f;
            maxAmmo = 2;
            _currentAmmo = maxAmmo;
            range = 8f;
        }

        protected override void OnToolUsed()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);

            foreach (Collider hit in hits)
            {
                if (hit.TryGetComponent<EnemyController>(out var enemy))
                {
                    enemy.ApplyBlindEffect(blindDuration);
                }
            }

            EventBus.Publish(new FlashBangUsedEvent
            {
                position = transform.position,
                radius = radius,
                blindDuration = blindDuration
            });
        }
    }

    public struct FlashBangUsedEvent
    {
        public Vector3 position;
        public float radius;
        public float blindDuration;
    }

    public class SleepDart : ToolBase
    {
        [Header("SleepDart Specific")]
        [SerializeField] private float sleepDuration = 20f;
        [SerializeField] private float projectileSpeed = 30f;
        [SerializeField] private GameObject dartPrefab;

        private void Awake()
        {
            toolName = "SleepDart";
            toolNameCN = "梦境叶";
            category = EToolCategory.DirectRemove;
            defaultSlot = EToolSlot.Slot3;
            cooldown = 3f;
            maxAmmo = 6;
            _currentAmmo = maxAmmo;
            range = 15f;
            damage = 0;
        }

        public override void Use()
        {
            if (!CanUse()) return;

            Vector3 targetDir = GetAimDirection();
            FireDart(targetDir);

            _currentAmmo--;
            _currentCooldown = cooldown;
            _isOnCooldown = true;

            EventBus.Publish(new ToolUsedEvent
            {
                toolName = toolName,
                category = category
            });
        }

        private Vector3 GetAimDirection()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                return ray.direction.normalized;
            }
            return transform.forward;
        }

        private void FireDart(Vector3 direction)
        {
            if (dartPrefab != null)
            {
                GameObject dart = Instantiate(dartPrefab, transform.position, Quaternion.LookRotation(direction));
                SleepDartProjectile proj = dart.GetComponent<SleepDartProjectile>();
                if (proj != null)
                {
                    proj.Initialize(direction, projectileSpeed, sleepDuration);
                }
            }
        }
    }

    public class SleepDartProjectile : MonoBehaviour
    {
        private Vector3 _direction;
        private float _speed;
        private float _sleepDuration;
        private float _lifetime;

        public void Initialize(Vector3 direction, float speed, float sleepDuration)
        {
            _direction = direction;
            _speed = speed;
            _sleepDuration = sleepDuration;
            _lifetime = 0f;
        }

        private void Update()
        {
            transform.position += _direction * _speed * Time.deltaTime;
            _lifetime += Time.deltaTime;

            if (_lifetime > 5f)
                Destroy(gameObject);

            RaycastHit hit;
            if (Physics.Raycast(transform.position, _direction, out hit, _speed * Time.deltaTime))
            {
                if (hit.collider.TryGetComponent<EnemyController>(out var enemy))
                {
                    enemy.ApplySleepEffect(_sleepDuration);
                }
                Destroy(gameObject);
            }
        }
    }

    public struct SleepDartUsedEvent
    {
        public Vector3 startPosition;
        public Vector3 direction;
        public float sleepDuration;
    }
}
