using UnityEngine;
using CatBrotato.Data;

namespace CatBrotato.Enemy
{
    /// <summary>
    /// Static factory for creating enemy GameObjects at runtime.
    /// Creates placeholder visuals with colored squares based on enemy type.
    /// </summary>
    public static class EnemyFactory
    {
        // Enemy type colors by internal name
        private static readonly Color StrayDogColor = new Color(0.6f, 0.4f, 0.2f);       // Brown
        private static readonly Color ShadowPuffColor = new Color(0.3f, 0.1f, 0.4f);     // Dark purple
        private static readonly Color AlleySnakeColor = new Color(0.2f, 0.7f, 0.2f);     // Green
        private static readonly Color RobotVacuumColor = new Color(0.5f, 0.5f, 0.5f);    // Gray
        private static readonly Color NoiseSpeakerColor = new Color(1f, 0.6f, 0f);       // Orange
        private static readonly Color DogAlphaColor = new Color(0.5f, 0f, 0f);           // Dark red
        private static readonly Color DefaultEnemyColor = new Color(0.8f, 0.2f, 0.2f);   // Red fallback

        private static readonly Color EliteTint = new Color(1f, 0.9f, 0.3f);             // Yellow tint

        // Cached placeholder sprite (lazily created)
        private static Sprite cachedPlaceholderSprite;

        /// <summary>
        /// Creates a fully configured enemy GameObject at the given position.
        /// </summary>
        /// <param name="data">Enemy configuration data.</param>
        /// <param name="position">World position to spawn at.</param>
        /// <param name="isElite">Whether this is an elite variant.</param>
        /// <param name="isBoss">Whether this is a boss variant.</param>
        /// <returns>The created enemy GameObject, or null on failure.</returns>
        public static GameObject CreateEnemy(EnemyData data, Vector2 position, bool isElite, bool isBoss)
        {
            if (data == null)
            {
                Debug.LogError("EnemyFactory.CreateEnemy: EnemyData is null!");
                return null;
            }

            // Create the root GameObject
            string objectName = data.displayName;
            if (isElite) objectName = "[Elite] " + objectName;
            if (isBoss) objectName = "[BOSS] " + objectName;

            GameObject enemyObj = new GameObject(objectName);
            enemyObj.transform.position = new Vector3(position.x, position.y, 0f);
            enemyObj.tag = "Enemy";
            enemyObj.layer = LayerMask.NameToLayer("Default");

            // SpriteRenderer - placeholder colored square
            SpriteRenderer sr = enemyObj.AddComponent<SpriteRenderer>();
            sr.sprite = GetPlaceholderSprite();
            sr.sortingOrder = 10;

            Color baseColor = GetColorForEnemy(data.internalName);

            // Apply elite yellow tint (blend)
            if (isElite)
            {
                baseColor = Color.Lerp(baseColor, EliteTint, 0.4f);
            }

            sr.color = baseColor;

            // Set scale based on type
            float baseScale = 1f;
            if (isBoss)
            {
                // Boss scale is applied by EnemyBase.Init, but set a base size too
                baseScale = 1.5f;
            }
            else if (isElite)
            {
                baseScale = 1.0f;
            }
            enemyObj.transform.localScale = Vector3.one * baseScale;

            // Rigidbody2D (Kinematic for enemies)
            Rigidbody2D rb = enemyObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            // CircleCollider2D
            CircleCollider2D col = enemyObj.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;

            // EnemyBase component
            EnemyBase enemyBase = enemyObj.AddComponent<EnemyBase>();
            enemyBase.Init(data, isElite, isBoss);

            // Set the original color so flash-white restores correctly
            enemyBase.SetOriginalColor(baseColor);

            // EnemyAI component
            enemyObj.AddComponent<EnemyAI>();

            return enemyObj;
        }

        /// <summary>
        /// Returns the appropriate color for an enemy based on its internal name.
        /// </summary>
        private static Color GetColorForEnemy(string internalName)
        {
            if (string.IsNullOrEmpty(internalName))
                return DefaultEnemyColor;

            string name = internalName.ToLowerInvariant();

            if (name.Contains("stray_dog") || name.Contains("straydog"))
                return StrayDogColor;
            if (name.Contains("shadow_puff") || name.Contains("shadowpuff"))
                return ShadowPuffColor;
            if (name.Contains("alley_snake") || name.Contains("alleysnake"))
                return AlleySnakeColor;
            if (name.Contains("robot_vacuum") || name.Contains("robotvacuum"))
                return RobotVacuumColor;
            if (name.Contains("noise_speaker") || name.Contains("noisespeaker"))
                return NoiseSpeakerColor;
            if (name.Contains("dog_alpha") || name.Contains("dogalpha"))
                return DogAlphaColor;

            return DefaultEnemyColor;
        }

        /// <summary>
        /// Creates or returns a cached placeholder square sprite.
        /// </summary>
        private static Sprite GetPlaceholderSprite()
        {
            if (cachedPlaceholderSprite != null)
                return cachedPlaceholderSprite;

            int size = 32;
            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Point;

            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }
            tex.SetPixels(pixels);
            tex.Apply();

            cachedPlaceholderSprite = Sprite.Create(
                tex,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                32f
            );

            return cachedPlaceholderSprite;
        }
    }
}
