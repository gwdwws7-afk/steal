using UnityEngine;
using INTIFALL.Tools;
using INTIFALL.System;

namespace INTIFALL.Growth
{
    public struct ToolUpgradePurchasedEvent
    {
        public string toolName;
        public int upgradeLevel;
    }

    public struct ToolUnlockedEvent
    {
        public string toolName;
        public int level;
    }

    public class ProgressionTree : MonoBehaviour
    {
        [Header("Level Progression")]
        [SerializeField] private int currentCompletedLevel = 0;

        [Header("Tool Unlocks")]
        [SerializeField] private string[] level1Unlocks = { "SmokeBomb", "FlashBang", "SoundBait", "SleepDart", "Rope" };
        [SerializeField] private string[] level2Unlocks = { "TimedNoise", "WallBreak" };
        [SerializeField] private string[] level3Unlocks = { "EMP", "Drone" };
        [SerializeField] private string[] level4Unlocks = { "SmokeBombUpgrade", "FlashBangUpgrade", "SoundBaitUpgrade" };
        [SerializeField] private string[] level5Unlocks = { "SmokeBombMax", "SleepDartUpgrade" };

        private System.Collections.Generic.Dictionary<string, int> _toolUpgradeLevels;
        private System.Collections.Generic.HashSet<string> _unlockedTools;

        public int CurrentCompletedLevel => currentCompletedLevel;

        private void Awake()
        {
            _toolUpgradeLevels = new System.Collections.Generic.Dictionary<string, int>();
            _unlockedTools = new System.Collections.Generic.HashSet<string>();
        }

        public void CompleteLevel(int level)
        {
            if (level <= currentCompletedLevel) return;

            currentCompletedLevel = level;

            UnlockToolsForLevel(level);

            EventBus.Publish(new LevelCompletedEvent
            {
                level = level,
                toolsUnlocked = GetToolsUnlockedAtLevel(level)
            });
        }

        private void UnlockToolsForLevel(int level)
        {
            string[] unlocks = level switch
            {
                1 => level1Unlocks,
                2 => level2Unlocks,
                3 => level3Unlocks,
                4 => level4Unlocks,
                5 => level5Unlocks,
                _ => null
            };

            if (unlocks == null) return;

            foreach (var toolName in unlocks)
            {
                if (!_unlockedTools.Contains(toolName))
                {
                    _unlockedTools.Add(toolName);
                    _toolUpgradeLevels[toolName] = 0;

                    EventBus.Publish(new ToolUnlockedEvent
                    {
                        toolName = toolName,
                        level = level
                    });
                }
            }
        }

        private string[] GetToolsUnlockedAtLevel(int level)
        {
            return level switch
            {
                1 => level1Unlocks,
                2 => level2Unlocks,
                3 => level3Unlocks,
                4 => level4Unlocks,
                5 => level5Unlocks,
                _ => new string[0]
            };
        }

        public bool IsToolUnlocked(string toolName)
        {
            return _unlockedTools.Contains(toolName);
        }

        public int GetUpgradeLevel(string toolName)
        {
            if (_toolUpgradeLevels.TryGetValue(toolName, out int level))
                return level;
            return 0;
        }

        public bool CanUpgrade(string toolName)
        {
            if (!IsToolUnlocked(toolName)) return false;

            int currentLvl = GetUpgradeLevel(toolName);
            int maxLevel = GetMaxUpgradeLevel(toolName);

            return currentLvl < maxLevel;
        }

        public int GetMaxUpgradeLevel(string toolName)
        {
            return toolName switch
            {
                "SmokeBomb" => 2,
                "FlashBang" => 2,
                "SoundBait" => 2,
                "SleepDart" => 2,
                "Rope" => 2,
                "TimedNoise" => 2,
                "WallBreak" => 2,
                "EMP" => 2,
                "Drone" => 2,
                _ => 1
            };
        }

        public void UpgradeTool(string toolName)
        {
            if (!CanUpgrade(toolName)) return;

            int newLevel = _toolUpgradeLevels[toolName] + 1;
            _toolUpgradeLevels[toolName] = newLevel;

            EventBus.Publish(new ToolUpgradePurchasedEvent
            {
                toolName = toolName,
                upgradeLevel = newLevel
            });
        }

        public float GetToolStatBonus(string toolName, string statName)
        {
            int level = GetUpgradeLevel(toolName);
            if (level == 0) return 0f;

            return (toolName, statName) switch
            {
                ("SmokeBomb", "Radius") => level * 2f,
                ("FlashBang", "BlindDuration") => level * 0.75f,
                ("SoundBait", "Range") => level * 2.5f,
                ("SleepDart", "SleepDuration") => level * 5f,
                ("Rope", "Speed") => level * 0.3f,
                ("TimedNoise", "MaxDelay") => level * 5f,
                ("WallBreak", "Speed") => level * 0.25f,
                ("EMP", "DisableDuration") => level * 2f,
                ("Drone", "EnergyDuration") => level * 7.5f,
                _ => 0f
            };
        }

        public string GetToolUpgradeName(string toolName)
        {
            int level = GetUpgradeLevel(toolName);
            if (level == 0) return toolName;

            return (toolName, level) switch
            {
                ("SmokeBomb", 1) => "烟雾弹·浓",
                ("SmokeBomb", 2) => "云雾祭礼·浓",
                ("FlashBang", 1) => "太阳刺·耀",
                ("FlashBang", 2) => "太阳刺·极",
                ("SoundBait", 1) => "鸦鸣石·烈",
                ("SoundBait", 2) => "鸦鸣石·极",
                ("SleepDart", 1) => "梦境叶·浓",
                ("SleepDart", 2) => "梦境叶·极",
                ("Rope", 1) => "索命结·速",
                ("Rope", 2) => "索命结·极",
                ("TimedNoise", 1) => "定时噪音·长",
                ("TimedNoise", 2) => "定时噪音·极",
                ("WallBreak", 1) => "凿石礼·速",
                ("WallBreak", 2) => "凿石礼·极",
                ("EMP", 1) => "EMP·充能",
                ("EMP", 2) => "EMP·极",
                ("Drone", 1) => "蜂群碎片·群",
                ("Drone", 2) => "蜂群碎片·速",
                _ => toolName
            };
        }

        public void ResetProgression()
        {
            currentCompletedLevel = 0;
            _toolUpgradeLevels.Clear();
            _unlockedTools.Clear();
        }

        public struct LevelCompletedEvent
        {
            public int level;
            public string[] toolsUnlocked;
        }
    }
}