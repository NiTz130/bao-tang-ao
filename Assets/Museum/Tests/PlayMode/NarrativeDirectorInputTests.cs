#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;

namespace Museum.Tests
{
    // Presses the real E key through the Input System (virtual keyboard).
    public class NarrativeDirectorInputTests : InputTestFixture
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
        public override void TearDown()
        {
            foreach (var o in created) if (o != null) Object.Destroy(o);
            created.Clear();
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator Director_PressE_TogglesStory()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();

            var panelGo = Track(new GameObject("Panel", typeof(RectTransform)));
            panelGo.SetActive(false);
            var group = panelGo.AddComponent<CanvasGroup>();
            TextMeshProUGUI Txt(string n)
            {
                var g = new GameObject(n, typeof(RectTransform));
                g.transform.SetParent(panelGo.transform, false);
                return g.AddComponent<TextMeshProUGUI>();
            }
            var body = Txt("b");
            var hint = Txt("h");
            var panel = panelGo.AddComponent<ExhibitInfoPanel>();
            Set(panel, "group", group);
            Set(panel, "titleText", Txt("t"));
            Set(panel, "metaText", Txt("m"));
            Set(panel, "bodyText", body);
            Set(panel, "hintText", hint);
            panelGo.SetActive(true);

            var info = Track(ScriptableObject.CreateInstance<ExhibitInfo>());
            info.title = "Tượng Phật";
            info.summary = "Tóm tắt";
            info.story = "Câu chuyện dài";
            info.source = "Bảo tàng";
            var anchorGo = Track(new GameObject("Anchor"));
            var anchor = anchorGo.AddComponent<NarrativeAnchor>();
            Set(anchor, "info", info);
            Set(anchor, "radius", 2f);

            var player = Track(new GameObject("Player")).transform;
            player.position = new Vector3(1, 0, 0);

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
            Assert.AreEqual(1f, group.alpha, "shown");
            Assert.AreEqual("Tóm tắt", body.text, "collapsed body");
            Assert.AreEqual("Nhấn E để đọc thêm", hint.text, "collapsed hint");

            Press(keyboard.eKey);
            yield return null;
            Release(keyboard.eKey);
            yield return null;
            Assert.AreEqual("Câu chuyện dài\n\nNguồn: Bảo tàng", body.text, "expanded body");
            Assert.AreEqual("Nhấn E để thu gọn", hint.text, "expanded hint");

            Press(keyboard.eKey);
            yield return null;
            Release(keyboard.eKey);
            yield return null;
            Assert.AreEqual("Tóm tắt", body.text, "collapsed again");
        }

        // --- Inspect mode. The camera sits at the origin and looks along +Z at exhibit A (0, 0, 3). B is at (2, 0, 3). ---

        Keyboard kb;
        CanvasGroup group;
        TextMeshProUGUI body, hint;
        Camera cam;
        Transform playerPos;
        ExhibitTargeting targeting;
        ExhibitInspector inspector;
        NarrativeAnchor anchorA, anchorB;

        NarrativeAnchor Anchor(string name, Vector3 position, string story)
        {
            var info = Track(ScriptableObject.CreateInstance<ExhibitInfo>());
            info.title = name;
            info.summary = "Tóm tắt " + name;
            info.story = story;
            info.source = "Nguồn " + name;
            var go = Track(new GameObject(name));
            go.transform.position = position;
            var anchor = go.AddComponent<NarrativeAnchor>();
            Set(anchor, "info", info);
            Set(anchor, "radius", 3f);
            var model = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            model.SetParent(go.transform, false);
            model.localScale = Vector3.one * 0.3f;
            return anchor;
        }

        IEnumerator RigInspect(Vector3 playerAt, bool withInspector = true)
        {
            kb = InputSystem.AddDevice<Keyboard>();
            InputSystem.AddDevice<Mouse>();

            var panelGo = Track(new GameObject("Panel", typeof(RectTransform)));
            panelGo.SetActive(false);
            group = panelGo.AddComponent<CanvasGroup>();
            TextMeshProUGUI Txt(string n)
            {
                var g = new GameObject(n, typeof(RectTransform));
                g.transform.SetParent(panelGo.transform, false);
                return g.AddComponent<TextMeshProUGUI>();
            }
            body = Txt("b");
            hint = Txt("h");
            var panel = panelGo.AddComponent<ExhibitInfoPanel>();
            Set(panel, "group", group);
            Set(panel, "titleText", Txt("t"));
            Set(panel, "metaText", Txt("m"));
            Set(panel, "bodyText", body);
            Set(panel, "hintText", hint);
            panelGo.SetActive(true);

            anchorA = Anchor("A", new Vector3(0f, 0f, 3f), "Chuyện A");
            anchorB = Anchor("B", new Vector3(2f, 0f, 3f), "Chuyện B");
            Physics.SyncTransforms();

            cam = Track(new GameObject("Cam")).AddComponent<Camera>();
            var targetingGo = Track(new GameObject("Targeting"));
            targetingGo.SetActive(false);
            targeting = targetingGo.AddComponent<ExhibitTargeting>();
            Set(targeting, "cam", cam);
            Set(targeting, "crosshair", Track(new GameObject("Crosshair", typeof(RectTransform))).AddComponent<UnityEngine.UI.Image>());
            targetingGo.SetActive(true);

            var actions = Track(Object.Instantiate(AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions")));

            if (withInspector)
            {
                var fpsGo = Track(new GameObject("Fps"));
                fpsGo.SetActive(false);
                var fps = fpsGo.AddComponent<FirstPersonController>();
                var volume = Track(new GameObject("Volume")).AddComponent<UnityEngine.Rendering.Volume>();
                volume.sharedProfile = Track(ScriptableObject.CreateInstance<UnityEngine.Rendering.VolumeProfile>());
                var inspectGo = Track(new GameObject("Inspector"));
                inspectGo.SetActive(false);
                inspector = inspectGo.AddComponent<ExhibitInspector>();
                Set(inspector, "cam", cam);
                Set(inspector, "player", fps);
                Set(inspector, "targeting", targeting);
                Set(inspector, "inspectLight", Track(new GameObject("Light")).AddComponent<Light>());
                Set(inspector, "inspectVolume", volume);
                Set(inspector, "actions", actions);
                Set(inspector, "moveSeconds", 0.1f);
                inspectGo.SetActive(true);
            }

            playerPos = Track(new GameObject("Player")).transform;
            playerPos.position = playerAt;
            var dirGo = Track(new GameObject("Director"));
            dirGo.SetActive(false);
            var dir = dirGo.AddComponent<NarrativeDirector>();
            Set(dir, "player", playerPos);
            Set(dir, "panel", panel);
            Set(dir, "actions", actions);
            Set(dir, "targeting", targeting);
            if (withInspector) Set(dir, "inspector", inspector);
            dirGo.SetActive(true);
            yield return null;
            yield return null;
        }

        IEnumerator Tap(KeyControl key)
        {
            Press(key);
            yield return null;
            Release(key);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PressE_WhileAiming_EntersInspectWithExpandedStory()
        {
            yield return RigInspect(new Vector3(0f, 0f, 1f));
            Assert.AreSame(anchorA, targeting.Current, "aiming at A");
            yield return Tap(kb.eKey);

            Assert.IsTrue(inspector.IsActive);
            Assert.AreSame(anchorA, inspector.Anchor);
            Assert.AreEqual("Chuyện A\n\nNguồn: Nguồn A", body.text);
            Assert.AreEqual(NarrativeDirector.InspectHint, hint.text);
        }

        [UnityTest]
        public IEnumerator PressE_WhileInspecting_ExitsAndRestoresPanel()
        {
            yield return RigInspect(new Vector3(0f, 0f, 1f));
            yield return Tap(kb.eKey);
            yield return new WaitForSecondsRealtime(0.2f);
            yield return Tap(kb.eKey);

            Assert.AreEqual(InspectPhase.Leaving, inspector.Phase);
            Assert.AreEqual("Tóm tắt A", body.text, "panel back to the anchor state");
            Assert.AreEqual("Nhấn E để đọc thêm", hint.text);
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.IsFalse(inspector.IsActive);
            Assert.AreEqual("Tóm tắt A", body.text);
        }

        [UnityTest]
        public IEnumerator Escape_WhileInspecting_RestoresPanel()
        {
            yield return RigInspect(new Vector3(0f, 0f, 1f));
            yield return Tap(kb.eKey);
            yield return new WaitForSecondsRealtime(0.2f);
            yield return Tap(kb.escapeKey);
            yield return new WaitForSecondsRealtime(0.2f);

            Assert.IsFalse(inspector.IsActive);
            Assert.AreEqual("Tóm tắt A", body.text);
            Assert.AreEqual("Nhấn E để đọc thêm", hint.text);
        }

        [UnityTest]
        public IEnumerator PressE_AimingAtOtherExhibitInsideOverlap_InspectsAimedOne()
        {
            // Nearer to B, so the anchor logic picks B; the camera still aims at A.
            yield return RigInspect(new Vector3(1.6f, 0f, 3f));
            Assert.AreEqual("Tóm tắt B", body.text, "zone is B");
            yield return Tap(kb.eKey);

            Assert.AreSame(anchorA, inspector.Anchor);
            Assert.AreEqual("Chuyện A\n\nNguồn: Nguồn A", body.text);
        }

        [UnityTest]
        public IEnumerator PressE_NotAiming_TogglesAsBefore()
        {
            yield return RigInspect(new Vector3(0f, 0f, 1f));
            cam.transform.rotation = Quaternion.LookRotation(Vector3.back);
            yield return null;
            Assert.IsNull(targeting.Current);
            yield return Tap(kb.eKey);

            Assert.IsFalse(inspector.IsActive);
            Assert.AreEqual("Chuyện A\n\nNguồn: Nguồn A", body.text);
            Assert.AreEqual("Nhấn E để thu gọn", hint.text);
        }

        [UnityTest]
        public IEnumerator NoInspectorAssigned_BehavesAsBefore()
        {
            yield return RigInspect(new Vector3(0f, 0f, 1f), withInspector: false);
            Assert.AreSame(anchorA, targeting.Current);
            yield return Tap(kb.eKey);

            Assert.AreEqual("Chuyện A\n\nNguồn: Nguồn A", body.text);
            Assert.AreEqual("Nhấn E để thu gọn", hint.text);
        }

        [UnityTest]
        public IEnumerator Digit1_TogglesLaser()
        {
            yield return RigInspect(new Vector3(0f, 0f, 1f));
            Assert.IsFalse(targeting.LaserOn);
            yield return Tap(kb.digit1Key);
            Assert.IsTrue(targeting.LaserOn);
            yield return Tap(kb.digit1Key);
            Assert.IsFalse(targeting.LaserOn);
        }

        [UnityTest]
        public IEnumerator Laser_InspectFarExhibit_KeepsLaserOnAfterExit()
        {
            // Camera and player 10 m from A (outside every anchor zone, radius 3): only the laser reaches it.
            yield return RigInspect(new Vector3(0f, 0f, -7f));
            cam.transform.position = new Vector3(0f, 0f, -7f);
            Physics.SyncTransforms();
            yield return null;
            Assert.IsNull(targeting.Current, "beyond 4 m without the laser");

            yield return Tap(kb.digit1Key);
            Assert.AreSame(anchorA, targeting.Current, "laser reaches A");
            yield return Tap(kb.eKey);
            Assert.IsTrue(inspector.IsActive);
            Assert.AreSame(anchorA, inspector.Anchor);

            yield return new WaitForSecondsRealtime(0.2f);
            yield return Tap(kb.eKey);
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.IsFalse(inspector.IsActive);
            Assert.IsTrue(targeting.LaserOn, "laser survives inspect");
        }
    }
}
#endif
