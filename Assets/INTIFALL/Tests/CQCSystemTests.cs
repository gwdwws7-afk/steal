using NUnit.Framework;
using INTIFALL.Player;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class CQCSystemTests
    {
        private CQCSystem _cqc;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("CQCSystem");
            _cqc = _go.AddComponent<CQCSystem>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void IsExecutingAction_InitiallyFalse()
        {
            Assert.IsFalse(_cqc.IsExecutingAction);
        }

        [Test]
        public void CurrentAction_InitiallyNone()
        {
            Assert.AreEqual(ECQCAction.None, _cqc.CurrentAction);
        }

        [Test]
        public void ActionProgress_InitiallyZero()
        {
            Assert.AreEqual(0f, _cqc.ActionProgress);
        }

        [Test]
        public void TryMeleeAttack_WithNoEnemy_ReturnsFalse()
        {
            Assert.IsFalse(_cqc.TryMeleeAttack());
        }

        [Test]
        public void TryBackstab_WithNoEnemy_ReturnsFalse()
        {
            Assert.IsFalse(_cqc.TryBackstab());
        }

        [Test]
        public void TrySleepDart_WithNoEnemy_ReturnsFalse()
        {
            Assert.IsFalse(_cqc.TrySleepDart());
        }
    }
}
