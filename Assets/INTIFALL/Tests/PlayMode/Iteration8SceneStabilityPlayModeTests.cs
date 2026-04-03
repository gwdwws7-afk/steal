using System.Collections;
using INTIFALL.AI;
using INTIFALL.Level;
using INTIFALL.System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class Iteration8SceneStabilityPlayModeTests
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
        public IEnumerator CoreScenes_TwoPassLoop_DoesNotAccumulateRuntimeManagersOrSubscribers()
        {
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < CoreSceneNames.Length; i++)
                {
                    string sceneName = CoreSceneNames[i];
                    SceneManager.LoadScene(sceneName);
                    yield return null;
                    yield return null;

                    Assert.IsNotNull(Object.FindFirstObjectByType<LevelLoader>(), $"Missing LevelLoader in {sceneName}");

                    int gameManagerCount = Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length;
                    Assert.LessOrEqual(gameManagerCount, 1, $"GameManager accumulation detected in {sceneName}");

                    int levelLoadedSubscribers = EventBus.GetSubscriberCount<LevelLoadedEvent>();
                    int outcomeSubscribers = EventBus.GetSubscriberCount<MissionOutcomeEvaluatedEvent>();
                    Assert.LessOrEqual(levelLoadedSubscribers, 10, $"LevelLoadedEvent subscribers too high in {sceneName}");
                    Assert.LessOrEqual(outcomeSubscribers, 8, $"MissionOutcomeEvaluatedEvent subscribers too high in {sceneName}");

                    Assert.LessOrEqual(EnemySquadCoordinator.ActiveEnemyCount, 64, $"Enemy squad registry growth detected in {sceneName}");
                }
            }
        }
    }
}
