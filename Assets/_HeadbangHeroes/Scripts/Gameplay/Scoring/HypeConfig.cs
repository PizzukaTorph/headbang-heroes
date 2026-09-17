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

        /// <summary>Minimum timing tier for an authored candidate to become a Finisher (default GREAT).</summary>
        public readonly Judgment FinisherMinJudgment;

        public HypeConfig(
            int perfectHype, int greatHype, int goodHype, int missHype, int maxHype,
            double theBangDuration,
            float theBangScoreMultiplier,
            float theBangMotionRewardMultiplier,
            float theBangFinisherMultiplier,
            HypeDuringTheBang hypeMode,
            Judgment finisherMinJudgment = Judgment.Great)
        {
            PerfectHype = perfectHype;
            GreatHype = greatHype;
            GoodHype = goodHype;
            MissHype = missHype;
            MaxHype = maxHype < 1 ? 1 : maxHype;
            TheBangDuration = theBangDuration <= 0d ? 8d : theBangDuration;
            TheBangScoreMultiplier = SanitizeMultiplier(theBangScoreMultiplier);
            TheBangMotionRewardMultiplier = SanitizeMultiplier(theBangMotionRewardMultiplier);
            TheBangFinisherMultiplier = SanitizeMultiplier(theBangFinisherMultiplier);
            HypeMode = hypeMode;
            FinisherMinJudgment = finisherMinJudgment;
        }

        // A reward multiplier must be a finite, >= 1 value (never shrinks or nullifies reward).
        static float SanitizeMultiplier(float v) => (float.IsNaN(v) || v < 1f) ? 1f : v;

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

        /// <summary>True when the timing tier is good enough to be a Finisher (Perfect &gt; Great &gt; Good &gt; Well &gt; Miss).</summary>
        public bool JudgmentQualifiesForFinisher(Judgment judgment)
            => TierRank(judgment) >= TierRank(FinisherMinJudgment);

        static int TierRank(Judgment j)
        {
            switch (j)
            {
                case Judgment.Perfect: return 4;
                case Judgment.Great: return 3;
                case Judgment.Good: return 2;
                case Judgment.Well: return 1;
                default: return 0; // Miss
            }
        }
    }
}
