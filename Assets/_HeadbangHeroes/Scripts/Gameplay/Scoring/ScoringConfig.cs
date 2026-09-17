namespace HeadbangHeroes.Gameplay.Scoring
{
    /// <summary>
    /// Data-driven scoring parameters. Pure gameplay-domain data (no Unity refs, no static mutable
    /// state). Numbers are prototype defaults per SCORING_SYSTEM_V1; all are tuning, not sacred.
    ///
    /// Event score is assembled from separable factors (SCORING_SYSTEM_V1 "Event scoring architecture"):
    ///   EventScore = BaseScore x TimingFactor x MotionQualityFactor x TechniqueFactor x Multiplier x TheBangModifier
    /// Each factor stays independently configurable and inspectable.
    /// </summary>
    public readonly struct ScoringConfig
    {
        public readonly float BaseScore;

        // Timing tier factors (Timing judges WHEN the player acted).
        public readonly float PerfectTimingFactor;
        public readonly float GreatTimingFactor;
        public readonly float GoodTimingFactor;

        // Motion Quality factor curve: final = MotionFloor + (1-MotionFloor)*motionQuality (0..1).
        public readonly float MotionFloor;

        // Multiplier progression: successful hits advance progress; every step of
        // HitsPerMultiplierStep raises the multiplier by 1 up to MaxMultiplier. MISS resets progress.
        public readonly int HitsPerMultiplierStep;
        public readonly int MaxMultiplier;

        public ScoringConfig(
            float baseScore,
            float perfectTimingFactor,
            float greatTimingFactor,
            float goodTimingFactor,
            float motionFloor,
            int hitsPerMultiplierStep,
            int maxMultiplier)
        {
            BaseScore = Sanitize(baseScore);
            PerfectTimingFactor = Sanitize(perfectTimingFactor);
            GreatTimingFactor = Sanitize(greatTimingFactor);
            GoodTimingFactor = Sanitize(goodTimingFactor);
            MotionFloor = motionFloor < 0f ? 0f : (motionFloor > 1f ? 1f : motionFloor);
            HitsPerMultiplierStep = hitsPerMultiplierStep < 1 ? 1 : hitsPerMultiplierStep;
            MaxMultiplier = maxMultiplier < 1 ? 1 : maxMultiplier;
        }

        // Guards misconfiguration: NaN/negative factors would silently produce 0/negative scores.
        static float Sanitize(float v) => (float.IsNaN(v) || v < 0f) ? 0f : v;

        public static ScoringConfig Default => new ScoringConfig(
            baseScore: 1000f,
            perfectTimingFactor: 1.00f,
            greatTimingFactor: 0.85f,
            goodTimingFactor: 0.60f,
            motionFloor: 0.5f,        // motion scales score between 0.5x and 1.0x; timing never erased
            hitsPerMultiplierStep: 10,
            maxMultiplier: 5);

        /// <summary>Timing factor for a judgment tier (MISS has no positive contribution).</summary>
        public float TimingFactor(Judgment judgment)
        {
            switch (judgment)
            {
                case Judgment.Perfect: return PerfectTimingFactor;
                case Judgment.Great: return GreatTimingFactor;
                case Judgment.Good: return GoodTimingFactor;
                default: return 0f;
            }
        }

        /// <summary>Motion factor in [MotionFloor, 1]; motion never fully erases a good timing hit.</summary>
        public float MotionFactor(float motionQuality)
        {
            var q = motionQuality < 0f ? 0f : (motionQuality > 1f ? 1f : motionQuality);
            return MotionFloor + (1f - MotionFloor) * q;
        }

        /// <summary>Multiplier for a given count of successful hits since the last reset (1..MaxMultiplier).</summary>
        public int MultiplierForHits(int successfulHits)
        {
            var m = 1 + successfulHits / HitsPerMultiplierStep;
            return m > MaxMultiplier ? MaxMultiplier : m;
        }
    }
}
