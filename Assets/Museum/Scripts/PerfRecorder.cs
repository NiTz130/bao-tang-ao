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
    // Free mode: F9 starts/stops a recording. F3 shows/hides the live overlay (for demo video).
    // Draw-call and triangle counters may read -1 outside the editor and development builds.
    public class PerfRecorder : MonoBehaviour
    {
        [SerializeField] Transform player;
        [SerializeField] Transform[] benchmarkPoints;
        [SerializeField] float settleSeconds = 2f;
        [SerializeField] float recordSeconds = 10f;
        [SerializeField] bool showOverlay;

        // Pass thresholds from the report (K01, K02).
        public const float MinFps = 60f, MaxP99Ms = 33f;

        // Mesh optimization of the 8 shipped scans that have an original count in SOURCE.txt (Docs/cuoi_ki/data/models.csv).
        public const int ScanTrisBefore = 9245508, ScanTrisAfter = 1089241;
        public const string OptimizationLine = "Tối ưu lưới 8 mô hình quét: 9.245.508 → 1.089.241 tam giác (giảm 88,2%)";

        public const string Header = "point,samples,avg_fps,avg_ms,p95_ms,p99_ms,low1_fps,draw_calls,triangles,sys_mem_mb,gfx_mem_mb";
        const string WiringError = "PerfRecorder: player and benchmark points must be assigned.";
        const double Mb = 1024.0 * 1024.0;
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;


        // Empty means Application.persistentDataPath. Tests set it to a temp folder.
        public string OutputDirectory { get; set; }
        public string LastCsvPath { get; private set; }
        public bool IsBenchmarking { get; private set; }
        public bool ShowOverlay { get => showOverlay; set => showOverlay = value; }
        public PerfSummary LastSummary { get; private set; }
        public string LastLabel { get; private set; }

        // Counters: draw calls, triangles, system memory, gfx memory.
        readonly ProfilerRecorder[] recorders = new ProfilerRecorder[4];
        readonly double[] sums = new double[4];
        readonly int[] counts = new int[4];
        readonly List<float> frames = new List<float>();
        bool recordersStarted, recording, fromCommandLine;

        // Overlay: values refresh every 0.5 s so the numbers stay readable on video.
        float windowTime, recordStart;
        int windowFrames;
        float liveFps, liveMs;
        string pointName = "";
        int pointIndex;
        GUIStyle style;

        // Original triangle counts from each model's SOURCE.txt, keyed by mesh name (= model folder name).
        static readonly Dictionary<string, int> OriginalTris = new Dictionary<string, int>
        {
            { "NgheThaiHoa", 2956829 }, { "BronzeCoinJar", 2802032 }, { "TuDucStele", 1918731 },
            { "ChamShiva", 698012 }, { "LordSivaMicmac", 620549 }, { "CaoDinh", 99887 },
            { "CoveredJarBlueWhite", 94021 }, { "HoiAnLantern", 55447 },
        };
        const int ScanMinTris = 20000;
        readonly List<MeshFilter> scans = new List<MeshFilter>();

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
            foreach (var mf in FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
                if (mf.sharedMesh != null && TriangleCount(mf.sharedMesh) >= ScanMinTris) scans.Add(mf);
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
            for (int p = 0; p < benchmarkPoints.Length; p++)
            {
                var point = benchmarkPoints[p];
                if (point == null) continue;
                pointIndex = p + 1;
                pointName = point.name;
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
            if (Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame) showOverlay = !showOverlay;
            windowTime += Time.unscaledDeltaTime;
            windowFrames++;
            if (windowTime >= 0.5f)
            {
                liveFps = windowFrames / windowTime;
                liveMs = windowTime * 1000f / windowFrames;
                windowTime = 0f;
                windowFrames = 0;
            }

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
            recordStart = Time.realtimeSinceStartup;
            recording = true;
        }

        string EndSegment(string label)
        {
            recording = false;
            var s = PerfStats.Compute(frames);
            LastSummary = s;
            LastLabel = label;
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

        static string Counter(ProfilerRecorder r, double unit, string format) =>
            r.Valid && r.LastValue >= 0 ? (r.LastValue / unit).ToString(format, Inv) : "n/a";

        static long TriangleCount(Mesh m)
        {
            long t = 0;
            for (int i = 0; i < m.subMeshCount; i++) t += m.GetIndexCount(i) / 3;
            return t;
        }

        // Vietnamese style: 1.234.567 (no culture lookup, so it works the same in every player).
        static string Thousands(long n) => n.ToString("N0", Inv).Replace(',', '.');

        public static string ScanLine(string meshName, long tris)
        {
            string line = $"Đang chỉ: {meshName}: {Thousands(tris)} tam giác";
            if (!OriginalTris.TryGetValue(meshName, out int before)) return line + " (không có số gốc)";
            double cut = 100.0 * (before - tris) / before;
            return line + $" (gốc {Thousands(before)}, giảm {cut.ToString("F1", Inv).Replace('.', ',')}%)";
        }

        // ponytail: picks by bounding box along the camera ray (scans without colliders); a cabinet box may win over its contents.
        MeshFilter AimedScan()
        {
            var cam = Camera.main;
            if (cam == null) return null;
            var ray = new Ray(cam.transform.position, cam.transform.forward);
            MeshFilter best = null;
            float bestDist = 10f;
            foreach (var mf in scans)
            {
                if (mf == null || !mf.TryGetComponent<Renderer>(out var r) || !r.enabled) continue;
                if (r.bounds.IntersectRay(ray, out float d) && d < bestDist) { bestDist = d; best = mf; }
            }
            return best;
        }

        static string Verdict(bool ok) => ok ? "ĐẠT" : "KHÔNG ĐẠT";

        public string OverlayText()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("ĐO HIỆU NĂNG (F3 ẩn/hiện)");
            sb.AppendLine($"FPS: {liveFps.ToString("F0", Inv)}   Khung hình: {liveMs.ToString("F2", Inv)} ms");
            if (recordersStarted)
            {
                sb.AppendLine($"Draw calls: {Counter(recorders[0], 1, "F0")}   Tam giác: {Counter(recorders[1], 1, "N0")}");
                sb.AppendLine($"RAM: {Counter(recorders[2], Mb, "F0")} MB   VRAM: {Counter(recorders[3], Mb, "F0")} MB");
            }
            sb.AppendLine(OptimizationLine);
            var aimed = AimedScan();
            sb.AppendLine(aimed != null ? ScanLine(aimed.sharedMesh.name, TriangleCount(aimed.sharedMesh)) : "Đang chỉ: (hướng tâm ngắm vào một mô hình quét)");
            if (IsBenchmarking)
                sb.AppendLine($"Benchmark {pointIndex}/{benchmarkPoints.Length}: {pointName}" + (recording ? $"  ĐANG GHI {(Time.realtimeSinceStartup - recordStart).ToString("F0", Inv)} s" : "  ổn định..."));
            else
                sb.AppendLine(recording ? $"ĐANG GHI {(Time.realtimeSinceStartup - recordStart).ToString("F0", Inv)} s (F9 dừng)" : "F9: bắt đầu ghi");
            if (LastLabel != null)
            {
                var s = LastSummary;
                sb.AppendLine($"Kết quả [{LastLabel}]: {s.AvgFps.ToString("F1", Inv)} FPS, p99 {s.P99FrameMs.ToString("F2", Inv)} ms, 1% low {s.Low1PctFps.ToString("F1", Inv)} FPS");
                sb.AppendLine($"FPS ≥ {MinFps:F0}: {Verdict(s.AvgFps >= MinFps)}   p99 ≤ {MaxP99Ms:F0} ms: {Verdict(s.P99FrameMs <= MaxP99Ms)}");
            }
            if (LastCsvPath != null) sb.AppendLine("CSV: " + LastCsvPath);
            return sb.ToString();
        }

        void OnGUI()
        {
            if (!showOverlay) return;
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.box) { fontSize = Mathf.Max(14, Screen.height / 50), alignment = TextAnchor.UpperLeft, richText = false };
                style.normal.textColor = Color.white;
                style.padding = new RectOffset(10, 10, 8, 8);
            }
            var content = new GUIContent(OverlayText().TrimEnd());
            var size = style.CalcSize(content);
            GUI.Box(new Rect(10, 10, size.x, size.y), content, style);
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
