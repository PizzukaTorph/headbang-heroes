using System.Collections.Generic;
using HeadbangHeroes.Charts;
using HeadbangHeroes.Gameplay;
using HeadbangHeroes.Gameplay.Timing;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class EventMatcherTests
    {
        static readonly TimingConfig Cfg = TimingConfig.Default;

        static MotionCandidate C(int id, double t, BangDirection d) => new MotionCandidate(id, t, d);
        static BangInput In(BangDirection d, double t) => new BangInput(d, t);

        [Test]
        public void CompatibleInWindow_ResolvesNearestAsHit()
        {
            var candidates = new List<MotionCandidate> { C(0, 10.0, BangDirection.Left) };
            var r = EventMatcher.Match(In(BangDirection.Left, 10.0), candidates, Cfg);
            Assert.AreEqual(MatchKind.Hit, r.Kind);
            Assert.AreEqual(0, r.MatchedId);
            Assert.AreEqual(Judgment.Perfect, r.Judgment);
        }

        [Test]
        public void TooEarlyForAll_ConsumesNothing()
        {
            var candidates = new List<MotionCandidate> { C(0, 10.0, BangDirection.Left) };
            // Input well before the GOOD early edge of the only candidate.
            var r = EventMatcher.Match(In(BangDirection.Left, 10.0 - (Cfg.GoodWindow + 0.05)), candidates, Cfg);
            Assert.AreEqual(MatchKind.TooEarly, r.Kind);
            Assert.IsFalse(r.Consumed);
            Assert.AreEqual(-1, r.MatchedId);
        }

        [Test]
        public void WrongDirectionInWindow_ConsumesMiss()
        {
            var candidates = new List<MotionCandidate> { C(0, 10.0, BangDirection.Left) };
            var r = EventMatcher.Match(In(BangDirection.Right, 10.0), candidates, Cfg);
            Assert.AreEqual(MatchKind.WrongConsumedMiss, r.Kind);
            Assert.AreEqual(0, r.MatchedId);
            Assert.AreEqual(Judgment.Miss, r.Judgment);
        }

        [Test]
        public void TwoOverlappingCompatible_NearestWins()
        {
            // Two Left events close together; input at 10.03 is nearer to the second (10.05) than first (10.00).
            var candidates = new List<MotionCandidate>
            {
                C(0, 10.00, BangDirection.Left),
                C(1, 10.05, BangDirection.Left),
            };
            var r = EventMatcher.Match(In(BangDirection.Left, 10.03), candidates, Cfg);
            Assert.AreEqual(MatchKind.Hit, r.Kind);
            Assert.AreEqual(1, r.MatchedId, "nearest-by-|error| candidate must win");
        }

        [Test]
        public void EqualErrorTie_BreaksToEarlierTimeThenId()
        {
            // Input exactly between two compatible events equidistant in time: earlier time wins.
            var candidates = new List<MotionCandidate>
            {
                C(5, 10.10, BangDirection.Left),
                C(2, 10.00, BangDirection.Left),
            };
            var r = EventMatcher.Match(In(BangDirection.Left, 10.05), candidates, Cfg);
            Assert.AreEqual(MatchKind.Hit, r.Kind);
            Assert.AreEqual(2, r.MatchedId, "equal |error| ties break to earlier authored time");
        }

        [Test]
        public void CompatiblePreferredOverNearerWrongDirection()
        {
            // A wrong-direction event is nearer in time, but a compatible one is still judgeable.
            var candidates = new List<MotionCandidate>
            {
                C(0, 10.00, BangDirection.Right), // nearer, wrong direction
                C(1, 10.06, BangDirection.Left),  // farther, compatible, still within GOOD of input@10.02
            };
            // input@10.02: error vs id0 = +0.02 (wrong dir), vs id1 = -0.04 (compatible, within Good 0.12)
            var r = EventMatcher.Match(In(BangDirection.Left, 10.02), candidates, Cfg);
            Assert.AreEqual(MatchKind.Hit, r.Kind);
            Assert.AreEqual(1, r.MatchedId, "a compatible judgeable candidate is preferred over a nearer wrong-direction one");
        }

        [Test]
        public void LateWithinExpiry_StillResolves()
        {
            var candidates = new List<MotionCandidate> { C(0, 10.0, BangDirection.Left) };
            var r = EventMatcher.Match(In(BangDirection.Left, 10.0 + Cfg.GoodWindow), candidates, Cfg);
            Assert.AreEqual(MatchKind.Hit, r.Kind);
            Assert.AreEqual(Judgment.Good, r.Judgment);
        }

        [Test]
        public void PastExpiry_NotJudgeable()
        {
            var candidates = new List<MotionCandidate> { C(0, 10.0, BangDirection.Left) };
            var r = EventMatcher.Match(In(BangDirection.Left, 10.0 + Cfg.LateExpiry + 0.01), candidates, Cfg);
            Assert.AreEqual(MatchKind.TooEarly, r.Kind, "past the late edge there is no judgeable candidate");
            Assert.IsFalse(r.Consumed);
        }

        [Test]
        public void EmptyCandidateSet_ConsumesNothing()
        {
            var r = EventMatcher.Match(In(BangDirection.Left, 10.0), new List<MotionCandidate>(), Cfg);
            Assert.IsFalse(r.Consumed);
        }
    }
}
