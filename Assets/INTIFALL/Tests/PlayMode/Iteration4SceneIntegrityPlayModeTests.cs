using System.Collections;
using INTIFALL.Level;
using INTIFALL.Narrative;
using INTIFALL.System;
using INTIFALL.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class Iteration4SceneIntegrityPlayModeTests
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
        public IEnumerator CoreScenes_RuntimeIntegrityAndMissionCoverage_Pass()
        {
            for (int i = 0; i < CoreSceneNames.Length; i++)
            {
                string sceneName = CoreSceneNames[i];
                Assert.IsTrue(Application.CanStreamedLevelBeLoaded(sceneName), $"Scene is not loadable: {sceneName}");

                SceneManager.LoadScene(sceneName);
                yield return null;
                yield return null;

                var loader = Object.FindFirstObjectByType<LevelLoader>();
                Assert.IsNotNull(loader, $"LevelLoader missing in {sceneName}");
                Assert.IsNotNull(loader.GetLevelData(), $"LevelData missing in {sceneName}");
                Assert.IsNotNull(loader.GetEnemySpawnData(), $"EnemySpawnData missing in {sceneName}");
                Assert.IsNotNull(loader.GetIntelSpawnData(), $"IntelSpawnData missing in {sceneName}");

                Assert.Greater(loader.EnemiesSpawned, 0, $"No enemies spawned in {sceneName}");
                Assert.Greater(loader.IntelSpawned, 0, $"No intel spawned in {sceneName}");

                Assert.IsNotNull(Object.FindFirstObjectByType<GameManager>(), $"GameManager missing in {sceneName}");
                Assert.IsNotNull(Object.FindFirstObjectByType<LevelFlowManager>(), $"LevelFlowManager missing in {sceneName}");
                Assert.IsNotNull(Object.FindFirstObjectByType<NarrativeManager>(), $"NarrativeManager missing in {sceneName}");
                Assert.IsNotNull(Object.FindFirstObjectByType<WillaComm>(), $"WillaComm missing in {sceneName}");

                Assert.IsNotNull(Object.FindFirstObjectByType<HUDManager>(), $"HUDManager missing in {sceneName}");
                Assert.IsNotNull(Object.FindFirstObjectByType<PauseMenuUI>(), $"PauseMenuUI missing in {sceneName}");
                Assert.IsNotNull(Object.FindFirstObjectByType<MissionDebriefUI>(), $"MissionDebriefUI missing in {sceneName}");

                Assert.IsNotNull(Object.FindFirstObjectByType<MissionExitPoint>(), $"MissionExitPoint missing in {sceneName}");

                var intelData = loader.GetIntelSpawnData();
                Assert.IsNotNull(intelData.intelPoints, $"intelPoints array null in {sceneName}");
                Assert.IsNotNull(intelData.supplyPoints, $"supplyPoints array null in {sceneName}");
                Assert.IsNotNull(intelData.exitPoints, $"exitPoints array null in {sceneName}");
                Assert.Greater(intelData.intelPoints.Length, 0, $"intelPoints empty in {sceneName}");
                Assert.Greater(intelData.supplyPoints.Length, 0, $"supplyPoints empty in {sceneName}");
                Assert.Greater(intelData.exitPoints.Length, 0, $"exitPoints empty in {sceneName}");
            }
        }
    }
}
