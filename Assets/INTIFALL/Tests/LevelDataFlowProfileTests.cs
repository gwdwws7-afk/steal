using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class LevelDataFlowProfileTests
    {
        private static readonly Regex FieldPattern = new Regex("^  (?<name>[A-Za-z0-9_]+): (?<value>.*)$", RegexOptions.Multiline);

        [Test]
        public void LevelDataAssets_DefineRouteAndTimingProfiles()
        {
            string levelsDir = Path.Combine(Application.dataPath, "Resources/INTIFALL/Levels");
            Assert.IsTrue(Directory.Exists(levelsDir), $"Level data directory not found: {levelsDir}");

            string[] files = Directory.GetFiles(levelsDir, "*.asset", SearchOption.TopDirectoryOnly);
            Assert.GreaterOrEqual(files.Length, 5, "Expected at least five LevelData assets.");

            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                string levelName = ReadString(content, "levelName");
                int plannedMainRoutes = ReadInt(content, "plannedMainRoutes");
                int plannedStealthRoutes = ReadInt(content, "plannedStealthRoutes");
                float designedCompletionMinutes = ReadFloat(content, "designedCompletionMinutes");
                float completionWindowMinMinutes = ReadFloat(content, "completionWindowMinMinutes");
                float completionWindowMaxMinutes = ReadFloat(content, "completionWindowMaxMinutes");
                int patrolPressureTier = ReadInt(content, "patrolPressureTier");
                float enemyDensityTargetPerMinute = ReadFloat(content, "enemyDensityTargetPerMinute");
                float infiltrationMinutes = ReadFloat(content, "infiltrationMinutes");
                float objectiveMinutes = ReadFloat(content, "objectiveMinutes");
                float extractionMinutes = ReadFloat(content, "extractionMinutes");
                int infiltrationEnemyBudget = ReadInt(content, "infiltrationEnemyBudget");
                int objectiveEnemyBudget = ReadInt(content, "objectiveEnemyBudget");
                int extractionEnemyBudget = ReadInt(content, "extractionEnemyBudget");
                int totalEnemyCount = ReadInt(content, "totalEnemyCount");
                float standardTime = ReadFloat(content, "standardTime");
                float timeLimit = ReadFloat(content, "timeLimit");

                Assert.GreaterOrEqual(plannedMainRoutes, 2, $"Main route count too low for {levelName}.");
                Assert.GreaterOrEqual(plannedStealthRoutes, 1, $"Stealth route count too low for {levelName}.");
                Assert.Greater(designedCompletionMinutes, 0f, $"Designed completion time must be positive for {levelName}.");
                Assert.LessOrEqual(completionWindowMinMinutes, designedCompletionMinutes, $"Designed time below completion min window for {levelName}.");
                Assert.GreaterOrEqual(completionWindowMaxMinutes, designedCompletionMinutes, $"Designed time above completion max window for {levelName}.");
                Assert.GreaterOrEqual(patrolPressureTier, 1, $"patrolPressureTier must be >=1 for {levelName}.");
                Assert.LessOrEqual(patrolPressureTier, 5, $"patrolPressureTier must be <=5 for {levelName}.");
                Assert.Greater(enemyDensityTargetPerMinute, 0f, $"enemyDensityTargetPerMinute must be positive for {levelName}.");
                Assert.Greater(infiltrationMinutes, 0f, $"infiltrationMinutes must be positive for {levelName}.");
                Assert.Greater(objectiveMinutes, 0f, $"objectiveMinutes must be positive for {levelName}.");
                Assert.Greater(extractionMinutes, 0f, $"extractionMinutes must be positive for {levelName}.");
                Assert.AreEqual(
                    designedCompletionMinutes,
                    infiltrationMinutes + objectiveMinutes + extractionMinutes,
                    0.01f,
                    $"Phase timing sum must match designedCompletionMinutes for {levelName}.");

                Assert.GreaterOrEqual(infiltrationEnemyBudget, 0, $"infiltrationEnemyBudget must be non-negative for {levelName}.");
                Assert.GreaterOrEqual(objectiveEnemyBudget, 0, $"objectiveEnemyBudget must be non-negative for {levelName}.");
                Assert.GreaterOrEqual(extractionEnemyBudget, 0, $"extractionEnemyBudget must be non-negative for {levelName}.");
                Assert.AreEqual(
                    totalEnemyCount,
                    infiltrationEnemyBudget + objectiveEnemyBudget + extractionEnemyBudget,
                    $"Encounter enemy budgets must equal totalEnemyCount for {levelName}.");

                float actualDensity = totalEnemyCount / Mathf.Max(0.01f, designedCompletionMinutes);
                Assert.LessOrEqual(
                    Mathf.Abs(actualDensity - enemyDensityTargetPerMinute),
                    0.12f,
                    $"Enemy density target diverges from actual value in {levelName}.");

                Assert.GreaterOrEqual(standardTime, designedCompletionMinutes * 60f, $"Standard time must cover designed completion for {levelName}.");
                Assert.Greater(timeLimit, standardTime, $"timeLimit must be greater than standardTime for {levelName}.");
            }
        }

        [Test]
        public void LevelDataAssets_DesignedCompletionTime_IsNonDecreasingByLevelIndex()
        {
            string levelsDir = Path.Combine(Application.dataPath, "Resources/INTIFALL/Levels");
            string[] files = Directory.GetFiles(levelsDir, "*.asset", SearchOption.TopDirectoryOnly);
            Assert.GreaterOrEqual(files.Length, 5, "Expected at least five LevelData assets.");

            List<(int levelIndex, float designedMinutes, string levelName)> items = new();
            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                items.Add((
                    ReadInt(content, "levelIndex"),
                    ReadFloat(content, "designedCompletionMinutes"),
                    ReadString(content, "levelName")));
            }

            var ordered = items.OrderBy(x => x.levelIndex).ToArray();
            for (int i = 1; i < ordered.Length; i++)
            {
                Assert.GreaterOrEqual(
                    ordered[i].designedMinutes,
                    ordered[i - 1].designedMinutes,
                    $"Designed completion time should not decrease from {ordered[i - 1].levelName} to {ordered[i].levelName}.");
            }
        }

        [Test]
        public void LevelDataAssets_PressureCurve_IsMonotonicAcrossLevels()
        {
            string levelsDir = Path.Combine(Application.dataPath, "Resources/INTIFALL/Levels");
            string[] files = Directory.GetFiles(levelsDir, "*.asset", SearchOption.TopDirectoryOnly);
            Assert.GreaterOrEqual(files.Length, 5, "Expected at least five LevelData assets.");

            List<(int levelIndex, int patrolPressureTier, float fullAlertDuration, int supplyPointCount, string levelName)> items = new();
            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                items.Add((
                    ReadInt(content, "levelIndex"),
                    ReadInt(content, "patrolPressureTier"),
                    ReadFloat(content, "fullAlertDuration"),
                    ReadInt(content, "supplyPointCount"),
                    ReadString(content, "levelName")));
            }

            var ordered = items.OrderBy(x => x.levelIndex).ToArray();
            for (int i = 1; i < ordered.Length; i++)
            {
                Assert.GreaterOrEqual(
                    ordered[i].patrolPressureTier,
                    ordered[i - 1].patrolPressureTier,
                    $"Patrol pressure tier should not decrease from {ordered[i - 1].levelName} to {ordered[i].levelName}.");

                Assert.GreaterOrEqual(
                    ordered[i].fullAlertDuration,
                    ordered[i - 1].fullAlertDuration,
                    $"Full alert duration should not decrease from {ordered[i - 1].levelName} to {ordered[i].levelName}.");

                Assert.LessOrEqual(
                    ordered[i].supplyPointCount,
                    ordered[i - 1].supplyPointCount,
                    $"Supply point count should not increase from {ordered[i - 1].levelName} to {ordered[i].levelName}.");
            }
        }

        private static string ReadString(string content, string field)
        {
            MatchCollection matches = FieldPattern.Matches(content);
            foreach (Match match in matches)
            {
                if (match.Groups["name"].Value == field)
                    return match.Groups["value"].Value.Trim();
            }

            Assert.Fail($"Missing field '{field}' in level asset content.");
            return string.Empty;
        }

        private static int ReadInt(string content, string field)
        {
            string raw = ReadString(content, field);
            Assert.IsTrue(int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value),
                $"Field '{field}' is not int: {raw}");
            return value;
        }

        private static float ReadFloat(string content, string field)
        {
            string raw = ReadString(content, field);
            Assert.IsTrue(float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value),
                $"Field '{field}' is not float: {raw}");
            return value;
        }
    }
}
