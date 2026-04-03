using System.Reflection;
using INTIFALL.System;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class MissionRewardBandingTests
    {
        private GameObject _go;
        private GameManager _gm;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("MissionRewardBanding_GameManager");
            _gm = _go.AddComponent<GameManager>();
            _gm.LoadLevel(3, "Level04_Qhipu_Core");
            _gm.StartGame();
            SetPrivateField("_playTime", 240f);
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        [Test]
        public void RankBands_SABC_HaveClearCreditSeparation()
        {
            MissionResult sRank = EvaluateScenario(
                enemiesKilled: 0,
                wasDiscovered: false,
                fullAlertTriggered: false,
                tookDamage: false,
                secondaryObjectivesCompleted: 2,
                intelCollected: 3,
                alertsTriggered: 0,
                toolsUsed: 1);

            MissionResult aRank = EvaluateScenario(
                enemiesKilled: 1,
                wasDiscovered: false,
                fullAlertTriggered: false,
                tookDamage: false,
                secondaryObjectivesCompleted: 1,
                intelCollected: 2,
                alertsTriggered: 0,
                toolsUsed: 1);

            MissionResult bRank = EvaluateScenario(
                enemiesKilled: 1,
                wasDiscovered: false,
                fullAlertTriggered: false,
                tookDamage: false,
                secondaryObjectivesCompleted: 1,
                intelCollected: 1,
                alertsTriggered: 1,
                toolsUsed: 2);

            MissionResult cRank = EvaluateScenario(
                enemiesKilled: 2,
                wasDiscovered: true,
                fullAlertTriggered: false,
                tookDamage: true,
                secondaryObjectivesCompleted: 0,
                intelCollected: 1,
                alertsTriggered: 2,
                toolsUsed: 4);

            Assert.AreEqual("S", sRank.Rank);
            Assert.AreEqual("A", aRank.Rank);
            Assert.AreEqual("B", bRank.Rank);
            Assert.AreEqual("C", cRank.Rank);

            Assert.Greater(sRank.CreditsEarned, aRank.CreditsEarned, "S reward should exceed A reward.");
            Assert.Greater(aRank.CreditsEarned, bRank.CreditsEarned, "A reward should exceed B reward.");
            Assert.Greater(bRank.CreditsEarned, cRank.CreditsEarned, "B reward should exceed C reward.");

            Assert.GreaterOrEqual(sRank.CreditsEarned - aRank.CreditsEarned, 120, "S/A reward gap too small.");
            Assert.GreaterOrEqual(aRank.CreditsEarned - bRank.CreditsEarned, 100, "A/B reward gap too small.");
            Assert.GreaterOrEqual(bRank.CreditsEarned - cRank.CreditsEarned, 80, "B/C reward gap too small.");
        }

        private MissionResult EvaluateScenario(
            int enemiesKilled,
            bool wasDiscovered,
            bool fullAlertTriggered,
            bool tookDamage,
            int secondaryObjectivesCompleted,
            int intelCollected,
            int alertsTriggered,
            int toolsUsed)
        {
            SetPrivateField("_enemiesKilled", enemiesKilled);
            SetPrivateField("_wasDiscovered", wasDiscovered);
            SetPrivateField("_fullAlertTriggered", fullAlertTriggered);
            SetPrivateField("_tookDamage", tookDamage);
            SetPrivateField("_alertsTriggered", alertsTriggered);
            SetPrivateField("_toolsUsed", toolsUsed);

            return _gm.CalculateMissionResult(
                secondaryObjectivesCompleted: secondaryObjectivesCompleted,
                intelCollected: intelCollected,
                intelRequired: 3,
                timeBudgetSeconds: 900f,
                extractionRouteId: "main",
                extractionRouteLabel: "Main Extraction",
                isMainRoute: true,
                routeRiskTier: 0,
                routeCreditMultiplier: 1f,
                routeSecondaryObjectiveBonus: 0);
        }

        private void SetPrivateField(string fieldName, object value)
        {
            FieldInfo field = typeof(GameManager).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing private field {fieldName}");
            field.SetValue(_gm, value);
        }
    }
}
