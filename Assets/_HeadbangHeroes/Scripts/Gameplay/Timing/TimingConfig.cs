namespace HeadbangHeroes.Gameplay.Timing
{
    /// <summary>
    /// Data-driven timing windows for judgment and candidate matching. Pure gameplay-domain data
    /// (no Unity refs, no static mutable state). All values are in seconds and measured as the
    /// absolute distance between the input song-time and an authored event song-time.
    ///
    /// Replaces the previous hardcoded <c>JudgmentSystem</c> constants. The defaults preserve the
    /// M0 tuning so behavior is unchanged until real-device calibration adjusts them.
    /// </summary>
    public readonly struct TimingConfig
    {
        /// <summary>|error| &lt;= this ⇒ PERFECT.</summary>
        public readonly double PerfectWindow;

        /// <summary>|error| &lt;= this ⇒ GREAT (when &gt; PerfectWindow).</summary>
        public readonly double GreatWindow;

        /// <summary>|error| &lt;= this ⇒ GOOD (when &gt; GreatWindow).</summary>
        public readonly double GoodWindow;

        /// <summary>
        /// |error| &lt;= this ⇒ WELL (when &gt; GoodWindow): a sloppy-but-present hit that still
        /// scores. Beyond this a consumed candidate is a genuine MISS. This is the outer edge of
        /// the judgeable/consume window — a cue WAS there, the player just timed it poorly.
        /// </summary>
        public readonly double WellWindow;

        /// <summary>
        /// How far AHEAD of an event (seconds) it becomes an eligible matching candidate.
        /// An input earlier than this relative to every candidate consumes nothing (but still
        /// moves the neck). Also drives cue activation horizon.
        /// </summary>
        public readonly double CandidateLead;

        /// <summary>
        /// How far PAST an event's time (seconds) it may still be matched before it expires.
        /// Equal to GoodWindow by default: a late input inside GOOD still resolves; beyond it the
        /// unresolved event expires as MISS.
        /// </summary>
        public readonly double LateExpiry;

        public TimingConfig(
            double perfectWindow,
            double greatWindow,
            double goodWindow,
            double wellWindow,
            double candidateLead,
            double lateExpiry)
        {
            PerfectWindow = perfectWindow;
            GreatWindow = greatWindow;
            GoodWindow = goodWindow;
            WellWindow = wellWindow;
            CandidateLead = candidateLead;
            LateExpiry = lateExpiry;
        }

        /// <summary>
        /// Generous prototype defaults. M0 optimizes for "is the headbang fun", so being roughly on
        /// the beat reads as Good/Great; WELL is the sloppy edge (1 point); a consumed cue is only a
        /// MISS when the player is completely early/late (beyond WELL), wrong-direction, or no cue.
        /// </summary>
        public static TimingConfig Default => new TimingConfig(
            perfectWindow: 0.070,
            greatWindow: 0.130,
            goodWindow: 0.190,
            wellWindow: 0.240,
            candidateLead: 1.0,
            lateExpiry: 0.240);

        /// <summary>Classifies an absolute timing error into a judgment tier (hit-only; no MISS gating here).</summary>
        public Judgment Classify(double signedError)
        {
            var e = signedError < 0 ? -signedError : signedError;
            if (e <= PerfectWindow) return Judgment.Perfect;
            if (e <= GreatWindow) return Judgment.Great;
            if (e <= GoodWindow) return Judgment.Good;
            if (e <= WellWindow) return Judgment.Well;
            return Judgment.Miss;
        }
    }
}
