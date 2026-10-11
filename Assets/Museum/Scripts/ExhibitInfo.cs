using UnityEngine;

namespace Museum
{
    [CreateAssetMenu(menuName = "Museum/Exhibit Info")]
    public class ExhibitInfo : ScriptableObject
    {
        public string title;
        public string period;
        public string material;
        public string origin;
        public string source;
        [TextArea(2, 4)] public string summary;
        [TextArea(5, 20)] public string story;
    }
}
