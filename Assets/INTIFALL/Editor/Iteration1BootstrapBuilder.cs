using INTIFALL.AI;
using INTIFALL.Data;
using INTIFALL.Level;
using INTIFALL.Narrative;
using INTIFALL.System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace INTIFALL.Editor
{
    public static class Iteration1BootstrapBuilder
    {
        private sealed class LevelSceneSpec
        {
            public int LevelIndex;
            public string SceneName;
            public string ScenePath;
            public string DisplayName;

            public LevelSceneSpec(int levelIndex, string sceneName, string scenePath, string displayName)
            {
                LevelIndex = levelIndex;
                SceneName = sceneName;
                ScenePath = scenePath;
                DisplayName = displayName;
            }
        }

        private static readonly LevelSceneSpec[] LevelScenes =
        {
            new LevelSceneSpec(0, "Level01_Qhapaq_Passage", "Assets/Scenes/Level01_Qhapaq_Passage.unity", "Golden Ruins"),
            new LevelSceneSpec(1, "Level02_Temple_Complex", "Assets/Scenes/Level02_Temple_Complex.unity", "Archive Maze"),
            new LevelSceneSpec(2, "Level03_Underground_Labs", "Assets/Scenes/Level03_Underground_Labs.unity", "Golden Bloodline"),
            new LevelSceneSpec(3, "Level04_Qhipu_Core", "Assets/Scenes/Level04_Qhipu_Core.unity", "Qhipu Core"),
            new LevelSceneSpec(4, "Level05_General_Taki_Villa", "Assets/Scenes/Level05_General_Taki_Villa.unity", "Solar Fall")
        };

        [MenuItem("INTIFALL/Recovery/Apply Iteration 1 Bootstrap")]
        public static void ApplyIteration1BootstrapMenu()
        {
            ApplyIteration1Bootstrap();
        }

        public static void ApplyIteration1Bootstrap()
        {
            AssetRecoveryBootstrap.GeneratePlaceholderAssets();

            int sceneSaveCount = 0;
            bool hasAssetChanges = false;

            foreach (LevelSceneSpec spec in LevelScenes)
            {
                LevelData levelData = AssetDatabase.LoadAssetAtPath<LevelData>(
                    $"Assets/INTIFALL/ScriptableObjects/Levels/LevelData_{spec.SceneName}.asset");
                EnemySpawnData enemySpawnData = AssetDatabase.LoadAssetAtPath<EnemySpawnData>(
                    $"Assets/INTIFALL/ScriptableObjects/Spawns/EnemySpawn_{spec.SceneName}.asset");
                IntelSpawnData intelSpawnData = AssetDatabase.LoadAssetAtPath<IntelSpawnData>(
                    $"Assets/INTIFALL/ScriptableObjects/Spawns/IntelSpawn_{spec.SceneName}.asset");

                if (levelData == null || enemySpawnData == null || intelSpawnData == null)
                {
                    Debug.LogWarning($"[Iteration1] Missing data asset for scene {spec.SceneName}, skipped.");
                    continue;
                }

                ConfigureLevelAssetData(spec, levelData, enemySpawnData, intelSpawnData);
                hasAssetChanges = true;

                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(spec.ScenePath) == null)
                {
                    Debug.LogWarning($"[Iteration1] Missing scene asset: {spec.ScenePath}");
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(spec.ScenePath, OpenSceneMode.Single);
                bool sceneChanged = ConfigureSceneForIteration1(spec, levelData, enemySpawnData, intelSpawnData);

                if (sceneChanged)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    sceneSaveCount++;
                    Debug.Log($"[Iteration1] Updated scene: {spec.ScenePath}");
                }
            }

            if (hasAssetChanges)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log($"[Iteration1] Bootstrap complete. Scenes saved: {sceneSaveCount}/{LevelScenes.Length}");
        }

        private static void ConfigureLevelAssetData(
            LevelSceneSpec spec,
            LevelData levelData,
            EnemySpawnData enemySpawnData,
            IntelSpawnData intelSpawnData)
        {
            enemySpawnData.levelIndex = spec.LevelIndex;
            enemySpawnData.levelName = spec.SceneName;
            enemySpawnData.spawnPoints = BuildEnemySpawns(spec.SceneName);
            enemySpawnData.availablePatrolRoutes = new[]
            {
                "route_outer",
                "route_inner"
            };
            enemySpawnData.maxConcurrentAlert = 2;
            enemySpawnData.communicationGroupSize = 3;

            intelSpawnData.levelIndex = spec.LevelIndex;
            intelSpawnData.levelName = spec.SceneName;
            intelSpawnData.intelPoints = BuildIntelSpawns(spec.SceneName);
            intelSpawnData.supplyPoints = new[]
            {
                new SupplyPointData
                {
                    supplyId = $"{spec.SceneName}_supply_01",
                    position = new Vector3(-13f, 0.6f, 8f),
                    providesFirstAid = true,
                    providesTools = true,
                    cooldownDuration = 20f
                }
            };
            intelSpawnData.exitPoints = new[]
            {
                new ExitPointData
                {
                    exitId = $"{spec.SceneName}_exit_main",
                    position = new Vector3(0f, 1f, 42f),
                    requiresAllIntel = true,
                    isMainExit = true
                }
            };
            intelSpawnData.ventEntrancePositions = new[]
            {
                new Vector3(-16f, 0f, 20f)
            };
            intelSpawnData.ventExitPositions = new[]
            {
                new Vector3(16f, 0f, 24f)
            };

            levelData.levelName = spec.SceneName;
            levelData.levelDisplayName = spec.DisplayName;
            levelData.levelIndex = spec.LevelIndex;
            levelData.sceneName = spec.SceneName;
            levelData.standardTime = 360f;
            levelData.timeLimit = 900f;
            levelData.totalEnemyCount = enemySpawnData.spawnPoints.Length;
            levelData.normalEnemyCount = CountEnemies(enemySpawnData.spawnPoints, EEnemySpawnType.Normal);
            levelData.reinforcedEnemyCount = CountEnemies(enemySpawnData.spawnPoints, EEnemySpawnType.Reinforced);
            levelData.heavyEnemyCount = CountEnemies(enemySpawnData.spawnPoints, EEnemySpawnType.Heavy);
            levelData.quipucamayocCount = CountEnemies(enemySpawnData.spawnPoints, EEnemySpawnType.Quipucamayoc);
            levelData.saqueosCount = CountEnemies(enemySpawnData.spawnPoints, EEnemySpawnType.Saqueos);
            levelData.qhipuFragmentCount = intelSpawnData.intelPoints.Length;
            levelData.terminalCount = 1;
            levelData.supplyPointCount = intelSpawnData.supplyPoints.Length;
            levelData.hasVentSystem = true;
            levelData.hasHangingPoints = true;
            levelData.hasBreakableWalls = false;
            levelData.hasElectronicDoors = false;
            levelData.hasSurveillanceCameras = false;
            levelData.baseCreditReward = 220 + (spec.LevelIndex * 20);
            levelData.secondaryObjectiveBonus = 60;
            levelData.zeroKillBonus = 150;
            levelData.noDamageBonus = 180;

            EditorUtility.SetDirty(enemySpawnData);
            EditorUtility.SetDirty(intelSpawnData);
            EditorUtility.SetDirty(levelData);
        }

        private static EnemySpawnPoint[] BuildEnemySpawns(string sceneName)
        {
            return new[]
            {
                new EnemySpawnPoint
                {
                    spawnId = $"{sceneName}_enemy_01",
                    position = new Vector3(10f, 0f, 12f),
                    rotation = Quaternion.identity,
                    enemyType = EEnemySpawnType.Normal,
                    isPatrol = true,
                    patrolRouteId = "route_outer",
                    awarenessLevel = 0
                },
                new EnemySpawnPoint
                {
                    spawnId = $"{sceneName}_enemy_02",
                    position = new Vector3(-10f, 0f, 22f),
                    rotation = Quaternion.Euler(0f, 180f, 0f),
                    enemyType = EEnemySpawnType.Normal,
                    isPatrol = true,
                    patrolRouteId = "route_outer",
                    awarenessLevel = 0
                },
                new EnemySpawnPoint
                {
                    spawnId = $"{sceneName}_enemy_03",
                    position = new Vector3(5f, 0f, 30f),
                    rotation = Quaternion.Euler(0f, 90f, 0f),
                    enemyType = EEnemySpawnType.Reinforced,
                    isPatrol = true,
                    patrolRouteId = "route_inner",
                    awarenessLevel = 1
                }
            };
        }

        private static IntelSpawnPoint[] BuildIntelSpawns(string sceneName)
        {
            return new[]
            {
                new IntelSpawnPoint
                {
                    intelId = $"{sceneName}_intel_01",
                    position = new Vector3(12f, 0.6f, 14f),
                    intelType = EIntelType.QhipuFragment,
                    displayName = "Qhipu Fragment 01",
                    description = "Recovered from outer patrol corridor.",
                    isHidden = false,
                    triggerEvents = new string[0]
                },
                new IntelSpawnPoint
                {
                    intelId = $"{sceneName}_intel_02",
                    position = new Vector3(-9f, 0.6f, 24f),
                    intelType = EIntelType.TerminalDocument,
                    displayName = "Archive Terminal 02",
                    description = "Contains route-map fragments.",
                    isHidden = false,
                    triggerEvents = new string[0]
                },
                new IntelSpawnPoint
                {
                    intelId = $"{sceneName}_intel_03",
                    position = new Vector3(0f, 0.6f, 34f),
                    intelType = EIntelType.AudioLog,
                    displayName = "Audio Log 03",
                    description = "Enemy command channel recording.",
                    isHidden = false,
                    triggerEvents = new string[0]
                }
            };
        }

        private static bool ConfigureSceneForIteration1(
            LevelSceneSpec spec,
            LevelData levelData,
            EnemySpawnData enemySpawnData,
            IntelSpawnData intelSpawnData)
        {
            bool changed = false;

            GameObject runtimeRoot = GetOrCreateRootObject("INTIFALL_Runtime", ref changed);
            Transform playerSpawn = GetOrCreateChild(runtimeRoot.transform, "PlayerSpawnPoint", ref changed);
            Transform enemyParent = GetOrCreateChild(runtimeRoot.transform, "EnemyRuntime", ref changed);
            Transform intelParent = GetOrCreateChild(runtimeRoot.transform, "IntelRuntime", ref changed);
            Transform systemsRoot = GetOrCreateChild(runtimeRoot.transform, "Systems", ref changed);

            playerSpawn.position = new Vector3(0f, 1f, 2f);
            enemyParent.position = Vector3.zero;
            intelParent.position = Vector3.zero;

            changed |= EnsureManagerComponents(systemsRoot);
            changed |= EnsureLevelLoader(runtimeRoot, playerSpawn, enemyParent, intelParent, levelData, enemySpawnData, intelSpawnData);
            changed |= EnsureGrayboxGeometry(runtimeRoot.transform);
            changed |= EnsureCameraAndLight();

            return changed;
        }

        private static bool EnsureManagerComponents(Transform systemsRoot)
        {
            bool changed = false;
            changed |= EnsureChildComponent<GameManager>(systemsRoot, "GameManager");
            changed |= EnsureChildComponent<LevelFlowManager>(systemsRoot, "LevelFlowManager");
            changed |= EnsureChildComponent<NarrativeManager>(systemsRoot, "NarrativeManager");
            return changed;
        }

        private static bool EnsureLevelLoader(
            GameObject runtimeRoot,
            Transform playerSpawn,
            Transform enemyParent,
            Transform intelParent,
            LevelData levelData,
            EnemySpawnData enemySpawnData,
            IntelSpawnData intelSpawnData)
        {
            bool changed = false;

            LevelLoader loader = runtimeRoot.GetComponent<LevelLoader>();
            if (loader == null)
            {
                loader = runtimeRoot.AddComponent<LevelLoader>();
                changed = true;
            }

            var serialized = new SerializedObject(loader);
            changed |= SetObjectReference(serialized, "levelData", levelData);
            changed |= SetObjectReference(serialized, "enemySpawnData", enemySpawnData);
            changed |= SetObjectReference(serialized, "intelSpawnData", intelSpawnData);
            changed |= SetBool(serialized, "autoResolveDataBySceneName", true);
            changed |= SetBool(serialized, "autoCreatePlaceholderPlayer", true);
            changed |= SetBool(serialized, "spawnMissionExit", true);
            changed |= SetObjectReference(serialized, "playerSpawnPoint", playerSpawn);
            changed |= SetObjectReference(serialized, "enemyParent", enemyParent);
            changed |= SetObjectReference(serialized, "intelParent", intelParent);
            changed |= SetArraySize(serialized, "enemyPrefabs", 0);
            changed |= SetArraySize(serialized, "intelPrefabs", 0);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return changed;
        }

        private static bool EnsureGrayboxGeometry(Transform runtimeRoot)
        {
            bool changed = false;

            Transform grayboxRoot = GetOrCreateChild(runtimeRoot, "Iteration1_Graybox", ref changed);
            GameObject ground = GetOrCreatePrimitive(grayboxRoot, "Ground", PrimitiveType.Plane, ref changed);
            ground.transform.position = new Vector3(0f, 0f, 20f);
            ground.transform.rotation = Quaternion.identity;
            ground.transform.localScale = new Vector3(8f, 1f, 8f);

            GameObject northWall = GetOrCreatePrimitive(grayboxRoot, "NorthWall", PrimitiveType.Cube, ref changed);
            northWall.transform.position = new Vector3(0f, 2f, 45f);
            northWall.transform.localScale = new Vector3(80f, 4f, 1f);

            GameObject southWall = GetOrCreatePrimitive(grayboxRoot, "SouthWall", PrimitiveType.Cube, ref changed);
            southWall.transform.position = new Vector3(0f, 2f, -5f);
            southWall.transform.localScale = new Vector3(80f, 4f, 1f);

            GameObject eastWall = GetOrCreatePrimitive(grayboxRoot, "EastWall", PrimitiveType.Cube, ref changed);
            eastWall.transform.position = new Vector3(40f, 2f, 20f);
            eastWall.transform.localScale = new Vector3(1f, 4f, 50f);

            GameObject westWall = GetOrCreatePrimitive(grayboxRoot, "WestWall", PrimitiveType.Cube, ref changed);
            westWall.transform.position = new Vector3(-40f, 2f, 20f);
            westWall.transform.localScale = new Vector3(1f, 4f, 50f);

            GameObject coverA = GetOrCreatePrimitive(grayboxRoot, "CoverA", PrimitiveType.Cube, ref changed);
            coverA.transform.position = new Vector3(8f, 1f, 18f);
            coverA.transform.localScale = new Vector3(4f, 2f, 1.5f);

            GameObject coverB = GetOrCreatePrimitive(grayboxRoot, "CoverB", PrimitiveType.Cube, ref changed);
            coverB.transform.position = new Vector3(-8f, 1f, 28f);
            coverB.transform.localScale = new Vector3(4f, 2f, 1.5f);

            return changed;
        }

        private static bool EnsureCameraAndLight()
        {
            bool changed = false;

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraGo = new GameObject("Main Camera");
                cameraGo.tag = "MainCamera";
                mainCamera = cameraGo.AddComponent<Camera>();
                cameraGo.AddComponent<AudioListener>();
                changed = true;
            }

            mainCamera.transform.position = new Vector3(0f, 22f, -20f);
            mainCamera.transform.rotation = Quaternion.Euler(32f, 0f, 0f);

            Light directional = Object.FindFirstObjectByType<Light>();
            if (directional == null)
            {
                GameObject lightGo = new GameObject("Directional Light");
                directional = lightGo.AddComponent<Light>();
                directional.type = LightType.Directional;
                changed = true;
            }

            directional.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            directional.intensity = 1.2f;

            return changed;
        }

        private static bool EnsureChildComponent<T>(Transform root, string childName) where T : Component
        {
            bool changed = false;
            Transform child = root.Find(childName);
            if (child == null)
            {
                GameObject childGo = new GameObject(childName);
                childGo.transform.SetParent(root, false);
                child = childGo.transform;
                changed = true;
            }

            if (child.GetComponent<T>() == null)
            {
                child.gameObject.AddComponent<T>();
                changed = true;
            }

            return changed;
        }

        private static GameObject GetOrCreateRootObject(string name, ref bool changed)
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null)
                return existing;

            changed = true;
            return new GameObject(name);
        }

        private static Transform GetOrCreateChild(Transform parent, string name, ref bool changed)
        {
            Transform child = parent.Find(name);
            if (child != null)
                return child;

            GameObject childGo = new GameObject(name);
            childGo.transform.SetParent(parent, false);
            changed = true;
            return childGo.transform;
        }

        private static GameObject GetOrCreatePrimitive(Transform parent, string name, PrimitiveType type, ref bool changed)
        {
            Transform child = parent.Find(name);
            if (child != null)
                return child.gameObject;

            GameObject primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            changed = true;
            return primitive;
        }

        private static int CountEnemies(EnemySpawnPoint[] spawnPoints, EEnemySpawnType type)
        {
            int count = 0;
            if (spawnPoints == null)
                return count;

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i].enemyType == type)
                    count++;
            }
            return count;
        }

        private static bool SetObjectReference(SerializedObject serialized, string propertyName, Object value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
                return false;

            if (property.objectReferenceValue == value)
                return false;

            property.objectReferenceValue = value;
            return true;
        }

        private static bool SetBool(SerializedObject serialized, string propertyName, bool value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
                return false;

            if (property.boolValue == value)
                return false;

            property.boolValue = value;
            return true;
        }

        private static bool SetArraySize(SerializedObject serialized, string propertyName, int size)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null || !property.isArray)
                return false;

            if (property.arraySize == size)
                return false;

            property.arraySize = size;
            return true;
        }
    }
}
