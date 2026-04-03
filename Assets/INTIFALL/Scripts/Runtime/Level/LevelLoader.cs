using INTIFALL.AI;
using INTIFALL.Data;
using INTIFALL.Economy;
using INTIFALL.Environment;
using INTIFALL.Growth;
using INTIFALL.Narrative;
using INTIFALL.Player;
using INTIFALL.System;
using INTIFALL.Tools;
using INTIFALL.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace INTIFALL.Level
{
    public class LevelLoader : MonoBehaviour
    {
        [Header("Level Data")]
        [SerializeField] private LevelData levelData;
        [SerializeField] private EnemySpawnData enemySpawnData;
        [SerializeField] private IntelSpawnData intelSpawnData;
        [SerializeField] private bool autoResolveDataBySceneName = true;
        [SerializeField] private bool preferResourceDataBySceneName = true;
        [SerializeField] private bool autoCreatePlaceholderPlayer = true;
        [SerializeField] private bool forceStrictRuntimeValidationInEditor = false;
        [SerializeField] private bool allowPlaceholderFallbackInStrictRuntime = false;
        [SerializeField] private bool spawnMissionExit = true;

        [Header("References")]
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform enemyParent;
        [SerializeField] private Transform intelParent;

        [Header("Prefabs")]
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private GameObject[] intelPrefabs;

        private GameObject _player;
        private int _enemiesSpawned;
        private int _intelSpawned;
        private static GameObject _runtimeEnemyFallbackPrefab;
        private static GameObject _runtimeIntelFallbackPrefab;

        public int EnemiesSpawned => _enemiesSpawned;
        public int IntelSpawned => _intelSpawned;

        private void Start()
        {
            LoadLevel();
        }

        public void LoadLevel()
        {
            AutoResolveDataReferences();
            EnsureParentsAndSpawnPoint();

            if (levelData == null)
            {
                Debug.LogWarning("LevelLoader: No level data assigned");
                return;
            }

            _enemiesSpawned = 0;
            _intelSpawned = 0;

            SpawnPlayer();
            EnsureRuntimeUI();
            EnsureRuntimeNarrative();
            EnsureRuntimeMissionSystems();
            EnsureRuntimeEconomyGrowth();
            SpawnEnemies();
            ApplyEnemyAITuning();
            SpawnIntel();
            SpawnExitPoints();
            SetupLevelEnvironment();

            GameManager.Instance?.LoadLevel(levelData.levelIndex, levelData.levelName);

            EventBus.Publish(new LevelLoadedEvent
            {
                levelIndex = levelData.levelIndex,
                levelName = levelData.levelName
            });
        }

        private void AutoResolveDataReferences()
        {
            if (!autoResolveDataBySceneName)
                return;

            string sceneName = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName))
                return;

            LevelData resolvedLevelData = Resources.Load<LevelData>($"INTIFALL/Levels/LevelData_{sceneName}");
            EnemySpawnData resolvedEnemySpawnData = Resources.Load<EnemySpawnData>($"INTIFALL/Spawns/EnemySpawn_{sceneName}");
            IntelSpawnData resolvedIntelSpawnData = Resources.Load<IntelSpawnData>($"INTIFALL/Spawns/IntelSpawn_{sceneName}");

            if (preferResourceDataBySceneName)
            {
                if (resolvedLevelData != null)
                    levelData = resolvedLevelData;
                if (resolvedEnemySpawnData != null)
                    enemySpawnData = resolvedEnemySpawnData;
                if (resolvedIntelSpawnData != null)
                    intelSpawnData = resolvedIntelSpawnData;
                return;
            }

            if (levelData == null)
                levelData = resolvedLevelData;
            if (enemySpawnData == null)
                enemySpawnData = resolvedEnemySpawnData;
            if (intelSpawnData == null)
                intelSpawnData = resolvedIntelSpawnData;
        }

        private void EnsureParentsAndSpawnPoint()
        {
            if (playerSpawnPoint == null)
            {
                var spawnGo = new GameObject("PlayerSpawnPoint");
                spawnGo.transform.position = new Vector3(0f, 1f, 0f);
                playerSpawnPoint = spawnGo.transform;
            }

            if (enemyParent == null)
                enemyParent = new GameObject("Enemies").transform;

            if (intelParent == null)
                intelParent = new GameObject("Intel").transform;
        }

        private void SpawnPlayer()
        {
            if (playerSpawnPoint == null)
            {
                Debug.LogWarning("LevelLoader: No player spawn point assigned");
                return;
            }

            _player = GameObject.FindGameObjectWithTag("Player");
            if (_player == null && autoCreatePlaceholderPlayer && ShouldUsePlaceholderFallback())
            {
                _player = CreatePlaceholderPlayer(playerSpawnPoint.position, playerSpawnPoint.rotation);
            }

            if (_player == null)
            {
                if (IsStrictRuntimeValidationEnabled())
                    Debug.LogError("LevelLoader: Missing Player and placeholder fallback is disabled in strict runtime.");
                return;
            }

            _player.transform.position = playerSpawnPoint.position;
            _player.transform.rotation = playerSpawnPoint.rotation;
        }

        private void SpawnEnemies()
        {
            bool allowPlaceholderFallback = ShouldUsePlaceholderFallback();
            EnemySpawnPoint[] spawnPoints = enemySpawnData != null ? enemySpawnData.spawnPoints : null;
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                if (!allowPlaceholderFallback)
                {
                    Debug.LogError("LevelLoader: Missing EnemySpawnData spawnPoints and placeholder fallback is disabled in strict runtime.");
                    return;
                }
                spawnPoints = CreateDefaultEnemySpawns();
            }

            foreach (var spawnPoint in spawnPoints)
            {
                GameObject enemyPrefab = GetEnemyPrefab(spawnPoint.enemyType);
                GameObject enemy;
                if (enemyPrefab != null)
                {
                    enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation, enemyParent);
                }
                else if (allowPlaceholderFallback)
                {
                    enemy = CreatePlaceholderEnemy(spawnPoint.position, spawnPoint.rotation, enemyParent);
                }
                else
                {
                    Debug.LogError($"LevelLoader: Missing enemy prefab for {spawnPoint.enemyType} and placeholder fallback is disabled.");
                    continue;
                }

                if (!enemy.activeSelf)
                    enemy.SetActive(true);

                SetupEnemy(enemy, spawnPoint);
                _enemiesSpawned++;
            }
        }

        private EnemySpawnPoint[] CreateDefaultEnemySpawns()
        {
            Vector3 basePos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            return new[]
            {
                new EnemySpawnPoint
                {
                    spawnId = "default_enemy_01",
                    position = basePos + new Vector3(8f, 0f, 8f),
                    rotation = Quaternion.identity,
                    enemyType = EEnemySpawnType.Normal
                }
            };
        }

        private void SetupEnemy(GameObject enemy, EnemySpawnPoint spawnPoint)
        {
            var stateMachine = enemy.GetComponent<EnemyStateMachine>();
            if (stateMachine != null && spawnPoint.isPatrol)
            {
                var patrolRoute = enemy.GetComponent<PatrolRoute>();
                if (patrolRoute != null && !string.IsNullOrEmpty(spawnPoint.patrolRouteId))
                {
                    patrolRoute.SetRouteId(spawnPoint.patrolRouteId);
                }
            }

            var combatTrigger = Object.FindFirstObjectByType<CombatTrigger>();
            if (combatTrigger != null && stateMachine != null)
                combatTrigger.TrackEnemy(stateMachine);
        }

        private GameObject GetEnemyPrefab(EEnemySpawnType type)
        {
            if (enemyPrefabs != null && enemyPrefabs.Length > 0)
            {
                int index = (int)type;
                if (index >= 0 && index < enemyPrefabs.Length && enemyPrefabs[index] != null)
                    return enemyPrefabs[index];

                if (enemyPrefabs[0] != null)
                    return enemyPrefabs[0];
            }

            GameObject loadedPrefab = LoadEnemyPrefabFromResources(type);
            if (loadedPrefab != null)
                return loadedPrefab;

            return GetRuntimeEnemyFallbackPrefab();
        }

        private void SpawnIntel()
        {
            bool allowPlaceholderFallback = ShouldUsePlaceholderFallback();
            IntelSpawnPoint[] intelPoints = intelSpawnData != null ? intelSpawnData.intelPoints : null;
            if (intelPoints == null || intelPoints.Length == 0)
            {
                if (!allowPlaceholderFallback)
                {
                    Debug.LogError("LevelLoader: Missing IntelSpawnData intelPoints and placeholder fallback is disabled in strict runtime.");
                    return;
                }
                intelPoints = CreateDefaultIntelSpawns();
            }

            foreach (var intelPoint in intelPoints)
            {
                SpawnIntelItem(intelPoint, allowPlaceholderFallback);
            }

            if (intelSpawnData?.supplyPoints == null || intelSpawnData.supplyPoints.Length == 0)
            {
                if (!allowPlaceholderFallback)
                {
                    Debug.LogError("LevelLoader: Missing supply points and placeholder fallback is disabled in strict runtime.");
                    return;
                }
                SpawnSupplyPoint(new SupplyPointData
                {
                    supplyId = "default_supply",
                    position = (playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero) + new Vector3(-4f, 0f, 4f),
                    providesFirstAid = true,
                    providesTools = true,
                    cooldownDuration = 30f
                });
                return;
            }

            foreach (var supplyPoint in intelSpawnData.supplyPoints)
            {
                SpawnSupplyPoint(supplyPoint);
            }
        }

        private IntelSpawnPoint[] CreateDefaultIntelSpawns()
        {
            Vector3 basePos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            return new[]
            {
                new IntelSpawnPoint
                {
                    intelId = "default_intel_01",
                    intelType = EIntelType.QhipuFragment,
                    position = basePos + new Vector3(4f, 0f, 8f),
                    displayName = "Qhipu Fragment 01"
                },
                new IntelSpawnPoint
                {
                    intelId = "default_intel_02",
                    intelType = EIntelType.TerminalDocument,
                    position = basePos + new Vector3(-5f, 0f, 10f),
                    displayName = "Terminal Document 02"
                },
                new IntelSpawnPoint
                {
                    intelId = "default_intel_03",
                    intelType = EIntelType.AudioLog,
                    position = basePos + new Vector3(0f, 0f, 14f),
                    displayName = "Audio Log 03"
                }
            };
        }

        private void SpawnIntelItem(IntelSpawnPoint intelPoint, bool allowPlaceholderFallback)
        {
            GameObject prefab = GetIntelPrefab(intelPoint.intelType);

            GameObject intel;
            if (prefab != null)
            {
                intel = Instantiate(prefab, intelPoint.position, Quaternion.identity, intelParent);
            }
            else if (allowPlaceholderFallback)
            {
                intel = CreatePlaceholderIntel(intelPoint.position, intelParent);
            }
            else
            {
                Debug.LogError($"LevelLoader: Missing intel prefab for {intelPoint.intelType} and placeholder fallback is disabled.");
                return;
            }

            if (!intel.activeSelf)
                intel.SetActive(true);

            IntelPickup pickup = intel.GetComponent<IntelPickup>();
            if (pickup == null)
                pickup = intel.AddComponent<IntelPickup>();

            int levelIndex = levelData != null ? levelData.levelIndex : 0;
            pickup.Configure(intelPoint.intelId, levelIndex, intelPoint.intelType, intelPoint.displayName);

            _intelSpawned++;
        }

        private GameObject GetIntelPrefab(EIntelType type)
        {
            if (intelPrefabs != null && intelPrefabs.Length > 0)
            {
                int index = (int)type;
                if (index >= 0 && index < intelPrefabs.Length && intelPrefabs[index] != null)
                    return intelPrefabs[index];

                if (intelPrefabs[0] != null)
                    return intelPrefabs[0];
            }

            GameObject loadedPrefab = LoadIntelPrefabFromResources(type);
            if (loadedPrefab != null)
                return loadedPrefab;

            return GetRuntimeIntelFallbackPrefab();
        }

        private void SpawnSupplyPoint(SupplyPointData supplyData)
        {
            string objectName = $"Supply_{supplyData.supplyId}";
            Transform existing = intelParent.Find(objectName);
            GameObject go = existing != null ? existing.gameObject : new GameObject(objectName);
            go.transform.SetParent(intelParent);
            go.transform.position = supplyData.position;

            SphereCollider col = go.GetComponent<SphereCollider>();
            if (col == null)
                col = go.AddComponent<SphereCollider>();
            col.radius = 1.5f;
            col.isTrigger = true;

            var supplyPoint = go.GetComponent<Economy.SupplyPoint>();
            if (supplyPoint == null)
                supplyPoint = go.AddComponent<Economy.SupplyPoint>();

            supplyPoint.Configure(supplyData.providesFirstAid, supplyData.providesTools, supplyData.cooldownDuration);
        }

        private void SpawnExitPoints()
        {
            if (!spawnMissionExit)
                return;

            bool allowPlaceholderFallback = ShouldUsePlaceholderFallback();
            ExitPointData[] exits = intelSpawnData != null ? intelSpawnData.exitPoints : null;
            if (exits == null || exits.Length == 0)
            {
                if (!allowPlaceholderFallback)
                {
                    Debug.LogError("LevelLoader: Missing exit points and placeholder fallback is disabled in strict runtime.");
                    return;
                }
                SpawnExitPoint(new ExitPointData
                {
                    exitId = "default_exit",
                    position = (playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero) + new Vector3(0f, 0f, 20f),
                    requiresAllIntel = true,
                    isMainExit = true
                });
                return;
            }

            foreach (var exit in exits)
            {
                SpawnExitPoint(exit);
            }
        }

        private void SpawnExitPoint(ExitPointData exitData)
        {
            string objectName = $"Exit_{exitData.exitId}";
            Transform existing = intelParent.Find(objectName);
            GameObject go = existing != null ? existing.gameObject : new GameObject(objectName);
            go.transform.SetParent(intelParent);
            go.transform.position = exitData.position;

            BoxCollider col = go.GetComponent<BoxCollider>();
            if (col == null)
                col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(2f, 2f, 2f);
            col.isTrigger = true;

            MissionExitPoint exitPoint = go.GetComponent<MissionExitPoint>();
            if (exitPoint == null)
                exitPoint = go.AddComponent<MissionExitPoint>();

            int levelIndex = levelData != null ? levelData.levelIndex : 0;
            int totalIntel = intelSpawnData != null && intelSpawnData.intelPoints != null
                ? intelSpawnData.intelPoints.Length
                : 0;
            int requiredIntel = exitData.requiredIntelCount >= 0
                ? exitData.requiredIntelCount
                : (exitData.requiresAllIntel ? totalIntel : 0);
            requiredIntel = Mathf.Clamp(requiredIntel, 0, totalIntel);

            string routeId = string.IsNullOrWhiteSpace(exitData.routeId)
                ? exitData.exitId
                : exitData.routeId;
            string routeLabel = string.IsNullOrWhiteSpace(exitData.routeLabel)
                ? (exitData.isMainExit ? "Main Extraction" : "Optional Extraction")
                : exitData.routeLabel;

            exitPoint.Configure(
                levelIndex,
                exitData.requiresAllIntel,
                requiredIntel,
                routeId,
                routeLabel,
                exitData.routeRiskTier,
                exitData.creditMultiplier,
                exitData.secondaryObjectiveBonus,
                exitData.isMainExit);
        }

        private void SetupLevelEnvironment()
        {
            if (levelData == null) return;

            ConfigureVentSystem(levelData.hasVentSystem);
            ConfigureSurveillanceCameras(levelData.hasSurveillanceCameras);
            ConfigureElectronicDoors(levelData.hasElectronicDoors);
        }

        private void ApplyEnemyAITuning()
        {
            if (levelData == null)
                return;

            int pressureTier = Mathf.Clamp(levelData.patrolPressureTier, 1, 5);
            float tierT = Mathf.InverseLerp(1f, 5f, pressureTier);

            float tunedAlertDuration = Mathf.Max(1f, levelData.baseAlertDuration);
            float tunedSearchDuration = Mathf.Max(1f, levelData.searchDuration);
            float tunedFullAlertFailDelay = Mathf.Max(tunedAlertDuration + 4f, levelData.fullAlertDuration);
            float tunedAlertDropDelay = Mathf.Lerp(1.0f, 1.6f, tierT);
            float tunedSuspiciousDuration = Mathf.Lerp(2.0f, 2.8f, tierT);

            EnemyStateMachine[] stateMachines = Object.FindObjectsByType<EnemyStateMachine>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < stateMachines.Length; i++)
            {
                if (stateMachines[i] == null)
                    continue;

                stateMachines[i].ConfigureTimingProfile(
                    tunedSearchDuration,
                    tunedAlertDuration,
                    tunedFullAlertFailDelay,
                    tunedAlertDropDelay,
                    tunedSuspiciousDuration);
            }

            float detectionPulseInterval = Mathf.Lerp(0.85f, 0.55f, tierT);
            float searchingPulseMultiplier = Mathf.Lerp(0.9f, 0.7f, tierT);
            float alertedPulseMultiplier = Mathf.Lerp(0.65f, 0.45f, tierT);
            float broadcastCooldown = Mathf.Lerp(1.25f, 0.8f, tierT);

            EnemyController[] controllers = Object.FindObjectsByType<EnemyController>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < controllers.Length; i++)
            {
                if (controllers[i] == null)
                    continue;

                controllers[i].ConfigureDetectionProfile(
                    detectionPulseInterval,
                    searchingPulseMultiplier,
                    alertedPulseMultiplier,
                    broadcastCooldown);
            }
        }

        private static void ConfigureVentSystem(bool enabled)
        {
            VentEntrance[] vents = Object.FindObjectsByType<VentEntrance>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < vents.Length; i++)
            {
                if (vents[i] != null)
                    vents[i].gameObject.SetActive(enabled);
            }
        }

        private static void ConfigureSurveillanceCameras(bool enabled)
        {
            SurveillanceCamera[] cameras = Object.FindObjectsByType<SurveillanceCamera>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] != null)
                    cameras[i].gameObject.SetActive(enabled);
            }
        }

        private static void ConfigureElectronicDoors(bool enabled)
        {
            ElectronicDoor[] doors = Object.FindObjectsByType<ElectronicDoor>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < doors.Length; i++)
            {
                if (doors[i] != null)
                    doors[i].gameObject.SetActive(enabled);
            }
        }

        private bool IsStrictRuntimeValidationEnabled()
        {
            if (forceStrictRuntimeValidationInEditor)
                return true;

            return !Application.isEditor && !Debug.isDebugBuild;
        }

        private bool ShouldUsePlaceholderFallback()
        {
            if (!IsStrictRuntimeValidationEnabled())
                return true;

            return allowPlaceholderFallbackInStrictRuntime;
        }

        private static GameObject CreatePlaceholderIntel(Vector3 position, Transform parent)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "IntelPlaceholder";
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.8f;

            Collider col = go.GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;

            return go;
        }

        private static GameObject CreatePlaceholderEnemy(Vector3 position, Quaternion rotation, Transform parent)
        {
            GameObject enemy = new GameObject("EnemyPlaceholder");
            enemy.transform.SetParent(parent);
            enemy.transform.position = position;
            enemy.transform.rotation = rotation;

            enemy.AddComponent<CharacterController>();
            enemy.AddComponent<EnemyStateMachine>();
            enemy.AddComponent<PerceptionModule>();
            enemy.AddComponent<EnemyController>();
            return enemy;
        }

        private static void EnsureRuntimeUI()
        {
            GameObject runtimeUiRoot = GameObject.Find("INTIFALL_Runtime_UI");
            if (runtimeUiRoot == null)
                runtimeUiRoot = new GameObject("INTIFALL_Runtime_UI");

            if (Object.FindFirstObjectByType<HUDManager>() == null)
                runtimeUiRoot.AddComponent<HUDManager>();

            if (Object.FindFirstObjectByType<PauseMenuUI>() == null)
                runtimeUiRoot.AddComponent<PauseMenuUI>();

            if (Object.FindFirstObjectByType<MissionDebriefUI>() == null)
                runtimeUiRoot.AddComponent<MissionDebriefUI>();
        }

        private static void EnsureRuntimeNarrative()
        {
            GameObject runtimeNarrativeRoot = GameObject.Find("INTIFALL_Runtime_Narrative");
            if (runtimeNarrativeRoot == null)
                runtimeNarrativeRoot = new GameObject("INTIFALL_Runtime_Narrative");

            if (Object.FindFirstObjectByType<NarrativeManager>() == null)
                runtimeNarrativeRoot.AddComponent<NarrativeManager>();

            if (Object.FindFirstObjectByType<WillaComm>() == null)
                runtimeNarrativeRoot.AddComponent<WillaComm>();
        }

        private static void EnsureRuntimeMissionSystems()
        {
            GameObject runtimeMissionRoot = GameObject.Find("INTIFALL_Runtime_Mission");
            if (runtimeMissionRoot == null)
                runtimeMissionRoot = new GameObject("INTIFALL_Runtime_Mission");

            if (Object.FindFirstObjectByType<SecondaryObjectiveTracker>() == null)
                runtimeMissionRoot.AddComponent<SecondaryObjectiveTracker>();
        }

        private static void EnsureRuntimeEconomyGrowth()
        {
            GameObject runtimeMetaRoot = GameObject.Find("INTIFALL_Runtime_Meta");
            if (runtimeMetaRoot == null)
                runtimeMetaRoot = new GameObject("INTIFALL_Runtime_Meta");

            if (Object.FindFirstObjectByType<CreditSystem>() == null)
                runtimeMetaRoot.AddComponent<CreditSystem>();

            if (Object.FindFirstObjectByType<ProgressionTree>() == null)
                runtimeMetaRoot.AddComponent<ProgressionTree>();

            if (Object.FindFirstObjectByType<BloodlineSystem>() == null)
                runtimeMetaRoot.AddComponent<BloodlineSystem>();
        }

        private static GameObject LoadEnemyPrefabFromResources(EEnemySpawnType type)
        {
            string typeName = type.ToString();
            string[] candidatePaths =
            {
                $"Prefabs/Enemies/{typeName}",
                $"INTIFALL/Prefabs/Enemies/{typeName}",
                $"INTIFALL/Enemies/{typeName}",
                $"Prefabs/{typeName}Enemy"
            };

            for (int i = 0; i < candidatePaths.Length; i++)
            {
                GameObject prefab = Resources.Load<GameObject>(candidatePaths[i]);
                if (prefab != null)
                    return prefab;
            }

            return null;
        }

        private static GameObject LoadIntelPrefabFromResources(EIntelType type)
        {
            string typeName = type.ToString();
            string[] candidatePaths =
            {
                $"Prefabs/Intel/{typeName}",
                $"INTIFALL/Prefabs/Intel/{typeName}",
                $"INTIFALL/Intel/{typeName}",
                $"Prefabs/{typeName}Intel"
            };

            for (int i = 0; i < candidatePaths.Length; i++)
            {
                GameObject prefab = Resources.Load<GameObject>(candidatePaths[i]);
                if (prefab != null)
                    return prefab;
            }

            return null;
        }

        private static GameObject GetRuntimeEnemyFallbackPrefab()
        {
            if (_runtimeEnemyFallbackPrefab != null)
                return _runtimeEnemyFallbackPrefab;

            GameObject fallback = new GameObject("RuntimeEnemyFallbackPrefab");
            fallback.SetActive(false);
            fallback.AddComponent<CharacterController>();
            fallback.AddComponent<EnemyStateMachine>();
            fallback.AddComponent<PerceptionModule>();
            fallback.AddComponent<EnemyController>();
            _runtimeEnemyFallbackPrefab = fallback;
            return _runtimeEnemyFallbackPrefab;
        }

        private static GameObject GetRuntimeIntelFallbackPrefab()
        {
            if (_runtimeIntelFallbackPrefab != null)
                return _runtimeIntelFallbackPrefab;

            GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fallback.name = "RuntimeIntelFallbackPrefab";
            fallback.SetActive(false);
            fallback.transform.localScale = Vector3.one * 0.8f;

            Collider col = fallback.GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;

            fallback.AddComponent<IntelPickup>();
            _runtimeIntelFallbackPrefab = fallback;
            return _runtimeIntelFallbackPrefab;
        }

        private static GameObject CreatePlaceholderPlayer(Vector3 position, Quaternion rotation)
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = position;
            player.transform.rotation = rotation;

            player.AddComponent<CharacterController>();
            player.AddComponent<PlayerStateMachine>();
            player.AddComponent<PlayerController>();
            player.AddComponent<PlayerHealthSystem>();
            player.AddComponent<PlayerCombatStateMachine>();
            player.AddComponent<CombatTrigger>();
            player.AddComponent<CQCSystem>();
            player.AddComponent<CoverSystem>();

            var toolManager = player.AddComponent<ToolManager>();
            var anchor = new GameObject("ToolAnchor");
            anchor.transform.SetParent(player.transform, false);
            toolManager.SetToolAnchor(anchor.transform);

            if (Camera.main == null)
            {
                GameObject cameraGo = new GameObject("MainCamera");
                cameraGo.tag = "MainCamera";
                cameraGo.transform.position = player.transform.position + new Vector3(0f, 1.8f, -4f);
                cameraGo.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
                cameraGo.AddComponent<Camera>();
            }

            return player;
        }

        public LevelData GetLevelData()
        {
            return levelData;
        }

        public EnemySpawnData GetEnemySpawnData()
        {
            return enemySpawnData;
        }

        public IntelSpawnData GetIntelSpawnData()
        {
            return intelSpawnData;
        }
    }
}
