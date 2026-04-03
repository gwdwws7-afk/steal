using NUnit.Framework;
using INTIFALL.Player;
using INTIFALL.System;
using UnityEngine;
using System.Reflection;

namespace INTIFALL.Tests
{
    public class PlayerHealthSystemTests
    {
        private static readonly MethodInfo UpdateMethod = typeof(PlayerHealthSystem).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
        private PlayerHealthSystem _health;
        private GameObject _go;

        private static void InvokeUpdate(PlayerHealthSystem health)
        {
            if (UpdateMethod == null)
                throw new global::System.MissingMethodException(nameof(PlayerHealthSystem), "Update");

            UpdateMethod.Invoke(health, null);
        }

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("PlayerHealth");
            _health = _go.AddComponent<PlayerHealthSystem>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void MaxHP_DefaultValue_Is5()
        {
            Assert.AreEqual(5, _health.MaxHP);
        }

        [Test]
        public void CurrentHP_InitiallyEqualsMaxHP()
        {
            Assert.AreEqual(5, _health.CurrentHP);
        }

        [Test]
        public void FirstAidCount_Initially_5()
        {
            Assert.AreEqual(5, _health.FirstAidCount);
        }

        [Test]
        public void IsDead_InitiallyFalse()
        {
            Assert.IsFalse(_health.IsDead);
        }

        [Test]
        public void IsUsingFirstAid_InitiallyFalse()
        {
            Assert.IsFalse(_health.IsUsingFirstAid);
        }

        [Test]
        public void CanUseFirstAid_AtFullHP_ReturnsFalse()
        {
            Assert.IsFalse(_health.CanUseFirstAid);
        }

        [Test]
        public void TakeDamage_ReducesHP()
        {
            _health.TakeDamage(1);
            Assert.AreEqual(4, _health.CurrentHP);
        }

        [Test]
        public void TakeDamage_ToZero_KillsPlayer()
        {
            _health.TakeDamage(5);
            Assert.AreEqual(0, _health.CurrentHP);
            Assert.IsTrue(_health.IsDead);
        }

        [Test]
        public void Heal_IncreasesHP()
        {
            _health.TakeDamage(2);
            _health.Heal(1);
            Assert.AreEqual(4, _health.CurrentHP);
        }

        [Test]
        public void Heal_DoesNotExceedMaxHP()
        {
            _health.Heal(10);
            Assert.AreEqual(5, _health.CurrentHP);
        }

        [Test]
        public void TakeDamage_WhileDead_DoesNothing()
        {
            _health.TakeDamage(5);
            Assert.IsTrue(_health.IsDead);

            _health.TakeDamage(1);
            Assert.AreEqual(0, _health.CurrentHP);
        }

        [Test]
        public void Heal_WhileDead_DoesNothing()
        {
            _health.TakeDamage(5);
            Assert.IsTrue(_health.IsDead);

            _health.Heal(5);
            Assert.AreEqual(0, _health.CurrentHP);
        }

        [Test]
        public void StartFirstAid_WithNoDamage_ReturnsFalse()
        {
            Assert.IsFalse(_health.CanUseFirstAid);
        }

        [Test]
        public void StartFirstAid_DecreasesFirstAidCount()
        {
            _health.TakeDamage(2);
            _health.StartFirstAid();

            for (int i = 0; i < 20; i++)
                InvokeUpdate(_health);

            Assert.AreEqual(4, _health.FirstAidCount);
        }

        [Test]
        public void ResetForLevel_RestoresHPAndFirstAid()
        {
            _health.TakeDamage(3);
            _health.StartFirstAid();
            for (int i = 0; i < 20; i++) InvokeUpdate(_health);

            _health.ResetForLevel();

            Assert.AreEqual(5, _health.CurrentHP);
            Assert.AreEqual(5, _health.FirstAidCount);
            Assert.IsFalse(_health.IsDead);
        }
    }
}
