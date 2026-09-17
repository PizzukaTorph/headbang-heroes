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

        /// <summary>|error| &lt;= this ⇒ GOOD (when &gt; GreatWindow). Beyond this a hit is a MISS.</summary>
        public readonly double GoodWindow;

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
            double candidateLead,
            double lateExpiry)
        {
            PerfectWindow = perfectWindow;
            GreatWindow = greatWindow;
            GoodWindow = goodWindow;
            CandidateLead = candidateLead;
            LateExpiry = lateExpiry;
        }

        /// <summary>Canonical M0-derived defaults (35/70/120 ms windows, 1.0 s candidate lead).</summary>
        public static TimingConfig Default => new TimingConfig(
            perfectWindow: 0.035,
            greatWindow: 0.070,
            goodWindow: 0.120,
            candidateLead: 1.0,
            lateExpiry: 0.120);

        /// <summary>Classifies an absolute timing error into a judgment tier (hit-only; no MISS gating here).</summary>
        public Judgment Classify(double signedError)
        {
            var e = signedError < 0 ? -signedError : signedError;
            if (e <= PerfectWindow) return Judgment.Perfect;
            if (e <= GreatWindow) return Judgment.Great;
            if (e <= GoodWindow) return Judgment.Good;
            return Judgment.Miss;
        }
    }
}
