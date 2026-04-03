using INTIFALL.AI;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class PerceptionModuleTests
    {
        private GameObject _enemyGo;
        private GameObject _targetGo;
        private PerceptionModule _perception;

        [SetUp]
        public void SetUp()
        {
            _enemyGo = new GameObject("Perception_Enemy");
            _perception = _enemyGo.AddComponent<PerceptionModule>();

            _targetGo = new GameObject("Perception_Target");
            _targetGo.transform.position = new Vector3(0f, 0f, 10f);

            _enemyGo.transform.position = Vector3.zero;
            _enemyGo.transform.rotation = Quaternion.identity;

            _perception.SetTarget(_targetGo.transform);
        }

        [TearDown]
        public void TearDown()
        {
            if (_targetGo != null)
                Object.DestroyImmediate(_targetGo);
            if (_enemyGo != null)
                Object.DestroyImmediate(_enemyGo);
        }

        [Test]
        public void ConfigurePerceptionProfile_ClampsValuesToSafeRanges()
        {
            _perception.ConfigurePerceptionProfile(
                visionDistanceMeters: -5f,
                visionAngleDegrees: 999f,
                crouchVisionMultiplierValue: -1f,
                shadowLuxThresholdValue: -10f,
                shadowDetectionPenaltyValue: 2f,
                walkSoundRadiusMeters: -3f,
                runSoundRadiusMeters: -9f,
                crouchSoundRadiusMeters: -7f,
                communicationRangeMeters: -2f);

            Assert.AreEqual(0.1f, _perception.GetVisionDistance(), 0.001f);
            Assert.AreEqual(179f, _perception.GetVisionAngle(), 0.001f);
            Assert.AreEqual(0.1f, _perception.CrouchVisionMultiplier, 0.001f);
            Assert.AreEqual(0f, _perception.ShadowLuxThreshold, 0.001f);
            Assert.AreEqual(1f, _perception.ShadowDetectionPenalty, 0.001f);
            Assert.AreEqual(0.1f, _perception.WalkSoundRadius, 0.001f);
            Assert.AreEqual(0.1f, _perception.RunSoundRadius, 0.001f);
            Assert.AreEqual(0.05f, _perception.CrouchSoundRadius, 0.001f);
            Assert.AreEqual(0f, _perception.CommunicationRange, 0.001f);
        }

        [Test]
        public void CanSeeTarget_InShadow_IsDeterministicAndParameterDriven()
        {
            _perception.ConfigurePerceptionProfile(
                visionDistanceMeters: 15f,
                visionAngleDegrees: 60f,
                crouchVisionMultiplierValue: 1f,
                shadowLuxThresholdValue: 100f,
                shadowDetectionPenaltyValue: 0.5f,
                walkSoundRadiusMeters: 5f,
                runSoundRadiusMeters: 12f,
                crouchSoundRadiusMeters: 2f,
                communicationRangeMeters: 30f);

            bool firstResult = _perception.CanSeeTarget();
            Assert.IsFalse(firstResult, "Target should be out of effective shadow-adjusted vision distance.");

            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(firstResult, _perception.CanSeeTarget(), "Detection result must be deterministic across repeated checks.");
            }

            _perception.ConfigurePerceptionProfile(
                visionDistanceMeters: 15f,
                visionAngleDegrees: 60f,
                crouchVisionMultiplierValue: 1f,
                shadowLuxThresholdValue: 100f,
                shadowDetectionPenaltyValue: 0.1f,
                walkSoundRadiusMeters: 5f,
                runSoundRadiusMeters: 12f,
                crouchSoundRadiusMeters: 2f,
                communicationRangeMeters: 30f);

            bool tunedResult = _perception.CanSeeTarget();
            Assert.IsTrue(tunedResult, "Lower shadow penalty should deterministically allow detection at the same position.");

            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(tunedResult, _perception.CanSeeTarget(), "Tuned detection result must remain deterministic.");
            }
        }
    }
}
