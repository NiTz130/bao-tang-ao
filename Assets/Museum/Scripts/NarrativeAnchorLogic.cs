using System;
using System.Collections.Generic;
using UnityEngine;

namespace Museum
{
    public sealed class NarrativeAnchorLogic
    {
        public int ActiveIndex { get; private set; } = -1;
        public bool Expanded { get; private set; }

        public static int FindNearest(IReadOnlyList<Vector3> positions, IReadOnlyList<float> radii, Vector3 player)
        {
            if (positions == null) throw new ArgumentNullException(nameof(positions));
            if (radii == null) throw new ArgumentNullException(nameof(radii));
            if (positions.Count != radii.Count) throw new ArgumentException("positions and radii must have the same count");

            int best = -1;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < positions.Count; i++)
            {
                float r = radii[i];
                if (r <= 0f) continue;
                float dx = positions[i].x - player.x;
                float dz = positions[i].z - player.z;
                float sqr = dx * dx + dz * dz;
                if (sqr <= r * r && sqr < bestSqr)
                {
                    best = i;
                    bestSqr = sqr;
                }
            }
            return best;
        }

        public bool Update(IReadOnlyList<Vector3> positions, IReadOnlyList<float> radii, Vector3 player)
        {
            int next = FindNearest(positions, radii, player);
            if (next == ActiveIndex) return false;
            ActiveIndex = next;
            Expanded = false;
            return true;
        }

        public void ToggleExpanded()
        {
            if (ActiveIndex == -1) return;
            Expanded = !Expanded;
        }

        public void Reset()
        {
            ActiveIndex = -1;
            Expanded = false;
        }
    }
}
