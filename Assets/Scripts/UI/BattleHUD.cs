using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CatBrotato.Core;
using CatBrotato.Player;
using CatBrotato.Combat;
using CatBrotato.Item;
using CatBrotato.Wave;

namespace CatBrotato.UI
{
    /// <summary>
    /// In-battle HUD: HP bar, wave timer, materials, kills, equipped weapons strip,
    /// and an items strip. Subscribes to PlayerHealth, GameManager, and WaveManager events.
    /// </summary>
    public class BattleHUD : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private WeaponHolder weaponHolder;
        [SerializeField] private ItemManager itemManager;
        [SerializeField] private WaveManager waveManager;

        private Image hpFill;
        private Text hpText;
        private Text waveLabel;
        private Text timerLabel;
        private Text materialsLabel;
        private Text killsLabel;
        private RectTransform weaponStrip;
        private RectTransform itemStrip;

        private void Awake()
        {
            BuildUI();
        }

        private void OnEnable()
        {
            HookPlayerEvents();
            HookManagerEvents();
            RefreshAll();
        }

        private void OnDisable()
        {
            UnhookPlayerEvents();
            UnhookManagerEvents();
        }

        private void Update()
        {
            if (waveManager != null && timerLabel != null)
            {
                float t = Mathf.Max(0f, waveManager.WaveTimer);
                timerLabel.text = $"⏱ {Mathf.CeilToInt(t)}s";
            }
            if (GameManager.Instance != null && materialsLabel != null)
            {
                materialsLabel.text = $"★ {GameManager.Instance.PlayerMaterials}";
            }
        }

        public void Bind(PlayerHealth health, WeaponHolder holder, ItemManager items, WaveManager waves)
        {
            UnhookPlayerEvents();
            UnhookManagerEvents();
            playerHealth = health;
            weaponHolder = holder;
            itemManager = items;
            waveManager = waves;
            HookPlayerEvents();
            HookManagerEvents();
            RefreshAll();
        }

        private void BuildUI()
        {
            // Top bar background
            GameObject topBar = UIBuilder.CreatePanel("TopBar", transform, new Color(0f, 0f, 0f, 0.45f));
            RectTransform topRt = topBar.GetComponent<RectTransform>();
            UIBuilder.Anchor(topRt, new Vector2(0f, 0.92f), Vector2.one, Vector2.zero, Vector2.zero);

            waveLabel = UIBuilder.CreateText("WaveLabel", topBar.transform, "Wave 1", 26, TextAnchor.MiddleLeft, Color.white);
            UIBuilder.Anchor(waveLabel.rectTransform, new Vector2(0.02f, 0f), new Vector2(0.20f, 1f), Vector2.zero, Vector2.zero);

            timerLabel = UIBuilder.CreateText("Timer", topBar.transform, "⏱ —", 26, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.6f));
            UIBuilder.Anchor(timerLabel.rectTransform, new Vector2(0.40f, 0f), new Vector2(0.60f, 1f), Vector2.zero, Vector2.zero);

            materialsLabel = UIBuilder.CreateText("Materials", topBar.transform, "★ 0", 24, TextAnchor.MiddleRight, new Color(1f, 0.85f, 0.4f));
            UIBuilder.Anchor(materialsLabel.rectTransform, new Vector2(0.70f, 0f), new Vector2(0.86f, 1f), Vector2.zero, new Vector2(-10f, 0f));

            killsLabel = UIBuilder.CreateText("Kills", topBar.transform, "💀 0", 22, TextAnchor.MiddleRight, new Color(0.85f, 0.85f, 0.95f));
            UIBuilder.Anchor(killsLabel.rectTransform, new Vector2(0.86f, 0f), new Vector2(0.99f, 1f), Vector2.zero, new Vector2(-10f, 0f));

            // HP bar
            GameObject hpBg = UIBuilder.CreatePanel("HpBackground", transform, new Color(0.15f, 0.05f, 0.05f, 0.85f));
            RectTransform hpBgRt = hpBg.GetComponent<RectTransform>();
            UIBuilder.Anchor(hpBgRt, new Vector2(0.02f, 0.86f), new Vector2(0.30f, 0.90f), Vector2.zero, Vector2.zero);

            GameObject hpFillGo = new GameObject("HpFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            hpFillGo.transform.SetParent(hpBg.transform, false);
            hpFill = hpFillGo.GetComponent<Image>();
            hpFill.color = new Color(0.85f, 0.25f, 0.30f, 1f);
            hpFill.type = Image.Type.Filled;
            hpFill.fillMethod = Image.FillMethod.Horizontal;
            hpFill.fillAmount = 1f;
            RectTransform hpFillRt = hpFillGo.GetComponent<RectTransform>();
            UIBuilder.Anchor(hpFillRt, Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-2f, -2f));

            hpText = UIBuilder.CreateText("HpText", hpBg.transform, "100 / 100", 18, TextAnchor.MiddleCenter, Color.white);
            RectTransform hpTextRt = hpText.rectTransform;
            UIBuilder.Anchor(hpTextRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Bottom strips
            GameObject bottomBar = UIBuilder.CreatePanel("BottomBar", transform, new Color(0f, 0f, 0f, 0.40f));
            RectTransform bottomRt = bottomBar.GetComponent<RectTransform>();
            UIBuilder.Anchor(bottomRt, Vector2.zero, new Vector2(1f, 0.10f), Vector2.zero, Vector2.zero);

            Text wepLbl = UIBuilder.CreateText("WepLbl", bottomBar.transform, "武器", 16, TextAnchor.MiddleLeft, new Color(1f, 0.85f, 0.4f));
            UIBuilder.Anchor(wepLbl.rectTransform, new Vector2(0.01f, 0.55f), new Vector2(0.10f, 0.95f), Vector2.zero, Vector2.zero);

            GameObject wepStrip = new GameObject("WeaponStrip", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            wepStrip.transform.SetParent(bottomBar.transform, false);
            weaponStrip = wepStrip.GetComponent<RectTransform>();
            UIBuilder.Anchor(weaponStrip, new Vector2(0.10f, 0.50f), new Vector2(0.55f, 0.95f), Vector2.zero, Vector2.zero);
            HorizontalLayoutGroup wepHlg = wepStrip.GetComponent<HorizontalLayoutGroup>();
            wepHlg.spacing = 4f;
            wepHlg.childAlignment = TextAnchor.MiddleLeft;
            wepHlg.childControlWidth = false;
            wepHlg.childControlHeight = false;

            Text itmLbl = UIBuilder.CreateText("ItmLbl", bottomBar.transform, "道具", 16, TextAnchor.MiddleLeft, new Color(0.6f, 0.85f, 1f));
            UIBuilder.Anchor(itmLbl.rectTransform, new Vector2(0.01f, 0.05f), new Vector2(0.10f, 0.50f), Vector2.zero, Vector2.zero);

            GameObject itmStrip = new GameObject("ItemStrip", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            itmStrip.transform.SetParent(bottomBar.transform, false);
            itemStrip = itmStrip.GetComponent<RectTransform>();
            UIBuilder.Anchor(itemStrip, new Vector2(0.10f, 0.05f), new Vector2(0.95f, 0.50f), Vector2.zero, Vector2.zero);
            HorizontalLayoutGroup itmHlg = itmStrip.GetComponent<HorizontalLayoutGroup>();
            itmHlg.spacing = 4f;
            itmHlg.childAlignment = TextAnchor.MiddleLeft;
            itmHlg.childControlWidth = false;
            itmHlg.childControlHeight = false;
        }

        private void HookPlayerEvents()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDamaged += HandleHpChanged;
                playerHealth.OnHealed += HandleHpChanged;
            }
            if (itemManager != null)
            {
                itemManager.OnItemAdded += HandleItemChanged;
                itemManager.OnItemStacked += HandleItemChanged;
                itemManager.OnItemRemoved += HandleItemRemoved;
            }
        }

        private void UnhookPlayerEvents()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDamaged -= HandleHpChanged;
                playerHealth.OnHealed -= HandleHpChanged;
            }
            if (itemManager != null)
            {
                itemManager.OnItemAdded -= HandleItemChanged;
                itemManager.OnItemStacked -= HandleItemChanged;
                itemManager.OnItemRemoved -= HandleItemRemoved;
            }
        }

        private void HookManagerEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveStarted += HandleWaveStarted;
            }
            if (waveManager != null)
            {
                waveManager.OnWaveStarted += HandleWaveStarted;
            }
        }

        private void UnhookManagerEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveStarted -= HandleWaveStarted;
            }
            if (waveManager != null)
            {
                waveManager.OnWaveStarted -= HandleWaveStarted;
            }
        }

        private void HandleHpChanged(float _)
        {
            RefreshHp();
        }

        private void HandleItemChanged(ItemInstance _)
        {
            RefreshItems();
        }

        private void HandleItemRemoved(int _)
        {
            RefreshItems();
        }

        private void HandleWaveStarted(int _)
        {
            RefreshAll();
        }

        private void RefreshAll()
        {
            RefreshHp();
            RefreshWaveLabel();
            RefreshKills();
            RefreshWeapons();
            RefreshItems();
        }

        private void RefreshHp()
        {
            if (playerHealth == null || hpFill == null) return;
            float max = playerHealth.MaxHp;
            float cur = playerHealth.CurrentHp;
            float ratio = max > 0f ? Mathf.Clamp01(cur / max) : 0f;
            hpFill.fillAmount = ratio;
            hpFill.color = ratio > 0.5f
                ? new Color(0.30f, 0.75f, 0.35f)
                : (ratio > 0.25f ? new Color(0.85f, 0.70f, 0.20f) : new Color(0.85f, 0.25f, 0.30f));
            if (hpText != null)
            {
                hpText.text = $"{Mathf.CeilToInt(cur)} / {Mathf.CeilToInt(max)}";
            }
        }

        private void RefreshWaveLabel()
        {
            if (waveLabel == null) return;
            int idx = waveManager != null
                ? waveManager.CurrentWaveIndex
                : (GameManager.Instance != null ? GameManager.Instance.CurrentWaveIndex : 0);
            bool boss = waveManager != null
                && waveManager.CurrentWaveConfig != null
                && waveManager.CurrentWaveConfig.isBossWave;
            waveLabel.text = boss ? $"BOSS — Wave {idx}" : $"Wave {idx}";
            waveLabel.color = boss ? new Color(1f, 0.5f, 0.4f) : Color.white;
        }

        private void RefreshKills()
        {
            if (killsLabel == null) return;
            int kills = waveManager != null ? waveManager.EnemiesKilledThisWave : 0;
            killsLabel.text = $"💀 {kills}";
        }

        private void RefreshWeapons()
        {
            if (weaponStrip == null) return;
            for (int i = weaponStrip.childCount - 1; i >= 0; i--)
            {
                Destroy(weaponStrip.GetChild(i).gameObject);
            }
            if (weaponHolder == null) return;
            IReadOnlyList<WeaponInstance> equipped = weaponHolder.EquippedWeapons;
            for (int i = 0; i < equipped.Count; i++)
            {
                WeaponInstance w = equipped[i];
                if (w == null || w.Config == null) continue;
                CreateSlotIcon(weaponStrip, w.Config.displayName, UIBuilder.RarityColor(w.Config.rarity));
            }
        }

        private void RefreshItems()
        {
            if (itemStrip == null) return;
            for (int i = itemStrip.childCount - 1; i >= 0; i--)
            {
                Destroy(itemStrip.GetChild(i).gameObject);
            }
            if (itemManager == null) return;
            IReadOnlyList<ItemInstance> owned = itemManager.OwnedItems;
            for (int i = 0; i < owned.Count; i++)
            {
                ItemInstance it = owned[i];
                if (it == null || it.Config == null) continue;
                string label = it.StackCount > 1
                    ? $"{it.DisplayName} ×{it.StackCount}"
                    : it.DisplayName;
                CreateSlotIcon(itemStrip, label, UIBuilder.RarityColor(it.Config.rarity));
            }
        }

        private static void CreateSlotIcon(RectTransform parent, string label, Color rarityColor)
        {
            GameObject slot = new GameObject("Slot", typeof(RectTransform), typeof(Image));
            slot.transform.SetParent(parent, false);
            Image img = slot.GetComponent<Image>();
            img.color = new Color(rarityColor.r * 0.5f, rarityColor.g * 0.5f, rarityColor.b * 0.5f, 0.85f);
            RectTransform rt = slot.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120f, 38f);

            GameObject border = new GameObject("Border", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(slot.transform, false);
            Image bImg = border.GetComponent<Image>();
            bImg.color = rarityColor;
            RectTransform bRt = border.GetComponent<RectTransform>();
            UIBuilder.Anchor(bRt, Vector2.zero, new Vector2(1f, 0.10f), Vector2.zero, Vector2.zero);

            Text txt = UIBuilder.CreateText("Label", slot.transform, label, 14, TextAnchor.MiddleCenter, Color.white);
            RectTransform txtRt = txt.rectTransform;
            UIBuilder.Anchor(txtRt, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f));
        }
    }
}
