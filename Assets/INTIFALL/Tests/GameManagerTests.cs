using NUnit.Framework;
using INTIFALL.System;
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
            _gm.LoadLevel(2, "祭司之眼");
            Assert.AreEqual(2, _gm.CurrentLevelIndex);
            Assert.AreEqual("祭司之眼", _gm.CurrentLevelName);
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
    }
}
