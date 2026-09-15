using UnityEngine;

namespace HeadbangHeroes.UI
{
    public sealed class ClosingCircleCue : MonoBehaviour
    {
        [SerializeField] RectTransform approachRing;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] float startScale = 2.4f;

        double duration;
        double elapsed;
        bool running;

        public void Show(double approachSeconds)
        {
            duration = System.Math.Max(0.01, approachSeconds);
            elapsed = 0;
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
            if (!running) return;
            elapsed += Time.unscaledDeltaTime;
            Apply(Mathf.Clamp01((float)(elapsed / duration)));
        }

        void Apply(float t)
        {
            if (approachRing == null) return;
            var scale = Mathf.Lerp(startScale, 1f, t);
            approachRing.localScale = Vector3.one * scale;
        }
    }
}
