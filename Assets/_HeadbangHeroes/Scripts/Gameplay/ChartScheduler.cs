using System;
using System.Collections.Generic;
using HeadbangHeroes.Audio;
using HeadbangHeroes.Charts;
using HeadbangHeroes.Gameplay.Timing;
using UnityEngine;

namespace HeadbangHeroes.Gameplay
{
    /// <summary>
    /// Owns authored chronology at runtime and resolves bang inputs against a BOUNDED set of
    /// unresolved candidate events using the deterministic <see cref="EventMatcher"/>.
    ///
    /// This replaces the M0 single-active-event assumption: any input is matched against every
    /// unresolved event inside the configured horizon around its song-time, not merely "the next
    /// array item". CURRENT/NEXT is derived here for presentation but never defines judgment time.
    ///
    /// Temporary: reads M0 <see cref="ChartDefinition"/> directly. Plan 03 will replace the source
    /// with an immutable RuntimeChart behind the same candidate-query shape.
    /// </summary>
    public sealed class ChartScheduler : MonoBehaviour
    {
        [SerializeField] AudioClock clock;
        [SerializeField] ChartDefinition chart;

        [Header("Timing windows (data-driven)")]
        [SerializeField, Min(0.001f)] double perfectWindow = 0.035;
        [SerializeField, Min(0.001f)] double greatWindow = 0.070;
        [SerializeField, Min(0.001f)] double goodWindow = 0.120;
        [SerializeField, Min(0.05f)] double candidateLead = 1.0;

        bool[] resolved;                // per-event resolution flag (resolved at most once)
        int cueIndex = -1;              // index whose cue is currently shown (CURRENT), or -1
        readonly List<MotionCandidate> candidateBuffer = new(16);

        public event Action<ChartEvent, double> CueActivated;   // (event, approachTime) — presentation only
        public event Action<ChartEvent> EventMissed;            // an unresolved event expired

        public TimingConfig Timing => new TimingConfig(perfectWindow, greatWindow, goodWindow, candidateLead, goodWindow);

        // ---- CURRENT / NEXT observer state (presentation), never authoritative for judgment ----
        public bool HasActiveEvent => cueIndex >= 0;
        public ChartEvent ActiveEvent => chart.events[cueIndex];
        public double ApproachTime => candidateLead;

        public double NextEventTime
        {
            get
            {
                if (chart == null || resolved == null) return -1;
                for (var i = 0; i < chart.events.Count; i++)
                    if (!resolved[i]) return chart.events[i].time;
                return -1;
            }
        }

        public void Configure(ChartDefinition value)
        {
            chart = value;
            ResetScheduler();
        }

        public void ConfigureApproachTime(double value) => candidateLead = Math.Max(0.05, value);

        public void ResetScheduler()
        {
            resolved = chart != null ? new bool[chart.events.Count] : Array.Empty<bool>();
            cueIndex = -1;
        }

        void Update()
        {
            if (clock == null || chart == null || resolved == null || !clock.IsScheduled) return;
            var now = clock.SongTime;
            var cfg = Timing;

            // Expire any unresolved event that has passed its late edge — exactly once.
            for (var i = 0; i < chart.events.Count; i++)
            {
                if (resolved[i]) continue;
                if (now - chart.events[i].time > cfg.LateExpiry)
                {
                    resolved[i] = true;
                    if (cueIndex == i) cueIndex = -1;
                    EventMissed?.Invoke(chart.events[i]);
                }
            }

            // CURRENT cue: the earliest unresolved event within the approach horizon.
            if (cueIndex < 0)
            {
                for (var i = 0; i < chart.events.Count; i++)
                {
                    if (resolved[i]) continue;
                    var until = chart.events[i].time - now;
                    if (until <= cfg.CandidateLead)
                    {
                        cueIndex = i;
                        CueActivated?.Invoke(chart.events[i], cfg.CandidateLead);
                    }
                    break; // only the earliest unresolved event is a CURRENT candidate
                }
            }
        }

        /// <summary>
        /// Resolves a semantic bang (already in song-time) against the unresolved candidate set.
        /// Marks the matched event resolved (once) and returns the deterministic result.
        /// Returns false only when nothing was consumed (too early / no chart) — the neck has
        /// already moved in that case; that is the controller's concern, not the scheduler's.
        /// </summary>
        public bool Resolve(in BangInput input, out MatchResult result)
        {
            result = MatchResult.NoneTooEarly;
            if (chart == null || resolved == null) return false;

            var cfg = Timing;
            candidateBuffer.Clear();
            for (var i = 0; i < chart.events.Count; i++)
            {
                if (resolved[i]) continue;
                var error = input.SongTime - chart.events[i].time;
                if (error < -cfg.GoodWindow || error > cfg.LateExpiry) continue; // outside judgeable window
                candidateBuffer.Add(new MotionCandidate(i, chart.events[i].time, chart.events[i].direction));
            }

            result = EventMatcher.Match(input, candidateBuffer, cfg);
            if (!result.Consumed) return false;

            var id = result.MatchedId;
            if (id >= 0 && id < resolved.Length)
            {
                resolved[id] = true;
                if (cueIndex == id) cueIndex = -1;
            }
            return true;
        }
    }
}
