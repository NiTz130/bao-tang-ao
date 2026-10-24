using System.Linq;
using NUnit.Framework;

namespace Museum.Tests
{
    public class PerfStatsTests
    {
        static float[] Range(int from, int to) => Enumerable.Range(from, to - from + 1).Select(i => (float)i).ToArray();

        [Test] public void Compute_KnownSeries_ExactValues()
        {
            // 1..20 ms, given in reverse order to prove sorting.
            var s = PerfStats.Compute(Range(1, 20).Reverse().ToArray());
            Assert.AreEqual(20, s.SampleCount);
            Assert.AreEqual(10.5f, s.AvgFrameMs, 1e-4f);
            Assert.AreEqual(1000f / 10.5f, s.AvgFps, 1e-3f);
            Assert.AreEqual(19f, s.P95FrameMs);
            Assert.AreEqual(20f, s.P99FrameMs);
            Assert.AreEqual(50f, s.Low1PctFps, 1e-3f); // worst 1 frame = 20 ms
        }

        [Test] public void Compute_100Samples_WorstOnePercentIsOneFrame()
        {
            var s = PerfStats.Compute(Range(1, 100));
            Assert.AreEqual(50.5f, s.AvgFrameMs, 1e-4f);
            Assert.AreEqual(95f, s.P95FrameMs);
            Assert.AreEqual(99f, s.P99FrameMs);
            Assert.AreEqual(10f, s.Low1PctFps, 1e-3f); // 1000 / 100
        }

        [Test] public void Compute_200Samples_WorstOnePercentIsTwoFrames()
        {
            var s = PerfStats.Compute(Range(1, 200));
            Assert.AreEqual(190f, s.P95FrameMs);
            Assert.AreEqual(198f, s.P99FrameMs);
            Assert.AreEqual(1000f / 199.5f, s.Low1PctFps, 1e-3f); // mean of 199 and 200
        }

        [Test] public void Compute_101Samples_WorstOnePercentRoundsUp()
        {
            var s = PerfStats.Compute(Range(1, 101));
            Assert.AreEqual(1000f / 100.5f, s.Low1PctFps, 1e-3f); // ceil(1.01) = 2 frames: 100 and 101
        }

        [Test] public void Compute_SingleSample_AllFieldsFromThatSample()
        {
            var s = PerfStats.Compute(new[] { 16f });
            Assert.AreEqual(1, s.SampleCount);
            Assert.AreEqual(16f, s.AvgFrameMs);
            Assert.AreEqual(62.5f, s.AvgFps, 1e-4f);
            Assert.AreEqual(16f, s.P95FrameMs);
            Assert.AreEqual(16f, s.P99FrameMs);
            Assert.AreEqual(62.5f, s.Low1PctFps, 1e-4f);
        }

        [Test] public void Compute_AllEqual_NoSpread()
        {
            var s = PerfStats.Compute(Enumerable.Repeat(10f, 50).ToArray());
            Assert.AreEqual(10f, s.AvgFrameMs, 1e-4f);
            Assert.AreEqual(100f, s.AvgFps, 1e-3f);
            Assert.AreEqual(10f, s.P95FrameMs);
            Assert.AreEqual(10f, s.P99FrameMs);
            Assert.AreEqual(100f, s.Low1PctFps, 1e-3f);
        }

        static void AssertEmpty(PerfSummary s)
        {
            Assert.AreEqual(0, s.SampleCount);
            Assert.AreEqual(0f, s.AvgFrameMs);
            Assert.AreEqual(0f, s.AvgFps);
            Assert.AreEqual(0f, s.P95FrameMs);
            Assert.AreEqual(0f, s.P99FrameMs);
            Assert.AreEqual(0f, s.Low1PctFps);
        }

        [Test] public void Compute_Null_ReturnsZeros() => AssertEmpty(PerfStats.Compute(null));
        [Test] public void Compute_Empty_ReturnsZeros() => AssertEmpty(PerfStats.Compute(new float[0]));
        [Test] public void Compute_AllInvalid_ReturnsZeros() =>
            AssertEmpty(PerfStats.Compute(new[] { 0f, -1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity }));

        [Test] public void Compute_MixedInvalid_IgnoresInvalidValues()
        {
            var s = PerfStats.Compute(new[] { float.NaN, -5f, 10f, 0f, float.PositiveInfinity, 20f, float.NegativeInfinity });
            Assert.AreEqual(2, s.SampleCount);
            Assert.AreEqual(15f, s.AvgFrameMs, 1e-4f);
            Assert.AreEqual(20f, s.P95FrameMs);
        }
    }
}
