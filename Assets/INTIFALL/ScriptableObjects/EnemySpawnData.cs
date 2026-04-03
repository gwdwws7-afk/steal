using UnityEngine;
using INTIFALL.AI;

namespace INTIFALL.Data
{
    public enum EEnemySpawnType
    {
        Normal,
        Reinforced,
        Heavy,
        Quipucamayoc,
        Saqueos
    }

    [global::System.Serializable]
    public class EnemySpawnPoint
    {
        public string spawnId;
        public Vector3 position;
        public Quaternion rotation;
        public EEnemySpawnType enemyType;
        public bool isPatrol;
        public string patrolRouteId;
        public int awarenessLevel = 0;
    }

    [CreateAssetMenu(fileName = "EnemySpawnData", menuName = "INTIFALL/Enemy Spawn Data")]
    public class EnemySpawnData : ScriptableObject
    {
        [Header("Level Reference")]
        public int levelIndex;
        public string levelName;

        [Header("Spawn Points")]
        public EnemySpawnPoint[] spawnPoints;

        [Header("Patrol Routes")]
        public string[] availablePatrolRoutes;

        [Header("Formation")]
        public int maxConcurrentAlert = 3;
        public int communicationGroupSize = 4;

        public EnemySpawnPoint GetSpawnPoint(string id)
        {
            if (spawnPoints == null) return null;

            foreach (var point in spawnPoints)
            {
                if (point.spawnId == id)
                    return point;
            }
            return null;
        }

        public EnemySpawnPoint[] GetSpawnPointsByType(EEnemySpawnType type)
        {
            if (spawnPoints == null) return new EnemySpawnPoint[0];

            global::System.Collections.Generic.List<EnemySpawnPoint> result = new();
            foreach (var point in spawnPoints)
            {
                if (point.enemyType == type)
                    result.Add(point);
            }
            return result.ToArray();
        }

        public int GetTotalSpawnCount()
        {
            return spawnPoints != null ? spawnPoints.Length : 0;
        }

        public EEnemySpawnType GetSpawnType(int index)
        {
            if (spawnPoints == null || index < 0 || index >= spawnPoints.Length)
                return EEnemySpawnType.Normal;
            return spawnPoints[index].enemyType;
        }
    }
}
