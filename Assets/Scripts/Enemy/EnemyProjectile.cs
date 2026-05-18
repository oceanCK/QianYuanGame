using UnityEngine;

namespace CatBrotato.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private float damage = 1f;
        [SerializeField] private float lifetime = 5f;

        private Vector2 direction;
        private Rigidbody2D rb;
        private float timer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Init(Vector2 dir, float projSpeed, float projDamage)
        {
            direction = dir.normalized;
            speed = projSpeed;
            damage = projDamage;
            timer = 0f;
        }

        private void FixedUpdate()
        {
            if (rb != null)
            {
                rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
            }

            timer += Time.fixedDeltaTime;
            if (timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null) return;

            if (other.CompareTag("Player"))
            {
                // Apply damage to the player
                other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                Destroy(gameObject);
            }

            // Destroy if hitting a wall or obstacle
            if (other.CompareTag("Wall"))
            {
                Destroy(gameObject);
            }
        }
    }
}
