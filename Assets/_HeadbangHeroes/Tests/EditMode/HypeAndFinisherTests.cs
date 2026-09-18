using HeadbangHeroes.Gameplay;
using HeadbangHeroes.Gameplay.Scoring;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class HypeAndFinisherTests
    {
        static RunScorer NewScorer() => new RunScorer(ScoringConfig.Default, HypeConfig.Default);

        static ResolvedEvent Ev(string id, Judgment j, float motion = 1f, bool finisher = false) =>
            new ResolvedEvent(id, j, 0d, motion, false, finisher);

        // ---- HYPE base generation ----

        [Test]
        public void HypeContribution_MatchesCanonicalDefaults()
        {
            var s = NewScorer();
            Assert.AreEqual(2, s.Resolve(Ev("a", Judgment.Perfect)).HypeContribution);
            Assert.AreEqual(1, s.Resolve(Ev("b", Judgment.Great)).HypeContribution);
            Assert.AreEqual(0, s.Resolve(Ev("c", Judgment.Good)).HypeContribution);
            Assert.AreEqual(0, s.Resolve(Ev("d", Judgment.Miss)).HypeContribution);
        }

        [Test]
        public void Miss_PreservesAccumulatedHype()
        {
            var s = NewScorer();
            s.Resolve(Ev("a", Judgment.Perfect)); // +2
            s.Resolve(Ev("b", Judgment.Perfect)); // +2 => 4
            var before = s.Hype.Hype;
            s.Resolve(Ev("c", Judgment.Miss));
            Assert.AreEqual(before, s.Hype.Hype, "MISS must not erase HYPE");
        }

        [Test]
        public void Hype_ReachesMaxAndBecomesReady()
        {
            var s = NewScorer(); // max 100, +2 per perfect => 50 perfects
            for (var i = 0; i < 50; i++) s.Resolve(Ev($"e{i}", Judgment.Perfect));
            Assert.AreEqual(100, s.Hype.Hype);
            Assert.IsTrue(s.Hype.IsReady);
        }

        // ---- THE BANG activation ----

        [Test]
        public void ActivateBelowReady_FailsWithNoSideEffects()
        {
            var s = NewScorer();
            s.Resolve(Ev("a", Judgment.Perfect)); // hype 2, not ready
            var before = s.Hype.Hype;
            Assert.IsFalse(s.TryActivateTheBang(10.0));
            Assert.IsFalse(s.Hype.TheBangActive);
            Assert.AreEqual(before, s.Hype.Hype, "failed activation has no side effects");
            Assert.AreEqual(0, s.Hype.TheBangActivations);
        }

        [Test]
        public void ValidActivation_StartsTheBangAndConsumesHype()
        {
            var s = NewScorer();
            for (var i = 0; i < 50; i++) s.Resolve(Ev($"e{i}", Judgment.Perfect));
            Assert.IsTrue(s.TryActivateTheBang(10.0));
            Assert.IsTrue(s.Hype.TheBangActive);
            Assert.AreEqual(0, s.Hype.Hype, "HYPE is spent to enter THE BANG");
            Assert.AreEqual(1, s.Hype.TheBangActivations);
        }

        [Test]
        public void TheBang_ExpiresAfterDuration()
        {
            var s = NewScorer(); // default duration 8s
            for (var i = 0; i < 50; i++) s.Resolve(Ev($"e{i}", Judgment.Perfect));
            s.TryActivateTheBang(10.0);
            s.Advance(17.9);
            Assert.IsTrue(s.Hype.TheBangActive);
            s.Advance(18.1);
            Assert.IsFalse(s.Hype.TheBangActive, "THE BANG expires after its window");
        }

        // ---- Circularity guard ----

        [Test]
        public void TheBangScoreMultiplier_DoesNotAmplifyHype()
        {
            var s = NewScorer();
            for (var i = 0; i < 50; i++) s.Resolve(Ev($"e{i}", Judgment.Perfect));
            s.TryActivateTheBang(10.0);
            var o = s.Resolve(Ev("bang", Judgment.Perfect));
            Assert.AreEqual(2, o.HypeContribution, "HYPE is base +2, never x10 by THE BANG reward multiplier");
            Assert.Greater(o.ScoreContribution, 0);
        }

        [Test]
        public void TheBang_CannotImmediatelySelfRefillToReady()
        {
            var s = NewScorer();
            for (var i = 0; i < 50; i++) s.Resolve(Ev($"e{i}", Judgment.Perfect));
            s.TryActivateTheBang(10.0); // hype -> 0
            // A single strong event during THE BANG adds only base HYPE, cannot re-reach max.
            var o = s.Resolve(Ev("bang", Judgment.Perfect));
            Assert.Less(s.Hype.Hype, s.Hype.MaxHype, "THE BANG cannot self-refill to READY in one event");
        }

        [Test]
        public void RewardContext_IsStableForTheSameEvent()
        {
            var s = NewScorer();
            for (var i = 0; i < 50; i++) s.Resolve(Ev($"e{i}", Judgment.Perfect));
            s.TryActivateTheBang(10.0);
            var o = s.Resolve(Ev("bang", Judgment.Perfect));
            Assert.IsTrue(o.DuringTheBang, "the event is scored under the context snapshotted before its own rewards");
        }

        // ---- Finisher ----

        [Test]
        public void Candidate_OutsideTheBang_DoesNotFinish()
        {
            var s = NewScorer();
            var o = s.Resolve(Ev("a", Judgment.Perfect, finisher: true));
            Assert.IsFalse(o.WasFinisher);
            Assert.AreEqual(0, s.Hype.FinishersExecuted);
        }

        [Test]
        public void NonCandidate_DuringTheBang_DoesNotFinish()
        {
            var s = NewScorer();
            for (var i = 0; i < 50; i++) s.Resolve(Ev($"e{i}", Judgment.Perfect));
            s.TryActivateTheBang(10.0);
            var o = s.Resolve(Ev("plain", Judgment.Perfect, finisher: false));
            Assert.IsFalse(o.WasFinisher);
        }

        [Test]
        public void EligibleCandidate_DuringTheBang_Finishes()
        {
            var s = NewScorer();
            for (var i = 0; i < 50; i++) s.Resolve(Ev($"e{i}", Judgment.Perfect));
            s.TryActivateTheBang(10.0);
            var o = s.Resolve(Ev("fin", Judgment.Perfect, finisher: true));
            Assert.IsTrue(o.WasFinisher);
            Assert.AreEqual(1, s.Hype.FinishersExecuted);
        }

        [Test]
        public void Finisher_ResolvesOncePerWindow()
        {
            var s = NewScorer();
            for (var i = 0; i < 50; i++) s.Resolve(Ev($"e{i}", Judgment.Perfect));
            s.TryActivateTheBang(10.0);
            s.Resolve(Ev("fin1", Judgment.Perfect, finisher: true));
            var o2 = s.Resolve(Ev("fin2", Judgment.Perfect, finisher: true));
            Assert.IsFalse(o2.WasFinisher, "only the first suitable candidate in the window finishes");
            Assert.AreEqual(1, s.Hype.FinishersExecuted);
        }

        [Test]
        public void MissCandidate_DuringTheBang_DoesNotFinish()
        {
            var s = NewScorer();
            for (var i = 0; i < 50; i++) s.Resolve(Ev($"e{i}", Judgment.Perfect));
            s.TryActivateTheBang(10.0);
            var o = s.Resolve(Ev("fin", Judgment.Miss, finisher: true));
            Assert.IsFalse(o.WasFinisher, "a missed candidate is not a Finisher");
        }

        [Test]
        public void GoodCandidate_DoesNotFinish_ByDefaultTierGate()
        {
            var s = NewScorer(); // default FinisherMinJudgment = Great
            for (var i = 0; i < 50; i++) s.Resolve(Ev($"e{i}", Judgment.Perfect));
            s.TryActivateTheBang(10.0);
            var o = s.Resolve(Ev("fin", Judgment.Good, finisher: true));
            Assert.IsFalse(o.WasFinisher, "a GOOD candidate is below the default Finisher tier");
            Assert.AreEqual(0, s.Hype.FinishersExecuted);
        }

        [Test]
        public void SecondActivationWindow_AllowsAnotherFinisher()
        {
            var s = NewScorer();
            for (var i = 0; i < 50; i++) s.Resolve(Ev($"e{i}", Judgment.Perfect));
            s.TryActivateTheBang(10.0);
            Assert.IsTrue(s.Resolve(Ev("fin1", Judgment.Perfect, finisher: true)).WasFinisher);
            s.Advance(19.0); // expire first window (10 + 8s)

            for (var i = 0; i < 50; i++) s.Resolve(Ev($"g{i}", Judgment.Perfect)); // refill hype
            s.TryActivateTheBang(20.0);
            Assert.IsTrue(s.Resolve(Ev("fin2", Judgment.Perfect, finisher: true)).WasFinisher,
                "a fresh THE BANG window allows a new Finisher");
            Assert.AreEqual(2, s.Hype.FinishersExecuted);
        }

        [Test]
        public void ActivateWhileActive_Fails()
        {
            var s = NewScorer();
            for (var i = 0; i < 50; i++) s.Resolve(Ev($"e{i}", Judgment.Perfect));
            Assert.IsTrue(s.TryActivateTheBang(10.0));
            // Even if hype somehow refilled, a second activation during the same window must fail.
            for (var i = 0; i < 50; i++) s.Resolve(Ev($"g{i}", Judgment.Perfect));
            Assert.IsFalse(s.TryActivateTheBang(11.0));
            Assert.AreEqual(1, s.Hype.TheBangActivations);
        }

        [Test]
        public void HypeDisabledDuringTheBang_StopsAccrual()
        {
            var cfg = new HypeConfig(2, 1, 0, 0, 100, 8d, 10f, 10f, 10f, HypeDuringTheBang.Disabled);
            var s = new RunScorer(ScoringConfig.Default, cfg);
            for (var i = 0; i < 50; i++) s.Resolve(Ev($"e{i}", Judgment.Perfect));
            s.TryActivateTheBang(10.0); // hype -> 0
            var o = s.Resolve(Ev("bang", Judgment.Perfect));
            Assert.AreEqual(0, o.HypeContribution, "HYPE generation is suspended during THE BANG in Disabled mode");
            Assert.AreEqual(0, s.Hype.Hype);
        }

        [Test]
        public void Reset_ClearsAllStateForRetry()
        {
            var s = NewScorer();
            for (var i = 0; i < 50; i++) s.Resolve(Ev($"e{i}", Judgment.Perfect));
            s.TryActivateTheBang(10.0);
            s.Resolve(Ev("fin", Judgment.Perfect, finisher: true));

            s.Reset();
            Assert.AreEqual(0, s.Hype.Hype);
            Assert.IsFalse(s.Hype.TheBangActive);
            Assert.AreEqual(0, s.Hype.TheBangActivations);
            Assert.AreEqual(0, s.Hype.FinishersExecuted);
            Assert.AreEqual(0, s.TotalHypeEarned);
            Assert.AreEqual(0, s.Scoring.Score);
            Assert.AreEqual(0, s.Scoring.LongestCombo);
        }

        [Test]
        public void ConfigGuards_RejectNaNAndNegativeFactors()
        {
            var badScoring = new ScoringConfig(float.NaN, -1f, 0.85f, 0.6f, 0.5f, 10, 5);
            var s = new RunScorer(badScoring, HypeConfig.Default);
            var o = s.Resolve(Ev("a", Judgment.Perfect));
            // BaseScore NaN -> 0, but a successful hit still floors to at least 1 (no silent 0).
            Assert.GreaterOrEqual(o.ScoreContribution, 1L);
        }
    }
}
