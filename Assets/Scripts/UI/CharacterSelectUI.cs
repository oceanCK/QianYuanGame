using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CatBrotato.Core;
using CatBrotato.Data;

namespace CatBrotato.UI
{
    /// <summary>
    /// Lists characters from ConfigManager, lets the player click one to start a run.
    /// Card panel on the right shows full stats for the highlighted character.
    /// </summary>
    public class CharacterSelectUI : MonoBehaviour
    {
        private RectTransform listContent;
        private Text detailName;
        private Text detailStats;
        private Text detailPassive;
        private Button confirmBtn;
        private Button backBtn;

        private readonly List<CharacterData> characters = new List<CharacterData>();
        private CharacterData selected;

        private void Awake()
        {
            BuildUI();
        }

        private void OnEnable()
        {
            RefreshCharacterList();
        }

        private void BuildUI()
        {
            UIBuilder.CreatePanel("Background", transform, new Color(0.06f, 0.04f, 0.10f, 1f));

            Text title = UIBuilder.CreateText("Title", transform, "选择角色 — Choose Your Cat", 42, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.5f));
            UIBuilder.SetAnchoredPos(title.rectTransform, new Vector2(0.5f, 0.93f), Vector2.zero, new Vector2(900f, 70f));

            // List panel (left side)
            GameObject listPanel = UIBuilder.CreatePanel("ListPanel", transform, new Color(0.10f, 0.08f, 0.15f, 0.85f));
            RectTransform listRt = listPanel.GetComponent<RectTransform>();
            UIBuilder.Anchor(listRt, new Vector2(0.05f, 0.15f), new Vector2(0.45f, 0.88f), Vector2.zero, Vector2.zero);

            GameObject scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollGo.transform.SetParent(listPanel.transform, false);
            RectTransform scrollRt = scrollGo.GetComponent<RectTransform>();
            UIBuilder.Anchor(scrollRt, Vector2.zero, Vector2.one, new Vector2(8f, 8f), new Vector2(-8f, -8f));
            scrollGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.25f);
            ScrollRect scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollGo.transform, false);
            RectTransform vpRt = viewport.GetComponent<RectTransform>();
            UIBuilder.Anchor(vpRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;
            scroll.viewport = vpRt;

            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            listContent = content.GetComponent<RectTransform>();
            listContent.anchorMin = new Vector2(0f, 1f);
            listContent.anchorMax = new Vector2(1f, 1f);
            listContent.pivot = new Vector2(0.5f, 1f);
            listContent.anchoredPosition = Vector2.zero;
            VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = listContent;

            // Detail panel (right side)
            GameObject detailPanel = UIBuilder.CreatePanel("DetailPanel", transform, new Color(0.10f, 0.08f, 0.15f, 0.85f));
            RectTransform detailRt = detailPanel.GetComponent<RectTransform>();
            UIBuilder.Anchor(detailRt, new Vector2(0.50f, 0.15f), new Vector2(0.95f, 0.88f), Vector2.zero, Vector2.zero);

            detailName = UIBuilder.CreateText("DetailName", detailPanel.transform, "—", 32, TextAnchor.UpperLeft, new Color(1f, 0.9f, 0.6f));
            RectTransform nameRt = detailName.rectTransform;
            UIBuilder.Anchor(nameRt, new Vector2(0f, 0.85f), new Vector2(1f, 1f), new Vector2(20f, 0f), new Vector2(-20f, -10f));

            detailStats = UIBuilder.CreateText("DetailStats", detailPanel.transform, "", 20, TextAnchor.UpperLeft, new Color(0.9f, 0.9f, 0.95f));
            RectTransform statsRt = detailStats.rectTransform;
            UIBuilder.Anchor(statsRt, new Vector2(0f, 0.35f), new Vector2(1f, 0.85f), new Vector2(20f, 10f), new Vector2(-20f, -10f));

            detailPassive = UIBuilder.CreateText("DetailPassive", detailPanel.transform, "", 18, TextAnchor.UpperLeft, new Color(0.85f, 1f, 0.85f));
            RectTransform passiveRt = detailPassive.rectTransform;
            UIBuilder.Anchor(passiveRt, new Vector2(0f, 0.05f), new Vector2(1f, 0.35f), new Vector2(20f, 10f), new Vector2(-20f, -10f));

            // Bottom buttons
            backBtn = UIBuilder.CreateButton("BackButton", transform, "返回 (Back)", new Vector2(180f, 50f), new Color(0.45f, 0.3f, 0.3f));
            UIBuilder.SetAnchoredPos(backBtn.GetComponent<RectTransform>(), new Vector2(0.2f, 0.07f), Vector2.zero, new Vector2(180f, 50f));
            backBtn.onClick.AddListener(OnBackClicked);

            confirmBtn = UIBuilder.CreateButton("ConfirmButton", transform, "出击 (Start Run)", new Vector2(240f, 60f), new Color(0.3f, 0.65f, 0.45f));
            UIBuilder.SetAnchoredPos(confirmBtn.GetComponent<RectTransform>(), new Vector2(0.7f, 0.07f), Vector2.zero, new Vector2(240f, 60f));
            confirmBtn.onClick.AddListener(OnConfirmClicked);
            confirmBtn.interactable = false;
        }

        private void RefreshCharacterList()
        {
            if (listContent == null) return;
            for (int i = listContent.childCount - 1; i >= 0; i--)
            {
                Destroy(listContent.GetChild(i).gameObject);
            }
            characters.Clear();
            selected = null;
            UpdateDetailPanel();
            if (confirmBtn != null) confirmBtn.interactable = false;

            if (ConfigManager.Instance == null) return;
            var all = ConfigManager.Instance.GetAllCharacters();
            if (all == null) return;
            characters.AddRange(all);

            for (int i = 0; i < characters.Count; i++)
            {
                CreateCharacterRow(characters[i]);
            }

            if (characters.Count > 0)
            {
                Select(characters[0]);
            }
        }

        private void CreateCharacterRow(CharacterData data)
        {
            Color bg = ColorForRoleClass(data.roleClass);
            Button row = UIBuilder.CreateButton($"Char_{data.id}", listContent, $"{data.displayName}  [{data.roleClass}]", new Vector2(0f, 56f), bg);
            LayoutElement le = row.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 56f;
            le.preferredHeight = 56f;
            CharacterData captured = data;
            row.onClick.AddListener(() => Select(captured));
        }

        private void Select(CharacterData data)
        {
            selected = data;
            UpdateDetailPanel();
            if (confirmBtn != null) confirmBtn.interactable = data != null;
        }

        private void UpdateDetailPanel()
        {
            if (detailName == null) return;
            if (selected == null)
            {
                detailName.text = "—";
                detailStats.text = "";
                detailPassive.text = "";
                return;
            }
            detailName.text = $"{selected.displayName}  ({selected.internalName})";
            detailStats.text =
                $"职业: {selected.roleClass}    猫种: {selected.catCategory}\n" +
                $"毛色: {selected.colorType}    花纹: {selected.patternType}    毛型: {selected.furType}\n\n" +
                $"生命: {selected.baseHp:0}    移速: {selected.baseMoveSpeed:0.0}    护甲: {selected.baseArmor:0}\n" +
                $"近战: {selected.baseMeleeDamage:0}    远程: {selected.baseRangedDamage:0}    攻速: {selected.baseAttackSpeed:0.00}\n" +
                $"暴击: {selected.baseCritChance:P0}    闪避: {selected.baseDodge:P0}    幸运: {selected.baseLuck:0}    采集: {selected.baseHarvesting:0}";
            detailPassive.text = string.IsNullOrEmpty(selected.passiveDesc)
                ? ""
                : $"被动: {selected.passiveDesc}";
        }

        private void OnConfirmClicked()
        {
            if (selected == null) return;
            if (GameManager.Instance == null) return;
            GameManager.Instance.StartGame(selected.id);
        }

        private void OnBackClicked()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.ChangeState(GameState.Menu);
        }

        private static Color ColorForRoleClass(RoleClass roleClass)
        {
            switch (roleClass)
            {
                case RoleClass.Balanced: return new Color(0.40f, 0.40f, 0.45f);
                case RoleClass.Melee: return new Color(0.55f, 0.30f, 0.30f);
                case RoleClass.Ranged: return new Color(0.30f, 0.45f, 0.55f);
                case RoleClass.Mage: return new Color(0.45f, 0.30f, 0.55f);
                case RoleClass.Tank: return new Color(0.40f, 0.40f, 0.40f);
                case RoleClass.Support: return new Color(0.30f, 0.55f, 0.45f);
                case RoleClass.Lucky: return new Color(0.55f, 0.50f, 0.25f);
                case RoleClass.Special: return new Color(0.50f, 0.30f, 0.45f);
                default: return new Color(0.35f, 0.35f, 0.40f);
            }
        }
    }
}
