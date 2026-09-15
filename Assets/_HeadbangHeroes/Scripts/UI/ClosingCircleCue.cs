using HeadbangHeroes.Audio;
using UnityEngine;

namespace HeadbangHeroes.UI
{
    public sealed class ClosingCircleCue : MonoBehaviour
    {
        [SerializeField] AudioClock clock;
        [SerializeField] RectTransform approachRing;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] float startScale = 2.4f;

        double targetSongTime;
        double approachSeconds;
        bool running;

        public void Show(double eventSongTime, double approachDuration)
        {
            targetSongTime = eventSongTime;
            approachSeconds = System.Math.Max(0.01, approachDuration);
            running = true;
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            Apply(0f);
        }

        public void Hide()
        {
            running = false;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        void Update()
        {
            if (!running || clock == null) return;
            var remaining = targetSongTime - clock.SongTime;
            var t = 1.0 - remaining / approachSeconds;
            Apply(Mathf.Clamp01((float)t));
        }

        void Apply(float t)
        {
            if (approachRing == null) return;
            var scale = Mathf.Lerp(startScale, 1f, t);
            approachRing.localScale = Vector3.one * scale;
        }
    }
}
