using UnityEngine;
using CatBrotato.Core;

namespace CatBrotato.UI
{
    /// <summary>
    /// Owns top-level UI panels and toggles them in response to GameManager state changes.
    /// Panels are assigned by SceneSetup at boot.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private MainMenuUI mainMenu;
        [SerializeField] private CharacterSelectUI characterSelect;
        [SerializeField] private BattleHUD battleHud;
        [SerializeField] private ShopUI shopUi;
        [SerializeField] private ResultsUI resultsUi;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
                ApplyState(GameManager.Instance.CurrentState);
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        public void RegisterPanels(MainMenuUI menu, CharacterSelectUI select, BattleHUD hud, ShopUI shop, ResultsUI results)
        {
            mainMenu = menu;
            characterSelect = select;
            battleHud = hud;
            shopUi = shop;
            resultsUi = results;
            if (GameManager.Instance != null)
            {
                ApplyState(GameManager.Instance.CurrentState);
            }
        }

        private void HandleGameStateChanged(GameState previous, GameState next)
        {
            ApplyState(next);
        }

        private void ApplyState(GameState state)
        {
            SetActive(mainMenu, state == GameState.Menu || state == GameState.Boot);
            SetActive(characterSelect, state == GameState.CharacterSelect);
            SetActive(battleHud, state == GameState.Battle);
            SetActive(shopUi, state == GameState.Shop);
            SetActive(resultsUi, state == GameState.Results);
        }

        private static void SetActive(MonoBehaviour panel, bool active)
        {
            if (panel == null) return;
            if (panel.gameObject.activeSelf != active)
            {
                panel.gameObject.SetActive(active);
            }
        }
    }
}
