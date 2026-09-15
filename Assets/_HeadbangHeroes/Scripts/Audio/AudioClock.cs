using UnityEngine;

namespace HeadbangHeroes.Audio
{
    public sealed class AudioClock : MonoBehaviour
    {
        [SerializeField] AudioSource source;
        [SerializeField, Min(0.05f)] double leadTime = 0.15;

        double dspStart;
        double songStartOffset;

        public double SongTime => songStartOffset + System.Math.Max(0d, AudioSettings.dspTime - dspStart);
        public bool IsScheduled { get; private set; }

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
        }

        public void Stop()
        {
            if (source != null) source.Stop();
            IsScheduled = false;
        }
    }
}
