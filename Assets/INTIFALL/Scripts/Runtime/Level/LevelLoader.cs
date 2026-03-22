using UnityEngine;
using INTIFALL.AI;
using INTIFALL.System;

namespace INTIFALL.Level
{
    public class LevelLoader : MonoBehaviour
    {
        [Header("Level Data")]
        [SerializeField] private LevelData levelData;
        [SerializeField] private EnemySpawnData enemySpawnData;
        [SerializeField] private IntelSpawnData intelSpawnData;

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

        public int EnemiesSpawned => _enemiesSpawned;
        public int IntelSpawned => _intelSpawned;

        private void Start()
        {
            LoadLevel();
        }

        public void LoadLevel()
        {
            if (levelData == null)
            {
                Debug.LogWarning("LevelLoader: No level data assigned");
                return;
            }

            SpawnPlayer();
            SpawnEnemies();
            SpawnIntel();
            SetupLevelEnvironment();

            EventBus.Publish(new LevelLoadedEvent
            {
                levelIndex = levelData.levelIndex,
                levelName = levelData.levelName
            });
        }

        private void SpawnPlayer()
        {
            if (playerSpawnPoint == null)
            {
                Debug.LogWarning("LevelLoader: No player spawn point assigned");
                return;
            }

            _player = GameObject.FindGameObjectWithTag("Player");
            if (_player != null)
            {
                _player.transform.position = playerSpawnPoint.position;
                _player.transform.rotation = playerSpawnPoint.rotation;
            }
        }

        private void SpawnEnemies()
        {
            if (enemySpawnData == null || enemySpawnData.spawnPoints == null)
                return;

            if (enemyParent == null)
            {
                enemyParent = new GameObject("Enemies").transform;
            }

            foreach (var spawnPoint in enemySpawnData.spawnPoints)
            {
                GameObject enemyPrefab = GetEnemyPrefab(spawnPoint.enemyType);
                if (enemyPrefab == null) continue;

                GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation, enemyParent);
                SetupEnemy(enemy, spawnPoint);
                _enemiesSpawned++;
            }
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
        }

        private GameObject GetEnemyPrefab(EEnemySpawnType type)
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
                return null;

            int index = (int)type;
            if (index >= enemyPrefabs.Length)
                index = 0;

            return enemyPrefabs[index];
        }

        private void SpawnIntel()
        {
            if (intelSpawnData == null)
                return;

            if (intelParent == null)
            {
                intelParent = new GameObject("Intel").transform;
            }

            if (intelSpawnData.intelPoints != null)
            {
                foreach (var intelPoint in intelSpawnData.intelPoints)
                {
                    SpawnIntelItem(intelPoint);
                }
            }

            if (intelSpawnData.supplyPoints != null)
            {
                foreach (var supplyPoint in intelSpawnData.supplyPoints)
                {
                    SpawnSupplyPoint(supplyPoint);
                }
            }
        }

        private void SpawnIntelItem(IntelSpawnPoint intelPoint)
        {
            if (intelPrefabs == null || intelPrefabs.Length == 0)
                return;

            int prefabIndex = (int)intelPoint.intelType;
            if (prefabIndex >= intelPrefabs.Length)
                prefabIndex = 0;

            GameObject intel = Instantiate(intelPrefabs[prefabIndex], intelPoint.position, Quaternion.identity, intelParent);
            _intelSpawned++;
        }

        private void SpawnSupplyPoint(SupplyPointData supplyData)
        {
            var supplyPoint = FindObjectOfType<Economy.SupplyPoint>();
            if (supplyPoint != null)
            {
                supplyPoint.transform.position = supplyData.position;
            }
        }

        private void SetupLevelEnvironment()
        {
            if (levelData == null) return;

            if (levelData.hasVentSystem)
            {
                EnableVentSystem();
            }

            if (levelData.hasSurveillanceCameras)
            {
                EnableSurveillanceCameras();
            }

            if (levelData.hasElectronicDoors)
            {
                EnableElectronicDoors();
            }
        }

        private void EnableVentSystem()
        {
        }

        private void EnableSurveillanceCameras()
        {
        }

        private void EnableElectronicDoors()
        {
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