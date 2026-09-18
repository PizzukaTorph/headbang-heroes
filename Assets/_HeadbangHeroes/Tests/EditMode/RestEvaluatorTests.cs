using HeadbangHeroes.Charts.Runtime;
using HeadbangHeroes.Gameplay.Rest;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class RestEvaluatorTests
    {
        static readonly RestConfig Cfg = RestConfig.Default;

        // Rest: start 10.0, duration 2.0, settling 0.8 => evaluation window [10.8, 12.0].
        static RuntimeRestEvent Rest() => new RuntimeRestEvent("rest0", 10.0, 2.0, 0.8);

        [Test]
        public void SettlingPhase_MomentumDoesNotFailStillness()
        {
            var ev = Rest();
            var e = new RestEvaluator();
            e.Begin(ev, Cfg);

            // High momentum DURING settling (before 10.8) must not count against the player.
            e.Sample(10.1, neckSpeed: 300f, neckDisplacement: 40f);
            e.Sample(10.5, neckSpeed: 200f, neckDisplacement: 30f);
            // Then still during evaluation.
            for (var t = 10.8; t <= 12.0; t += 1.0 / 120.0)
                e.Sample(t, neckSpeed: 2f, neckDisplacement: 1f);

            var outcome = e.Complete();
            Assert.IsTrue(outcome.Passed, "settling-phase momentum must not fail stillness");
            Assert.LessOrEqual(outcome.PeakSpeed, Cfg.MaxEvaluationSpeed);
        }

        [Test]
        public void MovementDuringEvaluation_FailsStillness()
        {
            var ev = Rest();
            var e = new RestEvaluator();
            e.Begin(ev, Cfg);
            // Big movement inside the evaluation window.
            e.Sample(11.0, neckSpeed: 150f, neckDisplacement: 25f);
            var outcome = e.Complete();
            Assert.IsFalse(outcome.Passed, "significant motion during evaluation fails stillness");
        }

        [Test]
        public void SamplesOutsideInterval_AreIgnored()
        {
            var ev = Rest();
            var e = new RestEvaluator();
            e.Begin(ev, Cfg);
            e.Sample(9.0, 500f, 90f);    // before rest
            e.Sample(13.0, 500f, 90f);   // after rest
            var outcome = e.Complete();
            Assert.AreEqual(0, outcome.EvaluationSamples);
            Assert.IsTrue(outcome.Passed, "no evaluation samples => stillness satisfied by default");
        }

        [Test]
        public void RenderRateIndependent_EquivalentHistorySameOutcome()
        {
            // Same authoritative motion (constant small speed) sampled at 60 vs 120 Hz.
            RestOutcome Run(double step)
            {
                var e = new RestEvaluator();
                e.Begin(Rest(), Cfg);
                for (var t = 10.8; t <= 12.0 + 1e-9; t += step)
                    e.Sample(t, neckSpeed: 5f, neckDisplacement: 3f);
                return e.Complete();
            }

            var at60 = Run(1.0 / 60.0);
            var at120 = Run(1.0 / 120.0);
            Assert.AreEqual(at60.Passed, at120.Passed);
            Assert.AreEqual(at60.PeakSpeed, at120.PeakSpeed, 1e-4f, "peak-based outcome is sample-rate independent");
            Assert.AreEqual(at60.PeakDisplacement, at120.PeakDisplacement, 1e-4f);
        }

        [Test]
        public void Evaluator_NeverExposesNeckMutation()
        {
            // The evaluator only reads doubles/floats and returns an outcome; it holds no neck
            // reference and cannot mutate neck state. This test documents the contract: Sample
            // takes values, Complete returns a value — there is no API to write neck state.
            var e = new RestEvaluator();
            e.Begin(Rest(), Cfg);
            e.Sample(11.0, 1f, 1f);
            var outcome = e.Complete();
            Assert.AreEqual("rest0", outcome.RestId);
            Assert.IsFalse(e.IsActive, "Complete ends the evaluation");
        }

        [Test]
        public void BoundaryAtEvaluationStart_IsCounted()
        {
            var ev = Rest();
            var e = new RestEvaluator();
            e.Begin(ev, Cfg);
            e.Sample(ev.EvaluationStart, 1f, 1f); // exactly at 10.8
            var outcome = e.Complete();
            Assert.AreEqual(1, outcome.EvaluationSamples);
        }
    }
}
