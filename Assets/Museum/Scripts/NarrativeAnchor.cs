using System.Collections.Generic;
using UnityEngine;

namespace Museum
{
    public class NarrativeAnchor : MonoBehaviour
    {
        [SerializeField] ExhibitInfo info;
        [SerializeField, Min(0.1f)] float radius = 2.5f;
        [SerializeField] Light[] highlightLights;

        public ExhibitInfo Info => info;
        public float Radius => radius;
        public IReadOnlyList<Light> HighlightLights => highlightLights ?? System.Array.Empty<Light>();

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
