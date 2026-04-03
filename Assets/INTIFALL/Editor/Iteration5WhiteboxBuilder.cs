using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using INTIFALL.AI;
using INTIFALL.Data;
using INTIFALL.Environment;
using INTIFALL.Level;
using INTIFALL.Narrative;
using INTIFALL.System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace INTIFALL.Editor
{
    public static class Iteration5WhiteboxBuilder
    {
        private const string ReportPath = "Documents/Iteration5_Whitebox_Rebuild_Report_2026-04-01.md";

        private enum GeometryLayer
        {
            Floor,
            Wall,
            Cover,
            Landmark,
            Route
        }

        private enum MarkerType
        {
            HangingPoint,
            VentEntrance,
            BreakableWall,
            ElectronicDoor,
            SurveillanceCamera
        }

        private sealed class LevelSpec
        {
            public int LevelIndex;
            public string SceneName;
            public string ScenePath;
            public string DisplayName;
            public float StandardTimeSeconds;
            public float TimeLimitSeconds;
            public int BaseCreditReward;
            public int SecondaryBonus;
            public int ZeroKillBonus;
            public int NoDamageBonus;
            public bool HasVentSystem;
            public bool HasHangingPoints;
            public bool HasBreakableWalls;
            public bool HasElectronicDoors;
            public bool HasSurveillanceCameras;
            public int MaxConcurrentAlert;
            public int CommunicationGroupSize;
            public Vector3 PlayerSpawn;
            public Vector3 CameraPosition;
            public Vector3 CameraEuler;
            public BoxSpec[] Geometry;
            public MarkerSpec[] Markers;
            public EnemySpawnSpec[] EnemySpawns;
            public IntelSpec[] IntelSpawns;
            public SupplySpec[] Supplies;
            public ExitSpec[] Exits;
        }

        private sealed class BoxSpec
        {
            public string Name;
            public GeometryLayer Layer;
            public Vector3 Position;
            public Vector3 Scale;

            public BoxSpec(string name, GeometryLayer layer, Vector3 position, Vector3 scale)
            {
                Name = name;
                Layer = layer;
                Position = position;
                Scale = scale;
            }
        }

        private sealed class MarkerSpec
        {
            public string Name;
            public MarkerType Type;
            public Vector3 Position;
            public Vector3 Scale;
            public Vector3 VentExitPosition;

            public MarkerSpec(string name, MarkerType type, Vector3 position, Vector3 scale)
            {
                Name = name;
                Type = type;
                Position = position;
                Scale = scale;
                VentExitPosition = position;
            }
        }

        private sealed class EnemySpawnSpec
        {
            public string SpawnId;
            public Vector3 Position;
            public Vector3 Euler;
            public EEnemySpawnType EnemyType;
            public bool IsPatrol;
            public string PatrolRouteId;
            public int AwarenessLevel;

            public EnemySpawnSpec(
                string spawnId,
                Vector3 position,
                Vector3 euler,
                EEnemySpawnType enemyType,
                bool isPatrol,
                string patrolRouteId,
                int awarenessLevel)
            {
                SpawnId = spawnId;
                Position = position;
                Euler = euler;
                EnemyType = enemyType;
                IsPatrol = isPatrol;
                PatrolRouteId = patrolRouteId;
                AwarenessLevel = awarenessLevel;
            }
        }

        private sealed class IntelSpec
        {
            public string IntelId;
            public EIntelType IntelType;
            public Vector3 Position;
            public string DisplayName;
            public string Description;
            public bool IsHidden;

            public IntelSpec(
                string intelId,
                EIntelType intelType,
                Vector3 position,
                string displayName,
                string description,
                bool isHidden)
            {
                IntelId = intelId;
                IntelType = intelType;
                Position = position;
                DisplayName = displayName;
                Description = description;
                IsHidden = isHidden;
            }
        }

        private sealed class SupplySpec
        {
            public string SupplyId;
            public Vector3 Position;
            public bool ProvidesFirstAid;
            public bool ProvidesTools;
            public float Cooldown;

            public SupplySpec(string supplyId, Vector3 position, bool providesFirstAid, bool providesTools, float cooldown)
            {
                SupplyId = supplyId;
                Position = position;
                ProvidesFirstAid = providesFirstAid;
                ProvidesTools = providesTools;
                Cooldown = cooldown;
            }
        }

        private sealed class ExitSpec
        {
            public string ExitId;
            public Vector3 Position;
            public bool RequiresAllIntel;
            public bool IsMainExit;

            public ExitSpec(string exitId, Vector3 position, bool requiresAllIntel, bool isMainExit)
            {
                ExitId = exitId;
                Position = position;
                RequiresAllIntel = requiresAllIntel;
                IsMainExit = isMainExit;
            }
        }

        private sealed class RunReport
        {
            public int SceneCount;
            public int SceneUpdatedCount;
            public int SceneMissingCount;
            public int LegacyWhiteboxDeletedCount;
            public readonly List<string> Notes = new();
        }

        [MenuItem("INTIFALL/Iteration5/Rebuild Whitebox + Enemy Config")]
        public static void RebuildWhiteboxAndConfigMenu()
        {
            RebuildWhiteboxAndConfigBatch();
        }

        // Command line entry:
        // Unity.exe -batchmode -quit -projectPath <path> -executeMethod INTIFALL.Editor.Iteration5WhiteboxBuilder.RebuildWhiteboxAndConfigBatch
        public static void RebuildWhiteboxAndConfigBatch()
        {
            AssetRecoveryBootstrap.GeneratePlaceholderAssets();

            LevelSpec[] specs = BuildSpecs();
            var report = new RunReport
            {
                SceneCount = specs.Length
            };

            for (int i = 0; i < specs.Length; i++)
            {
                LevelSpec spec = specs[i];
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(spec.ScenePath) == null)
                {
                    report.SceneMissingCount++;
                    report.Notes.Add($"[ERROR] Scene missing: {spec.ScenePath}");
                    continue;
                }

                ConfigureLevelAssets(spec, report);
                bool sceneChanged = ConfigureScene(spec, report);
                if (sceneChanged)
                    report.SceneUpdatedCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            WriteReport(report);
            Debug.Log(
                $"[Iteration5] Whitebox rebuild completed. " +
                $"Scenes updated={report.SceneUpdatedCount}/{report.SceneCount}, missing={report.SceneMissingCount}");
        }

        private static bool ConfigureScene(LevelSpec spec, RunReport report)
        {
            Scene scene = EditorSceneManager.OpenScene(spec.ScenePath, OpenSceneMode.Single);
            bool changed = false;

            GameObject runtimeRoot = GetOrCreateRoot("INTIFALL_Runtime", ref changed);
            Transform systems = GetOrCreateChild(runtimeRoot.transform, "Systems", Vector3.zero, ref changed);
            Transform playerSpawn = GetOrCreateChild(runtimeRoot.transform, "PlayerSpawnPoint", spec.PlayerSpawn, ref changed);
            Transform enemyRuntime = GetOrCreateChild(runtimeRoot.transform, "EnemyRuntime", Vector3.zero, ref changed);
            Transform intelRuntime = GetOrCreateChild(runtimeRoot.transform, "IntelRuntime", Vector3.zero, ref changed);

            playerSpawn.position = spec.PlayerSpawn;
            EnsureChildComponent<GameManager>(systems, "GameManager", ref changed);
            EnsureChildComponent<LevelFlowManager>(systems, "LevelFlowManager", ref changed);
            EnsureChildComponent<NarrativeManager>(systems, "NarrativeManager", ref changed);

            changed |= RebindLevelLoader(runtimeRoot, spec, playerSpawn, enemyRuntime, intelRuntime);
            changed |= CleanupLegacyWhitebox(scene, runtimeRoot.transform, report);
            changed |= BuildWhiteboxGeometry(runtimeRoot.transform, spec);
            changed |= BuildGameplayMarkers(runtimeRoot.transform, spec);
            changed |= EnsureCameraAndLight(spec);

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                report.Notes.Add($"[UPDATE] Rebuilt whitebox scene: {spec.SceneName}");
            }
            else
            {
                report.Notes.Add($"[PASS] Scene already matched target: {spec.SceneName}");
            }

            return changed;
        }

        private static bool RebindLevelLoader(
            GameObject runtimeRoot,
            LevelSpec spec,
            Transform playerSpawn,
            Transform enemyRuntime,
            Transform intelRuntime)
        {
            bool changed = false;
            LevelLoader loader = runtimeRoot.GetComponent<LevelLoader>();
            if (loader == null)
            {
                loader = runtimeRoot.AddComponent<LevelLoader>();
                changed = true;
            }

            LevelData levelData = AssetDatabase.LoadAssetAtPath<LevelData>(
                $"Assets/INTIFALL/ScriptableObjects/Levels/LevelData_{spec.SceneName}.asset");
            EnemySpawnData enemySpawnData = AssetDatabase.LoadAssetAtPath<EnemySpawnData>(
                $"Assets/INTIFALL/ScriptableObjects/Spawns/EnemySpawn_{spec.SceneName}.asset");
            IntelSpawnData intelSpawnData = AssetDatabase.LoadAssetAtPath<IntelSpawnData>(
                $"Assets/INTIFALL/ScriptableObjects/Spawns/IntelSpawn_{spec.SceneName}.asset");

            var serialized = new SerializedObject(loader);
            changed |= SetObjectReference(serialized, "levelData", levelData);
            changed |= SetObjectReference(serialized, "enemySpawnData", enemySpawnData);
            changed |= SetObjectReference(serialized, "intelSpawnData", intelSpawnData);
            changed |= SetObjectReference(serialized, "playerSpawnPoint", playerSpawn);
            changed |= SetObjectReference(serialized, "enemyParent", enemyRuntime);
            changed |= SetObjectReference(serialized, "intelParent", intelRuntime);
            changed |= SetBool(serialized, "autoResolveDataBySceneName", true);
            changed |= SetBool(serialized, "autoCreatePlaceholderPlayer", true);
            changed |= SetBool(serialized, "spawnMissionExit", true);
            changed |= SetArraySize(serialized, "enemyPrefabs", 0);
            changed |= SetArraySize(serialized, "intelPrefabs", 0);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return changed;
        }

        private static bool CleanupLegacyWhitebox(Scene scene, Transform runtimeRoot, RunReport report)
        {
            bool changed = false;
            string[] legacyNames = { "Iteration1_Graybox", "Iteration5_Whitebox" };
            for (int i = 0; i < legacyNames.Length; i++)
            {
                Transform legacy = runtimeRoot.Find(legacyNames[i]);
                if (legacy != null)
                {
                    UnityEngine.Object.DestroyImmediate(legacy.gameObject);
                    changed = true;
                    report.LegacyWhiteboxDeletedCount++;
                }
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (!root.name.StartsWith("PLACEHOLDER_", StringComparison.OrdinalIgnoreCase))
                    continue;

                UnityEngine.Object.DestroyImmediate(root);
                changed = true;
                report.LegacyWhiteboxDeletedCount++;
            }

            return changed;
        }

        private static bool BuildWhiteboxGeometry(Transform runtimeRoot, LevelSpec spec)
        {
            bool changed = false;
            GameObject whiteboxRoot = new("Iteration5_Whitebox");
            whiteboxRoot.transform.SetParent(runtimeRoot, false);
            changed = true;

            Transform floorRoot = CreateFolder(whiteboxRoot.transform, "Floor");
            Transform wallRoot = CreateFolder(whiteboxRoot.transform, "Walls");
            Transform coverRoot = CreateFolder(whiteboxRoot.transform, "Covers");
            Transform landmarkRoot = CreateFolder(whiteboxRoot.transform, "Landmarks");
            Transform routeRoot = CreateFolder(whiteboxRoot.transform, "Routes");

            for (int i = 0; i < spec.Geometry.Length; i++)
            {
                BoxSpec box = spec.Geometry[i];
                Transform parent = box.Layer switch
                {
                    GeometryLayer.Floor => floorRoot,
                    GeometryLayer.Wall => wallRoot,
                    GeometryLayer.Cover => coverRoot,
                    GeometryLayer.Landmark => landmarkRoot,
                    _ => routeRoot
                };

                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = box.Name;
                go.transform.SetParent(parent, false);
                go.transform.localPosition = box.Position;
                go.transform.localScale = box.Scale;
            }

            return changed;
        }

        private static bool BuildGameplayMarkers(Transform runtimeRoot, LevelSpec spec)
        {
            bool changed = false;
            Transform markerRoot = CreateFolder(runtimeRoot.Find("Iteration5_Whitebox"), "GameplayMarkers");

            for (int i = 0; i < spec.Markers.Length; i++)
            {
                MarkerSpec marker = spec.Markers[i];
                GameObject markerGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                markerGo.name = marker.Name;
                markerGo.transform.SetParent(markerRoot, false);
                markerGo.transform.localPosition = marker.Position;
                markerGo.transform.localScale = marker.Scale;

                BoxCollider baseCollider = markerGo.GetComponent<BoxCollider>();
                if (baseCollider != null)
                    baseCollider.isTrigger = marker.Type != MarkerType.BreakableWall;

                switch (marker.Type)
                {
                    case MarkerType.HangingPoint:
                        EnsureComponent<HangingPoint>(markerGo, ref changed);
                        break;
                    case MarkerType.VentEntrance:
                        VentEntrance vent = EnsureComponent<VentEntrance>(markerGo, ref changed);
                        var ventSerialized = new SerializedObject(vent);
                        changed |= SetVector3(ventSerialized, "exitPosition", marker.VentExitPosition);
                        ventSerialized.ApplyModifiedPropertiesWithoutUndo();
                        break;
                    case MarkerType.BreakableWall:
                        EnsureComponent<BreakableWall>(markerGo, ref changed);
                        break;
                    case MarkerType.ElectronicDoor:
                        EnsureComponent<ElectronicDoor>(markerGo, ref changed);
                        BoxCollider trigger = markerGo.AddComponent<BoxCollider>();
                        trigger.isTrigger = true;
                        trigger.size = new Vector3(marker.Scale.x + 1f, marker.Scale.y + 1f, marker.Scale.z + 1f);
                        changed = true;
                        break;
                    case MarkerType.SurveillanceCamera:
                        EnsureComponent<SurveillanceCamera>(markerGo, ref changed);
                        break;
                }

                changed = true;
            }

            return changed;
        }

        private static bool EnsureCameraAndLight(LevelSpec spec)
        {
            bool changed = false;

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraGo = new("Main Camera");
                cameraGo.tag = "MainCamera";
                mainCamera = cameraGo.AddComponent<Camera>();
                cameraGo.AddComponent<AudioListener>();
                changed = true;
            }

            if (mainCamera.transform.position != spec.CameraPosition)
            {
                mainCamera.transform.position = spec.CameraPosition;
                changed = true;
            }

            Quaternion targetRot = Quaternion.Euler(spec.CameraEuler);
            if (mainCamera.transform.rotation != targetRot)
            {
                mainCamera.transform.rotation = targetRot;
                changed = true;
            }

            Light directional = UnityEngine.Object.FindFirstObjectByType<Light>();
            if (directional == null)
            {
                GameObject lightGo = new("Directional Light");
                directional = lightGo.AddComponent<Light>();
                directional.type = LightType.Directional;
                changed = true;
            }

            Quaternion lightRot = Quaternion.Euler(50f, -30f, 0f);
            if (directional.transform.rotation != lightRot)
            {
                directional.transform.rotation = lightRot;
                changed = true;
            }

            if (Math.Abs(directional.intensity - 1.25f) > 0.001f)
            {
                directional.intensity = 1.25f;
                changed = true;
            }

            return changed;
        }

        private static void ConfigureLevelAssets(LevelSpec spec, RunReport report)
        {
            ConfigureLevelAssetPair(spec, "Assets/INTIFALL/ScriptableObjects", report);
            ConfigureLevelAssetPair(spec, "Assets/Resources/INTIFALL", report);
        }

        private static void ConfigureLevelAssetPair(LevelSpec spec, string baseRoot, RunReport report)
        {
            string levelPath = $"{baseRoot}/Levels/LevelData_{spec.SceneName}.asset";
            string enemyPath = $"{baseRoot}/Spawns/EnemySpawn_{spec.SceneName}.asset";
            string intelPath = $"{baseRoot}/Spawns/IntelSpawn_{spec.SceneName}.asset";

            LevelData levelData = LoadOrCreateAsset<LevelData>(levelPath);
            EnemySpawnData enemySpawnData = LoadOrCreateAsset<EnemySpawnData>(enemyPath);
            IntelSpawnData intelSpawnData = LoadOrCreateAsset<IntelSpawnData>(intelPath);

            if (levelData == null || enemySpawnData == null || intelSpawnData == null)
            {
                report.Notes.Add($"[ERROR] Failed to create level assets for {spec.SceneName} under {baseRoot}");
                return;
            }

            ApplyEnemyData(spec, enemySpawnData);
            ApplyIntelData(spec, intelSpawnData);
            ApplyLevelData(spec, levelData, enemySpawnData, intelSpawnData);

            EditorUtility.SetDirty(levelData);
            EditorUtility.SetDirty(enemySpawnData);
            EditorUtility.SetDirty(intelSpawnData);
        }

        private static void ApplyEnemyData(LevelSpec spec, EnemySpawnData data)
        {
            data.levelIndex = spec.LevelIndex;
            data.levelName = spec.SceneName;
            data.maxConcurrentAlert = spec.MaxConcurrentAlert;
            data.communicationGroupSize = spec.CommunicationGroupSize;

            var routeSet = new HashSet<string>(StringComparer.Ordinal);
            var spawns = new EnemySpawnPoint[spec.EnemySpawns.Length];
            for (int i = 0; i < spec.EnemySpawns.Length; i++)
            {
                EnemySpawnSpec specSpawn = spec.EnemySpawns[i];
                spawns[i] = new EnemySpawnPoint
                {
                    spawnId = specSpawn.SpawnId,
                    position = specSpawn.Position,
                    rotation = Quaternion.Euler(specSpawn.Euler),
                    enemyType = specSpawn.EnemyType,
                    isPatrol = specSpawn.IsPatrol,
                    patrolRouteId = specSpawn.PatrolRouteId,
                    awarenessLevel = specSpawn.AwarenessLevel
                };

                if (!string.IsNullOrEmpty(specSpawn.PatrolRouteId))
                    routeSet.Add(specSpawn.PatrolRouteId);
            }

            data.spawnPoints = spawns;
            string[] routes = new string[routeSet.Count];
            routeSet.CopyTo(routes);
            data.availablePatrolRoutes = routes;
        }

        private static void ApplyIntelData(LevelSpec spec, IntelSpawnData data)
        {
            data.levelIndex = spec.LevelIndex;
            data.levelName = spec.SceneName;

            var intelPoints = new IntelSpawnPoint[spec.IntelSpawns.Length];
            for (int i = 0; i < spec.IntelSpawns.Length; i++)
            {
                IntelSpec intel = spec.IntelSpawns[i];
                intelPoints[i] = new IntelSpawnPoint
                {
                    intelId = intel.IntelId,
                    intelType = intel.IntelType,
                    position = intel.Position,
                    displayName = intel.DisplayName,
                    description = intel.Description,
                    isHidden = intel.IsHidden,
                    triggerEvents = Array.Empty<string>()
                };
            }

            var supplies = new SupplyPointData[spec.Supplies.Length];
            for (int i = 0; i < spec.Supplies.Length; i++)
            {
                SupplySpec supply = spec.Supplies[i];
                supplies[i] = new SupplyPointData
                {
                    supplyId = supply.SupplyId,
                    position = supply.Position,
                    providesFirstAid = supply.ProvidesFirstAid,
                    providesTools = supply.ProvidesTools,
                    cooldownDuration = supply.Cooldown
                };
            }

            var exits = new ExitPointData[spec.Exits.Length];
            for (int i = 0; i < spec.Exits.Length; i++)
            {
                ExitSpec exit = spec.Exits[i];
                exits[i] = new ExitPointData
                {
                    exitId = exit.ExitId,
                    position = exit.Position,
                    requiresAllIntel = exit.RequiresAllIntel,
                    isMainExit = exit.IsMainExit
                };
            }

            var ventEntranceList = new List<Vector3>();
            var ventExitList = new List<Vector3>();
            for (int i = 0; i < spec.Markers.Length; i++)
            {
                if (spec.Markers[i].Type != MarkerType.VentEntrance)
                    continue;

                ventEntranceList.Add(spec.Markers[i].Position);
                ventExitList.Add(spec.Markers[i].VentExitPosition);
            }

            data.intelPoints = intelPoints;
            data.supplyPoints = supplies;
            data.exitPoints = exits;
            data.ventEntrancePositions = ventEntranceList.ToArray();
            data.ventExitPositions = ventExitList.ToArray();
        }

        private static void ApplyLevelData(LevelSpec spec, LevelData data, EnemySpawnData enemyData, IntelSpawnData intelData)
        {
            data.levelName = spec.SceneName;
            data.levelDisplayName = spec.DisplayName;
            data.levelIndex = spec.LevelIndex;
            data.sceneName = spec.SceneName;

            data.standardTime = spec.StandardTimeSeconds;
            data.timeLimit = spec.TimeLimitSeconds;

            data.totalEnemyCount = enemyData.spawnPoints != null ? enemyData.spawnPoints.Length : 0;
            data.normalEnemyCount = CountEnemyType(enemyData.spawnPoints, EEnemySpawnType.Normal);
            data.reinforcedEnemyCount = CountEnemyType(enemyData.spawnPoints, EEnemySpawnType.Reinforced);
            data.heavyEnemyCount = CountEnemyType(enemyData.spawnPoints, EEnemySpawnType.Heavy);
            data.quipucamayocCount = CountEnemyType(enemyData.spawnPoints, EEnemySpawnType.Quipucamayoc);
            data.saqueosCount = CountEnemyType(enemyData.spawnPoints, EEnemySpawnType.Saqueos);

            data.qhipuFragmentCount = CountIntelType(intelData.intelPoints, EIntelType.QhipuFragment);
            data.terminalCount = CountIntelType(intelData.intelPoints, EIntelType.TerminalDocument);
            data.supplyPointCount = intelData.supplyPoints != null ? intelData.supplyPoints.Length : 0;

            data.hasVentSystem = spec.HasVentSystem;
            data.hasHangingPoints = spec.HasHangingPoints;
            data.hasBreakableWalls = spec.HasBreakableWalls;
            data.hasElectronicDoors = spec.HasElectronicDoors;
            data.hasSurveillanceCameras = spec.HasSurveillanceCameras;

            data.baseCreditReward = spec.BaseCreditReward;
            data.secondaryObjectiveBonus = spec.SecondaryBonus;
            data.zeroKillBonus = spec.ZeroKillBonus;
            data.noDamageBonus = spec.NoDamageBonus;
        }

        private static int CountEnemyType(EnemySpawnPoint[] points, EEnemySpawnType type)
        {
            if (points == null)
                return 0;

            int count = 0;
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].enemyType == type)
                    count++;
            }
            return count;
        }

        private static int CountIntelType(IntelSpawnPoint[] points, EIntelType type)
        {
            if (points == null)
                return 0;

            int count = 0;
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].intelType == type)
                    count++;
            }
            return count;
        }

        private static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            EnsureFolderPath(GetParentFolder(path));
            T created = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(created, path);
            return created;
        }

        private static GameObject GetOrCreateRoot(string name, ref bool changed)
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null)
                return existing;

            changed = true;
            return new GameObject(name);
        }

        private static Transform GetOrCreateChild(Transform parent, string name, Vector3 localPosition, ref bool changed)
        {
            Transform child = parent.Find(name);
            if (child != null)
                return child;

            GameObject go = new(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            changed = true;
            return go.transform;
        }

        private static Transform CreateFolder(Transform parent, string name)
        {
            GameObject go = new(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static T EnsureComponent<T>(GameObject go, ref bool changed) where T : Component
        {
            T existing = go.GetComponent<T>();
            if (existing != null)
                return existing;

            changed = true;
            return go.AddComponent<T>();
        }

        private static void EnsureChildComponent<T>(Transform parent, string childName, ref bool changed) where T : Component
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                GameObject childGo = new(childName);
                childGo.transform.SetParent(parent, false);
                child = childGo.transform;
                changed = true;
            }

            if (child.GetComponent<T>() == null)
            {
                child.gameObject.AddComponent<T>();
                changed = true;
            }
        }

        private static bool SetObjectReference(SerializedObject serialized, string propertyName, UnityEngine.Object value)
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

        private static bool SetVector3(SerializedObject serialized, string propertyName, Vector3 value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null || property.propertyType != SerializedPropertyType.Vector3)
                return false;

            if (property.vector3Value == value)
                return false;

            property.vector3Value = value;
            return true;
        }

        private static void WriteReport(RunReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Iteration5 Whitebox Rebuild Report");
            sb.AppendLine();
            sb.AppendLine("Date: 2026-04-01  ");
            sb.AppendLine("Executor: Codex");
            sb.AppendLine();
            sb.AppendLine("## Summary");
            sb.AppendLine();
            sb.AppendLine($"1. Target scenes: {report.SceneCount}");
            sb.AppendLine($"2. Updated scenes: {report.SceneUpdatedCount}");
            sb.AppendLine($"3. Missing scenes: {report.SceneMissingCount}");
            sb.AppendLine($"4. Legacy whitebox objects removed: {report.LegacyWhiteboxDeletedCount}");
            sb.AppendLine();
            sb.AppendLine("## Notes");
            sb.AppendLine();
            if (report.Notes.Count == 0)
            {
                sb.AppendLine("1. No notes.");
            }
            else
            {
                for (int i = 0; i < report.Notes.Count; i++)
                {
                    sb.AppendLine($"{i + 1}. {report.Notes[i]}");
                }
            }

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
            string absolutePath = Path.Combine(projectRoot, ReportPath.Replace('/', Path.DirectorySeparatorChar));
            string parent = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
                Directory.CreateDirectory(parent);

            File.WriteAllText(absolutePath, sb.ToString(), Encoding.UTF8);
            Debug.Log($"[Iteration5] Report written: {ReportPath}");
        }

        private static void EnsureFolderPath(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                return;

            string[] parts = folderPath.Split('/');
            if (parts.Length == 0)
                return;

            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);

                current = next;
            }
        }

        private static string GetParentFolder(string assetPath)
        {
            int slash = assetPath.LastIndexOf('/');
            if (slash <= 0)
                return string.Empty;
            return assetPath.Substring(0, slash);
        }

        private static LevelSpec[] BuildSpecs()
        {
            return new[]
            {
                BuildLevel01(),
                BuildLevel02(),
                BuildLevel03(),
                BuildLevel04(),
                BuildLevel05()
            };
        }

        private static LevelSpec BuildLevel01()
        {
            string sceneName = "Level01_Qhapaq_Passage";
            return new LevelSpec
            {
                LevelIndex = 0,
                SceneName = sceneName,
                ScenePath = $"Assets/Scenes/{sceneName}.unity",
                DisplayName = "L01 Golden Rain Warehouse",
                StandardTimeSeconds = 1500f,
                TimeLimitSeconds = 1800f,
                BaseCreditReward = 220,
                SecondaryBonus = 60,
                ZeroKillBonus = 160,
                NoDamageBonus = 180,
                HasVentSystem = true,
                HasHangingPoints = true,
                HasBreakableWalls = false,
                HasElectronicDoors = false,
                HasSurveillanceCameras = false,
                MaxConcurrentAlert = 2,
                CommunicationGroupSize = 3,
                PlayerSpawn = new Vector3(-34f, 1f, -18f),
                CameraPosition = new Vector3(0f, 55f, -42f),
                CameraEuler = new Vector3(38f, 0f, 0f),
                Geometry = new[]
                {
                    new BoxSpec("Floor_Main", GeometryLayer.Floor, new Vector3(0f, -0.5f, 10f), new Vector3(80f, 1f, 70f)),
                    new BoxSpec("Wall_North", GeometryLayer.Wall, new Vector3(0f, 2f, 44f), new Vector3(80f, 4f, 1f)),
                    new BoxSpec("Wall_South", GeometryLayer.Wall, new Vector3(0f, 2f, -24f), new Vector3(80f, 4f, 1f)),
                    new BoxSpec("Wall_West", GeometryLayer.Wall, new Vector3(-40f, 2f, 10f), new Vector3(1f, 4f, 70f)),
                    new BoxSpec("Wall_East", GeometryLayer.Wall, new Vector3(40f, 2f, 10f), new Vector3(1f, 4f, 70f)),
                    new BoxSpec("ZoneDivider_AB", GeometryLayer.Wall, new Vector3(-6f, 2f, -4f), new Vector3(1f, 4f, 30f)),
                    new BoxSpec("ZoneDivider_CD", GeometryLayer.Wall, new Vector3(6f, 2f, 6f), new Vector3(1f, 4f, 28f)),
                    new BoxSpec("ZoneDivider_E", GeometryLayer.Wall, new Vector3(0f, 2f, 18f), new Vector3(32f, 4f, 1f)),
                    new BoxSpec("Cover_A1", GeometryLayer.Cover, new Vector3(-24f, 1f, -10f), new Vector3(6f, 2f, 2f)),
                    new BoxSpec("Cover_A2", GeometryLayer.Cover, new Vector3(-16f, 1f, -3f), new Vector3(5f, 2f, 2f)),
                    new BoxSpec("Cover_B1", GeometryLayer.Cover, new Vector3(16f, 1f, -9f), new Vector3(7f, 2f, 2f)),
                    new BoxSpec("Cover_C1", GeometryLayer.Cover, new Vector3(-20f, 1f, 8f), new Vector3(6f, 2f, 2f)),
                    new BoxSpec("Cover_D1", GeometryLayer.Cover, new Vector3(14f, 1f, 8f), new Vector3(6f, 2f, 2f)),
                    new BoxSpec("Cover_E1", GeometryLayer.Cover, new Vector3(-4f, 1f, 25f), new Vector3(6f, 2f, 2f)),
                    new BoxSpec("Cover_E2", GeometryLayer.Cover, new Vector3(10f, 1f, 26f), new Vector3(8f, 2f, 2f)),
                    new BoxSpec("Landmark_StoneRoom", GeometryLayer.Landmark, new Vector3(0f, 1.5f, 31f), new Vector3(12f, 3f, 8f)),
                    new BoxSpec("Route_A_Main", GeometryLayer.Route, new Vector3(-25f, 0.1f, 4f), new Vector3(3f, 0.2f, 40f)),
                    new BoxSpec("Route_B_Main", GeometryLayer.Route, new Vector3(18f, 0.1f, 6f), new Vector3(3f, 0.2f, 36f)),
                    new BoxSpec("Route_C_Stealth", GeometryLayer.Route, new Vector3(-2f, 0.1f, 22f), new Vector3(3f, 0.2f, 18f))
                },
                Markers = new[]
                {
                    new MarkerSpec("Hang_01", MarkerType.HangingPoint, new Vector3(-2f, 4f, 20f), new Vector3(0.6f, 0.6f, 0.6f)),
                    new MarkerSpec("Hang_02", MarkerType.HangingPoint, new Vector3(6f, 4f, 22f), new Vector3(0.6f, 0.6f, 0.6f)),
                    new MarkerSpec("Hang_03", MarkerType.HangingPoint, new Vector3(14f, 4f, 24f), new Vector3(0.6f, 0.6f, 0.6f)),
                    new MarkerSpec("Vent_Entry_A", MarkerType.VentEntrance, new Vector3(-31f, 1f, 6f), new Vector3(1.4f, 1.2f, 1.4f))
                    {
                        VentExitPosition = new Vector3(24f, 1f, 17f)
                    }
                },
                EnemySpawns = new[]
                {
                    new EnemySpawnSpec($"{sceneName}_enemy_01", new Vector3(-24f, 0f, -10f), new Vector3(0f, 90f, 0f), EEnemySpawnType.Normal, true, "route_cargo_a", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_02", new Vector3(14f, 0f, -9f), new Vector3(0f, 180f, 0f), EEnemySpawnType.Normal, true, "route_cargo_b", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_03", new Vector3(-20f, 0f, 8f), new Vector3(0f, 0f, 0f), EEnemySpawnType.Normal, true, "route_storage_c", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_04", new Vector3(12f, 0f, 8f), new Vector3(0f, 180f, 0f), EEnemySpawnType.Normal, true, "route_storage_d", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_05", new Vector3(-2f, 0f, 22f), new Vector3(0f, 90f, 0f), EEnemySpawnType.Normal, true, "route_transit_e", 1),
                    new EnemySpawnSpec($"{sceneName}_enemy_06", new Vector3(10f, 0f, 24f), new Vector3(0f, 270f, 0f), EEnemySpawnType.Normal, true, "route_transit_e", 1)
                },
                IntelSpawns = new[]
                {
                    new IntelSpec($"{sceneName}_terminal_a", EIntelType.TerminalDocument, new Vector3(-30f, 0.6f, -12f), "Terminal A", "Cargo intake logs", false),
                    new IntelSpec($"{sceneName}_terminal_b", EIntelType.TerminalDocument, new Vector3(-18f, 0.6f, 9f), "Terminal B", "Storage routing table", false),
                    new IntelSpec($"{sceneName}_qhipu_01", EIntelType.QhipuFragment, new Vector3(0f, 0.6f, 30f), "Qhipu Fragment 01", "Stone room fragment", false),
                    new IntelSpec($"{sceneName}_qhipu_02", EIntelType.QhipuFragment, new Vector3(5f, 0.6f, 30f), "Qhipu Fragment 02", "Stone room fragment", false),
                    new IntelSpec($"{sceneName}_qhipu_03", EIntelType.QhipuFragment, new Vector3(10f, 0.6f, 30f), "Qhipu Fragment 03", "Stone room fragment", false)
                },
                Supplies = new[]
                {
                    new SupplySpec($"{sceneName}_toolbox_a", new Vector3(12f, 0.6f, -8f), false, true, 20f),
                    new SupplySpec($"{sceneName}_toolbox_b", new Vector3(16f, 0.6f, 10f), false, true, 20f),
                    new SupplySpec($"{sceneName}_ammo_a", new Vector3(0f, 0.6f, 21f), false, true, 18f),
                    new SupplySpec($"{sceneName}_ammo_b", new Vector3(30f, 0.6f, -2f), false, true, 18f),
                    new SupplySpec($"{sceneName}_medkit", new Vector3(31f, 0.6f, 0f), true, false, 25f)
                },
                Exits = new[]
                {
                    new ExitSpec($"{sceneName}_exit_main", new Vector3(32f, 1f, 30f), true, true)
                }
            };
        }

        private static LevelSpec BuildLevel02()
        {
            string sceneName = "Level02_Temple_Complex";
            return new LevelSpec
            {
                LevelIndex = 1,
                SceneName = sceneName,
                ScenePath = $"Assets/Scenes/{sceneName}.unity",
                DisplayName = "L02 Priest Eye Council Tower",
                StandardTimeSeconds = 1800f,
                TimeLimitSeconds = 2100f,
                BaseCreditReward = 260,
                SecondaryBonus = 70,
                ZeroKillBonus = 170,
                NoDamageBonus = 190,
                HasVentSystem = true,
                HasHangingPoints = true,
                HasBreakableWalls = false,
                HasElectronicDoors = true,
                HasSurveillanceCameras = true,
                MaxConcurrentAlert = 3,
                CommunicationGroupSize = 4,
                PlayerSpawn = new Vector3(-26f, 1f, -20f),
                CameraPosition = new Vector3(0f, 64f, -50f),
                CameraEuler = new Vector3(36f, 0f, 0f),
                Geometry = new[]
                {
                    new BoxSpec("Floor_L1", GeometryLayer.Floor, new Vector3(0f, -0.5f, 0f), new Vector3(64f, 1f, 56f)),
                    new BoxSpec("Floor_L2", GeometryLayer.Floor, new Vector3(0f, 4.5f, 0f), new Vector3(56f, 1f, 48f)),
                    new BoxSpec("Floor_L3", GeometryLayer.Floor, new Vector3(0f, 9.5f, 4f), new Vector3(50f, 1f, 40f)),
                    new BoxSpec("Floor_L4", GeometryLayer.Floor, new Vector3(0f, 14.5f, 8f), new Vector3(42f, 1f, 32f)),
                    new BoxSpec("Floor_Rooftop", GeometryLayer.Floor, new Vector3(0f, 19.5f, 12f), new Vector3(34f, 1f, 24f)),
                    new BoxSpec("Wall_North", GeometryLayer.Wall, new Vector3(0f, 3f, 28f), new Vector3(64f, 6f, 1f)),
                    new BoxSpec("Wall_South", GeometryLayer.Wall, new Vector3(0f, 3f, -28f), new Vector3(64f, 6f, 1f)),
                    new BoxSpec("Wall_West", GeometryLayer.Wall, new Vector3(-32f, 3f, 0f), new Vector3(1f, 6f, 56f)),
                    new BoxSpec("Wall_East", GeometryLayer.Wall, new Vector3(32f, 3f, 0f), new Vector3(1f, 6f, 56f)),
                    new BoxSpec("Ramp_L1_L2", GeometryLayer.Landmark, new Vector3(-20f, 2f, -8f), new Vector3(6f, 4f, 10f)),
                    new BoxSpec("Ramp_L2_L3", GeometryLayer.Landmark, new Vector3(-10f, 7f, -2f), new Vector3(6f, 4f, 10f)),
                    new BoxSpec("Ramp_L3_L4", GeometryLayer.Landmark, new Vector3(0f, 12f, 4f), new Vector3(6f, 4f, 10f)),
                    new BoxSpec("Ramp_L4_Roof", GeometryLayer.Landmark, new Vector3(10f, 17f, 10f), new Vector3(6f, 4f, 10f)),
                    new BoxSpec("Cover_L1_A", GeometryLayer.Cover, new Vector3(-14f, 1f, -16f), new Vector3(6f, 2f, 2f)),
                    new BoxSpec("Cover_L2_A", GeometryLayer.Cover, new Vector3(-8f, 6f, -2f), new Vector3(5f, 2f, 2f)),
                    new BoxSpec("Cover_L3_A", GeometryLayer.Cover, new Vector3(4f, 11f, 6f), new Vector3(5f, 2f, 2f)),
                    new BoxSpec("Cover_L4_A", GeometryLayer.Cover, new Vector3(6f, 16f, 10f), new Vector3(5f, 2f, 2f)),
                    new BoxSpec("Route_A_TerminalControl", GeometryLayer.Route, new Vector3(-2f, 0.1f, 4f), new Vector3(3f, 0.2f, 46f)),
                    new BoxSpec("Route_B_VentStealth", GeometryLayer.Route, new Vector3(16f, 0.1f, 6f), new Vector3(3f, 0.2f, 42f)),
                    new BoxSpec("Route_C_DirectBreakthrough", GeometryLayer.Route, new Vector3(-16f, 0.1f, 8f), new Vector3(3f, 0.2f, 42f))
                },
                Markers = new[]
                {
                    new MarkerSpec("Hang_01", MarkerType.HangingPoint, new Vector3(-24f, 3f, -20f), new Vector3(0.6f, 0.6f, 0.6f)),
                    new MarkerSpec("Hang_02", MarkerType.HangingPoint, new Vector3(-24f, 8f, -12f), new Vector3(0.6f, 0.6f, 0.6f)),
                    new MarkerSpec("Hang_03", MarkerType.HangingPoint, new Vector3(-24f, 13f, -4f), new Vector3(0.6f, 0.6f, 0.6f)),
                    new MarkerSpec("Hang_04", MarkerType.HangingPoint, new Vector3(-24f, 18f, 4f), new Vector3(0.6f, 0.6f, 0.6f)),
                    new MarkerSpec("Vent_Entry_Main", MarkerType.VentEntrance, new Vector3(12f, 5f, -6f), new Vector3(1.4f, 1.2f, 1.4f))
                    {
                        VentExitPosition = new Vector3(8f, 14f, 10f)
                    },
                    new MarkerSpec("Door_L3_Control", MarkerType.ElectronicDoor, new Vector3(0f, 10f, 8f), new Vector3(2f, 3f, 0.8f)),
                    new MarkerSpec("Door_L4_Archive", MarkerType.ElectronicDoor, new Vector3(4f, 15f, 12f), new Vector3(2f, 3f, 0.8f)),
                    new MarkerSpec("Cam_L1_A", MarkerType.SurveillanceCamera, new Vector3(-8f, 1.8f, -14f), new Vector3(0.8f, 0.8f, 0.8f)),
                    new MarkerSpec("Cam_L1_B", MarkerType.SurveillanceCamera, new Vector3(8f, 1.8f, -12f), new Vector3(0.8f, 0.8f, 0.8f)),
                    new MarkerSpec("Cam_L2", MarkerType.SurveillanceCamera, new Vector3(10f, 6.8f, -2f), new Vector3(0.8f, 0.8f, 0.8f)),
                    new MarkerSpec("Cam_L3_A", MarkerType.SurveillanceCamera, new Vector3(0f, 11.8f, 4f), new Vector3(0.8f, 0.8f, 0.8f)),
                    new MarkerSpec("Cam_L3_B", MarkerType.SurveillanceCamera, new Vector3(6f, 11.8f, 6f), new Vector3(0.8f, 0.8f, 0.8f))
                },
                EnemySpawns = new[]
                {
                    new EnemySpawnSpec($"{sceneName}_enemy_01", new Vector3(-20f, 0f, -16f), new Vector3(0f, 90f, 0f), EEnemySpawnType.Normal, true, "route_l1_hall", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_02", new Vector3(6f, 0f, -18f), new Vector3(0f, 180f, 0f), EEnemySpawnType.Normal, true, "route_l1_corridor", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_03", new Vector3(-12f, 5f, -2f), new Vector3(0f, 0f, 0f), EEnemySpawnType.Normal, true, "route_l2_office", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_04", new Vector3(12f, 5f, -2f), new Vector3(0f, 180f, 0f), EEnemySpawnType.Normal, true, "route_l2_office", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_05", new Vector3(-8f, 10f, 8f), new Vector3(0f, 90f, 0f), EEnemySpawnType.Reinforced, true, "route_l3_meeting", 1),
                    new EnemySpawnSpec($"{sceneName}_enemy_06", new Vector3(14f, 10f, 8f), new Vector3(0f, 270f, 0f), EEnemySpawnType.Reinforced, true, "route_l3_service", 1),
                    new EnemySpawnSpec($"{sceneName}_enemy_07", new Vector3(0f, 10f, 12f), new Vector3(0f, 180f, 0f), EEnemySpawnType.Quipucamayoc, true, "route_l3_priest", 2)
                },
                IntelSpawns = new[]
                {
                    new IntelSpec($"{sceneName}_terminal_a", EIntelType.TerminalDocument, new Vector3(-18f, 0.6f, -18f), "Terminal A", "Tower entrance brief", false),
                    new IntelSpec($"{sceneName}_terminal_b", EIntelType.TerminalDocument, new Vector3(-10f, 5.6f, -1f), "Terminal B", "L2 office roster", false),
                    new IntelSpec($"{sceneName}_terminal_c", EIntelType.TerminalDocument, new Vector3(2f, 10.6f, 8f), "Terminal C", "Lighting control panel", false),
                    new IntelSpec($"{sceneName}_terminal_d", EIntelType.TerminalDocument, new Vector3(4f, 15.6f, 12f), "Terminal D", "Archive core terminal", false),
                    new IntelSpec($"{sceneName}_qhipu_01", EIntelType.QhipuFragment, new Vector3(-4f, 15.6f, 10f), "Qhipu Fragment 01", "Archive hallucination fragment", false),
                    new IntelSpec($"{sceneName}_qhipu_02", EIntelType.QhipuFragment, new Vector3(0f, 15.6f, 10f), "Qhipu Fragment 02", "Archive hallucination fragment", false),
                    new IntelSpec($"{sceneName}_qhipu_03", EIntelType.QhipuFragment, new Vector3(4f, 15.6f, 10f), "Qhipu Fragment 03", "Archive hallucination fragment", false)
                },
                Supplies = new[]
                {
                    new SupplySpec($"{sceneName}_toolbox_a", new Vector3(-20f, 5.6f, -4f), false, true, 20f),
                    new SupplySpec($"{sceneName}_toolbox_b", new Vector3(-12f, 15.6f, 10f), false, true, 20f),
                    new SupplySpec($"{sceneName}_ammo_a", new Vector3(-8f, 5.6f, 0f), false, true, 18f),
                    new SupplySpec($"{sceneName}_ammo_b", new Vector3(10f, 10.6f, 8f), false, true, 18f),
                    new SupplySpec($"{sceneName}_ammo_c", new Vector3(8f, 15.6f, 12f), false, true, 18f),
                    new SupplySpec($"{sceneName}_medkit", new Vector3(16f, 5.6f, -2f), true, false, 25f)
                },
                Exits = new[]
                {
                    new ExitSpec($"{sceneName}_exit_roof", new Vector3(16f, 20.5f, 16f), true, true)
                }
            };
        }

        private static LevelSpec BuildLevel03()
        {
            string sceneName = "Level03_Underground_Labs";
            return new LevelSpec
            {
                LevelIndex = 2,
                SceneName = sceneName,
                ScenePath = $"Assets/Scenes/{sceneName}.unity",
                DisplayName = "L03 Leech Awakening Lab",
                StandardTimeSeconds = 2100f,
                TimeLimitSeconds = 2400f,
                BaseCreditReward = 300,
                SecondaryBonus = 75,
                ZeroKillBonus = 180,
                NoDamageBonus = 210,
                HasVentSystem = true,
                HasHangingPoints = true,
                HasBreakableWalls = false,
                HasElectronicDoors = true,
                HasSurveillanceCameras = true,
                MaxConcurrentAlert = 3,
                CommunicationGroupSize = 4,
                PlayerSpawn = new Vector3(-28f, 1f, -18f),
                CameraPosition = new Vector3(0f, 68f, -52f),
                CameraEuler = new Vector3(38f, 0f, 0f),
                Geometry = new[]
                {
                    new BoxSpec("Floor_Surface", GeometryLayer.Floor, new Vector3(0f, -0.5f, 0f), new Vector3(72f, 1f, 54f)),
                    new BoxSpec("Floor_Underground", GeometryLayer.Floor, new Vector3(0f, -8.5f, 12f), new Vector3(48f, 1f, 36f)),
                    new BoxSpec("Wall_North", GeometryLayer.Wall, new Vector3(0f, 2f, 27f), new Vector3(72f, 4f, 1f)),
                    new BoxSpec("Wall_South", GeometryLayer.Wall, new Vector3(0f, 2f, -27f), new Vector3(72f, 4f, 1f)),
                    new BoxSpec("Wall_West", GeometryLayer.Wall, new Vector3(-36f, 2f, 0f), new Vector3(1f, 4f, 54f)),
                    new BoxSpec("Wall_East", GeometryLayer.Wall, new Vector3(36f, 2f, 0f), new Vector3(1f, 4f, 54f)),
                    new BoxSpec("Divider_A_B", GeometryLayer.Wall, new Vector3(-4f, 2f, 2f), new Vector3(1f, 4f, 30f)),
                    new BoxSpec("Ramp_SurfaceToUnderground", GeometryLayer.Landmark, new Vector3(18f, -4f, 12f), new Vector3(8f, 8f, 16f)),
                    new BoxSpec("Cover_Surface_A", GeometryLayer.Cover, new Vector3(-18f, 1f, -8f), new Vector3(6f, 2f, 2f)),
                    new BoxSpec("Cover_Surface_B", GeometryLayer.Cover, new Vector3(10f, 1f, 2f), new Vector3(6f, 2f, 2f)),
                    new BoxSpec("Cover_Lab_B", GeometryLayer.Cover, new Vector3(18f, 1f, 10f), new Vector3(5f, 2f, 2f)),
                    new BoxSpec("Cover_Underground_A", GeometryLayer.Cover, new Vector3(-10f, -7f, 16f), new Vector3(5f, 2f, 2f)),
                    new BoxSpec("Cover_Underground_B", GeometryLayer.Cover, new Vector3(6f, -7f, 16f), new Vector3(5f, 2f, 2f)),
                    new BoxSpec("Route_A_EmpCorridor", GeometryLayer.Route, new Vector3(-12f, 0.1f, 4f), new Vector3(3f, 0.2f, 42f)),
                    new BoxSpec("Route_B_SaqueosBypass", GeometryLayer.Route, new Vector3(0f, 0.1f, 8f), new Vector3(3f, 0.2f, 38f)),
                    new BoxSpec("Route_C_OuterWall", GeometryLayer.Route, new Vector3(20f, 0.1f, 4f), new Vector3(3f, 0.2f, 44f))
                },
                Markers = new[]
                {
                    new MarkerSpec("Hang_01", MarkerType.HangingPoint, new Vector3(-28f, 4f, -10f), new Vector3(0.6f, 0.6f, 0.6f)),
                    new MarkerSpec("Hang_02", MarkerType.HangingPoint, new Vector3(-18f, 6f, -2f), new Vector3(0.6f, 0.6f, 0.6f)),
                    new MarkerSpec("Hang_03", MarkerType.HangingPoint, new Vector3(-8f, 8f, 4f), new Vector3(0.6f, 0.6f, 0.6f)),
                    new MarkerSpec("Vent_Entry_Main", MarkerType.VentEntrance, new Vector3(-10f, 1f, -4f), new Vector3(1.4f, 1.2f, 1.4f))
                    {
                        VentExitPosition = new Vector3(12f, -7f, 14f)
                    },
                    new MarkerSpec("Door_EMP_A", MarkerType.ElectronicDoor, new Vector3(-4f, 1.5f, 4f), new Vector3(2f, 3f, 0.8f)),
                    new MarkerSpec("Door_EMP_B", MarkerType.ElectronicDoor, new Vector3(8f, 1.5f, 12f), new Vector3(2f, 3f, 0.8f)),
                    new MarkerSpec("Cam_A", MarkerType.SurveillanceCamera, new Vector3(12f, 1.8f, 0f), new Vector3(0.8f, 0.8f, 0.8f)),
                    new MarkerSpec("Cam_B", MarkerType.SurveillanceCamera, new Vector3(16f, 1.8f, 8f), new Vector3(0.8f, 0.8f, 0.8f)),
                    new MarkerSpec("Cam_C", MarkerType.SurveillanceCamera, new Vector3(8f, 1.8f, 12f), new Vector3(0.8f, 0.8f, 0.8f))
                },
                EnemySpawns = new[]
                {
                    new EnemySpawnSpec($"{sceneName}_enemy_01", new Vector3(-24f, 0f, -14f), new Vector3(0f, 90f, 0f), EEnemySpawnType.Normal, true, "route_entry_outer", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_02", new Vector3(-12f, 0f, 2f), new Vector3(0f, 180f, 0f), EEnemySpawnType.Normal, true, "route_gene_a", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_03", new Vector3(-2f, 0f, 2f), new Vector3(0f, 0f, 0f), EEnemySpawnType.Normal, true, "route_gene_a", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_04", new Vector3(14f, 0f, 10f), new Vector3(0f, 270f, 0f), EEnemySpawnType.Reinforced, true, "route_research_b", 1),
                    new EnemySpawnSpec($"{sceneName}_enemy_05", new Vector3(20f, 0f, 10f), new Vector3(0f, 180f, 0f), EEnemySpawnType.Reinforced, true, "route_research_b", 1),
                    new EnemySpawnSpec($"{sceneName}_enemy_06", new Vector3(-8f, -8f, 18f), new Vector3(0f, 90f, 0f), EEnemySpawnType.Saqueos, true, "route_saqueos_den", 1),
                    new EnemySpawnSpec($"{sceneName}_enemy_07", new Vector3(0f, -8f, 20f), new Vector3(0f, 0f, 0f), EEnemySpawnType.Saqueos, true, "route_saqueos_den", 1),
                    new EnemySpawnSpec($"{sceneName}_enemy_08", new Vector3(8f, -8f, 16f), new Vector3(0f, 270f, 0f), EEnemySpawnType.Saqueos, true, "route_saqueos_terminal", 2)
                },
                IntelSpawns = new[]
                {
                    new IntelSpec($"{sceneName}_terminal_a", EIntelType.TerminalDocument, new Vector3(-24f, 0.6f, -16f), "Terminal A", "Perimeter logs", false),
                    new IntelSpec($"{sceneName}_terminal_b", EIntelType.TerminalDocument, new Vector3(-10f, 0.6f, 2f), "Terminal B", "Gene cultivation logs", false),
                    new IntelSpec($"{sceneName}_terminal_c", EIntelType.TerminalDocument, new Vector3(16f, 0.6f, 10f), "Terminal C", "Lighting and elevator control", false),
                    new IntelSpec($"{sceneName}_terminal_d", EIntelType.TerminalDocument, new Vector3(4f, -7.4f, 16f), "Terminal D", "Core experiment archive", false),
                    new IntelSpec($"{sceneName}_qhipu_01", EIntelType.QhipuFragment, new Vector3(-2f, -7.4f, 20f), "Qhipu Fragment 01", "Underground hallucination shard", false),
                    new IntelSpec($"{sceneName}_qhipu_02", EIntelType.QhipuFragment, new Vector3(2f, -7.4f, 20f), "Qhipu Fragment 02", "Underground hallucination shard", false),
                    new IntelSpec($"{sceneName}_qhipu_03", EIntelType.QhipuFragment, new Vector3(6f, -7.4f, 20f), "Qhipu Fragment 03", "Underground hallucination shard", false)
                },
                Supplies = new[]
                {
                    new SupplySpec($"{sceneName}_toolbox_a", new Vector3(14f, 0.6f, 10f), false, true, 20f),
                    new SupplySpec($"{sceneName}_toolbox_b", new Vector3(8f, -7.4f, 16f), false, true, 20f),
                    new SupplySpec($"{sceneName}_ammo_a", new Vector3(-8f, 0.6f, 2f), false, true, 18f),
                    new SupplySpec($"{sceneName}_ammo_b", new Vector3(20f, 0.6f, 10f), false, true, 18f),
                    new SupplySpec($"{sceneName}_ammo_c", new Vector3(12f, -7.4f, 14f), false, true, 18f),
                    new SupplySpec($"{sceneName}_medkit", new Vector3(4f, -7.4f, 12f), true, false, 25f)
                },
                Exits = new[]
                {
                    new ExitSpec($"{sceneName}_exit_underground", new Vector3(22f, -7f, 20f), true, true)
                }
            };
        }

        private static LevelSpec BuildLevel04()
        {
            string sceneName = "Level04_Qhipu_Core";
            return new LevelSpec
            {
                LevelIndex = 3,
                SceneName = sceneName,
                ScenePath = $"Assets/Scenes/{sceneName}.unity",
                DisplayName = "L04 Ancestor Knot Core",
                StandardTimeSeconds = 2400f,
                TimeLimitSeconds = 2700f,
                BaseCreditReward = 340,
                SecondaryBonus = 80,
                ZeroKillBonus = 190,
                NoDamageBonus = 220,
                HasVentSystem = true,
                HasHangingPoints = true,
                HasBreakableWalls = true,
                HasElectronicDoors = true,
                HasSurveillanceCameras = false,
                MaxConcurrentAlert = 4,
                CommunicationGroupSize = 4,
                PlayerSpawn = new Vector3(38f, 1f, -22f),
                CameraPosition = new Vector3(0f, 76f, -58f),
                CameraEuler = new Vector3(40f, 0f, 0f),
                Geometry = new[]
                {
                    new BoxSpec("Floor_Base", GeometryLayer.Floor, new Vector3(0f, -0.5f, 0f), new Vector3(104f, 1f, 70f)),
                    new BoxSpec("Floor_MidTemple", GeometryLayer.Floor, new Vector3(0f, 4.5f, 8f), new Vector3(72f, 1f, 44f)),
                    new BoxSpec("Floor_CoreTemple", GeometryLayer.Floor, new Vector3(0f, 11.5f, 16f), new Vector3(52f, 1f, 30f)),
                    new BoxSpec("Floor_HighRoute", GeometryLayer.Floor, new Vector3(0f, 18.5f, 16f), new Vector3(44f, 1f, 6f)),
                    new BoxSpec("Wall_North", GeometryLayer.Wall, new Vector3(0f, 3f, 35f), new Vector3(104f, 6f, 1f)),
                    new BoxSpec("Wall_South", GeometryLayer.Wall, new Vector3(0f, 3f, -35f), new Vector3(104f, 6f, 1f)),
                    new BoxSpec("Wall_West", GeometryLayer.Wall, new Vector3(-52f, 3f, 0f), new Vector3(1f, 6f, 70f)),
                    new BoxSpec("Wall_East", GeometryLayer.Wall, new Vector3(52f, 3f, 0f), new Vector3(1f, 6f, 70f)),
                    new BoxSpec("Temple_Gate", GeometryLayer.Landmark, new Vector3(36f, 2.5f, -10f), new Vector3(8f, 5f, 2f)),
                    new BoxSpec("DataCenter_BlockA", GeometryLayer.Cover, new Vector3(12f, 5.5f, 6f), new Vector3(8f, 3f, 2f)),
                    new BoxSpec("DataCenter_BlockB", GeometryLayer.Cover, new Vector3(-8f, 5.5f, 8f), new Vector3(8f, 3f, 2f)),
                    new BoxSpec("Core_BlockA", GeometryLayer.Cover, new Vector3(6f, 12.5f, 16f), new Vector3(8f, 3f, 2f)),
                    new BoxSpec("Core_BlockB", GeometryLayer.Cover, new Vector3(-10f, 12.5f, 16f), new Vector3(8f, 3f, 2f)),
                    new BoxSpec("Route_A_Vent", GeometryLayer.Route, new Vector3(-20f, 0.1f, 4f), new Vector3(3f, 0.2f, 52f)),
                    new BoxSpec("Route_B_WallBreak", GeometryLayer.Route, new Vector3(0f, 0.1f, 8f), new Vector3(3f, 0.2f, 48f)),
                    new BoxSpec("Route_C_PriestBlindZone", GeometryLayer.Route, new Vector3(20f, 0.1f, 10f), new Vector3(3f, 0.2f, 50f))
                },
                Markers = new[]
                {
                    new MarkerSpec("Hang_01", MarkerType.HangingPoint, new Vector3(-20f, 16f, 10f), new Vector3(0.6f, 0.6f, 0.6f)),
                    new MarkerSpec("Hang_02", MarkerType.HangingPoint, new Vector3(-8f, 18f, 14f), new Vector3(0.6f, 0.6f, 0.6f)),
                    new MarkerSpec("Hang_03", MarkerType.HangingPoint, new Vector3(4f, 18f, 16f), new Vector3(0.6f, 0.6f, 0.6f)),
                    new MarkerSpec("Hang_04", MarkerType.HangingPoint, new Vector3(16f, 16f, 18f), new Vector3(0.6f, 0.6f, 0.6f)),
                    new MarkerSpec("Vent_Entry_Main", MarkerType.VentEntrance, new Vector3(-38f, 1f, -18f), new Vector3(1.4f, 1.2f, 1.4f))
                    {
                        VentExitPosition = new Vector3(20f, 12f, 16f)
                    },
                    new MarkerSpec("BreakWall_A", MarkerType.BreakableWall, new Vector3(-8f, 5f, 2f), new Vector3(2f, 3f, 0.8f)),
                    new MarkerSpec("BreakWall_B", MarkerType.BreakableWall, new Vector3(8f, 5f, 4f), new Vector3(2f, 3f, 0.8f)),
                    new MarkerSpec("BreakWall_C", MarkerType.BreakableWall, new Vector3(14f, 12f, 10f), new Vector3(2f, 3f, 0.8f)),
                    new MarkerSpec("Door_CoreControl", MarkerType.ElectronicDoor, new Vector3(0f, 5f, 6f), new Vector3(2f, 3f, 0.8f))
                },
                EnemySpawns = new[]
                {
                    new EnemySpawnSpec($"{sceneName}_enemy_01", new Vector3(34f, 0f, -12f), new Vector3(0f, 90f, 0f), EEnemySpawnType.Normal, true, "route_gate", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_02", new Vector3(28f, 0f, -10f), new Vector3(0f, 180f, 0f), EEnemySpawnType.Normal, true, "route_gate", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_03", new Vector3(22f, 0f, -8f), new Vector3(0f, 270f, 0f), EEnemySpawnType.Normal, true, "route_gate", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_04", new Vector3(10f, 5f, 4f), new Vector3(0f, 180f, 0f), EEnemySpawnType.Normal, true, "route_outer_corridor", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_05", new Vector3(-6f, 5f, 4f), new Vector3(0f, 0f, 0f), EEnemySpawnType.Normal, true, "route_outer_corridor", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_06", new Vector3(6f, 12f, 16f), new Vector3(0f, 90f, 0f), EEnemySpawnType.Normal, true, "route_core_patrol", 1),
                    new EnemySpawnSpec($"{sceneName}_enemy_07", new Vector3(-8f, 12f, 16f), new Vector3(0f, 270f, 0f), EEnemySpawnType.Normal, true, "route_core_patrol", 1),
                    new EnemySpawnSpec($"{sceneName}_enemy_08", new Vector3(12f, 5f, 8f), new Vector3(0f, 180f, 0f), EEnemySpawnType.Heavy, true, "route_data_center", 2),
                    new EnemySpawnSpec($"{sceneName}_enemy_09", new Vector3(-12f, 5f, 8f), new Vector3(0f, 0f, 0f), EEnemySpawnType.Heavy, true, "route_data_center", 2),
                    new EnemySpawnSpec($"{sceneName}_enemy_10", new Vector3(0f, 5f, 10f), new Vector3(0f, 90f, 0f), EEnemySpawnType.Quipucamayoc, true, "route_priest_center", 2),
                    new EnemySpawnSpec($"{sceneName}_enemy_11", new Vector3(0f, 12f, 18f), new Vector3(0f, 270f, 0f), EEnemySpawnType.Quipucamayoc, true, "route_priest_core", 2)
                },
                IntelSpawns = new[]
                {
                    new IntelSpec($"{sceneName}_terminal_a", EIntelType.TerminalDocument, new Vector3(32f, 0.6f, -12f), "Terminal A", "Gate security list", false),
                    new IntelSpec($"{sceneName}_terminal_b", EIntelType.TerminalDocument, new Vector3(12f, 5.6f, 4f), "Terminal B", "Priest roster", false),
                    new IntelSpec($"{sceneName}_terminal_c", EIntelType.TerminalDocument, new Vector3(0f, 5.6f, 10f), "Terminal C", "Qhipu protocol cache", false),
                    new IntelSpec($"{sceneName}_terminal_d", EIntelType.TerminalDocument, new Vector3(0f, 12.6f, 16f), "Terminal D", "Core terminal", false),
                    new IntelSpec($"{sceneName}_qhipu_01", EIntelType.QhipuFragment, new Vector3(-4f, 12.6f, 18f), "Qhipu Fragment 01", "Core vision shard", false),
                    new IntelSpec($"{sceneName}_qhipu_02", EIntelType.QhipuFragment, new Vector3(0f, 12.6f, 18f), "Qhipu Fragment 02", "Core vision shard", false),
                    new IntelSpec($"{sceneName}_qhipu_03", EIntelType.QhipuFragment, new Vector3(4f, 12.6f, 18f), "Qhipu Fragment 03", "Core vision shard", false)
                },
                Supplies = new[]
                {
                    new SupplySpec($"{sceneName}_toolbox_a", new Vector3(28f, 0.6f, -10f), false, true, 20f),
                    new SupplySpec($"{sceneName}_toolbox_b", new Vector3(12f, 5.6f, 8f), false, true, 20f),
                    new SupplySpec($"{sceneName}_ammo_a", new Vector3(18f, 5.6f, 4f), false, true, 18f),
                    new SupplySpec($"{sceneName}_ammo_b", new Vector3(-6f, 5.6f, 4f), false, true, 18f),
                    new SupplySpec($"{sceneName}_ammo_c", new Vector3(-12f, 16.6f, 16f), false, true, 18f),
                    new SupplySpec($"{sceneName}_medkit", new Vector3(34f, 0.6f, -6f), true, false, 25f)
                },
                Exits = new[]
                {
                    new ExitSpec($"{sceneName}_exit_main", new Vector3(44f, 1f, -24f), true, true)
                }
            };
        }

        private static LevelSpec BuildLevel05()
        {
            string sceneName = "Level05_General_Taki_Villa";
            return new LevelSpec
            {
                LevelIndex = 4,
                SceneName = sceneName,
                ScenePath = $"Assets/Scenes/{sceneName}.unity",
                DisplayName = "L05 Sunfall Inti Temple",
                StandardTimeSeconds = 2700f,
                TimeLimitSeconds = 3000f,
                BaseCreditReward = 400,
                SecondaryBonus = 100,
                ZeroKillBonus = 220,
                NoDamageBonus = 260,
                HasVentSystem = true,
                HasHangingPoints = true,
                HasBreakableWalls = false,
                HasElectronicDoors = true,
                HasSurveillanceCameras = false,
                MaxConcurrentAlert = 4,
                CommunicationGroupSize = 5,
                PlayerSpawn = new Vector3(-48f, 1f, -30f),
                CameraPosition = new Vector3(0f, 88f, -70f),
                CameraEuler = new Vector3(42f, 0f, 0f),
                Geometry = new[]
                {
                    new BoxSpec("Floor_Tier1", GeometryLayer.Floor, new Vector3(0f, -0.5f, 0f), new Vector3(124f, 1f, 84f)),
                    new BoxSpec("Floor_Tier2", GeometryLayer.Floor, new Vector3(0f, 7.5f, 10f), new Vector3(88f, 1f, 58f)),
                    new BoxSpec("Floor_Tier3", GeometryLayer.Floor, new Vector3(0f, 15.5f, 20f), new Vector3(58f, 1f, 38f)),
                    new BoxSpec("Floor_Tier4_Core", GeometryLayer.Floor, new Vector3(0f, 25.5f, 28f), new Vector3(38f, 1f, 26f)),
                    new BoxSpec("Wall_North", GeometryLayer.Wall, new Vector3(0f, 3f, 42f), new Vector3(124f, 6f, 1f)),
                    new BoxSpec("Wall_South", GeometryLayer.Wall, new Vector3(0f, 3f, -42f), new Vector3(124f, 6f, 1f)),
                    new BoxSpec("Wall_West", GeometryLayer.Wall, new Vector3(-62f, 3f, 0f), new Vector3(1f, 6f, 84f)),
                    new BoxSpec("Wall_East", GeometryLayer.Wall, new Vector3(62f, 3f, 0f), new Vector3(1f, 6f, 84f)),
                    new BoxSpec("Ramp_T1_T2", GeometryLayer.Landmark, new Vector3(-24f, 3.5f, -6f), new Vector3(10f, 8f, 14f)),
                    new BoxSpec("Ramp_T2_T3", GeometryLayer.Landmark, new Vector3(-8f, 11.5f, 8f), new Vector3(10f, 8f, 14f)),
                    new BoxSpec("Ramp_T3_T4", GeometryLayer.Landmark, new Vector3(8f, 20.5f, 20f), new Vector3(10f, 10f, 12f)),
                    new BoxSpec("MirrorArray_A", GeometryLayer.Cover, new Vector3(-12f, 17f, 20f), new Vector3(6f, 4f, 2f)),
                    new BoxSpec("MirrorArray_B", GeometryLayer.Cover, new Vector3(0f, 17f, 22f), new Vector3(6f, 4f, 2f)),
                    new BoxSpec("MirrorArray_C", GeometryLayer.Cover, new Vector3(12f, 17f, 24f), new Vector3(6f, 4f, 2f)),
                    new BoxSpec("Core_ThroneBlock", GeometryLayer.Landmark, new Vector3(0f, 27f, 30f), new Vector3(8f, 3f, 4f)),
                    new BoxSpec("Route_A_Ghost", GeometryLayer.Route, new Vector3(-18f, 0.1f, 6f), new Vector3(3f, 0.2f, 58f)),
                    new BoxSpec("Route_B_Combat", GeometryLayer.Route, new Vector3(0f, 0.1f, 8f), new Vector3(3f, 0.2f, 60f)),
                    new BoxSpec("Route_C_Intel", GeometryLayer.Route, new Vector3(18f, 0.1f, 10f), new Vector3(3f, 0.2f, 56f))
                },
                Markers = new[]
                {
                    new MarkerSpec("Hang_01", MarkerType.HangingPoint, new Vector3(-18f, 18f, 18f), new Vector3(0.6f, 0.6f, 0.6f)),
                    new MarkerSpec("Hang_02", MarkerType.HangingPoint, new Vector3(-8f, 20f, 20f), new Vector3(0.6f, 0.6f, 0.6f)),
                    new MarkerSpec("Hang_03", MarkerType.HangingPoint, new Vector3(2f, 20f, 22f), new Vector3(0.6f, 0.6f, 0.6f)),
                    new MarkerSpec("Hang_04", MarkerType.HangingPoint, new Vector3(12f, 20f, 24f), new Vector3(0.6f, 0.6f, 0.6f)),
                    new MarkerSpec("Vent_Entry_Main", MarkerType.VentEntrance, new Vector3(-46f, 1f, -20f), new Vector3(1.4f, 1.2f, 1.4f))
                    {
                        VentExitPosition = new Vector3(-6f, 8f, 8f)
                    },
                    new MarkerSpec("Door_Tier2", MarkerType.ElectronicDoor, new Vector3(0f, 8f, 12f), new Vector3(2f, 3f, 0.8f)),
                    new MarkerSpec("Door_Tier3", MarkerType.ElectronicDoor, new Vector3(4f, 16f, 24f), new Vector3(2f, 3f, 0.8f)),
                    new MarkerSpec("Door_Core", MarkerType.ElectronicDoor, new Vector3(0f, 26f, 30f), new Vector3(2f, 3f, 0.8f))
                },
                EnemySpawns = new[]
                {
                    new EnemySpawnSpec($"{sceneName}_enemy_01", new Vector3(-40f, 0f, -24f), new Vector3(0f, 90f, 0f), EEnemySpawnType.Normal, true, "route_outer_1", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_02", new Vector3(-28f, 0f, -20f), new Vector3(0f, 180f, 0f), EEnemySpawnType.Normal, true, "route_outer_1", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_03", new Vector3(-16f, 0f, -18f), new Vector3(0f, 270f, 0f), EEnemySpawnType.Normal, true, "route_outer_2", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_04", new Vector3(-4f, 0f, -16f), new Vector3(0f, 0f, 0f), EEnemySpawnType.Normal, true, "route_outer_2", 0),
                    new EnemySpawnSpec($"{sceneName}_enemy_05", new Vector3(-12f, 8f, 8f), new Vector3(0f, 90f, 0f), EEnemySpawnType.Reinforced, true, "route_mid_1", 1),
                    new EnemySpawnSpec($"{sceneName}_enemy_06", new Vector3(0f, 8f, 8f), new Vector3(0f, 180f, 0f), EEnemySpawnType.Reinforced, true, "route_mid_1", 1),
                    new EnemySpawnSpec($"{sceneName}_enemy_07", new Vector3(12f, 8f, 10f), new Vector3(0f, 270f, 0f), EEnemySpawnType.Reinforced, true, "route_mid_2", 1),
                    new EnemySpawnSpec($"{sceneName}_enemy_08", new Vector3(18f, 16f, 20f), new Vector3(0f, 180f, 0f), EEnemySpawnType.Reinforced, true, "route_solar_core", 1),
                    new EnemySpawnSpec($"{sceneName}_enemy_09", new Vector3(8f, 8f, 12f), new Vector3(0f, 90f, 0f), EEnemySpawnType.Quipucamayoc, true, "route_mid_priest", 2),
                    new EnemySpawnSpec($"{sceneName}_enemy_10", new Vector3(4f, 26f, 30f), new Vector3(0f, 180f, 0f), EEnemySpawnType.Quipucamayoc, true, "route_core_priest", 2),
                    new EnemySpawnSpec($"{sceneName}_enemy_11", new Vector3(0f, 16f, 20f), new Vector3(0f, 180f, 0f), EEnemySpawnType.Heavy, true, "route_taki_center", 3)
                },
                IntelSpawns = new[]
                {
                    new IntelSpec($"{sceneName}_terminal_a", EIntelType.TerminalDocument, new Vector3(-24f, 0.6f, -20f), "Terminal A", "Outer checkpoint records", false),
                    new IntelSpec($"{sceneName}_terminal_b", EIntelType.TerminalDocument, new Vector3(-4f, 8.6f, 8f), "Terminal B", "Mid tier light control", false),
                    new IntelSpec($"{sceneName}_terminal_c", EIntelType.TerminalDocument, new Vector3(10f, 8.6f, 12f), "Terminal C", "Priest comm log", false),
                    new IntelSpec($"{sceneName}_terminal_d", EIntelType.TerminalDocument, new Vector3(16f, 16.6f, 22f), "Terminal D", "Solar array control", false),
                    new IntelSpec($"{sceneName}_terminal_e", EIntelType.TerminalDocument, new Vector3(0f, 26.6f, 30f), "Terminal E", "Final decision terminal", false),
                    new IntelSpec($"{sceneName}_qhipu_01", EIntelType.QhipuFragment, new Vector3(-4f, 26.6f, 28f), "Qhipu Fragment 01", "Final vision shard", false),
                    new IntelSpec($"{sceneName}_qhipu_02", EIntelType.QhipuFragment, new Vector3(0f, 26.6f, 28f), "Qhipu Fragment 02", "Final vision shard", false),
                    new IntelSpec($"{sceneName}_qhipu_03", EIntelType.QhipuFragment, new Vector3(4f, 26.6f, 28f), "Qhipu Fragment 03", "Final vision shard", false)
                },
                Supplies = new[]
                {
                    new SupplySpec($"{sceneName}_toolbox_a", new Vector3(-20f, 0.6f, -18f), false, true, 20f),
                    new SupplySpec($"{sceneName}_toolbox_b", new Vector3(-8f, 8.6f, 8f), false, true, 20f),
                    new SupplySpec($"{sceneName}_ammo_a", new Vector3(-10f, 0.6f, -14f), false, true, 18f),
                    new SupplySpec($"{sceneName}_ammo_b", new Vector3(8f, 8.6f, 10f), false, true, 18f),
                    new SupplySpec($"{sceneName}_ammo_c", new Vector3(14f, 16.6f, 20f), false, true, 18f),
                    new SupplySpec($"{sceneName}_medkit", new Vector3(2f, 8.6f, 6f), true, false, 25f)
                },
                Exits = new[]
                {
                    new ExitSpec($"{sceneName}_exit_final", new Vector3(20f, 26f, 38f), true, true)
                }
            };
        }
    }
}
