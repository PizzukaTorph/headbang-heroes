using System;
using System.Collections.Generic;
using HeadbangHeroes.Charts;

namespace HeadbangHeroes.Gameplay.Timing
{
    /// <summary>
    /// Pure, deterministic runtime resolution state over a bounded set of authored MotionEvents.
    /// Owns the unresolved/resolved bookkeeping and the expiry clock comparison, with NO Unity or
    /// audio dependencies so the "resolve once / expire once / never re-consume" contracts are
    /// unit-testable without a scene.
    ///
    /// Events are copied into an internal array SORTED by (time, then stable Id) at construction,
    /// so CURRENT/NEXT queries and expiry do not assume the caller pre-sorted the chart.
    /// The stable Id is the authored event's original index (a durable authored id will replace
    /// this when Plan 03 introduces the immutable RuntimeChart).
    /// </summary>
    public sealed class CandidateResolver
    {
        readonly MotionCandidate[] events;   // sorted by time, then original index; Id == slot
        readonly int[] originalIndex;        // events[slot].Id -> original ChartDefinition index
        readonly bool[] resolved;            // indexed by slot (== MotionCandidate.Id)
        TimingConfig config;

        public CandidateResolver(IReadOnlyList<ChartEvent> source, in TimingConfig config)
        {
            this.config = config;
            var count = source?.Count ?? 0;

            // Sort original indices by (time, then original index) for a stable order.
            var order = new int[count];
            for (var i = 0; i < count; i++) order[i] = i;
            Array.Sort(order, (a, b) =>
            {
                var byTime = source[a].time.CompareTo(source[b].time);
                return byTime != 0 ? byTime : a.CompareTo(b);
            });

            events = new MotionCandidate[count];
            originalIndex = new int[count];
            for (var slot = 0; slot < count; slot++)
            {
                var orig = order[slot];
                originalIndex[slot] = orig;
                // Id == slot so resolved[Id] and matcher tie-break use the sorted-stable slot.
                events[slot] = new MotionCandidate(slot, source[orig].time, source[orig].direction);
            }

            resolved = new bool[count];
        }

        public int Count => events.Length;
        public TimingConfig Config => config;
        public void SetConfig(in TimingConfig value) => config = value;

        /// <summary>Original ChartDefinition index for a resolver slot (for looking up the source event).</summary>
        public int OriginalIndex(int slot) => originalIndex[slot];

        public bool IsResolved(int slot) => slot >= 0 && slot < resolved.Length && resolved[slot];

        /// <summary>Song-time of the earliest still-unresolved event, or -1 if none remain.</summary>
        public double NextUnresolvedTime()
        {
            for (var i = 0; i < events.Length; i++)
                if (!resolved[i]) return events[i].SongTime;
            return -1d;
        }

        /// <summary>Earliest unresolved event whose time is within the cue lead horizon of now, else -1.</summary>
        public int EarliestUnresolvedWithin(double now, double leadSeconds)
        {
            for (var i = 0; i < events.Length; i++)
            {
                if (resolved[i]) continue;
                return events[i].SongTime - now <= leadSeconds ? i : -1;
            }
            return -1;
        }

        public MotionCandidate At(int index) => events[index];

        /// <summary>
        /// Resolves an input against the unresolved judgeable candidates. On a consumed result the
        /// matched event is marked resolved exactly once. Returns false when nothing was consumed
        /// (too early / empty) — the neck has already moved upstream regardless.
        /// </summary>
        public bool Resolve(in BangInput input, List<MotionCandidate> buffer, out MatchResult result)
        {
            buffer.Clear();
            for (var i = 0; i < events.Length; i++)
            {
                if (resolved[i]) continue;
                var error = input.SongTime - events[i].SongTime;
                if (error < -config.GoodWindow || error > config.LateExpiry) continue;
                buffer.Add(events[i]);
            }

            result = EventMatcher.Match(input, buffer, config);
            if (!result.Consumed) return false;

            if (result.MatchedId >= 0 && result.MatchedId < resolved.Length)
                resolved[result.MatchedId] = true;
            return true;
        }

        /// <summary>
        /// Marks every unresolved event past its late edge as resolved, invoking
        /// <paramref name="onExpired"/> exactly once per expired event.
        /// </summary>
        public void Expire(double now, Action<MotionCandidate> onExpired)
        {
            for (var i = 0; i < events.Length; i++)
            {
                if (resolved[i]) continue;
                if (now - events[i].SongTime > config.LateExpiry)
                {
                    resolved[i] = true;
                    onExpired?.Invoke(events[i]);
                }
            }
        }
    }
}
