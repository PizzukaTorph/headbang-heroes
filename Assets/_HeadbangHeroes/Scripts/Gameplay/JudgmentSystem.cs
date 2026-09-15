using System;

namespace HeadbangHeroes.Gameplay
{
    public enum Judgment { Perfect, Great, Good, Miss }

    public readonly struct JudgmentResult
    {
        public readonly Judgment judgment;
        /// <summary>Signed timing error in seconds (input time - event time). Negative = early.</summary>
        public readonly double error;
        /// <summary>0..1 timing quality derived from the judgment tier.</summary>
        public readonly float timingQuality;
        /// <summary>0..1 motion quality sampled from the head motion at input time.</summary>
        public readonly float motionQuality;

        /// <summary>M0 per-event performance = TimingQuality * MotionQuality (0..1).</summary>
        public float Performance => timingQuality * motionQuality;

        public JudgmentResult(Judgment judgment, double error, float motionQuality = 1f)
        {
            this.judgment = judgment;
            this.error = error;
            this.timingQuality = JudgmentSystem.TimingQuality(judgment);
            this.motionQuality = motionQuality;
        }
    }

    public static class JudgmentSystem
    {
        // Absolute timing windows in seconds.
        public const double Perfect = 0.035;
        public const double Great = 0.070;
        public const double Good = 0.120;

        // Timing quality per tier (used for EventPerformance and scoring).
        public const float PerfectQuality = 1.00f;
        public const float GreatQuality = 0.85f;
        public const float GoodQuality = 0.60f;
        public const float MissQuality = 0.00f;

        public static JudgmentResult Evaluate(double signedError, float motionQuality = 1f)
        {
            var e = Math.Abs(signedError);
            var j = e <= Perfect ? Judgment.Perfect :
                    e <= Great ? Judgment.Great :
                    e <= Good ? Judgment.Good : Judgment.Miss;
            return new JudgmentResult(j, signedError, motionQuality);
        }

        public static float TimingQuality(Judgment judgment)
        {
            switch (judgment)
            {
                case Judgment.Perfect: return PerfectQuality;
                case Judgment.Great: return GreatQuality;
                case Judgment.Good: return GoodQuality;
                default: return MissQuality;
            }
        }

        /// <summary>
        /// Outcome of attempting to judge an active event against a directional input.
        /// </summary>
        public enum JudgeOutcome
        {
            /// <summary>Input was earlier than the Good window: leave the event active, do not consume.</summary>
            TooEarlyKeep,
            /// <summary>Input judged (Perfect/Great/Good or wrong-direction Miss): consume the event.</summary>
            Consumed
        }

        /// <summary>
        /// Pure decision used by the scheduler. Given a signed timing error, whether the input
        /// direction matched, and the motion quality, decides whether the event is consumed and
        /// produces the resulting judgment. No Unity dependencies, so it is unit-testable.
        /// </summary>
        public static JudgeOutcome ResolveInput(double signedError, bool directionMatches, float motionQuality, out JudgmentResult result)
        {
            // Too early to count: do not consume.
            if (signedError < -Good)
            {
                result = default;
                return JudgeOutcome.TooEarlyKeep;
            }

            result = Evaluate(signedError, motionQuality);
            if (!directionMatches)
                result = new JudgmentResult(Judgment.Miss, signedError, motionQuality);

            return JudgeOutcome.Consumed;
        }
    }
}
