using HeadbangHeroes.Charts;

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

        public BangInput(BangDirection direction, double songTime, double rawDeviceTime = 0d)
        {
            Direction = direction;
            SongTime = songTime;
            RawDeviceTime = rawDeviceTime;
        }
    }
}
