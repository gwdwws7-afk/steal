using INTIFALL.Economy;
using INTIFALL.System;
using UnityEngine;

namespace INTIFALL.Growth
{
    public struct LevelCompleteEvent
    {
        public int level;
        public int rankScore;
        public int creditsEarned;
        public string[] toolsUnlocked;
        public string passiveUnlocked;
    }

    public class LevelUpReward : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CreditSystem creditSystem;
        [SerializeField] private ProgressionTree progressionTree;
        [SerializeField] private BloodlineSystem bloodlineSystem;

        [Header("Mission Evaluation")]
        [SerializeField] private bool zeroKill = true;
        [SerializeField] private bool noDamage = true;
        [SerializeField] private int secondaryObjectivesCompleted;
        [SerializeField] private int intelCollected;
        [SerializeField] private float timeRemaining;

        private int _currentLevel = 1;

        public void SetMissionStats(bool zeroKillValue, bool noDamageValue, int secondaryObjectives, int intel, float timeLeft)
        {
            zeroKill = zeroKillValue;
            noDamage = noDamageValue;
            secondaryObjectivesCompleted = secondaryObjectives;
            intelCollected = intel;
            timeRemaining = timeLeft;
        }

        public int CalculateRank()
        {
            return EvaluateRankScore(
                zeroKill,
                noDamage,
                secondaryObjectivesCompleted,
                intelCollected,
                timeRemaining);
        }

        public static int EvaluateRankScore(
            bool zeroKillValue,
            bool noDamageValue,
            int secondaryObjectivesValue,
            int intelCollectedValue,
            float timeRemainingValue)
        {
            int secondary = Mathf.Max(0, secondaryObjectivesValue);
            int intel = Mathf.Max(0, intelCollectedValue);
            float time = Mathf.Max(0f, timeRemainingValue);

            if (zeroKillValue && noDamageValue && secondary >= 2 && intel >= 3)
                return 5;

            if (noDamageValue && secondary >= 1 && intel >= 2)
                return 4;

            if ((secondary >= 1 && (time > 0f || intel >= 1)) || (zeroKillValue && intel >= 2))
                return 3;

            if (secondary >= 1 || intel >= 1)
                return 2;

            return 1;
        }

        public static int EvaluateCredits(
            int rankScore,
            bool zeroKillValue,
            bool noDamageValue,
            int secondaryObjectivesValue,
            int intelCollectedValue,
            float timeRemainingValue)
        {
            int baseCredits = rankScore switch
            {
                5 => 820,
                4 => 560,
                3 => 340,
                2 => 185,
                _ => 95
            };

            int total = baseCredits;
            if (zeroKillValue) total += 140;
            if (noDamageValue) total += 160;

            if (timeRemainingValue > 180f)
                total += 130;
            else if (timeRemainingValue > 60f)
                total += 70;

            total += Mathf.Clamp(secondaryObjectivesValue, 0, 3) * 55;
            total += Mathf.Clamp(intelCollectedValue, 0, 5) * 45;
            return total;
        }

        public static string GetRankNameForScore(int rankScore)
        {
            return rankScore switch
            {
                5 => "S",
                4 => "A",
                3 => "B",
                2 => "C",
                _ => "D"
            };
        }

        public void CompleteLevel()
        {
            int rank = CalculateRank();
            int totalCredits = EvaluateCredits(
                rank,
                zeroKill,
                noDamage,
                secondaryObjectivesCompleted,
                intelCollected,
                timeRemaining);

            if (creditSystem != null)
                creditSystem.EarnCredits(totalCredits, $"Level_{_currentLevel}");

            progressionTree?.CompleteLevel(_currentLevel);
            bloodlineSystem?.UnlockPassiveForLevel(_currentLevel);

            EventBus.Publish(new LevelCompleteEvent
            {
                level = _currentLevel,
                rankScore = rank,
                creditsEarned = totalCredits,
                toolsUnlocked = GetToolsUnlockedAtLevel(_currentLevel),
                passiveUnlocked = bloodlineSystem != null
                    ? bloodlineSystem.GetPassiveName((EBloodlinePassive)_currentLevel)
                    : string.Empty
            });

            _currentLevel++;
        }

        private static string[] GetToolsUnlockedAtLevel(int level)
        {
            return level switch
            {
                1 => new[] { "SmokeBomb", "FlashBang", "SoundBait", "SleepDart", "Rope" },
                2 => new[] { "TimedNoise", "WallBreaker" },
                3 => new[] { "EMP", "DroneInterference" },
                4 => new[] { "SmokeBomb_Mk2", "FlashBang_Mk2", "SoundBait_Mk2" },
                5 => new[] { "AllToolsMaxed" },
                _ => new string[0]
            };
        }

        public void SetCurrentLevel(int level)
        {
            _currentLevel = level;
        }

        public int GetCurrentLevel()
        {
            return _currentLevel;
        }

        public string GetRankName(int rank)
        {
            return GetRankNameForScore(rank);
        }

        public string GetRankDescription(int rank)
        {
            return rank switch
            {
                5 => "Zero-kill, no-damage, with full objective control.",
                4 => "Clean execution with strong objective completion.",
                3 => "Stable completion with partial objective coverage.",
                2 => "Mission completed under pressure.",
                _ => "Mission completed."
            };
        }
    }
}
