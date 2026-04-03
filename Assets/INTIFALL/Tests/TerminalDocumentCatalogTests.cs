using System;
using System.IO;
using INTIFALL.Data;
using INTIFALL.Narrative;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class TerminalDocumentCatalogTests
    {
        [SetUp]
        public void SetUp()
        {
            TerminalDocumentCatalog.ResetForTests();
        }

        [Test]
        public void ResourceCatalog_CoversAllTerminalSpawnIds_AndContainsNoPlaceholders()
        {
            string catalogPath = Path.Combine(Application.dataPath, "Resources/INTIFALL/Narrative/TerminalDocuments.json");
            Assert.IsTrue(File.Exists(catalogPath), $"Expected terminal document catalog at: {catalogPath}");

            string[] spawnGuids = AssetDatabase.FindAssets("t:IntelSpawnData", new[] { "Assets/INTIFALL/ScriptableObjects/Spawns" });
            Assert.GreaterOrEqual(spawnGuids.Length, 5, "Expected level intel spawn assets for L01-L05.");

            int totalTerminalEntries = 0;
            bool[] levelHasAdvancedTrigger = new bool[5];

            for (int i = 0; i < spawnGuids.Length; i++)
            {
                string spawnPath = AssetDatabase.GUIDToAssetPath(spawnGuids[i]);
                IntelSpawnData spawn = AssetDatabase.LoadAssetAtPath<IntelSpawnData>(spawnPath);
                Assert.IsNotNull(spawn, $"Failed to load spawn asset: {spawnPath}");

                IntelSpawnPoint[] intelPoints = spawn.intelPoints ?? Array.Empty<IntelSpawnPoint>();
                for (int p = 0; p < intelPoints.Length; p++)
                {
                    IntelSpawnPoint point = intelPoints[p];
                    if (point == null || point.intelType != EIntelType.TerminalDocument)
                        continue;

                    totalTerminalEntries++;
                    bool found = TerminalDocumentCatalog.TryGet(point.intelId, spawn.levelIndex, out TerminalDocumentRecord record);
                    Assert.IsTrue(found, $"Missing terminal catalog entry for {point.intelId} at level {spawn.levelIndex}.");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(record.Title), $"Blank title for {point.intelId}.");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(record.Summary), $"Blank summary for {point.intelId}.");
                    Assert.IsFalse(ContainsPlaceholder(record.Title), $"Placeholder title for {point.intelId}: {record.Title}");
                    Assert.IsFalse(ContainsPlaceholder(record.Summary), $"Placeholder summary for {point.intelId}: {record.Summary}");

                    int normalizedLevel = Mathf.Clamp(spawn.levelIndex, 0, levelHasAdvancedTrigger.Length - 1);
                    if (!string.IsNullOrWhiteSpace(record.AdvancedTrigger))
                        levelHasAdvancedTrigger[normalizedLevel] = true;
                }
            }

            Assert.GreaterOrEqual(totalTerminalEntries, 20, "Expected at least 20 terminal documents across five levels.");
            for (int level = 0; level < levelHasAdvancedTrigger.Length; level++)
            {
                Assert.IsTrue(levelHasAdvancedTrigger[level], $"Expected at least one advanced trigger terminal on level {level}.");
            }
        }

        private static bool ContainsPlaceholder(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return false;

            string upper = source.ToUpperInvariant();
            return upper.Contains("TODO") || upper.Contains("TBD") || upper.Contains("PLACEHOLDER");
        }
    }
}
