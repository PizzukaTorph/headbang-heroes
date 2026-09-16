using HeadbangHeroes.Audio;
using HeadbangHeroes.Charts;
using HeadbangHeroes.Gameplay;
using HeadbangHeroes.Input;
using HeadbangHeroes.UI;
using UnityEngine;
using UnityEngine.InputSystem;

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
        [SerializeField] PrototypeHud hud;
        [SerializeField] bool startOnPlay = true;
        [SerializeField, Min(0f)] double startSongTime = 22.0;

        [Header("Playtest controls (Editor / Development builds)")]
        [SerializeField] bool enablePlaytestControls = true;
        [SerializeField] double calibrationStepMs = 5.0;

        readonly ComboScore score = new();
        ChartDefinition runtimeChart;
        double calibrationOffset;   // seconds, applied to judgment time only

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
                var data = ChartJsonLoader.Parse(chartJsonOverride);
                runtimeChart = ChartJsonLoader.CreateRuntimeChart(data);
                chart = runtimeChart;

                // Honor the chart's authored approach time if present.
                scheduler.ConfigureApproachTime(data.approachTime);
            }

            if (chart == null || chart.events == null || chart.events.Count == 0)
            {
                Debug.LogWarning("HH M0 cannot start: chart is missing or has no events.");
                return;
            }

            score.Reset();
            hud?.ResetHud();
            hud?.BindSources(clock, scheduler, head);
            hud?.SetCalibrationOffset(calibrationOffset);
            head?.ResetMotion();
            cue?.ResetCue();
            scheduler.Configure(chart);
            scheduler.ResetScheduler();
            clock.Play(song.audio, startSongTime);
        }

        void Update()
        {
            if (enablePlaytestControls) HandlePlaytestControls();
        }

        void HandlePlaytestControls()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.rKey.wasPressedThisFrame)
            {
                StartPrototype();
                return;
            }

            if (kb.spaceKey.wasPressedThisFrame && clock != null)
            {
                if (clock.IsPaused) clock.Resume();
                else clock.Pause();
            }

            if (kb.leftBracketKey.wasPressedThisFrame)
                AdjustCalibration(-calibrationStepMs / 1000.0);

            if (kb.rightBracketKey.wasPressedThisFrame)
                AdjustCalibration(calibrationStepMs / 1000.0);
        }

        void AdjustCalibration(double deltaSeconds)
        {
            calibrationOffset += deltaSeconds;
            hud?.SetCalibrationOffset(calibrationOffset);
            Debug.Log($"HH M0 calibration offset: {calibrationOffset * 1000.0:+0;-0;0} ms");
        }

        void OnCue(ChartEvent ev, double approachTime) => cue?.Show(ev.time, approachTime);

        void OnBang(BangDirection direction)
        {
            if (scheduler == null || !scheduler.HasActiveEvent) return;

            var dir = direction;
            var eventIntensity = scheduler.ActiveEvent.intensity;

            // Sample motion quality from the head's momentum leading into the beat,
            // before we apply the new impulse from this tap.
            var motionQuality = head != null ? head.SampleMotionQuality(dir) : 1f;

            if (!scheduler.TryJudge(dir, motionQuality, calibrationOffset, out var result))
                return; // early out-of-window tap: event left intact, nothing consumed.

            score.Apply(result);
            if (result.judgment != Judgment.Miss)
                head?.Bang(dir, eventIntensity);
            else
                head?.Bang(dir, 0.35f); // wrong taps still invert/launch physically

            cue?.Hide();
            hud?.Show(result, score.Combo, score.Score);

            Debug.Log($"{result.judgment} {result.error * 1000.0:+0;-0;0} ms | motion {result.motionQuality:0.00} | perf {result.Performance:0.00} | combo {score.Combo} | score {score.Score}");
        }

        void OnMiss(ChartEvent ev)
        {
            score.Apply(new JudgmentResult(Judgment.Miss, JudgmentSystem.Good + 0.001, 0f));
            cue?.Hide();
            hud?.ShowMiss(score.Combo, score.Score);
            Debug.Log($"MISS (expired) at {ev.time:0.000}s | combo {score.Combo} | score {score.Score}");
        }
    }
}
