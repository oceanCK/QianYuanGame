using System;
using System.Collections;
using UnityEngine;
using CatBrotato.Data;

namespace CatBrotato.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class EnemyBase : MonoBehaviour
    {
        public static event Action<EnemyBase> OnEnemyDeath;

        [Header("Runtime Stats")]
        [SerializeField] private float currentHp;
        [SerializeField] private float maxHp;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float contactDamage;
        [SerializeField] private float attackInterval;
        [SerializeField] private float aggroRange;
        [SerializeField] private bool isElite;
        [SerializeField] private bool isBoss;

        private const float EliteHpMultiplier = 2f;
        private const float EliteDamageMultiplier = 1.5f;
        private const float EliteSizeMultiplier = 1.3f;

        private const float BossHpMultiplier = 10f;
        private const float BossDamageMultiplier = 3f;
        private const float BossSizeMultiplier = 2f;

        private EnemyData config;
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private bool isDead;

        public EnemyData Config => config;
        public float CurrentHp => currentHp;
        public float MaxHp => maxHp;
        public float MoveSpeed => moveSpeed;
        public float ContactDamage => contactDamage;
        public float AttackInterval => attackInterval;
        public float AggroRange => aggroRange;
        public bool IsElite => isElite;
        public bool IsBoss => isBoss;
        public bool IsDead => isDead;
        public Rigidbody2D Rb => rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }

        public void Init(EnemyData data, bool elite, bool boss)
        {
            if (data == null)
            {
                Debug.LogError("EnemyBase.Init: EnemyData is null!");
                return;
            }

            config = data;
            isElite = elite;
            isBoss = boss;
            isDead = false;

            // Base stats
            maxHp = data.baseHp;
            moveSpeed = data.moveSpeed;
            contactDamage = data.contactDamage;
            attackInterval = data.attackInterval;
            aggroRange = data.aggroRange;

            // Apply elite multipliers
            if (isElite)
            {
                maxHp *= EliteHpMultiplier;
                contactDamage *= EliteDamageMultiplier;
                transform.localScale *= EliteSizeMultiplier;
            }

            // Apply boss multipliers (overrides elite if both set)
            if (isBoss)
            {
                maxHp *= BossHpMultiplier;
                contactDamage *= BossDamageMultiplier;
                transform.localScale *= BossSizeMultiplier;
            }

            currentHp = maxHp;

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        public void TakeDamage(float damage, Vector2 knockbackDir, float knockbackForce)
        {
            if (isDead) return;

            currentHp -= damage;

            // Apply knockback
            if (knockbackForce > 0f && rb != null)
            {
                rb.MovePosition(rb.position + knockbackDir.normalized * knockbackForce * Time.fixedDeltaTime);
            }

            // Flash white on hit
            if (spriteRenderer != null)
            {
                StartCoroutine(FlashWhite());
            }

            if (currentHp <= 0f)
            {
                Die();
            }
        }

        public void TakeDamage(float damage)
        {
            TakeDamage(damage, Vector2.zero, 0f);
        }

        private IEnumerator FlashWhite()
        {
            if (spriteRenderer == null) yield break;

            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);

            if (spriteRenderer != null && !isDead)
            {
                spriteRenderer.color = originalColor;
            }
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            // Spawn loot drops
            SpawnLootOnDeath();

            // Notify listeners
            OnEnemyDeath?.Invoke(this);

            // Destroy the enemy object
            Destroy(gameObject);
        }

        private void SpawnLootOnDeath()
        {
            if (config == null) return;

            // Determine loot amount based on enemy type
            int lootAmount = 1;
            if (isElite) lootAmount = 3;
            if (isBoss) lootAmount = 10;

            LootDrop.SpawnLoot(transform.position, lootAmount);
        }

        public void SetOriginalColor(Color color)
        {
            originalColor = color;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }
    }
}
