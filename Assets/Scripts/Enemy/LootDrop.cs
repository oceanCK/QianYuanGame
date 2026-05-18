using UnityEngine;

namespace CatBrotato.Enemy
{
    public class LootDrop : MonoBehaviour
    {
        [SerializeField] private float value = 1f;
        [SerializeField] private float magnetRange = 2f;
        [SerializeField] private float magnetSpeed = 8f;
        [SerializeField] private float bobAmplitude = 0.15f;
        [SerializeField] private float bobFrequency = 2f;
        [SerializeField] private float lifetime = 15f;

        private Transform playerTransform;
        private Vector2 basePosition;
        private float spawnTime;
        private bool isBeingCollected;

        public float Value => value;

        private void Start()
        {
            basePosition = transform.position;
            spawnTime = Time.time;

            // Find the player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        private void Update()
        {
            if (isBeingCollected) return;

            // Lifetime check
            if (Time.time - spawnTime >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            // Bob animation
            float bobOffset = Mathf.Sin((Time.time - spawnTime) * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
            transform.position = new Vector3(basePosition.x, basePosition.y + bobOffset, transform.position.z);

            // Magnetism: move toward player if close enough
            if (playerTransform != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
                if (distanceToPlayer <= magnetRange)
                {
                    Vector2 direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
                    float moveAmount = magnetSpeed * Time.deltaTime;
                    Vector2 newPos = (Vector2)transform.position + direction * moveAmount;
                    transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
                    basePosition = newPos;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null) return;

            // Collected by player (MaterialCollector or Player tag)
            if (other.CompareTag("Player") || other.GetComponent<MonoBehaviour>() != null && other.gameObject.CompareTag("Player"))
            {
                Collect();
            }
        }

        private void Collect()
        {
            if (isBeingCollected) return;
            isBeingCollected = true;

            // Add materials to GameManager
            if (CatBrotato.Core.GameManager.Instance != null)
            {
                CatBrotato.Core.GameManager.Instance.AddMaterials(Mathf.RoundToInt(value));
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// Spawns loot drop objects at the given position.
        /// </summary>
        /// <param name="position">World position to spawn loot at.</param>
        /// <param name="amount">Number of loot objects to create.</param>
        public static void SpawnLoot(Vector2 position, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                GameObject lootObj = new GameObject("LootDrop");

                // Scatter slightly around the position
                Vector2 offset = Random.insideUnitCircle * 0.5f;
                lootObj.transform.position = new Vector3(position.x + offset.x, position.y + offset.y, 0f);

                // Add SpriteRenderer with a yellow-gold color
                SpriteRenderer sr = lootObj.AddComponent<SpriteRenderer>();
                sr.color = new Color(1f, 0.85f, 0f); // Gold
                sr.sortingOrder = 3;

                // Create a small diamond/square sprite
                Texture2D tex = new Texture2D(8, 8);
                Color[] colors = new Color[64];
                for (int p = 0; p < 64; p++) colors[p] = Color.white;
                tex.SetPixels(colors);
                tex.Apply();
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 16f);

                lootObj.transform.localScale = Vector3.one * 0.4f;

                // Rotate 45 degrees to look like a diamond
                lootObj.transform.rotation = Quaternion.Euler(0f, 0f, 45f);

                // Add collider for pickup
                CircleCollider2D col = lootObj.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.3f;

                // Add Rigidbody2D (kinematic, no gravity)
                Rigidbody2D rb = lootObj.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;

                // Add LootDrop component
                LootDrop loot = lootObj.AddComponent<LootDrop>();
                loot.value = 1f;
            }
        }
    }
}
