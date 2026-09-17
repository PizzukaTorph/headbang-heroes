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
        [SerializeField] float startScale = 2.4f;
        [SerializeField] Vector2 horizontalOffset = new Vector2(185f, 0f);
        [SerializeField] Vector2 verticalOffset = new Vector2(0f, 185f);

        double targetSongTime;
        double approachSeconds = 1.0;
        double hitWindowSeconds = 0.12;   // ring rests + brightens while |remaining| <= this
        bool running;

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
            SetVisible(false);
        }

        /// <summary>Full reset for scene restart / re-arm: hide and clear state.</summary>
        public void ResetCue()
        {
            running = false;
            Progress = 0f;
            InHitWindow = false;
            SetVisible(false);
            if (approachRing != null)
            {
                approachRing.localScale = Vector3.one * startScale;
                var graphic = approachRing.GetComponent<UnityEngine.UI.Graphic>();
                if (graphic != null) graphic.color = approachColor;
            }
        }

        void Update()
        {
            if (!running || clock == null) return;
            var remaining = targetSongTime - clock.SongTime;
            InHitWindow = System.Math.Abs(remaining) <= hitWindowSeconds;
            Apply(EvaluateProgress());
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
