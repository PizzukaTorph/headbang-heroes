using System;
using System.Collections.Generic;
using HeadbangHeroes.Charts.Runtime;

namespace HeadbangHeroes.Gameplay.Timing
{
    /// <summary>
    /// Pure, deterministic runtime resolution state over a bounded set of authored
    /// <see cref="RuntimeMotionEvent"/>s. Owns the unresolved/resolved bookkeeping and the expiry
    /// clock comparison with NO Unity/audio dependencies, so "resolve once / expire once / never
    /// re-consume" is unit-testable without a scene.
    ///
    /// The source <see cref="RuntimeChart"/> is already ordered and immutable; this resolver keeps
    /// per-run resolution flags separately (it never mutates the chart). Candidate slots align 1:1
    /// with the chart's ordered motion events; the matcher's stable tie-break uses the slot index.
    /// </summary>
    public sealed class CandidateResolver
    {
        readonly IReadOnlyList<RuntimeMotionEvent> events;   // ordered (Time, Id) by the compiler
        readonly bool[] resolved;                            // indexed by slot
        TimingConfig config;

        public CandidateResolver(IReadOnlyList<RuntimeMotionEvent> orderedMotion, in TimingConfig config)
        {
            this.config = config;
            events = orderedMotion ?? Array.Empty<RuntimeMotionEvent>();
            resolved = new bool[events.Count];
        }

        public int Count => events.Count;
        public TimingConfig Config => config;
        public void SetConfig(in TimingConfig value) => config = value;

        public RuntimeMotionEvent At(int slot) => events[slot];
        public bool IsResolved(int slot) => slot >= 0 && slot < resolved.Length && resolved[slot];

        /// <summary>
        /// Gap (seconds) between this event and the previous authored event, or a large value for
        /// the first event. Used to bound the cue approach so a cue never starts mid-flight at
        /// dense tempos (which made close speed feel incoherent).
        /// </summary>
        public double TimeSincePrevious(int slot)
        {
            if (slot <= 0 || slot >= events.Count) return double.MaxValue;
            return events[slot].Time - events[slot - 1].Time;
        }

        /// <summary>Song-time of the earliest still-unresolved event, or -1 if none remain.</summary>
        public double NextUnresolvedTime()
        {
            for (var i = 0; i < events.Count; i++)
                if (!resolved[i]) return events[i].Time;
            return -1d;
        }

        /// <summary>Earliest unresolved slot whose time is within the cue lead horizon of now, else -1.</summary>
        public int EarliestUnresolvedWithin(double now, double leadSeconds)
        {
            for (var i = 0; i < events.Count; i++)
            {
                if (resolved[i]) continue;
                return events[i].Time - now <= leadSeconds ? i : -1;
            }
            return -1;
        }

        /// <summary>
        /// Resolves an input against unresolved judgeable candidates. On a consumed result the
        /// matched event is marked resolved exactly once. Returns false when nothing was consumed.
        /// </summary>
        public bool Resolve(in BangInput input, List<MotionCandidate> buffer, out MatchResult result)
        {
            buffer.Clear();
            for (var i = 0; i < events.Count; i++)
            {
                if (resolved[i]) continue;
                var error = input.SongTime - events[i].Time;
                if (error < -config.GoodWindow || error > config.LateExpiry) continue;
                buffer.Add(new MotionCandidate(i, events[i].Time, events[i].Direction));
            }

            result = EventMatcher.Match(input, buffer, config);
            if (!result.Consumed) return false;

            if (result.MatchedId >= 0 && result.MatchedId < resolved.Length)
                resolved[result.MatchedId] = true;
            return true;
        }

        /// <summary>
        /// Marks every unresolved event past its late edge as resolved, invoking
        /// <paramref name="onExpired"/> (with the slot index) exactly once per expired event.
        /// </summary>
        public void Expire(double now, Action<int> onExpired)
        {
            for (var i = 0; i < events.Count; i++)
            {
                if (resolved[i]) continue;
                if (now - events[i].Time > config.LateExpiry)
                {
                    resolved[i] = true;
                    onExpired?.Invoke(i);
                }
            }
        }
    }
}
