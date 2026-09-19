using HeadbangHeroes.UI;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class SectorPulseShapeTests
    {
        const double Approach = 0.9;
        const double HitWindow = 0.19;
        const float Start = 0.45f;
        const float Peak = 1.3f;

        static void Shape(double remaining, out float scale, out float tone)
            => SectorPulseCue.PeakShape(remaining, Approach, HitWindow, Start, Peak, null, out scale, out tone);

        [Test]
        public void PeakScaleIsAtTheEvent_NotBeforeOrAfter()
        {
            Shape(0.0, out var atEvent, out _);
            Shape(0.07, out var before, out _);
            Shape(-0.07, out var after, out _);
            Assert.AreEqual(Peak, atEvent, 1e-4f, "scale peaks exactly at remaining=0");
            Assert.Less(before, atEvent, "before the event the pulse has not yet peaked");
            Assert.Less(after, atEvent, "after the event the pulse has fallen off");
        }

        [Test]
        public void NoPlateau_ScaleStrictlyDecreasesEitherSideOfZero()
        {
            Shape(0.0, out var s0, out _);
            Shape(0.03, out var sBefore, out _);
            Shape(0.06, out var sBefore2, out _);
            Shape(-0.03, out var sAfter, out _);
            Shape(-0.06, out var sAfter2, out _);
            // Strictly less as we move away from 0 on both sides (no flat top).
            Assert.Less(sBefore, s0);
            Assert.Less(sBefore2, sBefore);
            Assert.Less(sAfter, s0);
            Assert.Less(sAfter2, sAfter);
        }

        [Test]
        public void ToneIsTriangularPeak_OneAtZero_ZeroAtWindowEdge()
        {
            Shape(0.0, out _, out var t0);
            Shape(HitWindow, out _, out var tEdgeBefore);
            Shape(-HitWindow, out _, out var tEdgeAfter);
            Shape(HitWindow * 2, out _, out var tBeyond);
            Assert.AreEqual(1f, t0, 1e-4f);
            Assert.AreEqual(0f, tEdgeBefore, 1e-3f);
            Assert.AreEqual(0f, tEdgeAfter, 1e-3f);
            Assert.AreEqual(0f, tBeyond, 1e-6f, "tone clamps at 0 beyond the window");
        }

        [Test]
        public void SymmetricFalloffAroundZero_ForEqualDistances()
        {
            // Within the hit window both sides use the same linear map, so |remaining| symmetric.
            Shape(0.05, out var sBefore, out var tBefore);
            Shape(-0.05, out var sAfter, out var tAfter);
            Assert.AreEqual(tBefore, tAfter, 1e-4f, "tone is symmetric in |remaining|");
            // Scale: before uses build-up curve, after uses linear falloff; both below peak.
            Assert.Less(sBefore, Peak);
            Assert.Less(sAfter, Peak);
        }
    }
}
