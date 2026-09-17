using HeadbangHeroes.Charts;

namespace HeadbangHeroes.Gameplay.Timing
{
    /// <summary>
    /// An unresolved authored event exposed to the matcher. This is a lightweight, engine-free
    /// view over the immutable RuntimeChart's ordered motion events. <see cref="Id"/> is the
    /// resolver slot: a stable identity used both for deterministic tie-breaking and for marking
    /// the event resolved exactly once.
    /// </summary>
    public readonly struct MotionCandidate
    {
        public readonly int Id;
        public readonly double SongTime;
        public readonly BangDirection Direction;

        public MotionCandidate(int id, double songTime, BangDirection direction)
        {
            Id = id;
            SongTime = songTime;
            Direction = direction;
        }
    }

    /// <summary>Why/how a bang resolved (or did not) against the candidate set.</summary>
    public enum MatchKind
    {
        /// <summary>Input was earlier than any eligible candidate's window: nothing consumed.</summary>
        TooEarly,
        /// <summary>A compatible candidate resolved as a hit (PERFECT/GREAT/GOOD).</summary>
        Hit,
        /// <summary>The best eligible candidate was consumed as a MISS (wrong direction / late edge).</summary>
        WrongConsumedMiss
    }

    /// <summary>Deterministic result of matching one <see cref="BangInput"/> against candidates.</summary>
    public readonly struct MatchResult
    {
        public readonly MatchKind Kind;
        public readonly int MatchedId;          // -1 when nothing consumed
        public readonly double SignedError;     // input.SongTime - event.SongTime
        public readonly Judgment Judgment;      // Miss when nothing/again miss

        public bool Consumed => Kind != MatchKind.TooEarly;

        public MatchResult(MatchKind kind, int matchedId, double signedError, Judgment judgment)
        {
            Kind = kind;
            MatchedId = matchedId;
            SignedError = signedError;
            Judgment = judgment;
        }

        public static MatchResult NoneTooEarly => new MatchResult(MatchKind.TooEarly, -1, 0d, Judgment.Miss);
    }
}
