using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Museum
{
    public class ExhibitInfoPanel : MonoBehaviour
    {
        const float MinBodySize = 16f;

        [SerializeField] CanvasGroup group;
        [SerializeField] TMP_Text titleText, metaText, bodyText, hintText;
        [SerializeField, Min(0f)] float fadeSeconds; // 0 = show/hide instantly

        float baseBodySize, targetAlpha;

        void Awake()
        {
            if (group == null || titleText == null || metaText == null || bodyText == null || hintText == null)
                Debug.LogError("ExhibitInfoPanel: thiếu tham chiếu UI (group/titleText/metaText/bodyText/hintText).", this);
            Hide();
            if (group != null) group.alpha = 0f;
        }

        void Update()
        {
            if (group == null || fadeSeconds <= 0f) return;
            group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, Time.unscaledDeltaTime / fadeSeconds);
        }

        public void Show(ExhibitInfo info, bool expanded, string hintOverride = null)
        {
            if (info == null) { Hide(); return; }
            if (group == null) return;

            titleText.text = info.title;
            metaText.text = JoinMeta(info.period, info.material, info.origin);
            bodyText.text = expanded
                ? info.story + (string.IsNullOrWhiteSpace(info.source) ? "" : "\n\nNguồn: " + info.source)
                : info.summary;
            hintText.text = hintOverride ?? (expanded ? "Nhấn E để thu gọn" : "Nhấn E để đọc thêm");
            FitToParent();

            SetTargetAlpha(1f);
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        public void Hide() => SetTargetAlpha(0f);

        void SetTargetAlpha(float alpha)
        {
            targetAlpha = alpha;
            if (group != null && fadeSeconds <= 0f) group.alpha = alpha;
        }

        // Shrinks the body font until the panel fits its parent with the same margin on top as at the bottom.
        // ponytail: fits only on Show; refit on canvas size change if window resizing mid-read matters.
        void FitToParent()
        {
            if (baseBodySize <= 0f) baseBodySize = bodyText.fontSize;
            bodyText.fontSize = baseBodySize;
            var rt = transform as RectTransform;
            var parent = transform.parent as RectTransform;
            if (rt == null || parent == null) return;

            float maxHeight = parent.rect.height - 2f * rt.anchoredPosition.y;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            while (rt.rect.height > maxHeight && bodyText.fontSize > MinBodySize)
            {
                bodyText.fontSize = Mathf.Max(MinBodySize, bodyText.fontSize - 1f);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }
        }

        public static string JoinMeta(params string[] parts)
        {
            if (parts == null) return "";
            var list = new System.Collections.Generic.List<string>();
            foreach (var p in parts)
                if (!string.IsNullOrWhiteSpace(p)) list.Add(p);
            return string.Join(" · ", list);
        }
    }
}
