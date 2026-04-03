using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using INTIFALL.Tools;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class ToolDataConfigurationTests
    {
        private static readonly Regex FieldPattern = new Regex("^  (?<name>[A-Za-z0-9_]+): (?<value>.*)$", RegexOptions.Multiline);

        [Test]
        public void ToolAssets_HaveConfiguredStatsAndDescriptions()
        {
            string toolsDir = Path.Combine(Application.dataPath, "INTIFALL/ScriptableObjects/Tools");
            Assert.IsTrue(Directory.Exists(toolsDir), $"Tool data directory not found: {toolsDir}");

            string[] files = Directory.GetFiles(toolsDir, "*.asset", SearchOption.TopDirectoryOnly);
            Assert.GreaterOrEqual(files.Length, 8, "Expected the full tool profile set.");

            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                string toolName = ReadString(content, "toolName");
                float range = ReadFloat(content, "range");
                float cooldown = ReadFloat(content, "cooldown");
                float duration = ReadFloat(content, "duration");
                int maxAmmo = ReadInt(content, "maxAmmo");
                int slotCost = ReadInt(content, "slotCost");
                string description = ReadString(content, "description");

                Assert.IsFalse(string.IsNullOrWhiteSpace(toolName), $"Missing toolName in {Path.GetFileName(file)}");
                Assert.Greater(range, 0f, $"Range must be configured for {toolName}");
                Assert.Greater(cooldown, 0f, $"Cooldown must be configured for {toolName}");
                Assert.GreaterOrEqual(duration, 0f, $"Duration must be non-negative for {toolName}");
                Assert.Greater(maxAmmo, 0, $"MaxAmmo must be configured for {toolName}");
                Assert.That(slotCost, Is.InRange(1, 2), $"slotCost must be 1 or 2 for {toolName}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(description), $"Description must be configured for {toolName}");
            }
        }

        [Test]
        public void ToolAssets_ToolNameCn_IsAsciiAndReadable()
        {
            string toolsDir = Path.Combine(Application.dataPath, "INTIFALL/ScriptableObjects/Tools");
            string[] files = Directory.GetFiles(toolsDir, "*.asset", SearchOption.TopDirectoryOnly);

            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                string toolName = ReadString(content, "toolName");
                string toolNameCn = ReadString(content, "toolNameCN");

                Assert.IsFalse(string.IsNullOrWhiteSpace(toolNameCn), $"toolNameCN missing for {toolName}");
                Assert.IsFalse(toolNameCn.Contains('?'), $"toolNameCN likely corrupted for {toolName}: {toolNameCn}");

                foreach (char c in toolNameCn)
                {
                    Assert.LessOrEqual(c, '~', $"toolNameCN must be ASCII-clean for {toolName}: {toolNameCn}");
                }
            }
        }

        [Test]
        public void ToolAssets_ToolNames_AreUnique()
        {
            string toolsDir = Path.Combine(Application.dataPath, "INTIFALL/ScriptableObjects/Tools");
            string[] files = Directory.GetFiles(toolsDir, "*.asset", SearchOption.TopDirectoryOnly);
            HashSet<string> toolNames = new();

            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                string toolName = ReadString(content, "toolName");
                Assert.IsTrue(toolNames.Add(toolName), $"Duplicate toolName detected: {toolName}");
            }
        }

        [Test]
        public void ToolAssets_NoSingleDominantChoice_AcrossRangeCooldownAndAmmo()
        {
            string toolsDir = Path.Combine(Application.dataPath, "INTIFALL/ScriptableObjects/Tools");
            string[] files = Directory.GetFiles(toolsDir, "*.asset", SearchOption.TopDirectoryOnly);
            Assert.GreaterOrEqual(files.Length, 8, "Expected enough tools for tradeoff validation.");

            List<ToolProfile> profiles = new();
            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                profiles.Add(new ToolProfile
                {
                    toolName = ReadString(content, "toolName"),
                    range = ReadFloat(content, "range"),
                    cooldown = ReadFloat(content, "cooldown"),
                    maxAmmo = ReadInt(content, "maxAmmo")
                });
            }

            for (int i = 0; i < profiles.Count; i++)
            {
                ToolProfile candidate = profiles[i];
                bool dominatesAll = true;

                for (int j = 0; j < profiles.Count; j++)
                {
                    if (i == j)
                        continue;

                    ToolProfile other = profiles[j];
                    bool dominatesOther =
                        candidate.range >= other.range &&
                        candidate.maxAmmo >= other.maxAmmo &&
                        candidate.cooldown <= other.cooldown &&
                        (candidate.range > other.range ||
                         candidate.maxAmmo > other.maxAmmo ||
                         candidate.cooldown < other.cooldown);

                    if (!dominatesOther)
                    {
                        dominatesAll = false;
                        break;
                    }
                }

                Assert.IsFalse(
                    dominatesAll,
                    $"Tool '{candidate.toolName}' dominates all other tools; expected meaningful tradeoffs.");
            }
        }

        [Test]
        public void ToolAssets_ContainAtLeastOneTwoSlotTool()
        {
            string toolsDir = Path.Combine(Application.dataPath, "INTIFALL/ScriptableObjects/Tools");
            string[] files = Directory.GetFiles(toolsDir, "*.asset", SearchOption.TopDirectoryOnly);

            bool foundTwoSlotTool = false;
            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                int slotCost = ReadInt(content, "slotCost");
                if (slotCost >= 2)
                {
                    foundTwoSlotTool = true;
                    break;
                }
            }

            Assert.IsTrue(foundTwoSlotTool, "Expected at least one tool asset with slotCost=2 for multi-slot occupancy rules.");
        }

        [Test]
        public void ToolAssets_RuntimePrefabBindings_AreValidAndContainToolBase()
        {
            const string toolsDir = "Assets/INTIFALL/ScriptableObjects/Tools";
            string[] toolDataGuids = AssetDatabase.FindAssets("t:ToolData", new[] { toolsDir });
            Assert.GreaterOrEqual(toolDataGuids.Length, 8, "Expected the full tool profile set.");

            foreach (string guid in toolDataGuids)
            {
                string toolAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                ToolData toolData = AssetDatabase.LoadAssetAtPath<ToolData>(toolAssetPath);
                Assert.IsNotNull(toolData, $"Failed to load ToolData at {toolAssetPath}");
                Assert.IsNotNull(toolData.runtimePrefab, $"runtimePrefab missing for tool asset {toolAssetPath}");

                string prefabPath = AssetDatabase.GetAssetPath(toolData.runtimePrefab);
                Assert.IsFalse(string.IsNullOrWhiteSpace(prefabPath), $"runtimePrefab path missing for {toolData.toolName}");

                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    int missingScriptCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefabRoot);
                    Assert.Zero(missingScriptCount, $"runtimePrefab contains missing script bindings: {prefabPath}");

                    ToolBase runtimeTool = prefabRoot.GetComponent<ToolBase>();
                    Assert.IsNotNull(runtimeTool, $"runtimePrefab must include ToolBase: {prefabPath}");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
        }

        [Test]
        public void CoreStealthToolkit_TuningBaseline_IsWithinIteration31Envelope()
        {
            string ropeContent = File.ReadAllText(Path.Combine(Application.dataPath, "INTIFALL/ScriptableObjects/Tools/ToolData_Rope.asset"));
            string smokeContent = File.ReadAllText(Path.Combine(Application.dataPath, "INTIFALL/ScriptableObjects/Tools/ToolData_SmokeBomb.asset"));
            string baitContent = File.ReadAllText(Path.Combine(Application.dataPath, "INTIFALL/ScriptableObjects/Tools/ToolData_SoundBait.asset"));
            string timedNoiseContent = File.ReadAllText(Path.Combine(Application.dataPath, "INTIFALL/ScriptableObjects/Tools/ToolData_TimedNoise.asset"));

            Assert.That(ReadFloat(ropeContent, "cooldown"), Is.GreaterThanOrEqualTo(5f).And.LessThanOrEqualTo(6f), "Rope cooldown baseline drifted.");
            Assert.That(ReadInt(ropeContent, "maxAmmo"), Is.EqualTo(2), "Rope ammo baseline drifted.");

            Assert.That(ReadFloat(smokeContent, "duration"), Is.GreaterThanOrEqualTo(8f), "Smoke duration baseline drifted.");
            Assert.That(ReadFloat(baitContent, "cooldown"), Is.GreaterThanOrEqualTo(8f), "SoundBait cooldown baseline drifted.");
            Assert.That(ReadInt(baitContent, "maxAmmo"), Is.EqualTo(3), "SoundBait ammo baseline drifted.");

            Assert.That(ReadFloat(timedNoiseContent, "cooldown"), Is.LessThanOrEqualTo(15f), "TimedNoise cooldown should support decoy alternative route.");
            Assert.That(ReadInt(timedNoiseContent, "maxAmmo"), Is.GreaterThanOrEqualTo(4), "TimedNoise ammo should support extended decoy chains.");
        }

        private static string ReadString(string content, string field)
        {
            MatchCollection matches = FieldPattern.Matches(content);
            foreach (Match match in matches)
            {
                if (match.Groups["name"].Value == field)
                    return match.Groups["value"].Value.Trim();
            }

            Assert.Fail($"Missing field '{field}' in asset content.");
            return string.Empty;
        }

        private static float ReadFloat(string content, string field)
        {
            string raw = ReadString(content, field);
            Assert.IsTrue(float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed),
                $"Field '{field}' is not a float: {raw}");
            return parsed;
        }

        private static int ReadInt(string content, string field)
        {
            string raw = ReadString(content, field);
            Assert.IsTrue(int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed),
                $"Field '{field}' is not an int: {raw}");
            return parsed;
        }

        private struct ToolProfile
        {
            public string toolName;
            public float range;
            public float cooldown;
            public int maxAmmo;
        }
    }
}
