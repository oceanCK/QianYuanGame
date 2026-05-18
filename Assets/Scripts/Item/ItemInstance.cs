using System;
using CatBrotato.Data;

namespace CatBrotato.Item
{
    [Serializable]
    public class ItemInstance
    {
        public ItemData Config { get; private set; }
        public int StackCount { get; private set; }

        public int Id => Config != null ? Config.id : 0;
        public string DisplayName => Config != null ? Config.displayName : string.Empty;
        public Rarity Rarity => Config != null ? Config.rarity : Rarity.Common;
        public ItemCategory Category => Config != null ? Config.itemCategory : ItemCategory.Food;
        public EffectType EffectType => Config != null ? Config.effectType : EffectType.StatBoost;
        public TriggerType TriggerType => Config != null ? Config.triggerType : TriggerType.Passive;
        public StackRule StackRule => Config != null ? Config.stackRule : StackRule.Additive;
        public int MaxStack => Config != null ? Config.maxStack : 1;
        public string AffectedStats => Config != null ? Config.affectedStats : string.Empty;
        public float ValueA => Config != null ? Config.valueA : 0f;
        public float ValueB => Config != null ? Config.valueB : 0f;
        public float ValueC => Config != null ? Config.valueC : 0f;
        public float Duration => Config != null ? Config.duration : 0f;

        public ItemInstance(ItemData data, int stackCount = 1)
        {
            Config = data;
            StackCount = stackCount;
        }

        public bool CanStack()
        {
            if (Config == null) return false;
            if (StackRule == StackRule.Unique) return StackCount < 1;
            return StackCount < MaxStack;
        }

        public void IncrementStack()
        {
            if (CanStack())
            {
                StackCount++;
            }
        }

        public void SetStack(int newStack)
        {
            StackCount = newStack < 0 ? 0 : newStack;
        }

        public float GetEffectiveValue(float baseValue)
        {
            switch (StackRule)
            {
                case StackRule.Additive:
                    return baseValue * StackCount;
                case StackRule.Multiplicative:
                    return baseValue * (StackCount > 0 ? StackCount : 1);
                case StackRule.Unique:
                default:
                    return baseValue;
            }
        }
    }
}
