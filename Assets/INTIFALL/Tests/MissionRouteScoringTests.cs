using INTIFALL.System;
using INTIFALL.Tools;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class MissionRouteScoringTests
    {
        private GameObject _go;
        private GameManager _gm;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("GameManager");
            _gm = _go.AddComponent<GameManager>();
            _gm.LoadLevel(2, "Level03_Underground_Labs");
            _gm.StartGame();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        [Test]
        public void OptionalExit_WithHigherRiskAndMultiplier_YieldsHigherCredits()
        {
            MissionResult main = _gm.CalculateMissionResult(
                secondaryObjectivesCompleted: 1,
                intelCollected: 2,
                intelRequired: 3,
                timeBudgetSeconds: 600f,
                extractionRouteId: "main",
                extractionRouteLabel: "Main Extraction",
                isMainRoute: true,
                routeRiskTier: 0,
                routeCreditMultiplier: 1f,
                routeSecondaryObjectiveBonus: 0);

            MissionResult optional = _gm.CalculateMissionResult(
                secondaryObjectivesCompleted: 1,
                intelCollected: 2,
                intelRequired: 3,
                timeBudgetSeconds: 600f,
                extractionRouteId: "coolant_tunnel",
                extractionRouteLabel: "Coolant Tunnel",
                isMainRoute: false,
                routeRiskTier: 3,
                routeCreditMultiplier: 1.25f,
                routeSecondaryObjectiveBonus: 1);

            Assert.Greater(optional.CreditsEarned, main.CreditsEarned);
            Assert.IsTrue(optional.UsedOptionalExit);
            Assert.AreEqual("coolant_tunnel", optional.ExtractionRouteId);
            Assert.AreEqual("Coolant Tunnel", optional.ExtractionRouteLabel);
        }

        [Test]
        public void MainExit_ResultContainsMainRouteDefaults()
        {
            MissionResult result = _gm.CalculateMissionResult(
                secondaryObjectivesCompleted: 0,
                intelCollected: 0,
                intelRequired: 3,
                timeBudgetSeconds: 600f,
                extractionRouteId: "main",
                extractionRouteLabel: "Main Extraction",
                isMainRoute: true,
                routeRiskTier: 0,
                routeCreditMultiplier: 1f,
                routeSecondaryObjectiveBonus: 0);

            Assert.IsFalse(result.UsedOptionalExit);
            Assert.AreEqual("main", result.ExtractionRouteId);
            Assert.AreEqual("Main Extraction", result.ExtractionRouteLabel);
            Assert.AreEqual(0, result.RouteRiskTier);
            Assert.AreEqual(1f, result.RouteCreditMultiplier, 0.001f);
        }

        [Test]
        public void OptionalExit_UnderHighPressure_DoesNotDominateCleanMainRoute()
        {
            MissionResult cleanMain = _gm.CalculateMissionResult(
                secondaryObjectivesCompleted: 1,
                intelCollected: 2,
                intelRequired: 3,
                timeBudgetSeconds: 600f,
                extractionRouteId: "main",
                extractionRouteLabel: "Main Extraction",
                isMainRoute: true,
                routeRiskTier: 0,
                routeCreditMultiplier: 1f,
                routeSecondaryObjectiveBonus: 0);

            SetPrivateField("_alertsTriggered", 4);
            SetPrivateField("_toolsUsed", 7);
            _gm.RecordDiscovery();

            MissionResult pressuredOptional = _gm.CalculateMissionResult(
                secondaryObjectivesCompleted: 1,
                intelCollected: 2,
                intelRequired: 3,
                timeBudgetSeconds: 600f,
                extractionRouteId: "coolant_tunnel",
                extractionRouteLabel: "Coolant Tunnel",
                isMainRoute: false,
                routeRiskTier: 3,
                routeCreditMultiplier: 1.25f,
                routeSecondaryObjectiveBonus: 1);

            Assert.LessOrEqual(pressuredOptional.CreditsEarned, cleanMain.CreditsEarned);
        }

        private void SetPrivateField(string fieldName, object value)
        {
            var field = typeof(GameManager).GetField(fieldName, global::System.Reflection.BindingFlags.Instance | global::System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing private field {fieldName}");
            field.SetValue(_gm, value);
        }
    }
}
