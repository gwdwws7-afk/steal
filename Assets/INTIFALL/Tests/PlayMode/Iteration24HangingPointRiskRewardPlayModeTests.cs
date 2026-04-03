using System.Collections;
using System.Collections.Generic;
using INTIFALL.Data;
using INTIFALL.Environment;
using INTIFALL.Level;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class Iteration24HangingPointRiskRewardPlayModeTests
    {
        private static readonly string[] CoreSceneNames =
        {
            "Level01_Qhapaq_Passage",
            "Level02_Temple_Complex",
            "Level03_Underground_Labs",
            "Level04_Qhipu_Core",
            "Level05_General_Taki_Villa"
        };

        private const float HighRiskDistance = 8f;
        private const float MediumRiskDistance = 14f;
        private const float LowRiskDistance = 22f;

        private const float HighRewardDistance = 7f;
        private const float MediumRewardDistance = 13f;
        private const float LowRewardDistance = 20f;

        private const float DeadPointEnemyDistance = 35f;
        private const float DeadPointObjectiveDistance = 30f;

        [UnityTest]
        public IEnumerator CoreScenes_HangingPointRiskRewardThresholds_Pass()
        {
            for (int i = 0; i < CoreSceneNames.Length; i++)
            {
                string sceneName = CoreSceneNames[i];
                SceneManager.LoadScene(sceneName);
                yield return null;
                yield return null;

                LevelLoader loader = Object.FindFirstObjectByType<LevelLoader>();
                Assert.IsNotNull(loader, $"LevelLoader missing in {sceneName}");

                LevelData levelData = loader.GetLevelData();
                EnemySpawnData enemyData = loader.GetEnemySpawnData();
                IntelSpawnData intelData = loader.GetIntelSpawnData();
                Assert.IsNotNull(levelData, $"LevelData missing in {sceneName}");
                Assert.IsNotNull(enemyData, $"EnemySpawnData missing in {sceneName}");
                Assert.IsNotNull(intelData, $"IntelSpawnData missing in {sceneName}");

                HangingPoint[] points = Object.FindObjectsByType<HangingPoint>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);

                if (!levelData.hasHangingPoints)
                    continue;

                Assert.GreaterOrEqual(points.Length, 1, $"Expected hanging points in {sceneName}.");

                List<Vector3> enemyPositions = CollectEnemyPositions(enemyData);
                List<Vector3> objectivePositions = CollectObjectivePositions(intelData);
                Assert.Greater(enemyPositions.Count, 0, $"Enemy position set is empty in {sceneName}.");
                Assert.Greater(objectivePositions.Count, 0, $"Objective position set is empty in {sceneName}.");

                int mediumOrAboveRisk = 0;
                int mediumOrAboveReward = 0;
                int deadPointCount = 0;

                for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
                {
                    Vector3 pointPosition = points[pointIndex].transform.position;
                    float nearestEnemyDistance = GetNearestDistance(pointPosition, enemyPositions);
                    float nearestObjectiveDistance = GetNearestDistance(pointPosition, objectivePositions);

                    int riskTier = ClassifyTier(nearestEnemyDistance, HighRiskDistance, MediumRiskDistance, LowRiskDistance);
                    int rewardTier = ClassifyTier(nearestObjectiveDistance, HighRewardDistance, MediumRewardDistance, LowRewardDistance);

                    if (riskTier >= 2)
                        mediumOrAboveRisk++;
                    if (rewardTier >= 2)
                        mediumOrAboveReward++;
                    if (nearestEnemyDistance > DeadPointEnemyDistance && nearestObjectiveDistance > DeadPointObjectiveDistance)
                        deadPointCount++;

                    Debug.Log(
                        $"[I24][{sceneName}] HangingPoint#{pointIndex} " +
                        $"enemyDist={nearestEnemyDistance:F2} riskTier={riskTier} " +
                        $"objectiveDist={nearestObjectiveDistance:F2} rewardTier={rewardTier}");
                }

                Assert.GreaterOrEqual(
                    mediumOrAboveRisk,
                    1,
                    $"No medium-or-higher risk hanging point found in {sceneName}.");
                Assert.GreaterOrEqual(
                    mediumOrAboveReward,
                    1,
                    $"No medium-or-higher reward hanging point found in {sceneName}.");
                Assert.AreEqual(
                    0,
                    deadPointCount,
                    $"Found hanging points with both low tactical pressure and low objective relevance in {sceneName}.");
            }
        }

        private static List<Vector3> CollectEnemyPositions(EnemySpawnData data)
        {
            List<Vector3> result = new List<Vector3>();
            if (data == null || data.spawnPoints == null)
                return result;

            for (int i = 0; i < data.spawnPoints.Length; i++)
            {
                result.Add(data.spawnPoints[i].position);
            }

            return result;
        }

        private static List<Vector3> CollectObjectivePositions(IntelSpawnData data)
        {
            List<Vector3> result = new List<Vector3>();
            if (data == null)
                return result;

            if (data.intelPoints != null)
            {
                for (int i = 0; i < data.intelPoints.Length; i++)
                    result.Add(data.intelPoints[i].position);
            }

            if (data.supplyPoints != null)
            {
                for (int i = 0; i < data.supplyPoints.Length; i++)
                    result.Add(data.supplyPoints[i].position);
            }

            if (data.exitPoints != null)
            {
                for (int i = 0; i < data.exitPoints.Length; i++)
                    result.Add(data.exitPoints[i].position);
            }

            return result;
        }

        private static float GetNearestDistance(Vector3 origin, List<Vector3> candidates)
        {
            if (candidates == null || candidates.Count == 0)
                return float.PositiveInfinity;

            float best = float.PositiveInfinity;
            for (int i = 0; i < candidates.Count; i++)
            {
                float dist = Vector3.Distance(origin, candidates[i]);
                if (dist < best)
                    best = dist;
            }

            return best;
        }

        private static int ClassifyTier(float distance, float high, float medium, float low)
        {
            if (distance <= high) return 3;
            if (distance <= medium) return 2;
            if (distance <= low) return 1;
            return 0;
        }
    }
}
