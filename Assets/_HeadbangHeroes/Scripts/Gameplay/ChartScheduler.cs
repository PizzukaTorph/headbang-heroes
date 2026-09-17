using System;
using System.Collections.Generic;
using HeadbangHeroes.Audio;
using HeadbangHeroes.Charts.Runtime;
using HeadbangHeroes.Gameplay.Timing;
using UnityEngine;

namespace HeadbangHeroes.Gameplay
{
    /// <summary>
    /// Unity adapter that owns authored chronology at runtime by consuming an immutable
    /// <see cref="RuntimeChart"/> and resolving bang inputs against a BOUNDED unresolved candidate
    /// set via the pure <see cref="CandidateResolver"/>.
    ///
    /// Per-run resolution state lives in the resolver (flags), NEVER by mutating the RuntimeChart.
    /// CURRENT/NEXT cue state is derived here for presentation but never defines judgment time.
    /// </summary>
    public sealed class ChartScheduler : MonoBehaviour
    {
        [SerializeField] AudioClock clock;

        [Header("Timing windows (data-driven, seconds)")]
        [SerializeField, Min(0.001f)] double perfectWindow = 0.035;
        [SerializeField, Min(0.001f)] double greatWindow = 0.070;
        [SerializeField, Min(0.001f)] double goodWindow = 0.120;
        [SerializeField, Min(0.001f)] double lateExpiry = 0.120;
        [Header("Presentation cue horizon (seconds)")]
        [SerializeField, Min(0.05f)] double cueLead = 1.0;

        RuntimeChart chart;
        CandidateResolver resolver;
        int cueIndex = -1;  // resolver slot whose cue is currently shown (CURRENT), or -1
        readonly List<MotionCandidate> candidateBuffer = new(16);

        public event Action<RuntimeMotionEvent, double> CueActivated;   // presentation only
        public event Action<RuntimeMotionEvent> EventMissed;            // an unresolved event expired

        public TimingConfig Timing => new TimingConfig(perfectWindow, greatWindow, goodWindow, cueLead, lateExpiry);

        public bool HasActiveEvent => cueIndex >= 0;
        public RuntimeMotionEvent ActiveEvent => resolver.At(cueIndex);
        public double ApproachTime => cueLead;
        public double NextEventTime => resolver?.NextUnresolvedTime() ?? -1d;

        /// <summary>Returns the authored runtime event for a resolved match id (slot), for outcome assembly.</summary>
        public bool TryGetEvent(int matchedId, out RuntimeMotionEvent ev)
        {
            if (resolver != null && matchedId >= 0 && matchedId < resolver.Count)
            {
                ev = resolver.At(matchedId);
                return true;
            }
            ev = default;
            return false;
        }

        /// <summary>Sets the immutable runtime chart for the run and resets per-run resolution state.</summary>
        public void Configure(RuntimeChart runtimeChart)
        {
            chart = runtimeChart;
            ResetScheduler();
        }

        public void ConfigureApproachTime(double value) => cueLead = Math.Max(0.05, value);

        public void ResetScheduler()
        {
            resolver = new CandidateResolver(chart != null ? chart.MotionEvents : null, Timing);
            cueIndex = -1;
        }

        void Update()
        {
            if (clock == null || chart == null || resolver == null || !clock.IsScheduled) return;
            resolver.SetConfig(Timing);
            var now = clock.SongTime;

            // Expire unresolved events past their late edge — exactly once each.
            resolver.Expire(now, slot =>
            {
                if (cueIndex == slot) cueIndex = -1;
                EventMissed?.Invoke(resolver.At(slot));
            });

            // CURRENT cue: the earliest unresolved event within the presentation lead horizon.
            if (cueIndex < 0)
            {
                var idx = resolver.EarliestUnresolvedWithin(now, cueLead);
                if (idx >= 0)
                {
                    cueIndex = idx;
                    CueActivated?.Invoke(resolver.At(idx), cueLead);
                }
            }
        }

        /// <summary>
        /// Resolves a semantic bang (already in song-time) against the unresolved candidate set.
        /// Returns false only when nothing was consumed (too early / no chart).
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
