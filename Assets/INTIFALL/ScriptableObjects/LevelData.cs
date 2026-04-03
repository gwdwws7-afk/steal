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

        [Header("Flow Design")]
        public int plannedMainRoutes = 2;
        public int plannedStealthRoutes = 1;
        public float designedCompletionMinutes = 12f;
        public float completionWindowMinMinutes = 10f;
        public float completionWindowMaxMinutes = 14f;
        [Range(1, 5)] public int patrolPressureTier = 1;
        public float enemyDensityTargetPerMinute = 0.5f;
        [TextArea] public string flowNotes;

        [Header("Phase Timing (Minutes)")]
        public float infiltrationMinutes = 4f;
        public float objectiveMinutes = 5f;
        public float extractionMinutes = 3f;

        [Header("Encounter Allocation")]
        public int infiltrationEnemyBudget = 2;
        public int objectiveEnemyBudget = 3;
        public int extractionEnemyBudget = 1;
        [TextArea] public string encounterNotes;

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

        [Header("Terminal Interaction")]
        public float terminalHackDurationSeconds = 3f;
        public float terminalAlertSuppressDurationSeconds = 8f;
        public float terminalAlertSuppressRadiusMeters = 18f;
        public bool terminalSuppressActiveAlerts = true;
        public bool terminalUnlockLinkedDoors = true;
        public bool terminalClearLinkedLightingAlertMode = true;

        [Header("Rewards")]
        public int baseCreditReward = 200;
        public int secondaryObjectiveBonus = 50;
        public int zeroKillBonus = 150;
        public int noDamageBonus = 200;
    }
}
