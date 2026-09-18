using HeadbangHeroes.Audio;
using HeadbangHeroes.Charts;
using UnityEngine;

namespace HeadbangHeroes.UI
{
    /// <summary>
    /// Timing cue per ADR 0001 (v0.0.2): a PULSE-IN-SECTOR with build-up. Instead of a circle
    /// shrinking on the head, the screen quadrant of the expected direction (Left/Right/Up/Down)
    /// pulses — intensity/scale grow over the approach window and PEAK exactly on the event time,
    /// then relax. This unifies WHEN (build-up + peak) and WHERE (which sector) and reads as "feel
    /// the beat" rather than "aim at a dot".
    ///
    /// Time authority is the DSP-backed <see cref="AudioClock"/>; this is presentation only and
    /// never shifts authored time or scoring. Graphics are non-raycast (never intercept a tap).
    /// One sector pulses per active event (no note-highway).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SectorPulseCue : MonoBehaviour
    {
        [SerializeField] AudioClock clock;
        [SerializeField] CanvasGroup left;
        [SerializeField] CanvasGroup right;
        [SerializeField] CanvasGroup up;
        [SerializeField] CanvasGroup down;

        [Header("Look")]
        [SerializeField, Range(0f, 1f)] float peakAlpha = 0.55f;
        [Tooltip("Curve shaping the build-up from cue appearance (0) to the event (1).")]
        [SerializeField] AnimationCurve buildup = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [Tooltip("Seconds the sector keeps flashing after the event before it clears.")]
        [SerializeField] float releaseSeconds = 0.18f;

        [SerializeField] Color approachColor = new Color(0.95f, 0.55f, 0.2f, 1f);
        [SerializeField] Color hitColor = new Color(0.3f, 1f, 0.45f, 1f);

        BangDirection activeDir;
        CanvasGroup activeGroup;
        UnityEngine.UI.Graphic activeGraphic;
        double eventTime;
        double approachSeconds = 1.0;
        double hitWindowSeconds = 0.15;
        bool running;
        float releaseTimer;

        void OnEnable() => ResetCue();

        /// <summary>Begin a pulse for an event: direction sector, event song-time, approach lead, hit window.</summary>
        public void Show(BangDirection direction, double eventSongTime, double approachDuration, double hitWindow)
        {
            ClearAll();
            activeDir = direction;
            activeGroup = GroupFor(direction);
            activeGraphic = activeGroup != null ? activeGroup.GetComponent<UnityEngine.UI.Graphic>() : null;
            eventTime = eventSongTime;
            approachSeconds = System.Math.Max(0.05, approachDuration);
            hitWindowSeconds = System.Math.Max(0.02, hitWindow);
            running = true;
            releaseTimer = 0f;
            Apply();
        }

        /// <summary>Clear the current pulse (event resolved / hidden).</summary>
        public void Hide()
        {
            // Enter a short release so the peak is visible for a beat even if resolved exactly on time.
            if (running) { running = false; releaseTimer = releaseSeconds; }
        }

        public void ResetCue()
        {
            running = false;
            releaseTimer = 0f;
            ClearAll();
        }

        void Update()
        {
            if (running && clock != null)
            {
                Apply();
            }
            else if (releaseTimer > 0f)
            {
                releaseTimer -= Time.deltaTime;
                if (activeGroup != null)
                    activeGroup.alpha = peakAlpha * Mathf.Clamp01(releaseTimer / Mathf.Max(0.01f, releaseSeconds));
                if (releaseTimer <= 0f) ClearAll();
            }
        }

        void Apply()
        {
            if (activeGroup == null) return;

            var remaining = eventTime - clock.SongTime;                 // >0 before event
            // Normalized build-up: 0 at cue appearance (approachSeconds before), 1 at the event.
            var t = 1.0 - remaining / approachSeconds;
            var shaped = buildup.Evaluate(Mathf.Clamp01((float)t));

            var inHit = System.Math.Abs(remaining) <= hitWindowSeconds;
            activeGroup.alpha = inHit ? peakAlpha : peakAlpha * shaped;

            if (activeGraphic != null)
                activeGraphic.color = inHit ? hitColor : approachColor;
        }

        CanvasGroup GroupFor(BangDirection d)
        {
            switch (d)
            {
                case BangDirection.Left: return left;
                case BangDirection.Right: return right;
                case BangDirection.Up: return up;
                default: return down;
            }
        }

        void ClearAll()
        {
            SetOff(left); SetOff(right); SetOff(up); SetOff(down);
            activeGroup = null;
            activeGraphic = null;
        }

        static void SetOff(CanvasGroup g) { if (g != null) g.alpha = 0f; }
    }
}
