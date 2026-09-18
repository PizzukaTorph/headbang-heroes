using HeadbangHeroes.Gameplay.Scoring;
using HeadbangHeroes.Meta;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class MetaProgressionTests
    {
        static readonly GradeConfig GCfg = GradeConfig.Default;
        static readonly GradeNormalization Norm = new GradeNormalization(200000, 60, 120, 2);
        static readonly ProgressionConfig PCfg = ProgressionConfig.Default;

        static RunResult Run(long score, int longest, int hype, int fin, int p, int g, int gd, int m,
            string song = "song1", string chart = "chart1", int cv = 2, int rv = 1, int well = 0) =>
            new RunResult(song, chart, cv, rv, score, p, g, gd, well, m, longest, completedCombos: 0,
                totalHypeEarned: hype, theBangActivations: fin > 0 ? 1 : 0, finishersExecuted: fin);

        // ---- Grade ----

        [Test]
        public void Grade_IsDeterministic()
        {
            var r = Run(150000, 40, 80, 1, 60, 20, 5, 2);
            var a = GradeCalculator.Evaluate(r, GCfg, Norm);
            var b = GradeCalculator.Evaluate(r, GCfg, Norm);
            Assert.AreEqual(a.Grade, b.Grade);
            Assert.AreEqual(a.Performance, b.Performance, 1e-6f);
        }

        [Test]
        public void Grade_TopRunIsS_WeakRunIsD()
        {
            var top = GradeCalculator.Evaluate(Run(200000, 60, 120, 2, 90, 5, 0, 0), GCfg, Norm);
            var weak = GradeCalculator.Evaluate(Run(1000, 1, 2, 0, 1, 0, 2, 30), GCfg, Norm);
            Assert.AreEqual(Grade.S, top.Grade);
            Assert.AreEqual(Grade.D, weak.Grade);
        }

        [Test]
        public void Grade_SafeButLowIntensity_DoesNotReachS()
        {
            // Good score + combo but zero HYPE and zero finishers should not be S (intensity matters).
            var r = Run(200000, 60, 0, 0, 90, 0, 0, 0);
            var res = GradeCalculator.Evaluate(r, GCfg, Norm);
            Assert.AreNotEqual(Grade.S, res.Grade);
        }

        [Test]
        public void Grade_NormalizesPerChart_NoHardcodedRawThreshold()
        {
            var run = Run(100000, 30, 60, 1, 50, 10, 5, 2);
            var easyNorm = new GradeNormalization(100000, 30, 60, 1);   // run meets easy targets
            var hardNorm = new GradeNormalization(400000, 120, 240, 4); // same run vs harder targets
            var easy = GradeCalculator.Evaluate(run, GCfg, easyNorm);
            var hard = GradeCalculator.Evaluate(run, GCfg, hardNorm);
            Assert.Greater(easy.Performance, hard.Performance, "same run grades higher against easier targets");
        }

        // ---- Records ----

        [Test]
        public void Record_CompatibleBetterReplaces_WorseDoesNot()
        {
            var a = new ChartRecord { songId="s", chartId="c", chartVersion=1, rulesVersion=1, bestScore=100, longestCombo=10 };
            var b = new ChartRecord { songId="s", chartId="c", chartVersion=1, rulesVersion=1, bestScore=200, longestCombo=5 };
            var merged = ChartRecord.MergeBest(a, b);
            Assert.AreEqual(200, merged.bestScore, "keeps higher score");
            Assert.AreEqual(10, merged.longestCombo, "keeps higher combo (best-of each metric)");
        }

        [Test]
        public void Record_IncompatibleVersions_AreNotComparable()
        {
            var a = new ChartRecord { songId="s", chartId="c", chartVersion=1, rulesVersion=1 };
            var b = new ChartRecord { songId="s", chartId="c", chartVersion=2, rulesVersion=1 };
            Assert.IsFalse(a.IsCompatibleWith(b));
            Assert.Throws<System.InvalidOperationException>(() => ChartRecord.MergeBest(a, b));
        }

        // ---- Progression ----

        [Test]
        public void Progression_XpUsesGradeModifier()
        {
            var profile = UserProfile.CreateDefault();
            var run = Run(200000, 60, 120, 2, 90, 5, 0, 0);
            var grade = GradeCalculator.Evaluate(run, GCfg, Norm); // S
            var outcome = ProgressionService.Apply(profile, run, grade, PCfg);
            Assert.AreEqual((long)(PCfg.BaseSongXp * PCfg.SModifier), outcome.XpEarned);
        }

        [Test]
        public void Progression_FirstClearAddsBonusHh_AndRecordCreated()
        {
            var profile = UserProfile.CreateDefault();
            var run = Run(50000, 20, 30, 0, 30, 10, 5, 3);
            var grade = GradeCalculator.Evaluate(run, GCfg, Norm);
            var outcome = ProgressionService.Apply(profile, run, grade, PCfg);
            Assert.IsTrue(outcome.FirstClear);
            Assert.AreEqual(PCfg.BaseHh + PCfg.FirstClearBonusHh, outcome.HhEarned);
            Assert.AreEqual(1, profile.records.Count);
        }

        [Test]
        public void Progression_SecondRunNewBest_ReplacesRecordAndPaysNewBestBonus()
        {
            var profile = UserProfile.CreateDefault();
            var first = Run(50000, 20, 30, 0, 30, 10, 5, 3);
            ProgressionService.Apply(profile, first, GradeCalculator.Evaluate(first, GCfg, Norm), PCfg);

            var better = Run(120000, 45, 60, 1, 60, 10, 2, 1);
            var outcome = ProgressionService.Apply(profile, better, GradeCalculator.Evaluate(better, GCfg, Norm), PCfg);

            Assert.IsFalse(outcome.FirstClear);
            Assert.IsTrue(outcome.NewRecord);
            Assert.AreEqual(1, profile.records.Count, "same chart identity keeps one record");
            Assert.AreEqual(120000, profile.records[0].bestScore);
            Assert.AreEqual(PCfg.BaseHh + PCfg.NewBestBonusHh, outcome.HhEarned);
        }

        [Test]
        public void Progression_LevelsUpWhenXpCrossesThreshold()
        {
            var profile = UserProfile.CreateDefault();
            profile.xp = PCfg.XpPerLevel - 10; // just below level 2
            profile.level = 1 + (int)(profile.xp / PCfg.XpPerLevel);
            var run = Run(200000, 60, 120, 2, 90, 5, 0, 0); // S => 150 xp, crosses threshold
            var outcome = ProgressionService.Apply(profile, run, GradeCalculator.Evaluate(run, GCfg, Norm), PCfg);
            Assert.IsTrue(outcome.LeveledUp);
            Assert.AreEqual(2, outcome.LevelAfter);
        }

        [Test]
        public void Progression_GrantsNoGameplayPower()
        {
            // Progression only touches xp/level/hh/records — never any gameplay config.
            // (Structural: ProgressionService has no access to ScoringConfig/TimingConfig/neck.)
            var profile = UserProfile.CreateDefault();
            var run = Run(200000, 60, 120, 2, 90, 5, 0, 0);
            ProgressionService.Apply(profile, run, GradeCalculator.Evaluate(run, GCfg, Norm), PCfg);
            Assert.Greater(profile.xp, 0);
            Assert.Greater(profile.hhCurrency, 0);
            // No field on UserProfile represents timing windows / multipliers / neck strength.
        }
    }
}
