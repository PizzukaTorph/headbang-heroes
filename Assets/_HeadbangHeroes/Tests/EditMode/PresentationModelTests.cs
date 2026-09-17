using HeadbangHeroes.Presentation;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class PresentationModelTests
    {
        // ---- BodyLeanModel ----

        [Test]
        public void BodyLean_EasesTowardFollowFractionOfNeckAngle()
        {
            var m = new BodyLeanModel(0.5f, 12f);
            for (var i = 0; i < 500; i++) m.Update(40f, 0f, 1f / 60f);
            Assert.AreEqual(20f, m.LeanZ, 0.5f, "body settles at followFraction * neck angle");
        }

        [Test]
        public void BodyLean_Deterministic()
        {
            var a = BodyLeanModel.CreateDefault();
            var b = BodyLeanModel.CreateDefault();
            for (var i = 0; i < 30; i++) { a.Update(30f, 10f, 0.016f); b.Update(30f, 10f, 0.016f); }
            Assert.AreEqual(a.LeanZ, b.LeanZ, 1e-6f);
            Assert.AreEqual(a.LeanX, b.LeanX, 1e-6f);
        }

        // ---- HairStrandModel ----

        [Test]
        public void Hair_LagsThenCatchesUpToHead()
        {
            var m = HairStrandModel.CreateDefault();
            m.Update(40f, 0.016f);
            Assert.Less(m.Angle, 40f, "hair lags the head on a sudden swing");
            for (var i = 0; i < 500; i++) m.Update(40f, 0.016f);
            Assert.AreEqual(40f, m.Angle, 0.5f, "hair settles onto a held head angle");
        }

        [Test]
        public void Hair_StaysWithinMaxLagOfHead()
        {
            var m = new HairStrandModel(90f, 9f, 30f);
            // Whip the head back and forth hard; the strand must never trail beyond maxLag.
            for (var i = 0; i < 200; i++)
            {
                var head = (i % 2 == 0) ? 42f : -42f;
                m.Update(head, 0.016f);
                Assert.LessOrEqual(System.Math.Abs(m.Angle - head), 30f + 1e-3f);
            }
        }

        [Test]
        public void Hair_Deterministic()
        {
            var a = HairStrandModel.CreateDefault();
            var b = HairStrandModel.CreateDefault();
            for (var i = 0; i < 40; i++) { a.Update(35f, 0.016f); b.Update(35f, 0.016f); }
            Assert.AreEqual(a.Angle, b.Angle, 1e-6f);
        }

        // ---- VenueIntensityModel ----

        [Test]
        public void Venue_RisesWithHypeAndTheBang()
        {
            var m = VenueIntensityModel.CreateDefault(reducedMotion: false);
            for (var i = 0; i < 300; i++) m.Update(1f, theBangActive: true, 0.016f);
            Assert.Greater(m.Intensity, 0.5f, "full HYPE + THE BANG drives a strong intensity");
        }

        [Test]
        public void Venue_ReducedMotion_CapsIntensity()
        {
            var m = VenueIntensityModel.CreateDefault(reducedMotion: true);
            for (var i = 0; i < 300; i++) m.Update(1f, theBangActive: true, 0.016f);
            Assert.LessOrEqual(m.Intensity, 0.36f, "reduced motion caps the reactive range");
        }

        [Test]
        public void Venue_FinisherPulse_DecaysBackDown()
        {
            var m = VenueIntensityModel.CreateDefault(reducedMotion: false);
            m.PulseFinisher();
            m.Update(0f, false, 0.016f);
            var peak = m.Intensity;
            for (var i = 0; i < 300; i++) m.Update(0f, false, 0.016f);
            Assert.Less(m.Intensity, peak, "the finisher pulse decays");
        }

        [Test]
        public void Venue_Deterministic()
        {
            var a = VenueIntensityModel.CreateDefault(false);
            var b = VenueIntensityModel.CreateDefault(false);
            for (var i = 0; i < 50; i++) { a.Update(0.5f, false, 0.016f); b.Update(0.5f, false, 0.016f); }
            Assert.AreEqual(a.Intensity, b.Intensity, 1e-6f);
        }

        // ---- Accessibility never changes inputs, only presentation ----

        [Test]
        public void ReducedMotion_DoesNotChangeInputSignals()
        {
            // Two venues fed identical signals; only the reduced-motion one is capped. The signals
            // (hype/bang) are inputs, never mutated by the presenter.
            var normal = VenueIntensityModel.CreateDefault(false);
            var reduced = VenueIntensityModel.CreateDefault(true);
            for (var i = 0; i < 300; i++)
            {
                normal.Update(1f, true, 0.016f);
                reduced.Update(1f, true, 0.016f);
            }
            Assert.Greater(normal.Intensity, reduced.Intensity,
                "reduced motion only lowers presentation intensity; it never touches the source signals");
        }
    }
}
