using System.Collections.Generic;
using HeadbangHeroes.Gameplay.Scoring;

namespace HeadbangHeroes.Meta
{
    /// <summary>Deterministic performance tags for result commentary selection (RESULTS_SCREEN_V1).</summary>
    public enum ResultTag
    {
        ZeroMiss,
        HugeCombo,
        HighHype,
        ManyPerfects,
        MultipleFinishers,
        NoFinisher,
        NoTheBang,
        ChaoticRun
    }

    /// <summary>
    /// Consumes an authoritative <see cref="RunResult"/> and produces the grade plus deterministic
    /// commentary tags. It NEVER recomputes gameplay score — it reads the finalized totals only
    /// (RESULTS_SCREEN_V1 data contract). Pure.
    /// </summary>
    public static class ResultsService
    {
        public static GradeResult Grade(in RunResult run, in GradeConfig config, in GradeNormalization norm)
            => GradeCalculator.Evaluate(run, config, norm);

        /// <summary>Derives lightweight, debuggable result tags from the authoritative totals.</summary>
        public static IReadOnlyList<ResultTag> DeriveTags(in RunResult run)
        {
            var tags = new List<ResultTag>(4);
            var total = run.PerfectCount + run.GreatCount + run.GoodCount + run.WellCount + run.MissCount;

            if (run.MissCount == 0 && total > 0) tags.Add(ResultTag.ZeroMiss);
            if (run.LongestCombo >= 30) tags.Add(ResultTag.HugeCombo);
            if (run.TotalHypeEarned >= 60) tags.Add(ResultTag.HighHype);
            if (total > 0 && run.PerfectCount >= total / 2) tags.Add(ResultTag.ManyPerfects);
            if (run.FinishersExecuted >= 2) tags.Add(ResultTag.MultipleFinishers);
            else if (run.FinishersExecuted == 0) tags.Add(ResultTag.NoFinisher);
            if (run.TheBangActivations == 0) tags.Add(ResultTag.NoTheBang);
            if (total > 0 && run.MissCount > total / 3) tags.Add(ResultTag.ChaoticRun);

            return tags;
        }
    }
}
