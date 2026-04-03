using INTIFALL.Data;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class SpawnCoverageTests
    {
        [Test]
        public void EnemySpawnAssets_HavePatrolCoveragePerLevel()
        {
            EnemySpawnData[] enemySpawns = Resources.LoadAll<EnemySpawnData>("INTIFALL/Spawns");
            Assert.GreaterOrEqual(enemySpawns.Length, 5, "Expected 5 enemy spawn assets.");

            foreach (EnemySpawnData spawn in enemySpawns)
            {
                Assert.IsNotNull(spawn, "EnemySpawnData entry is null.");
                Assert.IsNotNull(spawn.spawnPoints, $"spawnPoints missing for {spawn.levelName}");
                Assert.GreaterOrEqual(spawn.spawnPoints.Length, 6, $"Not enough enemy spawn points in {spawn.levelName}");
                Assert.IsNotNull(spawn.availablePatrolRoutes, $"availablePatrolRoutes missing for {spawn.levelName}");
                Assert.GreaterOrEqual(spawn.availablePatrolRoutes.Length, 3, $"Patrol route variety too low in {spawn.levelName}");
                Assert.GreaterOrEqual(spawn.maxConcurrentAlert, 2, $"maxConcurrentAlert too low in {spawn.levelName}");
                Assert.GreaterOrEqual(spawn.communicationGroupSize, 3, $"communicationGroupSize too low in {spawn.levelName}");
            }
        }

        [Test]
        public void IntelSpawnAssets_HaveMissionLoopCoveragePerLevel()
        {
            IntelSpawnData[] intelSpawns = Resources.LoadAll<IntelSpawnData>("INTIFALL/Spawns");
            Assert.GreaterOrEqual(intelSpawns.Length, 5, "Expected 5 intel spawn assets.");

            foreach (IntelSpawnData spawn in intelSpawns)
            {
                Assert.IsNotNull(spawn, "IntelSpawnData entry is null.");
                Assert.IsNotNull(spawn.intelPoints, $"intelPoints missing for {spawn.levelName}");
                Assert.GreaterOrEqual(spawn.intelPoints.Length, 5, $"intelPoints too low in {spawn.levelName}");

                Assert.IsNotNull(spawn.supplyPoints, $"supplyPoints missing for {spawn.levelName}");
                Assert.GreaterOrEqual(spawn.supplyPoints.Length, 3, $"supplyPoints too low in {spawn.levelName}");

                Assert.IsNotNull(spawn.exitPoints, $"exitPoints missing for {spawn.levelName}");
                Assert.GreaterOrEqual(spawn.exitPoints.Length, 2, $"exitPoints too low in {spawn.levelName}");

                bool hasMainExit = false;
                bool hasOptionalExit = false;
                for (int i = 0; i < spawn.exitPoints.Length; i++)
                {
                    ExitPointData exit = spawn.exitPoints[i];
                    Assert.IsFalse(string.IsNullOrWhiteSpace(exit.routeId), $"routeId missing for {spawn.levelName} exit {i}");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(exit.routeLabel), $"routeLabel missing for {spawn.levelName} exit {i}");
                    Assert.GreaterOrEqual(exit.routeRiskTier, 0, $"routeRiskTier below 0 for {spawn.levelName} exit {i}");
                    Assert.LessOrEqual(exit.routeRiskTier, 3, $"routeRiskTier above 3 for {spawn.levelName} exit {i}");
                    Assert.GreaterOrEqual(exit.creditMultiplier, 0.5f, $"creditMultiplier below range for {spawn.levelName} exit {i}");
                    Assert.LessOrEqual(exit.creditMultiplier, 2f, $"creditMultiplier above range for {spawn.levelName} exit {i}");

                    hasMainExit |= exit.isMainExit;
                    hasOptionalExit |= !exit.isMainExit;
                }

                Assert.IsTrue(hasMainExit, $"No main extraction route in {spawn.levelName}");
                Assert.IsTrue(hasOptionalExit, $"No optional extraction route in {spawn.levelName}");

                Assert.IsNotNull(spawn.ventEntrancePositions, $"ventEntrancePositions missing for {spawn.levelName}");
                Assert.IsNotNull(spawn.ventExitPositions, $"ventExitPositions missing for {spawn.levelName}");
                Assert.GreaterOrEqual(spawn.ventEntrancePositions.Length, 1, $"ventEntrancePositions too low in {spawn.levelName}");
                Assert.GreaterOrEqual(spawn.ventExitPositions.Length, 1, $"ventExitPositions too low in {spawn.levelName}");
            }
        }
    }
}
