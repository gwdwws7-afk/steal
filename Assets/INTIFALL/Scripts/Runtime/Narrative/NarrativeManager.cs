using System.Collections.Generic;
using UnityEngine;
using INTIFALL.System;

namespace INTIFALL.Narrative
{
    public enum ENarrativeEventType
    {
        QhipuVision,
        TerminalDocument,
        BloodlineResonance,
        EndingChoice,
        ScriptedTrigger
    }

    public struct NarrativeTriggeredEvent
    {
        public ENarrativeEventType eventType;
        public string eventId;
        public int levelIndex;
    }

    public struct TerminalDocumentReadEvent
    {
        public string terminalId;
        public int levelIndex;
        public string title;
        public string summary;
        public string advancedTrigger;
    }

    public class NarrativeManager : MonoBehaviour
    {
        [Header("Intel Collection")]
        [SerializeField] private int _intelCollected = 0;
        [SerializeField] private int _totalIntelPerLevel = 3;

        [Header("Qhipu Visions")]
        [SerializeField] private bool[] _qhipuCollected = new bool[15];

        [Header("Terminal Documents")]
        [SerializeField] private bool[] _terminalsRead = new bool[25];

        [Header("Bloodline Resonance")]
        [SerializeField] private bool _hasExperiencedResonance = false;

        private readonly HashSet<string> _collectedIntelKeys = new();
        private readonly HashSet<string> _readTerminalKeys = new();

        private static NarrativeManager _instance;
        public static NarrativeManager Instance => _instance;

        public int IntelCollected => _intelCollected;
        public int TotalIntelPerLevel => _totalIntelPerLevel;
        public bool HasExperiencedResonance => _hasExperiencedResonance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        public void CollectIntel(string intelId, int levelIndex)
        {
            string key = BuildProgressKey(intelId, levelIndex);
            if (string.IsNullOrEmpty(key))
                return;

            if (_collectedIntelKeys.Contains(key))
                return;

            _collectedIntelKeys.Add(key);
            _intelCollected++;

            EventBus.Publish(new NarrativeTriggeredEvent
            {
                eventType = ENarrativeEventType.QhipuVision,
                eventId = intelId,
                levelIndex = levelIndex
            });
        }

        public void ReadTerminal(string terminalId, int levelIndex)
        {
            ReadTerminal(terminalId, levelIndex, terminalId, string.Empty, null);
        }

        public void ReadTerminal(
            string terminalId,
            int levelIndex,
            string fallbackTitle,
            string fallbackSummary,
            string[] fallbackScriptedTriggers)
        {
            string key = BuildProgressKey(terminalId, levelIndex);
            if (string.IsNullOrEmpty(key))
                return;

            if (_readTerminalKeys.Contains(key))
                return;

            _readTerminalKeys.Add(key);

            bool hasCatalogEntry = TerminalDocumentCatalog.TryGet(terminalId, levelIndex, out TerminalDocumentRecord documentRecord);
            string resolvedTitle = hasCatalogEntry
                ? documentRecord.Title
                : (string.IsNullOrWhiteSpace(fallbackTitle) ? terminalId : fallbackTitle.Trim());
            string resolvedSummary = hasCatalogEntry
                ? documentRecord.Summary
                : (string.IsNullOrWhiteSpace(fallbackSummary) ? string.Empty : fallbackSummary.Trim());
            string advancedTrigger = hasCatalogEntry
                ? documentRecord.AdvancedTrigger
                : JoinTriggers(fallbackScriptedTriggers);

            EventBus.Publish(new TerminalDocumentReadEvent
            {
                terminalId = terminalId,
                levelIndex = levelIndex,
                title = resolvedTitle,
                summary = resolvedSummary,
                advancedTrigger = advancedTrigger
            });

            EventBus.Publish(new NarrativeTriggeredEvent
            {
                eventType = ENarrativeEventType.TerminalDocument,
                eventId = terminalId,
                levelIndex = levelIndex
            });

            PublishScriptedTriggers(advancedTrigger, levelIndex);
        }

        public void TriggerBloodlineResonance()
        {
            if (_hasExperiencedResonance) return;

            _hasExperiencedResonance = true;

            EventBus.Publish(new NarrativeTriggeredEvent
            {
                eventType = ENarrativeEventType.BloodlineResonance,
                eventId = "BloodlineResonance",
                levelIndex = 0
            });
        }

        public bool IsQhipuCollected(string qhipuId, int levelIndex)
        {
            return _collectedIntelKeys.Contains(BuildProgressKey(qhipuId, levelIndex));
        }

        public bool IsTerminalRead(string terminalId, int levelIndex)
        {
            return _readTerminalKeys.Contains(BuildProgressKey(terminalId, levelIndex));
        }

        public int GetIntelCollectedForLevel(int levelIndex)
        {
            int count = 0;
            string prefix = BuildLevelPrefix(levelIndex);
            foreach (string key in _collectedIntelKeys)
            {
                if (key.StartsWith(prefix, global::System.StringComparison.Ordinal))
                    count++;
            }

            return count;
        }

        public int GetTerminalsReadForLevel(int levelIndex)
        {
            int count = 0;
            string prefix = BuildLevelPrefix(levelIndex);
            foreach (string key in _readTerminalKeys)
            {
                if (key.StartsWith(prefix, global::System.StringComparison.Ordinal))
                    count++;
            }

            return count;
        }

        public void ResetNarrativeProgress()
        {
            _intelCollected = 0;
            _hasExperiencedResonance = false;
            _collectedIntelKeys.Clear();
            _readTerminalKeys.Clear();

            for (int i = 0; i < _qhipuCollected.Length; i++)
                _qhipuCollected[i] = false;

            for (int i = 0; i < _terminalsRead.Length; i++)
                _terminalsRead[i] = false;
        }

        public void ResetLevelNarrative(int levelIndex)
        {
            string prefix = BuildLevelPrefix(levelIndex);
            RemoveProgressByPrefix(_collectedIntelKeys, prefix);
            RemoveProgressByPrefix(_readTerminalKeys, prefix);

            _intelCollected = _collectedIntelKeys.Count;
        }

        private static string BuildProgressKey(string id, int levelIndex)
        {
            if (string.IsNullOrWhiteSpace(id))
                return string.Empty;

            int safeLevel = Mathf.Max(0, levelIndex);
            return $"{safeLevel}:{id.Trim()}";
        }

        private static string BuildLevelPrefix(int levelIndex)
        {
            int safeLevel = Mathf.Max(0, levelIndex);
            return $"{safeLevel}:";
        }

        private static void RemoveProgressByPrefix(HashSet<string> set, string prefix)
        {
            if (set == null || set.Count == 0 || string.IsNullOrEmpty(prefix))
                return;

            string[] snapshot = new string[set.Count];
            set.CopyTo(snapshot);
            for (int i = 0; i < snapshot.Length; i++)
            {
                string key = snapshot[i];
                if (key.StartsWith(prefix, global::System.StringComparison.Ordinal))
                    set.Remove(key);
            }
        }

        private static string JoinTriggers(string[] source)
        {
            if (source == null || source.Length == 0)
                return string.Empty;

            global::System.Collections.Generic.List<string> cleaned = new(source.Length);
            for (int i = 0; i < source.Length; i++)
            {
                string token = source[i];
                if (string.IsNullOrWhiteSpace(token))
                    continue;
                cleaned.Add(token.Trim());
            }

            if (cleaned.Count == 0)
                return string.Empty;

            return string.Join(",", cleaned);
        }

        private static void PublishScriptedTriggers(string triggerTokens, int levelIndex)
        {
            if (string.IsNullOrWhiteSpace(triggerTokens))
                return;

            string[] tokens = triggerTokens.Split(
                new[] { ',', ';', '|' },
                global::System.StringSplitOptions.RemoveEmptyEntries);
            if (tokens == null || tokens.Length == 0)
                return;

            int safeLevelIndex = Mathf.Max(0, levelIndex);
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                if (string.IsNullOrWhiteSpace(token))
                    continue;

                EventBus.Publish(new NarrativeTriggeredEvent
                {
                    eventType = ENarrativeEventType.ScriptedTrigger,
                    eventId = token.Trim(),
                    levelIndex = safeLevelIndex
                });
            }
        }
    }
}
