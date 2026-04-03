using System;
using System.Collections.Generic;
using UnityEngine;

namespace INTIFALL.Narrative
{
    [Serializable]
    public class TerminalDocumentCatalogEntry
    {
        public int levelIndex = -1;
        public string terminalId = string.Empty;
        public string title = string.Empty;
        public string summary = string.Empty;
        public string advancedTrigger = string.Empty;
    }

    [Serializable]
    public class TerminalDocumentCatalogDto
    {
        public TerminalDocumentCatalogEntry[] entries = Array.Empty<TerminalDocumentCatalogEntry>();
    }

    public readonly struct TerminalDocumentRecord
    {
        public readonly int LevelIndex;
        public readonly string TerminalId;
        public readonly string Title;
        public readonly string Summary;
        public readonly string AdvancedTrigger;

        public TerminalDocumentRecord(int levelIndex, string terminalId, string title, string summary, string advancedTrigger)
        {
            LevelIndex = levelIndex;
            TerminalId = terminalId;
            Title = title;
            Summary = summary;
            AdvancedTrigger = advancedTrigger;
        }
    }

    public static class TerminalDocumentCatalog
    {
        private const string ResourcePath = "INTIFALL/Narrative/TerminalDocuments";

        private static readonly Dictionary<string, TerminalDocumentRecord> RecordsByExactKey = new();
        private static readonly Dictionary<string, TerminalDocumentRecord> RecordsByTerminalId = new();
        private static bool _loaded;

        public static bool TryGet(string terminalId, int levelIndex, out TerminalDocumentRecord record)
        {
            EnsureLoaded();
            record = default;

            if (string.IsNullOrWhiteSpace(terminalId))
                return false;

            string normalizedId = NormalizeTerminalId(terminalId);
            string exactKey = BuildExactKey(levelIndex, normalizedId);
            if (RecordsByExactKey.TryGetValue(exactKey, out record))
                return true;

            return RecordsByTerminalId.TryGetValue(normalizedId, out record);
        }

        public static void ResetForTests()
        {
            _loaded = false;
            RecordsByExactKey.Clear();
            RecordsByTerminalId.Clear();
        }

        private static void EnsureLoaded()
        {
            if (_loaded)
                return;

            _loaded = true;
            RecordsByExactKey.Clear();
            RecordsByTerminalId.Clear();

            TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
            if (asset == null || string.IsNullOrWhiteSpace(asset.text))
                return;

            TerminalDocumentCatalogDto dto;
            try
            {
                dto = JsonUtility.FromJson<TerminalDocumentCatalogDto>(asset.text);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"TerminalDocumentCatalog: failed to parse catalog json. {ex.Message}");
                return;
            }

            if (dto?.entries == null || dto.entries.Length == 0)
                return;

            for (int i = 0; i < dto.entries.Length; i++)
            {
                TerminalDocumentCatalogEntry entry = dto.entries[i];
                if (entry == null)
                    continue;
                if (string.IsNullOrWhiteSpace(entry.terminalId))
                    continue;
                if (string.IsNullOrWhiteSpace(entry.title))
                    continue;
                if (string.IsNullOrWhiteSpace(entry.summary))
                    continue;

                string normalizedId = NormalizeTerminalId(entry.terminalId);
                var record = new TerminalDocumentRecord(
                    levelIndex: Mathf.Max(-1, entry.levelIndex),
                    terminalId: normalizedId,
                    title: entry.title.Trim(),
                    summary: entry.summary.Trim(),
                    advancedTrigger: string.IsNullOrWhiteSpace(entry.advancedTrigger) ? string.Empty : entry.advancedTrigger.Trim());

                RecordsByTerminalId[normalizedId] = record;
                RecordsByExactKey[BuildExactKey(record.LevelIndex, normalizedId)] = record;
            }
        }

        private static string BuildExactKey(int levelIndex, string normalizedTerminalId)
        {
            return $"{Mathf.Max(-1, levelIndex)}:{normalizedTerminalId}";
        }

        private static string NormalizeTerminalId(string terminalId)
        {
            return terminalId.Trim().ToLowerInvariant();
        }
    }
}
