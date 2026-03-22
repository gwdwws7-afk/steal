using UnityEngine;
using INTIFALL.System;

namespace INTIFALL.Narrative
{
    public enum ENarrativeEventType
    {
        QhipuVision,
        TerminalDocument,
        BloodlineResonance,
        EndingChoice
    }

    public struct NarrativeTriggeredEvent
    {
        public ENarrativeEventType eventType;
        public string eventId;
        public int levelIndex;
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
            int intelIndex = GetIntelIndex(intelId, levelIndex);
            if (intelIndex < 0 || intelIndex >= _qhipuCollected.Length)
                return;

            if (_qhipuCollected[intelIndex])
                return;

            _qhipuCollected[intelIndex] = true;
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
            int terminalIndex = GetTerminalIndex(terminalId, levelIndex);
            if (terminalIndex < 0 || terminalIndex >= _terminalsRead.Length)
                return;

            if (_terminalsRead[terminalIndex])
                return;

            _terminalsRead[terminalIndex] = true;

            EventBus.Publish(new NarrativeTriggeredEvent
            {
                eventType = ENarrativeEventType.TerminalDocument,
                eventId = terminalId,
                levelIndex = levelIndex
            });
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
            int index = GetIntelIndex(qhipuId, levelIndex);
            if (index < 0 || index >= _qhipuCollected.Length)
                return false;
            return _qhipuCollected[index];
        }

        public bool IsTerminalRead(string terminalId, int levelIndex)
        {
            int index = GetTerminalIndex(terminalId, levelIndex);
            if (index < 0 || index >= _terminalsRead.Length)
                return false;
            return _terminalsRead[index];
        }

        public int GetIntelCollectedForLevel(int levelIndex)
        {
            int start = levelIndex * _totalIntelPerLevel;
            int end = Mathf.Min(start + _totalIntelPerLevel, _qhipuCollected.Length);

            int count = 0;
            for (int i = start; i < end; i++)
            {
                if (_qhipuCollected[i])
                    count++;
            }
            return count;
        }

        public int GetTerminalsReadForLevel(int levelIndex)
        {
            int start = levelIndex * 5;
            int end = Mathf.Min(start + 5, _terminalsRead.Length);

            int count = 0;
            for (int i = start; i < end; i++)
            {
                if (_terminalsRead[i])
                    count++;
            }
            return count;
        }

        private int GetIntelIndex(string intelId, int levelIndex)
        {
            return levelIndex * _totalIntelPerLevel + GetIntelIdHash(intelId);
        }

        private int GetTerminalIndex(string terminalId, int levelIndex)
        {
            return levelIndex * 5 + GetTerminalIdHash(terminalId);
        }

        private int GetIntelIdHash(string intelId)
        {
            return Mathf.Abs(intelId.GetHashCode()) % _totalIntelPerLevel;
        }

        private int GetTerminalIdHash(string terminalId)
        {
            return Mathf.Abs(terminalId.GetHashCode()) % 5;
        }

        public void ResetNarrativeProgress()
        {
            _intelCollected = 0;
            _hasExperiencedResonance = false;

            for (int i = 0; i < _qhipuCollected.Length; i++)
                _qhipuCollected[i] = false;

            for (int i = 0; i < _terminalsRead.Length; i++)
                _terminalsRead[i] = false;
        }

        public void ResetLevelNarrative(int levelIndex)
        {
            int start = levelIndex * _totalIntelPerLevel;
            int end = Mathf.Min(start + _totalIntelPerLevel, _qhipuCollected.Length);

            for (int i = start; i < end; i++)
                _qhipuCollected[i] = false;
        }
    }
}