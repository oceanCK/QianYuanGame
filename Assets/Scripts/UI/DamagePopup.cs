using UnityEngine;
using UnityEngine.UI;

namespace CatBrotato.UI
{
    /// <summary>
    /// Floating world-space damage number. Spawns a self-managed Canvas root once,
    /// then attaches short-lived popups under it. Combat code calls
    /// <see cref="Show(Vector3, float, bool)"/> to display a number that floats up and fades out.
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        private const float Lifetime = 0.7f;
        private const float FloatDistance = 1.2f;
        private const float WorldCanvasScale = 0.01f;

        private static Transform popupRoot;

        private Text label;
        private RectTransform rect;
        private Vector3 startPos;
        private float age;

        public static void Show(Vector3 worldPos, float damage, bool crit = false)
        {
            EnsureRoot();

            GameObject go = new GameObject("DamagePopup", typeof(RectTransform));
            go.transform.SetParent(popupRoot, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200f, 60f);
            rt.localScale = Vector3.one;

            Text txt = go.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.alignment = TextAnchor.MiddleCenter;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;

            int rounded = Mathf.Max(1, Mathf.RoundToInt(damage));
            if (crit)
            {
                txt.text = $"{rounded}!";
                txt.fontSize = 56;
                txt.fontStyle = FontStyle.Bold;
                txt.color = new Color(1f, 0.85f, 0.25f);
            }
            else
            {
                txt.text = rounded.ToString();
                txt.fontSize = 42;
                txt.color = Color.white;
            }

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);

            DamagePopup popup = go.AddComponent<DamagePopup>();
            popup.label = txt;
            popup.rect = rt;
            float jitterX = Random.Range(-0.25f, 0.25f);
            popup.startPos = worldPos + new Vector3(jitterX, 0.4f, 0f);
            go.transform.position = popup.startPos;
        }

        private static void EnsureRoot()
        {
            if (popupRoot != null) return;

            GameObject rootGo = new GameObject("DamagePopupCanvas");
            Object.DontDestroyOnLoad(rootGo);
            Canvas canvas = rootGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;
            RectTransform rootRt = rootGo.GetComponent<RectTransform>();
            rootRt.sizeDelta = new Vector2(1f, 1f);
            rootGo.transform.localScale = Vector3.one * WorldCanvasScale;

            popupRoot = rootGo.transform;
        }

        private void Update()
        {
            age += Time.deltaTime;
            float t = age / Lifetime;
            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = startPos + Vector3.up * (FloatDistance * t);

            if (label != null)
            {
                Color c = label.color;
                c.a = 1f - t * t;
                label.color = c;
            }
        }
    }
}
