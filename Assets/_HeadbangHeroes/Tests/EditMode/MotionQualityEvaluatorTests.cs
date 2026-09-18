using HeadbangHeroes.Charts;
using HeadbangHeroes.Gameplay.Neck;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class MotionQualityEvaluatorTests
    {
        static readonly MotionQualityConfig Cfg = MotionQualityConfig.Default;

        static NeckMotionSnapshot Snap(float velocity, float travel, bool prepared) =>
            new NeckMotionSnapshot(BangAxis.Horizontal, 0f, velocity, travel, System.Math.Abs(velocity), prepared, 0);

        [Test]
        public void FirstBangUnprepared_ReturnsSetupQualityWithoutFabricatedTravel()
        {
            var r = MotionQualityEvaluator.Evaluate(Snap(0f, 0f, prepared: false), Cfg);
            Assert.IsTrue(r.WasSetup);
            Assert.AreEqual(Cfg.SetupQuality, r.Quality, 1e-4f);
            Assert.AreEqual(0f, r.TravelComponent, 1e-4f, "no preceding travel fabricated for setup bang");
        }

        [Test]
        public void QualityIsWithinUnitRange()
        {
            var r = MotionQualityEvaluator.Evaluate(Snap(500f, 100f, prepared: true), Cfg);
            Assert.GreaterOrEqual(r.Quality, 0f);
            Assert.LessOrEqual(r.Quality, 1f);
        }

        [Test]
        public void MoreTravelAndSpeed_RaiseQuality()
        {
            var low = MotionQualityEvaluator.Evaluate(Snap(20f, 2f, prepared: true), Cfg);
            var high = MotionQualityEvaluator.Evaluate(Snap(220f, 24f, prepared: true), Cfg);
            Assert.Greater(high.Quality, low.Quality);
        }

        [Test]
        public void Deterministic_SameSnapshotSameResult()
        {
            var a = MotionQualityEvaluator.Evaluate(Snap(150f, 12f, prepared: true), Cfg);
            var b = MotionQualityEvaluator.Evaluate(Snap(150f, 12f, prepared: true), Cfg);
            Assert.AreEqual(a.Quality, b.Quality, 1e-6f);
        }
    }
}
