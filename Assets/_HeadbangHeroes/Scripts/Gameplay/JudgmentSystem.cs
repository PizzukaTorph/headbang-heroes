namespace HeadbangHeroes.Gameplay
{
    public enum Judgment { Perfect, Great, Good, Well, Miss }

    public readonly struct JudgmentResult
    {
        public readonly Judgment judgment;
        /// <summary>Signed timing error in seconds (input song-time - event song-time). Negative = early.</summary>
        public readonly double error;
        /// <summary>0..1 timing quality derived from the judgment tier.</summary>
        public readonly float timingQuality;
        /// <summary>0..1 motion quality sampled from the neck's pre-inversion arrival.</summary>
        public readonly float motionQuality;

        /// <summary>Per-event performance = TimingQuality * MotionQuality (0..1).</summary>
        public float Performance => timingQuality * motionQuality;

        public JudgmentResult(Judgment judgment, double error, float motionQuality = 1f)
        {
            this.judgment = judgment;
            this.error = error;
            this.timingQuality = JudgmentSystem.TimingQuality(judgment);
            this.motionQuality = motionQuality;
        }

        /// <summary>An event the player never acted on (expired late). No timing/motion evidence.</summary>
        public static JudgmentResult ExpiredMiss() => new JudgmentResult(Judgment.Miss, 0d, 0f);
    }

    /// <summary>
    /// Judgment tier semantics. Timing WINDOWS now live in data-driven
    /// <see cref="Timing.TimingConfig"/> and matching lives in <see cref="Timing.EventMatcher"/>;
    /// this type only maps a tier to its normalized timing-quality weight.
    /// </summary>
    public static class JudgmentSystem
    {
        public const float PerfectQuality = 1.00f;
        public const float GreatQuality = 0.85f;
        public const float GoodQuality = 0.60f;
        public const float WellQuality = 0.25f;   // sloppy-but-there: still credited, low quality
        public const float MissQuality = 0.00f;

        public static float TimingQuality(Judgment judgment)
        {
            switch (judgment)
            {
                case Judgment.Perfect: return PerfectQuality;
                case Judgment.Great: return GreatQuality;
                case Judgment.Good: return GoodQuality;
                case Judgment.Well: return WellQuality;
                default: return MissQuality;
            }
        }
    }
}
