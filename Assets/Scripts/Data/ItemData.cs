using System;

namespace CatBrotato.Data
{
    [Serializable]
    public class ItemData
    {
        public int id;
        public string internalName;
        public string displayName;

        public ItemCategory itemCategory;
        public string itemSubType;
        public Rarity rarity;

        public EffectType effectType;
        public TriggerType triggerType;

        public string affectedStats;

        public float valueA;
        public float valueB;
        public float valueC;
        public float duration;

        public StackRule stackRule;
        public int maxStack;

        public int basePrice;
        public float shopWeight;

        public string iconKey;
        public string vfxKey;
        public string sfxKey;
    }
}
