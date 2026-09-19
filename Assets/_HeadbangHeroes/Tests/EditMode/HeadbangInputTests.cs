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

        // --- ResolveZone coverage. v0.0.3: EQUAL-PIXEL model. dx=x-w/2, dy=y-h/2; |dx|>|dy| -> L/R
        // else U/D. Boundary is the true 45° diagonal in PIXELS (symmetric on any aspect ratio).

        static BangDirection R(float x, float y, float w = 1080f, float h = 1920f)
            => HeadbangInput.ResolveZone(new Vector2(x, y), w, h);

        [Test]
        public void ResolveZone_ExactCenter_TieGoesVertical()
        {
            // At exact centre dx==dy==0; |dx|>|dy| is false -> vertical branch; dy==0 -> Up edge.
            var d = R(540f, 960f);
            Assert.IsTrue(d == BangDirection.Up || d == BangDirection.Down, $"centre tie resolved to {d}");
        }

        [Test]
        public void ResolveZone_NearCenter_SmallHorizontalBiasIsLeftRight()
        {
            // More horizontal than vertical pixel offset -> L/R.
            Assert.AreEqual(BangDirection.Right, R(540f + 60f, 960f + 5f));
            Assert.AreEqual(BangDirection.Left,  R(540f - 60f, 960f + 5f));
        }

        [Test]
        public void ResolveZone_PortraitSkewFixed_EqualPixelOffsetResolvesByPixels()
        {
            // v0.0.3 FIX: a tap 200px right AND 200px up of centre is equidistant in pixels; the
            // boundary is the pixel diagonal, so the tie resolves to the vertical branch (Up) —
            // NOT Right as the old per-axis-normalized model wrongly did on portrait. This removes
            // the L/R skew that caused WRONG_DIRECTION on Up/Down taps near the sides.
            Assert.AreEqual(BangDirection.Up, R(540f + 200f, 960f + 200f));
            // Clearly-more-horizontal-in-pixels still resolves Right.
            Assert.AreEqual(BangDirection.Right, R(540f + 200f, 960f + 120f));
            // Clearly-more-vertical-in-pixels resolves Up.
            Assert.AreEqual(BangDirection.Up, R(540f + 120f, 960f + 200f));
        }

        [Test]
        public void ResolveZone_JustEitherSideOfPixelDiagonal()
        {
            // Just more vertical (|dy| > |dx|) -> Up; just more horizontal -> Right.
            Assert.AreEqual(BangDirection.Up,    R(540f + 200f, 960f + 201f));
            Assert.AreEqual(BangDirection.Right, R(540f + 201f, 960f + 200f));
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
            Assert.AreEqual(BangDirection.Down, R(820f, 260f), "low-right thumb: Down (|dy| > |dx| in pixels)");
            Assert.AreEqual(BangDirection.Down, R(260f, 260f), "low-left thumb: Down");
            // Far to the side AND low: genuinely more horizontal in pixels -> Right (correct, not skew).
            Assert.AreEqual(BangDirection.Right, R(1040f, 600f), "far low-right is more horizontal in pixels -> Right");
        }

        [Test]
        public void ResolveZone_Corners()
        {
            // Exact corners have |dx|==|dy| (tie) -> vertical branch. Top corners -> Up, bottom -> Down.
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
