using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Museum.Tests
{
    // Checks the rotunda layout in the real museum scene. Read-only: the scene is never saved.
    // Numbers come from Docs/plans/2026-10-16-rotunda-rebuild.md.
    public class MuseumLayoutTests
    {
        const string ScenePath = "Assets/Museum/Scenes/Museum.unity";
        const float RotundaRadius = 8f;
        const float CapsuleRadius = 0.3f, CapsuleBottom = 0.35f, CapsuleTop = 1.35f, EyeHeight = 1.6f, AimRange = 4f;

        struct Room { public float XMin, XMax, ZMin, ZMax; }
        static readonly Room West = new Room { XMin = -20.2f, XMax = -8f, ZMin = -6f, ZMax = 6f };
        static readonly Room North = new Room { XMin = -6f, XMax = 6f, ZMin = 8f, ZMax = 20.2f };
        static readonly Room East = new Room { XMin = 8f, XMax = 20.2f, ZMin = -6f, ZMax = 6f };

        // Anchor name -> room box. null = rotunda (distance to (0,0) < 8).
        static readonly Dictionary<string, Room?> RoomOf = new Dictionary<string, Room?>
        {
            { "Exhibit_01", West }, { "Exhibit_02", West },
            { "Exhibit_03", North }, { "Exhibit_04", North },
            { "Exhibit_05", East }, { "Exhibit_Centerpiece", null },
        };

        // Standing spots (x, z).
        static readonly Dictionary<string, Vector2> Stand = new Dictionary<string, Vector2>
        {
            { "Exhibit_01", new Vector2(-10f, -2.5f) }, { "Exhibit_02", new Vector2(-15.5f, 2.5f) },
            { "Exhibit_03", new Vector2(-2.5f, 10.5f) }, { "Exhibit_04", new Vector2(2.5f, 15.5f) },
            { "Exhibit_05", new Vector2(11.5f, 0f) }, { "Exhibit_Centerpiece", new Vector2(0f, -3.2f) },
        };

        static readonly Vector2 Start = new Vector2(0, -14), Vestibule = new Vector2(0, -9), S = new Vector2(0, -6);
        const float D = 4.24f;
        static readonly Vector2 SW = new Vector2(-D, -D), W = new Vector2(-6, 0), NW = new Vector2(-D, D),
            N = new Vector2(0, 6), SE = new Vector2(D, -D), E = new Vector2(6, 0);

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

        static Vector3 V(Vector2 p, float y = 0f) => new Vector3(p.x, y, p.y);

        // Waypoints (x, z) from the start to the standing spot of an exhibit.
        static List<Vector2> Route(string name)
        {
            var r = new List<Vector2> { Start, new Vector2(-1.5f, -11f), Vestibule, S }; // detour left of the drum pedestal at (0, -11)
            switch (name)
            {
                case "Exhibit_01": case "Exhibit_02": r.AddRange(new[] { SW, W, new Vector2(-9, 0), new Vector2(-10.5f, 0) }); break;
                case "Exhibit_03": case "Exhibit_04": r.AddRange(new[] { SW, W, NW, N, new Vector2(0, 9), new Vector2(0, 10.5f) }); break;
                case "Exhibit_05": r.AddRange(new[] { SE, E, new Vector2(9, 0), new Vector2(10.5f, 0) }); break;
            }
            r.Add(Stand[name]);
            return r;
        }

        // Player root or any parent named "Player" belongs to the walker, not to the world.
        static bool IsPlayer(Collider c)
        {
            if (c.GetComponentInParent<CharacterController>() != null) return true;
            for (var t = c.transform; t != null; t = t.parent) if (t.name == "Player") return true;
            return false;
        }

        [Test]
        public void Anchors_SitInTheirRooms()
        {
            var anchors = All<NarrativeAnchor>();
            Assert.AreEqual(RoomOf.Count, anchors.Length);
            foreach (var a in anchors)
            {
                Assert.IsTrue(RoomOf.ContainsKey(a.name), "unexpected anchor " + a.name);
                var p = a.transform.position;
                var room = RoomOf[a.name];
                if (room == null)
                    Assert.Less(new Vector2(p.x, p.z).magnitude, RotundaRadius, a.name + " must be in the rotunda, at " + p);
                else
                {
                    var r = room.Value;
                    Assert.IsTrue(p.x >= r.XMin && p.x <= r.XMax && p.z >= r.ZMin && p.z <= r.ZMax,
                        a.name + " is outside its room, at " + p);
                }
            }
        }

        [Test]
        public void AnchorRadii_DoNotOverlap()
        {
            var anchors = All<NarrativeAnchor>();
            for (int i = 0; i < anchors.Length; i++)
                for (int j = i + 1; j < anchors.Length; j++)
                {
                    var a = anchors[i]; var b = anchors[j];
                    var d = new Vector2(a.transform.position.x - b.transform.position.x, a.transform.position.z - b.transform.position.z).magnitude;
                    Assert.GreaterOrEqual(d, a.Radius + b.Radius, a.name + " and " + b.name + " radii overlap");
                }
        }

        [Test]
        public void EveryExhibit_IsReachableOnFoot()
        {
            Physics.SyncTransforms();
            foreach (var name in Stand.Keys)
            {
                var route = Route(name);
                for (int i = 0; i + 1 < route.Count; i++)
                {
                    var a = V(route[i]); var b = V(route[i + 1]);
                    var delta = b - a;
                    var dist = delta.magnitude;
                    var hits = Physics.CapsuleCastAll(a + Vector3.up * CapsuleBottom, a + Vector3.up * CapsuleTop,
                        CapsuleRadius, delta / dist, dist, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                    var blocker = hits.Where(h => !IsPlayer(h.collider)).OrderBy(h => h.distance).FirstOrDefault();
                    Assert.IsNull(blocker.collider, name + ": path blocked on segment " + route[i] + " -> " + route[i + 1] +
                        (blocker.collider != null ? " by " + blocker.collider.name : ""));
                }
            }
        }

        [Test]
        public void EveryExhibit_IsTargetableFromItsStandingSpot()
        {
            Physics.SyncTransforms();
            foreach (var a in All<NarrativeAnchor>())
            {
                var rends = a.GetComponentsInChildren<Renderer>(true);
                Assert.IsNotEmpty(rends, a.name + ": no renderer");
                var bounds = rends[0].bounds;
                foreach (var r in rends.Skip(1)) bounds.Encapsulate(r.bounds);

                var eye = V(Stand[a.name], EyeHeight);
                var toCenter = bounds.center - eye;
                var dist = toCenter.magnitude;
                var isCenterpiece = a.name == "Exhibit_Centerpiece";
                if (!isCenterpiece) Assert.LessOrEqual(dist, AimRange, a.name + ": centre is out of aim range, " + dist);

                Assert.IsTrue(Physics.Raycast(eye, toCenter / dist, out var hit, dist + 0.5f,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide), a.name + ": ray hits nothing");
                Assert.AreEqual(a, hit.collider.GetComponentInParent<NarrativeAnchor>(),
                    a.name + ": ray hits " + hit.collider.name + " first");
                if (isCenterpiece) Assert.LessOrEqual(hit.distance, AimRange, a.name + ": hit too far, " + hit.distance);
            }
        }

        [Test]
        public void SceneTexts_UseBeVietnamPro_WithAllGlyphs()
        {
            foreach (var t in All<TMP_Text>())
            {
                var text = t.text;
                if (string.IsNullOrEmpty(text)) continue;
                Assert.IsNotNull(t.font, t.name + ": no font");
                Assert.IsTrue(t.font.name.StartsWith("BeVietnamPro"), t.name + ": font is " + t.font.name);
                text = text.Replace("\n", "").Replace("\r", "").Replace("\t", "");
                Assert.IsTrue(t.font.HasCharacters(text, out uint[] missing, true, true),
                    t.name + ": missing glyphs " + string.Join(",", missing ?? new uint[0]));
            }
        }

        [TestCase("ĐIÊU KHẮC CHĂM")]
        [TestCase("GỐM SỨ VIỆT NAM")]
        [TestCase("LĂNG TẨM NHÀ NGUYỄN")]
        public void RoomTitles_Exist(string title)
        {
            Assert.IsTrue(All<TMP_Text>().Any(t => t.text != null && t.text.ToUpperInvariant().Contains(title)),
                "no title text: " + title);
        }

        [Test]
        public void Anchors_AreInsideLightProbeBounds()
        {
            var points = All<LightProbeGroup>().SelectMany(g => g.probePositions.Select(g.transform.TransformPoint)).ToArray();
            Assert.IsNotEmpty(points, "no light probes");
            var bounds = new Bounds(points[0], Vector3.zero);
            foreach (var p in points) bounds.Encapsulate(p);
            foreach (var a in All<NarrativeAnchor>())
                Assert.IsTrue(bounds.Contains(a.transform.position + Vector3.up), a.name + " is outside the light probe bounds");
        }

        [Test]
        public void Player_StartsInVestibule()
        {
            var player = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).First(t => t.name == "Player").position;
            Assert.IsTrue(player.x >= -3f && player.x <= 3f && player.z >= -16f && player.z <= -8f, "Player at " + player);
        }
    }
}
