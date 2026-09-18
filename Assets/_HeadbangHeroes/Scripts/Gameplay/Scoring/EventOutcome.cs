namespace HeadbangHeroes.Gameplay.Scoring
{
    /// <summary>
    /// Authoritative outcome of one resolved scoring event, assembled from SEPARABLE dimensions.
    /// Timing, Motion Quality, technique/context and THE BANG/Finisher context are each preserved
    /// independently — no dimension relabels another (a PERFECT-timing / weak-motion event is still
    /// PERFECT timing with weak motion). Presentation/Results consume this; they never rebuild it.
    /// Immutable value type.
    /// </summary>
    public readonly struct EventOutcome
    {
        public readonly string EventId;

        // --- Timing dimension ---
        public readonly Judgment Judgment;
        public readonly double SignedTimingError;

        // --- Motion Quality dimension (0..1) ---
        public readonly float MotionQuality;
        public readonly bool WasSetupBang;

        // --- Technique/context factor (seam; 1.0 until technique scoring lands) ---
        public readonly float TechniqueFactor;

        // --- THE BANG / Finisher context that applied to THIS event (snapshotted before rewards) ---
        public readonly bool DuringTheBang;
        public readonly bool WasFinisher;

        // --- Derived contributions ---
        public readonly long ScoreContribution;
        public readonly int HypeContribution;
        public readonly int ComboAfter;
        public readonly int MultiplierAfter;

        public EventOutcome(
            string eventId,
            Judgment judgment,
            double signedTimingError,
            float motionQuality,
            bool wasSetupBang,
            float techniqueFactor,
            bool duringTheBang,
            bool wasFinisher,
            long scoreContribution,
            int hypeContribution,
            int comboAfter,
            int multiplierAfter)
        {
            EventId = eventId;
            Judgment = judgment;
            SignedTimingError = signedTimingError;
            MotionQuality = motionQuality;
            WasSetupBang = wasSetupBang;
            TechniqueFactor = techniqueFactor;
            DuringTheBang = duringTheBang;
            WasFinisher = wasFinisher;
            ScoreContribution = scoreContribution;
            HypeContribution = hypeContribution;
            ComboAfter = comboAfter;
            MultiplierAfter = multiplierAfter;
        }
    }
}
