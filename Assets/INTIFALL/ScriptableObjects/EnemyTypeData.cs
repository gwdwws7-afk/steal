using INTIFALL.System;
using UnityEngine;

namespace INTIFALL.AI
{
    [CreateAssetMenu(fileName = "EnemyTypeData", menuName = "INTIFALL/Enemy Type Data")]
    public class EnemyTypeData : ScriptableObject
    {
        [Header("Identity")]
        public EEnemyType enemyType;
        public string displayName;
        public string displayNameEnglish;
        public string displayNameChinese;
        public string localizationKey;

        [Header("Combat Stats")]
        public int hp = 1;
        public int damage = 1;
        public float attackRange = 3f;
        public float attackCooldown = 1.5f;

        [Header("Movement")]
        public float walkSpeed = 2.5f;
        public float runSpeed = 4.5f;

        [Header("Vision")]
        public float visionDistance = 15f;
        public float visionAngle = 60f;
        public float crouchVisionMultiplier = 0.5f;

        [Header("Hearing")]
        public float walkSoundRadius = 5f;
        public float runSoundRadius = 12f;
        public float crouchSoundRadius = 2f;

        [Header("Communication")]
        public float commRange = 30f;

        [Header("Behavior Flags")]
        public bool canPatrol = true;
        public bool canChase = true;
        public bool canCallReinforcements;
        public bool isInvisibleToStealth;

        public static EnemyTypeData GetDefaultData(EEnemyType type)
        {
            EnemyTypeData data = CreateInstance<EnemyTypeData>();
            data.enemyType = type;

            switch (type)
            {
                case EEnemyType.Normal:
                    data.displayNameEnglish = "Guard";
                    data.displayNameChinese = "普通士兵";
                    data.localizationKey = "enemy.guard";
                    data.hp = 1;
                    data.damage = 1;
                    data.walkSpeed = 2.5f;
                    data.runSpeed = 4.5f;
                    data.visionDistance = 15f;
                    data.visionAngle = 60f;
                    data.walkSoundRadius = 5f;
                    data.runSoundRadius = 12f;
                    data.commRange = 30f;
                    data.canPatrol = true;
                    data.canChase = true;
                    break;

                case EEnemyType.Reinforced:
                    data.displayNameEnglish = "Reinforced Guard";
                    data.displayNameChinese = "强化士兵";
                    data.localizationKey = "enemy.reinforced_guard";
                    data.hp = 2;
                    data.damage = 1;
                    data.walkSpeed = 3f;
                    data.runSpeed = 5.5f;
                    data.visionDistance = 15f;
                    data.visionAngle = 60f;
                    data.walkSoundRadius = 7.5f;
                    data.runSoundRadius = 18f;
                    data.commRange = 30f;
                    data.canPatrol = true;
                    data.canChase = true;
                    break;

                case EEnemyType.Heavy:
                    data.displayNameEnglish = "Heavy Guard";
                    data.displayNameChinese = "重型兵";
                    data.localizationKey = "enemy.heavy_guard";
                    data.hp = 3;
                    data.damage = 2;
                    data.walkSpeed = 1.5f;
                    data.runSpeed = 2.5f;
                    data.visionDistance = 8f;
                    data.visionAngle = 90f;
                    data.walkSoundRadius = 20f;
                    data.runSoundRadius = 25f;
                    data.commRange = 25f;
                    data.canPatrol = true;
                    data.canChase = true;
                    data.canCallReinforcements = true;
                    break;

                case EEnemyType.Quipucamayoc:
                    data.displayNameEnglish = "Quipucamayoc";
                    data.displayNameChinese = "祭司";
                    data.localizationKey = "enemy.quipucamayoc";
                    data.hp = 1;
                    data.damage = 1;
                    data.walkSpeed = 2.5f;
                    data.runSpeed = 4f;
                    data.visionDistance = 12f;
                    data.visionAngle = 80f;
                    data.commRange = 40f;
                    data.canPatrol = false;
                    data.canChase = true;
                    data.canCallReinforcements = true;
                    data.isInvisibleToStealth = true;
                    break;

                case EEnemyType.Saqueos:
                    data.displayNameEnglish = "Saqueos";
                    data.displayNameChinese = "噬掠者";
                    data.localizationKey = "enemy.saqueos";
                    data.hp = 2;
                    data.damage = 1;
                    data.walkSpeed = 3.5f;
                    data.runSpeed = 6f;
                    data.visionDistance = 20f;
                    data.visionAngle = 120f;
                    data.walkSoundRadius = 15f;
                    data.runSoundRadius = 20f;
                    data.commRange = 0f;
                    data.canPatrol = false;
                    data.canChase = true;
                    data.canCallReinforcements = false;
                    data.isInvisibleToStealth = false;
                    break;
            }

            data.displayName = data.displayNameEnglish;
            return data;
        }

        public string GetDisplayName(SystemLanguage language)
        {
            string localized = LocalizationService.Get(
                localizationKey,
                fallbackEnglish: displayNameEnglish,
                fallbackChinese: displayNameChinese,
                languageOverride: language);
            if (!string.IsNullOrWhiteSpace(localized))
                return localized;

            return displayName;
        }
    }
}