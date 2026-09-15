using UnityEngine;

namespace HeadbangHeroes.Audio
{
    /// <summary>
    /// Authoritative rhythm clock. Time is derived from AudioSettings.dspTime, never from
    /// frame count, coroutines or animation state. Supports scheduled start, pause and resume.
    /// While paused, SongTime is frozen at the moment of pause.
    /// </summary>
    public sealed class AudioClock : MonoBehaviour
    {
        [SerializeField] AudioSource source;
        [SerializeField, Min(0.05f)] double leadTime = 0.15;

        double dspStart;          // dspTime at which SongTime == songStartOffset
        double songStartOffset;   // song position (seconds) mapped to dspStart
        double pausedSongTime;     // frozen SongTime while paused

        public bool IsScheduled { get; private set; }
        public bool IsPaused { get; private set; }

        public double SongTime
        {
            get
            {
                if (!IsScheduled) return 0d;
                if (IsPaused) return pausedSongTime;
                return songStartOffset + System.Math.Max(0d, AudioSettings.dspTime - dspStart);
            }
        }

        public void Play(AudioClip clip, double startSongTime = 0d)
        {
            if (source == null || clip == null) return;

            source.Stop();
            source.clip = clip;
            songStartOffset = System.Math.Max(0d, System.Math.Min(startSongTime, clip.length - 0.01d));
            source.time = (float)songStartOffset;
            dspStart = AudioSettings.dspTime + leadTime;
            source.PlayScheduled(dspStart);
            IsScheduled = true;
            IsPaused = false;
        }

        public void Pause()
        {
            if (!IsScheduled || IsPaused || source == null) return;
            pausedSongTime = SongTime;
            source.Pause();
            IsPaused = true;
        }

        public void Resume()
        {
            if (!IsScheduled || !IsPaused || source == null) return;
            // Re-anchor dspStart so SongTime continues from where it was frozen.
            dspStart = AudioSettings.dspTime;
            songStartOffset = pausedSongTime;
            source.UnPause();
            IsPaused = false;
        }

        public void Stop()
        {
            if (source != null) source.Stop();
            IsScheduled = false;
            IsPaused = false;
        }
    }
}
