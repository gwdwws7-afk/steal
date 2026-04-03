using INTIFALL.System;
using INTIFALL.Tools;
using NUnit.Framework;
using System.Reflection;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class ToolRiskWindowScoringTests
    {
        private GameObject _go;
        private GameManager _gm;

        [SetUp]
        public void SetUp()
        {
            EventBus.ClearAllSubscribers();
            _go = new GameObject("ToolRiskWindowScoring_GameManager");
            _gm = _go.AddComponent<GameManager>();
            InvokePrivateLifecycle(_gm, "OnEnable");
        }

        [TearDown]
        public void TearDown()
        {
            if (_gm != null)
                InvokePrivateLifecycle(_gm, "OnDisable");

            if (_go != null)
                Object.DestroyImmediate(_go);

            EventBus.ClearAllSubscribers();
        }

        [Test]
        public void HighRisk_BalancedRopeSmokeSoundWindow_OutperformsNoToolPlan()
        {
            MissionResult baseline = EvaluateOptionalRoute(
                routeRiskTier: 3,
                routeMultiplier: 1.25f);

            MissionResult balanced = EvaluateOptionalRoute(
                routeRiskTier: 3,
                routeMultiplier: 1.25f,
                publishTools: () =>
                {
                    PublishToolUse("Rope", EToolCategory.Environmental, 6f);
                    PublishToolUse("SmokeBomb", EToolCategory.PerceptionDisrupt, 16f);
                    PublishToolUse("SoundBait", EToolCategory.AttentionShift, 8f);
                });

            Assert.Greater(balanced.CreditsEarned, baseline.CreditsEarned);
            Assert.GreaterOrEqual(balanced.CreditsEarned - baseline.CreditsEarned, 35,
                "High-risk balanced tool profile should provide a clear reward uplift.");
            Assert.Greater(balanced.ToolRiskWindowAdjustment, baseline.ToolRiskWindowAdjustment);
            Assert.AreEqual(1, balanced.RopeToolUses);
            Assert.AreEqual(1, balanced.SmokeToolUses);
            Assert.AreEqual(1, balanced.SoundBaitToolUses);
        }

        [Test]
        public void HighRisk_LowCooldownSpam_IsPenalizedVersusBalancedWindow()
        {
            MissionResult balanced = EvaluateOptionalRoute(
                routeRiskTier: 3,
                routeMultiplier: 1.25f,
                publishTools: () =>
                {
                    PublishToolUse("Rope", EToolCategory.Environmental, 6f);
                    PublishToolUse("SmokeBomb", EToolCategory.PerceptionDisrupt, 16f);
                    PublishToolUse("SoundBait", EToolCategory.AttentionShift, 8f);
                });

            MissionResult spammed = EvaluateOptionalRoute(
                routeRiskTier: 3,
                routeMultiplier: 1.25f,
                publishTools: () =>
                {
                    for (int i = 0; i < 10; i++)
                        PublishToolUse("SoundBait", EToolCategory.AttentionShift, 1f);
                });

            Assert.Less(spammed.CreditsEarned, balanced.CreditsEarned,
                "Tool spam should not outperform balanced high-risk tool usage.");
            Assert.Less(spammed.ToolRiskWindowAdjustment, balanced.ToolRiskWindowAdjustment);
        }

        [Test]
        public void MediumRisk_SmokeAndBaitWindow_OutperformsRopeOnlyPlan()
        {
            MissionResult ropeOnly = EvaluateOptionalRoute(
                routeRiskTier: 2,
                routeMultiplier: 1.15f,
                publishTools: () =>
                {
                    PublishToolUse("Rope", EToolCategory.Environmental, 6f);
                });

            MissionResult smokeAndBait = EvaluateOptionalRoute(
                routeRiskTier: 2,
                routeMultiplier: 1.15f,
                publishTools: () =>
                {
                    PublishToolUse("SmokeBomb", EToolCategory.PerceptionDisrupt, 16f);
                    PublishToolUse("SoundBait", EToolCategory.AttentionShift, 8f);
                });

            Assert.Greater(smokeAndBait.CreditsEarned, ropeOnly.CreditsEarned,
                "Medium-risk route should reward smoke+bait control window over rope-only plan.");
            Assert.GreaterOrEqual(smokeAndBait.CreditsEarned - ropeOnly.CreditsEarned, 20);
            Assert.Greater(smokeAndBait.ToolRiskWindowAdjustment, ropeOnly.ToolRiskWindowAdjustment);
        }

        [Test]
        public void HighRisk_SingleToolDominance_IsPenalizedVersusMixedToolkit()
        {
            MissionResult dominated = EvaluateOptionalRoute(
                routeRiskTier: 3,
                routeMultiplier: 1.25f,
                publishTools: () =>
                {
                    for (int i = 0; i < 6; i++)
                        PublishToolUse("Rope", EToolCategory.Environmental, 4f);
                });

            MissionResult mixed = EvaluateOptionalRoute(
                routeRiskTier: 3,
                routeMultiplier: 1.25f,
                publishTools: () =>
                {
                    PublishToolUse("Rope", EToolCategory.Environmental, 5f);
                    PublishToolUse("SmokeBomb", EToolCategory.PerceptionDisrupt, 14f);
                    PublishToolUse("SoundBait", EToolCategory.AttentionShift, 8f);
                });

            Assert.Less(dominated.ToolRiskWindowAdjustment, mixed.ToolRiskWindowAdjustment);
            Assert.Less(dominated.CreditsEarned, mixed.CreditsEarned);
        }

        private MissionResult EvaluateOptionalRoute(
            int routeRiskTier,
            float routeMultiplier,
            global::System.Action publishTools = null)
        {
            _gm.LoadLevel(4, "Level05_General_Taki_Villa");
            _gm.StartGame();

            publishTools?.Invoke();

            return _gm.CalculateMissionResult(
                secondaryObjectivesCompleted: 1,
                intelCollected: 2,
                intelRequired: 3,
                timeBudgetSeconds: 900f,
                extractionRouteId: "villa_rooftop",
                extractionRouteLabel: "Villa Rooftop",
                isMainRoute: false,
                routeRiskTier: routeRiskTier,
                routeCreditMultiplier: routeMultiplier,
                routeSecondaryObjectiveBonus: 1);
        }

        private static void PublishToolUse(string toolName, EToolCategory category, float cooldownSeconds)
        {
            EventBus.Publish(new ToolUsedEvent
            {
                toolName = toolName,
                category = category,
                cooldownSeconds = cooldownSeconds
            });
        }

        private static void InvokePrivateLifecycle(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method != null)
                method.Invoke(target, null);
        }
    }
}
