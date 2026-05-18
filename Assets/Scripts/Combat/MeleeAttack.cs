using UnityEngine;
using CatBrotato.Data;

namespace CatBrotato.Combat
{
    public class MeleeAttack : MonoBehaviour
    {
        [SerializeField] private LayerMask enemyLayerMask = ~0;
        [SerializeField] private GameObject meleeEffectPrefab;
        [SerializeField] private float hitboxDuration = 0.15f;

        /// <summary>
        /// Execute a melee attack toward the target.
        /// </summary>
        public void Execute(WeaponInstance weapon, Transform target,
            float damage, float critChance)
        {
            if (weapon == null || weapon.Config == null) return;

            WeaponData config = weapon.Config;
            Vector2 attackDirection = target != null
                ? ((Vector2)(target.position - transform.position)).normalized
                : Vector2.right;

            Vector2 attackCenter = (Vector2)transform.position
                + attackDirection * (config.range * 0.5f);

            // Find all enemies in melee range
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                attackCenter, config.range * 0.5f, enemyLayerMask);

            for (int i = 0; i < hits.Length; i++)
            {
                if (!hits[i].CompareTag("Enemy")) continue;

                // Calculate damage with crit
                float finalDamage = DamageCalculator.CalculateDamage(
                    damage, 0f, 0f, critChance, out bool isCrit);

                // Apply damage to enemy via IDamageable if available
                IDamageable damageable = hits[i].GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(finalDamage, isCrit);
                }

                // Apply knockback
                if (config.knockback > 0f)
                {
                    Rigidbody2D enemyRb = hits[i].GetComponent<Rigidbody2D>();
                    if (enemyRb != null)
                    {
                        Vector2 knockDir = ((Vector2)hits[i].transform.position
                            - (Vector2)transform.position).normalized;
                        enemyRb.AddForce(knockDir * config.knockback, ForceMode2D.Impulse);
                    }
                }
            }

            // Spawn visual effect
            SpawnEffect(attackCenter, attackDirection);
        }

        private void SpawnEffect(Vector2 position, Vector2 direction)
        {
            if (meleeEffectPrefab == null) return;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            GameObject effect = Instantiate(
                meleeEffectPrefab, position, Quaternion.Euler(0, 0, angle));

            Destroy(effect, hitboxDuration);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }

    /// <summary>
    /// Interface for any object that can receive damage.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float damage, bool isCrit);
    }
}
