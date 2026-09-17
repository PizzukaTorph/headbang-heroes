using HeadbangHeroes.Audio;
using HeadbangHeroes.Charts;
using HeadbangHeroes.Charts.Runtime;
using HeadbangHeroes.Gameplay;
using HeadbangHeroes.Gameplay.Neck;
using HeadbangHeroes.Gameplay.Scoring;
using HeadbangHeroes.Gameplay.Timing;
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
        [SerializeField] NeckMotionModel head;
        [SerializeField] ClosingCircleCue cue;
        [SerializeField] PrototypeHud hud;
        [SerializeField] bool startOnPlay = true;
        [SerializeField, Min(0f)] double startSongTime = 22.0;

        [Header("Playtest controls (Editor / Development builds)")]
        [SerializeField] bool enablePlaytestControls = true;
        [SerializeField] double calibrationStepMs = 5.0;

        readonly RunScorer scorer = new(ScoringConfig.Default, HypeConfig.Default);
        readonly MotionQualityConfig motionConfig = MotionQualityConfig.Default;
        RuntimeChart runtimeChart;

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

            if (chartJsonOverride == null)
            {
                Debug.LogWarning("HH M0 cannot start: no chart JSON assigned.");
                return;
            }

            ChartJsonData data;
            try
            {
                data = ChartJsonLoader.Parse(chartJsonOverride);
                runtimeChart = ChartCompiler.Compile(data);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"HH M0 cannot start: chart failed to compile. {e.Message}");
                return;
            }

            if (runtimeChart.MotionCount == 0)
            {
                Debug.LogWarning("HH M0 cannot start: compiled chart has no motion events.");
                return;
            }

            scheduler.ConfigureApproachTime(data.approachTime);

            scorer.Reset();
            hud?.ResetHud();
            hud?.BindSources(clock, scheduler, head);
            hud?.SetCalibrationOffset(clock.Calibration);
            input?.SetClock(clock);
            head?.ResetMotion();
            cue?.ResetCue();
            scheduler.Configure(runtimeChart);
            clock.Play(song.audio, startSongTime);
        }

        void Update()
        {
            // Advance time-based scoring state (THE BANG expiry) on the authoritative clock.
            if (clock != null && clock.IsScheduled && !clock.IsPaused)
                scorer.Advance(clock.SongTime);

            RefreshHypeHud();

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

            // Manual THE BANG activation (only succeeds when HYPE is READY).
            if (kb.bKey.wasPressedThisFrame && clock != null && clock.IsScheduled)
            {
                if (scorer.TryActivateTheBang(clock.SongTime))
                    Debug.Log("THE BANG activated!");
            }

            if (kb.leftBracketKey.wasPressedThisFrame)
                AdjustCalibration(-calibrationStepMs / 1000.0);

            if (kb.rightBracketKey.wasPressedThisFrame)
                AdjustCalibration(calibrationStepMs / 1000.0);
        }

        void AdjustCalibration(double deltaSeconds)
        {
            if (clock == null) return;
            clock.Calibration += deltaSeconds;         // AudioClock is the single calibration owner
            hud?.SetCalibrationOffset(clock.Calibration);
            Debug.Log($"HH M0 calibration offset: {clock.Calibration * 1000.0:+0;-0;0} ms");
        }

        void RefreshHypeHud()
        {
            var h = scorer.Hype;
            hud?.SetHype(h.Hype, h.MaxHype, h.IsReady, h.TheBangActive, h.FinishersExecuted);
        }

        void OnCue(RuntimeMotionEvent ev, double approachTime) => cue?.Show(ev.Time, approachTime, ev.Direction);

        void OnBang(BangInput bang)
        {
            // Physical input is always accepted. Tapping early, late, on the wrong zone, or with no
            // active chart event still changes the neck state; chart judgment is separate.
            var intensity = scheduler != null && scheduler.HasActiveEvent
                ? scheduler.ActiveEvent.Intensity
                : 1f;

            // Canonical order: snapshot pre-inversion evidence + apply the neck impulse immediately,
            // then resolve the authored candidate, judge timing, and evaluate motion quality from
            // the pre-inversion snapshot. Candidate failure never undoes the neck input.
            var motionQuality = 0f;
            var wasSetup = true;
            if (head != null)
            {
                var snapshot = head.Bang(bang.Direction, intensity);
                var mq = MotionQualityEvaluator.Evaluate(snapshot, motionConfig);
                motionQuality = mq.Quality;
                wasSetup = mq.WasSetup;
            }

            if (scheduler == null || !scheduler.Resolve(bang, out var match))
                return; // too early / no candidate: neck moved, nothing consumed.

            scheduler.TryGetEvent(match.MatchedId, out var ev);

            var resolved = new ResolvedEvent(
                ev.Id ?? match.MatchedId.ToString(),
                match.Judgment,
                match.SignedError,
                motionQuality,
                wasSetup,
                ev.FinisherCandidate);

            var outcome = scorer.Resolve(resolved);

            cue?.Hide();
            hud?.Show(new JudgmentResult(outcome.Judgment, outcome.SignedTimingError, outcome.MotionQuality),
                      outcome.ComboAfter, scorer.Scoring.Score);
            RefreshHypeHud();

            Debug.Log($"{outcome.Judgment} {outcome.SignedTimingError * 1000.0:+0;-0;0} ms | motion {outcome.MotionQuality:0.00} | +{outcome.ScoreContribution} | combo {outcome.ComboAfter} | x{outcome.MultiplierAfter} | hype {scorer.Hype.Hype}{(outcome.WasFinisher ? " | FINISHER!" : "")}{(outcome.DuringTheBang ? " | THE BANG" : "")}");
        }

        void OnMiss(RuntimeMotionEvent ev)
        {
            // An authored event expired unresolved: score it as a MISS (combo/multiplier reset,
            // HYPE preserved) through the same authoritative path.
            var resolved = new ResolvedEvent(ev.Id, Judgment.Miss, 0d, 0f, false, ev.FinisherCandidate);
            var outcome = scorer.Resolve(resolved);
            cue?.Hide();
            hud?.Show(new JudgmentResult(Judgment.Miss, 0d, 0f), outcome.ComboAfter, scorer.Scoring.Score);
            RefreshHypeHud();
            Debug.Log($"MISS (expired) at {ev.Time:0.000}s | combo {outcome.ComboAfter} | hype {scorer.Hype.Hype}");
        }
    }
}
