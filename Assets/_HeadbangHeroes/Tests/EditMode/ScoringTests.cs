using HeadbangHeroes.Gameplay;
using HeadbangHeroes.Gameplay.Scoring;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class ScoringTests
    {
        static RunScorer NewScorer() => new RunScorer(ScoringConfig.Default, HypeConfig.Default);

        static ResolvedEvent Hit(string id, Judgment j, float motion = 1f, bool finisher = false) =>
            new ResolvedEvent(id, j, 0d, motion, false, finisher);

        // ---- Determinism ----

        [Test]
        public void SameInputs_ProduceIdenticalScore()
        {
            long Run()
            {
                var s = NewScorer();
                s.Resolve(Hit("a", Judgment.Perfect));
                s.Resolve(Hit("b", Judgment.Great, 0.5f));
                s.Resolve(Hit("c", Judgment.Good, 0.9f));
                return s.Scoring.Score;
            }
            Assert.AreEqual(Run(), Run());
        }

        // ---- Dimension separation ----

        [Test]
        public void PerfectTimingWeakMotion_KeepsPerfectTimingAndWeakMotion()
        {
            var s = NewScorer();
            var o = s.Resolve(Hit("a", Judgment.Perfect, motion: 0.0f));
            Assert.AreEqual(Judgment.Perfect, o.Judgment, "timing tier unchanged by weak motion");
            Assert.AreEqual(0f, o.MotionQuality, 1e-4f, "motion reported separately, not relabeled");
        }

        [Test]
        public void GreatTimingStrongMotion_KeepsGreatTimingAndStrongMotion()
        {
            var s = NewScorer();
            var o = s.Resolve(Hit("a", Judgment.Great, motion: 1.0f));
            Assert.AreEqual(Judgment.Great, o.Judgment);
            Assert.AreEqual(1f, o.MotionQuality, 1e-4f);
        }

        [Test]
        public void StrongMotion_ScoresHigherThanWeakMotion_SameTiming()
        {
            var strong = NewScorer(); var oStrong = strong.Resolve(Hit("a", Judgment.Perfect, 1.0f));
            var weak = NewScorer(); var oWeak = weak.Resolve(Hit("a", Judgment.Perfect, 0.0f));
            Assert.Greater(oStrong.ScoreContribution, oWeak.ScoreContribution);
        }

        // ---- Combo ----

        [Test]
        public void PerfectAndGreat_BuildCombo()
        {
            var s = NewScorer();
            Assert.AreEqual(1, s.Resolve(Hit("a", Judgment.Perfect)).ComboAfter);
            Assert.AreEqual(2, s.Resolve(Hit("b", Judgment.Great)).ComboAfter);
        }

        [Test]
        public void Good_EndsCombo()
        {
            var s = NewScorer();
            s.Resolve(Hit("a", Judgment.Perfect));
            s.Resolve(Hit("b", Judgment.Great));
            var o = s.Resolve(Hit("c", Judgment.Good));
            Assert.AreEqual(0, o.ComboAfter, "GOOD ends the combo");
            Assert.AreEqual(2, s.Scoring.LongestCombo);
            Assert.AreEqual(1, s.Scoring.CompletedCombos);
        }

        [Test]
        public void Miss_ResetsCombo()
        {
            var s = NewScorer();
            s.Resolve(Hit("a", Judgment.Perfect));
            s.Resolve(Hit("b", Judgment.Perfect));
            var o = s.Resolve(Hit("c", Judgment.Miss));
            Assert.AreEqual(0, o.ComboAfter);
            Assert.AreEqual(2, s.Scoring.LongestCombo);
        }

        // ---- Multiplier ----

        [Test]
        public void Multiplier_AdvancesWithHits_AndGoodContributes()
        {
            var s = NewScorer(); // Default: 10 hits per step, max 5
            for (var i = 0; i < 9; i++) s.Resolve(Hit($"e{i}", Judgment.Good)); // GOOD still advances multiplier
            Assert.AreEqual(1, s.Scoring.Multiplier, "still x1 before the 10th hit");
            var o = s.Resolve(Hit("e9", Judgment.Good));
            Assert.AreEqual(2, o.MultiplierAfter, "10th successful hit reaches x2 even via GOOD");
        }

        [Test]
        public void Multiplier_ResetsOnMiss()
        {
            var s = NewScorer();
            for (var i = 0; i < 10; i++) s.Resolve(Hit($"e{i}", Judgment.Perfect));
            Assert.AreEqual(2, s.Scoring.Multiplier);
            s.Resolve(Hit("miss", Judgment.Miss));
            Assert.AreEqual(1, s.Scoring.Multiplier, "MISS resets multiplier progress");
        }

        [Test]
        public void Multiplier_CapsAtConfiguredMax()
        {
            var s = NewScorer(); // max 5 => 40 hits caps
            for (var i = 0; i < 100; i++) s.Resolve(Hit($"e{i}", Judgment.Perfect));
            Assert.AreEqual(5, s.Scoring.Multiplier);
        }

        // ---- RunResult ----

        [Test]
        public void RunResult_TotalsMatchAuthoritativeState()
        {
            var s = NewScorer();
            s.Resolve(Hit("a", Judgment.Perfect));
            s.Resolve(Hit("b", Judgment.Great));
            s.Resolve(Hit("c", Judgment.Good));
            s.Resolve(Hit("d", Judgment.Miss));

            var r = s.BuildResult("song1", "chart1", 3, 1);
            Assert.AreEqual("song1", r.SongId);
            Assert.AreEqual("chart1", r.ChartId);
            Assert.AreEqual(3, r.ChartVersion);
            Assert.AreEqual(1, r.RulesVersion);
            Assert.AreEqual(1, r.PerfectCount);
            Assert.AreEqual(1, r.GreatCount);
            Assert.AreEqual(1, r.GoodCount);
            Assert.AreEqual(1, r.MissCount);
            Assert.AreEqual(s.Scoring.Score, r.Score, "result score equals authoritative score (no recompute)");
            Assert.AreEqual(s.Scoring.LongestCombo, r.LongestCombo);
            Assert.AreEqual(s.TotalHypeEarned, r.TotalHypeEarned);
        }
    }
}
