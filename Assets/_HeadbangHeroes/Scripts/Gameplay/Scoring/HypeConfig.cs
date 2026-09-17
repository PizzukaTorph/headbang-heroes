namespace HeadbangHeroes.Gameplay.Scoring
{
    /// <summary>How HYPE is generated while THE BANG is active (anti-runaway control).</summary>
    public enum HypeDuringTheBang
    {
        /// <summary>HYPE keeps accruing at its normal (un-amplified) rate.</summary>
        Normal,
        /// <summary>HYPE generation is suspended for the duration of THE BANG.</summary>
        Disabled
    }

    /// <summary>
    /// Data-driven HYPE / THE BANG / Finisher parameters (SCORING_SYSTEM_V1). Prototype defaults;
    /// all tuning. THE BANG reward multipliers deliberately default to 10x to make the limit-break
    /// unmistakable during testing, but are configuration, never magic constants in logic.
    ///
    /// Crucially, THE BANG reward multipliers apply ONLY to the reward domain (score/motion/
    /// finisher). HYPE generation is governed separately by <see cref="HypeMode"/> and is NEVER
    /// amplified by the reward multiplier, so THE BANG cannot recursively refill itself.
    /// </summary>
    public readonly struct HypeConfig
    {
        public readonly int PerfectHype;
        public readonly int GreatHype;
        public readonly int GoodHype;
        public readonly int MissHype;
        public readonly int MaxHype;

        /// <summary>Duration (seconds) of the enhanced THE BANG window once activated.</summary>
        public readonly double TheBangDuration;

        public readonly float TheBangScoreMultiplier;
        public readonly float TheBangMotionRewardMultiplier;
        public readonly float TheBangFinisherMultiplier;

        public readonly HypeDuringTheBang HypeMode;

        public HypeConfig(
            int perfectHype, int greatHype, int goodHype, int missHype, int maxHype,
            double theBangDuration,
            float theBangScoreMultiplier,
            float theBangMotionRewardMultiplier,
            float theBangFinisherMultiplier,
            HypeDuringTheBang hypeMode)
        {
            PerfectHype = perfectHype;
            GreatHype = greatHype;
            GoodHype = goodHype;
            MissHype = missHype;
            MaxHype = maxHype < 1 ? 1 : maxHype;
            TheBangDuration = theBangDuration <= 0d ? 8d : theBangDuration;
            TheBangScoreMultiplier = theBangScoreMultiplier;
            TheBangMotionRewardMultiplier = theBangMotionRewardMultiplier;
            TheBangFinisherMultiplier = theBangFinisherMultiplier;
            HypeMode = hypeMode;
        }

        public static HypeConfig Default => new HypeConfig(
            perfectHype: 2,
            greatHype: 1,
            goodHype: 0,
            missHype: 0,
            maxHype: 100,
            theBangDuration: 8d,
            theBangScoreMultiplier: 10f,
            theBangMotionRewardMultiplier: 10f,
            theBangFinisherMultiplier: 10f,
            hypeMode: HypeDuringTheBang.Normal);

        /// <summary>Base HYPE contribution for a judgment tier (never amplified by THE BANG).</summary>
        public int HypeForJudgment(Judgment judgment)
        {
            switch (judgment)
            {
                case Judgment.Perfect: return PerfectHype;
                case Judgment.Great: return GreatHype;
                case Judgment.Good: return GoodHype;
                default: return MissHype;
            }
        }
    }
}
