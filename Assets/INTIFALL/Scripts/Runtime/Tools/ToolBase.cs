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
        public float CooldownProgress => _isOnCooldown ? _currentCooldown / cooldown : 1f;

        public virtual void Initialize(ToolManager manager)
        {
            _manager = manager;
            _currentAmmo = maxAmmo;
            _currentCooldown = 0f;
            _isOnCooldown = false;
        }

        public virtual bool CanUse()
        {
            if (_isOnCooldown) return false;
            if (ammo > 0 && _currentAmmo <= 0) return false;
            return true;
        }

        public virtual void Use()
        {
            if (!CanUse()) return;

            _currentAmmo--;
            _currentCooldown = cooldown;
            _isOnCooldown = true;

            OnToolUsed();

            EventBus.Publish(new ToolUsedEvent
            {
                toolName = toolName,
                category = category
            });
        }

        protected virtual void OnToolUsed() { }

        public virtual void Update()
        {
            if (_isOnCooldown)
            {
                _currentCooldown -= Time.deltaTime;
                if (_currentCooldown <= 0f)
                {
                    _currentCooldown = 0f;
                    _isOnCooldown = false;
                }
            }
        }

        public void Reload(int amount)
        {
            _currentAmmo = Mathf.Min(_currentAmmo + amount, maxAmmo);
        }

        public void ResetCooldown()
        {
            _currentCooldown = 0f;
            _isOnCooldown = false;
        }
    }

    public struct ToolUsedEvent
    {
        public string toolName;
        public EToolCategory category;
    }

    public struct ToolEquippedEvent
    {
        public string toolName;
        public EToolSlot slot;
    }
}
