using System.Collections;
using INTIFALL.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class Iteration6AISquadSearchPlayModeTests
    {
        private const float AlertDropToSearchDelayForTests = 1.25f;

        [UnityTest]
        public IEnumerator SquadAlertAndSearchLoop_RemainsStableWithoutOscillation()
        {
            EnemyController source = CreateEnemy("I6_Source", new Vector3(0f, 0f, 0f));
            EnemyController listener = CreateEnemy("I6_Listener", new Vector3(4f, 0f, 0f));

            EnemySquadCoordinator.Register(source);
            EnemySquadCoordinator.Register(listener);

            try
            {
                int waveId = EnemySquadCoordinator.NextWaveId();
                EnemySquadCoordinator.BroadcastAlert(source, new Vector3(3f, 0f, 3f), true, 20f, waveId);
                yield return null;

                Assert.AreEqual(EEnemyState.Alert, listener.StateMachine.CurrentState, "Listener should enter alert after high-priority squad alert.");

                listener.StateMachine.OnPlayerLost();
                Assert.AreEqual(EEnemyState.Alert, listener.StateMachine.CurrentState, "Listener should remain in alert until drop delay expires.");
                yield return new WaitForSeconds(AlertDropToSearchDelayForTests);
                Assert.AreEqual(EEnemyState.Searching, listener.StateMachine.CurrentState, "Listener should drop from alert to searching.");

                Vector3 startPos = listener.transform.position;
                for (int i = 0; i < 45; i++)
                    yield return null;

                Assert.AreEqual(EEnemyState.Searching, listener.StateMachine.CurrentState, "Listener should remain in searching state during short observation window.");
                Assert.Less(Vector3.Distance(startPos, listener.transform.position), 25f, "Listener should keep a bounded search movement envelope.");
            }
            finally
            {
                EnemySquadCoordinator.Unregister(source);
                EnemySquadCoordinator.Unregister(listener);

                if (source != null)
                    Object.Destroy(source.gameObject);
                if (listener != null)
                    Object.Destroy(listener.gameObject);
            }
        }

        private static EnemyController CreateEnemy(string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.AddComponent<CharacterController>();
            go.AddComponent<EnemyStateMachine>();
            go.AddComponent<PerceptionModule>();
            return go.AddComponent<EnemyController>();
        }
    }
}
