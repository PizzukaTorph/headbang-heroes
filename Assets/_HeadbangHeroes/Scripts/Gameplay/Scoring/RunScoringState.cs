namespace HeadbangHeroes.Gameplay.Scoring
{
    /// <summary>
    /// Authoritative run scoring state: score total, canonical combo, configurable multiplier and
    /// judgment counts. Pure C#. Combo/multiplier follow SCORING_SYSTEM_V1 exactly:
    ///   PERFECT/GREAT -> combo +1, multiplier progress
    ///   GOOD          -> combo ENDS (records a completed combo), multiplier progress
    ///   WELL          -> combo ENDS, NO multiplier progress (sloppy but credited, not a miss)
    ///   MISS          -> combo reset to 0, multiplier progress reset
    /// </summary>
    public sealed class RunScoringState
    {
        ScoringConfig config;

        long score;
        int combo;
        int longestCombo;
        int completedCombos;
        int multiplierHits;     // successful hits since last MISS (drives the multiplier)

        int perfect, great, good, well, miss;

        public RunScoringState(ScoringConfig config) => this.config = config;

        public void SetConfig(ScoringConfig value) => config = value;

        public long Score => score;
        public int Combo => combo;
        public int LongestCombo => longestCombo;
        public int CompletedCombos => completedCombos;
        public int Multiplier => config.MultiplierForHits(multiplierHits);
        public int PerfectCount => perfect;
        public int GreatCount => great;
        public int GoodCount => good;
        public int WellCount => well;
        public int MissCount => miss;

        public void Reset()
        {
            score = 0; combo = 0; longestCombo = 0; completedCombos = 0; multiplierHits = 0;
            perfect = great = good = well = miss = 0;
        }

        /// <summary>
        /// Applies the combo/multiplier transition for a judgment and returns the multiplier that
        /// applies to THIS event (computed AFTER the transition, so a hit uses its own advanced
        /// multiplier and a MISS uses the reset value). Score is added by the caller via
        /// <see cref="AddScore"/> so the separable factors stay explicit in the orchestrator.
        /// </summary>
        public void ApplyJudgment(Judgment judgment)
        {
            switch (judgment)
            {
                case Judgment.Perfect:
                    perfect++; combo++; multiplierHits++; TrackLongest(); break;
                case Judgment.Great:
                    great++; combo++; multiplierHits++; TrackLongest(); break;
                case Judgment.Good:
                    good++; EndCombo(); multiplierHits++; break;   // GOOD ends combo but advances multiplier
                case Judgment.Well:
                    well++; EndCombo(); break;                     // WELL: sloppy hit — ends combo, no multiplier progress, but not a miss
                case Judgment.Miss:
                    miss++; combo = 0; multiplierHits = 0; break;  // MISS resets combo AND multiplier
            }
        }

        public void AddScore(long amount) => score += amount < 0 ? 0 : amount;

        void TrackLongest()
        {
            if (combo > longestCombo) longestCombo = combo;
        }

        void EndCombo()
        {
            if (combo > 0) completedCombos++;
            if (combo > longestCombo) longestCombo = combo;
            combo = 0;
        }
    }
}
