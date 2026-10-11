using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Museum.Tests
{
    public class ExhibitInfoPanelTests
    {
        GameObject root;
        ExhibitInfoPanel panel;
        CanvasGroup group;
        TextMeshProUGUI title, meta, body, hint;
        ExhibitInfo info;

        static TextMeshProUGUI MakeText(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.AddComponent<TextMeshProUGUI>();
        }

        [SetUp]
        public void SetUp()
        {
            // Edit Mode does not run Awake on plain MonoBehaviours, so no LogError is raised here.
            root = new GameObject("Panel", typeof(RectTransform));
            group = root.AddComponent<CanvasGroup>();
            title = MakeText(root.transform, "t");
            meta = MakeText(root.transform, "m");
            body = MakeText(root.transform, "b");
            hint = MakeText(root.transform, "h");
            panel = root.AddComponent<ExhibitInfoPanel>();

            var so = new SerializedObject(panel);
            so.FindProperty("group").objectReferenceValue = group;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("metaText").objectReferenceValue = meta;
            so.FindProperty("bodyText").objectReferenceValue = body;
            so.FindProperty("hintText").objectReferenceValue = hint;
            so.ApplyModifiedPropertiesWithoutUndo();

            info = ScriptableObject.CreateInstance<ExhibitInfo>();
            info.title = "Tượng Phật";
            info.period = "Thế kỷ XV";
            info.material = "Đá sa thạch";
            info.origin = "";
            info.summary = "Tóm tắt";
            info.story = "Câu chuyện dài";
            info.source = "Bảo tàng";
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(info);
        }

        [Test] public void JoinMeta_AllParts() =>
            Assert.AreEqual("a · b · c", ExhibitInfoPanel.JoinMeta("a", "b", "c"));

        [Test] public void JoinMeta_SkipsNullEmptyWhitespace() =>
            Assert.AreEqual("a · c", ExhibitInfoPanel.JoinMeta("a", null, "", "  ", "c"));

        [Test] public void JoinMeta_AllEmpty_ReturnsEmpty() =>
            Assert.AreEqual("", ExhibitInfoPanel.JoinMeta("", null, " "));

        [Test] public void JoinMeta_NullArray_ReturnsEmpty() =>
            Assert.AreEqual("", ExhibitInfoPanel.JoinMeta(null));

        [Test] public void JoinMeta_PreservesVietnamese() =>
            Assert.AreEqual("Thế kỷ XV · Đá sa thạch", ExhibitInfoPanel.JoinMeta("Thế kỷ XV", "Đá sa thạch"));

        [Test] public void Show_Collapsed_ShowsSummaryAndHint()
        {
            panel.Show(info, false);
            Assert.AreEqual("Tượng Phật", title.text);
            Assert.AreEqual("Thế kỷ XV · Đá sa thạch", meta.text);
            Assert.AreEqual("Tóm tắt", body.text);
            Assert.AreEqual("Nhấn E để đọc thêm", hint.text);
            Assert.AreEqual(1f, group.alpha);
            Assert.IsFalse(group.blocksRaycasts);
            Assert.IsFalse(group.interactable);
        }

        [Test] public void Show_Expanded_ShowsStoryAndSource()
        {
            panel.Show(info, true);
            Assert.AreEqual("Câu chuyện dài\n\nNguồn: Bảo tàng", body.text);
            Assert.AreEqual("Nhấn E để thu gọn", hint.text);
            Assert.AreEqual(1f, group.alpha);
        }

        [Test] public void Show_ExpandedEmptySource_NoSourceLine()
        {
            info.source = "  ";
            panel.Show(info, true);
            Assert.AreEqual("Câu chuyện dài", body.text);
            StringAssert.DoesNotContain("Nguồn:", body.text);
        }

        [Test] public void Show_Null_HidesPanel()
        {
            panel.Show(info, false);
            panel.Show(null, false);
            Assert.AreEqual(0f, group.alpha);
        }

        [Test] public void Hide_SetsAlphaZero()
        {
            panel.Show(info, false);
            panel.Hide();
            Assert.AreEqual(0f, group.alpha);
        }

        [Test] public void Show_WithHintOverride_UsesIt()
        {
            var show = typeof(ExhibitInfoPanel).GetMethod("Show", new[] { typeof(ExhibitInfo), typeof(bool), typeof(string) });
            Assert.IsNotNull(show, "Show must accept hintOverride");
            show.Invoke(panel, new object[] { info, true, "Kéo chuột trái để xoay · Cuộn để phóng to · E hoặc Esc để quay lại" });
            Assert.AreEqual("Kéo chuột trái để xoay · Cuộn để phóng to · E hoặc Esc để quay lại", hint.text);
            Assert.AreEqual("Câu chuyện dài\n\nNguồn: Bảo tàng", body.text);
        }

        [Test] public void Show_WithoutHintOverride_KeepsDefaultHint()
        {
            panel.Show(info, false);
            Assert.AreEqual("Nhấn E để đọc thêm", hint.text);
            panel.Show(info, true);
            Assert.AreEqual("Nhấn E để thu gọn", hint.text);
        }
    }
}
