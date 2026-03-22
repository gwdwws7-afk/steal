using NUnit.Framework;
using INTIFALL.Player;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class CombatTriggerTests
    {
        private CombatTrigger _trigger;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("CombatTrigger");
            _trigger = _go.AddComponent<CombatTrigger>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void TrackEnemy_WithNull_DoesNotCrash()
        {
            _trigger.TrackEnemy(null);
        }

        [Test]
        public void UntrackEnemy_WithNull_DoesNotCrash()
        {
            _trigger.UntrackEnemy(null);
        }

        [Test]
        public void OnPlayerFiredWeapon_DoesNotCrash()
        {
            _trigger.OnPlayerFiredWeapon();
        }

        [Test]
        public void OnSaqueosAttacked_DoesNotCrash()
        {
            _trigger.OnSaqueosAttacked();
        }
    }
}