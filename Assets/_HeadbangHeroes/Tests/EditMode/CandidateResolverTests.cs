using System.Collections.Generic;
using HeadbangHeroes.Charts;
using HeadbangHeroes.Charts.Runtime;
using HeadbangHeroes.Gameplay;
using HeadbangHeroes.Gameplay.Timing;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class CandidateResolverTests
    {
        static readonly TimingConfig Cfg = TimingConfig.Default;

        static RuntimeMotionEvent Ev(string id, double t, BangDirection d) =>
            new RuntimeMotionEvent(id, t, NeckTechnique.Classic, NeckTrajectory.Horizontal, d, NeckModifier.None, 0d, 1f, false);

        static BangInput In(BangDirection d, double t) => new BangInput(d, t);

        // Ordered motion events (the compiler guarantees order; tests pass pre-ordered lists).
        static CandidateResolver Resolver(params RuntimeMotionEvent[] events) =>
            new CandidateResolver(events, Cfg);

        [Test]
        public void Resolve_MarksEventResolvedOnce_CannotBeConsumedAgain()
        {
            var r = Resolver(Ev("m0", 10.0, BangDirection.Left));
            var buf = new List<MotionCandidate>();

            Assert.IsTrue(r.Resolve(In(BangDirection.Left, 10.0), buf, out var first));
            Assert.AreEqual(MatchKind.Hit, first.Kind);

            Assert.IsFalse(r.Resolve(In(BangDirection.Left, 10.0), buf, out var second));
            Assert.IsFalse(second.Consumed);
        }

        [Test]
        public void Expire_FiresExactlyOncePerEvent()
        {
            var r = Resolver(Ev("m0", 10.0, BangDirection.Left), Ev("m1", 10.5, BangDirection.Right));
            var expired = new List<int>();

            r.Expire(20.0, slot => expired.Add(slot));
            r.Expire(30.0, slot => expired.Add(slot));

            Assert.AreEqual(2, expired.Count, "each event expires exactly once across repeated passes");
        }

        [Test]
        public void ExpiredEvent_CannotBeResolvedAfterwards()
        {
            var r = Resolver(Ev("m0", 10.0, BangDirection.Left));
            var buf = new List<MotionCandidate>();

            r.Expire(11.0, _ => { });
            Assert.IsFalse(r.Resolve(In(BangDirection.Left, 11.0), buf, out var res));
            Assert.IsFalse(res.Consumed, "an expired event cannot be resurrected by a later input");
        }

        [Test]
        public void ResolvedEvent_NeverEntersCandidateBuffer()
        {
            var r = Resolver(Ev("m0", 10.0, BangDirection.Left), Ev("m1", 10.03, BangDirection.Left));
            var buf = new List<MotionCandidate>();

            r.Resolve(In(BangDirection.Left, 10.0), buf, out _);   // resolves nearest (10.0)
            r.Resolve(In(BangDirection.Left, 10.02), buf, out var res2);
            foreach (var c in buf)
                Assert.That(c.SongTime, Is.Not.EqualTo(10.0).Within(1e-9), "resolved event must not reappear as a candidate");
            Assert.IsTrue(res2.Consumed);
        }

        [Test]
        public void EarlyBetweenGoodAndWell_ConsumesAsWell()
        {
            // Regression: the candidate buffer filter must use the WELL early edge, not GOOD.
            // A tap earlier than GoodWindow but within WellWindow must consume the cue as WELL,
            // instead of being dropped as no-consume and later expiring as MISS.
            var r = Resolver(Ev("m0", 10.0, BangDirection.Left));
            var buf = new List<MotionCandidate>();

            var earlyError = (Cfg.GoodWindow + Cfg.WellWindow) / 2.0;   // between the two edges
            Assert.IsTrue(r.Resolve(In(BangDirection.Left, 10.0 - earlyError), buf, out var res),
                "an early WELL tap must consume the cue");
            Assert.AreEqual(MatchKind.Hit, res.Kind);
            Assert.AreEqual(Judgment.Well, res.Judgment);
        }

        [Test]
        public void EarlierThanWellEdge_ConsumesNothing()
        {
            var r = Resolver(Ev("m0", 10.0, BangDirection.Left));
            var buf = new List<MotionCandidate>();

            Assert.IsFalse(r.Resolve(In(BangDirection.Left, 10.0 - (Cfg.WellWindow + 0.05)), buf, out var res),
                "beyond the WELL early edge nothing is judgeable");
            Assert.IsFalse(res.Consumed);
        }

        [Test]
        public void NextUnresolvedTime_IsEarliest()
        {
            var r = Resolver(Ev("m0", 10.0, BangDirection.Left), Ev("m1", 20.0, BangDirection.Right));
            Assert.AreEqual(10.0, r.NextUnresolvedTime(), 1e-9);
        }

        [Test]
        public void EarliestUnresolvedWithin_RespectsLeadHorizon()
        {
            var r = Resolver(Ev("m0", 10.0, BangDirection.Left));
            Assert.AreEqual(-1, r.EarliestUnresolvedWithin(now: 8.0, leadSeconds: 1.0), "outside lead horizon");
            Assert.AreEqual(0, r.EarliestUnresolvedWithin(now: 9.5, leadSeconds: 1.0), "inside lead horizon");
        }
    }
}
