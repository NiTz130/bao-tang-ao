using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Museum
{
    public class NarrativeDirector : MonoBehaviour
    {
        [SerializeField] Transform player;
        [SerializeField] ExhibitInfoPanel panel;
        [SerializeField] InputActionAsset actions;
        [SerializeField] ExhibitTargeting targeting; // optional: E on an aimed exhibit inspects it
        [SerializeField] ExhibitInspector inspector; // optional

        public const string InspectHint = "Kéo chuột trái để xoay · Cuộn để thu phóng · E hoặc Esc để quay lại";

        readonly NarrativeAnchorLogic logic = new NarrativeAnchorLogic();
        readonly List<NarrativeAnchor> anchors = new List<NarrativeAnchor>();
        readonly List<Vector3> positions = new List<Vector3>();
        readonly List<float> radii = new List<float>();
        InputAction interact;
        bool wasViewing;

        void OnEnable()
        {
            if (player == null || panel == null || actions == null)
            {
                Debug.LogError("NarrativeDirector: player, panel and actions must be assigned.", this);
                enabled = false;
                return;
            }

            // ponytail: anchors collected once; re-collect if anchors spawn at runtime.
            anchors.Clear();
            foreach (var a in FindObjectsByType<NarrativeAnchor>(FindObjectsSortMode.None))
            {
                if (a.Info == null)
                {
                    Debug.LogWarning($"NarrativeDirector: anchor '{a.name}' has no ExhibitInfo, skipped.", a);
                    continue;
                }
                anchors.Add(a);
            }

            interact = actions.FindAction("Player/Interact", true);
            interact.Enable();
            logic.Reset();
            panel.Hide();
        }

        void OnDisable()
        {
            interact?.Disable();
        }

        void Update()
        {
            positions.Clear();
            radii.Clear();
            for (int i = 0; i < anchors.Count; i++)
            {
                positions.Add(anchors[i].transform.position);
                radii.Add(anchors[i].Radius);
            }

            bool zoneChanged = logic.Update(positions, radii, player.position);

            if (interact.WasPressedThisFrame())
            {
                var aimed = targeting != null ? targeting.Current : null; // read before Enter disables targeting
                if (inspector != null && inspector.IsActive) inspector.Exit();
                else if (inspector != null && aimed != null && aimed.Info != null && inspector.Enter(aimed))
                    panel.Show(aimed.Info, true, InspectHint);
                else if (logic.ActiveIndex != -1)
                {
                    logic.ToggleExpanded();
                    panel.Show(anchors[logic.ActiveIndex].Info, logic.Expanded);
                }
            }

            // While an exhibit is held in view, the panel stays on it. Once it starts back (E or Esc), the panel follows the zone again.
            bool viewing = inspector != null && inspector.IsActive && inspector.Phase != InspectPhase.Leaving;
            if ((wasViewing && !viewing) || (zoneChanged && !viewing)) ShowZone();
            wasViewing = viewing;
        }

        void ShowZone()
        {
            if (logic.ActiveIndex == -1) panel.Hide();
            else panel.Show(anchors[logic.ActiveIndex].Info, logic.Expanded);
        }
    }
}
