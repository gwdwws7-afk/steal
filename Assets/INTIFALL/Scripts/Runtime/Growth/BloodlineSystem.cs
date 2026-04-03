using INTIFALL.System;
using UnityEngine;

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
        [SerializeField] private int currentLevel;

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
        [SerializeField] private bool hasReincarnationBonus;

        private EBloodlinePassive[] _unlockedPassives;
        private int _maxLevel = 5;

        public int CurrentLevel => currentLevel;

        public EBloodlinePassive[] UnlockedPassives
        {
            get
            {
                EnsureInitialized();
                return _unlockedPassives;
            }
        }

        private void EnsureInitialized()
        {
            if (_unlockedPassives == null)
                _unlockedPassives = new EBloodlinePassive[0];
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        public bool HasPassive(EBloodlinePassive passive)
        {
            EnsureInitialized();
            for (int i = 0; i < _unlockedPassives.Length; i++)
            {
                if (_unlockedPassives[i] == passive)
                    return true;
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

        public void UnlockPassiveForLevel(int level)
        {
            EnsureInitialized();
            if (level < 1 || level > _maxLevel)
                return;
            if (level <= currentLevel)
                return;

            EBloodlinePassive passive = GetPassiveForLevel(level);
            if (passive == EBloodlinePassive.None)
                return;

            global::System.Array.Resize(ref _unlockedPassives, _unlockedPassives.Length + 1);
            _unlockedPassives[_unlockedPassives.Length - 1] = passive;
            currentLevel = level;

            EventBus.Publish(new BloodlineUnlockedEvent
            {
                passive = passive,
                level = level
            });
        }

        private static EBloodlinePassive GetPassiveForLevel(int level)
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
            if (!HasPassive(EBloodlinePassive.AndesBreath))
                return;

            EventBus.Publish(new BloodlineEffectAppliedEvent
            {
                passive = EBloodlinePassive.AndesBreath,
                effectType = "NoiseReduction",
                value = crouchNoiseReduction
            });
        }

        public void ApplyPriestEyeEffect()
        {
            if (!HasPassive(EBloodlinePassive.PriestEye))
                return;

            EventBus.Publish(new BloodlineEffectAppliedEvent
            {
                passive = EBloodlinePassive.PriestEye,
                effectType = "TerminalHackSpeed",
                value = terminalHackSpeedBonus
            });
        }

        public void ApplyGoldenBloodEffect()
        {
            if (!HasPassive(EBloodlinePassive.GoldenBlood))
                return;

            EventBus.Publish(new BloodlineEffectAppliedEvent
            {
                passive = EBloodlinePassive.GoldenBlood,
                effectType = "EMPDuration",
                value = empDurationBonus
            });
        }

        public void ApplyLeechSenseEffect()
        {
            if (!HasPassive(EBloodlinePassive.LeechSense))
                return;

            EventBus.Publish(new BloodlineEffectAppliedEvent
            {
                passive = EBloodlinePassive.LeechSense,
                effectType = "SaqueosWarning",
                value = saqueosWarningTime
            });
        }

        public void ApplyReincarnationEndEffect()
        {
            if (!HasPassive(EBloodlinePassive.ReincarnationEnd))
                return;

            EventBus.Publish(new BloodlineEffectAppliedEvent
            {
                passive = EBloodlinePassive.ReincarnationEnd,
                effectType = "EndingBonus",
                value = 1f
            });
        }

        public void ResetBloodline()
        {
            EnsureInitialized();
            currentLevel = 0;
            _unlockedPassives = new EBloodlinePassive[0];
            hasReincarnationBonus = false;
        }

        public string GetPassiveName(EBloodlinePassive passive)
        {
            return passive switch
            {
                EBloodlinePassive.AndesBreath => "Andes Breath",
                EBloodlinePassive.PriestEye => "Priest Eye",
                EBloodlinePassive.GoldenBlood => "Golden Blood",
                EBloodlinePassive.LeechSense => "Leech Sense",
                EBloodlinePassive.ReincarnationEnd => "Reincarnation End",
                _ => string.Empty
            };
        }

        public string GetPassiveDescription(EBloodlinePassive passive)
        {
            return passive switch
            {
                EBloodlinePassive.AndesBreath => "Crouch and rope noise reduced by 20%.",
                EBloodlinePassive.PriestEye => "Terminal breach speed increased by 30%.",
                EBloodlinePassive.GoldenBlood => "EMP disable duration increased by 4 seconds.",
                EBloodlinePassive.LeechSense => "Saqueos warning triggered 5 seconds earlier within 20m.",
                EBloodlinePassive.ReincarnationEnd => "Final choice grants an additional ending-side effect.",
                _ => string.Empty
            };
        }
    }
}
