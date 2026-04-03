using System.Collections;
using System.Collections.Generic;
using System.Linq;
using INTIFALL.AI;
using INTIFALL.Level;
using INTIFALL.System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class Iteration9PerformanceGatePlayModeTests
    {
        private static readonly string[] CoreSceneNames =
        {
            "Level01_Qhapaq_Passage",
            "Level02_Temple_Complex",
            "Level03_Underground_Labs",
            "Level04_Qhipu_Core",
            "Level05_General_Taki_Villa"
        };

        [UnityTest]
        public IEnumerator CoreScenes_StressLoop_StaysWithinRuntimeBudget()
        {
            int startGcGen0 = global::System.GC.CollectionCount(0);
            List<float> frameMsSamples = new(256);

            const int passCount = 3;
            const int sampleFramesPerScene = 20;

            for (int pass = 0; pass < passCount; pass++)
            {
                for (int i = 0; i < CoreSceneNames.Length; i++)
                {
                    string sceneName = CoreSceneNames[i];
                    SceneManager.LoadScene(sceneName);
                    yield return null;
                    yield return null;
                    yield return null;

                    Assert.IsNotNull(Object.FindFirstObjectByType<LevelLoader>(), $"Missing LevelLoader in {sceneName}");

                    for (int frame = 0; frame < sampleFramesPerScene; frame++)
                    {
                        frameMsSamples.Add(Time.unscaledDeltaTime * 1000f);
                        yield return null;
                    }

                    int gameManagerCount = Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length;
                    Assert.LessOrEqual(gameManagerCount, 1, $"GameManager accumulation detected in {sceneName}");

                    int levelLoadedSubscribers = EventBus.GetSubscriberCount<LevelLoadedEvent>();
                    int outcomeSubscribers = EventBus.GetSubscriberCount<MissionOutcomeEvaluatedEvent>();
                    Assert.LessOrEqual(levelLoadedSubscribers, 12, $"LevelLoadedEvent subscribers too high in {sceneName}");
                    Assert.LessOrEqual(outcomeSubscribers, 10, $"MissionOutcomeEvaluatedEvent subscribers too high in {sceneName}");

                    Assert.LessOrEqual(EnemySquadCoordinator.ActiveEnemyCount, 96, $"Enemy squad registry growth detected in {sceneName}");
                }
            }

            Assert.Greater(frameMsSamples.Count, 0, "Expected non-empty frame sample set.");

            float averageMs = frameMsSamples.Average();
            List<float> orderedSamples = frameMsSamples.OrderBy(x => x).ToList();
            int p95Index = Mathf.Clamp(Mathf.FloorToInt(orderedSamples.Count * 0.95f), 0, orderedSamples.Count - 1);
            float p95Ms = orderedSamples[p95Index];
            int gcGen0Delta = global::System.GC.CollectionCount(0) - startGcGen0;

            Assert.LessOrEqual(averageMs, 80f, $"Average frame time too high: {averageMs:0.00}ms");
            Assert.LessOrEqual(p95Ms, 180f, $"P95 frame time too high: {p95Ms:0.00}ms");
            Assert.LessOrEqual(gcGen0Delta, 20, $"Gen0 GC count too high: {gcGen0Delta}");
        }
    }
}
