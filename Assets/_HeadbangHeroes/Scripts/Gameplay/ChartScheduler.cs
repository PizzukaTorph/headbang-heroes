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
        // Generous prototype windows: the point of M0 is "is the headbang fun", not millimetric
        // precision. Being roughly on the beat should read as Good/Great; WELL is the sloppy edge
        // that still credits 1 point; a consumed cue is only a MISS when the player is COMPLETELY
        // early or late (beyond the WELL edge), plays the wrong direction, or there is no cue.
        [SerializeField, Min(0.001f)] double perfectWindow = 0.090;
        [SerializeField, Min(0.001f)] double greatWindow = 0.180;
        [SerializeField, Min(0.001f)] double goodWindow = 0.300;
        [SerializeField, Min(0.001f)] double wellWindow = 0.450;   // sloppy-but-there hit; beyond this a consumed cue is a MISS
        [SerializeField, Min(0.001f)] double lateExpiry = 0.450;   // matches wellWindow so late Wells still consume the cue
        [Header("Presentation cue horizon (seconds)")]
        [SerializeField, Min(0.05f)] double cueLead = 1.0;

        RuntimeChart chart;
        CandidateResolver resolver;
        int cueIndex = -1;  // resolver slot whose cue is currently shown (CURRENT), or -1
        bool chartExhaustedLogged;
        readonly List<MotionCandidate> candidateBuffer = new(16);

        public event Action<RuntimeMotionEvent, double> CueActivated;   // presentation only
        public event Action<RuntimeMotionEvent> EventMissed;            // an unresolved event expired

        public TimingConfig Timing => new TimingConfig(perfectWindow, greatWindow, goodWindow, wellWindow, cueLead, lateExpiry);

        public bool HasActiveEvent => cueIndex >= 0;
        public RuntimeMotionEvent ActiveEvent => resolver.At(cueIndex);
        public double ApproachTime => cueLead;
        public double NextEventTime => resolver?.NextUnresolvedTime() ?? -1d;

        /// <summary>True once every authored event has been resolved (hit or expired) — end of chart.</summary>
        public bool AllResolved => resolver != null && resolver.Count > 0 && resolver.NextUnresolvedTime() < 0d;

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
            chartExhaustedLogged = false;
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

            // CURRENT cue: the earliest unresolved event. Its approach time is bounded by the gap
            // to the previous event so the ring always starts from full scale and closes at a rate
            // proportional to the spacing — consistent within a tempo, instead of appearing
            // mid-flight and snapping shut at dense tempos.
            if (cueIndex < 0)
            {
                var idx = resolver.EarliestUnresolvedWithin(now, cueLead);
                if (idx >= 0)
                {
                    var approach = System.Math.Min(cueLead, resolver.TimeSincePrevious(idx));
                    // Only start the cue once we are actually within its (bounded) approach window,
                    // so it begins at full scale rather than partway closed.
                    if (resolver.At(idx).Time - now <= approach)
                    {
                        cueIndex = idx;
                        CueActivated?.Invoke(resolver.At(idx), approach);
                    }
                }
                else if (resolver.NextUnresolvedTime() < 0d && !chartExhaustedLogged)
                {
                    chartExhaustedLogged = true;
                    Debug.Log($"HH M0: chart exhausted (all {resolver.Count} events resolved) at songT {now:0.000}. No more cues by design.");
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
