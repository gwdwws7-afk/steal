using System.Collections;
using System.Collections.Generic;
using System.Linq;
using INTIFALL.AI;
using INTIFALL.Core;
using INTIFALL.Level;
using INTIFALL.System;
using INTIFALL.Tools;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class Iteration13P3PerformanceStabilityGatePlayModeTests
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
        public IEnumerator CoreScenes_FivePass_PerformanceAndLeakGate_Passes()
        {
            int baselineLevelLoadedSubscribers = EventBus.GetSubscriberCount<LevelLoadedEvent>();
            int baselineOutcomeSubscribers = EventBus.GetSubscriberCount<MissionOutcomeEvaluatedEvent>();
            int baselineToolUsedSubscribers = EventBus.GetSubscriberCount<ToolUsedEvent>();
            int baselineAlertSubscribers = EventBus.GetSubscriberCount<AlertStateChangedEvent>();

            int startGcGen0 = global::System.GC.CollectionCount(0);
            int startGcGen1 = global::System.GC.CollectionCount(1);
            long startManagedMemory = global::System.GC.GetTotalMemory(true);

            var frameSamplesMs = new List<float>(1024);
            var sceneSwitchSamplesMs = new List<float>(128);
            int errorLikeLogCount = 0;

            Application.LogCallback logCallback = (_, _, type) =>
            {
                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                    errorLikeLogCount++;
            };

            Application.logMessageReceived += logCallback;

            const int passCount = 5;
            const int sampleFramesPerScene = 24;

            try
            {
                for (int pass = 0; pass < passCount; pass++)
                {
                    for (int i = 0; i < CoreSceneNames.Length; i++)
                    {
                        string sceneName = CoreSceneNames[i];

                        float switchStart = Time.realtimeSinceStartup;
                        SceneManager.LoadScene(sceneName);
                        yield return null;
                        yield return null;
                        yield return null;
                        sceneSwitchSamplesMs.Add((Time.realtimeSinceStartup - switchStart) * 1000f);

                        Assert.IsNotNull(Object.FindFirstObjectByType<LevelLoader>(), $"Missing LevelLoader in {sceneName}");

                        int gameManagerCount = Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length;
                        int saveLoadManagerCount = Object.FindObjectsByType<SaveLoadManager>(FindObjectsSortMode.None).Length;
                        Assert.LessOrEqual(gameManagerCount, 1, $"GameManager accumulation detected in {sceneName}");
                        Assert.LessOrEqual(saveLoadManagerCount, 1, $"SaveLoadManager accumulation detected in {sceneName}");

                        Assert.LessOrEqual(EventBus.GetSubscriberCount<LevelLoadedEvent>(), 18, $"LevelLoaded subscribers too high in {sceneName}");
                        Assert.LessOrEqual(EventBus.GetSubscriberCount<MissionOutcomeEvaluatedEvent>(), 16, $"MissionOutcome subscribers too high in {sceneName}");
                        Assert.LessOrEqual(EventBus.GetSubscriberCount<ToolUsedEvent>(), 18, $"ToolUsed subscribers too high in {sceneName}");
                        Assert.LessOrEqual(EventBus.GetSubscriberCount<AlertStateChangedEvent>(), 18, $"AlertState subscribers too high in {sceneName}");
                        Assert.LessOrEqual(EnemySquadCoordinator.ActiveEnemyCount, 128, $"Enemy squad registry growth detected in {sceneName}");

                        for (int frame = 0; frame < sampleFramesPerScene; frame++)
                        {
                            frameSamplesMs.Add(Time.unscaledDeltaTime * 1000f);
                            yield return null;
                        }
                    }
                }
            }
            finally
            {
                Application.logMessageReceived -= logCallback;
            }

            int gcGen0Delta = global::System.GC.CollectionCount(0) - startGcGen0;
            int gcGen1Delta = global::System.GC.CollectionCount(1) - startGcGen1;
            long endManagedMemory = global::System.GC.GetTotalMemory(true);
            float managedMemoryDeltaMb = (endManagedMemory - startManagedMemory) / (1024f * 1024f);

            Assert.Greater(frameSamplesMs.Count, 0, "Expected non-empty frame sample set.");
            Assert.Greater(sceneSwitchSamplesMs.Count, 0, "Expected non-empty scene switch sample set.");

            float frameAverageMs = frameSamplesMs.Average();
            float frameP95Ms = Percentile(frameSamplesMs, 0.95f);
            float switchAverageMs = sceneSwitchSamplesMs.Average();
            float switchP95Ms = Percentile(sceneSwitchSamplesMs, 0.95f);

            Assert.AreEqual(0, errorLikeLogCount, "Unexpected error/exception/assert logs detected during scene loops.");
            Assert.LessOrEqual(frameAverageMs, 90f, $"Average frame time too high: {frameAverageMs:0.00}ms");
            Assert.LessOrEqual(frameP95Ms, 200f, $"P95 frame time too high: {frameP95Ms:0.00}ms");
            Assert.LessOrEqual(switchAverageMs, 1200f, $"Average scene switch time too high: {switchAverageMs:0.00}ms");
            Assert.LessOrEqual(switchP95Ms, 2200f, $"P95 scene switch time too high: {switchP95Ms:0.00}ms");
            Assert.LessOrEqual(gcGen0Delta, 32, $"Gen0 GC count too high: {gcGen0Delta}");
            Assert.LessOrEqual(gcGen1Delta, 40, $"Gen1 GC count too high: {gcGen1Delta}");
            Assert.LessOrEqual(managedMemoryDeltaMb, 64f, $"Managed memory drift too high: {managedMemoryDeltaMb:0.00} MB");

            Assert.LessOrEqual(
                EventBus.GetSubscriberCount<LevelLoadedEvent>(),
                baselineLevelLoadedSubscribers + 4,
                "LevelLoadedEvent subscriber delta indicates a leak.");
            Assert.LessOrEqual(
                EventBus.GetSubscriberCount<MissionOutcomeEvaluatedEvent>(),
                baselineOutcomeSubscribers + 4,
                "MissionOutcomeEvaluatedEvent subscriber delta indicates a leak.");
            Assert.LessOrEqual(
                EventBus.GetSubscriberCount<ToolUsedEvent>(),
                baselineToolUsedSubscribers + 4,
                "ToolUsedEvent subscriber delta indicates a leak.");
            Assert.LessOrEqual(
                EventBus.GetSubscriberCount<AlertStateChangedEvent>(),
                baselineAlertSubscribers + 4,
                "AlertStateChangedEvent subscriber delta indicates a leak.");
        }

        private static float Percentile(List<float> samples, float percentile)
        {
            if (samples == null || samples.Count == 0)
                return 0f;

            List<float> ordered = samples.OrderBy(v => v).ToList();
            int index = Mathf.Clamp(Mathf.FloorToInt((ordered.Count - 1) * Mathf.Clamp01(percentile)), 0, ordered.Count - 1);
            return ordered[index];
        }
    }
}
