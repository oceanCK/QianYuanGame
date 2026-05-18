using System;

namespace CatBrotato.Data
{
    [Serializable]
    public class ShopData
    {
        public int id;
        public string internalName;
        public string displayName;

        public string shopType;

        public int unlockWaveMin;
        public int unlockWaveMax;

        public int weaponSlotCount;
        public int itemSlotCount;

        public int freeRefreshCount;
        public int baseRefreshCost;
        public float refreshCostGrowth;
        public float discountRate;
        public float priceMultiplier;

        public float[] rarityWeights;
        public string[] allowedItemCategories;

        public string backgroundKey;
        public string bgmKey;
    }
}
