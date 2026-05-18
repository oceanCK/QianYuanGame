using UnityEngine;
using CatBrotato.Data;

namespace CatBrotato.Enemy
{
    [RequireComponent(typeof(EnemyBase))]
    public class EnemyAI : MonoBehaviour
    {
        [SerializeField] private float patrolChangeInterval = 2f;
        [SerializeField] private float circleDistance = 3f;
        [SerializeField] private float fleeDistance = 2f;
        [SerializeField] private float projectileSpeed = 5f;

        private EnemyBase enemyBase;
        private Transform target;
        private float attackTimer;

        // Patrol state
        private Vector2 patrolDirection;
        private float patrolTimer;

        // Circle state
        private float circleAngle;

        // Stationary attack state
        private float rangedAttackTimer;

        private void Start()
        {
            enemyBase = GetComponent<EnemyBase>();

            // Find the player by tag
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }

            // Initialize patrol with a random direction
            patrolDirection = Random.insideUnitCircle.normalized;
            patrolTimer = patrolChangeInterval;

            // Initialize circle angle
            circleAngle = Random.Range(0f, 360f);
        }

        private void FixedUpdate()
        {
            if (enemyBase == null || enemyBase.IsDead) return;
            if (enemyBase.Config == null) return;

            // Decrement attack cooldown timer
            if (attackTimer > 0f)
            {
                attackTimer -= Time.fixedDeltaTime;
            }

            if (rangedAttackTimer > 0f)
            {
                rangedAttackTimer -= Time.fixedDeltaTime;
            }

            if (target == null)
            {
                // Try to find player again
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
                return;
            }

            switch (enemyBase.Config.behaviorType)
            {
                case BehaviorType.Chase:
                    MoveChase();
                    break;
                case BehaviorType.Patrol:
                    MovePatrol();
                    break;
                case BehaviorType.Stationary:
                    MoveStationary();
                    break;
                case BehaviorType.Circle:
                    MoveCircle();
                    break;
                case BehaviorType.Flee:
                    MoveFlee();
                    break;
            }
        }

        private void MoveChase()
        {
            if (target == null || enemyBase.Rb == null) return;

            Vector2 direction = ((Vector2)target.position - enemyBase.Rb.position).normalized;
            Vector2 newPosition = enemyBase.Rb.position + direction * enemyBase.MoveSpeed * Time.fixedDeltaTime;
            enemyBase.Rb.MovePosition(newPosition);
        }

        private void MovePatrol()
        {
            if (enemyBase.Rb == null) return;

            float distanceToPlayer = target != null
                ? Vector2.Distance(enemyBase.Rb.position, target.position)
                : float.MaxValue;

            // Switch to chase if player is within aggro range
            if (distanceToPlayer <= enemyBase.AggroRange && target != null)
            {
                Vector2 direction = ((Vector2)target.position - enemyBase.Rb.position).normalized;
                Vector2 newPosition = enemyBase.Rb.position + direction * enemyBase.MoveSpeed * Time.fixedDeltaTime;
                enemyBase.Rb.MovePosition(newPosition);
                return;
            }

            // Random patrol movement
            patrolTimer -= Time.fixedDeltaTime;
            if (patrolTimer <= 0f)
            {
                patrolDirection = Random.insideUnitCircle.normalized;
                patrolTimer = patrolChangeInterval;
            }

            Vector2 patrolPosition = enemyBase.Rb.position + patrolDirection * enemyBase.MoveSpeed * 0.5f * Time.fixedDeltaTime;
            enemyBase.Rb.MovePosition(patrolPosition);
        }

        private void MoveStationary()
        {
            // Stationary enemies don't move, they shoot projectiles
            if (target == null) return;

            if (rangedAttackTimer <= 0f)
            {
                ShootProjectile();
                rangedAttackTimer = enemyBase.AttackInterval;
            }
        }

        private void MoveCircle()
        {
            if (target == null || enemyBase.Rb == null) return;

            circleAngle += enemyBase.MoveSpeed * 30f * Time.fixedDeltaTime;
            if (circleAngle >= 360f) circleAngle -= 360f;

            float rad = circleAngle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * circleDistance;
            Vector2 targetPosition = (Vector2)target.position + offset;

            Vector2 newPosition = Vector2.MoveTowards(enemyBase.Rb.position, targetPosition, enemyBase.MoveSpeed * Time.fixedDeltaTime);
            enemyBase.Rb.MovePosition(newPosition);
        }

        private void MoveFlee()
        {
            if (target == null || enemyBase.Rb == null) return;

            float distanceToPlayer = Vector2.Distance(enemyBase.Rb.position, target.position);

            if (distanceToPlayer < fleeDistance)
            {
                // Move away from player
                Vector2 direction = (enemyBase.Rb.position - (Vector2)target.position).normalized;
                Vector2 newPosition = enemyBase.Rb.position + direction * enemyBase.MoveSpeed * Time.fixedDeltaTime;
                enemyBase.Rb.MovePosition(newPosition);
            }
        }

        private void ShootProjectile()
        {
            if (target == null) return;

            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;

            GameObject projectileObj = new GameObject("EnemyProjectile");
            projectileObj.transform.position = transform.position;

            SpriteRenderer sr = projectileObj.AddComponent<SpriteRenderer>();
            sr.color = Color.red;
            sr.sortingOrder = 5;

            // Create a small square sprite for the projectile
            Texture2D tex = new Texture2D(8, 8);
            Color[] colors = new Color[64];
            for (int i = 0; i < 64; i++) colors[i] = Color.white;
            tex.SetPixels(colors);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);

            projectileObj.transform.localScale = Vector3.one * 0.3f;

            CircleCollider2D col = projectileObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.3f;

            Rigidbody2D projRb = projectileObj.AddComponent<Rigidbody2D>();
            projRb.gravityScale = 0f;
            projRb.bodyType = RigidbodyType2D.Kinematic;

            EnemyProjectile proj = projectileObj.AddComponent<EnemyProjectile>();
            proj.Init(direction, projectileSpeed, enemyBase.ContactDamage);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (enemyBase == null || enemyBase.IsDead) return;
            if (attackTimer > 0f) return;

            if (collision.gameObject.CompareTag("Player"))
            {
                // Apply contact damage to player
                // The player script should have a method to take damage
                // We send a message to be decoupled from the player implementation
                collision.gameObject.SendMessage("TakeDamage", enemyBase.ContactDamage, SendMessageOptions.DontRequireReceiver);
                attackTimer = enemyBase.AttackInterval;
            }
        }
    }
}
