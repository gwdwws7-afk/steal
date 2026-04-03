using UnityEngine;
using INTIFALL.Input;
using INTIFALL.AI;
using INTIFALL.System;

namespace INTIFALL.Tools
{
    public class SleepDart : ToolBase
    {
        [Header("SleepDart Specific")]
        [SerializeField] private float sleepDuration = 16f;
        [SerializeField] private float projectileSpeed = 30f;
        [SerializeField] private float maxTravelDistance = 18f;
        [SerializeField] private GameObject dartPrefab;

        private void Awake()
        {
            toolName = "SleepDart";
            toolNameCN = "Sleep Dart";
            category = EToolCategory.DirectRemove;
            defaultSlot = EToolSlot.Slot3;
            cooldown = 4f;
            maxAmmo = 5;
            _currentAmmo = maxAmmo;
            range = 18f;
            duration = 16f;
            damage = 0;
        }

        public override void Use()
        {
            if (!CanUse())
                return;

            float effectiveSleepDuration = duration > 0f ? duration : sleepDuration;
            float effectiveTravelDistance = range > 0f ? range : maxTravelDistance;
            Vector3 targetDir = GetAimDirection();
            FireDart(targetDir, effectiveSleepDuration, effectiveTravelDistance);

            if (HasLimitedAmmo)
                _currentAmmo = Mathf.Max(0, _currentAmmo - 1);

            _currentCooldown = Mathf.Max(0f, cooldown);
            _isOnCooldown = _currentCooldown > 0f;

            EventBus.Publish(new ToolUsedEvent
            {
                toolName = toolName,
                category = category
            });

            EventBus.Publish(new SleepDartUsedEvent
            {
                startPosition = transform.position,
                direction = targetDir,
                sleepDuration = effectiveSleepDuration
            });
        }

        protected override void OnApplyToolData(ToolData data)
        {
            if (data.duration > 0f)
                sleepDuration = data.duration;
            if (data.range > 0f)
                maxTravelDistance = data.range;
        }

        private Vector3 GetAimDirection()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Ray ray = cam.ScreenPointToRay(InputCompat.MousePosition);
                return ray.direction.normalized;
            }

            return transform.forward;
        }

        private void FireDart(Vector3 direction, float appliedSleepDuration, float appliedTravelDistance)
        {
            if (dartPrefab == null)
                return;

            GameObject dart = Instantiate(dartPrefab, transform.position, Quaternion.LookRotation(direction));
            SleepDartProjectile proj = dart.GetComponent<SleepDartProjectile>();
            if (proj != null)
            {
                proj.Initialize(direction, projectileSpeed, appliedSleepDuration, appliedTravelDistance);
            }
        }
    }

    public class SleepDartProjectile : MonoBehaviour
    {
        private Vector3 _direction;
        private float _speed;
        private float _sleepDuration;
        private float _maxTravelDistance;
        private float _travelDistance;
        private float _lifetime;

        public void Initialize(Vector3 direction, float speed, float sleepDuration, float maxTravelDistance)
        {
            _direction = direction;
            _speed = speed;
            _sleepDuration = sleepDuration;
            _maxTravelDistance = Mathf.Max(1f, maxTravelDistance);
            _travelDistance = 0f;
            _lifetime = 0f;
        }

        private void Update()
        {
            float step = _speed * Time.deltaTime;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, _direction, out hit, step))
            {
                if (hit.collider.TryGetComponent<EnemyController>(out var enemy))
                {
                    enemy.ApplySleepEffect(_sleepDuration);
                }

                Destroy(gameObject);
                return;
            }

            transform.position += _direction * step;
            _travelDistance += step;
            _lifetime += Time.deltaTime;

            if (_travelDistance >= _maxTravelDistance || _lifetime > 5f)
                Destroy(gameObject);
        }
    }

    public struct SleepDartUsedEvent
    {
        public Vector3 startPosition;
        public Vector3 direction;
        public float sleepDuration;
    }
}
