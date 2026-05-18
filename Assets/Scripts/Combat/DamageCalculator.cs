using UnityEngine;

namespace CatBrotato.Combat
{
    public static class DamageCalculator
    {
        private const float CritMultiplier = 1.5f;
        private const float MinDamage = 1f;

        /// <summary>
        /// Calculate final damage after armor reduction and crit.
        /// Armor formula: finalDamage = rawDamage * (100 / (100 + armor))
        /// </summary>
        public static float CalculateDamage(float baseDamage, float bonusDamage,
            float targetArmor, float critChance, out bool isCrit)
        {
            float rawDamage = baseDamage + bonusDamage;

            // Armor reduction
            float armorFactor = 100f / (100f + Mathf.Max(targetArmor, 0f));
            float damage = rawDamage * armorFactor;

            // Crit roll
            isCrit = false;
            if (critChance > 0f)
            {
                float roll = Random.Range(0f, 100f);
                if (roll < critChance)
                {
                    isCrit = true;
                    damage *= CritMultiplier;
                }
            }

            // Minimum damage
            damage = Mathf.Max(damage, MinDamage);

            return damage;
        }

        /// <summary>
        /// Roll for dodge. Returns true if the attack is dodged.
        /// </summary>
        public static bool RollDodge(float dodgeChance)
        {
            if (dodgeChance <= 0f) return false;
            float roll = Random.Range(0f, 100f);
            return roll < dodgeChance;
        }
    }
}
