using System;

namespace CatBrotato.Data
{
    [Serializable]
    public class EnemyData
    {
        public int id;
        public string internalName;
        public string displayName;

        public ThreatCategory threatCategory;
        public string threatSource;
        public int threatLevel;

        public BehaviorType behaviorType;

        public float baseHp;
        public float moveSpeed;
        public float contactDamage;
        public float attackInterval;
        public float aggroRange;

        public string dropMaterial;

        public float eliteWeight;
        public float bossWeight;

        public string prefabKey;
        public int unlockWave;
    }
}
