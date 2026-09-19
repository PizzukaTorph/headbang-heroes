using HeadbangHeroes.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace HeadbangHeroes.Presentation
{
    /// <summary>
    /// Feel polish (v0.0.2 P12). PRESENTATION ONLY, capped, and trivially disabled:
    ///  - MICRO SCREEN-SHAKE: a bang injects trauma (scaled by intensity); a small capped jitter is
    ///    applied to a shake-root transform. Suppressed by ReducedShake.
    ///  - MOTION TRAIL: faint fading ghosts spawned behind the head at high neck speed. Suppressed by
    ///    ReducedFlash (reduced-effects).
    /// It reads the neck (speed) and receives bang pulses + accessibility from the controller; it
    /// NEVER touches gameplay/scoring/timing. The shake-root is a presentation container that does
    /// not include the protected cue/head readability elements' logic (offset is tiny and capped).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FeedbackFxPresenter : MonoBehaviour
    {
        [SerializeField] NeckMotionModel neck;      // read-only source
        [SerializeField] RectTransform shakeRoot;    // moved by the shake (a presentation container)
        [SerializeField] RectTransform head;         // trail ghosts spawn at the head position
        [SerializeField] RectTransform trailParent;   // where ghost images live (behind the head)

        [Header("Screen shake")]
        [SerializeField, Min(0f)] float maxShakePx = 16f;
        [SerializeField, Min(0.1f)] float traumaDecay = 3.5f;
        [SerializeField, Range(0f, 1f)] float bangTrauma = 0.5f;

        [Header("Motion trail")]
        [SerializeField, Min(0f)] float trailSpeedThreshold = 220f;  // deg/s of neck to start trailing
        [SerializeField, Min(0f)] float trailInterval = 0.04f;        // seconds between ghosts
        [SerializeField, Min(0f)] float ghostLifetime = 0.35f;
        [SerializeField, Range(0f, 1f)] float ghostAlpha = 0.25f;
        [SerializeField] int ghostPoolSize = 8;

        ShakeModel shake;
        Vector2 shakeHome;
        bool reducedShake, reducedFlash;

        // trail ghost pool
        RectTransform[] ghosts;
        Image[] ghostImgs;
        float[] ghostTimer;
        int ghostCursor;
        float trailAccum;

        public void SetSource(NeckMotionModel value) => neck = value;

        public void ApplySettings(in AccessibilitySettings settings)
        {
            reducedShake = settings.ReducedShake;
            reducedFlash = settings.ReducedFlash;
        }

        /// <summary>Called by the controller on a scored bang; intensity 0..1 scales the shake.</summary>
        public void OnBang(float intensity01)
        {
            if (reducedShake) return;
            shake?.AddTrauma(bangTrauma * Mathf.Clamp01(intensity01));
        }

        void Awake()
        {
            shake = new ShakeModel(traumaDecay, maxShakePx);
            if (shakeRoot != null) shakeHome = shakeRoot.anchoredPosition;
            BuildGhostPool();
        }

        public void ResetPresentation()
        {
            shake?.Reset();
            if (shakeRoot != null) shakeRoot.anchoredPosition = shakeHome;
            if (ghosts != null)
                for (var i = 0; i < ghosts.Length; i++)
                {
                    ghostTimer[i] = 0f;
                    if (ghosts[i] != null) ghosts[i].gameObject.SetActive(false);
                }
        }

        void LateUpdate()
        {
            var dt = Time.deltaTime;
            if (dt <= 0f) return;

            UpdateShake(dt);
            UpdateTrail(dt);
        }

        void UpdateShake(float dt)
        {
            if (shake == null || shakeRoot == null) return;
            var mag = shake.Advance(dt);
            if (reducedShake || mag <= 0.01f) { shakeRoot.anchoredPosition = shakeHome; return; }
            // Random jitter within a capped radius; presentation only.
            var jitter = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * mag;
            shakeRoot.anchoredPosition = shakeHome + jitter;
        }

        void UpdateTrail(float dt)
        {
            // Fade any live ghosts.
            if (ghosts != null)
                for (var i = 0; i < ghosts.Length; i++)
                {
                    if (ghostTimer[i] <= 0f) continue;
                    ghostTimer[i] -= dt;
                    var k = Mathf.Clamp01(ghostTimer[i] / Mathf.Max(0.01f, ghostLifetime));
                    if (ghostImgs[i] != null)
                    {
                        var c = ghostImgs[i].color; c.a = ghostAlpha * k; ghostImgs[i].color = c;
                    }
                    if (ghostTimer[i] <= 0f && ghosts[i] != null) ghosts[i].gameObject.SetActive(false);
                }

            if (reducedFlash || neck == null || head == null || ghosts == null) return;

            var speed = Mathf.Abs(neck.HorizontalVelocity) + Mathf.Abs(neck.VerticalVelocity);
            if (speed < trailSpeedThreshold) { trailAccum = 0f; return; }

            trailAccum += dt;
            if (trailAccum < trailInterval) return;
            trailAccum = 0f;
            SpawnGhost();
        }

        void SpawnGhost()
        {
            var idx = ghostCursor;
            ghostCursor = (ghostCursor + 1) % ghosts.Length;
            var g = ghosts[idx];
            var img = ghostImgs[idx];
            if (g == null) return;
            g.gameObject.SetActive(true);
            g.position = head.position;
            g.rotation = head.rotation;
            g.localScale = head.localScale;
            if (img != null) { var c = img.color; c.a = ghostAlpha; img.color = c; }
            ghostTimer[idx] = ghostLifetime;
        }

        void BuildGhostPool()
        {
            if (trailParent == null || head == null) return;
            var n = Mathf.Max(1, ghostPoolSize);
            ghosts = new RectTransform[n];
            ghostImgs = new Image[n];
            ghostTimer = new float[n];
            var headImg = head.GetComponent<Image>();
            for (var i = 0; i < n; i++)
            {
                var go = new GameObject($"Ghost{i}", typeof(RectTransform), typeof(Image));
                var rt = go.GetComponent<RectTransform>();
                rt.SetParent(trailParent, false);
                rt.sizeDelta = head.rect.size;
                var img = go.GetComponent<Image>();
                img.raycastTarget = false;
                if (headImg != null) img.sprite = headImg.sprite;
                var col = headImg != null ? headImg.color : Color.white; col.a = 0f;
                img.color = col;
                go.SetActive(false);
                ghosts[i] = rt; ghostImgs[i] = img; ghostTimer[i] = 0f;
            }
        }
    }
}
