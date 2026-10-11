using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Museum.Tests
{
    // Mirrors the scene panel: bottom-left at (48, 48), 680 wide, VerticalLayoutGroup + ContentSizeFitter,
    // real Be Vietnam Pro fonts and the longest real story, inside a fixed-size frame.
    public class ExhibitInfoPanelFitTests
    {
        const float Margin = 48f, BaseBody = 22f, MinBody = 16f;
        GameObject canvasGo;
        RectTransform frame, panelRt;
        ExhibitInfoPanel panel;
        TextMeshProUGUI body;
        ExhibitInfo longest;

        TextMeshProUGUI MakeText(string name, string font, float size, float lineSpacing = 0f)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(panelRt, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Museum/Fonts/" + font + " SDF.asset");
            Assert.IsNotNull(t.font, font);
            t.fontSize = size;
            t.lineSpacing = lineSpacing;
            return t;
        }

        [SetUp]
        public void SetUp()
        {
            canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            frame = new GameObject("Frame", typeof(RectTransform)).GetComponent<RectTransform>();
            frame.SetParent(canvasGo.transform, false);

            var go = new GameObject("Panel", typeof(RectTransform));
            panelRt = go.GetComponent<RectTransform>();
            panelRt.SetParent(frame, false);
            panelRt.anchorMin = panelRt.anchorMax = panelRt.pivot = Vector2.zero;
            panelRt.anchoredPosition = new Vector2(Margin, Margin);
            panelRt.sizeDelta = new Vector2(680f, 0f);
            var group = go.AddComponent<CanvasGroup>();
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(32, 32, 28, 28);
            vlg.spacing = 10f;
            vlg.childControlWidth = vlg.childControlHeight = vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            go.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var title = MakeText("Title", "BeVietnamPro-SemiBold", 34f);
            var meta = MakeText("Meta", "BeVietnamPro-Regular", 20f);
            body = MakeText("Body", "BeVietnamPro-Regular", BaseBody, 8f);
            var hint = MakeText("Hint", "BeVietnamPro-Regular", 18f);

            panel = go.AddComponent<ExhibitInfoPanel>();
            var so = new SerializedObject(panel);
            so.FindProperty("group").objectReferenceValue = group;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("metaText").objectReferenceValue = meta;
            so.FindProperty("bodyText").objectReferenceValue = body;
            so.FindProperty("hintText").objectReferenceValue = hint;
            so.ApplyModifiedPropertiesWithoutUndo();

            longest = AssetDatabase.LoadAssetAtPath<ExhibitInfo>("Assets/Museum/Content/Info_Exhibit_Centerpiece.asset");
            Assert.IsNotNull(longest, "Info_Exhibit_Centerpiece missing");
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(canvasGo);

        void SetFrameHeight(float h) => frame.sizeDelta = new Vector2(1920f, h);

        // ContentSizeFitter updates lazily; rebuild so the height reflects the current text.
        float PanelTop
        {
            get
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(panelRt);
                return panelRt.anchoredPosition.y + panelRt.rect.height;
            }
        }

        [TestCase(1080f)] // 16:9
        [TestCase(935f)]  // 21:9 (2560x1080 with match 0.5)
        public void LongestStory_Expanded_FitsInsideFrame(float frameHeight)
        {
            SetFrameHeight(frameHeight);
            panel.Show(longest, true);
            Assert.LessOrEqual(PanelTop, frameHeight - Margin, "panel top overflows the frame");
            Assert.GreaterOrEqual(body.fontSize, MinBody, "body text below the 16 px minimum");
        }

        [Test]
        public void ShortText_AfterLongStory_RestoresBaseFontSize()
        {
            SetFrameHeight(935f);
            panel.Show(longest, true);
            Assert.Less(body.fontSize, BaseBody, "precondition: the long story should need a smaller font");
            panel.Show(longest, false);
            Assert.AreEqual(BaseBody, body.fontSize);
        }

        [Test]
        public void Summary_FitsAtBaseFontSize()
        {
            SetFrameHeight(1080f);
            panel.Show(longest, false);
            Assert.AreEqual(BaseBody, body.fontSize);
            Assert.LessOrEqual(PanelTop, 1080f - Margin);
        }

        [Test]
        public void TinyFrame_StopsAtMinimumFontSize()
        {
            SetFrameHeight(300f);
            panel.Show(longest, true);
            Assert.AreEqual(MinBody, body.fontSize);
        }
    }
}
