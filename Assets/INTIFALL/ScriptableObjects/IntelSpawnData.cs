using UnityEngine;

namespace INTIFALL.Data
{
    public enum EIntelType
    {
        QhipuFragment,
        TerminalDocument,
        AudioLog,
        VisualClue
    }

    [System.Serializable]
    public class IntelSpawnPoint
    {
        public string intelId;
        public Vector3 position;
        public EIntelType intelType;
        public string displayName;
        public string description;
        public bool isHidden;
        public string[] triggerEvents;
    }

    [System.Serializable]
    public class SupplyPointData
    {
        public string supplyId;
        public Vector3 position;
        public bool providesFirstAid = true;
        public bool providesTools = true;
        public float cooldownDuration = 30f;
    }

    [System.Serializable]
    public class ExitPointData
    {
        public string exitId;
        public Vector3 position;
        public bool requiresAllIntel = false;
        public bool isMainExit = true;
    }

    [CreateAssetMenu(fileName = "IntelSpawnData", menuName = "INTIFALL/Intel Spawn Data")]
    public class IntelSpawnData : ScriptableObject
    {
        [Header("Level Reference")]
        public int levelIndex;
        public string levelName;

        [Header("Intel Spawns")]
        public IntelSpawnPoint[] intelPoints;

        [Header("Supply Points")]
        public SupplyPointData[] supplyPoints;

        [Header("Exit Points")]
        public ExitPointData[] exitPoints;

        [Header("Vent System")]
        public Vector3[] ventEntrancePositions;
        public Vector3[] ventExitPositions;

        public IntelSpawnPoint GetIntel(string id)
        {
            if (intelPoints == null) return null;

            foreach (var intel in intelPoints)
            {
                if (intel.intelId == id)
                    return intel;
            }
            return null;
        }

        public IntelSpawnPoint[] GetIntelByType(EIntelType type)
        {
            if (intelPoints == null) return new IntelSpawnPoint[0];

            System.Collections.Generic.List<IntelSpawnPoint> result = new();
            foreach (var intel in intelPoints)
            {
                if (intel.intelType == type)
                    result.Add(intel);
            }
            return result.ToArray();
        }

        public int GetTotalIntelCount()
        {
            return intelPoints != null ? intelPoints.Length : 0;
        }

        public SupplyPointData GetSupplyPoint(string id)
        {
            if (supplyPoints == null) return null;

            foreach (var supply in supplyPoints)
            {
                if (supply.supplyId == id)
                    return supply;
            }
            return null;
        }

        public ExitPointData GetExitPoint(string id)
        {
            if (exitPoints == null) return null;

            foreach (var exit in exitPoints)
            {
                if (exit.exitId == id)
                    return exit;
            }
            return null;
        }

        public Vector3 GetRandomVentEntrance()
        {
            if (ventEntrancePositions == null || ventEntrancePositions.Length == 0)
                return Vector3.zero;

            return ventEntrancePositions[Random.Range(0, ventEntrancePositions.Length)];
        }
    }
}