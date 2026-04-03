using System.Collections;
using INTIFALL.AI;
using INTIFALL.Data;
using INTIFALL.Level;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class Iteration13AITuningPlayModeTests
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
        public IEnumerator CoreScenes_LevelDrivenAiTimingProfile_IsApplied()
        {
            float previousAlertDuration = 0f;
            float previousFullAlertDelay = 0f;
            float previousDetectionPulse = float.MaxValue;

            for (int i = 0; i < CoreSceneNames.Length; i++)
            {
                string sceneName = CoreSceneNames[i];
                SceneManager.LoadScene(sceneName);
                yield return null;
                yield return null;

                LevelLoader loader = Object.FindFirstObjectByType<LevelLoader>();
                Assert.IsNotNull(loader, $"LevelLoader missing in {sceneName}");

                LevelData data = loader.GetLevelData();
                Assert.IsNotNull(data, $"LevelData missing in {sceneName}");

                EnemyStateMachine stateMachine = Object.FindFirstObjectByType<EnemyStateMachine>();
                EnemyController controller = Object.FindFirstObjectByType<EnemyController>();
                Assert.IsNotNull(stateMachine, $"EnemyStateMachine missing in {sceneName}");
                Assert.IsNotNull(controller, $"EnemyController missing in {sceneName}");

                int pressureTier = Mathf.Clamp(data.patrolPressureTier, 1, 5);
                float tierT = Mathf.InverseLerp(1f, 5f, pressureTier);

                float expectedAlertDuration = Mathf.Max(1f, data.baseAlertDuration);
                float expectedSearchDuration = Mathf.Max(1f, data.searchDuration);
                float expectedFullAlertFailDelay = Mathf.Max(expectedAlertDuration + 4f, data.fullAlertDuration);
                float expectedAlertDropDelay = Mathf.Lerp(1.0f, 1.6f, tierT);
                float expectedSuspiciousDuration = Mathf.Lerp(2.0f, 2.8f, tierT);
                float expectedDetectionPulse = Mathf.Lerp(0.85f, 0.55f, tierT);
                float expectedSearchingPulseMultiplier = Mathf.Lerp(0.9f, 0.7f, tierT);
                float expectedAlertedPulseMultiplier = Mathf.Lerp(0.65f, 0.45f, tierT);

                Assert.AreEqual(expectedAlertDuration, stateMachine.AlertDuration, 0.001f, $"Alert duration mismatch in {sceneName}");
                Assert.AreEqual(expectedSearchDuration, stateMachine.SearchDuration, 0.001f, $"Search duration mismatch in {sceneName}");
                Assert.AreEqual(expectedFullAlertFailDelay, stateMachine.FullAlertMissionFailDelay, 0.001f, $"Full-alert fail delay mismatch in {sceneName}");
                Assert.AreEqual(expectedAlertDropDelay, stateMachine.AlertDropToSearchDelay, 0.001f, $"Alert drop delay mismatch in {sceneName}");
                Assert.AreEqual(expectedSuspiciousDuration, stateMachine.SuspiciousDuration, 0.001f, $"Suspicious duration mismatch in {sceneName}");

                Assert.AreEqual(expectedDetectionPulse, controller.DetectionPulseInterval, 0.001f, $"Detection pulse mismatch in {sceneName}");
                Assert.AreEqual(expectedSearchingPulseMultiplier, controller.SearchingPulseMultiplier, 0.001f, $"Searching pulse multiplier mismatch in {sceneName}");
                Assert.AreEqual(expectedAlertedPulseMultiplier, controller.AlertedPulseMultiplier, 0.001f, $"Alerted pulse multiplier mismatch in {sceneName}");

                if (i > 0)
                {
                    Assert.GreaterOrEqual(
                        stateMachine.AlertDuration,
                        previousAlertDuration,
                        $"Alert duration should not decrease across level curve ({CoreSceneNames[i - 1]} -> {sceneName}).");
                    Assert.GreaterOrEqual(
                        stateMachine.FullAlertMissionFailDelay,
                        previousFullAlertDelay,
                        $"Full-alert fail delay should not decrease across level curve ({CoreSceneNames[i - 1]} -> {sceneName}).");
                    Assert.LessOrEqual(
                        controller.DetectionPulseInterval,
                        previousDetectionPulse,
                        $"Detection pulse interval should not increase with higher pressure ({CoreSceneNames[i - 1]} -> {sceneName}).");
                }

                previousAlertDuration = stateMachine.AlertDuration;
                previousFullAlertDelay = stateMachine.FullAlertMissionFailDelay;
                previousDetectionPulse = controller.DetectionPulseInterval;
            }
        }
    }
}
