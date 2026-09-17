using UnityEngine;

namespace HeadbangHeroes.Audio
{
    /// <summary>
    /// The single authoritative gameplay clock. Song-time is derived from AudioSettings.dspTime
    /// via a scheduled start anchor — never from Time.time, Time.deltaTime, frame count,
    /// coroutine/animation completion or cue state.
    ///
    /// All time math lives in the pure <see cref="SongTimeMap"/> (unit-testable without DSP);
    /// this MonoBehaviour only owns AudioSource scheduling and supplies real dsp timestamps.
    /// Calibration is owned here in ONE place and applied uniformly to the live clock and to
    /// <see cref="ToSongTime"/> (input timestamp conversion). It shifts alignment only; it never
    /// widens timing windows.
    /// </summary>
    public sealed class AudioClock : MonoBehaviour
    {
        [SerializeField] AudioSource source;
        [SerializeField, Min(0.05f)] double leadTime = 0.15;
        [Header("Latency compensation")]
        [Tooltip("Seed song-time with the device's measured audio-output latency so taps that are " +
                 "physically on the beat are not read as early (the sound reaches the player later " +
                 "than its DSP schedule). Manual [ ] calibration is applied on top of this.")]
        [SerializeField] bool autoCompensateOutputLatency = true;

        SongTimeMap map = SongTimeMap.Idle;

        public bool IsScheduled => map.Scheduled;
        public bool IsPaused => map.Paused;

        /// <summary>Calibration offset in seconds. Positive = treat the player as acting later.</summary>
        public double Calibration
        {
            get => map.Calibration;
            set => map.Calibration = value;
        }

        /// <summary>Measured audio-output latency currently compensated (seconds).</summary>
        public double OutputLatency => map.OutputLatency;

        /// <summary>
        /// Estimates the device audio-output latency from Unity's DSP buffer configuration
        /// (bufferLength * numBuffers / sampleRate). This is the dominant, measurable component of
        /// why a physically on-beat tap reads as early. Applied to the map as a constant run term.
        /// </summary>
        public void RefreshOutputLatency()
        {
            if (!autoCompensateOutputLatency) { map.OutputLatency = 0d; return; }
            AudioSettings.GetDSPBufferSize(out var bufferLength, out var numBuffers);
            var sr = AudioSettings.outputSampleRate;
            map.OutputLatency = sr > 0 ? (double)bufferLength * numBuffers / sr : 0d;
        }

        /// <summary>Authoritative song-time (seconds), calibration included.</summary>
        public double SongTime => map.SongTimeAt(AudioSettings.dspTime);

        /// <summary>
        /// Converts an arbitrary DSP timestamp into the same authoritative song-time domain
        /// (calibration included), so input is never compared against chart time in another domain.
        /// </summary>
        public double ToSongTime(double dspTimestamp) => map.SongTimeAt(dspTimestamp);

        /// <summary>The DSP "now"; input adapters capture this to timestamp a bang.</summary>
        public double DspNow => AudioSettings.dspTime;

        public void Play(AudioClip clip, double startSongTime = 0d)
        {
            if (source == null || clip == null) return;

            RefreshOutputLatency();   // measure once per run; compensates the DSP->speaker delay
            source.Stop();
            source.clip = clip;
            var offset = System.Math.Max(0d, System.Math.Min(startSongTime, clip.length - 0.01d));
            source.time = (float)offset;
            var anchor = AudioSettings.dspTime + leadTime;
            source.PlayScheduled(anchor);
            map.Schedule(anchor, offset);
        }

        /// <summary>Restart the current clip from <paramref name="startSongTime"/> with a fresh anchor.</summary>
        public void Restart(double startSongTime = 0d)
        {
            if (source == null || source.clip == null) return;
            Play(source.clip, startSongTime);
        }

        public void Pause()
        {
            if (!map.Scheduled || map.Paused || source == null) return;
            map.Pause(AudioSettings.dspTime);
            source.Pause();
        }

        public void Resume()
        {
            if (!map.Scheduled || !map.Paused || source == null) return;
            map.Resume(AudioSettings.dspTime);
            source.UnPause();
        }

        public void Stop()
        {
            if (source != null) source.Stop();
            map.Stop();
        }
    }
}
