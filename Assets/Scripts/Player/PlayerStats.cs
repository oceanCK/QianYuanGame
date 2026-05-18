using System.Collections.Generic;
using UnityEngine;
using CatBrotato.Data;

namespace CatBrotato.Player
{
    public class PlayerStats : MonoBehaviour
    {
        // Base stats (from character data)
        private float baseHp;
        private float baseMoveSpeed;
        private float baseMeleeDamage;
        private float baseRangedDamage;
        private float baseAttackSpeed;
        private float baseCritChance;
        private float baseArmor;
        private float baseDodge;
        private float baseLuck;
        private float baseHarvesting;

        // Bonus stats (from items/upgrades)
        private Dictionary<string, float> bonusStats = new Dictionary<string, float>();

        // Combined stat properties (base + bonus)
        public float MaxHp => baseHp + GetBonus("hp");
        public float MoveSpeed => baseMoveSpeed + GetBonus("moveSpeed");
        public float MeleeDamage => baseMeleeDamage + GetBonus("meleeDamage");
        public float RangedDamage => baseRangedDamage + GetBonus("rangedDamage");
        public float AttackSpeed => baseAttackSpeed + GetBonus("attackSpeed");
        public float CritChance => baseCritChance + GetBonus("critChance");
        public float Armor => baseArmor + GetBonus("armor");
        public float Dodge => baseDodge + GetBonus("dodge");
        public float Luck => baseLuck + GetBonus("luck");
        public float Harvesting => baseHarvesting + GetBonus("harvesting");

        public void InitFromCharacter(CharacterData data)
        {
            if (data == null)
            {
                Debug.LogError("PlayerStats: CharacterData is null.");
                return;
            }

            baseHp = data.baseHp;
            baseMoveSpeed = data.baseMoveSpeed;
            baseMeleeDamage = data.baseMeleeDamage;
            baseRangedDamage = data.baseRangedDamage;
            baseAttackSpeed = data.baseAttackSpeed;
            baseCritChance = data.baseCritChance;
            baseArmor = data.baseArmor;
            baseDodge = data.baseDodge;
            baseLuck = data.baseLuck;
            baseHarvesting = data.baseHarvesting;

            bonusStats.Clear();
        }

        public void AddStatBonus(string statName, float value)
        {
            string key = statName.ToLowerInvariant();
            if (bonusStats.ContainsKey(key))
            {
                bonusStats[key] += value;
            }
            else
            {
                bonusStats[key] = value;
            }
        }

        public void RemoveStatBonus(string statName, float value)
        {
            string key = statName.ToLowerInvariant();
            if (bonusStats.ContainsKey(key))
            {
                bonusStats[key] -= value;
                if (Mathf.Approximately(bonusStats[key], 0f))
                {
                    bonusStats.Remove(key);
                }
            }
        }

        public float GetStat(string statName)
        {
            switch (statName.ToLowerInvariant())
            {
                case "hp": return MaxHp;
                case "movespeed": return MoveSpeed;
                case "meleedamage": return MeleeDamage;
                case "rangeddamage": return RangedDamage;
                case "attackspeed": return AttackSpeed;
                case "critchance": return CritChance;
                case "armor": return Armor;
                case "dodge": return Dodge;
                case "luck": return Luck;
                case "harvesting": return Harvesting;
                default:
                    Debug.LogWarning($"PlayerStats: Unknown stat '{statName}'.");
                    return 0f;
            }
        }

        private float GetBonus(string statName)
        {
            string key = statName.ToLowerInvariant();
            return bonusStats.TryGetValue(key, out float value) ? value : 0f;
        }
    }
}
