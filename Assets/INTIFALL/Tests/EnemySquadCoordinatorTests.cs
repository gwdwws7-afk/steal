using INTIFALL.AI;
using NUnit.Framework;
using System.Reflection;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class EnemySquadCoordinatorTests
    {
        private GameObject _sourceGo;
        private GameObject _listenerGo;
        private GameObject _farGo;
        private EnemyController _source;
        private EnemyController _listener;
        private EnemyController _far;

        [SetUp]
        public void Setup()
        {
            EnemySquadCoordinator.ResetForTests();

            _source = CreateEnemy("source", new Vector3(0f, 0f, 0f), out _sourceGo);
            _listener = CreateEnemy("listener", new Vector3(4f, 0f, 0f), out _listenerGo);
            _far = CreateEnemy("far", new Vector3(50f, 0f, 0f), out _farGo);

            EnemySquadCoordinator.Register(_source);
            EnemySquadCoordinator.Register(_listener);
            EnemySquadCoordinator.Register(_far);
        }

        [TearDown]
        public void Teardown()
        {
            EnemySquadCoordinator.Unregister(_source);
            EnemySquadCoordinator.Unregister(_listener);
            EnemySquadCoordinator.Unregister(_far);

            if (_sourceGo != null) Object.DestroyImmediate(_sourceGo);
            if (_listenerGo != null) Object.DestroyImmediate(_listenerGo);
            if (_farGo != null) Object.DestroyImmediate(_farGo);

            EnemySquadCoordinator.ResetForTests();
        }

        [Test]
        public void BroadcastAlert_OnlyAffectsEnemiesInRange()
        {
            int waveId = EnemySquadCoordinator.NextWaveId();
            EnemySquadCoordinator.BroadcastAlert(_source, new Vector3(2f, 0f, 2f), true, 20f, waveId);

            Assert.AreEqual(EEnemyState.Alert, _listener.StateMachine.CurrentState, "In-range listener should react.");
            Assert.AreEqual(EEnemyState.Unaware, _far.StateMachine.CurrentState, "Out-of-range listener should stay unaware.");
        }

        [Test]
        public void ComputeSearchPoint_IsDeterministicPerInput()
        {
            Vector3 anchor = new Vector3(10f, 0f, -5f);
            Vector3 p1 = EnemySquadCoordinator.ComputeSearchPoint(anchor, 123, 6, 5f, 2);
            Vector3 p2 = EnemySquadCoordinator.ComputeSearchPoint(anchor, 123, 6, 5f, 2);
            Vector3 p3 = EnemySquadCoordinator.ComputeSearchPoint(anchor, 123, 6, 5f, 3);

            Assert.AreEqual(p1, p2, "Same input must produce deterministic search point.");
            Assert.AreNotEqual(p1, p3, "Different step should produce a different search point.");
        }

        [Test]
        public void UnregisterDestroyedEnemies_PurgesInvalidEntries()
        {
            Assert.GreaterOrEqual(EnemySquadCoordinator.ActiveEnemyCount, 3);

            Object.DestroyImmediate(_farGo);
            _farGo = null;
            _far = null;

            EnemySquadCoordinator.Unregister(null);
            Assert.AreEqual(2, EnemySquadCoordinator.ActiveEnemyCount);
        }

        private static EnemyController CreateEnemy(string name, Vector3 position, out GameObject go)
        {
            go = new GameObject(name);
            go.transform.position = position;

            CharacterController characterController = go.AddComponent<CharacterController>();
            EnemyStateMachine stateMachine = go.AddComponent<EnemyStateMachine>();
            go.AddComponent<PerceptionModule>();
            EnemyController enemyController = go.AddComponent<EnemyController>();

            BindInternalReference(enemyController, "_stateMachine", stateMachine);
            BindInternalReference(enemyController, "_cc", characterController);
            return enemyController;
        }

        private static void BindInternalReference(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Failed to reflect field {fieldName}.");
            field.SetValue(target, value);
        }
    }
}
