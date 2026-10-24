using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Museum.Tests
{
    // Reads the real museum scene and checks the effect wiring. Read-only: the scene is never saved.
    public class MuseumSceneWiringTests
    {
        const string ScenePath = "Assets/Museum/Scenes/Museum.unity";
        Scene scene;
        bool openedHere;

        [OneTimeSetUp]
        public void Open()
        {
            scene = SceneManager.GetSceneByPath(ScenePath);
            if (scene.isLoaded) return;
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            openedHere = true;
        }

        [OneTimeTearDown]
        public void Close()
        {
            if (openedHere) EditorSceneManager.CloseScene(scene, true);
        }

        T[] All<T>() where T : Component =>
            scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();

        static Object Ref(Object component, string field) =>
            new SerializedObject(component).FindProperty(field).objectReferenceValue;

        [Test]
        public void EveryAnchor_HasHighlightLightsAndCollider()
        {
            var anchors = All<NarrativeAnchor>();
            Assert.AreEqual(6, anchors.Length);
            foreach (var a in anchors)
            {
                Assert.GreaterOrEqual(a.HighlightLights.Count, 1, a.name + ": no highlight light");
                Assert.IsTrue(a.HighlightLights.All(l => l != null), a.name + ": null highlight light");
                Assert.IsNotEmpty(a.GetComponentsInChildren<Collider>(true), a.name + ": no collider");
            }
        }

        [Test]
        public void PlinthExhibitColliders_AreTriggers()
        {
            foreach (var a in All<NarrativeAnchor>().Where(a => a.name != "Exhibit_Centerpiece"))
                foreach (var c in a.GetComponentsInChildren<Collider>(true))
                    Assert.IsTrue(c.isTrigger, a.name + ": collider must not block walking");
        }

        [Test]
        public void ScreenFade_IsLastChildOfUi()
        {
            var ui = scene.GetRootGameObjects().Single(g => g.name == "UI").transform;
            var last = ui.GetChild(ui.childCount - 1);
            Assert.AreEqual("ScreenFade", last.name);
            Assert.IsNotNull(last.GetComponent<ScreenFader>());
            Assert.IsFalse(last.GetComponent<Image>().raycastTarget);
        }

        [Test]
        public void Targeting_IsWired()
        {
            var targeting = All<ExhibitTargeting>();
            Assert.AreEqual(1, targeting.Length);
            Assert.IsNotNull(Ref(targeting[0], "cam"));
            var crosshair = Ref(targeting[0], "crosshair") as Image;
            Assert.IsNotNull(crosshair);
            Assert.IsFalse(crosshair.raycastTarget);
        }

        [Test]
        public void Targeting_LaserIsWired()
        {
            var t = All<ExhibitTargeting>().Single();
            var line = Ref(t, "laserLine") as LineRenderer;
            var dot = Ref(t, "laserDot") as Transform;
            Assert.IsNotNull(line, "laserLine");
            Assert.IsNotNull(dot, "laserDot");
            Assert.IsNull(dot.GetComponent<Collider>(), "the dot must not be hit by the targeting ray");
            Assert.AreEqual("M_Laser", line.sharedMaterial.name);
            Assert.AreEqual("Universal Render Pipeline/Unlit", line.sharedMaterial.shader.name);
        }

        [Test]
        public void Panel_FadesIn()
        {
            var panel = All<ExhibitInfoPanel>().Single();
            Assert.Greater(new SerializedObject(panel).FindProperty("fadeSeconds").floatValue, 0f);
        }

        [Test]
        public void Inspector_IsWired()
        {
            var inspectors = All<ExhibitInspector>();
            Assert.AreEqual(1, inspectors.Length);
            foreach (var field in new[] { "cam", "player", "targeting", "inspectLight", "inspectVolume", "actions" })
                Assert.IsNotNull(Ref(inspectors[0], field), field);
        }

        [Test]
        public void Director_HasTargetingAndInspector()
        {
            var director = All<NarrativeDirector>().Single();
            Assert.IsNotNull(Ref(director, "targeting"));
            Assert.IsNotNull(Ref(director, "inspector"));
        }

        [Test]
        public void InspectVolume_HasDepthOfField_AndStartsAtZeroWeight()
        {
            var volume = Ref(All<ExhibitInspector>().Single(), "inspectVolume") as UnityEngine.Rendering.Volume;
            Assert.IsNotNull(volume);
            Assert.AreEqual(0f, volume.weight);
            Assert.IsTrue(volume.isGlobal);
            Assert.IsNotNull(volume.sharedProfile);
            Assert.IsTrue(volume.sharedProfile.Has<UnityEngine.Rendering.Universal.DepthOfField>());
            Assert.IsFalse(volume.sharedProfile.Has<UnityEngine.Rendering.Universal.ColorAdjustments>(), "D3: blur only, no darkening");
        }

        [Test]
        public void InspectLight_IsRealtimeShadowlessAndOff()
        {
            var light = Ref(All<ExhibitInspector>().Single(), "inspectLight") as Light;
            Assert.IsNotNull(light);
            Assert.IsFalse(light.enabled);
            Assert.AreEqual(LightShadows.None, light.shadows);
            Assert.AreEqual(LightmapBakeType.Realtime, light.lightmapBakeType);
        }

        [Test]
        public void EveryAnchor_HasModelChildWithRenderer()
        {
            foreach (var a in All<NarrativeAnchor>())
                Assert.IsTrue(a.transform.Cast<Transform>().Any(c => c.GetComponentInChildren<Renderer>(true) != null), a.name + ": no model to inspect");
        }

        [Test]
        public void PerfRecorder_OnPlayer_HasFiveNamedBenchmarkPoints()
        {
            var rec = All<PerfRecorder>().Single();
            Assert.IsNotNull(rec.GetComponent<CharacterController>(), "PerfRecorder must be on the Player");
            Assert.AreSame(rec.transform, Ref(rec, "player"));
            var pts = new SerializedObject(rec).FindProperty("benchmarkPoints");
            var expected = new[] { "Bench_Vestibule", "Bench_Rotunda", "Bench_West", "Bench_North", "Bench_East" };
            Assert.AreEqual(expected.Length, pts.arraySize);
            for (int i = 0; i < expected.Length; i++)
            {
                var t = pts.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
                Assert.IsNotNull(t, "benchmark point " + i + " is null");
                Assert.AreEqual(expected[i], t.name);
            }
            var root = scene.GetRootGameObjects().Single(g => g.name == "Benchmark");
            Assert.IsFalse(root.isStatic, "Benchmark root must not be static");
            Assert.IsEmpty(root.GetComponentsInChildren<NarrativeAnchor>(true), "Benchmark must not hold an anchor");
        }
    }
}
