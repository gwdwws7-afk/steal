using INTIFALL.System;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class GameManagerTests
    {
        private GameManager _gm;

        [SetUp]
        public void Setup()
        {
            var go = new GameObject("GameManager");
            _gm = go.AddComponent<GameManager>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_gm.gameObject);
        }

        [Test]
        public void StartGame_ShouldSetStateToPlaying()
        {
            _gm.StartGame();
            Assert.AreEqual(EGameState.Playing, _gm.CurrentState);
        }

        [Test]
        public void AddCredits_ShouldIncreaseCredits()
        {
            _gm.AddCredits(100);
            Assert.AreEqual(100, _gm.PlayerCredits);
        }

        [Test]
        public void SpendCredits_ShouldDecreaseCredits()
        {
            _gm.AddCredits(100);
            _gm.SpendCredits(30);
            Assert.AreEqual(70, _gm.PlayerCredits);
        }

        [Test]
        public void SpendCredits_ShouldNotGoBelowZero()
        {
            _gm.AddCredits(10);
            _gm.SpendCredits(100);
            Assert.AreEqual(0, _gm.PlayerCredits);
        }

        [Test]
        public void RecordEnemyKill_ShouldIncrementKillCount()
        {
            _gm.RecordEnemyKill();
            _gm.RecordEnemyKill();
            Assert.AreEqual(2, _gm.EnemiesKilled);
        }

        [Test]
        public void RecordEnemyKnockout_ShouldIncrementKO()
        {
            _gm.RecordEnemyKnockout();
            Assert.AreEqual(1, _gm.EnemiesKnockedOut);
        }

        [Test]
        public void RecordDiscovery_ShouldSetDiscoveredTrue()
        {
            Assert.IsFalse(_gm.WasDiscovered);
            _gm.RecordDiscovery();
            Assert.IsTrue(_gm.WasDiscovered);
        }

        [Test]
        public void RecordFullAlert_ShouldSetAlertTrue()
        {
            Assert.IsFalse(_gm.FullAlertTriggered);
            _gm.RecordFullAlert();
            Assert.IsTrue(_gm.FullAlertTriggered);
        }

        [Test]
        public void LoadLevel_ShouldSetLevelInfo()
        {
            _gm.LoadLevel(2, "Priest Eye");
            Assert.AreEqual(2, _gm.CurrentLevelIndex);
            Assert.AreEqual("Priest Eye", _gm.CurrentLevelName);
            Assert.AreEqual(EGameState.Playing, _gm.CurrentState);
        }

        [Test]
        public void PauseGame_ShouldSetStateToPaused()
        {
            _gm.StartGame();
            _gm.PauseGame();
            Assert.AreEqual(EGameState.Paused, _gm.CurrentState);
        }

        [Test]
        public void ResumeGame_ShouldReturnToPlaying()
        {
            _gm.StartGame();
            _gm.PauseGame();
            _gm.ResumeGame();
            Assert.AreEqual(EGameState.Playing, _gm.CurrentState);
        }

        [Test]
        public void CalculateMissionResult_UndiscoveredNoKills_ShouldReturnS()
        {
            _gm.LoadLevel(0, "test");
            _gm.StartGame();
            _gm.RecordEnemyKnockout();
            var result = _gm.CalculateMissionResult();
            Assert.AreEqual("S", result.Rank);
        }

        [Test]
        public void CalculateMissionResult_FullAlert_ShouldReturnC()
        {
            _gm.LoadLevel(0, "test");
            _gm.StartGame();
            _gm.RecordFullAlert();
            var result = _gm.CalculateMissionResult();
            Assert.AreEqual("C", result.Rank);
        }

        [Test]
        public void GameOver_ShouldSetStateToGameOver()
        {
            _gm.StartGame();
            _gm.GameOver();
            Assert.AreEqual(EGameState.GameOver, _gm.CurrentState);
        }

        [Test]
        public void LevelComplete_ShouldSetStateToLevelComplete()
        {
            _gm.StartGame();
            _gm.LevelComplete();
            Assert.AreEqual(EGameState.LevelComplete, _gm.CurrentState);
        }

        [Test]
        public void CalculateMissionResult_WithObjectiveInputs_ReturnsRankAndCredits()
        {
            _gm.LoadLevel(1, "Level02");
            _gm.StartGame();

            MissionResult result = _gm.CalculateMissionResult(
                secondaryObjectivesCompleted: 2,
                intelCollected: 3,
                intelRequired: 3,
                timeBudgetSeconds: 300f);

            Assert.AreEqual("S", result.Rank);
            Assert.Greater(result.CreditsEarned, 0);
            Assert.AreEqual(3, result.IntelCollected);
            Assert.AreEqual(3, result.IntelRequired);
            Assert.AreEqual("Main Extraction", result.ExtractionRouteLabel);
        }

        [Test]
        public void CalculateMissionResult_WithOptionalRoute_UpdatesRouteFields()
        {
            _gm.LoadLevel(2, "Level03");
            _gm.StartGame();

            MissionResult result = _gm.CalculateMissionResult(
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

            Assert.IsTrue(result.UsedOptionalExit);
            Assert.AreEqual("coolant_tunnel", result.ExtractionRouteId);
            Assert.AreEqual("Coolant Tunnel", result.ExtractionRouteLabel);
            Assert.AreEqual(3, result.RouteRiskTier);
            Assert.AreEqual(1.25f, result.RouteCreditMultiplier, 0.001f);
            Assert.Greater(result.CreditsEarned, 0);
            Assert.GreaterOrEqual(result.SecondaryObjectivesEvaluated, result.SecondaryObjectivesCompleted);
        }

        [Test]
        public void CalculateMissionResult_WithSecondaryTotalInput_PropagatesToResult()
        {
            _gm.LoadLevel(2, "Level03");
            _gm.StartGame();

            MissionResult result = _gm.CalculateMissionResult(
                secondaryObjectivesCompleted: 2,
                intelCollected: 3,
                intelRequired: 3,
                timeBudgetSeconds: 600f,
                extractionRouteId: "coolant_tunnel",
                extractionRouteLabel: "Coolant Tunnel",
                isMainRoute: false,
                routeRiskTier: 2,
                routeCreditMultiplier: 1.2f,
                routeSecondaryObjectiveBonus: 1,
                secondaryObjectivesTotal: 4);

            Assert.AreEqual(2, result.SecondaryObjectivesCompleted);
            Assert.AreEqual(3, result.SecondaryObjectivesEvaluated);
            Assert.AreEqual(5, result.SecondaryObjectivesTotal);
        }

        [Test]
        public void CalculateMissionResult_DefaultAndExplicitMainRoute_UseSameSettlementFields()
        {
            _gm.LoadLevel(1, "Level02");
            _gm.StartGame();

            MissionResult simple = _gm.CalculateMissionResult(
                secondaryObjectivesCompleted: 1,
                intelCollected: 2,
                intelRequired: 3,
                timeBudgetSeconds: 450f);

            MissionResult explicitMain = _gm.CalculateMissionResult(
                secondaryObjectivesCompleted: 1,
                intelCollected: 2,
                intelRequired: 3,
                timeBudgetSeconds: 450f,
                extractionRouteId: "main",
                extractionRouteLabel: "Main Extraction",
                isMainRoute: true,
                routeRiskTier: 0,
                routeCreditMultiplier: 1f,
                routeSecondaryObjectiveBonus: 0,
                secondaryObjectivesTotal: 2);

            Assert.AreEqual(simple.Rank, explicitMain.Rank);
            Assert.AreEqual(simple.CreditsEarned, explicitMain.CreditsEarned);
            Assert.AreEqual(simple.ExtractionRouteId, explicitMain.ExtractionRouteId);
            Assert.AreEqual(simple.SecondaryObjectivesTotal, explicitMain.SecondaryObjectivesTotal);
        }

        [Test]
        public void ApplyMissionResult_AddsCreditsAndSetsLevelComplete()
        {
            _gm.StartGame();

            MissionResult result = new MissionResult
            {
                CreditsEarned = 220,
                Rank = "B"
            };

            _gm.ApplyMissionResult(result);

            Assert.AreEqual(220, _gm.PlayerCredits);
            Assert.AreEqual(EGameState.LevelComplete, _gm.CurrentState);
        }
    }
}
