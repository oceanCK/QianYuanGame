using System;

namespace CatBrotato.Data
{
    [Serializable]
    public class CharacterData
    {
        public int id;
        public string internalName;
        public string displayName;

        public CatCategory catCategory;
        public string breedId;

        public FurType furType;
        public string colorType;
        public string patternType;

        public RoleClass roleClass;
        public int starterWeaponId;

        public float baseHp;
        public float baseMoveSpeed;
        public float baseMeleeDamage;
        public float baseRangedDamage;
        public float baseAttackSpeed;
        public float baseCritChance;
        public float baseArmor;
        public float baseDodge;
        public float baseLuck;
        public float baseHarvesting;

        public string passiveDesc;
        public string passiveKey;
        public string unlockCondition;
    }
}
