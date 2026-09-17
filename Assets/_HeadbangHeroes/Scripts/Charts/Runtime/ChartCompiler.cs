using System;
using System.Collections.Generic;

namespace HeadbangHeroes.Charts.Runtime
{
    /// <summary>Thrown when authoring chart data cannot be compiled into a valid runtime chart.</summary>
    public sealed class ChartValidationException : Exception
    {
        public ChartValidationException(string message) : base(message) { }
    }

    /// <summary>
    /// Validates and compiles authoring <see cref="ChartJsonData"/> into an immutable, ordered,
    /// prevalidated <see cref="RuntimeChart"/>. Runtime-incompatible data is a HARD failure
    /// (throws <see cref="ChartValidationException"/>) — unknown semantics are never silently
    /// dropped. Times are taken as precomputed seconds for the current subset; direction/technique
    /// naming (including the legacy M0 integer direction) is normalized here, once.
    /// </summary>
    public static class ChartCompiler
    {
        public const int SupportedSchemaVersion = 1;
        public const int SupportedRulesVersion = 1;

        public static RuntimeChart Compile(ChartJsonData data)
        {
            if (data == null) throw new ChartValidationException("Chart data is null.");

            var schema = data.schemaVersion == 0 ? SupportedSchemaVersion : data.schemaVersion;
            var rules = data.rulesVersion == 0 ? SupportedRulesVersion : data.rulesVersion;

            if (schema > SupportedSchemaVersion)
                throw new ChartValidationException(
                    $"Unsupported chart schemaVersion {schema} (client supports up to {SupportedSchemaVersion}).");
            if (rules > SupportedRulesVersion)
                throw new ChartValidationException(
                    $"Unsupported chart rulesVersion {rules} (client supports up to {SupportedRulesVersion}).");

            if (string.IsNullOrWhiteSpace(data.chartId))
                throw new ChartValidationException("Chart is missing a chartId.");

            var defaultTechnique = ParseTechnique(data.technique, NeckTechnique.Classic, "chart.technique");

            var motion = CompileMotion(data, defaultTechnique, out var seenIds);
            var rest = CompileRest(data, seenIds);

            return new RuntimeChart(
                data.songId ?? string.Empty,
                data.chartId,
                schema, data.version, rules,
                data.difficulty ?? string.Empty,
                motion, rest);
        }

        static RuntimeMotionEvent[] CompileMotion(ChartJsonData data, NeckTechnique defaultTechnique, out HashSet<string> seenIds)
        {
            seenIds = new HashSet<string>(StringComparer.Ordinal);
            if (data.events == null)
                throw new ChartValidationException("Chart has no events collection.");
            var list = new List<RuntimeMotionEvent>(data.events.Count);

            for (var i = 0; i < data.events.Count; i++)
            {
                var e = data.events[i];

                if (double.IsNaN(e.time) || double.IsInfinity(e.time) || e.time < 0d)
                    throw new ChartValidationException($"Motion event {i} has invalid time {e.time}.");
                if (e.duration < 0d || double.IsNaN(e.duration) || double.IsInfinity(e.duration))
                    throw new ChartValidationException($"Motion event {i} has invalid duration {e.duration}.");

                var id = string.IsNullOrWhiteSpace(e.id) ? $"m{i:0000}" : e.id.Trim();
                if (!seenIds.Add(id))
                    throw new ChartValidationException($"Duplicate motion event id '{id}'.");

                var direction = ParseDirection(e, i);
                var technique = ParseTechnique(e.technique, defaultTechnique, $"event {i} technique");
                var trajectory = ParseTrajectory(e.trajectory, DefaultTrajectory(direction), $"event {i} trajectory");
                var modifier = ParseModifier(e.modifier, NeckModifier.None, $"event {i} modifier");
                var intensity = e.intensity <= 0f ? 1f : (e.intensity > 1f ? 1f : e.intensity);

                list.Add(new RuntimeMotionEvent(
                    id, e.time, technique, trajectory, direction, modifier, e.duration, intensity, e.finisherCandidate));
            }

            list.Sort(CompareMotion);
            return list.ToArray();
        }

        static RuntimeRestEvent[] CompileRest(ChartJsonData data, HashSet<string> seenIds)
        {
            if (data.rests == null || data.rests.Count == 0) return Array.Empty<RuntimeRestEvent>();

            var list = new List<RuntimeRestEvent>(data.rests.Count);
            for (var i = 0; i < data.rests.Count; i++)
            {
                var r = data.rests[i];
                if (double.IsNaN(r.time) || r.time < 0d)
                    throw new ChartValidationException($"Rest {i} has invalid start time {r.time}.");
                if (r.duration <= 0d || double.IsNaN(r.duration) || double.IsInfinity(r.duration))
                    throw new ChartValidationException($"Rest {i} has invalid duration {r.duration}.");
                if (r.settlingDuration < 0d || r.settlingDuration >= r.duration)
                    throw new ChartValidationException(
                        $"Rest {i} settlingDuration {r.settlingDuration} must be in [0, duration {r.duration}) so an evaluation phase exists.");

                var id = string.IsNullOrWhiteSpace(r.id) ? $"r{i:0000}" : r.id.Trim();
                if (!seenIds.Add(id))
                    throw new ChartValidationException($"Duplicate rest event id '{id}'.");

                list.Add(new RuntimeRestEvent(id, r.time, r.duration, r.settlingDuration));
            }

            list.Sort((a, b) =>
            {
                var byTime = a.StartTime.CompareTo(b.StartTime);
                return byTime != 0 ? byTime : string.CompareOrdinal(a.Id, b.Id);
            });

            // Reject overlapping rest intervals (ambiguous stillness ownership).
            for (var i = 1; i < list.Count; i++)
            {
                if (list[i].StartTime < list[i - 1].EndTime)
                    throw new ChartValidationException(
                        $"Rest '{list[i].Id}' overlaps rest '{list[i - 1].Id}'.");
            }
            return list.ToArray();
        }

        static int CompareMotion(RuntimeMotionEvent a, RuntimeMotionEvent b)
        {
            var byTime = a.Time.CompareTo(b.Time);
            return byTime != 0 ? byTime : string.CompareOrdinal(a.Id, b.Id);
        }

        static NeckTrajectory DefaultTrajectory(BangDirection d)
            => d.Axis() == BangAxis.Horizontal ? NeckTrajectory.Horizontal : NeckTrajectory.Vertical;

        static BangDirection ParseDirection(ChartJsonEvent e, int index)
        {
            // Explicit name wins if present.
            if (!string.IsNullOrWhiteSpace(e.directionName))
            {
                switch (e.directionName.Trim().ToLowerInvariant())
                {
                    case "left": return BangDirection.Left;
                    case "right": return BangDirection.Right;
                    case "up": return BangDirection.Up;
                    case "down": return BangDirection.Down;
                    default:
                        throw new ChartValidationException($"Event {index} has unknown directionName '{e.directionName}'.");
                }
            }

            // Legacy M0 integer encoding: -1 = Left, +1 = Right, 2 = Up, 3 = Down.
            // 0 is treated as "unset" (JsonUtility cannot distinguish a missing field from 0), so a
            // malformed event without an explicit direction fails rather than silently becoming Left.
            switch (e.direction)
            {
                case -1: return BangDirection.Left;
                case 1: return BangDirection.Right;
                case 2: return BangDirection.Up;
                case 3: return BangDirection.Down;
                case 0:
                    throw new ChartValidationException(
                        $"Event {index} has no direction (set 'directionName' or a non-zero legacy 'direction').");
                default:
                    throw new ChartValidationException($"Event {index} has unknown direction code {e.direction}.");
            }
        }

        static NeckTechnique ParseTechnique(string value, NeckTechnique fallback, string where)
        {
            if (string.IsNullOrWhiteSpace(value)) return fallback;
            switch (Normalize(value))
            {
                case "classic":
                case "classicbang": return NeckTechnique.Classic;
                case "half": return NeckTechnique.Half;
                case "deep": return NeckTechnique.Deep;
                case "whiplash": return NeckTechnique.Whiplash;
                case "windmill": return NeckTechnique.Windmill;
                default: throw new ChartValidationException($"Unknown technique '{value}' in {where}.");
            }
        }

        static NeckTrajectory ParseTrajectory(string value, NeckTrajectory fallback, string where)
        {
            if (string.IsNullOrWhiteSpace(value)) return fallback;
            switch (Normalize(value))
            {
                case "horizontal": return NeckTrajectory.Horizontal;
                case "vertical": return NeckTrajectory.Vertical;
                case "circular": return NeckTrajectory.Circular;
                case "centeredge": return NeckTrajectory.CenterEdge;
                default: throw new ChartValidationException($"Unknown trajectory '{value}' in {where}.");
            }
        }

        static NeckModifier ParseModifier(string value, NeckModifier fallback, string where)
        {
            if (string.IsNullOrWhiteSpace(value)) return fallback;
            switch (Normalize(value))
            {
                case "none": return NeckModifier.None;
                case "double": return NeckModifier.Double;
                case "hold": return NeckModifier.Hold;
                case "accent": return NeckModifier.Accent;
                case "burst": return NeckModifier.Burst;
                default: throw new ChartValidationException($"Unknown modifier '{value}' in {where}.");
            }
        }

        static string Normalize(string s) => s.Trim().ToLowerInvariant().Replace("_", "").Replace("-", "").Replace(" ", "");
    }
}
