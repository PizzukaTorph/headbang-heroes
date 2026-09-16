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
