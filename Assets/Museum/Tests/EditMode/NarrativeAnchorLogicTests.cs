using System;
using NUnit.Framework;
using UnityEngine;

namespace Museum.Tests
{
    public class NarrativeAnchorLogicTests
    {
        static readonly Vector3[] One = { new Vector3(0, 0, 0) };

        static int Find(Vector3[] pos, float[] rad, Vector3 p) => NarrativeAnchorLogic.FindNearest(pos, rad, p);

        [Test] public void FindNearest_Inside_ReturnsIndex() =>
            Assert.AreEqual(0, Find(One, new[] { 2f }, new Vector3(1, 0, 0)));

        [Test] public void FindNearest_Outside_ReturnsMinusOne() =>
            Assert.AreEqual(-1, Find(One, new[] { 2f }, new Vector3(2.5f, 0, 0)));

        [Test] public void FindNearest_ExactlyOnBoundary_Included() =>
            Assert.AreEqual(0, Find(One, new[] { 2f }, new Vector3(2f, 0, 0)));

        [Test] public void FindNearest_IgnoresHeight() =>
            Assert.AreEqual(0, Find(One, new[] { 2f }, new Vector3(1, 100f, 0)));

        [Test] public void FindNearest_Overlapping_NearestWins()
        {
            var pos = new[] { new Vector3(0, 0, 0), new Vector3(3, 0, 0) };
            var rad = new[] { 5f, 5f };
            Assert.AreEqual(1, Find(pos, rad, new Vector3(2.5f, 0, 0)));
            Assert.AreEqual(0, Find(pos, rad, new Vector3(0.5f, 0, 0)));
        }

        [Test] public void FindNearest_EqualDistance_LowerIndexWins()
        {
            var pos = new[] { new Vector3(-1, 0, 0), new Vector3(1, 0, 0) };
            Assert.AreEqual(0, Find(pos, new[] { 5f, 5f }, Vector3.zero));
        }

        [Test] public void FindNearest_ZeroRadius_Ignored() =>
            Assert.AreEqual(-1, Find(One, new[] { 0f }, Vector3.zero));

        [Test] public void FindNearest_NegativeRadius_Ignored() =>
            Assert.AreEqual(-1, Find(One, new[] { -1f }, Vector3.zero));

        [Test] public void FindNearest_InvalidRadiusSkipped_NextValidReturned()
        {
            var pos = new[] { Vector3.zero, Vector3.zero };
            Assert.AreEqual(1, Find(pos, new[] { 0f, 1f }, Vector3.zero));
        }

        [Test] public void FindNearest_Empty_ReturnsMinusOne() =>
            Assert.AreEqual(-1, Find(new Vector3[0], new float[0], Vector3.zero));

        [Test] public void FindNearest_NullPositions_Throws() =>
            Assert.Throws<ArgumentNullException>(() => Find(null, new float[0], Vector3.zero));

        [Test] public void FindNearest_NullRadii_Throws() =>
            Assert.Throws<ArgumentNullException>(() => Find(new Vector3[0], null, Vector3.zero));

        [Test] public void FindNearest_CountMismatch_Throws() =>
            Assert.Throws<ArgumentException>(() => Find(One, new float[0], Vector3.zero));

        [Test] public void Update_ChangeReturnsTrue_UnchangedReturnsFalse()
        {
            var l = new NarrativeAnchorLogic();
            Assert.IsFalse(l.Update(One, new[] { 2f }, new Vector3(10, 0, 0)));
            Assert.IsTrue(l.Update(One, new[] { 2f }, Vector3.zero));
            Assert.AreEqual(0, l.ActiveIndex);
            Assert.IsFalse(l.Update(One, new[] { 2f }, new Vector3(0.5f, 0, 0)));
        }

        [Test] public void Update_LeavingAll_ResetsActiveAndReturnsTrue()
        {
            var l = new NarrativeAnchorLogic();
            l.Update(One, new[] { 2f }, Vector3.zero);
            Assert.IsTrue(l.Update(One, new[] { 2f }, new Vector3(10, 0, 0)));
            Assert.AreEqual(-1, l.ActiveIndex);
        }

        [Test] public void Update_ChangingAnchor_ResetsExpanded()
        {
            var pos = new[] { new Vector3(0, 0, 0), new Vector3(10, 0, 0) };
            var rad = new[] { 2f, 2f };
            var l = new NarrativeAnchorLogic();
            l.Update(pos, rad, Vector3.zero);
            l.ToggleExpanded();
            Assert.IsTrue(l.Expanded);
            l.Update(pos, rad, new Vector3(10, 0, 0));
            Assert.AreEqual(1, l.ActiveIndex);
            Assert.IsFalse(l.Expanded);
        }

        [Test] public void Update_StayingOnSameAnchor_KeepsExpanded()
        {
            var l = new NarrativeAnchorLogic();
            l.Update(One, new[] { 2f }, Vector3.zero);
            l.ToggleExpanded();
            l.Update(One, new[] { 2f }, new Vector3(1, 0, 0));
            Assert.IsTrue(l.Expanded);
        }

        [Test] public void ToggleExpanded_TogglesOnAndOff()
        {
            var l = new NarrativeAnchorLogic();
            l.Update(One, new[] { 2f }, Vector3.zero);
            l.ToggleExpanded();
            Assert.IsTrue(l.Expanded);
            l.ToggleExpanded();
            Assert.IsFalse(l.Expanded);
        }

        [Test] public void ToggleExpanded_NoActive_NoOp()
        {
            var l = new NarrativeAnchorLogic();
            l.ToggleExpanded();
            Assert.IsFalse(l.Expanded);
        }

        [Test] public void Reset_ClearsState()
        {
            var l = new NarrativeAnchorLogic();
            l.Update(One, new[] { 2f }, Vector3.zero);
            l.ToggleExpanded();
            l.Reset();
            Assert.AreEqual(-1, l.ActiveIndex);
            Assert.IsFalse(l.Expanded);
        }
    }
}
