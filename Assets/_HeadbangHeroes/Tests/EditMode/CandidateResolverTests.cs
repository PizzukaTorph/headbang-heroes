using System.Collections.Generic;
using HeadbangHeroes.Charts;
using HeadbangHeroes.Gameplay;
using HeadbangHeroes.Gameplay.Timing;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class CandidateResolverTests
    {
        static readonly TimingConfig Cfg = TimingConfig.Default;

        static ChartEvent Ev(double t, BangDirection d) =>
            new ChartEvent { time = t, direction = d, intensity = 1f };

        static BangInput In(BangDirection d, double t) => new BangInput(d, t);

        static CandidateResolver Resolver(params ChartEvent[] events) =>
            new CandidateResolver(events, Cfg);

        [Test]
        public void Resolve_MarksEventResolvedOnce_CannotBeConsumedAgain()
        {
            var r = Resolver(Ev(10.0, BangDirection.Left));
            var buf = new List<MotionCandidate>();

            Assert.IsTrue(r.Resolve(In(BangDirection.Left, 10.0), buf, out var first));
            Assert.AreEqual(MatchKind.Hit, first.Kind);

            // A second compatible input at the same time finds no unresolved candidate.
            Assert.IsFalse(r.Resolve(In(BangDirection.Left, 10.0), buf, out var second));
            Assert.IsFalse(second.Consumed);
        }

        [Test]
        public void Expire_FiresExactlyOncePerEvent()
        {
            var r = Resolver(Ev(10.0, BangDirection.Left), Ev(10.5, BangDirection.Right));
            var expiredIds = new List<int>();

            // First expiry pass well past both late edges.
            r.Expire(20.0, c => expiredIds.Add(c.Id));
            // Second pass must not fire again.
            r.Expire(30.0, c => expiredIds.Add(c.Id));

            Assert.AreEqual(2, expiredIds.Count, "each event expires exactly once across repeated passes");
        }

        [Test]
        public void ExpiredEvent_CannotBeResolvedAfterwards()
        {
            var r = Resolver(Ev(10.0, BangDirection.Left));
            var buf = new List<MotionCandidate>();

            r.Expire(11.0, _ => { }); // past late edge -> expired
            Assert.IsFalse(r.Resolve(In(BangDirection.Left, 11.0), buf, out var res));
            Assert.IsFalse(res.Consumed, "an expired event cannot be resurrected by a later input");
        }

        [Test]
        public void ResolvedEvent_NeverEntersCandidateBuffer()
        {
            var r = Resolver(Ev(10.0, BangDirection.Left), Ev(10.03, BangDirection.Left));
            var buf = new List<MotionCandidate>();

            r.Resolve(In(BangDirection.Left, 10.0), buf, out _);   // resolves nearest (10.0)
            // Next input near 10.0 should only see the still-unresolved 10.03 event.
            r.Resolve(In(BangDirection.Left, 10.02), buf, out var res2);
            foreach (var c in buf)
                Assert.AreNotEqual(10.0, c.SongTime, 1e-9, "resolved event must not reappear as a candidate");
            Assert.IsTrue(res2.Consumed);
        }

        [Test]
        public void EventsAreSortedByTime_NextUnresolvedIsEarliest()
        {
            // Deliberately unsorted input.
            var r = Resolver(Ev(30.0, BangDirection.Left), Ev(10.0, BangDirection.Right), Ev(20.0, BangDirection.Up));
            Assert.AreEqual(10.0, r.NextUnresolvedTime(), 1e-9, "unsorted chart is sorted internally");
        }

        [Test]
        public void EarliestUnresolvedWithin_RespectsLeadHorizon()
        {
            var r = Resolver(Ev(10.0, BangDirection.Left));
            Assert.AreEqual(-1, r.EarliestUnresolvedWithin(now: 8.0, leadSeconds: 1.0), "outside lead horizon");
            Assert.AreEqual(0, r.EarliestUnresolvedWithin(now: 9.5, leadSeconds: 1.0), "inside lead horizon");
        }

        [Test]
        public void OriginalIndex_MapsSortedSlotBackToAuthoredEvent()
        {
            var r = Resolver(Ev(30.0, BangDirection.Left), Ev(10.0, BangDirection.Right));
            // Slot 0 is the earliest (t=10.0) which was authored at index 1.
            Assert.AreEqual(1, r.OriginalIndex(0));
            Assert.AreEqual(0, r.OriginalIndex(1));
        }
    }
}
