using HeadbangHeroes.Input;
using UnityEngine;

namespace HeadbangHeroes.Presentation
{
    /// <summary>
    /// Minimal presentation-only FLP frame player. It observes the existing semantic input event
    /// and restarts one Erik headbang sequence; it never participates in chart resolution, timing,
    /// scoring, or neck simulation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FlpAvatarPresenter : MonoBehaviour
    {
        [SerializeField] SpriteRenderer target;
        [SerializeField] Sprite[] headbangFrames;
        [SerializeField, Min(1f)] float frameRate = 12f;
        [SerializeField] HeadbangInput input;
        [SerializeField] bool listenToInput = true;

        float elapsed;
        int frameIndex;
        bool playing;

        public bool IsPlaying => playing;

        void Awake()
        {
            if (target == null) target = GetComponent<SpriteRenderer>();
            ShowNeutral();
        }

        void OnEnable()
        {
            if (listenToInput && input != null) input.Bang += OnBang;
        }

        void OnDisable()
        {
            if (input != null) input.Bang -= OnBang;
        }

        void Update()
        {
            if (!playing || target == null || headbangFrames == null || headbangFrames.Length == 0) return;

            elapsed += Time.deltaTime;
            var nextFrame = Mathf.FloorToInt(elapsed * Mathf.Max(1f, frameRate));
            if (nextFrame >= headbangFrames.Length)
            {
                playing = false;
                ShowNeutral();
                return;
            }

            if (nextFrame != frameIndex)
            {
                frameIndex = nextFrame;
                target.sprite = headbangFrames[frameIndex];
            }
        }

        void OnBang(HeadbangHeroes.Gameplay.Timing.BangInput _)
        {
            TriggerHeadbang();
        }

        /// <summary>Restarts the visual sequence from its neutral frame.</summary>
        public void TriggerHeadbang()
        {
            if (target == null || headbangFrames == null || headbangFrames.Length == 0) return;
            elapsed = 0f;
            frameIndex = 0;
            playing = true;
            target.sprite = headbangFrames[0];
        }

        /// <summary>Stops playback and leaves the character on the neutral frame.</summary>
        public void ShowNeutral()
        {
            elapsed = 0f;
            frameIndex = 0;
            playing = false;
            if (target != null && headbangFrames != null && headbangFrames.Length > 0)
                target.sprite = headbangFrames[0];
        }
    }
}
