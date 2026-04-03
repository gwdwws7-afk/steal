using INTIFALL.AI;
using INTIFALL.System;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class LocalizationServiceTests
    {
        [SetUp]
        public void SetUp()
        {
            LocalizationService.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            LocalizationService.ResetForTests();
        }

        [Test]
        public void LocalizationTable_LoadsCoreKeys()
        {
            Assert.Greater(LocalizationService.LoadedKeyCount, 0, "Localization table should load at least one key.");
            Assert.IsTrue(LocalizationService.HasKey("enemy.guard"));
            Assert.IsTrue(LocalizationService.HasKey("enemy.heavy_guard"));
            Assert.IsTrue(LocalizationService.HasKey("menu.new_game"));
            Assert.IsTrue(LocalizationService.HasKey("debrief.title"));
        }

        [Test]
        public void LocalizationTable_ContainsIteration11RuntimeKeys()
        {
            string[] requiredKeys =
            {
                "menu.continue",
                "menu.feedback.continue_failed",
                "menu.slot.corrupted_backup_status",
                "menu.mission_snapshot.line3",
                "menu.mission_snapshot.compact",
                "arsenal.credits",
                "supply.prompt.ready",
                "door.prompt.locked",
                "vent.prompt.enter",
                "hang.prompt.attach",
                "intel.popup.acquired",
                "hud.first_aid.count",
                "toolhud.no_tool",
                "gameover.title.failed",
                "briefing.line.objectives",
                "briefing.contact",
                "hud.secondary.progress",
                "hud.secondary.complete_with_count",
                "hud.secondary.evaluating",
                "debrief.line.secondary_evaluated"
            };

            for (int i = 0; i < requiredKeys.Length; i++)
                Assert.IsTrue(LocalizationService.HasKey(requiredKeys[i]), $"Missing localization key: {requiredKeys[i]}");
        }

        [Test]
        public void BriefingLevelKeys_ArePresentForAllFiveLevels()
        {
            string[] prefixes =
            {
                "briefing.mission_name.",
                "briefing.mission_level.",
                "briefing.background.",
                "briefing.objective.",
                "briefing.warning.",
                "briefing.intel_hint."
            };

            for (int p = 0; p < prefixes.Length; p++)
            {
                for (int level = 1; level <= 5; level++)
                {
                    string key = $"{prefixes[p]}{level}";
                    Assert.IsTrue(LocalizationService.HasKey(key), $"Missing briefing localization key: {key}");
                }
            }
        }

        [Test]
        public void Get_UsesLanguageSpecificEntries_WhenKeyExists()
        {
            string en = LocalizationService.Get("enemy.guard", languageOverride: SystemLanguage.English);
            string zh = LocalizationService.Get("enemy.guard", languageOverride: SystemLanguage.ChineseSimplified);

            Assert.AreEqual("Guard", en);
            Assert.IsFalse(string.IsNullOrWhiteSpace(zh));
            Assert.AreNotEqual(en, zh);
        }

        [Test]
        public void Get_MissingKey_UsesLanguageFallback()
        {
            Assert.AreEqual(
                "Fallback EN",
                LocalizationService.Get(
                    "missing.key",
                    fallbackEnglish: "Fallback EN",
                    fallbackChinese: "Fallback CN",
                    languageOverride: SystemLanguage.English));

            Assert.AreEqual(
                "Fallback CN",
                LocalizationService.Get(
                    "missing.key",
                    fallbackEnglish: "Fallback EN",
                    fallbackChinese: "Fallback CN",
                    languageOverride: SystemLanguage.ChineseSimplified));
        }

        [Test]
        public void EnemyTypeLocalizationKeys_ArePresentInLocalizationTable()
        {
            foreach (EEnemyType type in global::System.Enum.GetValues(typeof(EEnemyType)))
            {
                EnemyTypeData data = EnemyTypeData.GetDefaultData(type);
                Assert.IsFalse(string.IsNullOrWhiteSpace(data.localizationKey));
                Assert.IsTrue(LocalizationService.HasKey(data.localizationKey), $"Missing localization table entry: {data.localizationKey}");
                Object.DestroyImmediate(data);
            }
        }
    }
}
