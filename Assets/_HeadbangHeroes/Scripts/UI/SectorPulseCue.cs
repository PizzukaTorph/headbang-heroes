using HeadbangHeroes.Audio;
using HeadbangHeroes.Charts;
using UnityEngine;
using UnityEngine.UI;

namespace HeadbangHeroes.UI
{
    /// <summary>
    /// Timing cue per ADR 0001 (v0.0.2): a PULSE pill next to the avatar in the expected direction
    /// (Left/Right/Up/Down). Instead of a circle shrinking on the head, a small pill GROWS and
    /// brightens over the approach window and PEAKS exactly on the event time, then relaxes —
    /// "feel the beat", not "aim at a dot". The tap area remains the whole screen sector; this pill
    /// is only the signal (where + when), not the touch target.
    ///
    /// Time authority is the DSP-backed <see cref="AudioClock"/>; presentation only, never shifts
    /// authored time or scoring. Non-raycast (never intercepts a tap). One pill pulses per event.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SectorPulseCue : MonoBehaviour
    {
        [SerializeField] AudioClock clock;
        [SerializeField] RectTransform left;
        [SerializeField] RectTransform right;
        [SerializeField] RectTransform up;
        [SerializeField] RectTransform down;

        [Header("Pulse shape")]
        [Tooltip("Scale of the pill at cue appearance (approach start).")]
        [SerializeField] float startScale = 0.45f;
        [Tooltip("Scale of the pill at the peak (event time).")]
        [SerializeField] float peakScale = 1.3f;
        [Tooltip("Curve shaping the build-up from appearance (0) to event (1).")]
        [SerializeField] AnimationCurve buildup = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [Tooltip("Seconds the pill lingers/relaxes after the event before clearing.")]
        [SerializeField] float releaseSeconds = 0.18f;

        [SerializeField] Color approachColor = new Color(0.95f, 0.55f, 0.2f, 0.55f);
        [SerializeField] Color hitColor = new Color(0.3f, 1f, 0.45f, 0.95f);

        RectTransform activePill;
        Graphic activeGraphic;
        double eventTime;
        double approachSeconds = 1.0;
        double hitWindowSeconds = 0.15;
        bool running;
        float releaseTimer;

        void OnEnable() => ResetCue();

        public void Show(BangDirection direction, double eventSongTime, double approachDuration, double hitWindow)
        {
            HideAllPills();
            activePill = PillFor(direction);
            activeGraphic = activePill != null ? activePill.GetComponent<Graphic>() : null;
            eventTime = eventSongTime;
            approachSeconds = System.Math.Max(0.05, approachDuration);
            hitWindowSeconds = System.Math.Max(0.02, hitWindow);
            running = true;
            releaseTimer = 0f;
            if (activePill != null) activePill.gameObject.SetActive(true);
            Apply();
        }

        public void Hide()
        {
            if (running) { running = false; releaseTimer = releaseSeconds; }
        }

        public void ResetCue()
        {
            running = false;
            releaseTimer = 0f;
            HideAllPills();
            activePill = null;
            activeGraphic = null;
        }

        void Update()
        {
            if (running && clock != null)
            {
                Apply();
            }
            else if (releaseTimer > 0f && activePill != null)
            {
                releaseTimer -= Time.deltaTime;
                var k = Mathf.Clamp01(releaseTimer / Mathf.Max(0.01f, releaseSeconds));
                SetPill(peakScale, hitColor, k);
                if (releaseTimer <= 0f && activePill != null) { activePill.gameObject.SetActive(false); activePill = null; }
            }
        }

        void Apply()
        {
            if (activePill == null || clock == null) return;   // destroyed/unset -> Unity == null

            var remaining = eventTime - clock.SongTime;                 // >0 before event
            var t = 1.0 - remaining / approachSeconds;                  // 0 at appearance, 1 at event
            var shaped = buildup.Evaluate(Mathf.Clamp01((float)t));
            var inHit = System.Math.Abs(remaining) <= hitWindowSeconds;

            var scale = Mathf.Lerp(startScale, peakScale, inHit ? 1f : shaped);
            SetPill(scale, inHit ? hitColor : approachColor, 1f);
        }

        void SetPill(float scale, Color color, float alphaMul)
        {
            if (activePill == null) return;   // pill destroyed (rebuild/retry) -> skip safely
            activePill.localScale = Vector3.one * scale;
            if (activeGraphic != null)
            {
                var c = color; c.a *= Mathf.Clamp01(alphaMul);
                activeGraphic.color = c;
            }
        }

        RectTransform PillFor(BangDirection d)
        {
            switch (d)
            {
                case BangDirection.Left: return left;
                case BangDirection.Right: return right;
                case BangDirection.Up: return up;
                default: return down;
            }
        }

        void HideAllPills()
        {
            if (left != null) left.gameObject.SetActive(false);
            if (right != null) right.gameObject.SetActive(false);
            if (up != null) up.gameObject.SetActive(false);
            if (down != null) down.gameObject.SetActive(false);
        }
    }
}
