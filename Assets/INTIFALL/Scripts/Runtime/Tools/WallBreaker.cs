using UnityEngine;
using INTIFALL.Audio;
using INTIFALL.Environment;
using INTIFALL.System;

namespace INTIFALL.Tools
{
    public class WallBreaker : ToolBase
    {
        [Header("WallBreak Specific")]
        [SerializeField] private float breakTime = 2.6f;
        [SerializeField] private float interactionRange = 2.4f;
        [SerializeField] private AudioClip[] breakSounds;
        [SerializeField] private GameObject breakEffectPrefab;

        private BreakableWall _targetWall;
        private bool _isBreaking;
        private float _breakProgress;

        public float BreakProgress => _breakProgress;
        public bool IsBreaking => _isBreaking;

        private void Awake()
        {
            toolName = "WallBreaker";
            toolNameCN = "Wall Breaker";
            category = EToolCategory.Environmental;
            defaultSlot = EToolSlot.Slot2;
            cooldown = 7f;
            maxAmmo = 2;
            _currentAmmo = maxAmmo;
            range = 2.4f;
            duration = 2.6f;
        }

        public override bool CanUse()
        {
            if (_isBreaking)
                return false;
            if (!base.CanUse())
                return false;

            float effectiveRange = range > 0f ? range : interactionRange;
            return FindBreakableWall(effectiveRange) != null;
        }

        public override void Use()
        {
            float effectiveRange = range > 0f ? range : interactionRange;
            _targetWall = FindBreakableWall(effectiveRange);
            if (_targetWall == null)
                return;

            _isBreaking = true;
            _breakProgress = 0f;
            if (HasLimitedAmmo)
                _currentAmmo = Mathf.Max(0, _currentAmmo - 1);

            EventBus.Publish(new WallBreakStartedByToolEvent
            {
                position = _targetWall.transform.position
            });
        }

        public override void Update()
        {
            base.Update();

            if (!_isBreaking)
                return;
            if (_targetWall == null)
            {
                CancelBreak();
                return;
            }

            float effectiveBreakTime = duration > 0f ? duration : breakTime;
            float effectiveRange = range > 0f ? range : interactionRange;
            if (Vector3.Distance(transform.position, _targetWall.transform.position) > effectiveRange)
            {
                CancelBreak();
                return;
            }

            _breakProgress += Time.deltaTime / Mathf.Max(0.1f, effectiveBreakTime);

            if (_breakProgress >= 1f)
            {
                CompleteBreak();
            }
        }

        protected override void OnApplyToolData(ToolData data)
        {
            if (data.duration > 0f)
                breakTime = data.duration;
            if (data.range > 0f)
                interactionRange = data.range;
        }

        private void CompleteBreak()
        {
            if (_targetWall != null)
            {
                _targetWall.Hit();
                _targetWall.StartBreaking();

                if (breakEffectPrefab != null)
                {
                    Instantiate(breakEffectPrefab, _targetWall.transform.position, Quaternion.identity);
                }

                AudioClip sound = breakSounds != null && breakSounds.Length > 0
                    ? breakSounds[Random.Range(0, breakSounds.Length)]
                    : null;
                AudioManager.Instance?.PlaySFX(sound);

                EventBus.Publish(new WallBrokenByToolEvent
                {
                    position = _targetWall.transform.position
                });
            }

            _isBreaking = false;
            _targetWall = null;
            _breakProgress = 0f;
            _currentCooldown = cooldown;
            _isOnCooldown = _currentCooldown > 0f;
        }

        private void CancelBreak()
        {
            _isBreaking = false;
            _targetWall = null;
            _breakProgress = 0f;

            if (HasLimitedAmmo && _currentAmmo < maxAmmo)
                _currentAmmo++;
        }

        private BreakableWall FindBreakableWall(float scanRange)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, scanRange);
            foreach (Collider hit in hits)
            {
                var wall = hit.GetComponent<BreakableWall>();
                if (wall != null && !wall.IsBroken)
                    return wall;
            }

            return null;
        }

        public void Cancel()
        {
            CancelBreak();
        }
    }

    public struct WallBreakStartedByToolEvent
    {
        public Vector3 position;
    }

    public struct WallBrokenByToolEvent
    {
        public Vector3 position;
    }
}
