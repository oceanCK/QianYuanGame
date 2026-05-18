using CatBrotato.Data;

namespace CatBrotato.Combat
{
    [System.Serializable]
    public class WeaponInstance
    {
        public WeaponData Config { get; private set; }
        public float CurrentCooldown { get; private set; }
        public int SlotIndex { get; set; }

        public WeaponInstance(WeaponData config, int slotIndex)
        {
            Config = config;
            SlotIndex = slotIndex;
            CurrentCooldown = 0f;
        }

        public bool CanAttack()
        {
            return CurrentCooldown <= 0f;
        }

        public void ResetCooldown()
        {
            CurrentCooldown = Config.cooldown;
        }

        public void TickCooldown(float deltaTime)
        {
            if (CurrentCooldown > 0f)
            {
                CurrentCooldown -= deltaTime;
            }
        }

        /// <summary>
        /// Calculate effective damage using player stats.
        /// </summary>
        public float CalculateEffectiveDamage(float playerBonusDamage, float attackSpeedMultiplier)
        {
            float baseDmg = Config.baseDamage + Config.baseDamage * Config.damageScale;
            float totalDamage = baseDmg + playerBonusDamage;
            return totalDamage;
        }

        /// <summary>
        /// Get effective cooldown factoring in attack speed.
        /// </summary>
        public float GetEffectiveCooldown(float attackSpeedBonus)
        {
            float speedMultiplier = 1f + attackSpeedBonus;
            if (speedMultiplier <= 0.1f) speedMultiplier = 0.1f;
            return Config.cooldown / speedMultiplier;
        }
    }
}
