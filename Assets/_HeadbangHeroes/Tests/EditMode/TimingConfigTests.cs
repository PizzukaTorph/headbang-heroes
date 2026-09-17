using HeadbangHeroes.Gameplay;
using HeadbangHeroes.Gameplay.Timing;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class TimingConfigTests
    {
        static readonly TimingConfig Cfg = TimingConfig.Default;

        [Test]
        public void ExactlyPerfectBoundary_IsPerfect()
        {
            Assert.AreEqual(Judgment.Perfect, Cfg.Classify(Cfg.PerfectWindow));
            Assert.AreEqual(Judgment.Perfect, Cfg.Classify(-Cfg.PerfectWindow));
        }

        [Test]
        public void JustOutsidePerfect_IsGreat()
        {
            var e = Cfg.PerfectWindow + 1e-4;
            Assert.AreEqual(Judgment.Great, Cfg.Classify(e));
            Assert.AreEqual(Judgment.Great, Cfg.Classify(-e));
        }

        [Test]
        public void ExactlyGreatBoundary_IsGreat()
        {
            Assert.AreEqual(Judgment.Great, Cfg.Classify(Cfg.GreatWindow));
            Assert.AreEqual(Judgment.Great, Cfg.Classify(-Cfg.GreatWindow));
        }

        [Test]
        public void JustOutsideGreat_IsGood()
        {
            var e = Cfg.GreatWindow + 1e-4;
            Assert.AreEqual(Judgment.Good, Cfg.Classify(e));
        }

        [Test]
        public void ExactlyGoodBoundary_IsGood()
        {
            Assert.AreEqual(Judgment.Good, Cfg.Classify(Cfg.GoodWindow));
            Assert.AreEqual(Judgment.Good, Cfg.Classify(-Cfg.GoodWindow));
        }

        [Test]
        public void OutsideGood_IsMiss()
        {
            var e = Cfg.GoodWindow + 1e-4;
            Assert.AreEqual(Judgment.Miss, Cfg.Classify(e));
            Assert.AreEqual(Judgment.Miss, Cfg.Classify(-e));
        }

        [Test]
        public void CustomConfig_WindowsAreDataDriven()
        {
            var cfg = new TimingConfig(0.02, 0.05, 0.10, 0.8, 0.10);
            Assert.AreEqual(Judgment.Perfect, cfg.Classify(0.02));
            Assert.AreEqual(Judgment.Great, cfg.Classify(0.05));
            Assert.AreEqual(Judgment.Good, cfg.Classify(0.10));
            Assert.AreEqual(Judgment.Miss, cfg.Classify(0.11));
        }
    }
}
