using UnityEngine;
using INTIFALL.System;

namespace INTIFALL.Tools
{
    public enum EToolCategory
    {
        PerceptionDisrupt,
        AttentionShift,
        DirectRemove,
        Environmental
    }

    public enum EToolSlot
    {
        Slot1,
        Slot2,
        Slot3,
        Slot4
    }

    public abstract class ToolBase : MonoBehaviour
    {
        [Header("Basic Info")]
        public string toolName;
        public string toolNameCN;
        public EToolCategory category;
        public EToolSlot defaultSlot;

        [Header("Combat Stats")]
        public int damage = 0;
        public float range = 0f;
        public float cooldown = 0f;
        public float duration = 0f;

        [Header("Resource Cost")]
        public int ammo = 0;
        public int maxAmmo = 0;
        public float energyCost = 0f;

        [Header("Audio/Visual")]
        public AudioClip useSound;
        public GameObject vfxPrefab;

        protected ToolManager _manager;
        protected float _currentCooldown;
        protected int _currentAmmo;
        protected bool _isOnCooldown;

        public float CurrentCooldown => _currentCooldown;
        public bool IsOnCooldown => _isOnCooldown;
        public int CurrentAmmo => _currentAmmo;
        public float CooldownProgress => ResolveCooldownProgress();
        protected bool HasLimitedAmmo => maxAmmo > 0 || ammo > 0;

        public virtual void ApplyToolData(ToolData data)
        {
            if (data == null)
                return;

            if (!string.IsNullOrWhiteSpace(data.toolName))
                toolName = data.toolName;
            if (!string.IsNullOrWhiteSpace(data.toolNameCN))
                toolNameCN = data.toolNameCN;

            category = data.category;
            defaultSlot = data.defaultSlot;

            damage = data.damage;
            range = Mathf.Max(0f, data.range);
            cooldown = Mathf.Max(0f, data.cooldown);
            duration = Mathf.Max(0f, data.duration);

            maxAmmo = Mathf.Max(0, data.maxAmmo);
            ammo = maxAmmo;
            energyCost = Mathf.Max(0f, data.energyCost);

            OnApplyToolData(data);
        }

        public virtual void Initialize(ToolManager manager)
        {
            _manager = manager;
            _currentAmmo = HasLimitedAmmo ? GetAmmoCapacity() : 0;
            _currentCooldown = 0f;
            _isOnCooldown = false;
        }

        public virtual bool CanUse()
        {
            if (_isOnCooldown) return false;
            if (HasLimitedAmmo && _currentAmmo <= 0) return false;
            return true;
        }

        public virtual void Use()
        {
            if (!CanUse()) return;

            if (HasLimitedAmmo)
                _currentAmmo = Mathf.Max(0, _currentAmmo - 1);

            _currentCooldown = Mathf.Max(0f, cooldown);
            _isOnCooldown = _currentCooldown > 0f;

            OnToolUsed();

            EventBus.Publish(new ToolUsedEvent
            {
                toolName = toolName,
                category = category,
                cooldownSeconds = Mathf.Max(0f, cooldown)
            });
        }

        protected virtual void OnToolUsed() { }
        protected virtual void OnApplyToolData(ToolData data) { }

        private static float ResolveDeltaTime()
        {
            if (!Application.isPlaying)
                return 0.1f;

            return Time.deltaTime;
        }

        public virtual void Update()
        {
            if (_isOnCooldown)
            {
                _currentCooldown -= ResolveDeltaTime();
                if (_currentCooldown <= 0f)
                {
                    _currentCooldown = 0f;
                    _isOnCooldown = false;
                }
            }
        }

        public void Reload(int amount)
        {
            if (!HasLimitedAmmo)
                return;

            _currentAmmo = Mathf.Min(_currentAmmo + amount, GetAmmoCapacity());
        }

        public void ResetCooldown()
        {
            _currentCooldown = 0f;
            _isOnCooldown = false;
        }

        private float ResolveCooldownProgress()
        {
            if (!_isOnCooldown)
                return 1f;

            if (cooldown <= 0f)
                return 1f;

            return Mathf.Clamp01(_currentCooldown / cooldown);
        }

        private int GetAmmoCapacity()
        {
            return Mathf.Max(0, Mathf.Max(maxAmmo, ammo));
        }
    }

    public struct ToolUsedEvent
    {
        public string toolName;
        public EToolCategory category;
        public float cooldownSeconds;
    }

    public struct ToolEquippedEvent
    {
        public string toolName;
        public EToolSlot slot;
    }

    public struct ToolEquipRejectedEvent
    {
        public string toolName;
        public int requestedSlotIndex;
        public int slotCost;
        public int remainingCapacity;
        public int maxCapacity;
        public string reason;
    }
}
