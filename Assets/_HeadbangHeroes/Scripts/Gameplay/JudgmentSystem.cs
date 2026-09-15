using System;

namespace HeadbangHeroes.Gameplay
{
    public enum Judgment { Perfect, Great, Good, Miss }

    public readonly struct JudgmentResult
    {
        public readonly Judgment judgment;
        public readonly double error;
        public JudgmentResult(Judgment judgment, double error)
        {
            this.judgment = judgment;
            this.error = error;
        }
    }

    public static class JudgmentSystem
    {
        public const double Perfect = 0.035;
        public const double Great = 0.070;
        public const double Good = 0.120;

        public static JudgmentResult Evaluate(double signedError)
        {
            var e = Math.Abs(signedError);
            var j = e <= Perfect ? Judgment.Perfect :
                    e <= Great ? Judgment.Great :
                    e <= Good ? Judgment.Good : Judgment.Miss;
            return new JudgmentResult(j, signedError);
        }
    }
}
