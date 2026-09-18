using System;

namespace HeadbangHeroes.Meta
{
    /// <summary>
    /// Per-chart personal best record. Identity is (songId, chartId, chartVersion, rulesVersion):
    /// records are only comparable when ALL FOUR match. Two records for different chart/rules
    /// versions are never blindly compared (SAVE_PROFILE_V1). Serializable value fields.
    /// </summary>
    [Serializable]
    public struct ChartRecord
    {
        public string songId;
        public string chartId;
        public int chartVersion;
        public int rulesVersion;

        public long bestScore;
        public int bestGrade;        // stored as (int)Grade for JsonUtility friendliness
        public int longestCombo;
        public int bestFinisherCount;
        public bool firstClear;
        public bool firstS;

        public Grade BestGrade => (Grade)bestGrade;

        /// <summary>True when the two records refer to the same comparable chart+rules identity.</summary>
        public bool IsCompatibleWith(in ChartRecord other)
            => string.Equals(songId, other.songId, StringComparison.Ordinal)
            && string.Equals(chartId, other.chartId, StringComparison.Ordinal)
            && chartVersion == other.chartVersion
            && rulesVersion == other.rulesVersion;

        public bool Matches(string song, string chart, int chartVer, int rulesVer)
            => string.Equals(songId, song, StringComparison.Ordinal)
            && string.Equals(chartId, chart, StringComparison.Ordinal)
            && chartVersion == chartVer
            && rulesVersion == rulesVer;

        /// <summary>
        /// Returns the better of two COMPATIBLE records (best-of each tracked metric). Throws if the
        /// records are incompatible — the caller must never merge across chart/rules versions.
        /// </summary>
        public static ChartRecord MergeBest(in ChartRecord a, in ChartRecord b)
        {
            if (!a.IsCompatibleWith(b))
                throw new InvalidOperationException("Cannot merge records across incompatible chart/rules versions.");

            return new ChartRecord
            {
                songId = a.songId,
                chartId = a.chartId,
                chartVersion = a.chartVersion,
                rulesVersion = a.rulesVersion,
                bestScore = a.bestScore >= b.bestScore ? a.bestScore : b.bestScore,
                bestGrade = a.bestGrade >= b.bestGrade ? a.bestGrade : b.bestGrade,
                longestCombo = a.longestCombo >= b.longestCombo ? a.longestCombo : b.longestCombo,
                bestFinisherCount = a.bestFinisherCount >= b.bestFinisherCount ? a.bestFinisherCount : b.bestFinisherCount,
                firstClear = a.firstClear || b.firstClear,
                firstS = a.firstS || b.firstS
            };
        }
    }
}
