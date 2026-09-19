using System.Collections.Generic;
using System.Text;
using HeadbangHeroes.Charts;
using HeadbangHeroes.Gameplay;

namespace HeadbangHeroes.Diagnostics
{
    /// <summary>Where a physical bang came from (for keyboard-vs-touch A/B).</summary>
    public enum InputSource { Keyboard, Mouse, Touch, Unknown }

    /// <summary>
    /// Diagnostic classification of a bang, DERIVED OUTSIDE the authoritative scoring path. It is a
    /// finer-grained taxonomy than the gameplay Judgment so we can tell WHERE vs WHEN failures apart.
    /// It never feeds back into scoring/timing.
    /// </summary>
    public enum BangFailure
    {
        Hit,
        WrongDirection,
        TooEarly,
        TooLateOrExpired,
        NoActiveCandidate,
        EventExpiredWithoutInput,
    }

    /// <summary>One physical bang (or one expired event) as observed for diagnostics. Pure data.</summary>
    public readonly struct BangRecord
    {
        public readonly int Sequence;
        public readonly InputSource Source;
        public readonly bool HasScreenPosition;
        public readonly float ScreenX, ScreenY;         // pixels (when available)
        public readonly float NormX, NormY;             // normalized -1..1 from screen centre
        public readonly BangDirection ResolvedDirection;
        public readonly double InputDspTime;
        public readonly double InputSongTime;
        public readonly int EventId;                    // resolver slot / -1
        public readonly bool HasExpected;
        public readonly BangDirection ExpectedDirection;
        public readonly double EventSongTime;
        public readonly double SignedErrorMs;
        public readonly bool Consumed;
        public readonly Judgment Judgment;
        public readonly BangFailure Failure;

        public double AbsErrorMs => SignedErrorMs < 0 ? -SignedErrorMs : SignedErrorMs;

        public BangRecord(int sequence, InputSource source, bool hasScreenPos, float sx, float sy,
            float nx, float ny, BangDirection resolvedDir, double inputDsp, double inputSong,
            int eventId, bool hasExpected, BangDirection expectedDir, double eventSong,
            double signedErrorMs, bool consumed, Judgment judgment, BangFailure failure)
        {
            Sequence = sequence; Source = source; HasScreenPosition = hasScreenPos;
            ScreenX = sx; ScreenY = sy; NormX = nx; NormY = ny;
            ResolvedDirection = resolvedDir; InputDspTime = inputDsp; InputSongTime = inputSong;
            EventId = eventId; HasExpected = hasExpected; ExpectedDirection = expectedDir;
            EventSongTime = eventSong; SignedErrorMs = signedErrorMs; Consumed = consumed;
            Judgment = judgment; Failure = failure;
        }
    }

    /// <summary>
    /// Accumulates <see cref="BangRecord"/>s for a session and produces a compact report. Pure C#:
    /// no Unity refs, no scoring authority. Dev/diagnostic only.
    /// </summary>
    public sealed class PlaytestDiagnostics
    {
        readonly List<BangRecord> records = new(256);
        int sequence;

        public IReadOnlyList<BangRecord> Records => records;
        public int Count => records.Count;

        public void Reset() { records.Clear(); sequence = 0; }

        public int NextSequence() => ++sequence;

        public void Add(in BangRecord r) => records.Add(r);

        // ---- Aggregations (pure, unit-testable) ----

        public int ConsumedCount()
        {
            var n = 0;
            foreach (var r in records) if (r.Consumed) n++;
            return n;
        }

        public int JudgmentCount(Judgment j)
        {
            var n = 0;
            foreach (var r in records) if (r.Consumed && r.Judgment == j) n++;
            return n;
        }

        public int FailureCount(BangFailure f)
        {
            var n = 0;
            foreach (var r in records) if (r.Failure == f) n++;
            return n;
        }

        /// <summary>Direction-confusion tallies: (expected, resolved) -> count, only for wrong-direction bangs.</summary>
        public Dictionary<(BangDirection expected, BangDirection got), int> DirectionConfusion()
        {
            var map = new Dictionary<(BangDirection, BangDirection), int>();
            foreach (var r in records)
            {
                if (r.Failure != BangFailure.WrongDirection || !r.HasExpected) continue;
                var key = (r.ExpectedDirection, r.ResolvedDirection);
                map.TryGetValue(key, out var c);
                map[key] = c + 1;
            }
            return map;
        }

        /// <summary>Signed timing stats (ms) over CONSUMED bangs only (real timing evidence).</summary>
        public bool TimingStats(out double medianSigned, out double meanSigned, out double medianAbs, out int early, out int late)
        {
            medianSigned = meanSigned = medianAbs = 0; early = late = 0;
            var signed = new List<double>();
            var abs = new List<double>();
            foreach (var r in records)
            {
                if (!r.Consumed) continue;
                signed.Add(r.SignedErrorMs);
                abs.Add(r.AbsErrorMs);
                if (r.SignedErrorMs < 0) early++; else late++;
            }
            if (signed.Count == 0) return false;
            double sum = 0; foreach (var v in signed) sum += v;
            meanSigned = sum / signed.Count;
            signed.Sort(); abs.Sort();
            medianSigned = Median(signed);
            medianAbs = Median(abs);
            return true;
        }

        static double Median(List<double> sorted)
        {
            var n = sorted.Count;
            if (n == 0) return 0;
            return (n % 2 == 1) ? sorted[n / 2] : 0.5 * (sorted[n / 2 - 1] + sorted[n / 2]);
        }

        /// <summary>Builds the compact PLAYTEST DIAGNOSTICS report. Clock offsets passed in (owned by AudioClock).</summary>
        public string BuildReport(double outputLatencyMs, double calibrationMs, InputSource? filterSource = null)
        {
            var sb = new StringBuilder(512);
            sb.Append("PLAYTEST DIAGNOSTICS");
            if (filterSource.HasValue) sb.Append(" [").Append(filterSource.Value).Append(']');
            sb.Append('\n');

            sb.Append("\nInputs: ").Append(records.Count);
            sb.Append("\nConsumed: ").Append(ConsumedCount()).Append("\n");

            sb.Append("\nJudgments:");
            sb.Append("\n  Perfect ").Append(JudgmentCount(Judgment.Perfect));
            sb.Append("\n  Great   ").Append(JudgmentCount(Judgment.Great));
            sb.Append("\n  Good    ").Append(JudgmentCount(Judgment.Good));
            sb.Append("\n  Well    ").Append(JudgmentCount(Judgment.Well));
            sb.Append("\n  Miss    ").Append(JudgmentCount(Judgment.Miss)).Append("\n");

            sb.Append("\nFailure:");
            sb.Append("\n  Hit                  ").Append(FailureCount(BangFailure.Hit));
            sb.Append("\n  Wrong direction      ").Append(FailureCount(BangFailure.WrongDirection));
            sb.Append("\n  Too early            ").Append(FailureCount(BangFailure.TooEarly));
            sb.Append("\n  Too late/expired     ").Append(FailureCount(BangFailure.TooLateOrExpired));
            sb.Append("\n  No active candidate  ").Append(FailureCount(BangFailure.NoActiveCandidate));
            sb.Append("\n  Expired w/o input    ").Append(FailureCount(BangFailure.EventExpiredWithoutInput)).Append("\n");

            var conf = DirectionConfusion();
            if (conf.Count > 0)
            {
                sb.Append("\nDirection confusion:");
                foreach (var kv in conf)
                    sb.Append("\n  Expected ").Append(kv.Key.expected).Append(" -> got ").Append(kv.Key.got).Append(": ").Append(kv.Value);
                sb.Append('\n');
            }

            if (TimingStats(out var ms, out var mean, out var mabs, out var early, out var late))
            {
                sb.Append("\nTiming (consumed):");
                sb.Append("\n  median signed error: ").Append(ms.ToString("+0;-0;0")).Append(" ms");
                sb.Append("\n  mean signed error:   ").Append(mean.ToString("+0;-0;0")).Append(" ms");
                sb.Append("\n  median abs error:    ").Append(mabs.ToString("0")).Append(" ms");
                sb.Append("\n  Early: ").Append(early).Append("   Late: ").Append(late).Append("\n");
            }

            sb.Append("\nClock:");
            sb.Append("\n  output latency compensation: ").Append(outputLatencyMs.ToString("0")).Append(" ms");
            sb.Append("\n  manual calibration:          ").Append(calibrationMs.ToString("+0;-0;0")).Append(" ms");
            sb.Append("\n  effective combined offset:   ").Append((outputLatencyMs + calibrationMs).ToString("+0;-0;0")).Append(" ms");
            return sb.ToString();
        }
    }
}
