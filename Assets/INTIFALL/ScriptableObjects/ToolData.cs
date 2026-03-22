using UnityEngine;

namespace INTIFALL.Tools
{
    [CreateAssetMenu(fileName = "ToolData", menuName = "INTIFALL/Tool Data")]
    public class ToolData : ScriptableObject
    {
        [Header("Basic Info")]
        public string toolName;
        public string toolNameCN;
        public EToolCategory category;
        public EToolSlot defaultSlot;

        [Header("Combat Stats")]
        public int damage;
        public float range;
        public float cooldown;
        public float duration;

        [Header("Resource Cost")]
        public int maxAmmo;
        public float energyCost;

        [Header("Upgrade")]
        public int upgradePrice;
        public ToolData upgradedVersion;

        [Header("Unlock")]
        public bool unlockedByDefault;
        public int unlockPrice;
        public int unlockLevel;

        [Header("Display")]
        public Sprite icon;
        public string description;
    }
}
