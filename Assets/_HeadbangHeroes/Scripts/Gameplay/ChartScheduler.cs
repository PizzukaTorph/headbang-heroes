using System;
using System.Collections.Generic;
using HeadbangHeroes.Audio;
using HeadbangHeroes.Charts;
using HeadbangHeroes.Gameplay.Timing;
using UnityEngine;

namespace HeadbangHeroes.Gameplay
{
    /// <summary>
    /// Unity adapter that owns authored chronology at runtime and resolves bang inputs against a
    /// BOUNDED unresolved candidate set via the pure <see cref="CandidateResolver"/> (which holds
    /// the deterministic resolve-once / expire-once bookkeeping and event sorting).
    ///
    /// This replaces the M0 single-active-event assumption. CURRENT/NEXT cue state is derived here
    /// for presentation but never defines judgment time. Temporary: reads M0
    /// <see cref="ChartDefinition"/>; Plan 03 will supply an immutable RuntimeChart behind the
    /// same candidate-query shape.
    /// </summary>
    public sealed class ChartScheduler : MonoBehaviour
    {
        [SerializeField] AudioClock clock;
        [SerializeField] ChartDefinition chart;

        [Header("Timing windows (data-driven, seconds)")]
        [SerializeField, Min(0.001f)] double perfectWindow = 0.035;
        [SerializeField, Min(0.001f)] double greatWindow = 0.070;
        [SerializeField, Min(0.001f)] double goodWindow = 0.120;
        [SerializeField, Min(0.001f)] double lateExpiry = 0.120;
        [Header("Presentation cue horizon (seconds)")]
        [SerializeField, Min(0.05f)] double cueLead = 1.0;

        CandidateResolver resolver;
        int cueIndex = -1;  // resolver index whose cue is currently shown (CURRENT), or -1
        readonly List<MotionCandidate> candidateBuffer = new(16);

        public event Action<ChartEvent, double> CueActivated;   // presentation only
        public event Action<ChartEvent> EventMissed;            // an unresolved event expired

        public TimingConfig Timing => new TimingConfig(perfectWindow, greatWindow, goodWindow, cueLead, lateExpiry);

        // ---- CURRENT / NEXT observer state (presentation), never authoritative for judgment ----
        public bool HasActiveEvent => cueIndex >= 0;
        public ChartEvent ActiveEvent => chart.events[resolver.OriginalIndex(cueIndex)];
        public double ApproachTime => cueLead;
        public double NextEventTime => resolver?.NextUnresolvedTime() ?? -1d;

        public void Configure(ChartDefinition value)
        {
            chart = value;
            ResetScheduler();
        }

        public void ConfigureApproachTime(double value) => cueLead = Math.Max(0.05, value);

        public void ResetScheduler()
        {
            resolver = new CandidateResolver(chart != null ? chart.events : null, Timing);
            cueIndex = -1;
        }

        void Update()
        {
            if (clock == null || chart == null || resolver == null || !clock.IsScheduled) return;
            resolver.SetConfig(Timing);
            var now = clock.SongTime;

            // Expire unresolved events past their late edge — exactly once each.
            resolver.Expire(now, expired =>
            {
                if (cueIndex >= 0 && cueIndex == expired.Id) cueIndex = -1;   // Id == resolver slot
                EventMissed?.Invoke(chart.events[resolver.OriginalIndex(expired.Id)]);
            });

            // CURRENT cue: the earliest unresolved event within the presentation lead horizon.
            if (cueIndex < 0)
            {
                var idx = resolver.EarliestUnresolvedWithin(now, cueLead);
                if (idx >= 0)
                {
                    cueIndex = idx;
                    CueActivated?.Invoke(chart.events[resolver.OriginalIndex(idx)], cueLead);
                }
            }
        }

        /// <summary>
        /// Resolves a semantic bang (already in song-time) against the unresolved candidate set.
        /// Returns false only when nothing was consumed (too early / no chart) — the neck has
        /// already moved upstream in that case.
        /// </summary>
        public bool Resolve(in BangInput input, out MatchResult result)
        {
            result = MatchResult.NoneTooEarly;
            if (resolver == null) return false;

            resolver.SetConfig(Timing);
            if (!resolver.Resolve(input, candidateBuffer, out result)) return false;

            if (cueIndex >= 0 && resolver.IsResolved(cueIndex)) cueIndex = -1;
            return true;
        }
    }
}
