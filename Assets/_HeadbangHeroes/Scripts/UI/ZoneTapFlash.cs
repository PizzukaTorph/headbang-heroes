using HeadbangHeroes.Charts;
using UnityEngine;

namespace HeadbangHeroes.UI
{
    /// <summary>
    /// Tap-feedback affordance (v0.0.2, P08 polish A): when the player taps, the ENTIRE screen
    /// section for that direction (Left / Right / Up / Down quadrant) flashes briefly, then fades.
    /// This makes it visible that the whole section is a valid tap area — you don't aim, you tap
    /// anywhere in that half. Complements the one-shot BangZoneHint at run start.
    ///
    /// PRESENTATION ONLY: driven by the input's Bang direction; it never reads/writes gameplay
    /// state and its graphics are non-raycast so they never intercept a tap.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZoneTapFlash : MonoBehaviour
    {
        [SerializeField] CanvasGroup left;
        [SerializeField] CanvasGroup right;
        [SerializeField] CanvasGroup up;
        [SerializeField] CanvasGroup down;
        [Tooltip("Seconds for the section flash to fade out.")]
        [SerializeField] float fadeSeconds = 0.25f;
        [Tooltip("Peak alpha of the flash.")]
        [SerializeField, Range(0f, 1f)] float peakAlpha = 0.35f;

        float lTimer, rTimer, uTimer, dTimer;

        void Awake() => ResetAll();

        public void ResetAll()
        {
            lTimer = rTimer = uTimer = dTimer = 0f;
            SetAlpha(left, 0f); SetAlpha(right, 0f); SetAlpha(up, 0f); SetAlpha(down, 0f);
        }

        /// <summary>Flash the section for a bang direction (called from the input's Bang event).</summary>
        public void Flash(BangDirection direction)
        {
            switch (direction)
            {
                case BangDirection.Left: lTimer = fadeSeconds; break;
                case BangDirection.Right: rTimer = fadeSeconds; break;
                case BangDirection.Up: uTimer = fadeSeconds; break;
                case BangDirection.Down: dTimer = fadeSeconds; break;
            }
        }

        void Update()
        {
            var dt = Time.deltaTime;
            Tick(left, ref lTimer, dt);
            Tick(right, ref rTimer, dt);
            Tick(up, ref uTimer, dt);
            Tick(down, ref dTimer, dt);
        }

        void Tick(CanvasGroup g, ref float timer, float dt)
        {
            if (g == null || timer <= 0f) return;
            timer -= dt;
            SetAlpha(g, timer <= 0f ? 0f : peakAlpha * Mathf.Clamp01(timer / Mathf.Max(0.01f, fadeSeconds)));
        }

        static void SetAlpha(CanvasGroup g, float a) { if (g != null) g.alpha = a; }
    }
}
