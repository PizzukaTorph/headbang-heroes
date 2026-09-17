namespace HeadbangHeroes.Gameplay.Scoring
{
    /// <summary>
    /// Immutable snapshot of the reward context that applies to a single event, captured BEFORE
    /// that event's rewards are applied. Passing this by value into outcome assembly guarantees an
    /// event cannot change the context under which it is being scored (anti-circularity).
    /// </summary>
    public readonly struct RewardContext
    {
        public readonly bool DuringTheBang;
        public readonly float ScoreMultiplier;
        public readonly float MotionRewardMultiplier;
        public readonly float FinisherMultiplier;
        public readonly bool FinisherAvailable;   // THE BANG active and no Finisher taken yet this window
        public readonly HypeDuringTheBang HypeMode;

        public RewardContext(bool duringTheBang, float scoreMultiplier, float motionRewardMultiplier,
            float finisherMultiplier, bool finisherAvailable, HypeDuringTheBang hypeMode)
        {
            DuringTheBang = duringTheBang;
            ScoreMultiplier = scoreMultiplier;
            MotionRewardMultiplier = motionRewardMultiplier;
            FinisherMultiplier = finisherMultiplier;
            FinisherAvailable = finisherAvailable;
            HypeMode = hypeMode;
        }
    }

    /// <summary>
    /// Owns HYPE, READY, THE BANG lifecycle and Finisher-window state. Pure C#, advanced by
    /// authoritative song-time (never render frames). It does NOT compute score — the orchestrator
    /// reads a <see cref="RewardContext"/> from here, applies scoring, then reports back HYPE
    /// contributions and Finisher consumption. THE BANG reward multipliers never amplify HYPE.
    /// </summary>
    public sealed class HypeSystem
    {
        HypeConfig config;

        int hype;
        bool theBangActive;
        double theBangEndSongTime;
        bool finisherTakenThisWindow;

        int theBangActivations;
        int finishersExecuted;

        public HypeSystem(HypeConfig config) => this.config = config;

        public void SetConfig(HypeConfig value) => config = value;

        public int Hype => hype;
        public int MaxHype => config.MaxHype;
        public bool IsReady => hype >= config.MaxHype;
        public bool TheBangActive => theBangActive;
        public int TheBangActivations => theBangActivations;
        public int FinishersExecuted => finishersExecuted;

        public void Reset()
        {
            hype = 0;
            theBangActive = false;
            theBangEndSongTime = 0d;
            finisherTakenThisWindow = false;
            theBangActivations = 0;
            finishersExecuted = 0;
        }

        /// <summary>Expires THE BANG when its window has elapsed in authoritative song-time.</summary>
        public void Advance(double songTime)
        {
            if (theBangActive && songTime >= theBangEndSongTime)
            {
                theBangActive = false;
                finisherTakenThisWindow = false;
            }
        }

        /// <summary>
        /// Adds base HYPE (already un-amplified). Skips accrual when THE BANG is active and the
        /// config disables HYPE during the window. HYPE never receives THE BANG reward multipliers.
        /// Clamped to MaxHype. Returns the amount actually added (for the EventOutcome).
        /// </summary>
        public int AddHype(int baseAmount)
        {
            if (theBangActive && config.HypeMode == HypeDuringTheBang.Disabled) return 0;
            if (baseAmount <= 0) return 0;
            var before = hype;
            hype += baseAmount;
            if (hype > config.MaxHype) hype = config.MaxHype;
            return hype - before;
        }

        /// <summary>
        /// Manually activates THE BANG. Only succeeds when READY and not already active; otherwise
        /// returns false with NO side effects. On success consumes HYPE and opens the window.
        /// </summary>
        public bool TryActivateTheBang(double songTime)
        {
            if (theBangActive || !IsReady) return false;
            theBangActive = true;
            theBangEndSongTime = songTime + config.TheBangDuration;
            finisherTakenThisWindow = false;
            hype = 0;                 // HYPE is spent to enter THE BANG
            theBangActivations++;
            return true;
        }

        /// <summary>Snapshot the reward context for the event about to be resolved (anti-circularity).</summary>
        public RewardContext CaptureRewardContext()
        {
            return new RewardContext(
                theBangActive,
                config.TheBangScoreMultiplier,
                config.TheBangMotionRewardMultiplier,
                config.TheBangFinisherMultiplier,
                finisherAvailable: theBangActive && !finisherTakenThisWindow,
                config.HypeMode);
        }

        /// <summary>Records that the current window's Finisher has been consumed (once per window).</summary>
        public void MarkFinisherExecuted()
        {
            if (!theBangActive || finisherTakenThisWindow) return;
            finisherTakenThisWindow = true;
            finishersExecuted++;
        }
    }
}
