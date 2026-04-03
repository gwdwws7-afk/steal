using System.Collections.Generic;
using UnityEngine;

namespace INTIFALL.System
{
    public static class LocalizationService
    {
        private const string LocalizationTableResourcePath = "INTIFALL/Localization/LocalizationTable";

        private static readonly Dictionary<string, LocalizedEntry> Table = new();
        private static bool _isLoaded;
        private static bool _hasLoadAttempt;
        private static SystemLanguage? _languageOverride;

        public static SystemLanguage CurrentLanguage => _languageOverride ?? Application.systemLanguage;

        public static int LoadedKeyCount
        {
            get
            {
                EnsureLoaded();
                return Table.Count;
            }
        }

        public static void SetLanguageOverride(SystemLanguage language)
        {
            _languageOverride = language;
        }

        public static void ClearLanguageOverride()
        {
            _languageOverride = null;
        }

        public static string Get(
            string key,
            string fallbackEnglish = "",
            string fallbackChinese = "",
            SystemLanguage? languageOverride = null)
        {
            EnsureLoaded();
            SystemLanguage language = languageOverride ?? CurrentLanguage;

            if (!string.IsNullOrWhiteSpace(key) && Table.TryGetValue(key, out LocalizedEntry entry))
            {
                string localized = SelectByLanguage(entry, language);
                if (!string.IsNullOrWhiteSpace(localized))
                    return localized;
            }

            if (language == SystemLanguage.ChineseSimplified || language == SystemLanguage.ChineseTraditional)
            {
                if (!string.IsNullOrWhiteSpace(fallbackChinese))
                    return fallbackChinese;
            }

            if (!string.IsNullOrWhiteSpace(fallbackEnglish))
                return fallbackEnglish;

            return key;
        }

        public static bool HasKey(string key)
        {
            EnsureLoaded();
            if (string.IsNullOrWhiteSpace(key))
                return false;

            return Table.ContainsKey(key);
        }

        public static void Reload()
        {
            ResetCacheOnly();
            EnsureLoaded();
        }

        public static void ResetForTests()
        {
            ResetCacheOnly();
            _languageOverride = null;
        }

        private static void ResetCacheOnly()
        {
            Table.Clear();
            _isLoaded = false;
            _hasLoadAttempt = false;
        }

        private static void EnsureLoaded()
        {
            if (_isLoaded || _hasLoadAttempt)
                return;

            _hasLoadAttempt = true;

            TextAsset tableAsset = Resources.Load<TextAsset>(LocalizationTableResourcePath);
            if (tableAsset == null || string.IsNullOrWhiteSpace(tableAsset.text))
            {
                _isLoaded = true;
                return;
            }

            LocalizationDocument document;
            try
            {
                document = JsonUtility.FromJson<LocalizationDocument>(tableAsset.text);
            }
            catch
            {
                _isLoaded = true;
                return;
            }

            if (document?.entries == null)
            {
                _isLoaded = true;
                return;
            }

            for (int i = 0; i < document.entries.Length; i++)
            {
                LocalizationEntry raw = document.entries[i];
                if (raw == null || string.IsNullOrWhiteSpace(raw.key))
                    continue;

                Table[raw.key] = new LocalizedEntry
                {
                    en = raw.en,
                    zhHans = raw.zhHans,
                    zhHant = raw.zhHant
                };
            }

            _isLoaded = true;
        }

        private static string SelectByLanguage(LocalizedEntry entry, SystemLanguage language)
        {
            if (language == SystemLanguage.ChineseSimplified)
                return !string.IsNullOrWhiteSpace(entry.zhHans) ? entry.zhHans : entry.en;

            if (language == SystemLanguage.ChineseTraditional)
            {
                if (!string.IsNullOrWhiteSpace(entry.zhHant))
                    return entry.zhHant;

                return !string.IsNullOrWhiteSpace(entry.zhHans) ? entry.zhHans : entry.en;
            }

            return entry.en;
        }

        [global::System.Serializable]
        private class LocalizationDocument
        {
            public LocalizationEntry[] entries;
        }

        [global::System.Serializable]
        private class LocalizationEntry
        {
            public string key;
            public string en;
            public string zhHans;
            public string zhHant;
        }

        private struct LocalizedEntry
        {
            public string en;
            public string zhHans;
            public string zhHant;
        }
    }
}
