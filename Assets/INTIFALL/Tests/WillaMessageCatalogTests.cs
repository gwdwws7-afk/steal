using System.Collections.Generic;
using System.IO;
using INTIFALL.Narrative;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class WillaMessageCatalogTests
    {
        [Test]
        public void BuildEffectiveCatalog_WithoutJson_UsesDefaults()
        {
            var catalog = WillaMessageCatalog.BuildEffectiveCatalog(string.Empty, out int imported, out string warning);

            Assert.AreEqual(0, imported);
            Assert.AreEqual(string.Empty, warning);
            Assert.IsTrue(
                catalog.ContainsKey(new WillaMessageCatalog.MessageKey(0, EWillaTrigger.MissionStart)),
                "Expected default level 0 mission start messages.");
        }

        [Test]
        public void BuildEffectiveCatalog_WithValidJson_OverridesEntry()
        {
            const string json = "{ \"entries\": [ { \"levelIndex\": 0, \"trigger\": \"MissionStart\", \"messages\": [ \"Override start.\" ] } ] }";

            var catalog = WillaMessageCatalog.BuildEffectiveCatalog(json, out int imported, out string warning);

            Assert.AreEqual(1, imported);
            Assert.AreEqual(string.Empty, warning);
            Assert.AreEqual(
                "Override start.",
                catalog[new WillaMessageCatalog.MessageKey(0, EWillaTrigger.MissionStart)][0]);
        }

        [Test]
        public void BuildEffectiveCatalog_WithInvalidEntries_IgnoresAndWarns()
        {
            const string json = "{ \"entries\": [ { \"levelIndex\": 1, \"trigger\": \"NotATrigger\", \"messages\": [ \"Bad\" ] }, { \"levelIndex\": 2, \"trigger\": \"IntelFound\", \"messages\": [] } ] }";

            var catalog = WillaMessageCatalog.BuildEffectiveCatalog(json, out int imported, out string warning);

            Assert.AreEqual(0, imported);
            Assert.IsTrue(warning.Contains("Ignored"), "Expected warning for invalid entries.");
            Assert.IsTrue(
                catalog.ContainsKey(new WillaMessageCatalog.MessageKey(2, EWillaTrigger.IntelFound)),
                "Default fallback entries should still exist.");
        }

        [Test]
        public void DefaultCatalog_HasCompleteNarrativeChain_ForFiveLevels()
        {
            var catalog = WillaMessageCatalog.BuildEffectiveCatalog(string.Empty, out _, out _);
            AssertRequiredCoverage(catalog, "default");
        }

        [Test]
        public void ResourceCatalog_HasCompleteNarrativeChain_ForFiveLevels()
        {
            string catalogPath = Path.Combine(Application.dataPath, "Resources/INTIFALL/Narrative/WillaMessages.json");
            Assert.IsTrue(File.Exists(catalogPath), $"Expected resource catalog file at: {catalogPath}");

            string json = File.ReadAllText(catalogPath);
            var catalog = WillaMessageCatalog.BuildEffectiveCatalog(json, out _, out _);
            AssertRequiredCoverage(catalog, "resource");
        }

        [Test]
        public void DefaultCatalog_HasAdvancedTriggerCoverage()
        {
            var catalog = WillaMessageCatalog.BuildEffectiveCatalog(string.Empty, out _, out _);

            AssertTriggerHasMessages(catalog, 4, EWillaTrigger.StoryReveal, "default");
            AssertTriggerHasMessages(catalog, 4, EWillaTrigger.Warning, "default");
            AssertTriggerHasMessages(catalog, -1, EWillaTrigger.Betrayal, "default");
        }

        [Test]
        public void ResourceCatalog_HasAdvancedTriggerCoverage()
        {
            string catalogPath = Path.Combine(Application.dataPath, "Resources/INTIFALL/Narrative/WillaMessages.json");
            Assert.IsTrue(File.Exists(catalogPath), $"Expected resource catalog file at: {catalogPath}");

            string json = File.ReadAllText(catalogPath);
            var catalog = WillaMessageCatalog.BuildEffectiveCatalog(json, out _, out _);

            AssertTriggerHasMessages(catalog, 4, EWillaTrigger.StoryReveal, "resource");
            AssertTriggerHasMessages(catalog, 4, EWillaTrigger.Warning, "resource");
            AssertTriggerHasMessages(catalog, -1, EWillaTrigger.Betrayal, "resource");
        }

        private static void AssertRequiredCoverage(
            Dictionary<WillaMessageCatalog.MessageKey, string[]> catalog,
            string sourceName)
        {
            var requiredTriggers = new[]
            {
                EWillaTrigger.MissionStart,
                EWillaTrigger.IntelFound,
                EWillaTrigger.MissionComplete
            };

            for (int level = 0; level < 5; level++)
            {
                for (int i = 0; i < requiredTriggers.Length; i++)
                {
                    EWillaTrigger trigger = requiredTriggers[i];
                    var key = new WillaMessageCatalog.MessageKey(level, trigger);
                    Assert.IsTrue(catalog.ContainsKey(key), $"Missing {sourceName} entry for level {level} trigger {trigger}.");

                    string[] messages = catalog[key];
                    Assert.IsNotNull(messages, $"Null message array for level {level} trigger {trigger}.");
                    Assert.Greater(messages.Length, 0, $"Empty message array for level {level} trigger {trigger}.");

                    for (int messageIndex = 0; messageIndex < messages.Length; messageIndex++)
                    {
                        string message = messages[messageIndex];
                        Assert.IsFalse(
                            string.IsNullOrWhiteSpace(message),
                            $"Blank message in {sourceName} level {level} trigger {trigger} at index {messageIndex}.");

                        string upper = message.ToUpperInvariant();
                        Assert.IsFalse(upper.Contains("TODO") || upper.Contains("TBD") || upper.Contains("PLACEHOLDER"),
                            $"Placeholder text in {sourceName} level {level} trigger {trigger}: {message}");
                    }
                }
            }
        }

        private static void AssertTriggerHasMessages(
            Dictionary<WillaMessageCatalog.MessageKey, string[]> catalog,
            int levelIndex,
            EWillaTrigger trigger,
            string sourceName)
        {
            var key = new WillaMessageCatalog.MessageKey(levelIndex, trigger);
            Assert.IsTrue(catalog.ContainsKey(key), $"Missing {sourceName} entry for level {levelIndex} trigger {trigger}.");

            string[] messages = catalog[key];
            Assert.IsNotNull(messages, $"Null message array for level {levelIndex} trigger {trigger}.");
            Assert.Greater(messages.Length, 0, $"Empty message array for level {levelIndex} trigger {trigger}.");

            for (int i = 0; i < messages.Length; i++)
            {
                string message = messages[i];
                Assert.IsFalse(string.IsNullOrWhiteSpace(message),
                    $"Blank message in {sourceName} level {levelIndex} trigger {trigger} at index {i}.");

                string upper = message.ToUpperInvariant();
                Assert.IsFalse(upper.Contains("TODO") || upper.Contains("TBD") || upper.Contains("PLACEHOLDER"),
                    $"Placeholder text in {sourceName} level {levelIndex} trigger {trigger}: {message}");
            }
        }
    }
}
