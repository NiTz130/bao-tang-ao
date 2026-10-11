#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Museum.Tests
{
    // Fields are set by reflection (consistent across this file).
    public class NarrativeDirectorPlayModeTests
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
            foreach (var o in created) if (o != null) Object.Destroy(o);
            created.Clear();
        }

        [UnityTest]
        public IEnumerator Director_ShowsAndHidesPanel_AsPlayerMoves()
        {
            // Panel (inactive while wiring so Awake sees the fields)
            var panelGo = Track(new GameObject("Panel", typeof(RectTransform)));
            panelGo.SetActive(false);
            var group = panelGo.AddComponent<CanvasGroup>();
            TextMeshProUGUI Txt(string n)
            {
                var g = new GameObject(n, typeof(RectTransform));
                g.transform.SetParent(panelGo.transform, false);
                return g.AddComponent<TextMeshProUGUI>();
            }
            var title = Txt("t");
            var panel = panelGo.AddComponent<ExhibitInfoPanel>();
            Set(panel, "group", group);
            Set(panel, "titleText", title);
            Set(panel, "metaText", Txt("m"));
            Set(panel, "bodyText", Txt("b"));
            Set(panel, "hintText", Txt("h"));
            panelGo.SetActive(true);

            // Anchor
            var info = Track(ScriptableObject.CreateInstance<ExhibitInfo>());
            info.title = "Tượng Phật";
            info.summary = "Tóm tắt";
            var anchorGo = Track(new GameObject("Anchor"));
            anchorGo.transform.position = Vector3.zero;
            var anchor = anchorGo.AddComponent<NarrativeAnchor>();
            Set(anchor, "info", info);
            Set(anchor, "radius", 2f);

            // Player far away
            var player = Track(new GameObject("Player")).transform;
            player.position = new Vector3(50, 0, 0);

            // Director (inactive while wiring)
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            Assert.IsNotNull(actions, "InputSystem_Actions.inputactions missing");
            var dirGo = Track(new GameObject("Director"));
            dirGo.SetActive(false);
            var dir = dirGo.AddComponent<NarrativeDirector>();
            Set(dir, "player", player);
            Set(dir, "panel", panel);
            Set(dir, "actions", actions);
            dirGo.SetActive(true);

            yield return null;
            Assert.AreEqual(0f, group.alpha, "far: hidden");

            player.position = new Vector3(1, 0, 0);
            yield return null;
            Assert.AreEqual(1f, group.alpha, "inside: shown");
            Assert.AreEqual("Tượng Phật", title.text);

            player.position = new Vector3(50, 0, 0);
            yield return null;
            Assert.AreEqual(0f, group.alpha, "outside: hidden");
        }

        [UnityTest]
        public IEnumerator Director_MissingWiring_LogsErrorAndDisables()
        {
            var dirGo = Track(new GameObject("Director"));
            dirGo.SetActive(false);
            var dir = dirGo.AddComponent<NarrativeDirector>();
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("NarrativeDirector: player, panel and actions"));
            dirGo.SetActive(true);
            yield return null;
            Assert.IsFalse(dir.enabled);
        }
    }
}
#endif
