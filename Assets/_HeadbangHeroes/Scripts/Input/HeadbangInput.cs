using System;
using HeadbangHeroes.Audio;
using HeadbangHeroes.Charts;
using HeadbangHeroes.Gameplay.Timing;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HeadbangHeroes.Input
{
    /// <summary>
    /// Four large invisible tap zones around screen centre. The active chart cue tells the
    /// player which zone matters; input itself stays spatially forgiving.
    ///
    /// This adapter is responsible for putting input into the authoritative time domain: it
    /// captures the best-available DSP timestamp for the press and converts it via
    /// <see cref="AudioClock.ToSongTime"/> before emitting a semantic <see cref="BangInput"/>.
    /// Gameplay resolution therefore never sees a raw Input System timestamp.
    /// </summary>
    public sealed class HeadbangInput : MonoBehaviour
    {
        [SerializeField] AudioClock clock;
        [Tooltip("WASD keyboard bangs (A=Left, D=Right, W=Up, S=Down) alongside mouse/touch, for desktop playtesting.")]
        [SerializeField] bool enableKeyboard = true;

        /// <summary>Emitted with the direction and the input's authoritative song-time.</summary>
        public event Action<BangInput> Bang;

        public void SetClock(AudioClock value) => clock = value;

        void Update()
        {
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.wasPressedThisFrame && !IsPointerOverUi(touch.touchId.ReadValue()))
                    Emit(touch.position.ReadValue(), SourceTouch);
            }

#if UNITY_EDITOR
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUi(-1))
                Emit(Mouse.current.position.ReadValue(), SourceMouse);
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (enableKeyboard) ReadKeyboard();   // dev convenience only; not in shipping builds
#endif
        }

        // Diagnostic source codes mirrored in HeadbangHeroes.Diagnostics.InputSource.
        const int SourceKeyboard = 0, SourceMouse = 1, SourceTouch = 2;

        /// <summary>
        /// True when the press landed on an interactive UI element (a button, THE BANG, an overlay).
        /// Such taps must NOT also emit a bang — the UI "eats" them. This keeps the whole screen a
        /// bang zone while making on-screen controls safe (Option A UX).
        /// </summary>
        static bool IsPointerOverUi(int pointerOrTouchId)
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es == null) return false;
#if UNITY_EDITOR
            if (pointerOrTouchId < 0) return es.IsPointerOverGameObject();   // mouse
#endif
            return es.IsPointerOverGameObject(pointerOrTouchId);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        void ReadKeyboard()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            // A/D = horizontal inversion, W/S = vertical. Same semantics as the tap wedges.
            if (kb.aKey.wasPressedThisFrame) EmitDirection(BangDirection.Left, SourceKeyboard, NoPos);
            if (kb.dKey.wasPressedThisFrame) EmitDirection(BangDirection.Right, SourceKeyboard, NoPos);
            if (kb.wKey.wasPressedThisFrame) EmitDirection(BangDirection.Up, SourceKeyboard, NoPos);
            if (kb.sKey.wasPressedThisFrame) EmitDirection(BangDirection.Down, SourceKeyboard, NoPos);
        }
#endif

        static readonly Vector2 NoPos = new Vector2(float.NaN, float.NaN);

        void Emit(Vector2 screenPosition, int source)
            => EmitDirection(ResolveZone(screenPosition, Screen.width, Screen.height), source, screenPosition);

        void EmitDirection(BangDirection direction, int source, Vector2 screenPosition)
        {
            // Best available timestamp for a press this frame is the current DSP time; convert it
            // once into song-time so judgment compares like-for-like clocks. Source + screen position
            // are carried for diagnostics only (never used by matching/judgment).
            var dspNow = clock != null ? clock.DspNow : 0d;
            var songTime = clock != null ? clock.ToSongTime(dspNow) : 0d;

            Bang?.Invoke(new BangInput(direction, songTime, dspNow, source, screenPosition));
        }

        /// <summary>
        /// Resolves the nearest cardinal zone from a screen position, using EQUAL-PIXEL geometry:
        /// the L/R vs U/D split is the true 45° diagonal in pixels from screen centre (|dx| vs |dy|).
        /// v0.0.3 fix (diagnostics): the old version normalized x and y by half-width/half-height
        /// INDEPENDENTLY, which on a tall portrait screen skewed the diagonal toward Left/Right and
        /// made taps high-but-slightly-sideways resolve L/R instead of Up/Down (a WRONG_DIRECTION
        /// source). Comparing raw pixel offsets makes the four wedges symmetric on any aspect ratio.
        /// </summary>
        public static BangDirection ResolveZone(Vector2 position, float width, float height)
        {
            var dx = position.x - width * 0.5f;
            var dy = position.y - height * 0.5f;

            if (Mathf.Abs(dx) > Mathf.Abs(dy))
                return dx < 0f ? BangDirection.Left : BangDirection.Right;

            return dy < 0f ? BangDirection.Down : BangDirection.Up;
        }
    }
}
