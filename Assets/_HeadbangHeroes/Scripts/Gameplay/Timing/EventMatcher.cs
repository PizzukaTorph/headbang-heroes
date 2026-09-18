using System.Collections.Generic;

namespace HeadbangHeroes.Gameplay.Timing
{
    /// <summary>
    /// Pure, deterministic matcher: resolves one <see cref="BangInput"/> against a bounded set of
    /// unresolved <see cref="MotionCandidate"/>s. No Unity dependencies, no side effects on the
    /// candidate list — the caller marks the returned <see cref="MatchResult.MatchedId"/> resolved.
    ///
    /// Selection policy (documented + test-covered):
    ///   1. A candidate is JUDGEABLE for this input when
    ///          -GoodWindow &lt;= (input.SongTime - event.SongTime) &lt;= LateExpiry.
    ///      (Earlier than the GOOD early edge = still too early to consume.)
    ///   2. Among judgeable candidates, prefer DIRECTION-COMPATIBLE ones.
    ///      Pick the smallest |error|; tie-break by earliest authored time, then smallest Id.
    ///   3. If no compatible judgeable candidate exists but a judgeable candidate does,
    ///      consume the nearest one (same tie-break) as a wrong-direction MISS.
    ///   4. If no candidate is judgeable (all still too far in the future), consume nothing
    ///      (TooEarly). The neck has already moved regardless; that is the caller's concern.
    /// One authored event is only ever offered once because resolved events leave the set.
    /// </summary>
    public static class EventMatcher
    {
        public static MatchResult Match(in BangInput input, IReadOnlyList<MotionCandidate> candidates, in TimingConfig config)
        {
            if (candidates == null || candidates.Count == 0)
                return MatchResult.NoneTooEarly;

            var haveCompatible = false;
            MotionCandidate bestCompatible = default;
            double bestCompatibleAbs = double.MaxValue;

            var haveAny = false;
            MotionCandidate bestAny = default;
            double bestAnyAbs = double.MaxValue;

            foreach (var c in candidates)
            {
                var error = input.SongTime - c.SongTime;

                // Judgeable window: not earlier than the WELL early edge, not past the late expiry.
                // A cue WAS present within this window — even a poorly-timed tap consumes it (WELL)
                // rather than passing through as a no-cue MISS.
                if (error < -config.WellWindow || error > config.LateExpiry)
                    continue;

                var abs = error < 0 ? -error : error;
                var compatible = c.Direction == input.Direction;

                if (compatible)
                {
                    if (IsBetter(abs, c, bestCompatibleAbs, bestCompatible, haveCompatible))
                    {
                        bestCompatible = c;
                        bestCompatibleAbs = abs;
                        haveCompatible = true;
                    }
                }

                if (IsBetter(abs, c, bestAnyAbs, bestAny, haveAny))
                {
                    bestAny = c;
                    bestAnyAbs = abs;
                    haveAny = true;
                }
            }

            if (haveCompatible)
            {
                var error = input.SongTime - bestCompatible.SongTime;
                var judgment = config.Classify(error);
                // Inside the judgeable window Classify returns a real tier (PERFECT..WELL); it can
                // only return MISS beyond WellWindow, which cannot happen here (|error| <= WellWindow),
                // so this is always a credited hit tier.
                return new MatchResult(MatchKind.Hit, bestCompatible.Id, error, judgment);
            }

            if (haveAny)
            {
                var error = input.SongTime - bestAny.SongTime;
                return new MatchResult(MatchKind.WrongConsumedMiss, bestAny.Id, error, Judgment.Miss);
            }

            return MatchResult.NoneTooEarly;
        }

        /// <summary>
        /// Deterministic "is candidate better than current best" test:
        /// smaller |error| wins; ties break to earlier authored time, then smaller Id.
        /// </summary>
        static bool IsBetter(double abs, in MotionCandidate c, double bestAbs, in MotionCandidate best, bool haveBest)
        {
            if (!haveBest) return true;
            if (abs < bestAbs - 1e-12) return true;
            if (abs > bestAbs + 1e-12) return false;
            // |error| tie: earliest authored time.
            if (c.SongTime < best.SongTime - 1e-12) return true;
            if (c.SongTime > best.SongTime + 1e-12) return false;
            // still tied: smallest stable Id.
            return c.Id < best.Id;
        }
    }
}
