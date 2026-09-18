namespace HeadbangHeroes.Charts.Runtime
{
    /// <summary>
    /// Immutable, prevalidated runtime motion event. Times are precomputed (seconds) so gameplay
    /// never re-derives them from beat math in the hot path. Direction reuses the existing
    /// <see cref="BangDirection"/> cardinal vocabulary; technique/trajectory/modifier use the
    /// canonical runtime enums. Value type + readonly so the chart cannot be mutated during a run.
    /// </summary>
    public readonly struct RuntimeMotionEvent
    {
        public readonly string Id;
        public readonly double Time;            // precomputed authoritative song-time (seconds)
        public readonly NeckTechnique Technique;
        public readonly NeckTrajectory Trajectory;
        public readonly BangDirection Direction;
        public readonly NeckModifier Modifier;
        public readonly double Duration;        // seconds; 0 for instantaneous
        public readonly float Intensity;        // 0..1 authored intensity
        public readonly bool FinisherCandidate;

        public RuntimeMotionEvent(
            string id,
            double time,
            NeckTechnique technique,
            NeckTrajectory trajectory,
            BangDirection direction,
            NeckModifier modifier,
            double duration,
            float intensity,
            bool finisherCandidate)
        {
            Id = id;
            Time = time;
            Technique = technique;
            Trajectory = trajectory;
            Direction = direction;
            Modifier = modifier;
            Duration = duration;
            Intensity = intensity;
            FinisherCandidate = finisherCandidate;
        }
    }

    /// <summary>
    /// Immutable authored Rest interval. Rest is its own event family (never a MotionEvent
    /// technique). Settling lets legitimate incoming momentum dissipate before stillness is
    /// measured. Thresholds live in <see cref="Gameplay.Rest.RestConfig"/>; per-event overrides
    /// may extend this later without changing the base meaning.
    /// </summary>
    public readonly struct RuntimeRestEvent
    {
        public readonly string Id;
        public readonly double StartTime;       // seconds
        public readonly double Duration;        // total rest interval (seconds)
        public readonly double SettlingDuration; // portion (seconds) allowed to bleed momentum

        public double EvaluationStart => StartTime + SettlingDuration;
        public double EndTime => StartTime + Duration;
        public double EvaluationDuration => System.Math.Max(0d, Duration - SettlingDuration);

        public RuntimeRestEvent(string id, double startTime, double duration, double settlingDuration)
        {
            Id = id;
            StartTime = startTime;
            Duration = duration;
            SettlingDuration = settlingDuration;
        }
    }
}
