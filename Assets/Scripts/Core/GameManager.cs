using System;
using UnityEngine;
using CatBrotato.Data;

namespace CatBrotato.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action<GameState, GameState> OnGameStateChanged;
        public event Action<int> OnWaveStarted;
        public event Action<int> OnWaveEnded;
        public event Action OnBossFight;
        public event Action OnGameOver;

        [SerializeField] private GameState initialState = GameState.Boot;

        public GameState CurrentState { get; private set; }
        public int CurrentWaveIndex { get; private set; }
        public int PlayerMaterials { get; set; }
        public int SelectedCharacterId { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            CurrentState = initialState;
            CurrentWaveIndex = 0;
            PlayerMaterials = 0;
        }

        private void Update()
        {
            switch (CurrentState)
            {
                case GameState.Boot:
                    // Wait for initialization, then move to menu
                    break;
                case GameState.Menu:
                    break;
                case GameState.CharacterSelect:
                    break;
                case GameState.Battle:
                    break;
                case GameState.Shop:
                    break;
                case GameState.Results:
                    break;
            }
        }

        public void ChangeState(GameState newState)
        {
            if (newState == CurrentState) return;

            GameState previousState = CurrentState;
            CurrentState = newState;
            OnGameStateChanged?.Invoke(previousState, newState);
        }

        public void StartGame(int characterId)
        {
            SelectedCharacterId = characterId;
            CurrentWaveIndex = 0;
            PlayerMaterials = 0;
            ChangeState(GameState.Battle);
            StartNextWave();
        }

        public void StartNextWave()
        {
            CurrentWaveIndex++;
            OnWaveStarted?.Invoke(CurrentWaveIndex);

            // Check for boss waves (every 5 waves)
            if (CurrentWaveIndex % 5 == 0)
            {
                OnBossFight?.Invoke();
            }
        }

        public void EndCurrentWave()
        {
            OnWaveEnded?.Invoke(CurrentWaveIndex);
            EnterShop();
        }

        public void EnterShop()
        {
            ChangeState(GameState.Shop);
        }

        public void ExitShop()
        {
            ChangeState(GameState.Battle);
            StartNextWave();
        }

        public void TriggerGameOver()
        {
            OnGameOver?.Invoke();
            ChangeState(GameState.Results);
        }

        public void ReturnToMenu()
        {
            CurrentWaveIndex = 0;
            PlayerMaterials = 0;
            ChangeState(GameState.Menu);
        }

        public void AddMaterials(int amount)
        {
            PlayerMaterials += amount;
        }

        public bool SpendMaterials(int amount)
        {
            if (PlayerMaterials < amount) return false;
            PlayerMaterials -= amount;
            return true;
        }
    }
}
