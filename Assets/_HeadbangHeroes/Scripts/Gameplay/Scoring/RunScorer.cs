namespace HeadbangHeroes.Gameplay.Scoring
{
    /// <summary>A fully-resolved event ready for scoring (timing + physical/semantic evidence).</summary>
    public readonly struct ResolvedEvent
    {
        public readonly string EventId;
        public readonly Judgment Judgment;
        public readonly double SignedTimingError;
        public readonly float MotionQuality;      // 0..1 (from MotionQualityEvaluator)
        public readonly bool WasSetupBang;
        public readonly bool FinisherCandidate;   // authored finisherCandidate flag
        public readonly float TechniqueFactor;    // seam; 1.0 until technique scoring exists

        public ResolvedEvent(string eventId, Judgment judgment, double signedTimingError,
            float motionQuality, bool wasSetupBang, bool finisherCandidate, float techniqueFactor = 1f)
        {
            EventId = eventId;
            Judgment = judgment;
            SignedTimingError = signedTimingError;
            MotionQuality = motionQuality;
            WasSetupBang = wasSetupBang;
            FinisherCandidate = finisherCandidate;
            TechniqueFactor = techniqueFactor;
        }
    }

    /// <summary>
    /// Domain orchestrator that turns a <see cref="ResolvedEvent"/> into an authoritative
    /// <see cref="EventOutcome"/> and updates run state (score/combo/multiplier + HYPE/THE BANG/
    /// Finisher). Pure C#. It composes <see cref="RunScoringState"/> and <see cref="HypeSystem"/>
    /// but neither recalculates the other's domain.
    ///
    /// Anti-circularity: the reward context (THE BANG multipliers, Finisher availability) is
    /// snapshotted BEFORE any reward is applied, and HYPE generation is always the base
    /// un-amplified value — so scoring THE BANG cannot change the context of the same event nor
    /// let THE BANG recursively refill itself.
    /// </summary>
    public sealed class RunScorer
    {
        readonly RunScoringState scoring;
        readonly HypeSystem hype;
        ScoringConfig scoringConfig;
        HypeConfig hypeConfig;

        int totalHypeEarned;

        public RunScorer(ScoringConfig scoringConfig, HypeConfig hypeConfig)
        {
            this.scoringConfig = scoringConfig;
            this.hypeConfig = hypeConfig;
            scoring = new RunScoringState(scoringConfig);
            hype = new HypeSystem(hypeConfig);
        }

        public RunScoringState Scoring => scoring;
        public HypeSystem Hype => hype;
        public int TotalHypeEarned => totalHypeEarned;

        public void SetConfig(ScoringConfig scoringCfg, HypeConfig hypeCfg)
        {
            scoringConfig = scoringCfg;
            hypeConfig = hypeCfg;
            scoring.SetConfig(scoringCfg);
            hype.SetConfig(hypeCfg);
        }

        public void Reset()
        {
            scoring.Reset();
            hype.Reset();
            totalHypeEarned = 0;
        }

        /// <summary>Advances time-based state (THE BANG expiry). Call with authoritative song-time.</summary>
        public void Advance(double songTime) => hype.Advance(songTime);

        /// <summary>Manually activate THE BANG (only from READY). Returns success.</summary>
        public bool TryActivateTheBang(double songTime) => hype.TryActivateTheBang(songTime);

        /// <summary>
        /// Resolves one event into an authoritative outcome, applying score/combo/multiplier/HYPE.
        /// </summary>
        public EventOutcome Resolve(in ResolvedEvent e)
        {
            // 1) Snapshot the reward context BEFORE applying anything from this event.
            var ctx = hype.CaptureRewardContext();

            var isHit = e.Judgment != Judgment.Miss;

            // 2) Finisher: the first successfully-performed authored candidate during THE BANG.
            var isFinisher = isHit && e.FinisherCandidate && ctx.FinisherAvailable;

            // 3) Combo / multiplier transition, then read the multiplier that applies to THIS event.
            scoring.ApplyJudgment(e.Judgment);
            var multiplier = scoring.Multiplier;

            // 4) Separable score factors (no dimension rewrites another).
            long scoreContribution = 0;
            if (isHit)
            {
                var timing = scoringConfig.TimingFactor(e.Judgment);
                var motion = scoringConfig.MotionFactor(e.MotionQuality);
                var raw = scoringConfig.BaseScore * timing * motion * e.TechniqueFactor * multiplier;
                if (ctx.DuringTheBang) raw *= ctx.ScoreMultiplier;
                if (isFinisher) raw *= ctx.FinisherMultiplier;
                scoreContribution = (long)raw;
                scoring.AddScore(scoreContribution);
            }

            // 5) HYPE — always the base value, never amplified by THE BANG reward multipliers.
            var hypeAdded = hype.AddHype(hypeConfig.HypeForJudgment(e.Judgment));
            totalHypeEarned += hypeAdded;

            // 6) Consume the Finisher for this window (once).
            if (isFinisher) hype.MarkFinisherExecuted();

            return new EventOutcome(
                e.EventId,
                e.Judgment,
                e.SignedTimingError,
                e.MotionQuality,
                e.WasSetupBang,
                e.TechniqueFactor,
                ctx.DuringTheBang,
                isFinisher,
                scoreContribution,
                hypeAdded,
                scoring.Combo,
                multiplier);
        }

        /// <summary>Builds the authoritative immutable run result. Aggregates state; does not recompute.</summary>
        public RunResult BuildResult(string songId, string chartId, int chartVersion, int rulesVersion)
        {
            return new RunResult(
                songId, chartId, chartVersion, rulesVersion,
                scoring.Score,
                scoring.PerfectCount, scoring.GreatCount, scoring.GoodCount, scoring.MissCount,
                scoring.LongestCombo, scoring.CompletedCombos,
                totalHypeEarned, hype.TheBangActivations, hype.FinishersExecuted);
        }
    }
}
