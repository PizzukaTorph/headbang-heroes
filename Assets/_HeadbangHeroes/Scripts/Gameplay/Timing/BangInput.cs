using HeadbangHeroes.Charts;
using UnityEngine;

namespace HeadbangHeroes.Gameplay.Timing
{
    /// <summary>
    /// A semantic bang expressed in the authoritative song-time domain. The raw device timestamp
    /// has already been converted to song-time by the time a BangInput exists, so gameplay
    /// resolution never compares an Input System clock against chart time.
    /// </summary>
    public readonly struct BangInput
    {
        /// <summary>Semantic inversion direction the player requested.</summary>
        public readonly BangDirection Direction;

        /// <summary>Authoritative song-time (seconds) at which the input occurred (calibration applied).</summary>
        public readonly double SongTime;

        /// <summary>Optional raw device timestamp kept for diagnostics only; never used for judgment.</summary>
        public readonly double RawDeviceTime;

        // ---- Diagnostic-only metadata (like RawDeviceTime): NEVER used by matching/judgment. ----
        /// <summary>0 = keyboard, 1 = mouse, 2 = touch, 3 = unknown (diagnostics/A-B only).</summary>
        public readonly int SourceCode;
        /// <summary>Screen position of the tap in pixels, when spatial (mouse/touch). NaN when N/A (keyboard).</summary>
        public readonly Vector2 ScreenPosition;
        public bool HasScreenPosition => !float.IsNaN(ScreenPosition.x);

        public BangInput(BangDirection direction, double songTime, double rawDeviceTime = 0d,
            int sourceCode = 3, Vector2 screenPosition = default)
        {
            Direction = direction;
            SongTime = songTime;
            RawDeviceTime = rawDeviceTime;
            SourceCode = sourceCode;
            // default(Vector2) is (0,0) which is a valid screen coord; use NaN sentinel for "no position".
            ScreenPosition = screenPosition == default ? new Vector2(float.NaN, float.NaN) : screenPosition;
        }
    }
}
