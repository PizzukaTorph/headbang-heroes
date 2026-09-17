using System.Collections.Generic;
using HeadbangHeroes.Charts;
using HeadbangHeroes.Gameplay.Neck;
using NUnit.Framework;
using UnityEngine;

namespace HeadbangHeroes.Tests
{
    public sealed class NeckMotionTests
    {
        static NeckMotionState NewNeck() => new NeckMotionState(NeckMotionConfig.Default);

        // Advance a neck by a total elapsed time delivered in fixed-size chunks (simulates a
        // steady render frame rate). Returns the model for chaining assertions.
        static NeckMotionState AdvanceInChunks(NeckMotionState neck, double totalSeconds, double chunk)
        {
            var remaining = totalSeconds;
            while (remaining > 1e-9)
            {
                var d = System.Math.Min(chunk, remaining);
                neck.Advance(d);
                remaining -= d;
            }
            return neck;
        }

        // ---------- Determinism / render-rate invariance ----------

        [Test]
        public void RenderRateInvariance_SameStateAcrossFrameRates()
        {
            const double total = 2.0; // seconds of authoritative time
            double[] frameRates = { 1d / 30d, 1d / 60d, 1d / 90d, 1d / 120d };

            var reference = NewNeck();
            reference.ApplyBang(BangDirection.Left, 1f);
            AdvanceInChunks(reference, total, 1d / 120d);

            foreach (var frame in frameRates)
            {
                var neck = NewNeck();
                neck.ApplyBang(BangDirection.Left, 1f);
                AdvanceInChunks(neck, total, frame);

                // Same total authoritative time => same number of fixed ticks => equal state.
                Assert.AreEqual(reference.Tick, neck.Tick,
                    $"tick count diverged at frame {frame}");
                Assert.AreEqual(reference.HorizontalDisplacement, neck.HorizontalDisplacement, 1e-3f,
                    $"displacement diverged at frame {frame}");
                Assert.AreEqual(reference.HorizontalVelocity, neck.HorizontalVelocity, 1e-2f,
                    $"velocity diverged at frame {frame}");
            }
        }

        [Test]
        public void TickCount_DependsOnElapsedTimeNotChunking()
        {
            var a = NewNeck();
            var b = NewNeck();
            AdvanceInChunks(a, 1.0, 1d / 30d);   // "30 FPS"
            AdvanceInChunks(b, 1.0, 1d / 120d);  // "120 FPS"
            Assert.AreEqual(a.Tick, b.Tick);
            Assert.AreEqual(120, a.Tick); // 1s at 120 Hz
        }

        [Test]
        public void Advance_RetainsSubStepRemainderWithoutLoss()
        {
            var neck = NewNeck();
            var step = NeckMotionConfig.Default.StepSeconds;
            // Feed less than one step twice; together they should cross one step boundary.
            var t1 = neck.Advance(step * 0.6);
            var t2 = neck.Advance(step * 0.6);
            Assert.AreEqual(0, t1);
            Assert.AreEqual(1, t2);
        }

        // ---------- Classic inversion semantics ----------

        static float LaunchDisplacementAfter(BangDirection dir, BangAxis axis, out float velocity)
        {
            var neck = NewNeck();
            neck.ApplyBang(dir, 1f);
            neck.Advance(NeckMotionConfig.Default.StepSeconds * 4);
            velocity = axis == BangAxis.Horizontal ? neck.HorizontalVelocity : neck.VerticalVelocity;
            return axis == BangAxis.Horizontal ? neck.HorizontalDisplacement : neck.VerticalDisplacement;
        }

        [Test]
        public void Left_LaunchesRight()
        {
            var d = LaunchDisplacementAfter(BangDirection.Left, BangAxis.Horizontal, out var v);
            Assert.Greater(v, 0f); // +x = right
            Assert.Greater(d, 0f);
        }

        [Test]
        public void Right_LaunchesLeft()
        {
            var d = LaunchDisplacementAfter(BangDirection.Right, BangAxis.Horizontal, out var v);
            Assert.Less(v, 0f);
            Assert.Less(d, 0f);
        }

        [Test]
        public void Up_LaunchesDown()
        {
            var d = LaunchDisplacementAfter(BangDirection.Up, BangAxis.Vertical, out var v);
            Assert.Less(v, 0f); // Up inversion launches Down (-y)
            Assert.Less(d, 0f);
        }

        [Test]
        public void Down_LaunchesUp()
        {
            var d = LaunchDisplacementAfter(BangDirection.Down, BangAxis.Vertical, out var v);
            Assert.Greater(v, 0f);
            Assert.Greater(d, 0f);
        }

        [Test]
        public void EarlyInversionAnywhere_ReversesMotionWithoutPoseSnap()
        {
            var neck = NewNeck();
            neck.ApplyBang(BangDirection.Left, 1f);       // launching right
            neck.Advance(NeckMotionConfig.Default.StepSeconds * 3);
            var midDisplacement = neck.HorizontalDisplacement;
            Assert.Greater(midDisplacement, 0f);

            // Invert mid-travel (before reaching any side): accepted, redirects motion left.
            neck.ApplyBang(BangDirection.Right, 1f);
            Assert.Less(neck.HorizontalVelocity, 0f); // now heading left
            // No teleport: displacement did not snap to a limit pose.
            Assert.Less(Mathf.Abs(neck.HorizontalDisplacement), NeckMotionConfig.Default.MaxAngle);
        }

        // ---------- Anti-spam through physics ----------

        [Test]
        public void RapidSpam_ProducesLessTravelEvidenceThanPacedInput()
        {
            var step = NeckMotionConfig.Default.StepSeconds;

            // Paced alternating input: let travel develop between inversions.
            var paced = NewNeck();
            NeckMotionSnapshot pacedSnap = default;
            for (var i = 0; i < 4; i++)
            {
                var dir = i % 2 == 0 ? BangDirection.Left : BangDirection.Right;
                pacedSnap = paced.ApplyBang(dir, 1f);
                AdvanceInChunks(paced, 0.4, step); // room to travel
            }
            var pacedNext = paced.CapturePreInversionSnapshot(BangDirection.Left);

            // Spam: invert almost every step, never letting travel develop.
            var spam = NewNeck();
            for (var i = 0; i < 12; i++)
            {
                var dir = i % 2 == 0 ? BangDirection.Left : BangDirection.Right;
                spam.ApplyBang(dir, 1f);
                spam.Advance(step); // one tick only
            }
            var spamNext = spam.CapturePreInversionSnapshot(BangDirection.Left);

            Assert.Greater(pacedNext.TravelSinceInversion, spamNext.TravelSinceInversion,
                "paced input should develop more travel evidence than rapid spam");
        }

        // ---------- MISS / passive continuity ----------

        [Test]
        public void NoInput_MotionContinuesAndRecoversWithoutSnap()
        {
            var neck = NewNeck();
            neck.ApplyBang(BangDirection.Left, 1f);
            var displacements = new List<float>();
            for (var i = 0; i < 200; i++)
            {
                neck.Advance(NeckMotionConfig.Default.StepSeconds);
                displacements.Add(neck.HorizontalDisplacement);
            }
            // Motion actually happened (not frozen) ...
            Assert.Greater(Mathf.Max(Mathf.Abs(displacements[0]), Mathf.Abs(displacements[10])), 0.1f);
            // ... and it recovered toward neutral rather than snapping.
            Assert.Less(Mathf.Abs(displacements[^1]), 2f);
        }

        // ---------- First bang / setup ----------

        [Test]
        public void FirstBang_IsUnpreparedButLaunchesMotion()
        {
            var neck = NewNeck();
            Assert.IsFalse(neck.Prepared);

            var snap = neck.ApplyBang(BangDirection.Left, 1f);
            Assert.IsFalse(snap.Prepared, "first-bang snapshot must report unprepared/setup");
            Assert.AreEqual(0f, snap.TravelSinceInversion, 1e-4f, "no preceding travel to fabricate");

            neck.Advance(NeckMotionConfig.Default.StepSeconds * 4);
            Assert.Greater(neck.HorizontalDisplacement, 0f, "setup bang still launches motion");
            Assert.IsTrue(neck.Prepared, "after first launch the neck is prepared");
        }

        // ---------- Pre-inversion snapshot immutability ----------

        [Test]
        public void PreInversionSnapshot_NotMutatedByApplyingBang()
        {
            var neck = NewNeck();
            neck.ApplyBang(BangDirection.Left, 1f);
            AdvanceInChunks(neck, 0.3, NeckMotionConfig.Default.StepSeconds);

            var arrivalVelocity = neck.HorizontalVelocity;
            var snap = neck.ApplyBang(BangDirection.Right, 1f); // captures pre-inversion, then impulses

            // Snapshot reflects the incoming (pre-impulse) motion, not the post-impulse velocity.
            Assert.AreEqual(arrivalVelocity, snap.Velocity, 1e-3f);
            Assert.AreNotEqual(snap.Velocity, neck.HorizontalVelocity,
                "applying the bang must not retro-actively change the captured evidence");
        }

        // ---------- Local history rollover ----------

        [Test]
        public void EventLocalTravel_RollsOverAtInversionBoundary()
        {
            var neck = NewNeck();
            neck.ApplyBang(BangDirection.Left, 1f);
            AdvanceInChunks(neck, 0.5, NeckMotionConfig.Default.StepSeconds);
            var beforeRollover = neck.CapturePreInversionSnapshot(BangDirection.Right).TravelSinceInversion;
            Assert.Greater(beforeRollover, 0f);

            neck.ApplyBang(BangDirection.Right, 1f); // new boundary resets local travel
            var justAfter = neck.CapturePreInversionSnapshot(BangDirection.Right).TravelSinceInversion;
            Assert.Less(justAfter, beforeRollover);
            Assert.AreEqual(0f, justAfter, 1e-3f, "travel evidence must roll over at the inversion boundary");
        }

        [Test]
        public void OldRunHistory_CannotInflateLaterEventEvidence()
        {
            // A big early gesture then a reset must not leak into later evidence.
            var neck = NewNeck();
            neck.ApplyBang(BangDirection.Left, 1f);
            AdvanceInChunks(neck, 1.0, NeckMotionConfig.Default.StepSeconds);
            neck.Reset();

            var snap = neck.CapturePreInversionSnapshot(BangDirection.Left);
            Assert.AreEqual(0f, snap.TravelSinceInversion, 1e-4f);
            Assert.AreEqual(0f, snap.PeakSpeedSinceInversion, 1e-4f);
            Assert.IsFalse(snap.Prepared);
        }

        [Test]
        public void RepeatedRuns_ProduceEquivalentEvidence()
        {
            NeckMotionSnapshot Run()
            {
                var neck = NewNeck();
                neck.ApplyBang(BangDirection.Left, 1f);
                AdvanceInChunks(neck, 0.4, NeckMotionConfig.Default.StepSeconds);
                return neck.CapturePreInversionSnapshot(BangDirection.Right);
            }

            var a = Run();
            var b = Run();
            Assert.AreEqual(a.TravelSinceInversion, b.TravelSinceInversion, 1e-5f);
            Assert.AreEqual(a.Velocity, b.Velocity, 1e-5f);
        }
    }
}
