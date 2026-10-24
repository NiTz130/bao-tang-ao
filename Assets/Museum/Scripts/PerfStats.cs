using System.Collections.Generic;

namespace Museum
{
    public readonly struct PerfSummary
    {
        public readonly int SampleCount;
        public readonly float AvgFrameMs, AvgFps, P95FrameMs, P99FrameMs, Low1PctFps;

        public PerfSummary(int sampleCount, float avgFrameMs, float avgFps, float p95FrameMs, float p99FrameMs, float low1PctFps)
        {
            SampleCount = sampleCount;
            AvgFrameMs = avgFrameMs;
            AvgFps = avgFps;
            P95FrameMs = p95FrameMs;
            P99FrameMs = p99FrameMs;
            Low1PctFps = low1PctFps;
        }
    }

    public static class PerfStats
    {
        // Only values > 0 and finite count. AvgFps = 1000 / mean ms. Pxx = nearest-rank percentile (rank = ceil(n*p/100)).
        // Low1PctFps = 1000 / mean of the worst ceil(n/100) frames (at least 1). No valid sample -> all zeros.
        public static PerfSummary Compute(IReadOnlyList<float> frameMs)
        {
            var v = new List<float>();
            if (frameMs != null)
                for (int i = 0; i < frameMs.Count; i++)
                {
                    float f = frameMs[i];
                    if (f > 0f && !float.IsNaN(f) && !float.IsInfinity(f)) v.Add(f);
                }
            int n = v.Count;
            if (n == 0) return default;

            v.Sort();
            double sum = 0;
            for (int i = 0; i < n; i++) sum += v[i];
            float avg = (float)(sum / n);

            int worst = (n + 99) / 100; // ceil(n / 100), integer math avoids float rounding
            double worstSum = 0;
            for (int i = n - worst; i < n; i++) worstSum += v[i];

            return new PerfSummary(n, avg, 1000f / avg, Percentile(v, 95), Percentile(v, 99), (float)(1000.0 * worst / worstSum));
        }

        static float Percentile(List<float> sorted, int p)
        {
            int rank = (sorted.Count * p + 99) / 100; // ceil(n * p / 100), 1-based
            return sorted[rank - 1];
        }
    }
}
