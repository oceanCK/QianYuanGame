using System;

namespace CatBrotato.Data
{
    [Serializable]
    public class WeaponData
    {
        public int id;
        public string internalName;
        public string displayName;

        public WeaponClass weaponClass;
        public WeaponSubType weaponSubType;
        public Rarity rarity;

        public DamageType damageType;
        public TargetingType targetingType;

        public float baseDamage;
        public float damageScale;
        public float cooldown;
        public float range;
        public float projectileSpeed;

        public int pierceCount;
        public int bounceCount;

        public float knockback;
        public float critBonus;
        public float lifeStealBonus;

        public string attackAnimKey;
        public string vfxKey;
        public string sfxKey;

        public int mergeToId;
        public float shopWeight;
    }
}
