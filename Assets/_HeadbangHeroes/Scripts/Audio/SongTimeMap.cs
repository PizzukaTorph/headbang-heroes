namespace HeadbangHeroes.Audio
{
    /// <summary>
    /// Pure, deterministic mapping between DSP time and authoritative song-time. Extracted from
    /// <see cref="AudioClock"/> so the clock lifecycle math (anchor / offset / calibration / pause /
    /// resume / restart) can be unit-tested without an AudioSource or real <c>AudioSettings.dspTime</c>.
    ///
    ///   songTime(dsp) = songStartOffset + max(0, dsp - anchorDsp) + calibration
    ///
    /// Calibration is applied here in exactly one place. Pause freezes song-time; Resume re-anchors
    /// to a supplied "current dsp" so playback continues from the frozen value with no drift.
    /// </summary>
    public struct SongTimeMap
    {
        public double AnchorDsp;
        public double SongStartOffset;
        public double Calibration;
        public bool Scheduled;
        public bool Paused;
        public double PausedSongTime;

        public static SongTimeMap Idle => default;

        /// <summary>Anchors a fresh start: songTime becomes <paramref name="startSongTime"/> at <paramref name="anchorDsp"/>.</summary>
        public void Schedule(double anchorDsp, double startSongTime)
        {
            AnchorDsp = anchorDsp;
            SongStartOffset = startSongTime < 0d ? 0d : startSongTime;
            Scheduled = true;
            Paused = false;
        }

        public double SongTimeAt(double dsp)
        {
            if (!Scheduled) return 0d;
            if (Paused) return PausedSongTime;
            var delta = dsp - AnchorDsp;
            if (delta < 0d) delta = 0d;
            return SongStartOffset + delta + Calibration;
        }

        public void Pause(double dsp)
        {
            if (!Scheduled || Paused) return;
            PausedSongTime = SongTimeAt(dsp);
            Paused = true;
        }

        public void Resume(double dsp)
        {
            if (!Scheduled || !Paused) return;
            // Re-anchor so the frozen song-time continues seamlessly. PausedSongTime already
            // includes calibration, so remove it from the offset to avoid double-counting.
            AnchorDsp = dsp;
            SongStartOffset = PausedSongTime - Calibration;
            Paused = false;
        }

        public void Stop()
        {
            Scheduled = false;
            Paused = false;
        }
    }
}
