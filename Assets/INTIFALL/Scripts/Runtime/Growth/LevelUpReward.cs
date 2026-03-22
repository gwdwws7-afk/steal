using UnityEngine;
using INTIFALL.Economy;
using INTIFALL.System;

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
        [SerializeField] private int secondaryObjectivesCompleted = 0;
        [SerializeField] private int intelCollected = 0;
        [SerializeField] private float timeRemaining = 0f;

        private int _currentLevel = 1;

        public void SetMissionStats(bool zeroKill, bool noDamage, int secondaryObjectives, int intel, float timeLeft)
        {
            this.zeroKill = zeroKill;
            this.noDamage = noDamage;
            this.secondaryObjectivesCompleted = secondaryObjectives;
            this.intelCollected = intel;
            this.timeRemaining = timeLeft;
        }

        public int CalculateRank()
        {
            if (zeroKill && noDamage && secondaryObjectivesCompleted >= 2 && intelCollected >= 3)
                return 5;
            if (noDamage && secondaryObjectivesCompleted >= 1 && intelCollected >= 2)
                return 4;
            if (!zeroKill && secondaryObjectivesCompleted >= 1)
                return 3;
            if (secondaryObjectivesCompleted >= 1)
                return 2;
            return 1;
        }

        public void CompleteLevel()
        {
            int rank = CalculateRank();

            int baseCredits = rank switch
            {
                5 => 500,
                4 => 350,
                3 => 200,
                2 => 100,
                _ => 50
            };

            int totalCredits = CalculateTotalCredits(baseCredits);

            if (creditSystem != null)
            {
                creditSystem.EarnCredits(totalCredits, $"Level_{_currentLevel}");
            }

            progressionTree?.CompleteLevel(_currentLevel);

            bloodlineSystem?.UnlockPassiveForLevel(_currentLevel);

            EventBus.Publish(new LevelCompleteEvent
            {
                level = _currentLevel,
                rankScore = rank,
                creditsEarned = totalCredits,
                toolsUnlocked = GetToolsUnlockedAtLevel(_currentLevel),
                passiveUnlocked = bloodlineSystem != null ? bloodlineSystem.GetPassiveName((EBloodlinePassive)_currentLevel) : ""
            });

            _currentLevel++;
        }

        private int CalculateTotalCredits(int baseCredits)
        {
            int total = baseCredits;

            if (zeroKill) total += 150;
            if (noDamage) total += 200;
            if (timeRemaining > 60) total += 100;
            total += secondaryObjectivesCompleted * 50;
            total += intelCollected * 50;

            return total;
        }

        private string[] GetToolsUnlockedAtLevel(int level)
        {
            return level switch
            {
                1 => new[] { "烟雾弹", "闪光弹", "声音诱饵", "睡眠弹", "绳技" },
                2 => new[] { "定时噪音", "墙壁破坏" },
                3 => new[] { "EMP", "无人机" },
                4 => new[] { "烟雾弹升级", "闪光弹升级", "声音诱饵升级" },
                5 => new[] { "所有工具满配" },
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
            return rank switch
            {
                5 => "S",
                4 => "A",
                3 => "B",
                2 => "C",
                _ => "D"
            };
        }

        public string GetRankDescription(int rank)
        {
            return rank switch
            {
                5 => "零击杀 + 未被发现 + 次要目标全完成",
                4 => "未被发现 + 次要目标完成",
                3 => "未触发全面警报",
                2 => "触发全面警报但完成",
                _ => "任务完成"
            };
        }
    }
}