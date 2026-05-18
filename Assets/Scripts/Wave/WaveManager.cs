using System;
using System.Collections.Generic;
using UnityEngine;
using CatBrotato.Core;
using CatBrotato.Data;
using CatBrotato.Enemy;

namespace CatBrotato.Wave
{
    public enum WaveState
    {
        WaitingToStart,
        InProgress,
        WaveComplete,
        BossWave,
        AllWavesComplete
    }

    public class WaveManager : MonoBehaviour
    {
        public event Action<int> OnWaveStarted;
        public event Action<int> OnWaveCompleted;
        public event Action OnAllWavesCompleted;

        [Header("References")]
        [SerializeField] private EnemySpawner enemySpawner;

        [Header("Wave Configuration")]
        [SerializeField] private List<WaveData> allWaves = new List<WaveData>();
        [SerializeField] private int totalWaves = 20;

        [Header("Runtime State")]
        [SerializeField] private int currentWaveIndex;
        [SerializeField] private int currentStageIndex;
        [SerializeField] private WaveState currentState = WaveState.WaitingToStart;
        [SerializeField] private float waveTimer;

        private WaveData currentWaveConfig;
        private int enemiesKilledThisWave;
        private int materialsEarnedThisWave;

        public WaveData CurrentWaveConfig => currentWaveConfig;
        public int CurrentWaveIndex => currentWaveIndex;
        public int CurrentStageIndex => currentStageIndex;
        public WaveState CurrentState => currentState;
        public float WaveTimer => waveTimer;
        public int EnemiesKilledThisWave => enemiesKilledThisWave;
        public int MaterialsEarnedThisWave => materialsEarnedThisWave;

        private void Start()
        {
            EnemyBase.OnEnemyDeath += HandleEnemyDeath;
        }

        private void OnDestroy()
        {
            EnemyBase.OnEnemyDeath -= HandleEnemyDeath;
        }

        private void Update()
        {
            switch (currentState)
            {
                case WaveState.WaitingToStart:
                    // Waiting for external call to StartNextWave()
                    break;

                case WaveState.InProgress:
                    UpdateWaveInProgress();
                    break;

                case WaveState.BossWave:
                    UpdateBossWave();
                    break;

                case WaveState.WaveComplete:
                    // Waiting for transition to shop or next wave
                    break;

                case WaveState.AllWavesComplete:
                    // Game is complete
                    break;
            }
        }

        private void UpdateWaveInProgress()
        {
            if (currentWaveConfig == null) return;

            waveTimer -= Time.deltaTime;

            // Wave end conditions: timer expires OR all enemies killed and budget exhausted
            bool timerExpired = waveTimer <= 0f;
            bool allEnemiesCleared = enemySpawner != null
                && enemySpawner.AliveEnemyCount == 0
                && waveTimer < currentWaveConfig.duration * 0.5f; // At least half the wave must pass

            if (timerExpired || allEnemiesCleared)
            {
                CompleteWave();
            }
        }

        private void UpdateBossWave()
        {
            // Boss wave ends when all boss enemies are dead
            if (enemySpawner != null && enemySpawner.AliveEnemyCount == 0)
            {
                CompleteWave();
            }
        }

        /// <summary>
        /// Starts the next wave. Call this from GameManager or UI.
        /// </summary>
        public void StartNextWave()
        {
            if (currentState == WaveState.AllWavesComplete) return;

            currentWaveIndex++;
            enemiesKilledThisWave = 0;
            materialsEarnedThisWave = 0;

            // Try to find wave data for this wave index
            currentWaveConfig = FindWaveData(currentWaveIndex);

            if (currentWaveConfig == null)
            {
                // Generate a default wave config if none exists
                currentWaveConfig = CreateDefaultWaveData(currentWaveIndex);
            }

            currentStageIndex = currentWaveConfig.stageIndex;
            waveTimer = currentWaveConfig.duration;

            // Determine if this is a boss wave
            if (currentWaveConfig.isBossWave)
            {
                currentState = WaveState.BossWave;
                if (enemySpawner != null)
                {
                    enemySpawner.SpawnBossEnemies(currentWaveConfig);
                }
            }
            else
            {
                currentState = WaveState.InProgress;
                if (enemySpawner != null)
                {
                    enemySpawner.StartSpawning(currentWaveConfig);
                }
            }

            OnWaveStarted?.Invoke(currentWaveIndex);

            Debug.Log($"Wave {currentWaveIndex} started! Duration: {waveTimer}s, Boss: {currentWaveConfig.isBossWave}");
        }

        private void CompleteWave()
        {
            if (enemySpawner != null)
            {
                enemySpawner.StopSpawning();
                enemySpawner.ClearAllEnemies();
            }

            // Grant wave completion rewards
            if (currentWaveConfig != null && GameManager.Instance != null)
            {
                GameManager.Instance.AddMaterials(currentWaveConfig.rewardMaterial);
                materialsEarnedThisWave += currentWaveConfig.rewardMaterial;
            }

            OnWaveCompleted?.Invoke(currentWaveIndex);

            // Check if all waves are done
            if (currentWaveIndex >= totalWaves)
            {
                currentState = WaveState.AllWavesComplete;
                OnAllWavesCompleted?.Invoke();
                Debug.Log("All waves completed! You win!");
                return;
            }

            currentState = WaveState.WaveComplete;

            // Trigger shop between waves
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EnterShop();
            }

            Debug.Log($"Wave {currentWaveIndex} completed! Enemies killed: {enemiesKilledThisWave}, Materials: {materialsEarnedThisWave}");
        }

        private void HandleEnemyDeath(EnemyBase enemy)
        {
            if (currentState == WaveState.InProgress || currentState == WaveState.BossWave)
            {
                enemiesKilledThisWave++;
            }
        }

        /// <summary>
        /// Returns information about the current wave for UI display.
        /// </summary>
        public WaveInfo GetCurrentWaveInfo()
        {
            return new WaveInfo
            {
                waveIndex = currentWaveIndex,
                stageIndex = currentStageIndex,
                timeRemaining = waveTimer,
                enemiesAlive = enemySpawner != null ? enemySpawner.AliveEnemyCount : 0,
                enemiesKilled = enemiesKilledThisWave,
                isBossWave = currentWaveConfig != null && currentWaveConfig.isBossWave,
                state = currentState
            };
        }

        private WaveData FindWaveData(int waveIndex)
        {
            if (allWaves == null) return null;

            foreach (WaveData wave in allWaves)
            {
                if (wave != null && wave.waveIndex == waveIndex)
                {
                    return wave;
                }
            }
            return null;
        }

        /// <summary>
        /// Creates a procedurally generated wave when no pre-authored data exists.
        /// </summary>
        private WaveData CreateDefaultWaveData(int waveIndex)
        {
            WaveData wave = new WaveData
            {
                id = waveIndex,
                stageIndex = (waveIndex - 1) / 5,
                waveIndex = waveIndex,
                displayName = $"Wave {waveIndex}",
                duration = 30f + waveIndex * 2f,
                normalEnemyPool = new int[] { 0, 1, 2 },
                eliteEnemyPool = new int[] { 0, 1 },
                bossEnemyPool = new int[] { 5 },
                spawnBudget = 10f + waveIndex * 3f,
                spawnInterval = Mathf.Max(0.5f, 2f - waveIndex * 0.05f),
                maxAliveCount = 15 + waveIndex * 2,
                rewardMaterial = 10 + waveIndex * 5,
                rewardChestCount = waveIndex % 5 == 0 ? 1 : 0,
                shopRefreshBonus = 0f,
                bgmKey = "",
                mapTag = "default",
                isEliteWave = waveIndex % 3 == 0,
                isBossWave = waveIndex % 5 == 0
            };

            return wave;
        }

        /// <summary>
        /// Loads wave data from a list (called during game initialization).
        /// </summary>
        public void LoadWaveData(List<WaveData> waves)
        {
            if (waves != null)
            {
                allWaves = new List<WaveData>(waves);
            }
        }
    }

    /// <summary>
    /// Struct for UI-friendly wave information.
    /// </summary>
    [Serializable]
    public struct WaveInfo
    {
        public int waveIndex;
        public int stageIndex;
        public float timeRemaining;
        public int enemiesAlive;
        public int enemiesKilled;
        public bool isBossWave;
        public WaveState state;
    }
}
