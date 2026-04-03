using System.Collections;
using INTIFALL.Data;
using INTIFALL.Environment;
using INTIFALL.Level;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class Iteration24HangingPointPatrolLinkPlayModeTests
    {
        private static readonly string[] CoreSceneNames =
        {
            "Level01_Qhapaq_Passage",
            "Level02_Temple_Complex",
            "Level03_Underground_Labs",
            "Level04_Qhipu_Core",
            "Level05_General_Taki_Villa"
        };

        private const float MinLinkedPatrolDistance = 4f;
        private const float MaxLinkedPatrolDistance = 16f;
        private const float HighPressureDistance = 8f;
        private const float CounterplayMinDistance = 6f;
        private const float CounterplayMaxDistance = 14f;

        [UnityTest]
        public IEnumerator CoreScenes_HangingPointPatrolLinkWindows_AreValid()
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
                EnemySpawnData enemySpawnData = loader.GetEnemySpawnData();
                Assert.IsNotNull(levelData, $"LevelData missing in {sceneName}");
                Assert.IsNotNull(enemySpawnData, $"EnemySpawnData missing in {sceneName}");
                Assert.IsNotNull(enemySpawnData.spawnPoints, $"spawnPoints missing in {sceneName}");
                Assert.Greater(enemySpawnData.spawnPoints.Length, 0, $"spawnPoints empty in {sceneName}");

                HangingPoint[] hangingPoints = Object.FindObjectsByType<HangingPoint>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);

                if (!levelData.hasHangingPoints)
                    continue;

                Assert.Greater(hangingPoints.Length, 0, $"No hanging points found in {sceneName}.");

                int highPressureCount = 0;
                int counterplayWindowCount = 0;

                for (int hpIndex = 0; hpIndex < hangingPoints.Length; hpIndex++)
                {
                    float nearestPatrolDist = GetNearestPatrolDistance(
                        hangingPoints[hpIndex].transform.position,
                        enemySpawnData.spawnPoints);

                    Assert.GreaterOrEqual(
                        nearestPatrolDist,
                        MinLinkedPatrolDistance,
                        $"Hanging point too close to patrol path in {sceneName} (index={hpIndex}).");
                    Assert.LessOrEqual(
                        nearestPatrolDist,
                        MaxLinkedPatrolDistance,
                        $"Hanging point not meaningfully linked to patrol in {sceneName} (index={hpIndex}).");

                    if (nearestPatrolDist <= HighPressureDistance)
                        highPressureCount++;
                    if (nearestPatrolDist >= CounterplayMinDistance && nearestPatrolDist <= CounterplayMaxDistance)
                        counterplayWindowCount++;

                    Debug.Log(
                        $"[I24-PATROL][{sceneName}] HangingPoint#{hpIndex} nearestPatrol={nearestPatrolDist:F2}");
                }

                Assert.GreaterOrEqual(
                    highPressureCount,
                    1,
                    $"Expected at least one high-pressure hanging point in {sceneName}.");
                Assert.GreaterOrEqual(
                    counterplayWindowCount,
                    1,
                    $"Expected at least one counterplay-window hanging point in {sceneName}.");
            }
        }

        private static float GetNearestPatrolDistance(Vector3 hangingPoint, EnemySpawnPoint[] spawns)
        {
            float best = float.PositiveInfinity;
            bool foundPatrol = false;

            for (int i = 0; i < spawns.Length; i++)
            {
                if (!spawns[i].isPatrol)
                    continue;

                foundPatrol = true;
                float dist = Vector3.Distance(hangingPoint, spawns[i].position);
                if (dist < best)
                    best = dist;
            }

            if (!foundPatrol)
            {
                for (int i = 0; i < spawns.Length; i++)
                {
                    float dist = Vector3.Distance(hangingPoint, spawns[i].position);
                    if (dist < best)
                        best = dist;
                }
            }

            return best;
        }
    }
}
