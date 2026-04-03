using NUnit.Framework;
using INTIFALL.Player;
using UnityEngine;
using System.Reflection;

namespace INTIFALL.Tests
{
    public class PlayerCombatStateMachineTests
    {
        private static readonly MethodInfo UpdateMethod = typeof(PlayerCombatStateMachine).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
        private PlayerCombatStateMachine _combat;
        private GameObject _go;

        private static void InvokeUpdate(PlayerCombatStateMachine combat)
        {
            if (UpdateMethod == null)
                throw new global::System.MissingMethodException(nameof(PlayerCombatStateMachine), "Update");

            UpdateMethod.Invoke(combat, null);
        }

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("PlayerCombat");
            _combat = _go.AddComponent<PlayerCombatStateMachine>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void InitialMode_IsNormal()
        {
            Assert.AreEqual(ECombatMode.Normal, _combat.CurrentMode);
        }

        [Test]
        public void IsInCombat_InitiallyFalse()
        {
            Assert.IsFalse(_combat.IsInCombat);
        }

        [Test]
        public void TransitionTo_Combat_ChangesMode()
        {
            _combat.TransitionTo(ECombatMode.Combat);
            Assert.AreEqual(ECombatMode.Combat, _combat.CurrentMode);
        }

        [Test]
        public void TransitionTo_SameState_DoesNothing()
        {
            _combat.TransitionTo(ECombatMode.Normal);
            Assert.AreEqual(ECombatMode.Normal, _combat.CurrentMode);
        }

        [Test]
        public void OnImmediateCombat_EntersCombat()
        {
            _combat.OnImmediateCombat();
            Assert.IsTrue(_combat.IsInCombat);
        }

        [Test]
        public void OnPlayerFiredWeapon_EntersCombat()
        {
            _combat.OnPlayerFiredWeapon();
            Assert.IsTrue(_combat.IsInCombat);
        }

        [Test]
        public void OnEnemyAlerted_BelowDelay_DoesNotEnterCombat()
        {
            _combat.OnEnemyAlerted(3f);
            Assert.IsFalse(_combat.IsInCombat);
        }

        [Test]
        public void OnEnemyAlerted_AtDelay_EntersCombat()
        {
            _combat.OnEnemyAlerted(5f);
            Assert.IsTrue(_combat.IsInCombat);
        }

        [Test]
        public void ForceExitCombat_ReturnsToNormal()
        {
            _combat.TransitionTo(ECombatMode.Combat);
            Assert.IsTrue(_combat.IsInCombat);

            _combat.ForceExitCombat();
            Assert.AreEqual(ECombatMode.Normal, _combat.CurrentMode);
        }

        [Test]
        public void CombatTimer_IncreasesInCombat()
        {
            _combat.TransitionTo(ECombatMode.Combat);

            for (int i = 0; i < 10; i++)
                InvokeUpdate(_combat);

            Assert.Greater(_combat.CombatTimer, 0f);
        }
    }
}
