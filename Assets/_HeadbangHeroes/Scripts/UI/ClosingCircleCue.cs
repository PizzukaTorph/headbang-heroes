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
        bool running;

        public float Progress { get; private set; }

        void OnEnable() => ResetCue();

        public void Show(double eventSongTime, double approachDuration, BangDirection direction)
        {
            targetSongTime = eventSongTime;
            PositionFor(direction);
            approachSeconds = System.Math.Max(0.01, approachDuration);
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
            SetVisible(false);
            if (approachRing != null) approachRing.localScale = Vector3.one * startScale;
        }

        void Update()
        {
            if (!running || clock == null) return;
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
            var scale = Mathf.Lerp(startScale, 1f, t);
            approachRing.localScale = Vector3.one * scale;
        }

        void SetVisible(bool visible)
        {
            if (canvasGroup != null) canvasGroup.alpha = visible ? 1f : 0f;
        }
    }
}
