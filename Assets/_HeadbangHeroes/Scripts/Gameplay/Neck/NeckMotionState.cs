using HeadbangHeroes.Charts;
using UnityEngine;

namespace HeadbangHeroes.Gameplay.Neck
{
    /// <summary>
    /// Authoritative, deterministic gameplay state for the neck. Pure C# — no MonoBehaviour,
    /// no scene transform, no render-frame timing. Owns:
    ///  - two spring-damper axes (horizontal/vertical),
    ///  - a fixed-step clock so simulation ticks depend only on authoritative elapsed time,
    ///  - prepared/setup (first-bang) semantics,
    ///  - pre-inversion snapshot capture,
    ///  - event/gesture-local motion evidence (no run-global maxima).
    ///
    /// Contract highlights (see FOUNDATION.md / TECHNICAL_CONTRACTS_V1.md):
    ///  - Every valid semantic bang changes motion, even early/late/wrong/no-event.
    ///  - A MISS never resets or snaps this state; only <see cref="Reset"/> does.
    ///  - Inversion is accepted anywhere in travel (no pose gating).
    /// </summary>
    public sealed class NeckMotionState
    {
        NeckMotionConfig config;
        NeckAxisState horizontal;
        NeckAxisState vertical;

        double elapsed;      // total authoritative time consumed since reset (seconds)
        long tick;           // authoritative fixed-step counter (== floor(elapsed / step))
        bool prepared;       // false until the first bang has launched motion from neutral

        /// <summary>Max authoritative time absorbed in a single Advance call (anti spiral-of-death).</summary>
        public const double MaxAdvanceSeconds = 0.25;

        public NeckMotionState(NeckMotionConfig config)
        {
            this.config = config;
            horizontal = NeckAxisState.Neutral;
            vertical = NeckAxisState.Neutral;
        }

        public NeckMotionConfig Config => config;
        public long Tick => tick;
        public bool Prepared => prepared;

        public float HorizontalDisplacement => horizontal.Displacement;
        public float VerticalDisplacement => vertical.Displacement;
        public float HorizontalVelocity => horizontal.Velocity;
        public float VerticalVelocity => vertical.Velocity;

        /// <summary>
        /// Replaces tuning without touching live physical state. If the fixed step changes, the
        /// elapsed clock is rescaled so the current tick index stays consistent with the new step
        /// (prevents a burst of catch-up ticks on a mid-run step change).
        /// </summary>
        public void SetConfig(NeckMotionConfig value)
        {
            config = value;
            elapsed = tick * config.StepSeconds; // keep floor(elapsed/step) == current tick
        }

        /// <summary>Full reset to neutral/unprepared. The only operation allowed to snap state.</summary>
        public void Reset()
        {
            horizontal.Reset();
            vertical.Reset();
            elapsed = 0d;
            tick = 0;
            prepared = false;
        }

        ref NeckAxisState AxisRef(BangAxis axis)
        {
            if (axis == BangAxis.Horizontal) return ref horizontal;
            return ref vertical;
        }

        NeckAxisState GetAxis(BangAxis axis) => axis == BangAxis.Horizontal ? horizontal : vertical;

        /// <summary>
        /// Advances the deterministic simulation by <paramref name="authoritativeDelta"/> seconds.
        ///
        /// Ticks are derived from the TOTAL accumulated authoritative time
        /// (<c>targetTick = floor(elapsed / step)</c>), not from a running float accumulator, so the
        /// number of executed steps depends only on total elapsed time and never on how that time
        /// was chunked across render frames. This eliminates float-order drift between frame rates.
        ///
        /// A single call absorbs at most <see cref="MaxAdvanceSeconds"/> to avoid a frame-hitch
        /// spiral of death; excess time is discarded rather than carried as permanent debt.
        /// Returns the number of fixed steps executed this call.
        /// </summary>
        public int Advance(double authoritativeDelta)
        {
            if (authoritativeDelta <= 0d) return 0;
            if (authoritativeDelta > MaxAdvanceSeconds) authoritativeDelta = MaxAdvanceSeconds;

            elapsed += authoritativeDelta;

            // Small epsilon so accumulated floating-point drift just under an exact tick boundary
            // (e.g. summing 1/90 s chunks toward a whole number of 1/120 s steps) still ticks.
            var targetTick = (long)System.Math.Floor(elapsed / config.StepSeconds + 1e-9);
            var steps = 0;
            while (tick < targetTick)
            {
                horizontal.Step(config);
                vertical.Step(config);
                tick++;
                steps++;
            }
            return steps;
        }

        /// <summary>
        /// Captures immutable pre-inversion evidence for the axis addressed by
        /// <paramref name="inversionPoint"/>, before any impulse from this bang is applied.
        /// </summary>
        public NeckMotionSnapshot CapturePreInversionSnapshot(BangDirection inversionPoint)
        {
            var axis = inversionPoint.Axis();
            var a = GetAxis(axis);
            return new NeckMotionSnapshot(
                axis,
                a.Displacement,
                a.Velocity,
                a.TravelSinceInversion,
                a.PeakSpeedSinceInversion,
                prepared,
                tick);
        }

        /// <summary>
        /// Applies a Classic bang. The named direction is the inversion point; motion launches
        /// toward the opposite side. Accepted anywhere in travel. This begins a new event-local
        /// evidence boundary and marks the neck prepared after the first launch.
        /// Returns the snapshot of the state as it arrived, immediately before the impulse.
        /// </summary>
        public NeckMotionSnapshot ApplyBang(BangDirection inversionPoint, float intensity = 1f)
        {
            var snapshot = CapturePreInversionSnapshot(inversionPoint);

            var axis = inversionPoint.Axis();
            var launch = inversionPoint.LaunchVector();
            var sign = axis == BangAxis.Horizontal ? launch.x : launch.y;

            ref var a = ref AxisRef(axis);
            a.BeginInversionBoundary();
            a.ApplyImpulse(sign, intensity, config);

            prepared = true; // subsequent bangs now have preceding travel to judge
            return snapshot;
        }
    }
}
