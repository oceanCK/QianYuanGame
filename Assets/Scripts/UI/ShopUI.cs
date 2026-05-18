using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CatBrotato.Core;
using CatBrotato.Data;
using CatBrotato.Shop;

namespace CatBrotato.UI
{
    /// <summary>
    /// Between-wave shop screen. Renders weapon/item offers from ShopManager,
    /// supports lock/buy/refresh, shows materials counter, and continues to the next wave.
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        private RectTransform weaponRow;
        private RectTransform itemRow;
        private Text titleLabel;
        private Text materialsLabel;
        private Text refreshCostLabel;
        private Button refreshBtn;
        private Button continueBtn;

        private readonly List<GameObject> spawnedCards = new List<GameObject>();

        private void Awake()
        {
            BuildUI();
        }

        private void OnEnable()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnShopOpened += HandleShopOpened;
                ShopManager.Instance.OnOffersRefreshed += HandleOffersRefreshed;
                ShopManager.Instance.OnPurchased += HandlePurchased;
                ShopManager.Instance.OnShopClosed += HandleShopClosed;
                if (ShopManager.Instance.IsOpen)
                {
                    HandleShopOpened(ShopManager.Instance.CurrentShop);
                }
            }
            UpdateMaterialsLabel();
            UpdateRefreshCostLabel();
        }

        private void OnDisable()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnShopOpened -= HandleShopOpened;
                ShopManager.Instance.OnOffersRefreshed -= HandleOffersRefreshed;
                ShopManager.Instance.OnPurchased -= HandlePurchased;
                ShopManager.Instance.OnShopClosed -= HandleShopClosed;
            }
        }

        private void Update()
        {
            UpdateMaterialsLabel();
        }

        private void BuildUI()
        {
            UIBuilder.CreatePanel("Background", transform, new Color(0.05f, 0.04f, 0.10f, 1f));

            titleLabel = UIBuilder.CreateText("Title", transform, "商店 — Shop", 36, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.5f));
            UIBuilder.SetAnchoredPos(titleLabel.rectTransform, new Vector2(0.5f, 0.95f), Vector2.zero, new Vector2(800f, 60f));

            materialsLabel = UIBuilder.CreateText("Materials", transform, "材料: 0", 26, TextAnchor.MiddleRight, new Color(1f, 0.85f, 0.4f));
            UIBuilder.Anchor(materialsLabel.rectTransform, new Vector2(0.65f, 0.92f), new Vector2(0.97f, 0.99f), Vector2.zero, Vector2.zero);

            // Weapon section
            Text weaponHeader = UIBuilder.CreateText("WeaponHeader", transform, "武器 Weapons", 22, TextAnchor.MiddleLeft, new Color(1f, 0.8f, 0.6f));
            UIBuilder.Anchor(weaponHeader.rectTransform, new Vector2(0.05f, 0.84f), new Vector2(0.5f, 0.89f), Vector2.zero, Vector2.zero);

            GameObject weaponPanel = UIBuilder.CreatePanel("WeaponRow", transform, new Color(0.10f, 0.08f, 0.15f, 0.85f));
            UIBuilder.Anchor(weaponPanel.GetComponent<RectTransform>(), new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.84f), Vector2.zero, Vector2.zero);
            weaponRow = BuildHorizontalContent(weaponPanel.transform, "WeaponContent");

            // Item section
            Text itemHeader = UIBuilder.CreateText("ItemHeader", transform, "道具 Items", 22, TextAnchor.MiddleLeft, new Color(0.85f, 1f, 0.85f));
            UIBuilder.Anchor(itemHeader.rectTransform, new Vector2(0.05f, 0.50f), new Vector2(0.5f, 0.55f), Vector2.zero, Vector2.zero);

            GameObject itemPanel = UIBuilder.CreatePanel("ItemRow", transform, new Color(0.10f, 0.08f, 0.15f, 0.85f));
            UIBuilder.Anchor(itemPanel.GetComponent<RectTransform>(), new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.50f), Vector2.zero, Vector2.zero);
            itemRow = BuildHorizontalContent(itemPanel.transform, "ItemContent");

            // Bottom buttons
            refreshBtn = UIBuilder.CreateButton("RefreshButton", transform, "刷新 (Refresh)", new Vector2(220f, 56f), new Color(0.35f, 0.45f, 0.65f));
            UIBuilder.SetAnchoredPos(refreshBtn.GetComponent<RectTransform>(), new Vector2(0.25f, 0.08f), Vector2.zero, new Vector2(220f, 56f));
            refreshBtn.onClick.AddListener(OnRefreshClicked);

            refreshCostLabel = UIBuilder.CreateText("RefreshCost", refreshBtn.transform, "", 16, TextAnchor.LowerCenter, new Color(1f, 0.85f, 0.4f));
            UIBuilder.Anchor(refreshCostLabel.rectTransform, Vector2.zero, Vector2.one, new Vector2(0f, -2f), new Vector2(0f, 0f));

            continueBtn = UIBuilder.CreateButton("ContinueButton", transform, "出击下一波 (Next Wave)", new Vector2(280f, 64f), new Color(0.3f, 0.65f, 0.45f));
            UIBuilder.SetAnchoredPos(continueBtn.GetComponent<RectTransform>(), new Vector2(0.7f, 0.08f), Vector2.zero, new Vector2(280f, 64f));
            continueBtn.onClick.AddListener(OnContinueClicked);
        }

        private static RectTransform BuildHorizontalContent(Transform parent, string contentName)
        {
            GameObject content = new GameObject(contentName, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(parent, false);
            RectTransform rt = content.GetComponent<RectTransform>();
            UIBuilder.Anchor(rt, Vector2.zero, Vector2.one, new Vector2(12f, 12f), new Vector2(-12f, -12f));
            HorizontalLayoutGroup hlg = content.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14f;
            hlg.padding = new RectOffset(8, 8, 8, 8);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childControlHeight = true;
            hlg.childControlWidth = true;
            ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            return rt;
        }

        private void HandleShopOpened(ShopData shop)
        {
            if (titleLabel != null && shop != null)
            {
                titleLabel.text = string.IsNullOrEmpty(shop.displayName)
                    ? "商店 — Shop"
                    : $"{shop.displayName}";
            }
            RebuildOffers();
        }

        private void HandleOffersRefreshed()
        {
            RebuildOffers();
        }

        private void HandlePurchased(ShopOffer offer)
        {
            RebuildOffers();
            UpdateMaterialsLabel();
        }

        private void HandleShopClosed()
        {
            ClearCards();
        }

        private void RebuildOffers()
        {
            ClearCards();

            if (ShopManager.Instance == null) return;

            IReadOnlyList<ShopOffer> wo = ShopManager.Instance.WeaponOffers;
            if (wo != null)
            {
                for (int i = 0; i < wo.Count; i++)
                {
                    GameObject card = CreateOfferCard(wo[i], weaponRow);
                    if (card != null) spawnedCards.Add(card);
                }
            }

            IReadOnlyList<ShopOffer> io = ShopManager.Instance.ItemOffers;
            if (io != null)
            {
                for (int i = 0; i < io.Count; i++)
                {
                    GameObject card = CreateOfferCard(io[i], itemRow);
                    if (card != null) spawnedCards.Add(card);
                }
            }

            UpdateRefreshCostLabel();
        }

        private void ClearCards()
        {
            for (int i = spawnedCards.Count - 1; i >= 0; i--)
            {
                if (spawnedCards[i] != null) Destroy(spawnedCards[i]);
            }
            spawnedCards.Clear();
        }

        private GameObject CreateOfferCard(ShopOffer offer, RectTransform parent)
        {
            if (offer == null || parent == null) return null;

            Color rarityColor = UIBuilder.RarityColor(offer.Rarity);
            Color cardBg = new Color(rarityColor.r * 0.35f, rarityColor.g * 0.35f, rarityColor.b * 0.35f, 0.95f);

            GameObject card = UIBuilder.CreatePanel($"Offer_{offer.Kind}_{offer.Id}", parent, cardBg);
            RectTransform cardRt = card.GetComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(180f, 0f);
            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredWidth = 180f;
            le.minWidth = 160f;
            le.flexibleWidth = 0f;
            le.flexibleHeight = 1f;

            // Rarity stripe at top
            GameObject stripe = UIBuilder.CreatePanel("RarityStripe", card.transform, rarityColor).gameObject;
            RectTransform stripeRt = stripe.GetComponent<RectTransform>();
            UIBuilder.Anchor(stripeRt, new Vector2(0f, 0.92f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            // Name
            Text nameTxt = UIBuilder.CreateText("Name", card.transform, offer.DisplayName, 18, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f));
            UIBuilder.Anchor(nameTxt.rectTransform, new Vector2(0.05f, 0.74f), new Vector2(0.95f, 0.92f), Vector2.zero, Vector2.zero);

            // Description / stats body
            Text bodyTxt = UIBuilder.CreateText("Body", card.transform, BuildBody(offer), 14, TextAnchor.UpperLeft, new Color(0.92f, 0.92f, 0.95f));
            UIBuilder.Anchor(bodyTxt.rectTransform, new Vector2(0.06f, 0.30f), new Vector2(0.94f, 0.74f), Vector2.zero, Vector2.zero);

            // Price
            Text priceTxt = UIBuilder.CreateText("Price", card.transform, $"{offer.FinalPrice} 材料", 18, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.4f));
            UIBuilder.Anchor(priceTxt.rectTransform, new Vector2(0f, 0.18f), new Vector2(1f, 0.30f), Vector2.zero, Vector2.zero);

            // Buttons row: Lock / Buy
            Button lockBtn = UIBuilder.CreateButton("Lock", card.transform, offer.IsLocked ? "已锁定" : "锁定", new Vector2(70f, 36f), offer.IsLocked ? new Color(0.6f, 0.5f, 0.2f) : new Color(0.3f, 0.3f, 0.4f));
            UIBuilder.Anchor(lockBtn.GetComponent<RectTransform>(), new Vector2(0.05f, 0.03f), new Vector2(0.45f, 0.17f), Vector2.zero, Vector2.zero);
            ShopOffer capturedForLock = offer;
            lockBtn.onClick.AddListener(() => OnLockClicked(capturedForLock));
            lockBtn.interactable = !offer.IsSold;

            Button buyBtn = UIBuilder.CreateButton("Buy", card.transform, offer.IsSold ? "已售出" : "购买", new Vector2(70f, 36f), offer.IsSold ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.3f, 0.65f, 0.45f));
            UIBuilder.Anchor(buyBtn.GetComponent<RectTransform>(), new Vector2(0.55f, 0.03f), new Vector2(0.95f, 0.17f), Vector2.zero, Vector2.zero);
            ShopOffer capturedForBuy = offer;
            buyBtn.onClick.AddListener(() => OnBuyClicked(capturedForBuy));
            int playerMaterials = GameManager.Instance != null ? GameManager.Instance.PlayerMaterials : 0;
            buyBtn.interactable = !offer.IsSold && playerMaterials >= offer.FinalPrice;

            return card;
        }

        private static string BuildBody(ShopOffer offer)
        {
            if (offer.Kind == OfferKind.Weapon && offer.Weapon != null)
            {
                WeaponData w = offer.Weapon;
                return
                    $"职业: {w.weaponClass}\n" +
                    $"伤害: {w.baseDamage:0}\n" +
                    $"冷却: {w.cooldown:0.00}s\n" +
                    $"射程: {w.range:0}\n" +
                    $"稀有度: {w.rarity}";
            }
            if (offer.Kind == OfferKind.Item && offer.Item != null)
            {
                ItemData it = offer.Item;
                string stats = string.IsNullOrEmpty(it.affectedStats) ? "—" : it.affectedStats;
                return
                    $"类别: {it.itemCategory}\n" +
                    $"加成: {stats}\n" +
                    $"数值: {it.valueA:0.##} / {it.valueB:0.##}\n" +
                    $"层数上限: {it.maxStack}\n" +
                    $"稀有度: {it.rarity}";
            }
            return string.Empty;
        }

        private void OnLockClicked(ShopOffer offer)
        {
            if (ShopManager.Instance == null || offer == null) return;
            ShopManager.Instance.ToggleLock(offer);
            RebuildOffers();
        }

        private void OnBuyClicked(ShopOffer offer)
        {
            if (ShopManager.Instance == null || offer == null) return;
            ShopManager.Instance.TryPurchase(offer);
        }

        private void OnRefreshClicked()
        {
            if (ShopManager.Instance == null) return;
            ShopManager.Instance.TryRefresh();
        }

        private void OnContinueClicked()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.ExitShop();
        }

        private void UpdateMaterialsLabel()
        {
            if (materialsLabel == null) return;
            int mats = GameManager.Instance != null ? GameManager.Instance.PlayerMaterials : 0;
            materialsLabel.text = $"材料: {mats}";
        }

        private void UpdateRefreshCostLabel()
        {
            if (refreshCostLabel == null || ShopManager.Instance == null) return;
            int cost = ShopManager.Instance.GetRefreshCost();
            refreshCostLabel.text = cost > 0 ? $"消耗 {cost}" : "免费";
            if (refreshBtn != null)
            {
                int mats = GameManager.Instance != null ? GameManager.Instance.PlayerMaterials : 0;
                refreshBtn.interactable = ShopManager.Instance.IsOpen && (cost == 0 || mats >= cost);
            }
        }
    }
}
