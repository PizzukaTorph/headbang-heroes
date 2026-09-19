using HeadbangHeroes.Charts;
using HeadbangHeroes.Input;
using NUnit.Framework;
using UnityEngine;

namespace HeadbangHeroes.Tests
{
    public sealed class HeadbangInputTests
    {
        [TestCase(100f, 960f, BangDirection.Left)]
        [TestCase(980f, 960f, BangDirection.Right)]
        [TestCase(540f, 1800f, BangDirection.Up)]
        [TestCase(540f, 100f, BangDirection.Down)]
        public void ResolveZone_MapsPortraitCardinalWedges(float x, float y, BangDirection expected)
        {
            Assert.AreEqual(expected, HeadbangInput.ResolveZone(new Vector2(x, y), 1080f, 1920f));
        }

        // --- Expanded diagnostic coverage (v0.0.2). Testing the wedge model AS-IS, not redesigning. ---
        // Model: nx=(x-w/2)/(w/2), ny=(y-h/2)/(h/2); |nx|>|ny| -> L/R else U/D. Boundary is the 45°
        // diagonal in NORMALIZED space. NB: this is documented, not changed.

        static BangDirection R(float x, float y, float w = 1080f, float h = 1920f)
            => HeadbangInput.ResolveZone(new Vector2(x, y), w, h);

        [Test]
        public void ResolveZone_ExactCenter_TieGoesVertical()
        {
            // At exact centre nx==ny==0; |nx|>|ny| is false -> vertical branch; y==centre -> Up edge.
            // Documenting the tie behavior (not asserting it's ideal).
            var d = R(540f, 960f);
            Assert.IsTrue(d == BangDirection.Up || d == BangDirection.Down, $"centre tie resolved to {d}");
        }

        [Test]
        public void ResolveZone_NearCenter_SmallHorizontalBiasIsLeftRight()
        {
            // Slightly more horizontal than vertical offset (in normalized units) -> L/R.
            // portrait: to beat the |nx|>|ny| test, horizontal norm must exceed vertical norm.
            Assert.AreEqual(BangDirection.Right, R(540f + 60f, 960f + 5f));
            Assert.AreEqual(BangDirection.Left,  R(540f - 60f, 960f + 5f));
        }

        [Test]
        public void ResolveZone_PortraitSkew_PixelDiagonalIsNotTheBoundary()
        {
            // KEY FINDING (documented): normalization is per-axis, so a tap 200px right AND 200px up
            // of centre is NOT on the boundary in portrait. nx=200/540=0.370, ny=200/960=0.208;
            // |nx|>|ny| -> resolves RIGHT even though it is equidistant in PIXELS. This skews the
            // effective wedges toward L/R on tall screens. Behavior is intentional-for-now.
            Assert.AreEqual(BangDirection.Right, R(540f + 200f, 960f + 200f));
            // Equal NORMALIZED offsets sit on the diagonal (vertical branch by tie): +0.30 each.
            Assert.AreEqual(BangDirection.Up, R(540f + 0.30f * 540f, 960f + 0.30f * 960f));
        }

        [Test]
        public void ResolveZone_JustEitherSideOfNormalizedDiagonal()
        {
            // Just inside vertical (ny slightly > nx) -> Up; just inside horizontal -> Right.
            Assert.AreEqual(BangDirection.Up,    R(540f + 0.30f * 540f, 960f + 0.31f * 960f));
            Assert.AreEqual(BangDirection.Right, R(540f + 0.31f * 540f, 960f + 0.30f * 960f));
        }

        [TestCase(1920f, 1080f)] // landscape
        [TestCase(1080f, 1080f)] // square
        public void ResolveZone_CardinalsHoldAcrossAspectRatios(float w, float h)
        {
            Assert.AreEqual(BangDirection.Left,  R(w * 0.1f, h * 0.5f, w, h));
            Assert.AreEqual(BangDirection.Right, R(w * 0.9f, h * 0.5f, w, h));
            Assert.AreEqual(BangDirection.Up,    R(w * 0.5f, h * 0.9f, w, h));
            Assert.AreEqual(BangDirection.Down,  R(w * 0.5f, h * 0.1f, w, h));
        }

        [Test]
        public void ResolveZone_LowerThumbPositions()
        {
            // A right-thumb resting low-right and a left-thumb low-left (common one/two-hand grips).
            Assert.AreEqual(BangDirection.Down, R(820f, 260f), "low-right thumb: still Down (below centre, x-norm < y-norm)");
            Assert.AreEqual(BangDirection.Down, R(260f, 260f), "low-left thumb: Down");
            // But a low tap far to the side flips to L/R due to portrait skew — documents the risk.
            Assert.AreEqual(BangDirection.Right, R(1040f, 600f), "far low-right resolves Right, not Down (skew)");
        }

        [Test]
        public void ResolveZone_Corners()
        {
            // Exact corners have |nx|==|ny| (tie) -> vertical branch. Top corners -> Up, bottom -> Down.
            Assert.AreEqual(BangDirection.Up, R(1080f, 1920f));   // top-right corner, tie -> Up
            Assert.AreEqual(BangDirection.Up, R(1079.9f, 1919.9f));
            Assert.AreEqual(BangDirection.Down, R(0f, 0f));       // bottom-left corner, tie -> Down
        }

        [Test]
        public void LaunchVector_IsOppositeTheInversionPoint()
        {
            Assert.AreEqual(Vector2.right, BangDirection.Left.LaunchVector());
            Assert.AreEqual(Vector2.left, BangDirection.Right.LaunchVector());
            Assert.AreEqual(Vector2.down, BangDirection.Up.LaunchVector());
            Assert.AreEqual(Vector2.up, BangDirection.Down.LaunchVector());
        }
    }
}
