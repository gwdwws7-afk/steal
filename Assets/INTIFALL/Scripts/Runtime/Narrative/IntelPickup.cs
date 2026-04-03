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

        public void Configure(string id, int level, EIntelType type, string name)
        {
            intelId = string.IsNullOrEmpty(id) ? "intel_00" : id;
            levelIndex = Mathf.Max(0, level);
            intelType = type;
            displayName = string.IsNullOrEmpty(name) ? intelId : name;
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

            if (destroyOnCollect)
                Destroy(gameObject);
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
