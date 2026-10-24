using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Museum
{
    // Put on the Player. Benchmark: "-benchmark" on the command line (writes CSV, then quits) or StartBenchmark().
    // Free mode: F9 starts/stops a recording. Draw-call and triangle counters may read -1 outside the editor and development builds.
    public class PerfRecorder : MonoBehaviour
    {
        [SerializeField] Transform player;
        [SerializeField] Transform[] benchmarkPoints;
        [SerializeField] float settleSeconds = 2f;
        [SerializeField] float recordSeconds = 10f;

        public const string Header = "point,samples,avg_fps,avg_ms,p95_ms,p99_ms,low1_fps,draw_calls,triangles,sys_mem_mb,gfx_mem_mb";
        const string WiringError = "PerfRecorder: player and benchmark points must be assigned.";
        const double Mb = 1024.0 * 1024.0;
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        // Empty means Application.persistentDataPath. Tests set it to a temp folder.
        public string OutputDirectory { get; set; }
        public string LastCsvPath { get; private set; }
        public bool IsBenchmarking { get; private set; }

        // Counters: draw calls, triangles, system memory, gfx memory.
        readonly ProfilerRecorder[] recorders = new ProfilerRecorder[4];
        readonly double[] sums = new double[4];
        readonly int[] counts = new int[4];
        readonly List<float> frames = new List<float>();
        bool recordersStarted, recording, fromCommandLine;

        void OnEnable()
        {
            fromCommandLine = Array.IndexOf(Environment.GetCommandLineArgs(), "-benchmark") >= 0;
            if (player == null || (fromCommandLine && (benchmarkPoints == null || benchmarkPoints.Length == 0)))
            {
                Debug.LogError(WiringError, this);
                enabled = false;
                return;
            }

            recorders[0] = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            recorders[1] = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            recorders[2] = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
            recorders[3] = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx Used Memory");
            recordersStarted = true;
        }

        void OnDisable()
        {
            if (!recordersStarted) return;
            for (int i = 0; i < recorders.Length; i++) recorders[i].Dispose();
            recordersStarted = false;
        }

        void Start()
        {
            if (fromCommandLine) Begin(true);
        }

        public void StartBenchmark() => Begin(false);

        void Begin(bool quit)
        {
            if (IsBenchmarking || !isActiveAndEnabled) return;
            if (benchmarkPoints == null || benchmarkPoints.Length == 0)
            {
                Debug.LogError(WiringError, this);
                enabled = false;
                return;
            }
            IsBenchmarking = true;
            StartCoroutine(Benchmark(quit));
        }

        IEnumerator Benchmark(bool quit)
        {
            var rows = new List<string>();
            foreach (var point in benchmarkPoints)
            {
                if (point == null) continue;
                Teleport(point);
                yield return new WaitForSecondsRealtime(settleSeconds);
                BeginSegment();
                yield return new WaitForSecondsRealtime(recordSeconds);
                rows.Add(EndSegment(point.name));
            }
            WriteCsv(rows);
            IsBenchmarking = false;
            if (quit) Application.Quit();
        }

        void Teleport(Transform point)
        {
            // CharacterController overrides transform changes while enabled.
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.SetPositionAndRotation(point.position, point.rotation);
            if (cc != null) cc.enabled = true;
        }

        void Update()
        {
            if (!IsBenchmarking && Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame)
            {
                if (recording) WriteCsv(new List<string> { EndSegment("free") });
                else BeginSegment();
            }
            if (!recording) return;

            frames.Add(Time.unscaledDeltaTime * 1000f);
            for (int i = 0; i < recorders.Length; i++)
            {
                if (!recorders[i].Valid) continue;
                sums[i] += recorders[i].LastValue;
                counts[i]++;
            }
        }

        void BeginSegment()
        {
            frames.Clear();
            Array.Clear(sums, 0, sums.Length);
            Array.Clear(counts, 0, counts.Length);
            recording = true;
        }

        string EndSegment(string label)
        {
            recording = false;
            var s = PerfStats.Compute(frames);
            return string.Join(",",
                label.Replace(',', ';'),
                s.SampleCount.ToString(Inv),
                s.AvgFps.ToString("F2", Inv),
                s.AvgFrameMs.ToString("F3", Inv),
                s.P95FrameMs.ToString("F3", Inv),
                s.P99FrameMs.ToString("F3", Inv),
                s.Low1PctFps.ToString("F2", Inv),
                Avg(0, 1).ToString("F0", Inv),
                Avg(1, 1).ToString("F0", Inv),
                Avg(2, Mb).ToString("F1", Inv),
                Avg(3, Mb).ToString("F1", Inv));
        }

        // Average of counter i divided by unit; -1 when the counter never gave a valid value.
        double Avg(int i, double unit) => counts[i] > 0 ? sums[i] / counts[i] / unit : -1.0;

        // ponytail: the file name has 1 s resolution; two writes in one second overwrite each other.
        void WriteCsv(List<string> rows)
        {
            try
            {
                string dir = string.IsNullOrEmpty(OutputDirectory) ? Application.persistentDataPath : OutputDirectory;
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, "perf_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", Inv) + ".csv");
                var lines = new List<string> { Header };
                lines.AddRange(rows);
                File.WriteAllLines(path, lines);
                LastCsvPath = path;
                Debug.Log("PerfRecorder: wrote " + path);
            }
            catch (Exception e)
            {
                Debug.LogError("PerfRecorder: cannot write CSV. " + e.Message, this);
            }
        }
    }
}
