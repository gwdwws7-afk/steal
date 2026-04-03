using INTIFALL.Data;
using INTIFALL.System;
using UnityEngine;

namespace INTIFALL.Narrative
{
    public struct IntelCollectedInSceneEvent
    {
        public string intelId;
        public int levelIndex;
        public EIntelType intelType;
    }

    [RequireComponent(typeof(Collider))]
    public class IntelPickup : MonoBehaviour
    {
        [SerializeField] private string intelId = "intel_00";
        [SerializeField] private int levelIndex;
        [SerializeField] private EIntelType intelType = EIntelType.QhipuFragment;
        [SerializeField] private string displayName = "Intel";
        [SerializeField] private string description = string.Empty;
        [SerializeField] private string[] scriptedNarrativeTriggers = global::System.Array.Empty<string>();
        [SerializeField] private bool destroyOnCollect = true;

        private bool _collected;

        private void Awake()
        {
            EnsureTriggerCollider();
        }

        private void Reset()
        {
            EnsureTriggerCollider();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_collected) return;
            if (!other.CompareTag("Player")) return;

            Collect();
        }

        public void Configure(
            string id,
            int level,
            EIntelType type,
            string name,
            string descriptionText = "",
            string[] triggerEvents = null)
        {
            intelId = string.IsNullOrEmpty(id) ? "intel_00" : id;
            levelIndex = Mathf.Max(0, level);
            intelType = type;
            displayName = string.IsNullOrEmpty(name) ? intelId : name;
            description = string.IsNullOrWhiteSpace(descriptionText) ? string.Empty : descriptionText.Trim();
            scriptedNarrativeTriggers = triggerEvents != null
                ? (string[])triggerEvents.Clone()
                : global::System.Array.Empty<string>();
        }

        public void Collect()
        {
            if (_collected) return;
            _collected = true;

            NarrativeManager narrative = NarrativeManager.Instance;
            if (narrative == null)
                narrative = Object.FindFirstObjectByType<NarrativeManager>();

            narrative?.CollectIntel(intelId, levelIndex);

            EventBus.Publish(new IntelCollectedInSceneEvent
            {
                intelId = intelId,
                levelIndex = levelIndex,
                intelType = intelType
            });

            PublishScriptedNarrativeTriggers();

            if (destroyOnCollect)
                Destroy(gameObject);
        }

        private void PublishScriptedNarrativeTriggers()
        {
            if (scriptedNarrativeTriggers == null || scriptedNarrativeTriggers.Length == 0)
                return;

            for (int i = 0; i < scriptedNarrativeTriggers.Length; i++)
            {
                string token = scriptedNarrativeTriggers[i];
                if (string.IsNullOrWhiteSpace(token))
                    continue;

                EventBus.Publish(new NarrativeTriggeredEvent
                {
                    eventType = ENarrativeEventType.ScriptedTrigger,
                    eventId = token.Trim(),
                    levelIndex = levelIndex
                });
            }
        }

        private void EnsureTriggerCollider()
        {
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
                sphere.radius = 0.8f;
                sphere.isTrigger = true;
                return;
            }

            col.isTrigger = true;
        }
    }
}
