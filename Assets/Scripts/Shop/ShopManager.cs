using System;
using System.Collections.Generic;
using UnityEngine;
using CatBrotato.Core;
using CatBrotato.Data;
using CatBrotato.Combat;
using CatBrotato.Item;

namespace CatBrotato.Shop
{
    /// <summary>
    /// Rolls between-wave shop offers, handles refreshes/locks, and routes purchases
    /// to WeaponHolder and ItemManager. Reads ShopData from ConfigManager.
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        public event Action<ShopData> OnShopOpened;
        public event Action OnOffersRefreshed;
        public event Action<ShopOffer> OnPurchased;
        public event Action OnShopClosed;

        [SerializeField] private int defaultShopId = 6001;

        // Default weapon prices keyed by Rarity ordinal (Common..Legendary).
        // Weapons don't carry basePrice in WeaponData, so we use a rarity table.
        private static readonly int[] WeaponBasePriceByRarity = { 8, 16, 28, 45, 70 };

        private readonly List<ShopOffer> weaponOffers = new List<ShopOffer>();
        private readonly List<ShopOffer> itemOffers = new List<ShopOffer>();

        private ShopData currentShop;
        private int refreshCount;
        private int freeRefreshesUsed;

        public IReadOnlyList<ShopOffer> WeaponOffers => weaponOffers;
        public IReadOnlyList<ShopOffer> ItemOffers => itemOffers;
        public ShopData CurrentShop => currentShop;
        public bool IsOpen => currentShop != null;

        private WeaponHolder weaponHolder;
        private ItemManager itemManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void BindPlayer(WeaponHolder holder, ItemManager items)
        {
            weaponHolder = holder;
            itemManager = items;
        }

        private void HandleGameStateChanged(GameState previous, GameState next)
        {
            if (next == GameState.Shop)
            {
                OpenShop(defaultShopId);
            }
            else if (previous == GameState.Shop && next != GameState.Shop)
            {
                CloseShop();
            }
        }

        public void OpenShop(int shopId)
        {
            if (ConfigManager.Instance == null)
            {
                Debug.LogError("[ShopManager] ConfigManager missing — cannot open shop.");
                return;
            }
            ShopData data = ConfigManager.Instance.GetShop(shopId);
            if (data == null)
            {
                Debug.LogError($"[ShopManager] Shop {shopId} not found in config.");
                return;
            }
            currentShop = data;
            refreshCount = 0;
            freeRefreshesUsed = 0;
            RollOffers(keepLocked: false);
            OnShopOpened?.Invoke(currentShop);
        }

        public void CloseShop()
        {
            if (!IsOpen) return;
            currentShop = null;
            weaponOffers.Clear();
            itemOffers.Clear();
            OnShopClosed?.Invoke();
        }

        /// <summary>
        /// Returns the cost of the next refresh, taking free refreshes into account.
        /// </summary>
        public int GetRefreshCost()
        {
            if (currentShop == null) return 0;
            if (freeRefreshesUsed < currentShop.freeRefreshCount) return 0;
            float cost = currentShop.baseRefreshCost
                         + currentShop.refreshCostGrowth * refreshCount;
            return Mathf.Max(0, Mathf.CeilToInt(cost));
        }

        public bool TryRefresh()
        {
            if (!IsOpen) return false;
            int cost = GetRefreshCost();
            if (cost > 0)
            {
                if (GameManager.Instance == null) return false;
                if (!GameManager.Instance.SpendMaterials(cost)) return false;
            }
            else
            {
                freeRefreshesUsed++;
            }
            refreshCount++;
            RollOffers(keepLocked: true);
            OnOffersRefreshed?.Invoke();
            return true;
        }

        public void ToggleLock(ShopOffer offer)
        {
            if (offer == null) return;
            offer.IsLocked = !offer.IsLocked;
        }

        public bool TryPurchase(ShopOffer offer)
        {
            if (offer == null || offer.IsSold) return false;
            if (GameManager.Instance == null) return false;
            if (!GameManager.Instance.SpendMaterials(offer.FinalPrice)) return false;

            bool ok = false;
            if (offer.Kind == OfferKind.Weapon)
            {
                if (weaponHolder == null)
                {
                    Debug.LogWarning("[ShopManager] No WeaponHolder bound — refunding.");
                    GameManager.Instance.AddMaterials(offer.FinalPrice);
                    return false;
                }
                ok = weaponHolder.EquipWeapon(offer.Weapon);
            }
            else
            {
                if (itemManager == null)
                {
                    Debug.LogWarning("[ShopManager] No ItemManager bound — refunding.");
                    GameManager.Instance.AddMaterials(offer.FinalPrice);
                    return false;
                }
                ok = itemManager.AddItem(offer.Item) != null;
            }

            if (!ok)
            {
                GameManager.Instance.AddMaterials(offer.FinalPrice);
                return false;
            }

            offer.IsSold = true;
            offer.IsLocked = false;
            OnPurchased?.Invoke(offer);
            return true;
        }

        // --- Roll logic ---

        private void RollOffers(bool keepLocked)
        {
            if (currentShop == null) return;

            // Snapshot locked offers so we can carry them across refresh.
            List<ShopOffer> lockedWeapons = keepLocked ? CollectLocked(weaponOffers) : null;
            List<ShopOffer> lockedItems = keepLocked ? CollectLocked(itemOffers) : null;

            weaponOffers.Clear();
            itemOffers.Clear();

            int weaponSlots = Mathf.Max(0, currentShop.weaponSlotCount);
            int itemSlots = Mathf.Max(0, currentShop.itemSlotCount);

            // Re-add locked first (they keep their original price).
            if (lockedWeapons != null)
            {
                for (int i = 0; i < lockedWeapons.Count && weaponOffers.Count < weaponSlots; i++)
                {
                    weaponOffers.Add(lockedWeapons[i]);
                }
            }
            if (lockedItems != null)
            {
                for (int i = 0; i < lockedItems.Count && itemOffers.Count < itemSlots; i++)
                {
                    itemOffers.Add(lockedItems[i]);
                }
            }

            // Fill remaining slots with new rolls.
            while (weaponOffers.Count < weaponSlots)
            {
                ShopOffer rolled = RollWeaponOffer();
                if (rolled != null) weaponOffers.Add(rolled);
                else break;
            }
            while (itemOffers.Count < itemSlots)
            {
                ShopOffer rolled = RollItemOffer();
                if (rolled != null) itemOffers.Add(rolled);
                else break;
            }
        }

        private static List<ShopOffer> CollectLocked(List<ShopOffer> source)
        {
            List<ShopOffer> result = new List<ShopOffer>();
            for (int i = 0; i < source.Count; i++)
            {
                ShopOffer offer = source[i];
                if (offer != null && offer.IsLocked && !offer.IsSold)
                {
                    result.Add(offer);
                }
            }
            return result;
        }

        private ShopOffer RollWeaponOffer()
        {
            List<WeaponData> all = ConfigManager.Instance.GetAllWeapons();
            if (all == null || all.Count == 0) return null;

            Rarity rarity = RollRarity();
            WeaponData picked = WeightedPickWeapon(all, rarity);
            if (picked == null) picked = WeightedPickWeapon(all, null);
            if (picked == null) return null;

            int price = ComputeWeaponPrice(picked);
            return ShopOffer.ForWeapon(picked, price);
        }

        private ShopOffer RollItemOffer()
        {
            List<ItemData> all = ConfigManager.Instance.GetAllItems();
            if (all == null || all.Count == 0) return null;

            HashSet<string> allowed = BuildAllowedCategorySet(currentShop.allowedItemCategories);

            Rarity rarity = RollRarity();
            ItemData picked = WeightedPickItem(all, rarity, allowed);
            if (picked == null) picked = WeightedPickItem(all, null, allowed);
            if (picked == null) return null;

            int price = ComputeItemPrice(picked);
            return ShopOffer.ForItem(picked, price);
        }

        private Rarity RollRarity()
        {
            float[] weights = currentShop != null ? currentShop.rarityWeights : null;
            if (weights == null || weights.Length == 0) return Rarity.Common;

            float total = 0f;
            for (int i = 0; i < weights.Length; i++) total += weights[i];
            if (total <= 0f) return Rarity.Common;

            float roll = UnityEngine.Random.value * total;
            float acc = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                acc += weights[i];
                if (roll <= acc)
                {
                    return (Rarity)Mathf.Clamp(i, 0, (int)Rarity.Legendary);
                }
            }
            return Rarity.Common;
        }

        private static HashSet<string> BuildAllowedCategorySet(string[] categories)
        {
            if (categories == null || categories.Length == 0) return null;
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < categories.Length; i++)
            {
                if (!string.IsNullOrEmpty(categories[i])) set.Add(categories[i]);
            }
            return set.Count > 0 ? set : null;
        }

        private WeaponData WeightedPickWeapon(List<WeaponData> source, Rarity? rarityFilter)
        {
            float total = 0f;
            for (int i = 0; i < source.Count; i++)
            {
                WeaponData w = source[i];
                if (w == null) continue;
                if (rarityFilter.HasValue && w.rarity != rarityFilter.Value) continue;
                float wt = w.shopWeight > 0f ? w.shopWeight : 0f;
                total += wt;
            }
            if (total <= 0f) return null;

            float roll = UnityEngine.Random.value * total;
            float acc = 0f;
            for (int i = 0; i < source.Count; i++)
            {
                WeaponData w = source[i];
                if (w == null) continue;
                if (rarityFilter.HasValue && w.rarity != rarityFilter.Value) continue;
                float wt = w.shopWeight > 0f ? w.shopWeight : 0f;
                acc += wt;
                if (roll <= acc) return w;
            }
            return null;
        }

        private ItemData WeightedPickItem(List<ItemData> source, Rarity? rarityFilter, HashSet<string> allowedCategories)
        {
            float total = 0f;
            for (int i = 0; i < source.Count; i++)
            {
                ItemData it = source[i];
                if (it == null) continue;
                if (rarityFilter.HasValue && it.rarity != rarityFilter.Value) continue;
                if (allowedCategories != null && !allowedCategories.Contains(it.itemCategory.ToString())) continue;
                float wt = it.shopWeight > 0f ? it.shopWeight : 0f;
                total += wt;
            }
            if (total <= 0f) return null;

            float roll = UnityEngine.Random.value * total;
            float acc = 0f;
            for (int i = 0; i < source.Count; i++)
            {
                ItemData it = source[i];
                if (it == null) continue;
                if (rarityFilter.HasValue && it.rarity != rarityFilter.Value) continue;
                if (allowedCategories != null && !allowedCategories.Contains(it.itemCategory.ToString())) continue;
                float wt = it.shopWeight > 0f ? it.shopWeight : 0f;
                acc += wt;
                if (roll <= acc) return it;
            }
            return null;
        }

        private int ComputeWeaponPrice(WeaponData weapon)
        {
            int rarityIdx = Mathf.Clamp((int)weapon.rarity, 0, WeaponBasePriceByRarity.Length - 1);
            float price = WeaponBasePriceByRarity[rarityIdx];
            return ApplyShopPriceModifiers(price);
        }

        private int ComputeItemPrice(ItemData item)
        {
            return ApplyShopPriceModifiers(item.basePrice);
        }

        private int ApplyShopPriceModifiers(float basePrice)
        {
            float multiplier = currentShop != null ? currentShop.priceMultiplier : 1f;
            if (multiplier <= 0f) multiplier = 1f;
            float discount = currentShop != null ? Mathf.Clamp01(currentShop.discountRate) : 0f;
            float price = basePrice * multiplier * (1f - discount);
            return Mathf.Max(0, Mathf.CeilToInt(price));
        }
    }
}
