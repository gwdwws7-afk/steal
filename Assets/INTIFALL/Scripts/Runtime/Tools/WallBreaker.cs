using UnityEngine;
using INTIFALL.Environment;
using INTIFALL.System;

namespace INTIFALL.Tools
{
    public class WallBreaker : ToolBase
    {
        [Header("WallBreak Specific")]
        [SerializeField] private float breakTime = 3f;
        [SerializeField] private float range = 2f;
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
            toolNameCN = "凿石礼";
            category = EToolCategory.Environmental;
            defaultSlot = EToolSlot.Slot2;
            cooldown = 10f;
            maxAmmo = 3;
            _currentAmmo = maxAmmo;
            range = 2f;
        }

        public override bool CanUse()
        {
            if (_isBreaking) return false;
            if (!base.CanUse()) return false;
            return FindBreakableWall() != null;
        }

        public override void Use()
        {
            _targetWall = FindBreakableWall();
            if (_targetWall == null) return;

            _isBreaking = true;
            _breakProgress = 0f;
            _currentAmmo--;

            EventBus.Publish(new WallBreakStartedByToolEvent
            {
                position = _targetWall.transform.position
            });
        }

        private void Update()
        {
            if (!_isBreaking) return;
            if (_targetWall == null)
            {
                CancelBreak();
                return;
            }

            if (Vector3.Distance(transform.position, _targetWall.transform.position) > range)
            {
                CancelBreak();
                return;
            }

            _breakProgress += Time.deltaTime / breakTime;

            if (_breakProgress >= 1f)
            {
                CompleteBreak();
            }
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
            _isOnCooldown = true;
        }

        private void CancelBreak()
        {
            _isBreaking = false;
            _targetWall = null;
            _breakProgress = 0f;

            if (_currentAmmo < maxAmmo)
                _currentAmmo++;
        }

        private BreakableWall FindBreakableWall()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, range);
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