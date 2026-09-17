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

        /// <summary>Emitted with the direction and the input's authoritative song-time.</summary>
        public event Action<BangInput> Bang;

        public void SetClock(AudioClock value) => clock = value;

        void Update()
        {
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.wasPressedThisFrame)
                    Emit(touch.position.ReadValue());
            }

#if UNITY_EDITOR
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                Emit(Mouse.current.position.ReadValue());
#endif
        }

        void Emit(Vector2 screenPosition)
        {
            var direction = ResolveZone(screenPosition, Screen.width, Screen.height);

            // Best available timestamp for a press this frame is the current DSP time; convert it
            // once into song-time so judgment compares like-for-like clocks.
            var dspNow = clock != null ? clock.DspNow : 0d;
            var songTime = clock != null ? clock.ToSongTime(dspNow) : 0d;

            Bang?.Invoke(new BangInput(direction, songTime, dspNow));
        }

        /// <summary>
        /// Resolves the nearest cardinal zone without requiring visible buttons.
        /// Normalizing by half-screen extents keeps the four wedges useful on portrait devices.
        /// </summary>
        public static BangDirection ResolveZone(Vector2 position, float width, float height)
        {
            var halfWidth = Mathf.Max(1f, width * 0.5f);
            var halfHeight = Mathf.Max(1f, height * 0.5f);
            var x = (position.x - halfWidth) / halfWidth;
            var y = (position.y - halfHeight) / halfHeight;

            if (Mathf.Abs(x) > Mathf.Abs(y))
                return x < 0f ? BangDirection.Left : BangDirection.Right;

            return y < 0f ? BangDirection.Down : BangDirection.Up;
        }
    }
}
