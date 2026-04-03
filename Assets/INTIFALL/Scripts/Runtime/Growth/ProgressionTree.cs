using INTIFALL.System;
using INTIFALL.Tools;
using UnityEngine;

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
        [SerializeField] private int currentCompletedLevel;

        [Header("Tool Unlocks")]
        [SerializeField] private string[] level1Unlocks = { "SmokeBomb", "FlashBang", "SoundBait", "SleepDart", "Rope" };
        [SerializeField] private string[] level2Unlocks = { "TimedNoise", "WallBreak" };
        [SerializeField] private string[] level3Unlocks = { "EMP", "Drone" };
        [SerializeField] private string[] level4Unlocks = { "SmokeBombUpgrade", "FlashBangUpgrade", "SoundBaitUpgrade" };
        [SerializeField] private string[] level5Unlocks = { "SmokeBombMax", "SleepDartUpgrade" };

        private global::System.Collections.Generic.Dictionary<string, int> _toolUpgradeLevels;
        private global::System.Collections.Generic.HashSet<string> _unlockedTools;

        public int CurrentCompletedLevel => currentCompletedLevel;

        private void EnsureInitialized()
        {
            if (_toolUpgradeLevels == null)
                _toolUpgradeLevels = new global::System.Collections.Generic.Dictionary<string, int>();
            if (_unlockedTools == null)
                _unlockedTools = new global::System.Collections.Generic.HashSet<string>();
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        public void CompleteLevel(int level)
        {
            EnsureInitialized();
            if (level <= currentCompletedLevel)
                return;
            if (level != currentCompletedLevel + 1)
                return;

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
            EnsureInitialized();
            string[] unlocks = level switch
            {
                1 => level1Unlocks,
                2 => level2Unlocks,
                3 => level3Unlocks,
                4 => level4Unlocks,
                5 => level5Unlocks,
                _ => null
            };

            if (unlocks == null)
                return;

            foreach (string toolName in unlocks)
            {
                if (_unlockedTools.Contains(toolName))
                    continue;

                _unlockedTools.Add(toolName);
                _toolUpgradeLevels[toolName] = 0;

                EventBus.Publish(new ToolUnlockedEvent
                {
                    toolName = toolName,
                    level = level
                });
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
            EnsureInitialized();
            return _unlockedTools.Contains(toolName);
        }

        public int GetUpgradeLevel(string toolName)
        {
            EnsureInitialized();
            if (_toolUpgradeLevels.TryGetValue(toolName, out int level))
                return level;
            return 0;
        }

        public bool CanUpgrade(string toolName)
        {
            EnsureInitialized();
            if (!IsToolUnlocked(toolName))
                return false;

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
            EnsureInitialized();
            if (!CanUpgrade(toolName))
                return;

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
            EnsureInitialized();
            int level = GetUpgradeLevel(toolName);
            if (level == 0)
                return 0f;

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
            EnsureInitialized();
            int level = GetUpgradeLevel(toolName);
            if (level == 0)
                return toolName;

            return (toolName, level) switch
            {
                ("SmokeBomb", 1) => "Smoke Bomb Mk2",
                ("SmokeBomb", 2) => "Smoke Bomb Mk3",
                ("FlashBang", 1) => "Flash Bang Mk2",
                ("FlashBang", 2) => "Flash Bang Mk3",
                ("SoundBait", 1) => "Sound Bait Mk2",
                ("SoundBait", 2) => "Sound Bait Mk3",
                ("SleepDart", 1) => "Sleep Dart Mk2",
                ("SleepDart", 2) => "Sleep Dart Mk3",
                ("Rope", 1) => "Rope Tool Mk2",
                ("Rope", 2) => "Rope Tool Mk3",
                ("TimedNoise", 1) => "Timed Noise Mk2",
                ("TimedNoise", 2) => "Timed Noise Mk3",
                ("WallBreak", 1) => "Wall Break Mk2",
                ("WallBreak", 2) => "Wall Break Mk3",
                ("EMP", 1) => "EMP Mk2",
                ("EMP", 2) => "EMP Mk3",
                ("Drone", 1) => "Drone Swarm Mk2",
                ("Drone", 2) => "Drone Swarm Mk3",
                _ => toolName
            };
        }

        public void ResetProgression()
        {
            EnsureInitialized();
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
