using UnityEngine;
using CatBrotato.Data;
using CatBrotato.Player;

namespace CatBrotato.Item
{
    /// <summary>
    /// Applies and removes item effects. Supports passive StatBoost effects directly,
    /// and reports trigger-based effects via static helper methods used by ItemManager.
    /// </summary>
    public static class ItemEffectProcessor
    {
        // Special stat key handled separately by ItemManager regen tick.
        public const string HpRegenStat = "hpregen";

        /// <summary>
        /// Applies a single stack of an item's effect to the given PlayerStats.
        /// Returns true if effect was applied through stats system; false if the effect
        /// is trigger-based or handled elsewhere (e.g. hp regen, on-kill, on-damaged).
        /// </summary>
        public static bool ApplyOneStack(ItemInstance item, PlayerStats stats)
        {
            if (item == null || stats == null) return false;
            if (item.EffectType != EffectType.StatBoost) return false;
            if (item.TriggerType != TriggerType.Passive) return false;

            string stat = item.AffectedStats;
            if (string.IsNullOrEmpty(stat)) return false;

            // Hp regen is not in PlayerStats — caller handles it.
            if (IsRegenStat(stat)) return false;

            stats.AddStatBonus(stat, item.ValueA);
            return true;
        }

        /// <summary>
        /// Reverses one stack of an item's effect on the given PlayerStats.
        /// </summary>
        public static bool RemoveOneStack(ItemInstance item, PlayerStats stats)
        {
            if (item == null || stats == null) return false;
            if (item.EffectType != EffectType.StatBoost) return false;
            if (item.TriggerType != TriggerType.Passive) return false;

            string stat = item.AffectedStats;
            if (string.IsNullOrEmpty(stat)) return false;
            if (IsRegenStat(stat)) return false;

            stats.RemoveStatBonus(stat, item.ValueA);
            return true;
        }

        public static bool IsRegenStat(string statName)
        {
            if (string.IsNullOrEmpty(statName)) return false;
            return statName.Equals(HpRegenStat, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns the per-second healing contributed by a single stack of a regen item.
        /// </summary>
        public static float GetRegenPerSecond(ItemInstance item)
        {
            if (item == null) return 0f;
            if (!IsRegenStat(item.AffectedStats)) return 0f;
            return item.ValueA;
        }

        /// <summary>
        /// True if the item should fire when the player kills an enemy.
        /// </summary>
        public static bool IsOnKill(ItemInstance item)
        {
            return item != null && item.TriggerType == TriggerType.OnKill;
        }

        /// <summary>
        /// True if the item should fire when the player takes damage (e.g. revive amulet).
        /// </summary>
        public static bool IsOnDamaged(ItemInstance item)
        {
            return item != null && item.TriggerType == TriggerType.OnDamaged;
        }

        /// <summary>
        /// True if the item should fire when a wave starts (one-shot heal/buff).
        /// </summary>
        public static bool IsOnWaveStart(ItemInstance item)
        {
            return item != null && item.TriggerType == TriggerType.OnWaveStart;
        }
    }
}
