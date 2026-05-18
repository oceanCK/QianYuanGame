using System;
using System.Collections.Generic;
using UnityEngine;
using CatBrotato.Core;
using CatBrotato.Data;
using CatBrotato.Enemy;
using CatBrotato.Player;

namespace CatBrotato.Item
{
    /// <summary>
    /// Holds the player's owned items, applies their stat bonuses, and runs trigger effects
    /// (hp regen tick, on-kill, on-damaged, on-wave-start).
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class ItemManager : MonoBehaviour
    {
        public event Action<ItemInstance> OnItemAdded;
        public event Action<ItemInstance> OnItemStacked;
        public event Action<int> OnItemRemoved;

        [SerializeField] private float regenAccumulator;

        private readonly List<ItemInstance> ownedItems = new List<ItemInstance>();
        private readonly Dictionary<int, ItemInstance> itemLookup = new Dictionary<int, ItemInstance>();

        private PlayerStats playerStats;
        private PlayerHealth playerHealth;

        public IReadOnlyList<ItemInstance> OwnedItems => ownedItems;
        public int TotalItemCount
        {
            get
            {
                int sum = 0;
                for (int i = 0; i < ownedItems.Count; i++) sum += ownedItems[i].StackCount;
                return sum;
            }
        }

        private void Awake()
        {
            playerStats = GetComponent<PlayerStats>();
            playerHealth = GetComponent<PlayerHealth>();
        }

        private void OnEnable()
        {
            EnemyBase.OnEnemyDeath += HandleEnemyDeath;
            if (playerHealth != null)
            {
                playerHealth.OnDamaged += HandlePlayerDamaged;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveStarted += HandleWaveStarted;
            }
        }

        private void OnDisable()
        {
            EnemyBase.OnEnemyDeath -= HandleEnemyDeath;
            if (playerHealth != null)
            {
                playerHealth.OnDamaged -= HandlePlayerDamaged;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveStarted -= HandleWaveStarted;
            }
        }

        private void Update()
        {
            TickHpRegen(Time.deltaTime);
        }

        /// <summary>
        /// Adds an item by id. Stacks if already owned and stack rule allows.
        /// Returns the resulting ItemInstance, or null on failure.
        /// </summary>
        public ItemInstance AddItem(int itemId)
        {
            if (ConfigManager.Instance == null) return null;
            ItemData data = ConfigManager.Instance.GetItem(itemId);
            if (data == null)
            {
                Debug.LogWarning($"[ItemManager] No ItemData for id {itemId}.");
                return null;
            }
            return AddItem(data);
        }

        public ItemInstance AddItem(ItemData data)
        {
            if (data == null) return null;

            if (itemLookup.TryGetValue(data.id, out ItemInstance existing))
            {
                if (!existing.CanStack())
                {
                    Debug.Log($"[ItemManager] {data.displayName} at max stack.");
                    return existing;
                }
                existing.IncrementStack();
                ItemEffectProcessor.ApplyOneStack(existing, playerStats);
                OnItemStacked?.Invoke(existing);
                return existing;
            }

            ItemInstance instance = new ItemInstance(data, 1);
            ownedItems.Add(instance);
            itemLookup[data.id] = instance;
            ItemEffectProcessor.ApplyOneStack(instance, playerStats);
            OnItemAdded?.Invoke(instance);
            return instance;
        }

        public bool RemoveItem(int itemId)
        {
            if (!itemLookup.TryGetValue(itemId, out ItemInstance instance)) return false;

            // Reverse all stacks.
            for (int i = 0; i < instance.StackCount; i++)
            {
                ItemEffectProcessor.RemoveOneStack(instance, playerStats);
            }

            ownedItems.Remove(instance);
            itemLookup.Remove(itemId);
            OnItemRemoved?.Invoke(itemId);
            return true;
        }

        public bool HasItem(int itemId)
        {
            return itemLookup.ContainsKey(itemId);
        }

        public ItemInstance GetItem(int itemId)
        {
            itemLookup.TryGetValue(itemId, out ItemInstance instance);
            return instance;
        }

        public void ClearAll()
        {
            for (int i = 0; i < ownedItems.Count; i++)
            {
                ItemInstance instance = ownedItems[i];
                for (int s = 0; s < instance.StackCount; s++)
                {
                    ItemEffectProcessor.RemoveOneStack(instance, playerStats);
                }
            }
            ownedItems.Clear();
            itemLookup.Clear();
            regenAccumulator = 0f;
        }

        private void TickHpRegen(float deltaTime)
        {
            if (playerHealth == null) return;
            if (playerHealth.IsDead) return;

            float regenPerSecond = 0f;
            for (int i = 0; i < ownedItems.Count; i++)
            {
                ItemInstance item = ownedItems[i];
                float per = ItemEffectProcessor.GetRegenPerSecond(item);
                if (per > 0f)
                {
                    regenPerSecond += per * item.StackCount;
                }
            }

            if (regenPerSecond <= 0f) return;

            regenAccumulator += regenPerSecond * deltaTime;
            if (regenAccumulator >= 1f)
            {
                int healAmount = Mathf.FloorToInt(regenAccumulator);
                playerHealth.Heal(healAmount);
                regenAccumulator -= healAmount;
            }
        }

        private void HandleEnemyDeath(EnemyBase enemy)
        {
            for (int i = 0; i < ownedItems.Count; i++)
            {
                ItemInstance item = ownedItems[i];
                if (!ItemEffectProcessor.IsOnKill(item)) continue;
                ApplyTriggerEffect(item);
            }
        }

        private void HandlePlayerDamaged(float damage)
        {
            for (int i = 0; i < ownedItems.Count; i++)
            {
                ItemInstance item = ownedItems[i];
                if (!ItemEffectProcessor.IsOnDamaged(item)) continue;
                ApplyTriggerEffect(item);
            }
        }

        private void HandleWaveStarted(int waveIndex)
        {
            for (int i = 0; i < ownedItems.Count; i++)
            {
                ItemInstance item = ownedItems[i];
                if (!ItemEffectProcessor.IsOnWaveStart(item)) continue;
                ApplyTriggerEffect(item);
            }
        }

        /// <summary>
        /// Applies a one-shot effect for a triggered item.
        /// </summary>
        private void ApplyTriggerEffect(ItemInstance item)
        {
            if (item == null || playerHealth == null) return;

            string stat = item.AffectedStats;
            if (string.IsNullOrEmpty(stat)) return;

            // Heal-on-trigger covers revive amulet and on-wave-start heal items.
            if (stat.Equals("hp", StringComparison.OrdinalIgnoreCase))
            {
                playerHealth.Heal(item.ValueA * item.StackCount);
            }
        }
    }
}
