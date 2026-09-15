namespace HeadbangHeroes.Gameplay
{
    public sealed class ComboScore
    {
        public int Combo { get; private set; }
        public long Score { get; private set; }

        /// <summary>
        /// Applies a judged event. Score is driven by per-event performance
        /// (TimingQuality * MotionQuality), not by the judgment tier alone.
        /// A Miss breaks the combo and adds no score.
        /// </summary>
        public void Apply(JudgmentResult result)
        {
            if (result.judgment == Judgment.Miss)
            {
                Combo = 0;
                return;
            }

            Combo++;
            var performance = result.Performance;
            var multiplier = 1f + System.Math.Min(Combo, 100) / 100f;
            Score += (long)(1000f * performance * multiplier);
        }

        public void Reset()
        {
            Combo = 0;
            Score = 0;
        }
    }
}
