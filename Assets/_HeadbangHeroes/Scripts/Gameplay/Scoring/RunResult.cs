namespace HeadbangHeroes.Gameplay.Scoring
{
    /// <summary>
    /// Authoritative, immutable end-of-run summary consumed by Results/Progression. It carries
    /// enough to render the performance report and compute a grade downstream WITHOUT recomputing
    /// any gameplay scoring (RESULTS_SCREEN_V1 data contract). Results renders this; it never
    /// rebuilds it. The final grade is intentionally left to the Results/grade package (Plan 06).
    /// </summary>
    public readonly struct RunResult
    {
        // Identity / version (score attribution across chart/rules versions).
        public readonly string SongId;
        public readonly string ChartId;
        public readonly int ChartVersion;
        public readonly int RulesVersion;

        // Performance report.
        public readonly long Score;
        public readonly int PerfectCount;
        public readonly int GreatCount;
        public readonly int GoodCount;
        public readonly int WellCount;
        public readonly int MissCount;
        public readonly int LongestCombo;
        public readonly int CompletedCombos;
        public readonly int TotalHypeEarned;
        public readonly int TheBangActivations;
        public readonly int FinishersExecuted;

        public RunResult(
            string songId, string chartId, int chartVersion, int rulesVersion,
            long score,
            int perfectCount, int greatCount, int goodCount, int wellCount, int missCount,
            int longestCombo, int completedCombos,
            int totalHypeEarned, int theBangActivations, int finishersExecuted)
        {
            SongId = songId;
            ChartId = chartId;
            ChartVersion = chartVersion;
            RulesVersion = rulesVersion;
            Score = score;
            PerfectCount = perfectCount;
            GreatCount = greatCount;
            GoodCount = goodCount;
            WellCount = wellCount;
            MissCount = missCount;
            LongestCombo = longestCombo;
            CompletedCombos = completedCombos;
            TotalHypeEarned = totalHypeEarned;
            TheBangActivations = theBangActivations;
            FinishersExecuted = finishersExecuted;
        }
    }
}
