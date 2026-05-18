using System.Collections.Generic;
using UnityEngine;
using CatBrotato.Data;
using CatBrotato.Wave;

namespace CatBrotato.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ArenaManager arenaManager;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnInterval = 1f;

        private WaveManager waveManager;
        private readonly List<EnemyBase> aliveEnemies = new List<EnemyBase>();
        private float spawnTimer;
        private float spawnBudgetRemaining;
        private bool isSpawning;

        public int AliveEnemyCount => aliveEnemies.Count;
        public List<EnemyBase> AliveEnemies => aliveEnemies;

        private void Start()
        {
            // Subscribe to enemy death events
            EnemyBase.OnEnemyDeath += HandleEnemyDeath;
        }

        private void OnDestroy()
        {
            EnemyBase.OnEnemyDeath -= HandleEnemyDeath;
        }

        private void Update()
        {
            if (!isSpawning) return;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f && spawnBudgetRemaining > 0f)
            {
                TrySpawnFromWave();
                spawnTimer = spawnInterval;
            }
        }

        /// <summary>
        /// Begins spawning enemies for the given wave configuration.
        /// </summary>
        public void StartSpawning(WaveData waveData)
        {
            if (waveData == null)
            {
                Debug.LogWarning("EnemySpawner.StartSpawning: WaveData is null!");
                return;
            }

            spawnBudgetRemaining = waveData.spawnBudget;
            spawnInterval = waveData.spawnInterval;
            spawnTimer = 0f; // Spawn immediately on first tick
            isSpawning = true;
        }

        /// <summary>
        /// Stops the spawner from creating more enemies.
        /// </summary>
        public void StopSpawning()
        {
            isSpawning = false;
        }

        /// <summary>
        /// Destroys all currently alive enemies.
        /// </summary>
        public void ClearAllEnemies()
        {
            for (int i = aliveEnemies.Count - 1; i >= 0; i--)
            {
                if (aliveEnemies[i] != null)
                {
                    Destroy(aliveEnemies[i].gameObject);
                }
            }
            aliveEnemies.Clear();
        }

        /// <summary>
        /// Spawns a specific enemy by ID at a random edge position.
        /// </summary>
        public EnemyBase SpawnEnemy(int enemyId, bool isElite, bool isBoss)
        {
            // Look up enemy data from a database or registry
            EnemyData data = FindEnemyData(enemyId);
            if (data == null)
            {
                Debug.LogWarning($"EnemySpawner.SpawnEnemy: No EnemyData found for id {enemyId}");
                return null;
            }

            Vector2 spawnPos = Vector2.zero;
            if (arenaManager != null)
            {
                spawnPos = arenaManager.GetRandomSpawnPosition();
            }
            else
            {
                // Fallback: random position if no arena manager
                spawnPos = Random.insideUnitCircle * 10f;
            }

            GameObject enemyObj = EnemyFactory.CreateEnemy(data, spawnPos, isElite, isBoss);
            if (enemyObj == null) return null;

            EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                aliveEnemies.Add(enemy);
            }

            return enemy;
        }

        private void TrySpawnFromWave()
        {
            if (waveManager == null)
            {
                waveManager = FindObjectOfType<WaveManager>();
            }

            WaveData currentWave = waveManager != null ? waveManager.CurrentWaveConfig : null;
            if (currentWave == null) return;

            // Respect max alive count
            if (aliveEnemies.Count >= currentWave.maxAliveCount) return;

            // Determine what to spawn
            bool spawnElite = currentWave.isEliteWave && Random.value < 0.3f;
            bool spawnBoss = false;

            int[] enemyPool;
            if (spawnElite && currentWave.eliteEnemyPool != null && currentWave.eliteEnemyPool.Length > 0)
            {
                enemyPool = currentWave.eliteEnemyPool;
            }
            else if (currentWave.normalEnemyPool != null && currentWave.normalEnemyPool.Length > 0)
            {
                enemyPool = currentWave.normalEnemyPool;
            }
            else
            {
                return;
            }

            int enemyId = enemyPool[Random.Range(0, enemyPool.Length)];
            SpawnEnemy(enemyId, spawnElite, spawnBoss);

            spawnBudgetRemaining -= 1f;
        }

        /// <summary>
        /// Spawns boss enemies for a boss wave.
        /// </summary>
        public void SpawnBossEnemies(WaveData waveData)
        {
            if (waveData == null || waveData.bossEnemyPool == null) return;

            foreach (int bossId in waveData.bossEnemyPool)
            {
                SpawnEnemy(bossId, false, true);
            }
        }

        private void HandleEnemyDeath(EnemyBase enemy)
        {
            if (enemy != null)
            {
                aliveEnemies.Remove(enemy);
            }
        }

        /// <summary>
        /// Finds EnemyData by ID. In a full implementation, this would query a database/registry.
        /// For demo purposes, searches all EnemyData-holding objects or uses a simple lookup.
        /// </summary>
        private EnemyData FindEnemyData(int enemyId)
        {
            // Attempt to find via a static registry if available
            // For now, return a placeholder approach: search all EnemyBase objects for matching config
            // In production, you would use ConfigDatabase<EnemyData> or a ScriptableObject lookup
            return EnemyDataRegistry.GetEnemyData(enemyId);
        }
    }

    /// <summary>
    /// Simple static registry for EnemyData lookups.
    /// Populate this at game start from your ConfigDatabase or JSON data.
    /// </summary>
    public static class EnemyDataRegistry
    {
        private static readonly Dictionary<int, EnemyData> registry = new Dictionary<int, EnemyData>();

        public static void Register(EnemyData data)
        {
            if (data != null && !registry.ContainsKey(data.id))
            {
                registry[data.id] = data;
            }
        }

        public static void RegisterAll(List<EnemyData> dataList)
        {
            if (dataList == null) return;
            foreach (EnemyData data in dataList)
            {
                Register(data);
            }
        }

        public static EnemyData GetEnemyData(int id)
        {
            registry.TryGetValue(id, out EnemyData data);
            return data;
        }

        public static void Clear()
        {
            registry.Clear();
        }
    }
}
