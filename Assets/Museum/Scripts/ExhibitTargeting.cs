using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Museum
{
    // Casts a ray from the camera. The aimed exhibit's spotlights brighten; the crosshair grows and changes color.
    public class ExhibitTargeting : MonoBehaviour
    {
        [SerializeField] Camera cam;
        [SerializeField] Image crosshair;
        [SerializeField, Min(0.1f)] float maxDistance = 4f;
        [SerializeField, Min(1f)] float lightBoost = 1.5f;
        [SerializeField, Min(0f)] float transitionSeconds = 0.12f; // 0 = instant
        [SerializeField] float idleSize = 6f, targetSize = 12f;
        [SerializeField] Color idleColor = new Color(1f, 1f, 1f, 0.7f);
        [SerializeField] Color targetColor = new Color32(0xE8, 0xB0, 0x4A, 0xFF);

        public NarrativeAnchor Current { get; private set; }

        // Base intensity is stored once per light, so repeated hovers cannot drift it.
        readonly Dictionary<Light, float> baseIntensity = new Dictionary<Light, float>();
        readonly HashSet<Light> boosted = new HashSet<Light>();
        readonly List<Light> destroyed = new List<Light>();
        float blend; // 0 = idle, 1 = on target

        void OnEnable()
        {
            if (cam == null || crosshair == null)
            {
                Debug.LogError("ExhibitTargeting: cam and crosshair must be assigned.", this);
                enabled = false;
            }
        }

        void OnDisable()
        {
            foreach (var pair in baseIntensity)
                if (pair.Key != null) pair.Key.intensity = pair.Value;
            Current = null;
            blend = 0f;
            if (crosshair != null) crosshair.enabled = false;
        }

        void Update()
        {
            Current = FindTarget();

            boosted.Clear();
            if (Current != null)
                foreach (var light in Current.HighlightLights)
                {
                    if (light == null) continue;
                    if (!baseIntensity.ContainsKey(light)) baseIntensity[light] = light.intensity;
                    boosted.Add(light);
                }

            float t = transitionSeconds <= 0f ? 1f : Time.unscaledDeltaTime / transitionSeconds;
            destroyed.Clear();
            foreach (var pair in baseIntensity)
            {
                var light = pair.Key;
                if (light == null) { destroyed.Add(light); continue; }
                float target = boosted.Contains(light) ? pair.Value * lightBoost : pair.Value;
                float step = pair.Value * (lightBoost - 1f) * t;
                light.intensity = t >= 1f ? target : Mathf.MoveTowards(light.intensity, target, step);
            }
            foreach (var light in destroyed) baseIntensity.Remove(light);

            blend = Mathf.MoveTowards(blend, Current != null ? 1f : 0f, t);
            crosshair.enabled = Cursor.lockState != CursorLockMode.None;
            crosshair.rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(idleSize, targetSize, blend);
            crosshair.color = Color.Lerp(idleColor, targetColor, blend);
        }

        NarrativeAnchor FindTarget()
        {
            var ray = new Ray(cam.transform.position, cam.transform.forward);
            return Physics.Raycast(ray, out var hit, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide)
                ? hit.collider.GetComponentInParent<NarrativeAnchor>()
                : null;
        }
    }
}
