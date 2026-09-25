using HeadbangHeroes.Presentation;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class Avatar3DMappingTests
    {
        static Avatar3DMappingConfig Config(float max = 42f)
        {
            var config = Avatar3DMappingConfig.Default;
            config.maxVisualAngle = max;
            return config;
        }

        [Test]
        public void NeutralNeckStateProducesNeutralPose()
        {
            var pose = Avatar3DMapping.Map(0f, 0f, Config());

            Assert.AreEqual(0f, pose.neckPitch);
            Assert.AreEqual(0f, pose.neckRoll);
            Assert.AreEqual(0f, pose.headPitch);
            Assert.AreEqual(0f, pose.headRoll);
            Assert.AreEqual(0f, pose.chestPitch);
            Assert.AreEqual(0f, pose.chestRoll);
        }

        [Test]
        public void PositiveAndNegativeHorizontalAnglesPreserveDirection()
        {
            var positive = Avatar3DMapping.Map(20f, 0f, Config());
            var negative = Avatar3DMapping.Map(-20f, 0f, Config());

            Assert.Greater(positive.neckRoll, 0f);
            Assert.Greater(positive.headRoll, 0f);
            Assert.Less(negative.neckRoll, 0f);
            Assert.Less(negative.headRoll, 0f);
        }

        [Test]
        public void MappingClampsAtConfiguredMaximum()
        {
            var pose = Avatar3DMapping.Map(100f, -100f, Config(30f));

            Assert.AreEqual(30f * 0.35f, pose.neckRoll, 0.0001f);
            Assert.AreEqual(-30f * 0.65f, pose.headPitch, 0.0001f);
        }

        [Test]
        public void DistributionUsesConfiguredNeckHeadAndChestShares()
        {
            var pose = Avatar3DMapping.Map(20f, 10f, Config());

            Assert.AreEqual(10f * 0.35f, pose.neckPitch, 0.0001f);
            Assert.AreEqual(10f * 0.65f, pose.headPitch, 0.0001f);
            Assert.AreEqual(20f * 0.08f, pose.chestRoll, 0.0001f);
        }
    }
}
