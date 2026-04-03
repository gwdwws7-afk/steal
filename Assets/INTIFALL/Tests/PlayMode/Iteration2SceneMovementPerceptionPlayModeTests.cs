using System.Collections;
using System.Reflection;
using INTIFALL.AI;
using INTIFALL.Level;
using INTIFALL.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class Iteration2SceneMovementPerceptionPlayModeTests
    {
        private const float AlertDropToSearchDelayForTests = 1.25f;

        private static readonly string[] CoreSceneNames =
        {
            "Level01_Qhapaq_Passage",
            "Level02_Temple_Complex",
            "Level03_Underground_Labs",
            "Level04_Qhipu_Core",
            "Level05_General_Taki_Villa"
        };

        [UnityTest]
        public IEnumerator CoreScenes_MovementAndPerceptionChains_WorkForSmoke()
        {
            for (int i = 0; i < CoreSceneNames.Length; i++)
            {
                string sceneName = CoreSceneNames[i];
                SceneManager.LoadScene(sceneName);
                yield return null;
                yield return null;

                var flow = Object.FindFirstObjectByType<LevelFlowManager>();
                if (flow != null)
                    Object.Destroy(flow.gameObject);

                GameObject player = TryFindPlayer();
                Assert.IsNotNull(player, $"Player missing in {sceneName}");

                var playerController = player.GetComponent<PlayerController>();
                Assert.IsNotNull(playerController, $"PlayerController missing in {sceneName}");

                ValidateMovementStateChain(playerController, sceneName);

                var enemyStateMachine = Object.FindFirstObjectByType<EnemyStateMachine>();
                Assert.IsNotNull(enemyStateMachine, $"EnemyStateMachine missing in {sceneName}");

                yield return ValidatePerceptionStateChain(enemyStateMachine, player.transform.position, sceneName);
            }
        }

        private static void ValidateMovementStateChain(PlayerController controller, string sceneName)
        {
            SetPrivateField(controller, "_moveInput", Vector3.forward);
            SetPrivateField(controller, "_sprintHeld", false);
            SetPrivateField(controller, "_crouchHeld", false);
            InvokePrivateMethod(controller, "HandleGroundMovement");
            Assert.AreEqual(EPlayerState.Walk, controller.State, $"Walk state transition failed in {sceneName}");
            Assert.Greater(controller.CurrentSpeed, 0f, $"Walk speed not applied in {sceneName}");

            SetPrivateField(controller, "_sprintHeld", true);
            SetPrivateField(controller, "_crouchHeld", false);
            InvokePrivateMethod(controller, "HandleGroundMovement");
            Assert.AreEqual(EPlayerState.Sprint, controller.State, $"Sprint state transition failed in {sceneName}");

            SetPrivateField(controller, "_sprintHeld", false);
            SetPrivateField(controller, "_crouchHeld", true);
            InvokePrivateMethod(controller, "HandleGroundMovement");
            Assert.AreEqual(EPlayerState.Crouch, controller.State, $"Crouch state transition failed in {sceneName}");

            SetPrivateField(controller, "_crouchHeld", false);
            SetPrivateField(controller, "_rollPressed", true);
            SetPrivateField(controller, "_isRolling", false);
            SetPrivateField(controller, "_moveInput", Vector3.forward);
            InvokePrivateMethod(controller, "UpdateRoll");
            Assert.IsTrue(controller.IsRolling, $"Roll not triggered in {sceneName}");
            Assert.AreEqual(EPlayerState.Roll, controller.State, $"Roll state transition failed in {sceneName}");
        }

        private static IEnumerator ValidatePerceptionStateChain(EnemyStateMachine stateMachine, Vector3 playerPosition, string sceneName)
        {
            stateMachine.TransitionTo(EEnemyState.Unaware);

            stateMachine.OnPlayerDetected(playerPosition);
            Assert.AreEqual(EEnemyState.Suspicious, stateMachine.CurrentState, $"Suspicious transition failed in {sceneName}");

            stateMachine.OnPlayerLost();
            Assert.AreEqual(EEnemyState.Unaware, stateMachine.CurrentState, $"Lost-to-unaware transition failed in {sceneName}");

            stateMachine.OnPlayerDetected(playerPosition);
            stateMachine.OnPlayerDetected(playerPosition);
            Assert.AreEqual(EEnemyState.Searching, stateMachine.CurrentState, $"Searching transition failed in {sceneName}");

            stateMachine.OnPlayerDetected(playerPosition);
            Assert.AreEqual(EEnemyState.Alert, stateMachine.CurrentState, $"Alert transition failed in {sceneName}");

            stateMachine.OnPlayerLost();
            Assert.AreEqual(EEnemyState.Alert, stateMachine.CurrentState, $"Lost should keep alert until delay expires in {sceneName}");
            yield return new WaitForSeconds(AlertDropToSearchDelayForTests);
            Assert.AreEqual(EEnemyState.Searching, stateMachine.CurrentState, $"Lost-to-searching transition failed in {sceneName}");
        }

        private static void InvokePrivateMethod(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Failed to reflect method {methodName}");
            method.Invoke(target, null);
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Failed to reflect field {fieldName}");
            field.SetValue(target, value);
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
