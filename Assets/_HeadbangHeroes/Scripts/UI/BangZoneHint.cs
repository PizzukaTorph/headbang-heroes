using UnityEngine;

namespace HeadbangHeroes.UI
{
    /// <summary>
    /// Onboarding affordance (v0.0.2, P08 polish A): briefly reveals the FOUR real bang zones the
    /// input maps to. The screen is divided from its centre into Left / Right / Up / Down quadrants
    /// (matching <see cref="Input.HeadbangInput.ResolveZone"/>): a tap anywhere in a quadrant bangs
    /// that direction. This shows that honestly, then fades so play stays avatar-centric per
    /// GAMEPLAY_UX_V1 ("play by watching the avatar, not a detached interface").
    ///
    /// PRESENTATION ONLY: it draws hint graphics and fades; it never reads/writes gameplay state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BangZoneHint : MonoBehaviour
    {
        [SerializeField] CanvasGroup group;          // whole hint overlay
        [Tooltip("Seconds fully visible before it starts fading.")]
        [SerializeField] float holdSeconds = 2.0f;
        [Tooltip("Seconds to fade out after the hold.")]
        [SerializeField] float fadeSeconds = 1.0f;
        [Tooltip("Show only on the first run of the session, or every run.")]
        [SerializeField] bool everyRun = true;

        float timer;
        bool shownOnce;

        void Awake() { if (group != null) group.alpha = 0f; }

        /// <summary>Reveal the zone hint (called at run start). Respects the every-run/first-run flag.</summary>
        public void Show()
        {
            if (group == null) return;
            if (!everyRun && shownOnce) { group.alpha = 0f; return; }
            shownOnce = true;
            timer = holdSeconds + fadeSeconds;
            group.alpha = 1f;
        }

        /// <summary>Hide immediately (e.g. on abort).</summary>
        public void HideNow()
        {
            timer = 0f;
            if (group != null) group.alpha = 0f;
        }

        void Update()
        {
            if (group == null || timer <= 0f) return;
            timer -= Time.deltaTime;
            // Full alpha during the hold window, linear fade over the last fadeSeconds.
            group.alpha = fadeSeconds <= 0f
                ? (timer > 0f ? 1f : 0f)
                : Mathf.Clamp01(timer / fadeSeconds);
            if (timer <= 0f) group.alpha = 0f;
        }
    }
}
