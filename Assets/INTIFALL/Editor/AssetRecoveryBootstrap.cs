using System.Collections.Generic;
using INTIFALL.AI;
using INTIFALL.Data;
using INTIFALL.Tools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace INTIFALL.Editor
{
    public static class AssetRecoveryBootstrap
    {
        private sealed class SceneSpec
        {
            public string Path;
            public string Marker;

            public SceneSpec(string path, string marker)
            {
                Path = path;
                Marker = marker;
            }
        }

        private sealed class ToolSpec
        {
            public string Name;
            public EToolCategory Category;
            public EToolSlot Slot;
            public int UnlockLevel;
            public int UnlockPrice;
            public int UpgradePrice;
            public string PrefabSourceName;

            public ToolSpec(
                string name,
                EToolCategory category,
                EToolSlot slot,
                int unlockLevel,
                int unlockPrice,
                int upgradePrice,
                string prefabSourceName)
            {
                Name = name;
                Category = category;
                Slot = slot;
                UnlockLevel = unlockLevel;
                UnlockPrice = unlockPrice;
                UpgradePrice = upgradePrice;
                PrefabSourceName = prefabSourceName;
            }
        }

        private static readonly SceneSpec[] SceneSpecs =
        {
            new SceneSpec("Assets/SharedAssets/Benchmark/BenchmarkScene.unity", "Benchmark"),
            new SceneSpec("Assets/Scenes/Terminal/TerminalScene.unity", "Terminal"),
            new SceneSpec("Assets/Scenes/Oasis/OasisScene.unity", "Oasis"),
            new SceneSpec("Assets/Scenes/Garden/GardenScene.unity", "Garden"),
            new SceneSpec("Assets/Scenes/Cockpit/CockpitScene.unity", "Cockpit"),
            new SceneSpec("Assets/Scenes/Level01_Qhapaq_Passage.unity", "Level01"),
            new SceneSpec("Assets/Scenes/Level02_Temple_Complex.unity", "Level02"),
            new SceneSpec("Assets/Scenes/Level03_Underground_Labs.unity", "Level03"),
            new SceneSpec("Assets/Scenes/Level04_Qhipu_Core.unity", "Level04"),
            new SceneSpec("Assets/Scenes/Level05_General_Taki_Villa.unity", "Level05")
        };

        private static readonly ToolSpec[] ToolSpecs =
        {
            new ToolSpec("SmokeBomb", EToolCategory.PerceptionDisrupt, EToolSlot.Slot1, 1, 100, 120, "SmokeBomb"),
            new ToolSpec("FlashBang", EToolCategory.PerceptionDisrupt, EToolSlot.Slot1, 1, 100, 120, "FlashBang"),
            new ToolSpec("SleepDart", EToolCategory.DirectRemove, EToolSlot.Slot3, 1, 120, 140, "SleepDart"),
            new ToolSpec("EMP", EToolCategory.PerceptionDisrupt, EToolSlot.Slot2, 3, 200, 240, "EMP"),
            new ToolSpec("TimedNoise", EToolCategory.AttentionShift, EToolSlot.Slot2, 2, 160, 200, "TimedNoise"),
            new ToolSpec("SoundBait", EToolCategory.AttentionShift, EToolSlot.Slot3, 1, 110, 150, "SoundBait"),
            new ToolSpec("DroneInterference", EToolCategory.AttentionShift, EToolSlot.Slot4, 3, 240, 280, "DroneInterference"),
            new ToolSpec("WallBreaker", EToolCategory.Environmental, EToolSlot.Slot2, 2, 180, 220, "WallBreaker"),
            new ToolSpec("WallBreak", EToolCategory.Environmental, EToolSlot.Slot2, 2, 180, 220, "WallBreaker"),
            new ToolSpec("Drone", EToolCategory.AttentionShift, EToolSlot.Slot4, 3, 240, 280, "DroneInterference"),
            new ToolSpec("Rope", EToolCategory.Environmental, EToolSlot.Slot4, 1, 80, 120, string.Empty)
        };

        private static readonly string[] LevelSceneNames =
        {
            "Level01_Qhapaq_Passage",
            "Level02_Temple_Complex",
            "Level03_Underground_Labs",
            "Level04_Qhipu_Core",
            "Level05_General_Taki_Villa"
        };

        [MenuItem("INTIFALL/Recovery/Generate Placeholder Assets")]
        public static void GeneratePlaceholderAssetsMenu()
        {
            GeneratePlaceholderAssets();
        }

        public static void GeneratePlaceholderAssets()
        {
            Debug.Log("[AssetRecovery] Starting placeholder asset recovery.");

            CreateScenes();
            CreateToolPrefabs();
            CreateScriptableObjects();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[AssetRecovery] Placeholder asset recovery completed.");
        }

        private static void CreateScenes()
        {
            foreach (SceneSpec spec in SceneSpecs)
            {
                EnsureFolderPath(GetParentFolder(spec.Path));

                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(spec.Path) != null)
                {
                    continue;
                }

                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                var marker = new GameObject("PLACEHOLDER_" + spec.Marker);
                marker.transform.position = Vector3.zero;

                EditorSceneManager.SaveScene(scene, spec.Path);
                Debug.Log("[AssetRecovery] Created scene: " + spec.Path);
            }
        }

        private static void CreateToolPrefabs()
        {
            const string resourcesToolsFolder = "Assets/Resources/Prefabs/Tools";
            const string intifallToolsFolder = "Assets/INTIFALL/Prefabs/Tools";

            EnsureFolderPath(resourcesToolsFolder);
            EnsureFolderPath(intifallToolsFolder);

            CreateToolPrefab<SmokeBomb>("SmokeBomb", resourcesToolsFolder, intifallToolsFolder);
            CreateToolPrefab<FlashBang>("FlashBang", resourcesToolsFolder, intifallToolsFolder);
            CreateToolPrefab<SleepDart>("SleepDart", resourcesToolsFolder, intifallToolsFolder);
            CreateToolPrefab<EMP>("EMP", resourcesToolsFolder, intifallToolsFolder);
            CreateToolPrefab<TimedNoise>("TimedNoise", resourcesToolsFolder, intifallToolsFolder);
            CreateToolPrefab<SoundBait>("SoundBait", resourcesToolsFolder, intifallToolsFolder);
            CreateToolPrefab<DroneInterference>("DroneInterference", resourcesToolsFolder, intifallToolsFolder);
            CreateToolPrefab<WallBreaker>("WallBreaker", resourcesToolsFolder, intifallToolsFolder);
        }

        private static void CreateToolPrefab<T>(string prefabName, string folderA, string folderB) where T : ToolBase
        {
            string pathA = folderA + "/" + prefabName + ".prefab";
            string pathB = folderB + "/" + prefabName + ".prefab";

            CreatePrefabIfMissing<T>(prefabName, pathA);
            CreatePrefabIfMissing<T>(prefabName, pathB);
        }

        private static void CreatePrefabIfMissing<T>(string objectName, string assetPath) where T : ToolBase
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(assetPath) != null)
            {
                return;
            }

            var go = new GameObject(objectName);
            go.AddComponent<T>();

            PrefabUtility.SaveAsPrefabAsset(go, assetPath);
            Object.DestroyImmediate(go);

            Debug.Log("[AssetRecovery] Created prefab: " + assetPath);
        }

        private static void CreateScriptableObjects()
        {
            EnsureFolderPath("Assets/INTIFALL/ScriptableObjects");
            EnsureFolderPath("Assets/INTIFALL/ScriptableObjects/EnemyTypes");
            EnsureFolderPath("Assets/INTIFALL/ScriptableObjects/Tools");
            EnsureFolderPath("Assets/INTIFALL/ScriptableObjects/Levels");
            EnsureFolderPath("Assets/INTIFALL/ScriptableObjects/Spawns");

            CreateGameConfigAsset();
            CreateEnemyTypeAssets();
            CreateToolDataAssets();
            CreateLevelDataAssets();
            CreateSpawnAssets();
        }

        private static void CreateGameConfigAsset()
        {
            const string path = "Assets/INTIFALL/ScriptableObjects/GameConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<GameConfig>(path) != null)
            {
                return;
            }

            GameConfig config = GameConfig.DefaultConfig();
            AssetDatabase.CreateAsset(config, path);
            Debug.Log("[AssetRecovery] Created asset: " + path);
        }

        private static void CreateEnemyTypeAssets()
        {
            foreach (EEnemyType type in (EEnemyType[])global::System.Enum.GetValues(typeof(EEnemyType)))
            {
                string path = "Assets/INTIFALL/ScriptableObjects/EnemyTypes/EnemyType_" + type + ".asset";
                if (AssetDatabase.LoadAssetAtPath<EnemyTypeData>(path) != null)
                {
                    continue;
                }

                EnemyTypeData data = EnemyTypeData.GetDefaultData(type);
                AssetDatabase.CreateAsset(data, path);
                Debug.Log("[AssetRecovery] Created asset: " + path);
            }
        }

        private static void CreateToolDataAssets()
        {
            var levelUnlockMap = new Dictionary<string, ToolData>();

            foreach (ToolSpec spec in ToolSpecs)
            {
                string path = "Assets/INTIFALL/ScriptableObjects/Tools/ToolData_" + spec.Name + ".asset";
                ToolData existing = AssetDatabase.LoadAssetAtPath<ToolData>(path);
                if (existing != null)
                {
                    levelUnlockMap[spec.Name] = existing;
                    continue;
                }

                ToolData data = ScriptableObject.CreateInstance<ToolData>();
                data.toolName = spec.Name;
                data.toolNameCN = spec.Name;
                data.category = spec.Category;
                data.defaultSlot = spec.Slot;
                data.unlockLevel = spec.UnlockLevel;
                data.unlockPrice = spec.UnlockPrice;
                data.upgradePrice = spec.UpgradePrice;
                data.unlockedByDefault = spec.UnlockLevel <= 1;

                if (!string.IsNullOrEmpty(spec.PrefabSourceName))
                {
                    string prefabPath = "Assets/Resources/Prefabs/Tools/" + spec.PrefabSourceName + ".prefab";
                    data.runtimePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                }

                AssetDatabase.CreateAsset(data, path);
                levelUnlockMap[spec.Name] = data;
                Debug.Log("[AssetRecovery] Created asset: " + path);
            }
        }

        private static void CreateLevelDataAssets()
        {
            for (int i = 0; i < LevelSceneNames.Length; i++)
            {
                string sceneName = LevelSceneNames[i];
                string path = "Assets/INTIFALL/ScriptableObjects/Levels/LevelData_" + sceneName + ".asset";

                if (AssetDatabase.LoadAssetAtPath<LevelData>(path) != null)
                {
                    continue;
                }

                LevelData data = ScriptableObject.CreateInstance<LevelData>();
                data.levelName = sceneName;
                data.levelDisplayName = sceneName;
                data.levelIndex = i;
                data.sceneName = sceneName;
                AssetDatabase.CreateAsset(data, path);
                Debug.Log("[AssetRecovery] Created asset: " + path);
            }
        }

        private static void CreateSpawnAssets()
        {
            for (int i = 0; i < LevelSceneNames.Length; i++)
            {
                string sceneName = LevelSceneNames[i];

                string enemyPath = "Assets/INTIFALL/ScriptableObjects/Spawns/EnemySpawn_" + sceneName + ".asset";
                if (AssetDatabase.LoadAssetAtPath<EnemySpawnData>(enemyPath) == null)
                {
                    EnemySpawnData enemyData = ScriptableObject.CreateInstance<EnemySpawnData>();
                    enemyData.levelIndex = i;
                    enemyData.levelName = sceneName;
                    enemyData.spawnPoints = new EnemySpawnPoint[0];
                    enemyData.availablePatrolRoutes = new string[0];
                    AssetDatabase.CreateAsset(enemyData, enemyPath);
                    Debug.Log("[AssetRecovery] Created asset: " + enemyPath);
                }

                string intelPath = "Assets/INTIFALL/ScriptableObjects/Spawns/IntelSpawn_" + sceneName + ".asset";
                if (AssetDatabase.LoadAssetAtPath<IntelSpawnData>(intelPath) == null)
                {
                    IntelSpawnData intelData = ScriptableObject.CreateInstance<IntelSpawnData>();
                    intelData.levelIndex = i;
                    intelData.levelName = sceneName;
                    intelData.intelPoints = new IntelSpawnPoint[0];
                    intelData.supplyPoints = new SupplyPointData[0];
                    intelData.exitPoints = new ExitPointData[0];
                    intelData.ventEntrancePositions = new Vector3[0];
                    intelData.ventExitPositions = new Vector3[0];
                    AssetDatabase.CreateAsset(intelData, intelPath);
                    Debug.Log("[AssetRecovery] Created asset: " + intelPath);
                }
            }
        }

        private static void EnsureFolderPath(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return;
            }

            string[] parts = folderPath.Split('/');
            if (parts.Length == 0)
            {
                return;
            }

            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

        private static string GetParentFolder(string assetPath)
        {
            int slashIndex = assetPath.LastIndexOf('/');
            if (slashIndex <= 0)
            {
                return string.Empty;
            }
            return assetPath.Substring(0, slashIndex);
        }
    }
}
