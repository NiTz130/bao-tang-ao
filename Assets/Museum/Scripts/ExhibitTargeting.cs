using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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

        // Key 1 toggles the laser: long reach, a red line from the "hand" and a dot at the end point.
        [SerializeField] InputAction laserToggle = new InputAction("Laser", InputActionType.Button, "<Keyboard>/1");
        [SerializeField, Min(1f)] float laserDistance = 20f;
        [SerializeField] LineRenderer laserLine; // optional
        [SerializeField] Transform laserDot; // optional

        public NarrativeAnchor Current { get; private set; }
        public bool LaserOn => laserOn;

        // Base intensity is stored once per light, so repeated hovers cannot drift it.
        readonly Dictionary<Light, float> baseIntensity = new Dictionary<Light, float>();
        readonly HashSet<Light> boosted = new HashSet<Light>();
        readonly List<Light> destroyed = new List<Light>();
        float blend; // 0 = idle, 1 = on target
        bool laserOn;

        void OnEnable()
        {
            if (cam == null || crosshair == null)
            {
                Debug.LogError("ExhibitTargeting: cam and crosshair must be assigned.", this);
                enabled = false;
                return;
            }
            laserToggle.Enable();
        }

        void OnDisable()
        {
            foreach (var pair in baseIntensity)
                if (pair.Key != null) pair.Key.intensity = pair.Value;
            Current = null;
            blend = 0f;
            if (crosshair != null) crosshair.enabled = false;
            laserToggle.Disable();
            ShowLaser(false); // laserOn is kept: the inspector disables this component on every inspect
        }

        void Update()
        {
            if (laserToggle.WasPressedThisFrame()) laserOn = !laserOn;
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

        // Runs after the camera has moved, so the line does not lag.
        void LateUpdate()
        {
            bool show = laserOn && Cursor.lockState != CursorLockMode.None;
            ShowLaser(show);
            if (!show) return;
            var t = cam.transform;
            var end = Cast(out var hit) ? hit.point : t.position + t.forward * laserDistance;
            if (laserLine != null)
            {
                laserLine.SetPosition(0, t.position + t.right * 0.15f - t.up * 0.2f + t.forward * 0.1f);
                laserLine.SetPosition(1, end);
            }
            if (laserDot != null) laserDot.position = end;
        }

        void ShowLaser(bool on)
        {
            if (laserLine != null) laserLine.enabled = on;
            if (laserDot != null) laserDot.gameObject.SetActive(on);
        }

        bool Cast(out RaycastHit hit) =>
            Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out hit, laserOn ? laserDistance : maxDistance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

        NarrativeAnchor FindTarget() => Cast(out var hit) ? hit.collider.GetComponentInParent<NarrativeAnchor>() : null;
    }
}
