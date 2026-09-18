using HeadbangHeroes.Gameplay.Scoring;

namespace HeadbangHeroes.Meta
{
    public enum Grade { D, C, B, A, S }

    /// <summary>Normalization targets for one chart so different songs/difficulties are comparable.</summary>
    public readonly struct GradeNormalization
    {
        public readonly long TargetScore;
        public readonly int TargetLongestCombo;
        public readonly int TargetHype;
        public readonly int TargetFinishers;

        public GradeNormalization(long targetScore, int targetLongestCombo, int targetHype, int targetFinishers)
        {
            TargetScore = targetScore < 1 ? 1 : targetScore;
            TargetLongestCombo = targetLongestCombo < 1 ? 1 : targetLongestCombo;
            TargetHype = targetHype < 1 ? 1 : targetHype;
            TargetFinishers = targetFinishers < 1 ? 1 : targetFinishers;
        }
    }

    /// <summary>
    /// Data-driven grade weights + thresholds. Pure gameplay-meta data. The grade is a normalized
    /// weighted blend of multiple performance dimensions (not raw score alone), so a player cannot
    /// earn the top grade by playing safe without intensity, nor by aggression without consistency.
    /// Weights/thresholds are prototype defaults per SCORING_SYSTEM_V1; all tuning.
    /// </summary>
    public readonly struct GradeConfig
    {
        public readonly float ScoreWeight;
        public readonly float MaxComboWeight;
        public readonly float HypeWeight;
        public readonly float FinisherWeight;
        public readonly float CombosWeight;

        // Minimum normalized performance (0..1) required for each grade (checked high to low).
        public readonly float SThreshold;
        public readonly float AThreshold;
        public readonly float BThreshold;
        public readonly float CThreshold;

        public GradeConfig(
            float scoreWeight, float maxComboWeight, float hypeWeight, float finisherWeight, float combosWeight,
            float sThreshold, float aThreshold, float bThreshold, float cThreshold)
        {
            ScoreWeight = scoreWeight;
            MaxComboWeight = maxComboWeight;
            HypeWeight = hypeWeight;
            FinisherWeight = finisherWeight;
            CombosWeight = combosWeight;
            SThreshold = sThreshold;
            AThreshold = aThreshold;
            BThreshold = bThreshold;
            CThreshold = cThreshold;
        }

        public static GradeConfig Default => new GradeConfig(
            scoreWeight: 0.40f, maxComboWeight: 0.20f, hypeWeight: 0.20f, finisherWeight: 0.15f, combosWeight: 0.05f,
            sThreshold: 0.90f, aThreshold: 0.75f, bThreshold: 0.55f, cThreshold: 0.35f);
    }

    /// <summary>Result of grading a run: the letter plus the normalized 0..1 performance blend.</summary>
    public readonly struct GradeResult
    {
        public readonly Grade Grade;
        public readonly float Performance;   // 0..1 normalized blend

        public GradeResult(Grade grade, float performance)
        {
            Grade = grade;
            Performance = performance;
        }
    }

    /// <summary>
    /// Pure S/A/B/C/D grade calculation from an authoritative <see cref="RunResult"/>. It never
    /// recomputes gameplay scoring — it reads the finalized totals and normalizes them against the
    /// chart's targets, then blends by configured weights and maps to a letter by threshold.
    /// </summary>
    public static class GradeCalculator
    {
        public static GradeResult Evaluate(in RunResult result, in GradeConfig config, in GradeNormalization norm)
        {
            var score = Ratio(result.Score, norm.TargetScore);
            var combo = Ratio(result.LongestCombo, norm.TargetLongestCombo);
            var hype = Ratio(result.TotalHypeEarned, norm.TargetHype);
            var fin = Ratio(result.FinishersExecuted, norm.TargetFinishers);
            var combos = Ratio(result.CompletedCombos, norm.TargetLongestCombo); // reuse combo target as a proxy

            var perf =
                config.ScoreWeight * score +
                config.MaxComboWeight * combo +
                config.HypeWeight * hype +
                config.FinisherWeight * fin +
                config.CombosWeight * combos;

            perf = Clamp01(perf);

            Grade grade;
            if (perf >= config.SThreshold) grade = Grade.S;
            else if (perf >= config.AThreshold) grade = Grade.A;
            else if (perf >= config.BThreshold) grade = Grade.B;
            else if (perf >= config.CThreshold) grade = Grade.C;
            else grade = Grade.D;

            return new GradeResult(grade, perf);
        }

        static float Ratio(double value, double target)
        {
            if (target <= 0d) return 0f;
            var r = (float)(value / target);
            return r < 0f ? 0f : (r > 1f ? 1f : r);
        }

        static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
    }
}
