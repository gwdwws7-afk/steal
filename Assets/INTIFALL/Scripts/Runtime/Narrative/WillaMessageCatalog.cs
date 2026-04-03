using System;
using System.Collections.Generic;
using UnityEngine;

namespace INTIFALL.Narrative
{
    public static class WillaMessageCatalog
    {
        [Serializable]
        private class CatalogDto
        {
            public EntryDto[] entries;
        }

        [Serializable]
        private class EntryDto
        {
            public int levelIndex = -1;
            public string trigger = string.Empty;
            public string[] messages = Array.Empty<string>();
        }

        public readonly struct MessageKey : IEquatable<MessageKey>
        {
            public readonly int LevelIndex;
            public readonly EWillaTrigger Trigger;

            public MessageKey(int levelIndex, EWillaTrigger trigger)
            {
                LevelIndex = levelIndex;
                Trigger = trigger;
            }

            public bool Equals(MessageKey other)
            {
                return LevelIndex == other.LevelIndex && Trigger == other.Trigger;
            }

            public override bool Equals(object obj)
            {
                return obj is MessageKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (LevelIndex * 397) ^ (int)Trigger;
                }
            }
        }

        private static readonly Dictionary<MessageKey, string[]> DefaultCatalog = new()
        {
            [new MessageKey(0, EWillaTrigger.MissionStart)] = new[]
            {
                "Killa, stay unseen and move through the refinery district.",
                "Find the tablet fragment and leave before reinforcements arrive."
            },
            [new MessageKey(0, EWillaTrigger.IntelFound)] = new[]
            {
                "That fragment is active. Keep it away from imperial handlers.",
                "Good find. Extract and we can decode the first strand."
            },
            [new MessageKey(0, EWillaTrigger.MissionComplete)] = new[]
            {
                "Clean extraction. Rank {rank} ({rank_score}) with {credits} credits secured.",
                "Mission complete: intel {intel_collected}/{intel_required}, secondary {secondary_completed}/{secondary_total}, route {route_label} ({route_type})."
            },

            [new MessageKey(1, EWillaTrigger.MissionStart)] = new[]
            {
                "Archive nodes ahead. Recover your bloodline records.",
                "This facility holds what they erased from your history."
            },
            [new MessageKey(1, EWillaTrigger.IntelFound)] = new[]
            {
                "Record received. Their narrative is starting to break.",
                "Confirmed. Your lineage was targeted, not exiled by chance."
            },
            [new MessageKey(1, EWillaTrigger.MissionComplete)] = new[]
            {
                "Archive run complete: rank {rank}, stealth {stealth_status}, credits {credits}, route {route_label}."
            },

            [new MessageKey(2, EWillaTrigger.MissionStart)] = new[]
            {
                "Labs are active. Expect patrols and rapid response teams.",
                "Secure experiment logs. We need proof, not rumors."
            },
            [new MessageKey(2, EWillaTrigger.IntelFound)] = new[]
            {
                "Data confirms Saqueos field trials. Keep collecting evidence.",
                "This archive links command signatures to live test orders."
            },
            [new MessageKey(2, EWillaTrigger.MissionComplete)] = new[]
            {
                "Lab evidence secured. Intel missing: {intel_missing}. Combat style: {combat_style}. Alerts: {alerts_triggered}, tools: {tools_used}."
            },

            [new MessageKey(3, EWillaTrigger.MissionStart)] = new[]
            {
                "Qhipu core is ahead. Security will escalate from here.",
                "Stay focused. This node determines the final outcome path."
            },
            [new MessageKey(3, EWillaTrigger.IntelFound)] = new[]
            {
                "Core thread recovered. The prophecy has multiple endpoints.",
                "Good pull. Their contingency routes are now exposed."
            },
            [new MessageKey(3, EWillaTrigger.MissionComplete)] = new[]
            {
                "Core sector clear. Rank {rank}, damage status: {damage_status}, route risk {route_risk}. Prepare for terminal confrontation."
            },

            [new MessageKey(4, EWillaTrigger.MissionStart)] = new[]
            {
                "Final sector. Every choice from here is irreversible.",
                "This is the last run, Killa. Pick your ending carefully."
            },
            [new MessageKey(4, EWillaTrigger.IntelFound)] = new[]
            {
                "Final proof secured. Taki's fallback protocol is now on record.",
                "Intel chain complete. We can close this on your signal."
            },
            [new MessageKey(4, EWillaTrigger.StoryReveal)] = new[]
            {
                "You now have the full truth. The system bends to your decision."
            },
            [new MessageKey(4, EWillaTrigger.Warning)] = new[]
            {
                "Hostiles converging on your position. Move now."
            },
            [new MessageKey(4, EWillaTrigger.MissionComplete)] = new[]
            {
                "Final objective complete. Rank {rank}, credits {credits}, stealth {stealth_status}, route {route_label}."
            },

            [new MessageKey(-1, EWillaTrigger.MissionStart)] = new[]
            {
                "Stay focused, Killa. We only get one clean run."
            },
            [new MessageKey(-1, EWillaTrigger.IntelFound)] = new[]
            {
                "Intel secured. Keep moving."
            },
            [new MessageKey(-1, EWillaTrigger.MissionComplete)] = new[]
            {
                "Objective complete: rank {rank} ({rank_score}), intel {intel_collected}/{intel_required}, credits {credits}, route {route_label}."
            },
            [new MessageKey(-1, EWillaTrigger.StoryReveal)] = new[]
            {
                "Truth received. Expect heavy resistance."
            },
            [new MessageKey(-1, EWillaTrigger.Warning)] = new[]
            {
                "Warning: threat spike detected near your route."
            },
            [new MessageKey(-1, EWillaTrigger.Betrayal)] = new[]
            {
                "Signal integrity dropped. Someone inside turned."
            }
        };

        public static Dictionary<MessageKey, string[]> BuildEffectiveCatalog(string json, out int importedEntries, out string warning)
        {
            Dictionary<MessageKey, string[]> effective = CloneCatalog(DefaultCatalog);
            importedEntries = 0;
            warning = string.Empty;

            if (string.IsNullOrWhiteSpace(json))
                return effective;

            if (!TryParseCatalog(json, out CatalogDto parsed, out string parseError))
            {
                warning = parseError;
                return effective;
            }

            if (parsed.entries == null || parsed.entries.Length == 0)
                return effective;

            int invalidEntryCount = 0;
            for (int i = 0; i < parsed.entries.Length; i++)
            {
                EntryDto entry = parsed.entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.trigger))
                {
                    invalidEntryCount++;
                    continue;
                }

                if (!Enum.TryParse(entry.trigger.Trim(), true, out EWillaTrigger trigger))
                {
                    invalidEntryCount++;
                    continue;
                }

                string[] normalizedMessages = NormalizeMessages(entry.messages);
                if (normalizedMessages.Length == 0)
                {
                    invalidEntryCount++;
                    continue;
                }

                effective[new MessageKey(entry.levelIndex, trigger)] = normalizedMessages;
                importedEntries++;
            }

            if (invalidEntryCount > 0)
                warning = $"Ignored {invalidEntryCount} invalid message entries.";

            return effective;
        }

        private static bool TryParseCatalog(string json, out CatalogDto dto, out string error)
        {
            dto = null;
            error = string.Empty;

            try
            {
                dto = JsonUtility.FromJson<CatalogDto>(json);
                if (dto == null)
                {
                    error = "Catalog JSON parsed to null document.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = $"Catalog JSON parse failed: {ex.Message}";
                return false;
            }
        }

        private static string[] NormalizeMessages(string[] source)
        {
            if (source == null || source.Length == 0)
                return Array.Empty<string>();

            List<string> cleaned = new(source.Length);
            for (int i = 0; i < source.Length; i++)
            {
                string message = source[i];
                if (string.IsNullOrWhiteSpace(message))
                    continue;

                string normalized = message.Trim();
                if (normalized.Contains('\uFFFD'))
                    continue;

                cleaned.Add(normalized);
            }

            return cleaned.ToArray();
        }

        private static Dictionary<MessageKey, string[]> CloneCatalog(Dictionary<MessageKey, string[]> source)
        {
            Dictionary<MessageKey, string[]> clone = new(source.Count);
            foreach (KeyValuePair<MessageKey, string[]> kv in source)
            {
                string[] messages = kv.Value ?? Array.Empty<string>();
                string[] copy = new string[messages.Length];
                Array.Copy(messages, copy, messages.Length);
                clone[kv.Key] = copy;
            }
            return clone;
        }
    }
}
