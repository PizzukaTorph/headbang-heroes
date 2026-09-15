using HeadbangHeroes.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace HeadbangHeroes.Tests
{
    public sealed class HeadMotionStateTests
    {
        static HeadMotionState NewState() => HeadMotionState.CreateDefault();

        [Test]
        public void MotionQuality_IsWithinUnitRange()
        {
            var s = NewState();
            var q = s.EvaluateMotionQuality(1f);
            Assert.GreaterOrEqual(q, 0f);
            Assert.LessOrEqual(q, 1f);
        }

        [Test]
        public void CoherentMotion_ScoresHigherThanIncoherent()
        {
            var s = NewState();
            s.ApplyImpulse(1f, 1f);           // moving right
            s.Integrate(0.016f);

            var coherent = s.EvaluateMotionQuality(1f);   // asked to go right
            var incoherent = s.EvaluateMotionQuality(-1f); // asked to go left
            Assert.Greater(coherent, incoherent);
        }

        [Test]
        public void StillHead_HasLowerQualityThanMovingHead()
        {
            var still = NewState();
            var stillQuality = still.EvaluateMotionQuality(1f);

            var moving = NewState();
            moving.ApplyImpulse(1f, 1f);
            moving.Integrate(0.016f);
            var movingQuality = moving.EvaluateMotionQuality(1f);

            Assert.Greater(movingQuality, stillQuality);
        }

        [Test]
        public void PeakAmplitude_TracksMaxAngle()
        {
            var s = NewState();
            s.ApplyImpulse(1f, 1f);
            for (var i = 0; i < 30; i++) s.Integrate(0.016f);

            Assert.Greater(s.peakAmplitude, 0f);
            Assert.GreaterOrEqual(s.peakAmplitude, Mathf.Abs(s.angle));
        }

        [Test]
        public void Integrate_IsDeterministic()
        {
            var a = NewState();
            var b = NewState();
            a.ApplyImpulse(1f, 1f);
            b.ApplyImpulse(1f, 1f);
            for (var i = 0; i < 20; i++)
            {
                a.Integrate(0.016f);
                b.Integrate(0.016f);
            }
            Assert.AreEqual(a.angle, b.angle, 1e-6f);
            Assert.AreEqual(a.velocity, b.velocity, 1e-6f);
        }

        [Test]
        public void SpringReturnsHeadTowardNeutralOverTime()
        {
            var s = NewState();
            s.ApplyImpulse(1f, 1f);
            for (var i = 0; i < 400; i++) s.Integrate(0.016f);
            // After enough time the damped spring should settle near neutral.
            Assert.Less(Mathf.Abs(s.angle), 2f);
        }

        [Test]
        public void ResetDynamics_ClearsState()
        {
            var s = NewState();
            s.ApplyImpulse(1f, 1f);
            s.Integrate(0.016f);
            s.ResetDynamics();
            Assert.AreEqual(0f, s.angle);
            Assert.AreEqual(0f, s.velocity);
            Assert.AreEqual(0f, s.peakAmplitude);
        }
    }
}
