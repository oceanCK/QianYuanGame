using System.Collections.Generic;
using UnityEngine;
using CatBrotato.Data;
using CatBrotato.Player;

namespace CatBrotato.Combat
{
    public class WeaponHolder : MonoBehaviour
    {
        [SerializeField] private int maxWeaponSlots = 6;
        [SerializeField] private MeleeAttack meleeAttack;
        [SerializeField] private RangedAttack rangedAttack;

        private List<WeaponInstance> equippedWeapons = new List<WeaponInstance>();
        private PlayerStats playerStats;

        public IReadOnlyList<WeaponInstance> EquippedWeapons => equippedWeapons;
        public int MaxSlots => maxWeaponSlots;

        private void Awake()
        {
            playerStats = GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                playerStats = GetComponentInParent<PlayerStats>();
            }

            if (meleeAttack == null)
            {
                meleeAttack = GetComponent<MeleeAttack>();
            }
            if (rangedAttack == null)
            {
                rangedAttack = GetComponent<RangedAttack>();
            }
        }

        private void Update()
        {
            // Tick cooldowns for all weapons
            float attackSpeedBonus = playerStats != null ? playerStats.AttackSpeed : 0f;

            for (int i = 0; i < equippedWeapons.Count; i++)
            {
                WeaponInstance weapon = equippedWeapons[i];
                // Scale cooldown tick by attack speed
                float speedMultiplier = 1f + attackSpeedBonus;
                if (speedMultiplier < 0.1f) speedMultiplier = 0.1f;
                weapon.TickCooldown(Time.deltaTime * speedMultiplier);
            }
        }

        /// <summary>
        /// Called by AutoAttackSystem when a target is available.
        /// Tries to attack with each weapon that is ready.
        /// </summary>
        public void TryAttackAll(Transform target)
        {
            if (target == null) return;

            for (int i = 0; i < equippedWeapons.Count; i++)
            {
                WeaponInstance weapon = equippedWeapons[i];
                if (!weapon.CanAttack()) continue;

                float distToTarget = Vector2.Distance(
                    transform.position, target.position);

                // Check if target is within weapon range
                if (distToTarget > weapon.Config.range) continue;

                ExecuteAttack(weapon, target);
                weapon.ResetCooldown();
            }
        }

        public bool EquipWeapon(WeaponData data)
        {
            if (data == null)
            {
                Debug.LogWarning("WeaponHolder: Cannot equip null weapon data.");
                return false;
            }

            if (equippedWeapons.Count >= maxWeaponSlots)
            {
                Debug.LogWarning("WeaponHolder: All weapon slots are full.");
                return false;
            }

            int slotIndex = equippedWeapons.Count;
            WeaponInstance instance = new WeaponInstance(data, slotIndex);
            equippedWeapons.Add(instance);
            return true;
        }

        public bool RemoveWeapon(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= equippedWeapons.Count)
            {
                Debug.LogWarning($"WeaponHolder: Invalid slot index {slotIndex}.");
                return false;
            }

            equippedWeapons.RemoveAt(slotIndex);

            // Reassign slot indices
            for (int i = 0; i < equippedWeapons.Count; i++)
            {
                equippedWeapons[i].SlotIndex = i;
            }

            return true;
        }

        public bool UpgradeWeapon(int slotIndex, WeaponData newData)
        {
            if (slotIndex < 0 || slotIndex >= equippedWeapons.Count)
            {
                Debug.LogWarning($"WeaponHolder: Invalid slot index {slotIndex}.");
                return false;
            }

            if (newData == null)
            {
                Debug.LogWarning("WeaponHolder: Cannot upgrade with null weapon data.");
                return false;
            }

            equippedWeapons[slotIndex] = new WeaponInstance(newData, slotIndex);
            return true;
        }

        private void ExecuteAttack(WeaponInstance weapon, Transform target)
        {
            float bonusDamage = 0f;
            if (playerStats != null)
            {
                if (weapon.Config.weaponClass == WeaponClass.Melee)
                {
                    bonusDamage = playerStats.MeleeDamage;
                }
                else if (weapon.Config.weaponClass == WeaponClass.Ranged)
                {
                    bonusDamage = playerStats.RangedDamage;
                }
                else
                {
                    // Magic uses the higher of melee/ranged bonus
                    bonusDamage = Mathf.Max(playerStats.MeleeDamage, playerStats.RangedDamage);
                }
            }

            float damage = weapon.CalculateEffectiveDamage(bonusDamage, 0f);
            float critChance = playerStats != null
                ? playerStats.CritChance + weapon.Config.critBonus
                : weapon.Config.critBonus;

            switch (weapon.Config.weaponClass)
            {
                case WeaponClass.Melee:
                    if (meleeAttack != null)
                    {
                        meleeAttack.Execute(weapon, target, damage, critChance);
                    }
                    break;

                case WeaponClass.Ranged:
                case WeaponClass.Magic:
                    if (rangedAttack != null)
                    {
                        rangedAttack.Execute(weapon, target, damage, critChance);
                    }
                    break;
            }
        }
    }
}
