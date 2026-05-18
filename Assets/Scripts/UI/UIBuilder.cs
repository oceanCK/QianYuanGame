using UnityEngine;
using UnityEngine.UI;

namespace CatBrotato.UI
{
    /// <summary>
    /// Static helpers for building runtime uGUI primitives. Used by UI panels that
    /// construct themselves in code so the demo doesn't depend on prefab assets.
    /// </summary>
    public static class UIBuilder
    {
        public static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image img = go.GetComponent<Image>();
            img.color = color;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        public static Text CreateText(string name, Transform parent, string content, int fontSize, TextAnchor anchor, Color? color = null)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            Text txt = go.GetComponent<Text>();
            txt.text = content;
            txt.fontSize = fontSize;
            txt.alignment = anchor;
            txt.color = color ?? Color.white;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            return txt;
        }

        public static Button CreateButton(string name, Transform parent, string label, Vector2 size, Color bgColor)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            Image img = go.GetComponent<Image>();
            img.color = bgColor;
            Button btn = go.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = bgColor;
            cb.highlightedColor = Color.Lerp(bgColor, Color.white, 0.2f);
            cb.pressedColor = Color.Lerp(bgColor, Color.black, 0.2f);
            cb.disabledColor = new Color(bgColor.r * 0.5f, bgColor.g * 0.5f, bgColor.b * 0.5f, 0.5f);
            btn.colors = cb;

            Text labelTxt = CreateText("Label", go.transform, label, 18, TextAnchor.MiddleCenter);
            RectTransform labelRt = labelTxt.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            return btn;
        }

        public static Image CreateImage(string name, Transform parent, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image img = go.GetComponent<Image>();
            img.color = color;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            return img;
        }

        public static void Anchor(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        public static void SetAnchoredPos(RectTransform rt, Vector2 anchor, Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
        }

        public static Color RarityColor(CatBrotato.Data.Rarity rarity)
        {
            switch (rarity)
            {
                case CatBrotato.Data.Rarity.Common: return new Color(0.75f, 0.75f, 0.75f);
                case CatBrotato.Data.Rarity.Uncommon: return new Color(0.4f, 0.85f, 0.4f);
                case CatBrotato.Data.Rarity.Rare: return new Color(0.35f, 0.6f, 1f);
                case CatBrotato.Data.Rarity.Epic: return new Color(0.75f, 0.45f, 1f);
                case CatBrotato.Data.Rarity.Legendary: return new Color(1f, 0.7f, 0.2f);
                default: return Color.white;
            }
        }
    }
}
