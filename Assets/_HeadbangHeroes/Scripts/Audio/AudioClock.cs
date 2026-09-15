using UnityEngine;

namespace HeadbangHeroes.Audio
{
    public sealed class AudioClock : MonoBehaviour
    {
        [SerializeField] AudioSource source;
        [SerializeField, Min(0.05f)] double leadTime = 0.15;

        double dspStart;
        public double SongTime => System.Math.Max(0d, AudioSettings.dspTime - dspStart);
        public bool IsScheduled { get; private set; }

        public void Play(AudioClip clip)
        {
            if (source == null || clip == null) return;
            source.Stop();
            source.clip = clip;
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
