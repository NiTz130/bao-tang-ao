#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Museum.Tests
{
    // The camera sits at the origin and looks along +Z. Each exhibit is an anchor with a cube model child.
    public class ExhibitInspectorTests : InputTestFixture
    {
        readonly List<Object> created = new List<Object>();
        Camera cam;
        FirstPersonController player;
        ExhibitTargeting targeting;
        Light light;
        Volume volume;
        ExhibitInspector inspector;
        Keyboard keyboard;
        Mouse mouse;
        CursorLockMode savedLock;

        static void Set(object target, string field, object value)
        {
            var f = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsNotNull(f, field);
            f.SetValue(target, value);
        }

        T Track<T>(T o) where T : Object { created.Add(o); return o; }

        public override void Setup()
        {
            base.Setup();
            savedLock = Cursor.lockState;
            keyboard = InputSystem.AddDevice<Keyboard>();
            mouse = InputSystem.AddDevice<Mouse>();
        }

        [TearDown]
        public override void TearDown()
        {
            foreach (var o in created) if (o != null) Object.DestroyImmediate(o);
            created.Clear();
            Cursor.lockState = savedLock;
            base.TearDown();
        }

        ExhibitInspector Rig(float moveSeconds = 0.2f, float bob = 0f, float spin = 0f, float spinDelay = 2f, bool wire = true)
        {
            cam = Track(new GameObject("Cam")).AddComponent<Camera>();

            var playerGo = Track(new GameObject("Player"));
            playerGo.SetActive(false); // only the Frozen flag is used
            player = playerGo.AddComponent<FirstPersonController>();

            var targetingGo = Track(new GameObject("Targeting"));
            targetingGo.SetActive(false);
            targeting = targetingGo.AddComponent<ExhibitTargeting>();
            Set(targeting, "cam", cam);
            Set(targeting, "crosshair", Track(new GameObject("Crosshair", typeof(RectTransform))).AddComponent<Image>());
            targetingGo.SetActive(true);

            light = Track(new GameObject("InspectLight")).AddComponent<Light>();
            light.type = LightType.Point;
            volume = Track(new GameObject("InspectVolume")).AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = Track(ScriptableObject.CreateInstance<VolumeProfile>());

            var actions = Track(Object.Instantiate(AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions")));

            var go = Track(new GameObject("Inspector"));
            go.SetActive(false);
            var i = go.AddComponent<ExhibitInspector>();
            if (wire)
            {
                Set(i, "cam", cam);
                Set(i, "player", player);
                Set(i, "targeting", targeting);
                Set(i, "inspectLight", light);
                Set(i, "inspectVolume", volume);
                Set(i, "actions", actions);
            }
            Set(i, "moveSeconds", moveSeconds);
            Set(i, "bobAmplitude", bob);
            Set(i, "bobPeriod", 1f);
            Set(i, "idleSpinDegreesPerSecond", spin);
            Set(i, "idleSpinDelay", spinDelay);
            go.SetActive(true);
            inspector = i;
            return i;
        }

        (NarrativeAnchor anchor, Transform model) Exhibit(float size = 0.3f, Vector3? position = null, bool withModel = true)
        {
            var go = Track(new GameObject("Exhibit"));
            go.transform.position = position ?? new Vector3(0f, 0f, 3f);
            var anchor = go.AddComponent<NarrativeAnchor>();
            if (!withModel) return (anchor, null);
            var model = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            model.name = "Model";
            model.SetParent(go.transform, false);
            model.localPosition = new Vector3(0.1f, 0.5f, -0.2f);
            model.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            model.localScale = Vector3.one * size;
            Physics.SyncTransforms();
            return (anchor, model);
        }

        Vector3 ViewPoint() => cam.ViewportToWorldPoint(new Vector3(0.66f, 0.5f, 0.85f));

        static float MaxExtent(Transform model)
        {
            var size = model.GetComponent<Renderer>().bounds.size;
            return Mathf.Max(size.x, size.y, size.z);
        }

        static IEnumerator Wait(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
        }

        static void AssertHome(Transform model, Transform parent)
        {
            Assert.AreSame(parent, model.parent, "parent");
            Assert.Less(Vector3.Distance(new Vector3(0.1f, 0.5f, -0.2f), model.localPosition), 1e-4f, "localPosition");
            Assert.Less(Quaternion.Angle(Quaternion.Euler(-90f, 0f, 0f), model.localRotation), 0.01f, "localRotation");
            Assert.Less(Vector3.Distance(Vector3.one * 0.3f, model.localScale), 1e-4f, "localScale");
        }

        [UnityTest]
        public IEnumerator Enter_AfterMoveSeconds_ModelAtViewPoint()
        {
            Rig();
            var (anchor, model) = Exhibit();
            Assert.IsTrue(inspector.Enter(anchor));
            yield return Wait(0.35f);

            Assert.AreEqual(InspectPhase.Viewing, inspector.Phase);
            Assert.Less(Vector3.Distance(ViewPoint(), model.GetComponent<Renderer>().bounds.center), 0.01f);
            Assert.AreEqual(0.45f, MaxExtent(model), 0.01f);
        }

        [UnityTest]
        public IEnumerator Enter_SetsSideEffects()
        {
            Rig();
            var (anchor, model) = Exhibit();
            inspector.Enter(anchor);
            yield return Wait(0.35f);

            Assert.IsFalse(model.GetComponent<Collider>().enabled, "collider off");
            Assert.IsTrue(light.enabled, "light on");
            Assert.AreEqual(4f, light.intensity, 1e-3f);
            Assert.AreEqual(1f, volume.weight, 1e-4f);
            Assert.IsTrue(player.Frozen);
            Assert.IsFalse(targeting.enabled);
            Assert.AreSame(anchor, inspector.Anchor);
        }

        [UnityTest]
        public IEnumerator Enter_Midway_IsBetweenHomeAndTarget()
        {
            Rig(moveSeconds: 0.6f);
            var (anchor, model) = Exhibit();
            var home = model.position;
            inspector.Enter(anchor);
            yield return Wait(0.25f);

            Assert.AreEqual(InspectPhase.Entering, inspector.Phase);
            var center = model.GetComponent<Renderer>().bounds.center;
            Assert.Greater(Vector3.Distance(home, model.position), 0.05f);
            Assert.Greater(Vector3.Distance(ViewPoint(), center), 0.05f);
        }

        [UnityTest]
        public IEnumerator Exit_RestoresExactLocalPose()
        {
            Rig(spin: 90f, spinDelay: 0f);
            var (anchor, model) = Exhibit();
            inspector.Enter(anchor);
            yield return Wait(0.3f);
            Press(mouse.leftButton);
            for (int f = 0; f < 5; f++) { Set(mouse.delta, new Vector2(30f, 15f)); Set(mouse.scroll, new Vector2(0f, 120f)); yield return null; }
            Release(mouse.leftButton);
            yield return Wait(0.2f);

            inspector.Exit();
            yield return Wait(0.35f);
            Assert.AreEqual(InspectPhase.Idle, inspector.Phase);
            AssertHome(model, anchor.transform);
        }

        [UnityTest]
        public IEnumerator Exit_RestoresSideEffects()
        {
            Rig();
            var (anchor, model) = Exhibit();
            inspector.Enter(anchor);
            yield return Wait(0.3f);
            inspector.Exit();
            yield return Wait(0.35f);

            Assert.IsTrue(model.GetComponent<Collider>().enabled, "collider on");
            Assert.IsFalse(light.enabled, "light off");
            Assert.AreEqual(0f, volume.weight);
            Assert.IsFalse(player.Frozen);
            Assert.IsTrue(targeting.enabled);
            Assert.IsNull(inspector.Anchor);
        }

        [UnityTest]
        public IEnumerator ExitDuringEnter_ReturnsHome()
        {
            Rig(moveSeconds: 0.6f);
            var (anchor, model) = Exhibit();
            inspector.Enter(anchor);
            yield return Wait(0.2f);
            inspector.Exit();
            Assert.AreEqual(InspectPhase.Leaving, inspector.Phase);
            yield return Wait(0.8f);

            Assert.AreEqual(InspectPhase.Idle, inspector.Phase);
            AssertHome(model, anchor.transform);
        }

        [UnityTest]
        public IEnumerator EnterWhileActiveOrLeaving_IsIgnored()
        {
            Rig();
            var (a, _) = Exhibit();
            var (b, _) = Exhibit(position: new Vector3(1f, 0f, 3f));
            Assert.IsTrue(inspector.Enter(a));
            Assert.IsFalse(inspector.Enter(b), "while entering");
            yield return Wait(0.3f);
            Assert.IsFalse(inspector.Enter(b), "while viewing");
            inspector.Exit();
            Assert.IsFalse(inspector.Enter(b), "while leaving");
            Assert.AreSame(a, inspector.Anchor);
        }

        [UnityTest]
        public IEnumerator Drag_WithAttack_Rotates()
        {
            Rig();
            var (anchor, model) = Exhibit();
            inspector.Enter(anchor);
            yield return Wait(0.3f);
            var before = model.rotation;
            Press(mouse.leftButton);
            for (int f = 0; f < 5; f++) { Set(mouse.delta, new Vector2(40f, 0f)); yield return null; }

            Assert.Greater(Quaternion.Angle(before, model.rotation), 5f);
        }

        [UnityTest]
        public IEnumerator Look_WithoutAttack_DoesNotRotateModel()
        {
            Rig();
            var (anchor, model) = Exhibit();
            inspector.Enter(anchor);
            yield return Wait(0.3f);
            var before = model.rotation;
            for (int f = 0; f < 5; f++) { Set(mouse.delta, new Vector2(40f, 20f)); yield return null; }

            Assert.Less(Quaternion.Angle(before, model.rotation), 0.01f);
        }

        [UnityTest]
        public IEnumerator Scroll_ZoomsWithinLimits()
        {
            Rig();
            var (anchor, model) = Exhibit();
            inspector.Enter(anchor);
            yield return Wait(0.3f);
            float viewScale = model.localScale.x;
            for (int f = 0; f < 20; f++) { Set(mouse.scroll, new Vector2(0f, 120f)); yield return null; }
            Assert.AreEqual(viewScale * 1.6f, model.localScale.x, 1e-4f, "max zoom");
            for (int f = 0; f < 40; f++) { Set(mouse.scroll, new Vector2(0f, -120f)); yield return null; }
            Assert.AreEqual(viewScale * 0.6f, model.localScale.x, 1e-4f, "min zoom");
        }

        [UnityTest]
        public IEnumerator Bob_MovesVertically()
        {
            Rig(bob: 0.012f);
            var (anchor, model) = Exhibit();
            inspector.Enter(anchor);
            yield return Wait(0.3f);
            float baseY = ViewPoint().y, min = float.MaxValue, max = float.MinValue;
            for (float t = 0f; t < 1.2f; t += Time.unscaledDeltaTime)
            {
                float y = model.GetComponent<Renderer>().bounds.center.y - baseY;
                min = Mathf.Min(min, y);
                max = Mathf.Max(max, y);
                yield return null;
            }
            Assert.LessOrEqual(max, 0.0125f);
            Assert.GreaterOrEqual(min, -0.0125f);
            Assert.Greater(max - min, 0.018f);
        }

        [UnityTest]
        public IEnumerator Bob_ZeroAmplitude_StaysStill()
        {
            Rig(bob: 0f);
            var (anchor, model) = Exhibit();
            inspector.Enter(anchor);
            yield return Wait(0.3f);
            var start = model.position;
            yield return Wait(0.5f);
            Assert.Less(Vector3.Distance(start, model.position), 1e-4f);
        }

        [UnityTest]
        public IEnumerator IdleSpin_StopsWhileDragging_ResumesAfterDelay()
        {
            Rig(spin: 90f, spinDelay: 0.2f);
            var (anchor, model) = Exhibit();
            inspector.Enter(anchor);
            yield return Wait(0.3f);
            Press(mouse.leftButton);
            yield return null;
            var held = model.rotation;
            yield return Wait(0.3f);
            Assert.Less(Quaternion.Angle(held, model.rotation), 0.01f, "no spin while dragging");

            Release(mouse.leftButton);
            yield return Wait(0.6f);
            Assert.Greater(Quaternion.Angle(held, model.rotation), 5f, "spins again after the delay");
        }

        [UnityTest]
        public IEnumerator DisableWhileActive_RestoresImmediately()
        {
            Rig();
            var (anchor, model) = Exhibit();
            inspector.Enter(anchor);
            yield return Wait(0.3f);
            inspector.enabled = false;

            AssertHome(model, anchor.transform);
            Assert.IsTrue(model.GetComponent<Collider>().enabled);
            Assert.IsFalse(player.Frozen);
            Assert.AreEqual(InspectPhase.Idle, inspector.Phase);
        }

        [UnityTest]
        public IEnumerator ModelDestroyedWhileActive_NoErrors()
        {
            Rig();
            var (anchor, model) = Exhibit();
            inspector.Enter(anchor);
            yield return Wait(0.3f);
            Object.Destroy(model.gameObject);
            yield return null;
            yield return null;

            Assert.AreEqual(InspectPhase.Idle, inspector.Phase);
            Assert.IsFalse(player.Frozen);
            Assert.IsTrue(targeting.enabled);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator AnchorDestroyedWhileActive_RemovesFloatingModel()
        {
            Rig();
            var (anchor, model) = Exhibit();
            inspector.Enter(anchor);
            yield return Wait(0.3f);
            Object.Destroy(anchor.gameObject);
            yield return null;
            yield return null;

            Assert.AreEqual(InspectPhase.Idle, inspector.Phase);
            Assert.IsTrue(model == null, "the detached model must not stay floating");
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator AnchorWithoutModel_EnterReturnsFalse()
        {
            Rig();
            var (anchor, _) = Exhibit(withModel: false);
            Assert.IsFalse(inspector.Enter(anchor));
            Assert.IsFalse(inspector.Enter(null));
            yield return null;

            Assert.AreEqual(InspectPhase.Idle, inspector.Phase);
            Assert.IsFalse(player.Frozen);
            Assert.IsTrue(targeting.enabled);
            Assert.IsFalse(light.enabled);
        }

        [UnityTest]
        public IEnumerator LargeModel_FitsViewSize()
        {
            Rig();
            var (anchor, model) = Exhibit(size: 3.4f);
            inspector.Enter(anchor);
            yield return Wait(0.3f);

            Assert.AreEqual(0.45f, MaxExtent(model), 0.01f);
        }

        [UnityTest]
        public IEnumerator Escape_Exits()
        {
            Rig();
            var (anchor, model) = Exhibit();
            inspector.Enter(anchor);
            yield return Wait(0.3f);
            Press(keyboard.escapeKey);
            yield return null;
            Release(keyboard.escapeKey);
            Assert.AreEqual(InspectPhase.Leaving, inspector.Phase);
            yield return Wait(0.35f);

            Assert.AreEqual(InspectPhase.Idle, inspector.Phase);
            AssertHome(model, anchor.transform);
        }

        [UnityTest]
        public IEnumerator MissingWiring_LogsErrorAndDisables()
        {
            LogAssert.Expect(LogType.Error, "ExhibitInspector: cam, player, targeting, inspectLight, inspectVolume and actions must be assigned.");
            var i = Rig(wire: false);
            yield return null;
            Assert.IsFalse(i.enabled);
        }

        [UnityTest]
        public IEnumerator ZeroMoveSeconds_IsInstant()
        {
            Rig(moveSeconds: 0f);
            var (anchor, model) = Exhibit();
            inspector.Enter(anchor);
            yield return null;
            Assert.AreEqual(InspectPhase.Viewing, inspector.Phase);

            inspector.Exit();
            Assert.AreEqual(InspectPhase.Idle, inspector.Phase);
            AssertHome(model, anchor.transform);
        }
    }
}
#endif
