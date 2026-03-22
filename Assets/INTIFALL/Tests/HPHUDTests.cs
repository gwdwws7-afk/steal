using NUnit.Framework;
using INTIFALL.UI;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class HPHUDTests
    {
        private HPHUD _hpHUD;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("HPHUD");
            _hpHUD = _go.AddComponent<HPHUD>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void ShowDamageEffect_DoesNotCrash()
        {
            _hpHUD.ShowDamageEffect();
        }

        [Test]
        public void ShowHealEffect_DoesNotCrash()
        {
            _hpHUD.ShowHealEffect();
        }

        [Test]
        public void UpdateHPRecoveryState_DoesNotCrash()
        {
            _hpHUD.UpdateHPRecoveryState();
        }
    }
}