using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace Museum.Tests
{
    public class FadeTests
    {
        readonly List<Object> created = new List<Object>();

        static void Set(object target, string field, object value)
        {
            var f = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsNotNull(f, field);
            f.SetValue(target, value);
        }

        T Track<T>(T o) where T : Object { created.Add(o); return o; }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            foreach (var o in created) if (o != null) Object.Destroy(o);
            created.Clear();
        }

        (ExhibitInfoPanel panel, CanvasGroup group, ExhibitInfo info) MakePanel(float fadeSeconds)
        {
            var go = Track(new GameObject("Panel", typeof(RectTransform)));
            go.SetActive(false);
            var group = go.AddComponent<CanvasGroup>();
            TextMeshProUGUI Txt(string n)
            {
                var g = new GameObject(n, typeof(RectTransform));
                g.transform.SetParent(go.transform, false);
                return g.AddComponent<TextMeshProUGUI>();
            }
            var panel = go.AddComponent<ExhibitInfoPanel>();
            Set(panel, "group", group);
            Set(panel, "titleText", Txt("t"));
            Set(panel, "metaText", Txt("m"));
            Set(panel, "bodyText", Txt("b"));
            Set(panel, "hintText", Txt("h"));
            Set(panel, "fadeSeconds", fadeSeconds);
            go.SetActive(true);

            var info = Track(ScriptableObject.CreateInstance<ExhibitInfo>());
            info.title = "Tượng";
            info.summary = "Tóm tắt";
            return (panel, group, info);
        }

        [UnityTest]
        public IEnumerator Panel_FadesIn_Gradually()
        {
            var (panel, group, info) = MakePanel(0.5f);
            yield return null;
            panel.Show(info, false);
            Assert.AreEqual(0f, group.alpha, "Show must not jump to full alpha");
            yield return null;
            Assert.Greater(group.alpha, 0f);
            Assert.Less(group.alpha, 1f);
            yield return new WaitForSecondsRealtime(0.7f);
            Assert.AreEqual(1f, group.alpha);
        }

        [UnityTest]
        public IEnumerator Panel_FadesOut_ToZero()
        {
            var (panel, group, info) = MakePanel(0.5f);
            panel.Show(info, false);
            yield return new WaitForSecondsRealtime(0.7f);
            panel.Hide();
            yield return new WaitForSecondsRealtime(0.7f);
            Assert.AreEqual(0f, group.alpha);
        }

        [UnityTest]
        public IEnumerator Panel_ShowDuringFadeOut_ContinuesFromCurrentAlpha()
        {
            var (panel, group, info) = MakePanel(0.5f);
            panel.Show(info, false);
            yield return new WaitForSecondsRealtime(0.7f);
            panel.Hide();
            yield return null;
            float mid = group.alpha;
            Assert.Less(mid, 1f);
            panel.Show(info, false);
            Assert.AreEqual(mid, group.alpha, "Show must not reset alpha");
            yield return null;
            Assert.GreaterOrEqual(group.alpha, mid);
        }

        [UnityTest]
        public IEnumerator Panel_FadesWhileTimeScaleIsZero()
        {
            var (panel, group, info) = MakePanel(0.5f);
            Time.timeScale = 0f;
            panel.Show(info, false);
            yield return new WaitForSecondsRealtime(0.7f);
            Assert.AreEqual(1f, group.alpha);
        }

        GameObject MakeFader(float seconds)
        {
            var go = Track(new GameObject("ScreenFade"));
            go.SetActive(false);
            var fader = go.AddComponent<ScreenFader>();
            Set(fader, "seconds", seconds);
            return go;
        }

        [UnityTest]
        public IEnumerator ScreenFader_StartsBlack_FadesOut_ThenDeactivates()
        {
            var go = MakeFader(0.3f);
            go.SetActive(true);
            var group = go.GetComponent<CanvasGroup>();
            Assert.AreEqual(1f, group.alpha);
            Assert.IsFalse(group.blocksRaycasts);
            yield return null;
            Assert.Less(group.alpha, 1f);
            yield return new WaitForSecondsRealtime(1f);
            Assert.AreEqual(0f, group.alpha);
            Assert.IsFalse(go.activeSelf);
        }

        [Test]
        public void ScreenFader_ZeroSeconds_DeactivatesImmediately()
        {
            var go = MakeFader(0f);
            go.SetActive(true);
            Assert.IsFalse(go.activeSelf);
            Assert.AreEqual(0f, go.GetComponent<CanvasGroup>().alpha);
        }
    }
}
