using HeadbangHeroes.Audio;
using HeadbangHeroes.Charts;
using HeadbangHeroes.Gameplay;
using HeadbangHeroes.Input;
using HeadbangHeroes.UI;
using UnityEngine;

namespace HeadbangHeroes.Core
{
    public sealed class PrototypeController : MonoBehaviour
    {
        [SerializeField] SongDefinition song;
        [SerializeField] AudioClock clock;
        [SerializeField] ChartScheduler scheduler;
        [SerializeField] HeadbangInput input;
        [SerializeField] HeadMotionModel head;
        [SerializeField] ClosingCircleCue cue;

        readonly ComboScore score = new();

        void OnEnable()
        {
            if (input != null) input.Bang += OnBang;
            if (scheduler != null)
            {
                scheduler.CueActivated += OnCue;
                scheduler.EventMissed += OnMiss;
            }
        }

        void OnDisable()
        {
            if (input != null) input.Bang -= OnBang;
            if (scheduler != null)
            {
                scheduler.CueActivated -= OnCue;
                scheduler.EventMissed -= OnMiss;
            }
        }

        public void StartPrototype()
        {
            if (song == null || song.audio == null || song.chart == null) return;
            score.Reset();
            scheduler.Configure(song.chart);
            scheduler.ResetScheduler();
            clock.Play(song.audio);
        }

        void OnCue(ChartEvent ev, double approachTime) => cue?.Show(approachTime);

        void OnBang(float direction)
        {
            var dir = direction < 0 ? BangDirection.Left : BangDirection.Right;
            if (!scheduler.TryJudge(dir, out var result)) return;

            score.Apply(result.judgment);
            if (result.judgment != Judgment.Miss)
                head?.Bang(direction, 1f);
            cue?.Hide();

            Debug.Log($"{result.judgment} {result.error * 1000.0:+0;-0;0} ms | combo {score.Combo} | score {score.Score}");
        }

        void OnMiss(ChartEvent ev)
        {
            score.Apply(Judgment.Miss);
            cue?.Hide();
        }
    }
}
