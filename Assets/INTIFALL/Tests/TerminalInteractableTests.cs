using System.Reflection;
using INTIFALL.Environment;
using INTIFALL.Narrative;
using INTIFALL.System;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class TerminalInteractableTests
    {
        private GameObject _narrativeGo;
        private NarrativeManager _narrative;
        private GameObject _terminalGo;
        private TerminalInteractable _terminal;

        [SetUp]
        public void SetUp()
        {
            EventBus.ClearAllSubscribers();

            _narrativeGo = new GameObject("NarrativeManager");
            _narrative = _narrativeGo.AddComponent<NarrativeManager>();
            InvokePrivate(_narrative, "Awake");

            _terminalGo = new GameObject("TerminalInteractable");
            _terminalGo.AddComponent<BoxCollider>();
            _terminal = _terminalGo.AddComponent<TerminalInteractable>();
            _terminal.Configure("terminal_alpha", 2, "Terminal Alpha", "Fallback summary for QA terminal", new[] { "story_reveal" });
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.ClearAllSubscribers();

            if (_terminalGo != null)
                Object.DestroyImmediate(_terminalGo);
            if (_narrativeGo != null)
                Object.DestroyImmediate(_narrativeGo);
        }

        [Test]
        public void CompleteHackImmediate_PublishesEvents_AndRecordsNarrativeProgress()
        {
            int intelEventCount = 0;
            int hackedEventCount = 0;
            int suppressedEventCount = 0;
            int terminalDocumentReadCount = 0;
            int scriptedTriggerCount = 0;
            TerminalAlertSuppressedEvent suppressedEvent = default;
            TerminalDocumentReadEvent terminalReadEvent = default;

            global::System.Action<IntelCollectedInSceneEvent> onIntel = evt =>
            {
                if (evt.intelId == "terminal_alpha" && evt.levelIndex == 2)
                    intelEventCount++;
            };
            global::System.Action<TerminalHackCompletedEvent> onHacked = evt =>
            {
                if (evt.terminalId == "terminal_alpha" && evt.levelIndex == 2)
                    hackedEventCount++;
            };
            global::System.Action<TerminalAlertSuppressedEvent> onSuppressed = evt =>
            {
                if (evt.sourceTerminalId == "terminal_alpha" && evt.levelIndex == 2)
                {
                    suppressedEventCount++;
                    suppressedEvent = evt;
                }
            };
            global::System.Action<TerminalDocumentReadEvent> onTerminalRead = evt =>
            {
                if (evt.terminalId == "terminal_alpha" && evt.levelIndex == 2)
                {
                    terminalDocumentReadCount++;
                    terminalReadEvent = evt;
                }
            };
            global::System.Action<NarrativeTriggeredEvent> onNarrativeTriggered = evt =>
            {
                if (evt.levelIndex == 2 && evt.eventType == ENarrativeEventType.ScriptedTrigger)
                    scriptedTriggerCount++;
            };

            EventBus.Subscribe(onIntel);
            EventBus.Subscribe(onHacked);
            EventBus.Subscribe(onSuppressed);
            EventBus.Subscribe(onTerminalRead);
            EventBus.Subscribe(onNarrativeTriggered);
            try
            {
                _terminal.CompleteHackImmediate();

                Assert.IsTrue(_terminal.IsHacked);
                Assert.AreEqual(1, intelEventCount);
                Assert.AreEqual(1, hackedEventCount);
                Assert.AreEqual(1, suppressedEventCount);
                Assert.AreEqual(1, terminalDocumentReadCount);
                Assert.Greater(suppressedEvent.waveId, 0);
                Assert.Greater(suppressedEvent.effectRadiusMeters, 0f);
                Assert.AreEqual(_terminal.transform.position, suppressedEvent.sourcePosition);
                Assert.AreEqual("Terminal Alpha", terminalReadEvent.title);
                Assert.AreEqual("Fallback summary for QA terminal", terminalReadEvent.summary);
                Assert.AreEqual("story_reveal", terminalReadEvent.advancedTrigger);
                Assert.AreEqual(1, scriptedTriggerCount);
                Assert.IsTrue(_narrative.IsTerminalRead("terminal_alpha", 2));
                Assert.AreEqual(1, _narrative.GetIntelCollectedForLevel(2));

                _terminal.CompleteHackImmediate();
                Assert.AreEqual(1, intelEventCount, "Terminal hack should be idempotent.");
                Assert.AreEqual(1, hackedEventCount, "Terminal hack should not publish duplicate completion events.");
            }
            finally
            {
                EventBus.Unsubscribe(onIntel);
                EventBus.Unsubscribe(onHacked);
                EventBus.Unsubscribe(onSuppressed);
                EventBus.Unsubscribe(onTerminalRead);
                EventBus.Unsubscribe(onNarrativeTriggered);
            }
        }

        [Test]
        public void ApplyLevelTuning_OverridesRuntimeParameters()
        {
            _terminal.ApplyLevelTuning(
                hackDurationSeconds: 2.4f,
                suppressDurationSeconds: 9.2f,
                suppressRadiusMeters: 27f,
                suppressAlerts: true,
                unlockDoors: false,
                clearLightingAlertMode: false);

            _terminal.CompleteHackImmediate();

            Assert.IsTrue(_terminal.IsHacked);
        }

        private static void InvokePrivate(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Missing private method: {methodName}");
            method.Invoke(target, null);
        }
    }
}
