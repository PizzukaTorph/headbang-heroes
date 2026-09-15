namespace HeadbangHeroes.Gameplay
{
    public sealed class ComboScore
    {
        public int Combo { get; private set; }
        public long Score { get; private set; }

        public void Apply(Judgment judgment)
        {
            if (judgment == Judgment.Miss)
            {
                Combo = 0;
                return;
            }

            Combo++;
            var quality = judgment == Judgment.Perfect ? 1f :
                          judgment == Judgment.Great ? 0.85f : 0.60f;
            var multiplier = 1f + System.Math.Min(Combo, 100) / 100f;
            Score += (long)(1000f * quality * multiplier);
        }

        public void Reset()
        {
            Combo = 0;
            Score = 0;
        }
    }
}
