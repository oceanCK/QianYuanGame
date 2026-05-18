using UnityEngine;
using CatBrotato.Data;

namespace CatBrotato.Combat
{
    public class RangedAttack : MonoBehaviour
    {
        [SerializeField] private GameObject defaultProjectilePrefab;

        /// <summary>
        /// Spawn a projectile aimed at the target.
        /// </summary>
        public void Execute(WeaponInstance weapon, Transform target,
            float damage, float critChance)
        {
            if (weapon == null || weapon.Config == null) return;
            if (defaultProjectilePrefab == null)
            {
                Debug.LogWarning("RangedAttack: No projectile prefab assigned.");
                return;
            }

            WeaponData config = weapon.Config;

            Vector2 spawnPos = transform.position;
            Vector2 direction;

            if (target != null)
            {
                direction = ((Vector2)target.position - spawnPos).normalized;
            }
            else
            {
                direction = Vector2.right;
            }

            // Spawn projectile
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            GameObject projectileObj = Instantiate(
                defaultProjectilePrefab, spawnPos, Quaternion.Euler(0, 0, angle));

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                // Calculate final damage with crit
                float finalDamage = DamageCalculator.CalculateDamage(
                    damage, 0f, 0f, critChance, out bool isCrit);

                projectile.Init(
                    direction: direction,
                    speed: config.projectileSpeed,
                    damage: finalDamage,
                    knockback: config.knockback,
                    pierceCount: config.pierceCount,
                    bounceCount: config.bounceCount,
                    isCrit: isCrit
                );
            }
            else
            {
                Debug.LogWarning("RangedAttack: Projectile prefab missing Projectile component.");
                Destroy(projectileObj);
            }
        }
    }
}
