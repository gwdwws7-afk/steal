using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class LevelEncounterCoverageTests
    {
        private static readonly Regex FieldPattern = new("^  (?<name>[A-Za-z0-9_]+): (?<value>.*)$", RegexOptions.Multiline);

        [Test]
        public void LevelEncounterProfiles_AlignWithSpawnAssets()
        {
            string levelsDir = Path.Combine(Application.dataPath, "Resources/INTIFALL/Levels");
            string spawnsDir = Path.Combine(Application.dataPath, "Resources/INTIFALL/Spawns");
            Assert.IsTrue(Directory.Exists(levelsDir), $"Levels dir missing: {levelsDir}");
            Assert.IsTrue(Directory.Exists(spawnsDir), $"Spawns dir missing: {spawnsDir}");

            string[] levelFiles = Directory.GetFiles(levelsDir, "*.asset", SearchOption.TopDirectoryOnly);
            Assert.GreaterOrEqual(levelFiles.Length, 5, "Expected at least five LevelData assets.");

            foreach (string levelFile in levelFiles)
            {
                string levelContent = File.ReadAllText(levelFile);
                string levelName = ReadString(levelContent, "levelName");

                int totalEnemyCount = ReadInt(levelContent, "totalEnemyCount");
                int infiltrationEnemyBudget = ReadInt(levelContent, "infiltrationEnemyBudget");
                int objectiveEnemyBudget = ReadInt(levelContent, "objectiveEnemyBudget");
                int extractionEnemyBudget = ReadInt(levelContent, "extractionEnemyBudget");
                int plannedMainRoutes = ReadInt(levelContent, "plannedMainRoutes");
                int plannedStealthRoutes = ReadInt(levelContent, "plannedStealthRoutes");

                int budgetTotal = infiltrationEnemyBudget + objectiveEnemyBudget + extractionEnemyBudget;
                Assert.AreEqual(totalEnemyCount, budgetTotal, $"Encounter budget mismatch in {levelName}");

                string enemySpawnFile = Path.Combine(spawnsDir, $"EnemySpawn_{levelName}.asset");
                Assert.IsTrue(File.Exists(enemySpawnFile), $"EnemySpawnData missing for {levelName}");

                string enemyContent = File.ReadAllText(enemySpawnFile);
                int spawnCount = Regex.Matches(enemyContent, "^  - spawnId: ", RegexOptions.Multiline).Count;
                Assert.AreEqual(totalEnemyCount, spawnCount, $"Enemy spawn count mismatch in {levelName}");

                int routeCount = Regex.Matches(enemyContent, "^  - route_", RegexOptions.Multiline).Count;
                int requiredRouteCount = plannedMainRoutes + plannedStealthRoutes;
                Assert.GreaterOrEqual(routeCount, requiredRouteCount,
                    $"Patrol route count below planned route target in {levelName}");
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
    }
}
