using System;
using HeadbangHeroes.Charts.Runtime;

namespace HeadbangHeroes.Gameplay.Rest
{
    /// <summary>Deterministic result of an authored Rest interval. Not scored here (Plan 04 owns scoring).</summary>
    public readonly struct RestOutcome
    {
        public readonly string RestId;
        public readonly bool Passed;
        public readonly float PeakSpeed;          // deg/s observed during the evaluation phase
        public readonly float PeakDisplacement;   // |deg| observed during the evaluation phase
        public readonly int EvaluationSamples;    // authoritative samples counted in evaluation

        public RestOutcome(string restId, bool passed, float peakSpeed, float peakDisplacement, int samples)
        {
            RestId = restId;
            Passed = passed;
            PeakSpeed = peakSpeed;
            PeakDisplacement = peakDisplacement;
            EvaluationSamples = samples;
        }
    }

    /// <summary>
    /// Evaluates the canonical Authored Rest lifecycle:
    ///
    ///   Rest begins → settling phase (momentum may dissipate) → stillness evaluation → RestOutcome
    ///
    /// It reads authoritative neck evidence (speed/displacement) and NEVER mutates or snaps the
    /// neck. Only samples taken during the evaluation phase (after settling) count, so a player is
    /// never punished for legitimate incoming momentum created by the preceding required action.
    ///
    /// Determinism / FPS-independence: the outcome depends only on the peak speed/displacement of
    /// the authoritative neck history sampled within the evaluation window. Aggregating by max is
    /// insensitive to how many samples were delivered GIVEN EQUIVALENT SAMPLED HISTORY. To keep this
    /// true in practice, the consumer must feed samples from the authoritative fixed simulation
    /// tick (not a render frame), so a low render rate cannot skip a transient neck spike.
    /// Pure C#: no Unity refs, no scene, testable in isolation.
    /// </summary>
    public sealed class RestEvaluator
    {
        RuntimeRestEvent rest;
        RestConfig config;
        bool active;

        float peakSpeed;
        float peakDisplacement;
        int evaluationSamples;

        public bool IsActive => active;

        public void Begin(in RuntimeRestEvent restEvent, in RestConfig restConfig)
        {
            rest = restEvent;
            config = restConfig;
            active = true;
            peakSpeed = 0f;
            peakDisplacement = 0f;
            evaluationSamples = 0;
        }

        /// <summary>
        /// Feeds one authoritative neck sample at <paramref name="songTime"/>. Samples before the
        /// evaluation phase (during settling) or outside the interval are ignored — they never
        /// affect the outcome. Does not touch the neck.
        /// </summary>
        public void Sample(double songTime, float neckSpeed, float neckDisplacement)
        {
            if (!active) return;
            if (songTime < rest.EvaluationStart || songTime > rest.EndTime) return;

            var speed = neckSpeed < 0f ? -neckSpeed : neckSpeed;
            var disp = neckDisplacement < 0f ? -neckDisplacement : neckDisplacement;
            if (speed > peakSpeed) peakSpeed = speed;
            if (disp > peakDisplacement) peakDisplacement = disp;
            evaluationSamples++;
        }

        /// <summary>
        /// Finalizes the outcome. Stillness passes when the evaluation-phase peaks stay within
        /// the configured thresholds. If no evaluation samples were taken (e.g. a degenerate
        /// zero-length evaluation window), stillness is considered satisfied by default.
        /// </summary>
        public RestOutcome Complete()
        {
            active = false;
            var passed = evaluationSamples == 0 ||
                         (peakSpeed <= config.MaxEvaluationSpeed &&
                          peakDisplacement <= config.MaxEvaluationDisplacement);
            return new RestOutcome(rest.Id, passed, peakSpeed, peakDisplacement, evaluationSamples);
        }
    }
}
