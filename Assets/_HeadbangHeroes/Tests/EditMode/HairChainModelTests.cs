using HeadbangHeroes.Presentation;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class HairChainModelTests
    {
        const float Dt = 1f / 120f;

        static HairChainModel Long() => new HairChainModel(HairMotionTier.Long);

        static void Settle(HairChainModel m, float headAngle, int steps = 2000)
        {
            for (var i = 0; i < steps; i++) m.Update(headAngle, 0f, 0f, Dt);
        }

        [Test]
        public void TierSegmentCounts()
        {
            Assert.AreEqual(0, new HairChainModel(HairMotionTier.Bald).SegmentCount);
            Assert.AreEqual(1, new HairChainModel(HairMotionTier.Short).SegmentCount);
            Assert.AreEqual(4, new HairChainModel(HairMotionTier.Long).SegmentCount);
        }

        [Test]
        public void Bald_DoesNothing()
        {
            var m = new HairChainModel(HairMotionTier.Bald);
            m.Update(45f, 300f, 1f, Dt);   // must not throw / no segments
            Assert.AreEqual(0, m.SegmentCount);
        }

        [Test]
        public void SettlesToTheHeadAngle()
        {
            var m = Long();
            Settle(m, 30f);
            for (var i = 0; i < m.SegmentCount; i++)
                Assert.AreEqual(30f, m.SegmentAngle(i), 1.0f, $"segment {i} should settle at the head angle");
        }

        [Test]
        public void Lags_DeeperSegmentsTrailDuringASwing()
        {
            var m = Long();
            // Head snaps from 0 to +40 with high velocity; sample shortly after — deeper segments trail.
            for (var i = 0; i < 8; i++) m.Update(40f, 400f, 0f, Dt);
            // Segment 0 (near head) should be further along than the tip during the transient.
            Assert.Greater(m.SegmentAngle(0), m.SegmentAngle(m.SegmentCount - 1),
                "the segment nearest the head leads; the tip lags");
        }

        [Test]
        public void BendClamp_TipCannotExceedMaxBendFromParentChain()
        {
            var m = Long();
            // Extreme instantaneous head swing; even so each segment stays within maxBend of its parent.
            for (var i = 0; i < 4; i++) m.Update(180f, 5000f, 1f, Dt);
            var maxBend = HairMotionTier.Long.MaxBend * 1.2f + 0.5f; // +HYPE exaggeration + epsilon
            var parent = 180f;
            for (var i = 0; i < m.SegmentCount; i++)
            {
                Assert.LessOrEqual(System.Math.Abs(m.SegmentAngle(i) - parent), maxBend,
                    $"segment {i} exceeded max bend from its parent");
                parent = m.SegmentAngle(i);
            }
        }

        [Test]
        public void Deterministic_SameInputsSameOutput()
        {
            var a = Long(); var b = Long();
            for (var i = 0; i < 50; i++)
            {
                var head = Wave(i);
                a.Update(head, 200f, 0.5f, Dt);
                b.Update(head, 200f, 0.5f, Dt);
            }
            for (var i = 0; i < a.SegmentCount; i++)
                Assert.AreEqual(a.SegmentAngle(i), b.SegmentAngle(i), 1e-4f);
        }

        static float Wave(int i) => 30f * (float)System.Math.Sin(i * 0.3);

        [Test]
        public void Reset_ReturnsToZero()
        {
            var m = Long();
            Settle(m, 40f);
            m.Reset();
            for (var i = 0; i < m.SegmentCount; i++)
                Assert.AreEqual(0f, m.SegmentAngle(i), 1e-6f);
        }

        [Test]
        public void Hype_IsBoundedExaggeration_NotRunaway()
        {
            // With HYPE=1 the settle target is still the head angle (exaggeration affects transient
            // bend/overshoot, not the resting position). Confirms HYPE cannot run the hair away.
            var m = Long();
            Settle(m, 25f);        // settle without hype
            for (var i = 0; i < 2000; i++) m.Update(25f, 0f, 1f, Dt); // now with full hype, same head
            for (var i = 0; i < m.SegmentCount; i++)
                Assert.AreEqual(25f, m.SegmentAngle(i), 1.0f, "HYPE must not shift the resting angle");
        }
    }
}
