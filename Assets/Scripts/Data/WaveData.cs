using System;

namespace CatBrotato.Data
{
    [Serializable]
    public class WaveData
    {
        public int id;
        public int stageIndex;
        public int waveIndex;
        public string displayName;

        public float duration;

        public int[] normalEnemyPool;
        public int[] eliteEnemyPool;
        public int[] bossEnemyPool;

        public float spawnBudget;
        public float spawnInterval;
        public int maxAliveCount;

        public int rewardMaterial;
        public int rewardChestCount;
        public float shopRefreshBonus;

        public string bgmKey;
        public string mapTag;

        public bool isEliteWave;
        public bool isBossWave;
    }
}
