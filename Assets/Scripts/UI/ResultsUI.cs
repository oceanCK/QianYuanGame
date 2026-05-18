using UnityEngine;
using UnityEngine.UI;
using CatBrotato.Core;
using CatBrotato.Wave;

namespace CatBrotato.UI
{
    /// <summary>
    /// End-of-run screen. Shown on Game Over or All Waves Complete.
    /// Displays victory/defeat headline, wave reached, materials gathered, and offers
    /// buttons to retry or return to the main menu.
    /// </summary>
    public class ResultsUI : MonoBehaviour
    {
        private Text headline;
        private Text summary;
        private Button retryBtn;
        private Button menuBtn;

        [SerializeField] private WaveManager waveManager;

        private bool victory;

        private void Awake()
        {
            BuildUI();
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameOver += HandleGameOver;
            }
            if (waveManager != null)
            {
                waveManager.OnAllWavesCompleted += HandleAllWavesCompleted;
            }
            RefreshSummary();
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameOver -= HandleGameOver;
            }
            if (waveManager != null)
            {
                waveManager.OnAllWavesCompleted -= HandleAllWavesCompleted;
            }
        }

        public void Bind(WaveManager waves)
        {
            if (waveManager != null)
            {
                waveManager.OnAllWavesCompleted -= HandleAllWavesCompleted;
            }
            waveManager = waves;
            if (waveManager != null && enabled && gameObject.activeInHierarchy)
            {
                waveManager.OnAllWavesCompleted += HandleAllWavesCompleted;
            }
        }

        private void BuildUI()
        {
            UIBuilder.CreatePanel("Background", transform, new Color(0.04f, 0.03f, 0.08f, 1f));

            headline = UIBuilder.CreateText("Headline", transform, "失败 — Defeat", 64, TextAnchor.MiddleCenter, new Color(1f, 0.5f, 0.5f));
            UIBuilder.SetAnchoredPos(headline.rectTransform, new Vector2(0.5f, 0.78f), Vector2.zero, new Vector2(900f, 100f));

            summary = UIBuilder.CreateText("Summary", transform, "", 24, TextAnchor.MiddleCenter, new Color(0.92f, 0.92f, 0.95f));
            UIBuilder.SetAnchoredPos(summary.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800f, 260f));

            retryBtn = UIBuilder.CreateButton("RetryButton", transform, "再战 (Retry)", new Vector2(240f, 60f), new Color(0.3f, 0.65f, 0.45f));
            UIBuilder.SetAnchoredPos(retryBtn.GetComponent<RectTransform>(), new Vector2(0.35f, 0.18f), Vector2.zero, new Vector2(240f, 60f));
            retryBtn.onClick.AddListener(OnRetryClicked);

            menuBtn = UIBuilder.CreateButton("MenuButton", transform, "返回主菜单 (Menu)", new Vector2(240f, 60f), new Color(0.45f, 0.3f, 0.5f));
            UIBuilder.SetAnchoredPos(menuBtn.GetComponent<RectTransform>(), new Vector2(0.65f, 0.18f), Vector2.zero, new Vector2(240f, 60f));
            menuBtn.onClick.AddListener(OnMenuClicked);
        }

        private void HandleGameOver()
        {
            victory = false;
            RefreshSummary();
        }

        private void HandleAllWavesCompleted()
        {
            victory = true;
            RefreshSummary();
        }

        private void RefreshSummary()
        {
            if (headline == null) return;

            if (victory)
            {
                headline.text = "胜利 — Victory!";
                headline.color = new Color(0.7f, 1f, 0.7f);
            }
            else
            {
                headline.text = "失败 — Defeat";
                headline.color = new Color(1f, 0.5f, 0.5f);
            }

            int waveReached = 0;
            int kills = 0;
            if (waveManager != null)
            {
                waveReached = waveManager.CurrentWaveIndex;
                kills = waveManager.EnemiesKilledThisWave;
            }
            else if (GameManager.Instance != null)
            {
                waveReached = GameManager.Instance.CurrentWaveIndex;
            }

            int materials = GameManager.Instance != null ? GameManager.Instance.PlayerMaterials : 0;

            summary.text =
                $"到达波次: {waveReached}\n" +
                $"剩余材料: {materials}\n" +
                $"本波击杀: {kills}\n\n" +
                (victory ? "你拯救了千缘猫域！" : "再接再厉，下一次一定！");
        }

        private void OnRetryClicked()
        {
            if (GameManager.Instance == null) return;
            int charId = GameManager.Instance.SelectedCharacterId;
            GameManager.Instance.StartGame(charId);
        }

        private void OnMenuClicked()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.ReturnToMenu();
        }
    }
}
