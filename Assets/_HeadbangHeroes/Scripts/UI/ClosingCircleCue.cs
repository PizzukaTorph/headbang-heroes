using HeadbangHeroes.Audio;
using HeadbangHeroes.Charts;
using UnityEngine;

namespace HeadbangHeroes.UI
{
    /// <summary>
    /// Closing-circle timing cue. The approach ring scale is computed every frame from the
    /// authoritative audio clock (AudioClock.SongTime) and the absolute event timestamp:
    ///
    ///   remaining = eventTime - songTime
    ///   progress  = 1 - remaining / approachTime   (clamped 0..1)
    ///
    /// At progress == 1 the ring coincides exactly with the target ring.
    /// The animation is NOT time-driven by coroutines or animation duration; only rendering
    /// happens in Update while the time source stays the DSP-backed clock.
    /// </summary>
    public sealed class ClosingCircleCue : MonoBehaviour
    {
        [SerializeField] AudioClock clock;
        [SerializeField] RectTransform approachRing;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] RectTransform hitMarker;   // small tick that shows where the last tap landed
        [SerializeField] float startScale = 2.4f;
        [SerializeField] Vector2 horizontalOffset = new Vector2(185f, 0f);
        [SerializeField] Vector2 verticalOffset = new Vector2(0f, 185f);
        [SerializeField] float markerMaxRadius = 90f;   // px offset from target for a full-window error
        [SerializeField] float markerHold = 0.45f;      // seconds the marker stays fully visible
        [SerializeField] float markerReferenceWindow = 0.16f; // error mapped onto this (the GOOD edge)

        double targetSongTime;
        double approachSeconds = 1.0;
        double hitWindowSeconds = 0.12;   // ring rests + brightens while |remaining| <= this
        bool running;

        // Hit-marker state: where the player's last tap landed relative to the target.
        float markerTimer;
        Vector2 markerBaseAnchored;
        Color markerBaseColor = Color.white;
        UnityEngine.UI.Graphic markerGraphic;

        [SerializeField] Color approachColor = new Color(0.9f, 0.2f, 0.2f, 1f);
        [SerializeField] Color hitColor = new Color(0.3f, 1f, 0.4f, 1f);

        public float Progress { get; private set; }
        public bool InHitWindow { get; private set; }

        void OnEnable() => ResetCue();

        public void Show(double eventSongTime, double approachDuration, BangDirection direction, double hitWindow)
        {
            targetSongTime = eventSongTime;
            PositionFor(direction);
            approachSeconds = System.Math.Max(0.01, approachDuration);
            hitWindowSeconds = System.Math.Max(0.01, hitWindow);
            running = true;
            if (approachRing != null) approachRing.gameObject.SetActive(true);
            SetVisible(true);
            Apply(EvaluateProgress());
        }

        void PositionFor(BangDirection direction)
        {
            var rect = transform as RectTransform;
            if (rect == null) return;

            switch (direction)
            {
                case BangDirection.Left: rect.anchoredPosition = -horizontalOffset; break;
                case BangDirection.Right: rect.anchoredPosition = horizontalOffset; break;
                case BangDirection.Up: rect.anchoredPosition = verticalOffset; break;
                case BangDirection.Down: rect.anchoredPosition = -verticalOffset; break;
            }
        }

        public void Hide()
        {
            running = false;
            // Keep the shared CanvasGroup visible while a hit marker is still fading, but hide the
            // ring itself. This lets the "where did my tap land" marker linger after the event
            // resolves, without a separate canvas.
            if (approachRing != null) approachRing.gameObject.SetActive(false);
            if (markerTimer <= 0f) SetVisible(false);
        }

        /// <summary>
        /// Show where the player's tap landed relative to the target: a blue ring offset from the
        /// centre in proportion to the signed timing error (early = below, late = above). Purely
        /// presentation — gives the player a mental map to self-calibrate. Does not affect judging.
        /// </summary>
        public void ShowHitMarker(double signedErrorSeconds)
        {
            if (hitMarker == null) return;

            var frac = Mathf.Clamp01((float)(System.Math.Abs(signedErrorSeconds) / System.Math.Max(0.001, markerReferenceWindow)));
            var dir = signedErrorSeconds >= 0d ? 1f : -1f;   // late = up, early = down
            markerBaseAnchored = new Vector2(0f, dir * frac * markerMaxRadius);
            hitMarker.anchoredPosition = markerBaseAnchored;

            if (markerGraphic == null) markerGraphic = hitMarker.GetComponent<UnityEngine.UI.Graphic>();
            markerBaseColor = markerGraphic != null ? markerGraphic.color : Color.white;
            markerBaseColor.a = 1f;
            if (markerGraphic != null) markerGraphic.color = markerBaseColor;
            markerTimer = markerHold;
            hitMarker.gameObject.SetActive(true);
        }

        /// <summary>Full reset for scene restart / re-arm: hide and clear state.</summary>
        public void ResetCue()
        {
            running = false;
            Progress = 0f;
            InHitWindow = false;
            markerTimer = 0f;
            if (hitMarker != null) hitMarker.gameObject.SetActive(false);
            SetVisible(false);
            if (approachRing != null)
            {
                approachRing.gameObject.SetActive(true);
                approachRing.localScale = Vector3.one * startScale;
                var graphic = approachRing.GetComponent<UnityEngine.UI.Graphic>();
                if (graphic != null) graphic.color = approachColor;
            }
        }

        void Update()
        {
            UpdateHitMarker();
            if (!running || clock == null) return;
            var remaining = targetSongTime - clock.SongTime;
            InHitWindow = System.Math.Abs(remaining) <= hitWindowSeconds;
            Apply(EvaluateProgress());
        }

        void UpdateHitMarker()
        {
            if (hitMarker == null || markerTimer <= 0f) return;
            markerTimer -= Time.deltaTime;
            if (markerGraphic != null)
            {
                var c = markerBaseColor;
                c.a = Mathf.Clamp01(markerTimer / Mathf.Max(0.01f, markerHold));
                markerGraphic.color = c;
            }
            if (markerTimer <= 0f)
            {
                hitMarker.gameObject.SetActive(false);
                if (!running) SetVisible(false);
            }
        }

        float EvaluateProgress()
        {
            var remaining = targetSongTime - clock.SongTime;
            var t = 1.0 - remaining / approachSeconds;
            return Mathf.Clamp01((float)t);
        }

        void Apply(float t)
        {
            Progress = t;
            if (approachRing == null) return;

            // While inside the hit window the ring rests exactly on the target (scale 1) and turns
            // to the hit colour, giving a clear "tap now" beat instead of a ring that vanishes at
            // the instant of the minimum. This is presentation feedback; it does not change judging.
            var scale = InHitWindow ? 1f : Mathf.Lerp(startScale, 1f, t);
            approachRing.localScale = Vector3.one * scale;

            var graphic = approachRing.GetComponent<UnityEngine.UI.Graphic>();
            if (graphic != null) graphic.color = InHitWindow ? hitColor : approachColor;
        }

        void SetVisible(bool visible)
        {
            if (canvasGroup != null) canvasGroup.alpha = visible ? 1f : 0f;
        }
    }
}
