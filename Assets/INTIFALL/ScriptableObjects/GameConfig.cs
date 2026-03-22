using UnityEngine;

namespace INTIFALL.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "INTIFALL/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Player Settings")]
        public int playerMaxHP = 5;
        public int playerMaxFirstAid = 5;
        public float playerInvincibilityDuration = 0.5f;

        [Header("Stamina Settings")]
        public float maxStamina = 100f;
        public float sprintStaminaDrain = 15f;
        public float staminaRecovery = 10f;

        [Header("Movement Speeds")]
        public float walkSpeed = 4.5f;
        public float sprintSpeed = 7.0f;
        public float crouchSpeed = 1.5f;
        public float rollSpeed = 6.0f;
        public float ropeSpeed = 3.0f;

        [Header("Combat Settings")]
        public float alertToCombatDelay = 5f;
        public float combatExitDistance = 30f;
        public float meleeRange = 2f;
        public float meleeCastTime = 0.5f;
        public float backstabRange = 2f;
        public float backstabCastTime = 1.0f;
        public float sleepDartRange = 15f;

        [Header("Enemy Settings")]
        public float enemyVisionDistance = 15f;
        public float enemyVisionAngle = 60f;
        public float enemyWalkSoundRadius = 5f;
        public float enemyRunSoundRadius = 12f;
        public float enemyCommRange = 30f;

        [Header("Tool Settings")]
        public int maxToolSlots = 4;
        public float smokeBombRadius = 4f;
        public float flashBangDuration = 2.5f;
        public float empDisableDuration = 12f;
        public float sleepDartSleepDuration = 20f;

        [Header("Economy Settings")]
        public int startingCredits = 0;
        public int sRankBaseCredit = 500;
        public int aRankBaseCredit = 350;
        public int bRankBaseCredit = 200;
        public int zeroKillBonus = 150;
        public int noDamageBonus = 200;
        public int intelBonus = 50;

        [Header("Quality Settings")]
        public bool enablePostProcessing = true;
        public bool enableDynamicLighting = true;
        public bool enableFootstepSounds = true;
        public int targetFrameRate = 60;

        [Header("Difficulty Multipliers")]
        public float enemyDetectionSpeed = 1f;
        public float enemyAttackDamage = 1f;
        public float missionTimeLimit = 1f;

        public static GameConfig DefaultConfig()
        {
            GameConfig config = CreateInstance<GameConfig>();
            return config;
        }
    }
}