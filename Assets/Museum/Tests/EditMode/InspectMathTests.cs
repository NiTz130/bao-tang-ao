using System;
using NUnit.Framework;

namespace Museum.Tests
{
    public class InspectMathTests
    {
        static float Call(string name, params object[] args)
        {
            var type = typeof(NarrativeAnchor).Assembly.GetType("Museum.InspectMath");
            Assert.IsNotNull(type, "InspectMath must exist");
            return (float)type.GetMethod(name).Invoke(null, args);
        }

        [Test] public void FitScale_LargeModel_ShrinksToTarget() => Assert.AreEqual(0.1324f, Call("FitScale", 3.4f, 0.45f), 0.0001f);
        [Test] public void FitScale_SmallModel_GrowsToTarget() => Assert.AreEqual(2.1429f, Call("FitScale", 0.21f, 0.45f), 0.0001f);
        [Test] public void FitScale_ZeroNegativeOrNaN_ReturnsOne()
        {
            foreach (float value in new[] { 0f, -1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                Assert.AreEqual(1f, Call("FitScale", value, 0.45f));
        }
        [Test] public void Ease_Endpoints_AndClamp()
        {
            foreach (float value in new[] { -1f, 0f, 0.5f, 1f, 2f })
                Assert.AreEqual(Math.Max(0f, Math.Min(1f, value)), Call("Ease", value), 1e-5f);
        }
        [Test] public void Zoom_StepsUpAndDown()
        {
            Assert.AreEqual(1.1f, Call("Zoom", 1f, 1f, 0.6f, 1.6f), 1e-4f);
            Assert.AreEqual(0.9091f, Call("Zoom", 1f, -1f, 0.6f, 1.6f), 1e-4f);
        }
        [Test] public void Zoom_ManySteps_StopsAtLimits()
        {
            Assert.AreEqual(1.6f, Call("Zoom", 1f, 50f, 0.6f, 1.6f));
            Assert.AreEqual(0.6f, Call("Zoom", 1f, -50f, 0.6f, 1.6f));
        }
        [Test] public void Bob_ZeroAmplitudeOrPeriod_IsZero()
        {
            Assert.AreEqual(0f, Call("Bob", 1f, 0f, 3f));
            Assert.AreEqual(0f, Call("Bob", 1f, 0.012f, 0f));
            Assert.AreEqual(0f, Call("Bob", 1f, 0.012f, -1f));
        }
        [Test] public void Bob_IsPeriodic()
        {
            Assert.AreEqual(Call("Bob", 0.4f, 0.012f, 3f), Call("Bob", 3.4f, 0.012f, 3f), 1e-5f);
            Assert.AreEqual(0.012f, Call("Bob", 0.75f, 0.012f, 3f), 1e-5f);
        }
    }
}
