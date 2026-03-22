using NUnit.Framework;
using INTIFALL.Growth;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class BloodlineSystemTests
    {
        private BloodlineSystem _bloodline;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("BloodlineSystem");
            _bloodline = _go.AddComponent<BloodlineSystem>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void CurrentLevel_InitiallyZero()
        {
            Assert.AreEqual(0, _bloodline.CurrentLevel);
        }

        [Test]
        public void UnlockPassiveForLevel_Level1_UnlocksAndesBreath()
        {
            _bloodline.UnlockPassiveForLevel(1);
            Assert.IsTrue(_bloodline.HasPassive(EBloodlinePassive.AndesBreath));
            Assert.AreEqual(1, _bloodline.CurrentLevel);
        }

        [Test]
        public void UnlockPassiveForLevel_Level2_UnlocksPriestEye()
        {
            _bloodline.UnlockPassiveForLevel(2);
            Assert.IsTrue(_bloodline.HasPassive(EBloodlinePassive.PriestEye));
            Assert.AreEqual(2, _bloodline.CurrentLevel);
        }

        [Test]
        public void UnlockPassiveForLevel_Level3_UnlocksGoldenBlood()
        {
            _bloodline.UnlockPassiveForLevel(3);
            Assert.IsTrue(_bloodline.HasPassive(EBloodlinePassive.GoldenBlood));
        }

        [Test]
        public void UnlockPassiveForLevel_Level4_UnlocksLeechSense()
        {
            _bloodline.UnlockPassiveForLevel(4);
            Assert.IsTrue(_bloodline.HasPassive(EBloodlinePassive.LeechSense));
        }

        [Test]
        public void UnlockPassiveForLevel_Level5_UnlocksReincarnationEnd()
        {
            _bloodline.UnlockPassiveForLevel(5);
            Assert.IsTrue(_bloodline.HasPassive(EBloodlinePassive.ReincarnationEnd));
        }

        [Test]
        public void UnlockPassiveForLevel_Duplicate_DoesNothing()
        {
            _bloodline.UnlockPassiveForLevel(1);
            _bloodline.UnlockPassiveForLevel(1);
            Assert.AreEqual(1, _bloodline.CurrentLevel);
        }

        [Test]
        public void GetCrouchNoiseMultiplier_WithoutPassive_Returns1()
        {
            Assert.AreEqual(1f, _bloodline.GetCrouchNoiseMultiplier());
        }

        [Test]
        public void GetCrouchNoiseMultiplier_WithPassive_ReturnsReduced()
        {
            _bloodline.UnlockPassiveForLevel(1);
            Assert.Less(_bloodline.GetCrouchNoiseMultiplier(), 1f);
        }

        [Test]
        public void GetTerminalHackSpeedMultiplier_WithoutPassive_Returns1()
        {
            Assert.AreEqual(1f, _bloodline.GetTerminalHackSpeedMultiplier());
        }

        [Test]
        public void GetEMPDisabledDurationBonus_WithoutPassive_Returns0()
        {
            Assert.AreEqual(0f, _bloodline.GetEMPDisabledDurationBonus());
        }

        [Test]
        public void GetSaqueosWarningDistance_WithoutPassive_Returns0()
        {
            Assert.AreEqual(0f, _bloodline.GetSaqueosWarningDistance());
        }

        [Test]
        public void ResetBloodline_ResetsLevel()
        {
            _bloodline.UnlockPassiveForLevel(3);
            _bloodline.ResetBloodline();
            Assert.AreEqual(0, _bloodline.CurrentLevel);
            Assert.AreEqual(0, _bloodline.UnlockedPassives.Length);
        }

        [Test]
        public void GetPassiveName_ReturnsCorrectName()
        {
            Assert.AreEqual("安第斯之息", _bloodline.GetPassiveName(EBloodlinePassive.AndesBreath));
            Assert.AreEqual("祭司之眼", _bloodline.GetPassiveName(EBloodlinePassive.PriestEye));
            Assert.AreEqual("黄金之血", _bloodline.GetPassiveName(EBloodlinePassive.GoldenBlood));
            Assert.AreEqual("蚂蟥感知", _bloodline.GetPassiveName(EBloodlinePassive.LeechSense));
            Assert.AreEqual("轮回终章", _bloodline.GetPassiveName(EBloodlinePassive.ReincarnationEnd));
        }
    }
}