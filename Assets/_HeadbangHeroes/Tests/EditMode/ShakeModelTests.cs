using HeadbangHeroes.Presentation;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class ShakeModelTests
    {
        [Test]
        public void Trauma_CapsAtOne()
        {
            var m = new ShakeModel();
            m.AddTrauma(0.8f);
            m.AddTrauma(0.8f);
            Assert.AreEqual(1f, m.Trauma, 1e-6f);
        }

        [Test]
        public void DecaysToZero()
        {
            var m = new ShakeModel(decayPerSecond: 4f, maxOffset: 20f);
            m.AddTrauma(1f);
            for (var i = 0; i < 200; i++) m.Advance(1f / 60f);
            Assert.AreEqual(0f, m.Trauma, 1e-6f);
            Assert.AreEqual(0f, m.Advance(1f / 60f), 1e-6f);
        }

        [Test]
        public void OffsetIsQuadraticInTrauma()
        {
            var m = new ShakeModel(decayPerSecond: 0.0001f, maxOffset: 100f);
            m.AddTrauma(0.5f);
            var offset = m.Advance(0f);   // dt 0 -> no decay
            Assert.AreEqual(0.25f * 100f, offset, 0.1f);   // 0.5^2 * 100 = 25
        }

        [Test]
        public void Reset_ZeroesTrauma()
        {
            var m = new ShakeModel();
            m.AddTrauma(1f);
            m.Reset();
            Assert.AreEqual(0f, m.Trauma, 1e-6f);
        }

        [Test]
        public void NegativeTrauma_Clamped()
        {
            var m = new ShakeModel();
            m.AddTrauma(-5f);
            Assert.AreEqual(0f, m.Trauma, 1e-6f);
        }
    }
}
