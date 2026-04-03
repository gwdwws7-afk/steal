using INTIFALL.Core;
using NUnit.Framework;

namespace INTIFALL.Tests
{
    public class SaveLoadManagerMigrationTests
    {
        [Test]
        public void DeserializeWithMigration_LegacySchema_UpgradesToCurrentVersion()
        {
            const string legacyJson = "{\"credits\":320,\"highestLevel\":3,\"currentLevel\":0,\"bloodlineLevel\":2,\"totalPlayTime\":147.5}";

            SaveLoadManager.SaveData data = SaveLoadManager.DeserializeWithMigration(legacyJson);

            Assert.IsNotNull(data);
            Assert.AreEqual(SaveLoadManager.CurrentSaveSchemaVersion, data.schemaVersion);
            Assert.AreEqual(320, data.credits);
            Assert.AreEqual(3, data.highestLevel);
            Assert.AreEqual(3, data.currentLevel);
            Assert.AreEqual("main", data.lastMissionRouteId);
            Assert.AreEqual("Main Extraction", data.lastMissionRouteLabel);
            Assert.AreEqual(1f, data.lastMissionRouteMultiplier, 0.001f);
            Assert.AreEqual(0, data.lastMissionToolRiskWindowAdjustment);
            Assert.AreEqual(0f, data.lastMissionToolCooldownLoad, 0.001f);
            Assert.AreEqual(0, data.lastMissionRopeToolUses);
            Assert.AreEqual(0, data.lastMissionSmokeToolUses);
            Assert.AreEqual(0, data.lastMissionSoundBaitToolUses);
            Assert.IsNotNull(data.unlockedTools);
        }

        [Test]
        public void DeserializeWithMigration_CurrentSchema_PreservesMissionSnapshot()
        {
            const string json = "{\"schemaVersion\":2,\"credits\":540,\"highestLevel\":5,\"currentLevel\":5,\"bloodlineLevel\":4,\"totalPlayTime\":900.0,\"hasMissionSnapshot\":true,\"lastMissionRank\":\"A\",\"lastMissionRankScore\":4,\"lastMissionCredits\":640,\"lastMissionIntelCollected\":3,\"lastMissionIntelRequired\":3,\"lastMissionSecondaryCompleted\":2,\"lastMissionSecondaryTotal\":3,\"lastMissionUsedOptionalExit\":true,\"lastMissionRouteId\":\"upper_ring\",\"lastMissionRouteLabel\":\"Upper Ring Catwalk\",\"lastMissionRouteRiskTier\":3,\"lastMissionRouteMultiplier\":1.3,\"lastMissionToolsUsed\":5,\"lastMissionAlertsTriggered\":2,\"lastMissionToolRiskWindowAdjustment\":35,\"lastMissionToolCooldownLoad\":30.0,\"lastMissionRopeToolUses\":1,\"lastMissionSmokeToolUses\":2,\"lastMissionSoundBaitToolUses\":1,\"unlockedTools\":[\"SmokeBomb\",\"EMP\"]}";

            SaveLoadManager.SaveData data = SaveLoadManager.DeserializeWithMigration(json);

            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.schemaVersion);
            Assert.IsTrue(data.hasMissionSnapshot);
            Assert.AreEqual("A", data.lastMissionRank);
            Assert.AreEqual("upper_ring", data.lastMissionRouteId);
            Assert.AreEqual("Upper Ring Catwalk", data.lastMissionRouteLabel);
            Assert.AreEqual(3, data.lastMissionRouteRiskTier);
            Assert.AreEqual(1.3f, data.lastMissionRouteMultiplier, 0.001f);
            Assert.AreEqual(35, data.lastMissionToolRiskWindowAdjustment);
            Assert.AreEqual(30f, data.lastMissionToolCooldownLoad, 0.001f);
            Assert.AreEqual(1, data.lastMissionRopeToolUses);
            Assert.AreEqual(2, data.lastMissionSmokeToolUses);
            Assert.AreEqual(1, data.lastMissionSoundBaitToolUses);
            Assert.AreEqual(2, data.unlockedTools.Length);
        }

        [Test]
        public void DeserializeWithMigration_InvalidJson_ReturnsNull()
        {
            SaveLoadManager.SaveData data = SaveLoadManager.DeserializeWithMigration("{bad json}");
            Assert.IsNull(data);
        }
    }
}
