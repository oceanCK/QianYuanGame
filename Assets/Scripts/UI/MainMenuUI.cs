using UnityEngine;
using UnityEngine.UI;
using CatBrotato.Core;

namespace CatBrotato.UI
{
    /// <summary>
    /// Title screen with a "Start" button that transitions to character select.
    /// Builds its UI procedurally on Awake.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        private void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            UIBuilder.CreatePanel("Background", transform, new Color(0.08f, 0.05f, 0.12f, 1f));

            Text title = UIBuilder.CreateText("Title", transform, "千缘猫域", 72, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.5f));
            UIBuilder.SetAnchoredPos(title.rectTransform, new Vector2(0.5f, 0.7f), Vector2.zero, new Vector2(800f, 120f));

            Text subtitle = UIBuilder.CreateText("Subtitle", transform, "QianYuan Cat Domain — Brotato Demo", 24, TextAnchor.MiddleCenter, new Color(0.85f, 0.85f, 0.95f));
            UIBuilder.SetAnchoredPos(subtitle.rectTransform, new Vector2(0.5f, 0.6f), Vector2.zero, new Vector2(800f, 50f));

            Button startBtn = UIBuilder.CreateButton("StartButton", transform, "开始游戏 (Start)", new Vector2(280f, 70f), new Color(0.3f, 0.65f, 0.45f));
            UIBuilder.SetAnchoredPos(startBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(280f, 70f));
            startBtn.onClick.AddListener(OnStartClicked);

            Button quitBtn = UIBuilder.CreateButton("QuitButton", transform, "退出 (Quit)", new Vector2(200f, 50f), new Color(0.55f, 0.3f, 0.3f));
            UIBuilder.SetAnchoredPos(quitBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.27f), Vector2.zero, new Vector2(200f, 50f));
            quitBtn.onClick.AddListener(OnQuitClicked);

            Text hint = UIBuilder.CreateText("Hint", transform, "WASD 移动 · 自动攻击 · 关卡间进入商店", 18, TextAnchor.MiddleCenter, new Color(0.7f, 0.7f, 0.8f));
            UIBuilder.SetAnchoredPos(hint.rectTransform, new Vector2(0.5f, 0.12f), Vector2.zero, new Vector2(700f, 40f));
        }

        private void OnStartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.CharacterSelect);
            }
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
