using UnityEngine;

namespace Museum
{
    public static class InspectMath
    {
        public static float FitScale(float maxExtent, float targetSize) =>
            maxExtent <= 0f || float.IsNaN(maxExtent) || float.IsInfinity(maxExtent) ? 1f : targetSize / maxExtent;

        public static float Ease(float t) => Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

        public static float Zoom(float current, float scrollSteps, float min = 0.6f, float max = 1.6f) =>
            Mathf.Clamp(current * Mathf.Pow(1.1f, scrollSteps), min, max);

        public static float Bob(float time, float amplitude, float period) =>
            period <= 0f || amplitude == 0f ? 0f : amplitude * Mathf.Sin(2f * Mathf.PI * time / period);
    }
}
