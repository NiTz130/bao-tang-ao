using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Museum.Tests
{
    // Real physics: the camera at the origin aims at exhibits with box colliders. Base light intensity 30, boost x1.5.
    public class ExhibitTargetingTests
    {
        const float Base = 30f, Boosted = 45f;
        readonly List<Object> created = new List<Object>();
        CursorLockMode savedLock;

        static void Set(object target, string field, object value)
        {
            var f = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsNotNull(f, field);
            f.SetValue(target, value);
        }

        GameObject Track(GameObject go) { created.Add(go); return go; }

        [SetUp] public void SetUp() => savedLock = Cursor.lockState;

        [TearDown]
        public void TearDown()
        {
            Cursor.lockState = savedLock;
            foreach (var o in created) if (o != null) Object.Destroy(o);
            created.Clear();
        }

        (Camera cam, Image crosshair, ExhibitTargeting targeting) Rig(float transition = 0f)
        {
            var cam = Track(new GameObject("Cam")).AddComponent<Camera>();
            var crosshair = Track(new GameObject("Crosshair", typeof(RectTransform))).AddComponent<Image>();
            var go = Track(new GameObject("Targeting"));
            go.SetActive(false);
            var t = go.AddComponent<ExhibitTargeting>();
            Set(t, "cam", cam);
            Set(t, "crosshair", crosshair);
            Set(t, "transitionSeconds", transition);
            go.SetActive(true);
            return (cam, crosshair, t);
        }

        Light MakeLight()
        {
            var light = Track(new GameObject("Spot")).AddComponent<Light>();
            light.type = LightType.Spot;
            light.intensity = Base;
            return light;
        }

        NarrativeAnchor MakeExhibit(Vector3 position, Light[] lights, bool colliderOnChild = false)
        {
            var go = Track(new GameObject("Exhibit"));
            go.transform.position = position;
            var colliderHost = go;
            if (colliderOnChild)
            {
                colliderHost = new GameObject("Model");
                colliderHost.transform.SetParent(go.transform, false);
            }
            colliderHost.AddComponent<BoxCollider>().isTrigger = true;
            var anchor = go.AddComponent<NarrativeAnchor>();
            if (lights != null) Set(anchor, "highlightLights", lights);
            Physics.SyncTransforms();
            return anchor;
        }

        static void Aim(Camera cam, Vector3 at) => cam.transform.rotation = Quaternion.LookRotation(at - cam.transform.position);
        static void LookAway(Camera cam) => cam.transform.rotation = Quaternion.LookRotation(Vector3.back);

        [UnityTest]
        public IEnumerator Aim_SetsCurrent_BoostsLight_GrowsCrosshair()
        {
            var (cam, crosshair, t) = Rig();
            var light = MakeLight();
            var exhibit = MakeExhibit(new Vector3(0, 0, 2), new[] { light });
            Aim(cam, exhibit.transform.position);
            yield return null;

            Assert.AreSame(exhibit, t.Current);
            Assert.AreEqual(Boosted, light.intensity, 1e-3f);
            Assert.AreEqual(12f, crosshair.rectTransform.sizeDelta.x, 1e-3f);
            Assert.Less(Vector4.Distance((Color)new Color32(0xE8, 0xB0, 0x4A, 0xFF), crosshair.color), 1e-4f);
        }

        [UnityTest]
        public IEnumerator LookAway_RestoresExactBaseIntensity_AndIdleCrosshair()
        {
            var (cam, crosshair, t) = Rig();
            var light = MakeLight();
            var exhibit = MakeExhibit(new Vector3(0, 0, 2), new[] { light });
            Aim(cam, exhibit.transform.position);
            yield return null;
            LookAway(cam);
            yield return null;

            Assert.IsNull(t.Current);
            Assert.AreEqual(Base, light.intensity);
            Assert.AreEqual(6f, crosshair.rectTransform.sizeDelta.x, 1e-3f);
        }

        [UnityTest]
        public IEnumerator RepeatedHover_DoesNotDriftBaseIntensity()
        {
            var (cam, _, _) = Rig();
            var light = MakeLight();
            var exhibit = MakeExhibit(new Vector3(0, 0, 2), new[] { light });
            for (int i = 0; i < 5; i++)
            {
                Aim(cam, exhibit.transform.position);
                yield return null;
                Assert.AreEqual(Boosted, light.intensity, 1e-3f, "hover " + i);
                LookAway(cam);
                yield return null;
                Assert.AreEqual(Base, light.intensity, "after hover " + i);
            }
        }

        [UnityTest]
        public IEnumerator SwitchFromAToB_RestoresA_BoostsB()
        {
            var (cam, _, t) = Rig();
            var lightA = MakeLight();
            var lightB = MakeLight();
            var a = MakeExhibit(new Vector3(0, 0, 2), new[] { lightA });
            var b = MakeExhibit(new Vector3(2, 0, 2), new[] { lightB });
            Aim(cam, a.transform.position);
            yield return null;
            Aim(cam, b.transform.position);
            yield return null;

            Assert.AreSame(b, t.Current);
            Assert.AreEqual(Base, lightA.intensity);
            Assert.AreEqual(Boosted, lightB.intensity, 1e-3f);
        }

        [UnityTest]
        public IEnumerator Transition_RampsLightGradually()
        {
            var (cam, _, _) = Rig(transition: 1f);
            var light = MakeLight();
            var exhibit = MakeExhibit(new Vector3(0, 0, 2), new[] { light });
            LookAway(cam);
            yield return null;
            yield return null;
            Aim(cam, exhibit.transform.position);
            yield return null;

            Assert.Greater(light.intensity, Base);
            Assert.Less(light.intensity, Boosted);
        }

        [UnityTest]
        public IEnumerator BeyondMaxDistance_NoTarget()
        {
            var (cam, _, t) = Rig();
            var light = MakeLight();
            var exhibit = MakeExhibit(new Vector3(0, 0, 6), new[] { light });
            Aim(cam, exhibit.transform.position);
            yield return null;

            Assert.IsNull(t.Current);
            Assert.AreEqual(Base, light.intensity);
        }

        [UnityTest]
        public IEnumerator WallInBetween_NoTarget()
        {
            var (cam, _, t) = Rig();
            var light = MakeLight();
            var exhibit = MakeExhibit(new Vector3(0, 0, 3), new[] { light });
            var wall = Track(new GameObject("Wall"));
            wall.transform.position = new Vector3(0, 0, 1.5f);
            wall.AddComponent<BoxCollider>().size = new Vector3(2, 2, 0.2f);
            Physics.SyncTransforms();
            Aim(cam, exhibit.transform.position);
            yield return null;

            Assert.IsNull(t.Current);
            Assert.AreEqual(Base, light.intensity);
        }

        [UnityTest]
        public IEnumerator ColliderOnChild_ResolvesParentAnchor()
        {
            var (cam, _, t) = Rig();
            var exhibit = MakeExhibit(new Vector3(0, 0, 2), new[] { MakeLight() }, colliderOnChild: true);
            Aim(cam, exhibit.transform.position);
            yield return null;

            Assert.AreSame(exhibit, t.Current);
        }

        [UnityTest]
        public IEnumerator NoLightsOrNullEntries_TargetsWithoutErrors()
        {
            var (cam, _, t) = Rig();
            var noLights = MakeExhibit(new Vector3(0, 0, 2), null);
            var nullEntry = MakeExhibit(new Vector3(2, 0, 2), new Light[] { null });
            Aim(cam, noLights.transform.position);
            yield return null;
            Assert.AreSame(noLights, t.Current);
            Aim(cam, nullEntry.transform.position);
            yield return null;
            Assert.AreSame(nullEntry, t.Current);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator ExhibitDestroyedWhileAimed_ClearsTargetAndRestoresLight()
        {
            var (cam, _, t) = Rig();
            var light = MakeLight();
            var exhibit = MakeExhibit(new Vector3(0, 0, 2), new[] { light });
            Aim(cam, exhibit.transform.position);
            yield return null;
            Object.Destroy(exhibit.gameObject);
            yield return null;
            yield return null;

            Assert.IsTrue(t.Current == null);
            Assert.AreEqual(Base, light.intensity);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator LightDestroyedWhileBoosted_NoErrors()
        {
            var (cam, _, _) = Rig();
            var light = MakeLight();
            var exhibit = MakeExhibit(new Vector3(0, 0, 2), new[] { light });
            Aim(cam, exhibit.transform.position);
            yield return null;
            Object.Destroy(light.gameObject);
            yield return null;
            LookAway(cam);
            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Disable_RestoresLights()
        {
            var (cam, _, t) = Rig();
            var light = MakeLight();
            var exhibit = MakeExhibit(new Vector3(0, 0, 2), new[] { light });
            Aim(cam, exhibit.transform.position);
            yield return null;
            t.enabled = false;

            Assert.AreEqual(Base, light.intensity);
            Assert.IsNull(t.Current);
        }

        [UnityTest]
        public IEnumerator Disable_HidesCrosshair()
        {
            var (_, crosshair, targeting) = Rig();
            yield return null;
            crosshair.enabled = true;
            targeting.enabled = false;
            Assert.IsFalse(crosshair.enabled);
        }

        [UnityTest]
        public IEnumerator CursorUnlocked_HidesCrosshair()
        {
            var (_, crosshair, _) = Rig();
            Cursor.lockState = CursorLockMode.None;
            yield return null;
            Assert.IsFalse(crosshair.enabled);
        }

        // --- Laser: key 1 gives a 20 m reach, a red line from the "hand" and a dot at the end point. ---

        static readonly Vector3 Hand = new Vector3(0.15f, -0.2f, 0.1f);

        // Built like the scene: the line starts disabled, the dot is a sphere without collider and starts inactive.
        (LineRenderer line, Transform dot) AddLaser(ExhibitTargeting t, bool on = true)
        {
            var line = Track(new GameObject("Line")).AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.enabled = false;
            var dot = Track(GameObject.CreatePrimitive(PrimitiveType.Sphere));
            Object.DestroyImmediate(dot.GetComponent<SphereCollider>());
            dot.SetActive(false);
            Set(t, "laserLine", line);
            Set(t, "laserDot", dot.transform);
            if (on) Set(t, "laserOn", true);
            return (line, dot.transform);
        }

        [UnityTest]
        public IEnumerator Laser_ExtendsReach_OffKeepsShortReach()
        {
            var (_, _, t) = Rig();
            var exhibit = MakeExhibit(new Vector3(0, 0, 10), null);
            Set(t, "laserOn", true);
            yield return null;
            Assert.AreSame(exhibit, t.Current);

            Set(t, "laserOn", false);
            yield return null;
            Assert.IsNull(t.Current);
        }

        [UnityTest]
        public IEnumerator Laser_WallBlocks_DotAtWallHit_LineFromHand()
        {
            Cursor.lockState = CursorLockMode.Locked;
            var (cam, _, t) = Rig();
            var (line, dot) = AddLaser(t);
            MakeExhibit(new Vector3(0, 0, 10), null);
            var wall = Track(new GameObject("Wall"));
            wall.transform.position = new Vector3(0, 0, 5f);
            wall.AddComponent<BoxCollider>().size = new Vector3(4, 4, 0.2f);
            Physics.SyncTransforms();
            yield return null;

            Assert.IsNull(t.Current);
            Assert.AreEqual(4.9f, dot.position.z, 1e-3f);
            Assert.AreEqual(0f, dot.position.x, 1e-3f);
            Assert.Less(Vector3.Distance(line.GetPosition(0), cam.transform.TransformPoint(Hand)), 1e-4f);
            Assert.Less(Vector3.Distance(line.GetPosition(1), dot.position), 1e-4f);
        }

        [UnityTest]
        public IEnumerator Laser_NothingHit_DotAtLaserDistance()
        {
            Cursor.lockState = CursorLockMode.Locked;
            var (cam, _, t) = Rig();
            var (line, dot) = AddLaser(t);
            cam.transform.position = new Vector3(1, 2, 3);
            cam.transform.rotation = Quaternion.Euler(0, 90, 0);
            yield return null;

            var expected = cam.transform.position + cam.transform.forward * 20f;
            Assert.Less(Vector3.Distance(dot.position, expected), 1e-3f);
            Assert.Less(Vector3.Distance(line.GetPosition(1), expected), 1e-3f);
        }

        [UnityTest]
        public IEnumerator Laser_DotHasNoCollider_TargetDoesNotFlicker()
        {
            Cursor.lockState = CursorLockMode.Locked;
            var (_, _, t) = Rig();
            var (_, dot) = AddLaser(t);
            var exhibit = MakeExhibit(new Vector3(0, 0, 10), null);
            for (int i = 0; i < 3; i++)
            {
                yield return null;
                Assert.AreSame(exhibit, t.Current, "frame " + i);
            }
            Assert.IsNull(dot.GetComponent<Collider>());
            Assert.IsTrue(dot.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator Laser_Hidden_WhenOff_WhenCursorUnlocked_OnDisable_VisibleAgainAfterEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            var (_, _, t) = Rig();
            var (line, dot) = AddLaser(t, on: false);
            yield return null;
            Assert.IsFalse(line.enabled, "off");
            Assert.IsFalse(dot.gameObject.activeSelf, "off");

            Set(t, "laserOn", true);
            yield return null;
            Assert.IsTrue(line.enabled, "on");
            Assert.IsTrue(dot.gameObject.activeSelf, "on");

            Cursor.lockState = CursorLockMode.None;
            yield return null;
            Assert.IsFalse(line.enabled, "unlocked");
            Assert.IsFalse(dot.gameObject.activeSelf, "unlocked");

            Cursor.lockState = CursorLockMode.Locked;
            yield return null;
            t.enabled = false;
            Assert.IsFalse(line.enabled, "disabled");
            Assert.IsFalse(dot.gameObject.activeSelf, "disabled");
            Assert.IsTrue(t.LaserOn, "laserOn survives disable");

            t.enabled = true;
            yield return null;
            Assert.IsTrue(line.enabled, "re-enabled");
            Assert.IsTrue(dot.gameObject.activeSelf, "re-enabled");
        }

        [UnityTest]
        public IEnumerator Laser_WithoutLineAndDot_WorksWithoutErrors()
        {
            Cursor.lockState = CursorLockMode.Locked;
            var (_, _, t) = Rig();
            var exhibit = MakeExhibit(new Vector3(0, 0, 10), null);
            Set(t, "laserOn", true);
            yield return null;
            yield return null;
            Assert.AreSame(exhibit, t.Current);
            t.enabled = false;
            t.enabled = true;
            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator MissingWiring_LogsErrorAndDisables()
        {
            var go = Track(new GameObject("Targeting"));
            go.SetActive(false);
            var t = go.AddComponent<ExhibitTargeting>();
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("ExhibitTargeting: cam and crosshair"));
            go.SetActive(true);
            yield return null;
            Assert.IsFalse(t.enabled);
        }
    }
}
