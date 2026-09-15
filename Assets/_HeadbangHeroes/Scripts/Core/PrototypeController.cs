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
        [SerializeField] TextAsset chartJsonOverride;
        [SerializeField] AudioClock clock;
        [SerializeField] ChartScheduler scheduler;
        [SerializeField] HeadbangInput input;
        [SerializeField] HeadMotionModel head;
        [SerializeField] ClosingCircleCue cue;
        [SerializeField] bool startOnPlay = true;
        [SerializeField, Min(0f)] double startSongTime = 22.0;

        readonly ComboScore score = new();
        ChartDefinition runtimeChart;

        void Start()
        {
            if (startOnPlay) StartPrototype();
        }

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
            if (song == null || song.audio == null || scheduler == null || clock == null)
            {
                Debug.LogWarning("HH M0 cannot start: song/audio/scheduler/clock is missing.");
                return;
            }

            var chart = song.chart;
            if (chartJsonOverride != null)
            {
                runtimeChart = ChartJsonLoader.CreateRuntimeChart(chartJsonOverride);
                chart = runtimeChart;
            }

            if (chart == null)
            {
                Debug.LogWarning("HH M0 cannot start: no chart is assigned.");
                return;
            }

            score.Reset();
            scheduler.Configure(chart);
            scheduler.ResetScheduler();
            clock.Play(song.audio, startSongTime);
        }

        void OnCue(ChartEvent ev, double approachTime) => cue?.Show(ev.time, approachTime);

        void OnBang(float direction)
        {
            if (scheduler == null || !scheduler.HasActiveEvent) return;
            var activeEvent = scheduler.ActiveEvent;
            var dir = direction < 0 ? BangDirection.Left : BangDirection.Right;
            if (!scheduler.TryJudge(dir, out var result)) return;

            score.Apply(result.judgment);
            if (result.judgment != Judgment.Miss)
                head?.Bang(direction, activeEvent.intensity);
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
