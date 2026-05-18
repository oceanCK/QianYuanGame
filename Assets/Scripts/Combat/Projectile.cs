using UnityEngine;

namespace CatBrotato.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifespan = 3f;
        [SerializeField] private float bounceDetectionRange = 8f;
        [SerializeField] private LayerMask enemyLayerMask = ~0;

        private Vector2 direction;
        private float speed;
        private float damage;
        private float knockback;
        private int pierceCount;
        private int bounceCount;
        private bool isCrit;

        private float lifetimeTimer;
        private Rigidbody2D rb;
        private bool isInitialized;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
        }

        public void Init(Vector2 direction, float speed, float damage,
            float knockback, int pierceCount, int bounceCount, bool isCrit)
        {
            this.direction = direction.normalized;
            this.speed = speed;
            this.damage = damage;
            this.knockback = knockback;
            this.pierceCount = pierceCount;
            this.bounceCount = bounceCount;
            this.isCrit = isCrit;

            lifetimeTimer = lifespan;
            isInitialized = true;
        }

        private void FixedUpdate()
        {
            if (!isInitialized) return;

            rb.linearVelocity = direction * speed;
        }

        private void Update()
        {
            if (!isInitialized) return;

            lifetimeTimer -= Time.deltaTime;
            if (lifetimeTimer <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isInitialized) return;
            if (!other.CompareTag("Enemy")) return;

            // Apply damage
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, isCrit);
            }

            // Apply knockback
            if (knockback > 0f)
            {
                Rigidbody2D enemyRb = other.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 knockDir = ((Vector2)other.transform.position
                        - (Vector2)transform.position).normalized;
                    enemyRb.AddForce(knockDir * knockback, ForceMode2D.Impulse);
                }
            }

            // Handle pierce
            if (pierceCount > 0)
            {
                pierceCount--;
                return; // Continue without destroying
            }

            // Handle bounce
            if (bounceCount > 0)
            {
                bounceCount--;
                Transform newTarget = FindBounceTarget(other.transform);
                if (newTarget != null)
                {
                    direction = ((Vector2)newTarget.position
                        - (Vector2)transform.position).normalized;
                    return; // Redirect without destroying
                }
            }

            // No pierce or bounce remaining, destroy
            Destroy(gameObject);
        }

        private Transform FindBounceTarget(Transform hitEnemy)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position, bounceDetectionRange, enemyLayerMask);

            Transform nearest = null;
            float nearestDist = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                if (!hits[i].CompareTag("Enemy")) continue;
                if (hits[i].transform == hitEnemy) continue;

                float dist = Vector2.Distance(
                    transform.position, hits[i].transform.position);

                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = hits[i].transform;
                }
            }

            return nearest;
        }
    }
}
