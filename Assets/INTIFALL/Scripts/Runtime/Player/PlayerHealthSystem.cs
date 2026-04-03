using UnityEngine;
using INTIFALL.System;

namespace INTIFALL.Player
{
    public enum EHPEventType
    {
        Damaged,
        Healed,
        Died,
        ForcedReviveUsed
    }

    public struct HPChangedEvent
    {
        public int currentHP;
        public int maxHP;
        public int changeAmount;
        public bool isHealing;
    }

    public struct PlayerDiedEvent
    {
        public int lastDamageSource;
    }

    public struct PlayerForcedReviveEvent
    {
        public int revivesRemaining;
    }

    public class PlayerHealthSystem : MonoBehaviour
    {
        [Header("HP Settings")]
        [SerializeField] private int maxHP = 5;
        [SerializeField] private int currentHP;

        [Header("First Aid")]
        [SerializeField] private int firstAidCount = 5;
        [SerializeField] private int firstAidRestoreAmount = 5;
        [SerializeField] private float firstAidUseDelay = 2f;
        [SerializeField] private float firstAidChannelTime = 1.5f;

        [Header("Forced Revive")]
        [SerializeField] private int forcedReviveCount = 1;
        [SerializeField] private bool forcedReviveAvailable = true;

        [Header("Invincibility")]
        [SerializeField] private float invincibilityDuration = 0.5f;

        private float _invincibilityTimer;
        private float _firstAidChannelTimer;
        private bool _isUsingFirstAid;
        private float _firstAidDelayTimer;
        private bool _isDead;
        private int _forcedRevivesRemaining;
        private bool _initialized;

        public int MaxHP
        {
            get
            {
                EnsureInitialized();
                return maxHP;
            }
        }

        public int CurrentHP
        {
            get
            {
                EnsureInitialized();
                return currentHP;
            }
        }

        public int FirstAidCount
        {
            get
            {
                EnsureInitialized();
                return firstAidCount;
            }
        }

        public bool IsDead
        {
            get
            {
                EnsureInitialized();
                return _isDead;
            }
        }

        public bool IsUsingFirstAid
        {
            get
            {
                EnsureInitialized();
                return _isUsingFirstAid;
            }
        }

        public bool CanUseFirstAid
        {
            get
            {
                EnsureInitialized();
                return firstAidCount > 0 && currentHP < maxHP && !_isUsingFirstAid && _firstAidDelayTimer <= 0f;
            }
        }

        public float FirstAidProgress
        {
            get
            {
                EnsureInitialized();
                return _isUsingFirstAid ? _firstAidChannelTimer / firstAidChannelTime : 0f;
            }
        }

        public float FirstAidDelayRemaining
        {
            get
            {
                EnsureInitialized();
                return _firstAidDelayTimer;
            }
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;

            if (maxHP <= 0)
                maxHP = 1;

            if (currentHP <= 0)
                currentHP = maxHP;

            _forcedRevivesRemaining = forcedReviveCount;
            forcedReviveAvailable = _forcedRevivesRemaining > 0;
            _initialized = true;
        }

        private static float ResolveDeltaTime()
        {
            if (!Application.isPlaying)
                return 0.1f;

            return Time.deltaTime;
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void Update()
        {
            EnsureInitialized();
            UpdateTimers();
            CheckForcedRevive();
        }

        private void UpdateTimers()
        {
            float delta = ResolveDeltaTime();

            if (_invincibilityTimer > 0)
                _invincibilityTimer -= delta;

            if (_firstAidDelayTimer > 0)
                _firstAidDelayTimer -= delta;

            if (_isUsingFirstAid)
            {
                _firstAidChannelTimer += delta;
                if (_firstAidChannelTimer >= firstAidChannelTime)
                {
                    CompleteFirstAid();
                }
            }
        }

        private void CheckForcedRevive()
        {
            if (_isDead && forcedReviveAvailable && _forcedRevivesRemaining > 0)
            {
                TriggerForcedRevive();
            }
        }

        public void TakeDamage(int amount, int damageSourceId = 0)
        {
            EnsureInitialized();
            if (_isDead) return;
            if (_invincibilityTimer > 0) return;

            currentHP = Mathf.Max(0, currentHP - amount);

            EventBus.Publish(new HPChangedEvent
            {
                currentHP = currentHP,
                maxHP = maxHP,
                changeAmount = amount,
                isHealing = false
            });

            _invincibilityTimer = invincibilityDuration;

            if (currentHP <= 0)
            {
                Die(damageSourceId);
            }
        }

        public void Heal(int amount)
        {
            EnsureInitialized();
            if (_isDead) return;

            int oldHP = currentHP;
            currentHP = Mathf.Min(maxHP, currentHP + amount);
            int actualHeal = currentHP - oldHP;

            if (actualHeal > 0)
            {
                EventBus.Publish(new HPChangedEvent
                {
                    currentHP = currentHP,
                    maxHP = maxHP,
                    changeAmount = actualHeal,
                    isHealing = true
                });
            }
        }

        public void StartFirstAid()
        {
            EnsureInitialized();
            if (!CanUseFirstAid) return;

            _isUsingFirstAid = true;
            _firstAidChannelTimer = 0f;
        }

        public void CancelFirstAid()
        {
            EnsureInitialized();
            _isUsingFirstAid = false;
            _firstAidChannelTimer = 0f;
        }

        private void CompleteFirstAid()
        {
            _isUsingFirstAid = false;
            _firstAidChannelTimer = 0f;
            firstAidCount--;

            Heal(firstAidRestoreAmount);

            _firstAidDelayTimer = firstAidUseDelay;
        }

        private void TriggerForcedRevive()
        {
            _forcedRevivesRemaining--;
            forcedReviveAvailable = _forcedRevivesRemaining > 0;

            currentHP = maxHP;
            _isDead = false;

            EventBus.Publish(new PlayerForcedReviveEvent { revivesRemaining = _forcedRevivesRemaining });
            EventBus.Publish(new HPChangedEvent
            {
                currentHP = currentHP,
                maxHP = maxHP,
                changeAmount = maxHP,
                isHealing = true
            });
        }

        private void Die(int damageSourceId)
        {
            _isDead = true;

            EventBus.Publish(new PlayerDiedEvent { lastDamageSource = damageSourceId });
            EventBus.Publish(new HPChangedEvent
            {
                currentHP = 0,
                maxHP = maxHP,
                changeAmount = 0,
                isHealing = false
            });
        }

        public void ResetForLevel()
        {
            EnsureInitialized();
            currentHP = maxHP;
            firstAidCount = 5;
            _isUsingFirstAid = false;
            _firstAidChannelTimer = 0f;
            _firstAidDelayTimer = 0f;
            _isDead = false;
            _invincibilityTimer = 0f;
            _forcedRevivesRemaining = forcedReviveCount;
            forcedReviveAvailable = _forcedRevivesRemaining > 0;
        }
    }
}
