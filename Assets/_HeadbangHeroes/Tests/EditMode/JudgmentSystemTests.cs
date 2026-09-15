using HeadbangHeroes.Gameplay;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class JudgmentSystemTests
    {
        // ---- Timing window boundaries ----

        [Test]
        public void ExactlyPerfectBoundary_IsPerfect()
        {
            Assert.AreEqual(Judgment.Perfect, JudgmentSystem.Evaluate(JudgmentSystem.Perfect).judgment);
            Assert.AreEqual(Judgment.Perfect, JudgmentSystem.Evaluate(-JudgmentSystem.Perfect).judgment);
        }

        [Test]
        public void JustOutsidePerfect_IsGreat()
        {
            var e = JudgmentSystem.Perfect + 0.0001;
            Assert.AreEqual(Judgment.Great, JudgmentSystem.Evaluate(e).judgment);
            Assert.AreEqual(Judgment.Great, JudgmentSystem.Evaluate(-e).judgment);
        }

        [Test]
        public void ExactlyGreatBoundary_IsGreat()
        {
            Assert.AreEqual(Judgment.Great, JudgmentSystem.Evaluate(JudgmentSystem.Great).judgment);   // 70ms
            Assert.AreEqual(Judgment.Great, JudgmentSystem.Evaluate(-JudgmentSystem.Great).judgment);
        }

        [Test]
        public void JustOutsideGreat_IsGood()
        {
            var e = JudgmentSystem.Great + 0.0001;
            Assert.AreEqual(Judgment.Good, JudgmentSystem.Evaluate(e).judgment);
        }

        [Test]
        public void ExactlyGoodBoundary_IsGood()
        {
            Assert.AreEqual(Judgment.Good, JudgmentSystem.Evaluate(JudgmentSystem.Good).judgment);      // 120ms
            Assert.AreEqual(Judgment.Good, JudgmentSystem.Evaluate(-JudgmentSystem.Good).judgment);
        }

        [Test]
        public void OutsideGood_IsMiss()
        {
            var e = JudgmentSystem.Good + 0.0001;
            Assert.AreEqual(Judgment.Miss, JudgmentSystem.Evaluate(e).judgment);
            Assert.AreEqual(Judgment.Miss, JudgmentSystem.Evaluate(-e).judgment);
        }

        // ---- TimingQuality & Performance ----

        [Test]
        public void TimingQuality_MatchesTiers()
        {
            Assert.AreEqual(1.00f, JudgmentSystem.TimingQuality(Judgment.Perfect));
            Assert.AreEqual(0.85f, JudgmentSystem.TimingQuality(Judgment.Great));
            Assert.AreEqual(0.60f, JudgmentSystem.TimingQuality(Judgment.Good));
            Assert.AreEqual(0.00f, JudgmentSystem.TimingQuality(Judgment.Miss));
        }

        [Test]
        public void Performance_IsTimingTimesMotion()
        {
            var r = JudgmentSystem.Evaluate(0.0, 0.5f); // Perfect, motion 0.5
            Assert.AreEqual(1.0f, r.timingQuality, 1e-4f);
            Assert.AreEqual(0.5f, r.motionQuality, 1e-4f);
            Assert.AreEqual(0.5f, r.Performance, 1e-4f);
        }

        // ---- ResolveInput: scheduler decision logic ----

        [Test]
        public void EarlyInputOutsideWindow_DoesNotConsume()
        {
            var error = -(JudgmentSystem.Good + 0.05); // far too early
            var outcome = JudgmentSystem.ResolveInput(error, true, 1f, out _);
            Assert.AreEqual(JudgmentSystem.JudgeOutcome.TooEarlyKeep, outcome);
        }

        [Test]
        public void ValidInput_Consumes()
        {
            var outcome = JudgmentSystem.ResolveInput(0.0, true, 1f, out var result);
            Assert.AreEqual(JudgmentSystem.JudgeOutcome.Consumed, outcome);
            Assert.AreEqual(Judgment.Perfect, result.judgment);
        }

        [Test]
        public void LateInputWithinWindow_ConsumesAsGood()
        {
            var outcome = JudgmentSystem.ResolveInput(JudgmentSystem.Good, true, 1f, out var result);
            Assert.AreEqual(JudgmentSystem.JudgeOutcome.Consumed, outcome);
            Assert.AreEqual(Judgment.Good, result.judgment);
        }

        [Test]
        public void WrongDirection_ConsumesAsMiss()
        {
            var outcome = JudgmentSystem.ResolveInput(0.0, false, 1f, out var result);
            Assert.AreEqual(JudgmentSystem.JudgeOutcome.Consumed, outcome);
            Assert.AreEqual(Judgment.Miss, result.judgment);
        }
    }
}
