using System.Collections;
using System.Reflection;
using INTIFALL.Data;
using INTIFALL.Environment;
using INTIFALL.Level;
using INTIFALL.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class Iteration23HangingPointCoveragePlayModeTests
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
        public IEnumerator CoreScenes_HangingPointCoverageAndSafeDetach_Pass()
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
                Assert.IsNotNull(levelData, $"LevelData missing in {sceneName}");

                HangingPoint[] hangingPoints = Object.FindObjectsByType<HangingPoint>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);

                if (levelData.hasHangingPoints)
                {
                    Assert.GreaterOrEqual(
                        hangingPoints.Length,
                        1,
                        $"LevelData indicates hanging points, but none found in scene: {sceneName}");
                }

                if (hangingPoints.Length == 0)
                    continue;

                GameObject player = TryFindPlayer();
                Assert.IsNotNull(player, $"Player missing in {sceneName}");

                PlayerController controller = player.GetComponent<PlayerController>();
                Assert.IsNotNull(controller, $"PlayerController missing in {sceneName}");

                HangingPoint point = hangingPoints[0];
                player.transform.position = point.transform.position;
                AttachPointToPlayer(point, player);
                yield return null;

                Assert.IsTrue(point.IsOccupied, $"HangingPoint should be occupied after attach in {sceneName}");
                Assert.IsTrue(controller.IsOnRope, $"Player should enter rope state after attach in {sceneName}");

                // Move beyond detach range to validate safe auto-release path.
                player.transform.position = point.transform.position + new Vector3(0f, 0f, 6f);
                yield return null;
                yield return null;

                Assert.IsFalse(point.IsOccupied, $"HangingPoint should auto-release when player exits safe detach range in {sceneName}");
                Assert.IsFalse(controller.IsOnRope, $"Player should detach from rope when leaving safe range in {sceneName}");
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

        private static void AttachPointToPlayer(HangingPoint point, GameObject player)
        {
            SetPrivateField(point, "_player", player);
            InvokePrivateMethod(point, "Attach");
        }

        private static void InvokePrivateMethod(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Failed to reflect method: {methodName}");
            method.Invoke(target, null);
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Failed to reflect field: {fieldName}");
            field.SetValue(target, value);
        }
    }
}
