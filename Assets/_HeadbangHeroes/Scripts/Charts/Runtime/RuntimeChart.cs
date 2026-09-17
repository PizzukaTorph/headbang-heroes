using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HeadbangHeroes.Charts.Runtime
{
    /// <summary>
    /// Immutable, prevalidated, deterministically-ordered runtime chart consumed by gameplay.
    /// It owns authored chronology and identity/version context; it does NOT judge input, move the
    /// neck, render cues, or track per-run resolution state (that lives in ChartRuntime/run state).
    ///
    /// Motion events are ordered by (Time, then Id) so equal-time events have a stable order.
    /// <see cref="QueryRange"/> gives the Plan-02 matcher cheap bounded access without exposing
    /// authoring data or embedding any judgment.
    /// </summary>
    public sealed class RuntimeChart
    {
        public string SongId { get; }
        public string ChartId { get; }
        public int SchemaVersion { get; }
        public int ChartVersion { get; }
        public int RulesVersion { get; }
        public string Difficulty { get; }

        readonly RuntimeMotionEvent[] motion;    // sorted (Time, Id)
        readonly RuntimeRestEvent[] rest;        // sorted (StartTime, Id)
        readonly ReadOnlyCollection<RuntimeMotionEvent> motionView;
        readonly ReadOnlyCollection<RuntimeRestEvent> restView;

        // Exposed as a read-only view that cannot be cast back to the backing array and mutated.
        public IReadOnlyList<RuntimeMotionEvent> MotionEvents => motionView;
        public IReadOnlyList<RuntimeRestEvent> RestEvents => restView;

        public int MotionCount => motion.Length;
        public int RestCount => rest.Length;

        internal RuntimeChart(
            string songId, string chartId,
            int schemaVersion, int chartVersion, int rulesVersion,
            string difficulty,
            RuntimeMotionEvent[] sortedMotion,
            RuntimeRestEvent[] sortedRest)
        {
            SongId = songId;
            ChartId = chartId;
            SchemaVersion = schemaVersion;
            ChartVersion = chartVersion;
            RulesVersion = rulesVersion;
            Difficulty = difficulty;
            motion = sortedMotion;
            rest = sortedRest;
            motionView = new ReadOnlyCollection<RuntimeMotionEvent>(motion);
            restView = new ReadOnlyCollection<RuntimeRestEvent>(rest);
        }

        /// <summary>
        /// Appends the motion events whose Time is within [fromTime, toTime] (inclusive) to
        /// <paramref name="buffer"/>, in deterministic chart order. Does not clear the buffer.
        /// Cheap linear scan over the immutable ordered array; a cursor optimization can replace
        /// this later without changing the contract.
        /// </summary>
        public void QueryRange(double fromTime, double toTime, List<RuntimeMotionEvent> buffer)
        {
            if (buffer == null) return;
            for (var i = 0; i < motion.Length; i++)
            {
                var t = motion[i].Time;
                if (t < fromTime) continue;
                if (t > toTime) break; // ordered by time: nothing later can match
                buffer.Add(motion[i]);
            }
        }

        /// <summary>Time of the first motion event at or after <paramref name="time"/>, or -1.</summary>
        public double NextMotionTimeAtOrAfter(double time)
        {
            for (var i = 0; i < motion.Length; i++)
                if (motion[i].Time >= time) return motion[i].Time;
            return -1d;
        }

        /// <summary>Returns the authored Rest interval active at <paramref name="time"/>, if any.</summary>
        public bool TryGetRestAt(double time, out RuntimeRestEvent result)
        {
            for (var i = 0; i < rest.Length; i++)
            {
                if (time >= rest[i].StartTime && time <= rest[i].EndTime)
                {
                    result = rest[i];
                    return true;
                }
            }
            result = default;
            return false;
        }
    }
}
