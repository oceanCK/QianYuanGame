using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CatBrotato.Core;
using CatBrotato.Combat;
using CatBrotato.Data;
using CatBrotato.Enemy;
using CatBrotato.Item;
using CatBrotato.Player;
using CatBrotato.Shop;
using CatBrotato.UI;
using CatBrotato.Wave;

namespace CatBrotato
{
    /// <summary>
    /// Runtime bootstrap. Place a single empty GameObject with this component in the
    /// startup scene; it spawns every manager, the player rig, a procedural projectile
    /// prefab, the UI canvas, and wires every cross-system binding.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class SceneSetup : MonoBehaviour
    {
        private GameObject playerGo;
        private PlayerStats playerStats;
        private PlayerHealth playerHealth;
        private WeaponHolder weaponHolder;
        private WaveManager waveManager;
        private bool playerInitialized;

        private void Awake()
        {
            EnsureTags();
            BuildCamera();
            BuildManagers();
            BuildPlayer();
            BuildUI();
            HookGameState();
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Menu);
            }
        }

        private static void EnsureTags()
        {
            // Tags must already exist in the Unity project's TagManager.
            // Required: "Player", "Enemy", "Material".
        }

        private void BuildCamera()
        {
            if (Camera.main != null) return;
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 8f;
            cam.backgroundColor = new Color(0.07f, 0.06f, 0.10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();
        }

        private void BuildManagers()
        {
            GameObject managers = new GameObject("Managers");
            managers.AddComponent<ConfigManager>();
            managers.AddComponent<GameManager>();
            managers.AddComponent<ShopManager>();
            managers.AddComponent<UIManager>();

            GameObject waveGo = new GameObject("WaveSystem");
            ArenaManager arena = waveGo.AddComponent<ArenaManager>();
            EnemySpawner spawner = waveGo.AddComponent<EnemySpawner>();
            waveManager = waveGo.AddComponent<WaveManager>();

            InjectPrivateField(spawner, "arenaManager", arena);
            InjectPrivateField(waveManager, "enemySpawner", spawner);

            if (ConfigManager.Instance != null)
            {
                EnemyDataRegistry.RegisterAll(ConfigManager.Instance.GetAllEnemies());
                waveManager.LoadWaveData(ConfigManager.Instance.GetAllWaves());
            }
        }

        private void BuildPlayer()
        {
            playerGo = new GameObject("Player");
            playerGo.tag = "Player";
            playerGo.transform.position = Vector3.zero;

            Rigidbody2D rb = playerGo.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CircleCollider2D body = playerGo.AddComponent<CircleCollider2D>();
            body.radius = 0.35f;
            body.isTrigger = false;

            SpriteRenderer sr = playerGo.AddComponent<SpriteRenderer>();
            sr.sprite = BuildCircleSprite(64, new Color(1f, 0.9f, 0.55f));
            sr.sortingOrder = 10;

            playerStats = playerGo.AddComponent<PlayerStats>();
            playerHealth = playerGo.AddComponent<PlayerHealth>();

            GameObject pickup = new GameObject("PickupRange");
            pickup.transform.SetParent(playerGo.transform, false);
            pickup.AddComponent<MaterialCollector>();

            MeleeAttack melee = playerGo.AddComponent<MeleeAttack>();
            RangedAttack ranged = playerGo.AddComponent<RangedAttack>();
            weaponHolder = playerGo.AddComponent<WeaponHolder>();
            playerGo.AddComponent<AutoAttackSystem>();
            playerGo.AddComponent<ItemManager>();
            playerGo.AddComponent<PlayerController>();

            GameObject projectilePrefab = BuildProjectilePrefab();
            InjectPrivateField(ranged, "defaultProjectilePrefab", projectilePrefab);
            InjectPrivateField(weaponHolder, "meleeAttack", melee);
            InjectPrivateField(weaponHolder, "rangedAttack", ranged);

            playerGo.SetActive(false);
        }

        private static GameObject BuildProjectilePrefab()
        {
            GameObject go = new GameObject("Projectile");
            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.15f;
            col.isTrigger = true;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = BuildCircleSprite(16, new Color(1f, 0.8f, 0.3f));
            sr.sortingOrder = 20;
            go.AddComponent<Projectile>();
            go.SetActive(false);
            return go;
        }

        private static Sprite BuildCircleSprite(int size, Color color)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - r + 0.5f;
                    float dy = y - r + 0.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(r - d);
                    Color c = color;
                    c.a *= a;
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private void BuildUI()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            GameObject canvasGo = new GameObject("UICanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            MainMenuUI menu = CreatePanel<MainMenuUI>("MainMenu", canvasGo.transform);
            CharacterSelectUI select = CreatePanel<CharacterSelectUI>("CharacterSelect", canvasGo.transform);
            BattleHUD hud = CreatePanel<BattleHUD>("BattleHUD", canvasGo.transform);
            ShopUI shop = CreatePanel<ShopUI>("ShopUI", canvasGo.transform);
            ResultsUI results = CreatePanel<ResultsUI>("Results", canvasGo.transform);

            hud.Bind(playerHealth, weaponHolder, playerGo.GetComponent<ItemManager>(), waveManager);
            results.Bind(waveManager);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.RegisterPanels(menu, select, hud, shop, results);
            }

            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.BindPlayer(weaponHolder, playerGo.GetComponent<ItemManager>());
            }
        }

        private static T CreatePanel<T>(string name, Transform parent) where T : MonoBehaviour
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            T comp = go.AddComponent<T>();
            go.SetActive(false);
            return comp;
        }

        private void HookGameState()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.OnGameStateChanged += HandleStateChanged;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(GameState previous, GameState current)
        {
            if (current == GameState.Battle && !playerInitialized)
            {
                InitializePlayerForRun();
            }
            else if (current == GameState.Menu)
            {
                playerInitialized = false;
                if (playerGo != null) playerGo.SetActive(false);
            }

            bool inGameplay = current == GameState.Battle || current == GameState.Shop;
            if (playerGo != null && playerInitialized)
            {
                playerGo.SetActive(inGameplay);
            }

            if (current == GameState.Battle && waveManager != null && playerInitialized)
            {
                waveManager.StartNextWave();
            }
        }

        private void InitializePlayerForRun()
        {
            if (GameManager.Instance == null || ConfigManager.Instance == null) return;

            CharacterData charData = ConfigManager.Instance.GetCharacter(GameManager.Instance.SelectedCharacterId);
            if (charData == null)
            {
                Debug.LogError($"SceneSetup: Character {GameManager.Instance.SelectedCharacterId} not found.");
                return;
            }

            playerStats.InitFromCharacter(charData);
            playerHealth.Init(playerStats.MaxHp);

            for (int i = weaponHolder.EquippedWeapons.Count - 1; i >= 0; i--)
            {
                weaponHolder.RemoveWeapon(i);
            }
            WeaponData starter = ConfigManager.Instance.GetWeapon(charData.starterWeaponId);
            if (starter != null) weaponHolder.EquipWeapon(starter);

            playerGo.transform.position = Vector3.zero;
            playerGo.SetActive(true);
            playerInitialized = true;
        }

        private static void InjectPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"SceneSetup: Field '{fieldName}' not found on {target.GetType().Name}.");
            }
        }
    }
}
