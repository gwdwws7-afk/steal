using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using INTIFALL.Data;
using INTIFALL.Level;
using INTIFALL.Narrative;
using INTIFALL.System;
using INTIFALL.Tools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace INTIFALL.Editor
{
    public static class Iteration4AssetAndIntegrityPipeline
    {
        private const string IterationDate = "2026-04-01";
        private const string AssetRecoveryLogPath = "Documents/Asset_Recovery_Execution_Log_2026-04-01.md";
        private const string SceneIntegrityAuditPath = "Documents/Iteration4_Scene_Integrity_Audit_2026-04-01.md";

        private sealed class SceneSpec
        {
            public int LevelIndex;
            public string SceneName;
            public string ScenePath;
            public string DisplayName;

            public SceneSpec(int levelIndex, string sceneName, string scenePath, string displayName)
            {
                LevelIndex = levelIndex;
                SceneName = sceneName;
                ScenePath = scenePath;
                DisplayName = displayName;
            }
        }

        private sealed class AssetReplacementReport
        {
            public int ScenesScanned;
            public int ScenesUpdated;
            public int ToolDataScanned;
            public int ToolDataUpdated;
            public int ToolDataMissingRuntimePrefab;
            public int ResourceMirrorsCreated;
            public int MissingSourceAssets;
            public readonly List<string> Notes = new();
        }

        private sealed class ToolPrefabSpec
        {
            public string PrefabName;
            public Type ComponentType;

            public ToolPrefabSpec(string prefabName, Type componentType)
            {
                PrefabName = prefabName;
                ComponentType = componentType;
            }
        }

        private sealed class SceneAuditRow
        {
            public string SceneName;
            public bool MissingScriptsPass;
            public int MissingScriptCount;
            public bool LevelLoaderPass;
            public bool DataBindingPass;
            public bool ParentReferencePass;
            public bool ManagerPass;
            public bool MissionDataPass;
            public readonly List<string> Notes = new();
        }

        private sealed class SceneIntegrityAuditReport
        {
            public readonly List<SceneAuditRow> Rows = new();
            public readonly List<string> ToolWarnings = new();
            public int ToolDataScanned;
            public int ToolDataMissingRuntimePrefab;
        }

        private static readonly SceneSpec[] CoreScenes =
        {
            new SceneSpec(0, "Level01_Qhapaq_Passage", "Assets/Scenes/Level01_Qhapaq_Passage.unity", "Golden Ruins"),
            new SceneSpec(1, "Level02_Temple_Complex", "Assets/Scenes/Level02_Temple_Complex.unity", "Archive Maze"),
            new SceneSpec(2, "Level03_Underground_Labs", "Assets/Scenes/Level03_Underground_Labs.unity", "Golden Bloodline"),
            new SceneSpec(3, "Level04_Qhipu_Core", "Assets/Scenes/Level04_Qhipu_Core.unity", "Qhipu Core"),
            new SceneSpec(4, "Level05_General_Taki_Villa", "Assets/Scenes/Level05_General_Taki_Villa.unity", "Solar Fall")
        };

        private static readonly Dictionary<string, string> ToolPrefabAlias = new(StringComparer.OrdinalIgnoreCase)
        {
            { "SmokeBomb", "SmokeBomb" },
            { "FlashBang", "FlashBang" },
            { "SleepDart", "SleepDart" },
            { "EMP", "EMP" },
            { "TimedNoise", "TimedNoise" },
            { "SoundBait", "SoundBait" },
            { "DroneInterference", "DroneInterference" },
            { "WallBreaker", "WallBreaker" },
            { "Rope", "Rope" },
            { "Drone", "DroneInterference" },
            { "WallBreak", "WallBreaker" }
        };

        private static readonly ToolPrefabSpec[] CanonicalToolPrefabs =
        {
            new ToolPrefabSpec("SmokeBomb", typeof(SmokeBomb)),
            new ToolPrefabSpec("FlashBang", typeof(FlashBang)),
            new ToolPrefabSpec("SleepDart", typeof(SleepDart)),
            new ToolPrefabSpec("EMP", typeof(EMP)),
            new ToolPrefabSpec("TimedNoise", typeof(TimedNoise)),
            new ToolPrefabSpec("SoundBait", typeof(SoundBait)),
            new ToolPrefabSpec("DroneInterference", typeof(DroneInterference)),
            new ToolPrefabSpec("WallBreaker", typeof(WallBreaker)),
            new ToolPrefabSpec("Rope", typeof(RopeTool))
        };

        [MenuItem("INTIFALL/Iteration4/T1 Apply Source Asset Replacement")]
        public static void ApplySourceAssetReplacementMenu()
        {
            AssetReplacementReport report = ApplySourceAssetReplacement();
            WriteAssetRecoveryLog(report);
            Debug.Log("[Iteration4] T1 asset replacement finished.");
        }

        [MenuItem("INTIFALL/Iteration4/T2 Run Scene Integrity Audit")]
        public static void RunSceneIntegrityAuditMenu()
        {
            SceneIntegrityAuditReport report = RunSceneIntegrityAudit();
            WriteSceneIntegrityAudit(report);
            Debug.Log("[Iteration4] T2 scene integrity audit finished.");
        }

        [MenuItem("INTIFALL/Iteration4/T1+T2 Execute Pipeline")]
        public static void RunT1AndT2Menu()
        {
            RunT1AndT2Batch();
        }

        // Command line entry:
        // Unity.exe -batchmode -quit -projectPath <path> -executeMethod INTIFALL.Editor.Iteration4AssetAndIntegrityPipeline.RunT1AndT2Batch
        public static void RunT1AndT2Batch()
        {
            AssetReplacementReport t1Report = ApplySourceAssetReplacement();
            WriteAssetRecoveryLog(t1Report);

            SceneIntegrityAuditReport t2Report = RunSceneIntegrityAudit();
            WriteSceneIntegrityAudit(t2Report);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static AssetReplacementReport ApplySourceAssetReplacement()
        {
            var report = new AssetReplacementReport();

            EnsureFolderPath("Assets/Resources/INTIFALL");
            EnsureFolderPath("Assets/Resources/INTIFALL/Levels");
            EnsureFolderPath("Assets/Resources/INTIFALL/Spawns");
            EnsureFolderPath("Assets/Resources/Prefabs");
            EnsureFolderPath("Assets/Resources/Prefabs/Tools");

            EnsureResourceMirrorsForCoreData(report);
            EnsureCanonicalToolPrefabs(report);
            RebindToolDataRuntimePrefabs(report);
            EnsureCoreSceneBindings(report);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return report;
        }

        private static void EnsureResourceMirrorsForCoreData(AssetReplacementReport report)
        {
            for (int i = 0; i < CoreScenes.Length; i++)
            {
                SceneSpec spec = CoreScenes[i];

                string levelSource = $"Assets/INTIFALL/ScriptableObjects/Levels/LevelData_{spec.SceneName}.asset";
                string levelMirror = $"Assets/Resources/INTIFALL/Levels/LevelData_{spec.SceneName}.asset";
                EnsureAssetMirror(levelSource, levelMirror, report, "LevelData");

                string enemySource = $"Assets/INTIFALL/ScriptableObjects/Spawns/EnemySpawn_{spec.SceneName}.asset";
                string enemyMirror = $"Assets/Resources/INTIFALL/Spawns/EnemySpawn_{spec.SceneName}.asset";
                EnsureAssetMirror(enemySource, enemyMirror, report, "EnemySpawnData");

                string intelSource = $"Assets/INTIFALL/ScriptableObjects/Spawns/IntelSpawn_{spec.SceneName}.asset";
                string intelMirror = $"Assets/Resources/INTIFALL/Spawns/IntelSpawn_{spec.SceneName}.asset";
                EnsureAssetMirror(intelSource, intelMirror, report, "IntelSpawnData");
            }
        }

        private static void RebindToolDataRuntimePrefabs(AssetReplacementReport report)
        {
            string[] toolGuids = AssetDatabase.FindAssets("t:ToolData", new[] { "Assets/INTIFALL/ScriptableObjects/Tools" });
            var mirroredPrefabs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < toolGuids.Length; i++)
            {
                report.ToolDataScanned++;
                string toolPath = AssetDatabase.GUIDToAssetPath(toolGuids[i]);
                ToolData data = AssetDatabase.LoadAssetAtPath<ToolData>(toolPath);
                if (data == null)
                    continue;

                string prefabName = ResolveToolPrefabName(data.toolName);
                GameObject canonicalPrefab = LoadToolPrefab(prefabName);
                if (canonicalPrefab == null)
                {
                    report.ToolDataMissingRuntimePrefab++;
                    report.Notes.Add($"[WARN] Missing runtime prefab for ToolData '{data.toolName}' at '{toolPath}'.");
                    continue;
                }

                if (data.runtimePrefab != canonicalPrefab)
                {
                    data.runtimePrefab = canonicalPrefab;
                    EditorUtility.SetDirty(data);
                    report.ToolDataUpdated++;
                    report.Notes.Add($"[UPDATE] Bound ToolData '{data.toolName}' runtimePrefab -> '{AssetDatabase.GetAssetPath(canonicalPrefab)}'.");
                }

                if (!mirroredPrefabs.Contains(prefabName))
                {
                    string source = $"Assets/INTIFALL/Prefabs/Tools/{prefabName}.prefab";
                    string mirror = $"Assets/Resources/Prefabs/Tools/{prefabName}.prefab";
                    EnsureAssetMirror(source, mirror, report, "ToolPrefab");
                    mirroredPrefabs.Add(prefabName);
                }
            }
        }

        private static void EnsureCanonicalToolPrefabs(AssetReplacementReport report)
        {
            for (int i = 0; i < CanonicalToolPrefabs.Length; i++)
            {
                ToolPrefabSpec spec = CanonicalToolPrefabs[i];

                EnsureToolPrefabAtPath(
                    $"Assets/INTIFALL/Prefabs/Tools/{spec.PrefabName}.prefab",
                    spec,
                    report);

                EnsureToolPrefabAtPath(
                    $"Assets/Resources/Prefabs/Tools/{spec.PrefabName}.prefab",
                    spec,
                    report);
            }
        }

        private static void EnsureCoreSceneBindings(AssetReplacementReport report)
        {
            for (int i = 0; i < CoreScenes.Length; i++)
            {
                SceneSpec spec = CoreScenes[i];
                report.ScenesScanned++;

                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(spec.ScenePath) == null)
                {
                    report.MissingSourceAssets++;
                    report.Notes.Add($"[ERROR] Missing scene asset '{spec.ScenePath}'.");
                    continue;
                }

                LevelData levelData = AssetDatabase.LoadAssetAtPath<LevelData>(
                    $"Assets/INTIFALL/ScriptableObjects/Levels/LevelData_{spec.SceneName}.asset");
                EnemySpawnData enemySpawnData = AssetDatabase.LoadAssetAtPath<EnemySpawnData>(
                    $"Assets/INTIFALL/ScriptableObjects/Spawns/EnemySpawn_{spec.SceneName}.asset");
                IntelSpawnData intelSpawnData = AssetDatabase.LoadAssetAtPath<IntelSpawnData>(
                    $"Assets/INTIFALL/ScriptableObjects/Spawns/IntelSpawn_{spec.SceneName}.asset");

                if (levelData == null || enemySpawnData == null || intelSpawnData == null)
                {
                    report.MissingSourceAssets++;
                    report.Notes.Add($"[ERROR] Missing Level/Spawn asset set for '{spec.SceneName}'.");
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(spec.ScenePath, OpenSceneMode.Single);
                bool sceneChanged = false;

                GameObject runtimeRoot = GameObject.Find("INTIFALL_Runtime");
                if (runtimeRoot == null)
                {
                    runtimeRoot = new GameObject("INTIFALL_Runtime");
                    sceneChanged = true;
                }

                Transform playerSpawn = GetOrCreateChild(runtimeRoot.transform, "PlayerSpawnPoint", new Vector3(0f, 1f, 2f), ref sceneChanged);
                Transform enemyParent = GetOrCreateChild(runtimeRoot.transform, "EnemyRuntime", Vector3.zero, ref sceneChanged);
                Transform intelParent = GetOrCreateChild(runtimeRoot.transform, "IntelRuntime", Vector3.zero, ref sceneChanged);
                Transform systemsRoot = GetOrCreateChild(runtimeRoot.transform, "Systems", Vector3.zero, ref sceneChanged);

                EnsureChildComponent<GameManager>(systemsRoot, "GameManager", ref sceneChanged);
                EnsureChildComponent<LevelFlowManager>(systemsRoot, "LevelFlowManager", ref sceneChanged);
                EnsureChildComponent<NarrativeManager>(systemsRoot, "NarrativeManager", ref sceneChanged);

                LevelLoader loader = runtimeRoot.GetComponent<LevelLoader>();
                if (loader == null)
                {
                    loader = runtimeRoot.AddComponent<LevelLoader>();
                    sceneChanged = true;
                }

                SerializedObject serialized = new SerializedObject(loader);
                sceneChanged |= SetObjectReference(serialized, "levelData", levelData);
                sceneChanged |= SetObjectReference(serialized, "enemySpawnData", enemySpawnData);
                sceneChanged |= SetObjectReference(serialized, "intelSpawnData", intelSpawnData);
                sceneChanged |= SetObjectReference(serialized, "playerSpawnPoint", playerSpawn);
                sceneChanged |= SetObjectReference(serialized, "enemyParent", enemyParent);
                sceneChanged |= SetObjectReference(serialized, "intelParent", intelParent);
                sceneChanged |= SetBool(serialized, "autoResolveDataBySceneName", true);
                sceneChanged |= SetBool(serialized, "autoCreatePlaceholderPlayer", true);
                sceneChanged |= SetBool(serialized, "spawnMissionExit", true);
                serialized.ApplyModifiedPropertiesWithoutUndo();

                if (sceneChanged)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    report.ScenesUpdated++;
                    report.Notes.Add($"[UPDATE] Scene bindings refreshed: '{spec.ScenePath}'.");
                }
                else
                {
                    report.Notes.Add($"[PASS] Scene bindings already healthy: '{spec.ScenePath}'.");
                }
            }
        }

        private static SceneIntegrityAuditReport RunSceneIntegrityAudit()
        {
            var report = new SceneIntegrityAuditReport();

            for (int i = 0; i < CoreScenes.Length; i++)
            {
                SceneSpec spec = CoreScenes[i];
                var row = new SceneAuditRow
                {
                    SceneName = spec.SceneName
                };

                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(spec.ScenePath) == null)
                {
                    row.MissingScriptsPass = false;
                    row.LevelLoaderPass = false;
                    row.DataBindingPass = false;
                    row.ParentReferencePass = false;
                    row.ManagerPass = false;
                    row.MissionDataPass = false;
                    row.Notes.Add("Scene file missing.");
                    report.Rows.Add(row);
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(spec.ScenePath, OpenSceneMode.Single);
                row.MissingScriptCount = CountMissingScripts(scene);
                row.MissingScriptsPass = row.MissingScriptCount == 0;
                if (!row.MissingScriptsPass)
                    row.Notes.Add($"Missing scripts: {row.MissingScriptCount}");

                LevelLoader loader = UnityEngine.Object.FindFirstObjectByType<LevelLoader>();
                row.LevelLoaderPass = loader != null;
                if (!row.LevelLoaderPass)
                {
                    row.DataBindingPass = false;
                    row.ParentReferencePass = false;
                    row.MissionDataPass = false;
                    row.Notes.Add("LevelLoader missing.");
                }
                else
                {
                    LevelData levelData = loader.GetLevelData();
                    EnemySpawnData enemyData = loader.GetEnemySpawnData();
                    IntelSpawnData intelData = loader.GetIntelSpawnData();

                    row.DataBindingPass = levelData != null &&
                                          enemyData != null &&
                                          intelData != null &&
                                          string.Equals(levelData.sceneName, spec.SceneName, StringComparison.Ordinal);

                    if (!row.DataBindingPass)
                        row.Notes.Add("LevelLoader data references missing or scene name mismatch.");

                    SerializedObject loaderSerialized = new SerializedObject(loader);
                    bool hasPlayerSpawn = HasObjectReference(loaderSerialized, "playerSpawnPoint");
                    bool hasEnemyParent = HasObjectReference(loaderSerialized, "enemyParent");
                    bool hasIntelParent = HasObjectReference(loaderSerialized, "intelParent");
                    row.ParentReferencePass = hasPlayerSpawn && hasEnemyParent && hasIntelParent;
                    if (!row.ParentReferencePass)
                        row.Notes.Add("LevelLoader parent/spawn references missing.");

                    row.MissionDataPass = enemyData != null &&
                                          enemyData.spawnPoints != null &&
                                          enemyData.spawnPoints.Length > 0 &&
                                          intelData != null &&
                                          intelData.intelPoints != null &&
                                          intelData.intelPoints.Length > 0 &&
                                          intelData.supplyPoints != null &&
                                          intelData.supplyPoints.Length > 0 &&
                                          intelData.exitPoints != null &&
                                          intelData.exitPoints.Length > 0;
                    if (!row.MissionDataPass)
                        row.Notes.Add("Enemy/Intel/Supply/Exit spawn data incomplete.");
                }

                row.ManagerPass = UnityEngine.Object.FindFirstObjectByType<GameManager>() != null &&
                                  UnityEngine.Object.FindFirstObjectByType<LevelFlowManager>() != null &&
                                  UnityEngine.Object.FindFirstObjectByType<NarrativeManager>() != null;
                if (!row.ManagerPass)
                    row.Notes.Add("Missing one or more critical managers (GameManager/LevelFlowManager/NarrativeManager).");

                report.Rows.Add(row);
            }

            AuditToolRuntimePrefabs(report);
            return report;
        }

        private static void AuditToolRuntimePrefabs(SceneIntegrityAuditReport report)
        {
            string[] toolGuids = AssetDatabase.FindAssets("t:ToolData", new[] { "Assets/INTIFALL/ScriptableObjects/Tools" });
            for (int i = 0; i < toolGuids.Length; i++)
            {
                report.ToolDataScanned++;
                string toolPath = AssetDatabase.GUIDToAssetPath(toolGuids[i]);
                ToolData data = AssetDatabase.LoadAssetAtPath<ToolData>(toolPath);
                if (data == null)
                    continue;

                if (data.runtimePrefab == null)
                {
                    report.ToolDataMissingRuntimePrefab++;
                    report.ToolWarnings.Add($"[WARN] ToolData '{data.toolName}' has null runtimePrefab ({toolPath}).");
                    continue;
                }

                string prefabPath = AssetDatabase.GetAssetPath(data.runtimePrefab);
                bool validPrefabReference =
                    !string.IsNullOrEmpty(prefabPath) &&
                    prefabPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) &&
                    AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;

                if (!validPrefabReference)
                {
                    report.ToolDataMissingRuntimePrefab++;
                    report.ToolWarnings.Add($"[WARN] runtimePrefab '{data.runtimePrefab.name}' cannot be resolved as a prefab asset ({toolPath}).");
                }
            }
        }

        private static int CountMissingScripts(Scene scene)
        {
            int total = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                total += CountMissingScriptsRecursive(roots[i].transform);
            }

            return total;
        }

        private static int CountMissingScriptsRecursive(Transform root)
        {
            int total = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root.gameObject);
            for (int i = 0; i < root.childCount; i++)
            {
                total += CountMissingScriptsRecursive(root.GetChild(i));
            }

            return total;
        }

        private static void WriteAssetRecoveryLog(AssetReplacementReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Asset Recovery Execution Log");
            sb.AppendLine();
            sb.AppendLine($"Date: {IterationDate}  ");
            sb.AppendLine("Executor: Codex  ");
            sb.AppendLine("Iteration: 4 - T1 Source Asset Replacement Pass");
            sb.AppendLine();
            sb.AppendLine("## Summary");
            sb.AppendLine();
            sb.AppendLine($"1. Core scenes scanned: {report.ScenesScanned}");
            sb.AppendLine($"2. Core scenes updated: {report.ScenesUpdated}");
            sb.AppendLine($"3. ToolData assets scanned: {report.ToolDataScanned}");
            sb.AppendLine($"4. ToolData runtimePrefab updates: {report.ToolDataUpdated}");
            sb.AppendLine($"5. Resource mirror assets created: {report.ResourceMirrorsCreated}");
            sb.AppendLine($"6. Missing source assets detected: {report.MissingSourceAssets}");
            sb.AppendLine($"7. ToolData missing runtimePrefab after pass: {report.ToolDataMissingRuntimePrefab}");
            sb.AppendLine();
            sb.AppendLine("## Detailed Log");
            sb.AppendLine();

            if (report.Notes.Count == 0)
            {
                sb.AppendLine("1. No changes were required.");
            }
            else
            {
                for (int i = 0; i < report.Notes.Count; i++)
                {
                    sb.AppendLine($"{i + 1}. {report.Notes[i]}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("## Result");
            sb.AppendLine();
            sb.AppendLine(report.MissingSourceAssets == 0
                ? "T1 PASS: source asset replacement pass completed with no missing critical sources."
                : "T1 WARN: pass completed with missing source assets; see detailed log.");

            WriteDocument(AssetRecoveryLogPath, sb.ToString());
        }

        private static void WriteSceneIntegrityAudit(SceneIntegrityAuditReport report)
        {
            int scenePassCount = report.Rows.Count(IsSceneRowPass);
            int sceneFailCount = report.Rows.Count - scenePassCount;

            var sb = new StringBuilder();
            sb.AppendLine("# Iteration 4 Scene Integrity Audit");
            sb.AppendLine();
            sb.AppendLine($"Date: {IterationDate}  ");
            sb.AppendLine("Executor: Codex  ");
            sb.AppendLine("Iteration: 4 - T2 Scene Integrity Validator + Coverage Audit");
            sb.AppendLine();
            sb.AppendLine("## Coverage Table");
            sb.AppendLine();
            sb.AppendLine("| Scene | Missing Scripts | LevelLoader | Data Binding | Parent Refs | Managers | Mission Data | Result |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|");

            for (int i = 0; i < report.Rows.Count; i++)
            {
                SceneAuditRow row = report.Rows[i];
                string result = IsSceneRowPass(row) ? "PASS" : "FAIL";
                sb.AppendLine(
                    $"| `{row.SceneName}` | {ToPassFail(row.MissingScriptsPass)} ({row.MissingScriptCount}) | {ToPassFail(row.LevelLoaderPass)} | {ToPassFail(row.DataBindingPass)} | {ToPassFail(row.ParentReferencePass)} | {ToPassFail(row.ManagerPass)} | {ToPassFail(row.MissionDataPass)} | {result} |");
            }

            sb.AppendLine();
            sb.AppendLine("## Scene Notes");
            sb.AppendLine();

            int noteIndex = 1;
            for (int i = 0; i < report.Rows.Count; i++)
            {
                SceneAuditRow row = report.Rows[i];
                if (row.Notes.Count == 0)
                    continue;

                sb.AppendLine($"{noteIndex}. `{row.SceneName}`");
                for (int n = 0; n < row.Notes.Count; n++)
                {
                    sb.AppendLine($"   - {row.Notes[n]}");
                }
                noteIndex++;
            }

            if (noteIndex == 1)
                sb.AppendLine("1. No scene-level warnings.");

            sb.AppendLine();
            sb.AppendLine("## Tool Runtime Prefab Audit");
            sb.AppendLine();
            sb.AppendLine($"1. ToolData scanned: {report.ToolDataScanned}");
            sb.AppendLine($"2. ToolData missing/invalid runtimePrefab: {report.ToolDataMissingRuntimePrefab}");

            if (report.ToolWarnings.Count > 0)
            {
                sb.AppendLine("3. Warnings:");
                for (int i = 0; i < report.ToolWarnings.Count; i++)
                {
                    sb.AppendLine($"   - {report.ToolWarnings[i]}");
                }
            }
            else
            {
                sb.AppendLine("3. Warnings: none");
            }

            sb.AppendLine();
            sb.AppendLine("## Result");
            sb.AppendLine();
            sb.AppendLine($"1. Scene PASS count: {scenePassCount}");
            sb.AppendLine($"2. Scene FAIL count: {sceneFailCount}");
            sb.AppendLine(sceneFailCount == 0 && report.ToolDataMissingRuntimePrefab == 0
                ? "3. T2 PASS: all audited scenes passed integrity checks."
                : "3. T2 WARN: integrity issues remain; inspect notes.");

            WriteDocument(SceneIntegrityAuditPath, sb.ToString());
        }

        private static bool IsSceneRowPass(SceneAuditRow row)
        {
            return row.MissingScriptsPass &&
                   row.LevelLoaderPass &&
                   row.DataBindingPass &&
                   row.ParentReferencePass &&
                   row.ManagerPass &&
                   row.MissionDataPass;
        }

        private static string ToPassFail(bool value)
        {
            return value ? "PASS" : "FAIL";
        }

        private static void WriteDocument(string relativePath, string content)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
            string absolutePath = Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            string parent = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
                Directory.CreateDirectory(parent);

            File.WriteAllText(absolutePath, content, Encoding.UTF8);
            Debug.Log($"[Iteration4] Wrote document: {relativePath}");
        }

        private static void EnsureAssetMirror(
            string sourcePath,
            string mirrorPath,
            AssetReplacementReport report,
            string label)
        {
            if (AssetDatabase.LoadMainAssetAtPath(sourcePath) == null)
            {
                report.MissingSourceAssets++;
                report.Notes.Add($"[WARN] Missing {label} source: '{sourcePath}'.");
                return;
            }

            if (AssetDatabase.LoadMainAssetAtPath(mirrorPath) != null)
                return;

            EnsureFolderPath(GetParentFolder(mirrorPath));
            bool copied = AssetDatabase.CopyAsset(sourcePath, mirrorPath);
            if (copied)
            {
                report.ResourceMirrorsCreated++;
                report.Notes.Add($"[CREATE] Mirror {label}: '{mirrorPath}' from '{sourcePath}'.");
            }
            else
            {
                report.Notes.Add($"[WARN] Failed to create mirror {label}: '{mirrorPath}'.");
            }
        }

        private static void EnsureToolPrefabAtPath(string prefabPath, ToolPrefabSpec spec, AssetReplacementReport report)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null)
            {
                EnsureFolderPath(GetParentFolder(prefabPath));
                var created = new GameObject(spec.PrefabName);
                created.AddComponent(spec.ComponentType);
                PrefabUtility.SaveAsPrefabAsset(created, prefabPath);
                UnityEngine.Object.DestroyImmediate(created);

                report.ResourceMirrorsCreated++;
                report.Notes.Add($"[CREATE] Tool prefab created: '{prefabPath}'.");
                return;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            bool changed = false;

            int missingScriptCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefabRoot);
            if (missingScriptCount > 0)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefabRoot);
                changed = true;
                report.Notes.Add($"[UPDATE] Removed {missingScriptCount} missing script entries from '{prefabPath}'.");
            }

            if (prefabRoot.GetComponent(spec.ComponentType) == null)
            {
                prefabRoot.AddComponent(spec.ComponentType);
                changed = true;
                report.Notes.Add($"[UPDATE] Added '{spec.ComponentType.Name}' component to '{prefabPath}'.");
            }

            if (changed)
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);

            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        private static string ResolveToolPrefabName(string toolName)
        {
            if (string.IsNullOrEmpty(toolName))
                return string.Empty;

            if (ToolPrefabAlias.TryGetValue(toolName, out string mappedName))
                return mappedName;

            return toolName;
        }

        private static GameObject LoadToolPrefab(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
                return null;

            string intifallPath = $"Assets/INTIFALL/Prefabs/Tools/{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(intifallPath);
            if (prefab != null)
                return prefab;

            string resourcesPath = $"Assets/Resources/Prefabs/Tools/{prefabName}.prefab";
            return AssetDatabase.LoadAssetAtPath<GameObject>(resourcesPath);
        }

        private static Transform GetOrCreateChild(Transform parent, string name, Vector3 localPosition, ref bool changed)
        {
            Transform child = parent.Find(name);
            if (child != null)
                return child;

            var childGo = new GameObject(name);
            childGo.transform.SetParent(parent, false);
            childGo.transform.localPosition = localPosition;
            changed = true;
            return childGo.transform;
        }

        private static void EnsureChildComponent<T>(Transform parent, string childName, ref bool changed)
            where T : Component
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                GameObject childGo = new GameObject(childName);
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

        private static bool HasObjectReference(SerializedObject serialized, string propertyName)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
                return false;

            return property.objectReferenceValue != null;
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

        private static string GetParentFolder(string path)
        {
            int index = path.LastIndexOf('/');
            if (index <= 0)
                return string.Empty;
            return path.Substring(0, index);
        }
    }
}
