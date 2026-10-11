using UnityEngine;

namespace Museum
{
    // Full-screen black overlay that fades out once when the scene starts, then switches itself off.
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : MonoBehaviour
    {
        const float MaxStep = 0.05f; // the first frames after loading can be long; do not skip the fade

        [SerializeField, Min(0f)] float seconds = 1f;

        CanvasGroup group;

        void Awake()
        {
            group = GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
            group.alpha = 1f;
            if (seconds <= 0f) Finish();
        }

        void Update()
        {
            group.alpha = Mathf.MoveTowards(group.alpha, 0f, Mathf.Min(Time.unscaledDeltaTime, MaxStep) / seconds);
            if (group.alpha <= 0f) Finish();
        }

        void Finish()
        {
            group.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
}
