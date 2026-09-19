using HeadbangHeroes.Charts;
using HeadbangHeroes.Diagnostics;
using HeadbangHeroes.Gameplay;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class PlaytestDiagnosticsTests
    {
        static BangRecord Hit(PlaytestDiagnostics d, double signedMs, BangDirection dir = BangDirection.Left,
            Judgment j = Judgment.Perfect)
            => new BangRecord(d.NextSequence(), InputSource.Touch, true, 0, 0, 0, 0, dir,
                0, 0, 0, true, dir, 0, signedMs, true, j, BangFailure.Hit);

        static BangRecord Wrong(PlaytestDiagnostics d, BangDirection expected, BangDirection got)
            => new BangRecord(d.NextSequence(), InputSource.Touch, true, 0, 0, 0, 0, got,
                0, 0, 0, true, expected, 0, 0, true, Judgment.Miss, BangFailure.WrongDirection);

        static BangRecord Expired(PlaytestDiagnostics d, BangDirection dir)
            => new BangRecord(d.NextSequence(), InputSource.Unknown, false, 0, 0, 0, 0, dir,
                0, 0, -1, true, dir, 0, 0, false, Judgment.Miss, BangFailure.EventExpiredWithoutInput);

        [Test]
        public void EmptySession_ReportsNothingAndNoTimingStats()
        {
            var d = new PlaytestDiagnostics();
            Assert.AreEqual(0, d.Count);
            Assert.AreEqual(0, d.ConsumedCount());
            Assert.IsFalse(d.TimingStats(out _, out _, out _, out _, out _));
            StringAssert.Contains("Inputs: 0", d.BuildReport(0, 0));
        }

        [Test]
        public void AllHitSession_CountsAndConsumed()
        {
            var d = new PlaytestDiagnostics();
            for (var i = 0; i < 5; i++) d.Add(Hit(d, 10));
            Assert.AreEqual(5, d.Count);
            Assert.AreEqual(5, d.ConsumedCount());
            Assert.AreEqual(5, d.JudgmentCount(Judgment.Perfect));
            Assert.AreEqual(5, d.FailureCount(BangFailure.Hit));
            Assert.AreEqual(0, d.JudgmentCount(Judgment.Miss));
        }

        [Test]
        public void WrongDirectionHeavy_AggregatesConfusionMatrix()
        {
            var d = new PlaytestDiagnostics();
            d.Add(Wrong(d, BangDirection.Right, BangDirection.Down));
            d.Add(Wrong(d, BangDirection.Right, BangDirection.Down));
            d.Add(Wrong(d, BangDirection.Left, BangDirection.Down));
            d.Add(Hit(d, 5));
            var conf = d.DirectionConfusion();
            Assert.AreEqual(2, conf[(BangDirection.Right, BangDirection.Down)]);
            Assert.AreEqual(1, conf[(BangDirection.Left, BangDirection.Down)]);
            Assert.AreEqual(3, d.FailureCount(BangFailure.WrongDirection));
        }

        [Test]
        public void SignedTimingStats_ComputeMedianMeanEarlyLate()
        {
            var d = new PlaytestDiagnostics();
            // signed errors: -100, -50, +20, +200 (over consumed hits)
            d.Add(Hit(d, -100)); d.Add(Hit(d, -50)); d.Add(Hit(d, 20)); d.Add(Hit(d, 200));
            Assert.IsTrue(d.TimingStats(out var med, out var mean, out var mabs, out var early, out var late));
            Assert.AreEqual(2, early);
            Assert.AreEqual(2, late);
            Assert.AreEqual((-100 - 50 + 20 + 200) / 4.0, mean, 1e-9);   // mean = +17.5
            Assert.AreEqual((-50 + 20) / 2.0, med, 1e-9);                 // median of 4 = avg of middle two
            Assert.AreEqual((50 + 100) / 2.0, mabs, 1e-9);               // abs sorted 20,50,100,200 -> med 75
        }

        [Test]
        public void ExpiredWithoutInput_NotConsumed_NotInTiming()
        {
            var d = new PlaytestDiagnostics();
            d.Add(Expired(d, BangDirection.Up));
            d.Add(Hit(d, 30));
            Assert.AreEqual(1, d.FailureCount(BangFailure.EventExpiredWithoutInput));
            Assert.AreEqual(1, d.ConsumedCount());
            Assert.IsTrue(d.TimingStats(out _, out _, out _, out var early, out var late));
            Assert.AreEqual(0, early);
            Assert.AreEqual(1, late);   // only the +30 hit counts
        }

        [Test]
        public void Report_ContainsClockOffsets()
        {
            var d = new PlaytestDiagnostics();
            d.Add(Hit(d, 0));
            var report = d.BuildReport(outputLatencyMs: 90, calibrationMs: -40);
            StringAssert.Contains("output latency compensation: 90 ms", report);
            StringAssert.Contains("manual calibration:", report);
            StringAssert.Contains("effective combined offset:", report);
        }
    }
}
