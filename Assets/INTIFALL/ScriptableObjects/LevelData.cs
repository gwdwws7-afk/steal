using UnityEngine;

namespace INTIFALL.Data
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "INTIFALL/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        public string levelName;
        public string levelDisplayName;
        public int levelIndex;
        public string sceneName;

        [Header("Timing")]
        public float standardTime = 300f;
        public float timeLimit = 600f;

        [Header("Enemy Configuration")]
        public int totalEnemyCount;
        public int normalEnemyCount;
        public int reinforcedEnemyCount;
        public int heavyEnemyCount;
        public int quipucamayocCount;
        public int saqueosCount;

        [Header("Intel")]
        public int qhipuFragmentCount = 3;
        public int terminalCount = 3;
        public int supplyPointCount = 2;

        [Header("Difficulty")]
        public float baseAlertDuration = 5f;
        public float searchDuration = 8f;
        public float fullAlertDuration = 30f;

        [Header("Special Elements")]
        public bool hasVentSystem = true;
        public bool hasHangingPoints = true;
        public bool hasBreakableWalls = false;
        public bool hasElectronicDoors = false;
        public bool hasSurveillanceCameras = false;

        [Header("Rewards")]
        public int baseCreditReward = 200;
        public int secondaryObjectiveBonus = 50;
        public int zeroKillBonus = 150;
        public int noDamageBonus = 200;
    }
}