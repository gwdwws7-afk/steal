using UnityEngine;
using INTIFALL.System;

namespace INTIFALL.Growth
{
    public enum EBloodlinePassive
    {
        None,
        AndesBreath,
        PriestEye,
        GoldenBlood,
        LeechSense,
        ReincarnationEnd
    }

    public struct BloodlineUnlockedEvent
    {
        public EBloodlinePassive passive;
        public int level;
    }

    public struct BloodlineEffectAppliedEvent
    {
        public EBloodlinePassive passive;
        public string effectType;
        public float value;
    }

    public class BloodlineSystem : MonoBehaviour
    {
        [Header("Passive Configuration")]
        [SerializeField] private int currentLevel = 0;

        [Header("Andes Breath (L01)")]
        [SerializeField] private float crouchNoiseReduction = 0.2f;
        [SerializeField] private float ropeNoiseReduction = 0.2f;

        [Header("Priest Eye (L02)")]
        [SerializeField] private float terminalHackSpeedBonus = 0.3f;

        [Header("Golden Blood (L03)")]
        [SerializeField] private float empDurationBonus = 4f;

        [Header("Leech Sense (L04)")]
        [SerializeField] private float saqueosWarningDistance = 20f;
        [SerializeField] private float saqueosWarningTime = 5f;

        [Header("Reincarnation End (L05)")]
        [SerializeField] private bool hasReincarnationBonus = false;

        private EBloodlinePassive[] _unlockedPassives;
        private int _maxLevel = 5;

        public int CurrentLevel => currentLevel;
        public EBloodlinePassive[] UnlockedPassives => _unlockedPassives;

        public bool HasPassive(EBloodlinePassive passive)
        {
            for (int i = 0; i < _unlockedPassives.Length; i++)
            {
                if (_unlockedPassives[i] == passive) return true;
            }
            return false;
        }

        public float GetCrouchNoiseMultiplier()
        {
            return HasPassive(EBloodlinePassive.AndesBreath) ? (1f - crouchNoiseReduction) : 1f;
        }

        public float GetRopeNoiseMultiplier()
        {
            return HasPassive(EBloodlinePassive.AndesBreath) ? (1f - ropeNoiseReduction) : 1f;
        }

        public float GetTerminalHackSpeedMultiplier()
        {
            return HasPassive(EBloodlinePassive.PriestEye) ? (1f + terminalHackSpeedBonus) : 1f;
        }

        public float GetEMPDisabledDurationBonus()
        {
            return HasPassive(EBloodlinePassive.GoldenBlood) ? empDurationBonus : 0f;
        }

        public float GetSaqueosWarningDistance()
        {
            return HasPassive(EBloodlinePassive.LeechSense) ? saqueosWarningDistance : 0f;
        }

        public float GetSaqueosWarningTime()
        {
            return HasPassive(EBloodlinePassive.LeechSense) ? saqueosWarningTime : 0f;
        }

        public bool HasReincarnationEndBonus()
        {
            return hasReincarnationBonus && HasPassive(EBloodlinePassive.ReincarnationEnd);
        }

        private void Awake()
        {
            _unlockedPassives = new EBloodlinePassive[0];
        }

        public void UnlockPassiveForLevel(int level)
        {
            if (level < 1 || level > _maxLevel) return;
            if (level <= currentLevel) return;

            EBloodlinePassive passive = GetPassiveForLevel(level);
            if (passive == EBloodlinePassive.None) return;

            System.Array.Resize(ref _unlockedPassives, _unlockedPassives.Length + 1);
            _unlockedPassives[_unlockedPassives.Length - 1] = passive;

            currentLevel = level;

            EventBus.Publish(new BloodlineUnlockedEvent
            {
                passive = passive,
                level = level
            });
        }

        private EBloodlinePassive GetPassiveForLevel(int level)
        {
            return level switch
            {
                1 => EBloodlinePassive.AndesBreath,
                2 => EBloodlinePassive.PriestEye,
                3 => EBloodlinePassive.GoldenBlood,
                4 => EBloodlinePassive.LeechSense,
                5 => EBloodlinePassive.ReincarnationEnd,
                _ => EBloodlinePassive.None
            };
        }

        public void ApplyAndesBreathEffect()
        {
            if (!HasPassive(EBloodlinePassive.AndesBreath)) return;

            EventBus.Publish(new BloodlineEffectAppliedEvent
            {
                passive = EBloodlinePassive.AndesBreath,
                effectType = "NoiseReduction",
                value = crouchNoiseReduction
            });
        }

        public void ApplyPriestEyeEffect()
        {
            if (!HasPassive(EBloodlinePassive.PriestEye)) return;

            EventBus.Publish(new BloodlineEffectAppliedEvent
            {
                passive = EBloodlinePassive.PriestEye,
                effectType = "TerminalHackSpeed",
                value = terminalHackSpeedBonus
            });
        }

        public void ApplyGoldenBloodEffect()
        {
            if (!HasPassive(EBloodlinePassive.GoldenBlood)) return;

            EventBus.Publish(new BloodlineEffectAppliedEvent
            {
                passive = EBloodlinePassive.GoldenBlood,
                effectType = "EMPDuration",
                value = empDurationBonus
            });
        }

        public void ApplyLeechSenseEffect()
        {
            if (!HasPassive(EBloodlinePassive.LeechSense)) return;

            EventBus.Publish(new BloodlineEffectAppliedEvent
            {
                passive = EBloodlinePassive.LeechSense,
                effectType = "SaqueosWarning",
                value = saqueosWarningTime
            });
        }

        public void ApplyReincarnationEndEffect()
        {
            if (!HasPassive(EBloodlinePassive.ReincarnationEnd)) return;

            EventBus.Publish(new BloodlineEffectAppliedEvent
            {
                passive = EBloodlinePassive.ReincarnationEnd,
                effectType = "EndingBonus",
                value = 1f
            });
        }

        public void ResetBloodline()
        {
            currentLevel = 0;
            _unlockedPassives = new EBloodlinePassive[0];
            hasReincarnationBonus = false;
        }

        public string GetPassiveName(EBloodlinePassive passive)
        {
            return passive switch
            {
                EBloodlinePassive.AndesBreath => "安第斯之息",
                EBloodlinePassive.PriestEye => "祭司之眼",
                EBloodlinePassive.GoldenBlood => "黄金之血",
                EBloodlinePassive.LeechSense => "蚂蟥感知",
                EBloodlinePassive.ReincarnationEnd => "轮回终章",
                _ => ""
            };
        }

        public string GetPassiveDescription(EBloodlinePassive passive)
        {
            return passive switch
            {
                EBloodlinePassive.AndesBreath => "匍匐/绳技噪音 -20%",
                EBloodlinePassive.PriestEye => "终端破解速度 +30%",
                EBloodlinePassive.GoldenBlood => "EMP 禁用时间 +4s",
                EBloodlinePassive.LeechSense => "Saqueos 20m 内提前 5s 预警",
                EBloodlinePassive.ReincarnationEnd => "终局三选一产生额外效果",
                _ => ""
            };
        }
    }
}