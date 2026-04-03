using INTIFALL.Level;
using INTIFALL.System;
using INTIFALL.UI;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class MissionDebriefUITests
    {
        private GameObject _go;
        private MissionDebriefUI _debrief;

        [SetUp]
        public void Setup()
        {
            LocalizationService.SetLanguageOverride(SystemLanguage.English);
            _go = new GameObject("MissionDebriefUI");
            _debrief = _go.AddComponent<MissionDebriefUI>();
        }

        [TearDown]
        public void TearDown()
        {
            LocalizationService.ClearLanguageOverride();
            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        [Test]
        public void ShowDebrief_StoresOutcomeAndSummary()
        {
            int shownCount = 0;
            MissionDebriefShownEvent lastShown = default;

            global::System.Action<MissionDebriefShownEvent> onShown = evt =>
            {
                shownCount++;
                lastShown = evt;
            };

            EventBus.Subscribe(onShown);
            try
            {
                var outcome = BuildOutcome("A", 4, 420);
                _debrief.ShowDebrief(outcome);

                Assert.IsTrue(_debrief.IsVisible);
                Assert.AreEqual("A", _debrief.LastOutcome.rank);
                StringAssert.Contains("Rank: A", _debrief.LastSummary);
                StringAssert.Contains("Credits: 420", _debrief.LastSummary);
                Assert.AreEqual(1, shownCount);
                Assert.AreEqual("A", lastShown.rank);
                Assert.AreEqual(420, lastShown.creditsEarned);
            }
            finally
            {
                EventBus.Unsubscribe(onShown);
            }
        }

        [Test]
        public void HideDebrief_ClearsVisibleState()
        {
            _debrief.ShowDebrief(BuildOutcome("B", 3, 260));
            Assert.IsTrue(_debrief.IsVisible);

            _debrief.HideDebrief();

            Assert.IsFalse(_debrief.IsVisible);
        }

        [Test]
        public void MissionOutcomeEvent_AutoShowsDebrief()
        {
            var method = typeof(MissionDebriefUI).GetMethod(
                "OnMissionOutcomeEvaluated",
                global::System.Reflection.BindingFlags.Instance | global::System.Reflection.BindingFlags.NonPublic);

            Assert.IsNotNull(method, "Missing MissionDebriefUI.OnMissionOutcomeEvaluated handler.");
            method.Invoke(_debrief, new object[] { BuildOutcome("S", 5, 700) });

            Assert.IsTrue(_debrief.IsVisible);
            StringAssert.Contains("Rank: S", _debrief.LastSummary);
        }

        [Test]
        public void ShowDebrief_IncludesRouteAndPressureMetrics()
        {
            var outcome = BuildOutcome("A", 4, 480);
            outcome.extractionRouteLabel = "Coolant Tunnel";
            outcome.usedOptionalExit = true;
            outcome.routeRiskTier = 3;
            outcome.routeCreditMultiplier = 1.25f;
            outcome.alertsTriggered = 2;
            outcome.toolsUsed = 5;
            outcome.secondaryObjectivesTotal = 3;
            outcome.secondaryObjectivesEvaluated = 3;
            outcome.toolRiskWindowAdjustment = 60;
            outcome.toolCooldownLoad = 30f;
            outcome.ropeToolUses = 1;
            outcome.smokeToolUses = 2;
            outcome.soundBaitToolUses = 1;

            _debrief.ShowDebrief(outcome);

            StringAssert.Contains("Coolant Tunnel", _debrief.LastSummary);
            StringAssert.Contains("risk 3", _debrief.LastSummary);
            StringAssert.Contains("alerts 2", _debrief.LastSummary);
            StringAssert.Contains("tools used 5", _debrief.LastSummary);
            StringAssert.Contains("Secondary Objectives: 2/3", _debrief.LastSummary);
            StringAssert.Contains("Secondary (evaluated): 3/3", _debrief.LastSummary);
            StringAssert.Contains("Tool Window: +60 (Balanced window)", _debrief.LastSummary);
            StringAssert.Contains("Tool Mix: rope 1, smoke 2, bait 1, cooldown load 30.0s", _debrief.LastSummary);
        }

        private static MissionOutcomeEvaluatedEvent BuildOutcome(string rank, int rankScore, int credits)
        {
            return new MissionOutcomeEvaluatedEvent
            {
                levelIndex = 2,
                rank = rank,
                rankScore = rankScore,
                creditsEarned = credits,
                intelCollected = 3,
                intelRequired = 3,
                secondaryObjectivesCompleted = 2,
                secondaryObjectivesEvaluated = 2,
                secondaryObjectivesTotal = 2,
                zeroKill = rank == "S",
                noDamage = true,
                wasDiscovered = false,
                fullAlertTriggered = false,
                extractionRouteId = "main",
                extractionRouteLabel = "Main Extraction",
                usedOptionalExit = false,
                routeRiskTier = 0,
                routeCreditMultiplier = 1f,
                alertsTriggered = 0,
                toolsUsed = 0,
                toolRiskWindowAdjustment = 0,
                toolCooldownLoad = 0f,
                ropeToolUses = 0,
                smokeToolUses = 0,
                soundBaitToolUses = 0
            };
        }
    }
}
