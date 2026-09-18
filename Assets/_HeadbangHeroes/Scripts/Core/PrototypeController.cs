using HeadbangHeroes.Audio;
using HeadbangHeroes.Charts;
using HeadbangHeroes.Charts.Runtime;
using HeadbangHeroes.Gameplay;
using HeadbangHeroes.Gameplay.Neck;
using HeadbangHeroes.Gameplay.Scoring;
using HeadbangHeroes.Gameplay.Timing;
using HeadbangHeroes.Input;
using HeadbangHeroes.Presentation;
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
        [SerializeField] BangZoneHint zoneHint;   // onboarding: reveal the 4 bang zones at run start
        [SerializeField] ZoneTapFlash zoneFlash;  // per-tap: flash the tapped section

        [Header("Presentation (downstream only)")]
        [SerializeField] NeckPresenter neckPresenter;
        [SerializeField] BodyReactionPresenter bodyPresenter;
        [SerializeField] HairReactionPresenter hairPresenter;
        [SerializeField] VenueReactionPresenter venuePresenter;
        [SerializeField] HapticsService haptics;

        [SerializeField] bool startOnPlay = true;
        [SerializeField, Min(0f)] double startSongTime = 11.8;

        [Header("Playtest controls (Editor / Development builds)")]
        [SerializeField] bool enablePlaytestControls = true;
        [SerializeField] double calibrationStepMs = 20.0;

        [Header("Accessibility (presentation only; never changes scoring)")]
        [SerializeField] bool reducedFlash = false;
        [SerializeField] bool reducedShake = false;
        [SerializeField] bool hapticsEnabled = true;
        [SerializeField, Range(0f, 1f)] float hapticIntensity = 1f;

        readonly RunScorer scorer = new(ScoringConfig.Default, HypeConfig.Default);
        readonly MotionQualityConfig motionConfig = MotionQualityConfig.Default;
        RuntimeChart runtimeChart;
        bool wasReady;
        bool running;                 // true from run start until the RunResult is finalized
        double completionGraceUntil;  // small tail after last event before finalizing

        /// <summary>Fired exactly once when a run finishes, carrying the authoritative RunResult.</summary>
        public event System.Action<RunResult> RunCompleted;

        // ---- Public entry points shared by keyboard playtest controls AND on-screen touch UI ----
        // These are the single source of truth for each action so touch and keyboard never diverge.

        /// <summary>True while a run is active and the clock is scheduled (gameplay in progress).</summary>
        public bool IsRunning => running && clock != null && clock.IsScheduled;

        /// <summary>True when the clock is currently paused.</summary>
        public bool IsPaused => clock != null && clock.IsPaused;

        /// <summary>True when HYPE is READY and THE BANG can be activated (drives the THE BANG button).</summary>
        public bool IsHypeReady => scorer.Hype.IsReady && clock != null && clock.IsScheduled && !clock.IsPaused;

        /// <summary>Current calibration offset (seconds) — for on-screen readout.</summary>
        public double CalibrationSeconds => clock != null ? clock.Calibration : 0d;

        /// <summary>Current compensated output latency (seconds) — for on-screen readout.</summary>
        public double OutputLatencySeconds => clock != null ? clock.OutputLatency : 0d;

        /// <summary>Toggle pause/resume (keyboard Space and the on-screen PAUSE button).</summary>
        public void TogglePause()
        {
            if (clock == null || !clock.IsScheduled) return;
            if (clock.IsPaused) clock.Resume();
            else clock.Pause();
        }

        public void Pause() { if (clock != null && clock.IsScheduled && !clock.IsPaused) clock.Pause(); }
        public void Resume() { if (clock != null && clock.IsScheduled && clock.IsPaused) clock.Resume(); }

        /// <summary>
        /// Abandon the current run without finalizing a RunResult (QUIT). Stops the clock and clears
        /// the running flag; no RunCompleted is fired, so the meta pipeline does not record it.
        /// </summary>
        public void AbortRun()
        {
            running = false;
            if (clock != null) clock.Stop();
        }

        /// <summary>Manually activate THE BANG (only succeeds when HYPE is READY). Keyboard B + touch button.</summary>
        public bool TryActivateTheBang()
        {
            if (clock == null || !clock.IsScheduled) return false;
            if (scorer.TryActivateTheBang(clock.SongTime))
            {
                haptics?.Play(HapticEvent.TheBangActivated);
                RefreshHypeHud();
                Debug.Log("THE BANG activated!");
                return true;
            }
            return false;
        }

        /// <summary>Nudge calibration by one step (+/-). Used by keyboard [ ] and the on-screen +/- buttons.</summary>
        public void NudgeCalibration(int steps) => AdjustCalibration(steps * calibrationStepMs / 1000.0);

        /// <summary>Applies persisted profile settings/calibration to the run (called by the flow before start).</summary>
        public void ApplyProfileSettings(double calibrationSeconds, bool rFlash, bool rShake, bool hEnabled, float hIntensity)
        {
            reducedFlash = rFlash;
            reducedShake = rShake;
            hapticsEnabled = hEnabled;
            hapticIntensity = hIntensity;
            if (clock != null) clock.Calibration = calibrationSeconds;
        }

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
            wasReady = false;
            var access = new AccessibilitySettings
            {
                ReducedFlash = reducedFlash,
                ReducedShake = reducedShake,
                HapticsEnabled = hapticsEnabled,
                HapticIntensity = hapticIntensity
            };
            hud?.ResetHud();
            hud?.BindSources(clock, scheduler, head);
            hud?.SetCalibrationOffset(clock.Calibration);
            hud?.SetLatencyOffset(clock.OutputLatency);
            input?.SetClock(clock);
            head?.ResetMotion();
            neckPresenter?.ResetPresentation();
            bodyPresenter?.ResetPresentation();
            hairPresenter?.ResetPresentation();
            venuePresenter?.ResetPresentation();
            venuePresenter?.ApplySettings(access);
            haptics?.ApplySettings(access);
            cue?.ResetCue();
            zoneHint?.Show();
            zoneFlash?.ResetAll();
            scheduler.Configure(runtimeChart);
            clock.Play(song.audio, startSongTime);
            running = true;
            completionGraceUntil = 0d;
        }

        void Update()
        {
            // Advance time-based scoring state (THE BANG expiry) on the authoritative clock.
            if (clock != null && clock.IsScheduled && !clock.IsPaused)
                scorer.Advance(clock.SongTime);

            RefreshHypeHud();
            PushPresentationSignals();
            CheckRunCompletion();

            if (enablePlaytestControls) HandlePlaytestControls();
        }

        void CheckRunCompletion()
        {
            if (!running || scheduler == null || clock == null || !clock.IsScheduled) return;
            if (!scheduler.AllResolved) return;

            // Give a short tail so the last event's feedback is seen before Results.
            if (completionGraceUntil <= 0d)
            {
                completionGraceUntil = clock.SongTime + 0.5;
                return;
            }
            if (clock.SongTime < completionGraceUntil) return;

            running = false;
            var result = scorer.BuildResult(
                runtimeChart.SongId, runtimeChart.ChartId, runtimeChart.ChartVersion, runtimeChart.RulesVersion);
            clock.Stop();
            RunCompleted?.Invoke(result);   // fired exactly once per run
        }

        void PushPresentationSignals()
        {
            var h = scorer.Hype;
            var hypeFraction = h.MaxHype > 0 ? (float)h.Hype / h.MaxHype : 0f;
            venuePresenter?.SetPerformanceSignals(hypeFraction, h.TheBangActive);

            if (clock != null && clock.IsScheduled)
            {
                var length = song != null && song.audio != null ? song.audio.length : 0.0;
                hud?.SetTopBar(scorer.Scoring.Score, clock.SongTime, length, scorer.Scoring.Multiplier);
            }
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

            if (kb.spaceKey.wasPressedThisFrame) TogglePause();

            // Manual THE BANG activation (only succeeds when HYPE is READY).
            if (kb.bKey.wasPressedThisFrame) TryActivateTheBang();

            if (kb.leftBracketKey.wasPressedThisFrame) NudgeCalibration(-1);
            if (kb.rightBracketKey.wasPressedThisFrame) NudgeCalibration(+1);
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

        void OnCue(RuntimeMotionEvent ev, double approachTime)
            => cue?.Show(ev.Time, approachTime, ev.Direction, scheduler != null ? scheduler.Timing.GoodWindow : 0.12);

        void OnBang(BangInput bang)
        {
            // Section tap feedback: flash the whole quadrant the player tapped (presentation only).
            zoneFlash?.Flash(bang.Direction);

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
            {
                // Neck moved, but no authored candidate was consumed (too early / none active).
                var nextT = scheduler != null ? scheduler.NextEventTime : -1d;
                Debug.Log($"(no consume) bang {bang.Direction} @songT {bang.SongTime:0.000} | next ev {nextT:0.000} | activeCue {(scheduler != null && scheduler.HasActiveEvent)}");
                return;
            }

            scheduler.TryGetEvent(match.MatchedId, out var ev);

            var resolved = new ResolvedEvent(
                ev.Id ?? match.MatchedId.ToString(),
                match.Judgment,
                match.SignedError,
                motionQuality,
                wasSetup,
                ev.FinisherCandidate);

            var outcome = scorer.Resolve(resolved);

            // Semantic presentation feedback (downstream only; never affects the outcome above).
            PlayJudgmentHaptic(outcome.Judgment);
            if (outcome.WasFinisher)
            {
                haptics?.Play(HapticEvent.Finisher);
                venuePresenter?.PulseFinisher();
            }
            // HYPE READY haptic fires once on the not-ready -> ready transition, not every bang.
            var ready = scorer.Hype.IsReady;
            if (ready && !wasReady) haptics?.Play(HapticEvent.HypeReady);
            wasReady = ready;

            // Snapshot the cue on the tap: a blue ring concentric to the target at the size the
            // closing ring had at that instant — dead-on coincides with the target, early is a
            // larger ring, late a smaller one. Lets the player build a mental map to self-calibrate.
            cue?.ShowHitMarker(outcome.SignedTimingError);
            cue?.Hide();
            hud?.Show(new JudgmentResult(outcome.Judgment, outcome.SignedTimingError, outcome.MotionQuality),
                      outcome.ComboAfter, scorer.Scoring.Score);
            RefreshHypeHud();

            // Diagnostic: pressed vs expected direction and why a MISS happened (timing vs wrong-dir).
            var reason = match.Kind == MatchKind.WrongConsumedMiss
                ? $" WRONG-DIR (pressed {bang.Direction}, expected {ev.Direction})"
                : outcome.Judgment == Judgment.Miss ? " TIMING-MISS" : "";
            Debug.Log($"{outcome.Judgment} {outcome.SignedTimingError * 1000.0:+0;-0;0} ms{reason} | motion {outcome.MotionQuality:0.00} | +{outcome.ScoreContribution} | combo {outcome.ComboAfter} | x{outcome.MultiplierAfter} | hype {scorer.Hype.Hype}{(outcome.WasFinisher ? " | FINISHER!" : "")}{(outcome.DuringTheBang ? " | THE BANG" : "")}");
        }

        void PlayJudgmentHaptic(Judgment judgment)
        {
            switch (judgment)
            {
                case Judgment.Perfect: haptics?.Play(HapticEvent.Perfect); break;
                case Judgment.Great: haptics?.Play(HapticEvent.Great); break;
                case Judgment.Miss: haptics?.Play(HapticEvent.Miss); break;
            }
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
