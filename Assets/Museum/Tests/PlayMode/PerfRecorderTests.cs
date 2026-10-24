#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Museum.Tests
{
    // Fields are set by reflection (same as the other PlayMode tests).
    public class PerfRecorderTests
    {
        readonly List<Object> created = new List<Object>();
        string tempDir;

        static void Set(object target, string field, object value)
        {
            var f = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsNotNull(f, field);
            f.SetValue(target, value);
        }

        T Track<T>(T o) where T : Object { created.Add(o); return o; }

        Transform Point(string name, Vector3 pos)
        {
            var g = Track(new GameObject(name));
            g.transform.position = pos;
            return g.transform;
        }

        [SetUp]
        public void SetUp()
        {
            tempDir = Path.Combine(Path.GetTempPath(), "perf_test_" + System.Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var o in created) if (o != null) Object.Destroy(o);
            created.Clear();
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }

        [UnityTest]
        public IEnumerator Benchmark_TwoPoints_WritesCsvAndEndsAtLastPoint()
        {
            var player = Track(new GameObject("Player", typeof(CharacterController))).transform;
            var a = Point("A", new Vector3(1, 0, 2));
            var b = Point("B", new Vector3(5, 0, 6));

            var go = Track(new GameObject("Perf"));
            go.SetActive(false);
            var rec = go.AddComponent<PerfRecorder>();
            Set(rec, "player", player);
            Set(rec, "benchmarkPoints", new[] { a, b });
            Set(rec, "settleSeconds", 0.05f);
            Set(rec, "recordSeconds", 0.1f);
            rec.OutputDirectory = tempDir;
            go.SetActive(true);

            rec.StartBenchmark();
            Assert.IsTrue(rec.IsBenchmarking);
            float t0 = Time.realtimeSinceStartup;
            while (rec.IsBenchmarking && Time.realtimeSinceStartup - t0 < 10f) yield return null;

            Assert.IsFalse(rec.IsBenchmarking, "benchmark must finish");
            Assert.IsNotNull(rec.LastCsvPath);
            Assert.IsTrue(File.Exists(rec.LastCsvPath));
            Assert.AreEqual(tempDir, Path.GetDirectoryName(rec.LastCsvPath));
            var lines = File.ReadAllLines(rec.LastCsvPath);
            Assert.AreEqual(3, lines.Length);
            Assert.AreEqual("point,samples,avg_fps,avg_ms,p95_ms,p99_ms,low1_fps,draw_calls,triangles,sys_mem_mb,gfx_mem_mb", lines[0]);
            Assert.IsTrue(lines[1].StartsWith("A,"), lines[1]);
            Assert.IsTrue(lines[2].StartsWith("B,"), lines[2]);
            Assert.AreEqual(11, lines[1].Split(',').Length);
            Assert.Greater(int.Parse(lines[1].Split(',')[1]), 0, "at least one frame sampled");
            Assert.AreEqual(b.position, player.position);
        }

        [UnityTest]
        public IEnumerator Recorder_MissingPlayer_LogsErrorAndDisables()
        {
            var go = Track(new GameObject("Perf"));
            go.SetActive(false);
            var rec = go.AddComponent<PerfRecorder>();
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("PerfRecorder: player and benchmark points"));
            go.SetActive(true);
            yield return null;
            Assert.IsFalse(rec.enabled);
        }

        [UnityTest]
        public IEnumerator Benchmark_NoPoints_LogsErrorAndDisables()
        {
            var player = Track(new GameObject("Player")).transform;
            var go = Track(new GameObject("Perf"));
            go.SetActive(false);
            var rec = go.AddComponent<PerfRecorder>();
            Set(rec, "player", player);
            rec.OutputDirectory = tempDir;
            go.SetActive(true);

            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("PerfRecorder: player and benchmark points"));
            rec.StartBenchmark();
            yield return null;

            Assert.IsFalse(rec.enabled);
            Assert.IsFalse(rec.IsBenchmarking);
            Assert.IsNull(rec.LastCsvPath);
        }
    }
}
#endif
