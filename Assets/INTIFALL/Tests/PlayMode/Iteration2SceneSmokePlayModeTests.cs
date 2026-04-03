using System.Collections;
using INTIFALL.Level;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class Iteration2SceneSmokePlayModeTests
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
        public IEnumerator CoreScenes_LoadAndSpawnLoopActors()
        {
            for (int i = 0; i < CoreSceneNames.Length; i++)
            {
                string sceneName = CoreSceneNames[i];
                Assert.IsTrue(
                    Application.CanStreamedLevelBeLoaded(sceneName),
                    $"Scene not loadable via build settings: {sceneName}");

                SceneManager.LoadScene(sceneName);
                yield return null;
                yield return null;

                var loader = Object.FindFirstObjectByType<LevelLoader>();
                Assert.IsNotNull(loader, $"LevelLoader missing in {sceneName}");
                Assert.Greater(loader.EnemiesSpawned, 0, $"No enemies spawned in {sceneName}");
                Assert.Greater(loader.IntelSpawned, 0, $"No intel spawned in {sceneName}");
                Assert.IsNotNull(Object.FindFirstObjectByType<MissionExitPoint>(), $"MissionExitPoint missing in {sceneName}");

                GameObject player = TryFindPlayer();
                Assert.IsNotNull(player, $"Player missing in {sceneName}");
            }
        }

        private static GameObject TryFindPlayer()
        {
            try
            {
                return GameObject.FindGameObjectWithTag("Player");
            }
            catch (UnityException)
            {
                return null;
            }
        }
    }
}
