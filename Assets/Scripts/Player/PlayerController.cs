using UnityEngine;
using CatBrotato.Core;

namespace CatBrotato.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Rigidbody2D rb;
        private PlayerStats playerStats;
        private PlayerHealth playerHealth;
        private Combat.WeaponHolder weaponHolder;

        private Vector2 moveInput;
        private bool isDead;
        private bool isInvulnerable;
        private float invulnerabilityTimer;

        private const float InvulnerabilityDuration = 0.5f;

        public PlayerStats Stats => playerStats;
        public PlayerHealth Health => playerHealth;
        public Combat.WeaponHolder WeaponHolder => weaponHolder;
        public Vector2 FacingDirection { get; private set; } = Vector2.right;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            playerStats = GetComponent<PlayerStats>();
            playerHealth = GetComponent<PlayerHealth>();
            weaponHolder = GetComponent<Combat.WeaponHolder>();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDeath += HandleDeath;
                playerHealth.OnDamaged += HandleDamaged;
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDeath -= HandleDeath;
                playerHealth.OnDamaged -= HandleDamaged;
            }
        }

        private void Update()
        {
            if (isDead) return;

            // Read input
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(horizontal, vertical).normalized;

            // Update facing direction
            if (moveInput.sqrMagnitude > 0.01f)
            {
                FacingDirection = moveInput.normalized;
                FlipSprite(moveInput.x);
            }

            // Handle invulnerability timer
            if (isInvulnerable)
            {
                invulnerabilityTimer -= Time.deltaTime;
                if (invulnerabilityTimer <= 0f)
                {
                    isInvulnerable = false;
                    playerHealth.SetInvulnerable(false);
                }
            }
        }

        private void FixedUpdate()
        {
            if (isDead) return;

            float speed = playerStats != null ? playerStats.MoveSpeed : 5f;
            rb.linearVelocity = moveInput * speed;
        }

        private void FlipSprite(float horizontalInput)
        {
            if (spriteRenderer == null) return;

            if (horizontalInput < -0.01f)
            {
                spriteRenderer.flipX = true;
            }
            else if (horizontalInput > 0.01f)
            {
                spriteRenderer.flipX = false;
            }
        }

        private void HandleDamaged(float damage)
        {
            if (isDead) return;

            // Start invulnerability frames
            isInvulnerable = true;
            invulnerabilityTimer = InvulnerabilityDuration;
            playerHealth.SetInvulnerable(true);
        }

        private void HandleDeath()
        {
            isDead = true;
            rb.linearVelocity = Vector2.zero;
            moveInput = Vector2.zero;

            // Trigger game over through GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }

        public void ResetPlayer()
        {
            isDead = false;
            isInvulnerable = false;
            invulnerabilityTimer = 0f;
            rb.linearVelocity = Vector2.zero;
        }
    }
}
