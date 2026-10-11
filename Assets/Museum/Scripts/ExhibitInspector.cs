using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Museum
{
    public enum InspectPhase { Idle, Entering, Viewing, Leaving }

    // Lifts an exhibit's model off its plinth to float in front of the camera.
    // Drag with the left button to rotate, scroll to zoom. The model always returns to its exact local pose.
    public class ExhibitInspector : MonoBehaviour
    {
        [SerializeField] Camera cam;
        [SerializeField] FirstPersonController player;
        [SerializeField] ExhibitTargeting targeting;
        [SerializeField] Light inspectLight;
        [SerializeField] Volume inspectVolume;
        [SerializeField] InputActionAsset actions;
        [SerializeField, Min(0f)] float moveSeconds = 0.45f; // 0 = instant
        [SerializeField, Min(0.1f)] float viewDistance = 0.85f;
        [SerializeField] Vector2 viewportPoint = new Vector2(0.66f, 0.5f); // right of the info panel
        [SerializeField, Min(0.01f)] float viewSize = 0.45f; // largest side after fitting
        [SerializeField] float rotateDegreesPerPixel = 0.25f;
        [SerializeField, Min(0f)] float bobAmplitude = 0.012f;
        [SerializeField, Min(0f)] float bobPeriod = 3f;
        [SerializeField] float idleSpinDegreesPerSecond = 12f;
        [SerializeField, Min(0f)] float idleSpinDelay = 2f;
        [SerializeField, Min(0f)] float lightIntensity = 4f;

        public InspectPhase Phase { get; private set; }
        public bool IsActive => Phase != InspectPhase.Idle;
        public NarrativeAnchor Anchor { get; private set; }

        readonly List<Collider> disabledColliders = new List<Collider>();
        InputAction look, attack;
        Transform model, homeParent;
        Vector3 homeLocalPosition, homeLocalScale;
        Quaternion homeLocalRotation;
        Vector3 startCenter, startOffset, startScale, startPosition, leaveScale;
        Quaternion leaveRotation;
        float fit, progress, zoom = 1f, viewTime, lastDrag, leaveWeight;

        void OnEnable()
        {
            if (cam == null || player == null || targeting == null || inspectLight == null || inspectVolume == null || actions == null)
            {
                Debug.LogError("ExhibitInspector: cam, player, targeting, inspectLight, inspectVolume and actions must be assigned.", this);
                enabled = false;
                return;
            }

            // Not disabled in OnDisable: FirstPersonController shares Player/Look.
            look = actions.FindAction("Player/Look", true);
            attack = actions.FindAction("Player/Attack", true);
            look.Enable();
            attack.Enable();
            inspectLight.enabled = false;
            inspectVolume.weight = 0f;
        }

        // Data safety: the model is never left off its plinth.
        void OnDisable()
        {
            if (IsActive) Restore();
        }

        public bool Enter(NarrativeAnchor anchor)
        {
            if (!enabled || IsActive || anchor == null) return false;
            var found = FindModel(anchor.transform);
            if (found == null) return false;

            model = found;
            Anchor = anchor;
            homeParent = model.parent;
            homeLocalPosition = model.localPosition;
            homeLocalRotation = model.localRotation;
            homeLocalScale = model.localScale;

            disabledColliders.Clear();
            foreach (var c in model.GetComponentsInChildren<Collider>())
                if (c.enabled) { c.enabled = false; disabledColliders.Add(c); }

            model.SetParent(null, true);
            var bounds = Bounds(model);
            startCenter = bounds.center;
            startOffset = bounds.center - model.position;
            startScale = model.localScale;
            fit = InspectMath.FitScale(Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z), viewSize);

            progress = 0f;
            zoom = 1f;
            lastDrag = float.NegativeInfinity;
            Phase = InspectPhase.Entering;
            player.Frozen = true;
            targeting.enabled = false;
            inspectLight.enabled = true;
            inspectLight.intensity = 0f;
            return true;
        }

        public void Exit()
        {
            if (Phase != InspectPhase.Entering && Phase != InspectPhase.Viewing) return;
            if (moveSeconds <= 0f || model == null) { Restore(); return; }
            startPosition = model.position;
            leaveRotation = model.rotation;
            leaveScale = model.localScale;
            leaveWeight = inspectVolume.weight;
            progress = 0f;
            Phase = InspectPhase.Leaving;
        }

        void Update()
        {
            if (!IsActive) return;
            if (model == null) { Restore(); return; }
            if (homeParent == null)
            {
                // The anchor is gone: drop its orphaned model instead of leaving it floating.
                Destroy(model.gameObject);
                model = null;
                Restore();
                return;
            }

            if (Phase != InspectPhase.Leaving && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Exit();

            float dt = Time.unscaledDeltaTime;
            Vector3 viewPoint = cam.ViewportToWorldPoint(new Vector3(viewportPoint.x, viewportPoint.y, viewDistance));

            switch (Phase)
            {
                case InspectPhase.Entering:
                {
                    progress = moveSeconds <= 0f ? 1f : Mathf.Min(1f, progress + dt / moveSeconds);
                    float e = InspectMath.Ease(progress);
                    float s = Mathf.Lerp(1f, fit, e);
                    model.localScale = startScale * s;
                    model.position = Vector3.Lerp(startCenter, viewPoint, e) - startOffset * s;
                    SetEffects(e);
                    if (progress >= 1f) { Phase = InspectPhase.Viewing; viewTime = 0f; }
                    break;
                }
                case InspectPhase.Viewing:
                {
                    if (attack.IsPressed())
                    {
                        Vector2 d = look.ReadValue<Vector2>() * rotateDegreesPerPixel;
                        model.rotation = Quaternion.AngleAxis(-d.x, cam.transform.up) * Quaternion.AngleAxis(d.y, cam.transform.right) * model.rotation;
                        lastDrag = Time.unscaledTime;
                    }
                    else if (Time.unscaledTime - lastDrag >= idleSpinDelay)
                        model.rotation = Quaternion.AngleAxis(idleSpinDegreesPerSecond * dt, Vector3.up) * model.rotation;

                    // ponytail: one zoom step per frame of scroll, whatever the platform's scroll units; read Mouse.current because no action is bound to scroll.
                    if (Mouse.current != null)
                        zoom = InspectMath.Zoom(zoom, Mathf.Clamp(Mouse.current.scroll.ReadValue().y, -1f, 1f));
                    model.localScale = startScale * (fit * zoom);

                    viewTime += dt;
                    Vector3 offset = Bounds(model).center - model.position;
                    model.position = viewPoint + cam.transform.up * InspectMath.Bob(viewTime, bobAmplitude, bobPeriod) - offset;
                    break;
                }
                case InspectPhase.Leaving:
                {
                    progress = Mathf.Min(1f, progress + dt / moveSeconds);
                    float e = InspectMath.Ease(progress);
                    model.position = Vector3.Lerp(startPosition, homeParent.TransformPoint(homeLocalPosition), e);
                    model.rotation = Quaternion.Slerp(leaveRotation, homeParent.rotation * homeLocalRotation, e);
                    model.localScale = Vector3.Lerp(leaveScale, Vector3.Scale(homeParent.lossyScale, homeLocalScale), e);
                    SetEffects(leaveWeight * (1f - e));
                    if (progress >= 1f) Restore();
                    break;
                }
            }
        }

        void SetEffects(float weight)
        {
            inspectVolume.weight = weight;
            inspectLight.intensity = lightIntensity * weight;
        }

        // Shared by the end of Leaving, OnDisable and lost references. Every reference may already be destroyed.
        void Restore()
        {
            if (model != null && homeParent != null)
            {
                model.SetParent(homeParent, false);
                model.localPosition = homeLocalPosition;
                model.localRotation = homeLocalRotation;
                model.localScale = homeLocalScale;
            }
            foreach (var c in disabledColliders)
                if (c != null) c.enabled = true;
            disabledColliders.Clear();

            if (player != null) player.Frozen = false;
            if (targeting != null) targeting.enabled = true;
            if (inspectLight != null) inspectLight.enabled = false;
            if (inspectVolume != null) inspectVolume.weight = 0f;
            model = null;
            homeParent = null;
            Anchor = null;
            zoom = 1f;
            Phase = InspectPhase.Idle;
        }

        static Transform FindModel(Transform anchor)
        {
            foreach (Transform child in anchor)
                if (child.GetComponentInChildren<Renderer>() != null) return child;
            return null;
        }

        static Bounds Bounds(Transform root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            var b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            return b;
        }
    }
}
