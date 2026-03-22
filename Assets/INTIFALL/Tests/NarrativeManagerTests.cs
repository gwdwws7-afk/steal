using NUnit.Framework;
using INTIFALL.Narrative;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class NarrativeManagerTests
    {
        private NarrativeManager _narrative;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("NarrativeManager");
            _narrative = _go.AddComponent<NarrativeManager>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void IntelCollected_InitiallyZero()
        {
            Assert.AreEqual(0, _narrative.IntelCollected);
        }

        [Test]
        public void TotalIntelPerLevel_Is3()
        {
            Assert.AreEqual(3, _narrative.TotalIntelPerLevel);
        }

        [Test]
        public void HasExperiencedResonance_InitiallyFalse()
        {
            Assert.IsFalse(_narrative.HasExperiencedResonance);
        }

        [Test]
        public void CollectIntel_NewIntel_IncrementsCount()
        {
            _narrative.CollectIntel("Qhipu_01", 0);
            Assert.AreEqual(1, _narrative.IntelCollected);
        }

        [Test]
        public void CollectIntel_SameIntel_DoesNotIncrement()
        {
            _narrative.CollectIntel("Qhipu_01", 0);
            _narrative.CollectIntel("Qhipu_01", 0);
            Assert.AreEqual(1, _narrative.IntelCollected);
        }

        [Test]
        public void IsQhipuCollected_AfterCollect_ReturnsTrue()
        {
            _narrative.CollectIntel("Qhipu_01", 0);
            Assert.IsTrue(_narrative.IsQhipuCollected("Qhipu_01", 0));
        }

        [Test]
        public void TriggerBloodlineResonance_SetsFlag()
        {
            _narrative.TriggerBloodlineResonance();
            Assert.IsTrue(_narrative.HasExperiencedResonance);
        }

        [Test]
        public void ResetNarrativeProgress_ResetsAll()
        {
            _narrative.CollectIntel("Qhipu_01", 0);
            _narrative.TriggerBloodlineResonance();

            _narrative.ResetNarrativeProgress();

            Assert.AreEqual(0, _narrative.IntelCollected);
            Assert.IsFalse(_narrative.HasExperiencedResonance);
        }

        [Test]
        public void GetIntelCollectedForLevel_ReturnsCorrectCount()
        {
            _narrative.CollectIntel("Qhipu_01", 0);
            _narrative.CollectIntel("Qhipu_02", 0);

            Assert.AreEqual(2, _narrative.GetIntelCollectedForLevel(0));
        }
    }
}