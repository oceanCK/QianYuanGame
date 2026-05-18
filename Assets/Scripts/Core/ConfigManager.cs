using System.Collections.Generic;
using UnityEngine;
using CatBrotato.Data;

namespace CatBrotato.Core
{
    public class ConfigManager : MonoBehaviour
    {
        public static ConfigManager Instance { get; private set; }

        private Dictionary<int, CharacterData> characterDict = new Dictionary<int, CharacterData>();
        private Dictionary<int, WeaponData> weaponDict = new Dictionary<int, WeaponData>();
        private Dictionary<int, EnemyData> enemyDict = new Dictionary<int, EnemyData>();
        private Dictionary<int, WaveData> waveDict = new Dictionary<int, WaveData>();
        private Dictionary<string, WaveData> waveLookup = new Dictionary<string, WaveData>();
        private Dictionary<int, ItemData> itemDict = new Dictionary<int, ItemData>();
        private Dictionary<int, ShopData> shopDict = new Dictionary<int, ShopData>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadAllConfigs();
        }

        private void LoadAllConfigs()
        {
            LoadCharacters();
            LoadWeapons();
            LoadEnemies();
            LoadWaves();
            LoadItems();
            LoadShops();

            Debug.Log("[ConfigManager] All configs loaded. " +
                      $"Characters: {characterDict.Count}, " +
                      $"Weapons: {weaponDict.Count}, " +
                      $"Enemies: {enemyDict.Count}, " +
                      $"Waves: {waveDict.Count}, " +
                      $"Items: {itemDict.Count}, " +
                      $"Shops: {shopDict.Count}");
        }

        private void LoadCharacters()
        {
            var json = LoadJsonText("Configs/characters");
            if (json == null) return;

            var db = JsonUtility.FromJson<ConfigDatabase<CharacterData>>(json);
            if (db == null || db.items == null) return;

            characterDict.Clear();
            foreach (var item in db.items)
            {
                characterDict[item.id] = item;
            }
        }

        private void LoadWeapons()
        {
            var json = LoadJsonText("Configs/weapons");
            if (json == null) return;

            var db = JsonUtility.FromJson<ConfigDatabase<WeaponData>>(json);
            if (db == null || db.items == null) return;

            weaponDict.Clear();
            foreach (var item in db.items)
            {
                weaponDict[item.id] = item;
            }
        }

        private void LoadEnemies()
        {
            var json = LoadJsonText("Configs/enemies");
            if (json == null) return;

            var db = JsonUtility.FromJson<ConfigDatabase<EnemyData>>(json);
            if (db == null || db.items == null) return;

            enemyDict.Clear();
            foreach (var item in db.items)
            {
                enemyDict[item.id] = item;
            }
        }

        private void LoadWaves()
        {
            var json = LoadJsonText("Configs/waves");
            if (json == null) return;

            var db = JsonUtility.FromJson<ConfigDatabase<WaveData>>(json);
            if (db == null || db.items == null) return;

            waveDict.Clear();
            waveLookup.Clear();
            foreach (var item in db.items)
            {
                waveDict[item.id] = item;
                string key = WaveKey(item.stageIndex, item.waveIndex);
                waveLookup[key] = item;
            }
        }

        private void LoadItems()
        {
            var json = LoadJsonText("Configs/items");
            if (json == null) return;

            var db = JsonUtility.FromJson<ConfigDatabase<ItemData>>(json);
            if (db == null || db.items == null) return;

            itemDict.Clear();
            foreach (var item in db.items)
            {
                itemDict[item.id] = item;
            }
        }

        private void LoadShops()
        {
            var json = LoadJsonText("Configs/shops");
            if (json == null) return;

            var db = JsonUtility.FromJson<ConfigDatabase<ShopData>>(json);
            if (db == null || db.items == null) return;

            shopDict.Clear();
            foreach (var item in db.items)
            {
                shopDict[item.id] = item;
            }
        }

        private string LoadJsonText(string resourcePath)
        {
            var textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset == null)
            {
                Debug.LogWarning($"[ConfigManager] Config not found at Resources/{resourcePath}");
                return null;
            }
            return textAsset.text;
        }

        private static string WaveKey(int stageIndex, int waveIndex)
        {
            return $"{stageIndex}_{waveIndex}";
        }

        // --- Public Accessors ---

        public CharacterData GetCharacter(int id)
        {
            characterDict.TryGetValue(id, out var data);
            return data;
        }

        public WeaponData GetWeapon(int id)
        {
            weaponDict.TryGetValue(id, out var data);
            return data;
        }

        public EnemyData GetEnemy(int id)
        {
            enemyDict.TryGetValue(id, out var data);
            return data;
        }

        public WaveData GetWave(int stageIndex, int waveIndex)
        {
            string key = WaveKey(stageIndex, waveIndex);
            waveLookup.TryGetValue(key, out var data);
            return data;
        }

        public WaveData GetWaveById(int id)
        {
            waveDict.TryGetValue(id, out var data);
            return data;
        }

        public ItemData GetItem(int id)
        {
            itemDict.TryGetValue(id, out var data);
            return data;
        }

        public ShopData GetShop(int id)
        {
            shopDict.TryGetValue(id, out var data);
            return data;
        }

        public List<CharacterData> GetAllCharacters()
        {
            return new List<CharacterData>(characterDict.Values);
        }

        public List<WeaponData> GetAllWeapons()
        {
            return new List<WeaponData>(weaponDict.Values);
        }

        public List<EnemyData> GetAllEnemies()
        {
            return new List<EnemyData>(enemyDict.Values);
        }

        public List<WaveData> GetAllWaves()
        {
            return new List<WaveData>(waveDict.Values);
        }

        public List<ItemData> GetAllItems()
        {
            return new List<ItemData>(itemDict.Values);
        }

        public List<ShopData> GetAllShops()
        {
            return new List<ShopData>(shopDict.Values);
        }
    }
}
