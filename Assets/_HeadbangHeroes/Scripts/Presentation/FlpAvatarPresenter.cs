using HeadbangHeroes.Input;
using UnityEngine;
using UnityEngine.InputSystem;

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
        enum PlaybackState
        {
            Idle,
            Headbang,
            Horns
        }

        [SerializeField] SpriteRenderer target;
        [SerializeField] Sprite idleFrame;
        [SerializeField] Sprite[] headbangFrames;
        [SerializeField] Sprite[] hornsFrames;
        [SerializeField, Min(1f)] float frameRate = 12f;
        [SerializeField] HeadbangInput input;
        [SerializeField] bool listenToInput = true;
        [SerializeField] bool enableDebugHorns = true;

        float elapsed;
        int frameIndex;
        PlaybackState state;

        public bool IsPlaying => state != PlaybackState.Idle;
        public string CurrentState => state.ToString();

        void Awake()
        {
            if (target == null) target = GetComponent<SpriteRenderer>();
            PlayIdle();
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
            if (enableDebugHorns && Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
                PlayHorns();

            if (!IsPlaying || target == null) return;

            elapsed += Time.deltaTime;
            var frames = state == PlaybackState.Horns ? hornsFrames : headbangFrames;
            if (frames == null || frames.Length == 0)
            {
                PlayIdle();
                return;
            }

            var nextFrame = Mathf.FloorToInt(elapsed * Mathf.Max(1f, frameRate));
            if (nextFrame >= frames.Length)
            {
                PlayIdle();
                return;
            }

            if (nextFrame != frameIndex)
            {
                frameIndex = nextFrame;
                target.sprite = frames[frameIndex];
            }
        }

        void OnBang(HeadbangHeroes.Gameplay.Timing.BangInput _)
        {
            TriggerHeadbang();
        }

        /// <summary>Restarts the visual sequence from its neutral frame.</summary>
        public void TriggerHeadbang()
        {
            PlayHeadbang();
        }

        /// <summary>Shows the neutral frame and stops any one-shot presentation.</summary>
        public void PlayIdle()
        {
            elapsed = 0f;
            frameIndex = 0;
            state = PlaybackState.Idle;
            if (target != null) target.sprite = idleFrame != null ? idleFrame : FirstFrame(headbangFrames);
        }

        /// <summary>Restarts the headbang one-shot from frame zero.</summary>
        public void PlayHeadbang()
        {
            PlayOneShot(PlaybackState.Headbang, headbangFrames);
        }

        /// <summary>Restarts the horns one-shot from frame zero.</summary>
        public void PlayHorns()
        {
            PlayOneShot(PlaybackState.Horns, hornsFrames);
        }

        void PlayOneShot(PlaybackState nextState, Sprite[] frames)
        {
            if (target == null || frames == null || frames.Length == 0) return;
            elapsed = 0f;
            frameIndex = 0;
            state = nextState;
            target.sprite = frames[0];
        }

        static Sprite FirstFrame(Sprite[] frames)
        {
            return frames != null && frames.Length > 0 ? frames[0] : null;
        }
    }
}
