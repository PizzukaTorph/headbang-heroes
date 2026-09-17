using HeadbangHeroes.Charts;
using UnityEngine;

namespace HeadbangHeroes.Gameplay.Neck
{
    /// <summary>
    /// Authoritative, deterministic gameplay state for the neck. Pure C# — no MonoBehaviour,
    /// no scene transform, no render-frame timing. Owns:
    ///  - two spring-damper axes (horizontal/vertical),
    ///  - a fixed-step accumulator so simulation ticks depend only on authoritative elapsed time,
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

        double accumulator;   // unspent authoritative time (seconds)
        long tick;            // authoritative fixed-step counter
        bool prepared;        // false until the first bang has launched motion from neutral

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

        /// <summary>Replaces tuning without touching live physical state (e.g. inspector edits).</summary>
        public void SetConfig(NeckMotionConfig value) => config = value;

        /// <summary>Full reset to neutral/unprepared. The only operation allowed to snap state.</summary>
        public void Reset()
        {
            horizontal.Reset();
            vertical.Reset();
            accumulator = 0d;
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
        /// Runs a whole number of fixed steps; leftover time is retained in the accumulator so no
        /// simulation time is lost or double-applied across variable render frames.
        /// Returns the number of fixed steps executed.
        /// </summary>
        public int Advance(double authoritativeDelta)
        {
            if (authoritativeDelta <= 0d) return 0;
            accumulator += authoritativeDelta;

            var steps = 0;
            var maxStepsGuard = 100000; // safety against pathological deltas
            while (accumulator >= config.StepSeconds && steps < maxStepsGuard)
            {
                horizontal.Step(config);
                vertical.Step(config);
                accumulator -= config.StepSeconds;
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
