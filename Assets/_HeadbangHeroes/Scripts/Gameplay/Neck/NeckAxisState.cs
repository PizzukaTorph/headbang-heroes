using UnityEngine;

namespace HeadbangHeroes.Gameplay.Neck
{
    /// <summary>
    /// Pure deterministic spring-damper integrator for a single neck axis, plus bounded
    /// event/gesture-local motion evidence.
    ///
    /// This type contains NO run-global maxima. Motion Quality evidence (travel and incoming
    /// peak velocity) is accumulated only since the most recent inversion boundary and is
    /// reset when a new gesture begins. This replaces the M0 run-global <c>peakAmplitude</c>.
    ///
    /// The struct is deliberately UnityEngine-free except for the math helpers, so it can be
    /// unit-tested without a scene or render loop.
    /// </summary>
    public struct NeckAxisState
    {
        public float Displacement;   // current angle (deg)
        public float Velocity;       // deg/s

        // --- event/gesture-local evidence (reset at each inversion boundary) ---
        public float TravelSinceInversion;    // accumulated |Δdisplacement| since boundary (deg)
        public float PeakSpeedSinceInversion;  // max |velocity| observed since boundary (deg/s)

        public static NeckAxisState Neutral => default;

        public void Reset()
        {
            Displacement = 0f;
            Velocity = 0f;
            TravelSinceInversion = 0f;
            PeakSpeedSinceInversion = 0f;
        }

        /// <summary>
        /// Marks a new gesture/inversion boundary: local evidence restarts from here so the
        /// next event is judged only on motion produced after this point. Peak resets to zero
        /// (the arrival peak has already been captured in the pre-inversion snapshot), so a
        /// spam launch impulse cannot inflate a later event's incoming-motion evidence.
        /// </summary>
        public void BeginInversionBoundary()
        {
            TravelSinceInversion = 0f;
            PeakSpeedSinceInversion = 0f;
        }

        /// <summary>Applies a launch impulse along <paramref name="sign"/> (-1 / +1), scaled by intensity.</summary>
        public void ApplyImpulse(float sign, float intensity, in NeckMotionConfig config)
        {
            var s = sign >= 0f ? 1f : -1f;
            Velocity += s * config.Impulse * Mathf.Clamp01(intensity);
            Velocity = Mathf.Clamp(Velocity, -config.MaxVelocity, config.MaxVelocity);
            if (Mathf.Abs(Velocity) > PeakSpeedSinceInversion)
                PeakSpeedSinceInversion = Mathf.Abs(Velocity);
        }

        /// <summary>
        /// Advances the integrator by exactly one fixed step. Deterministic: identical inputs and
        /// step size always produce identical state, independent of render frame pacing.
        /// </summary>
        public void Step(in NeckMotionConfig config)
        {
            var dt = (float)config.StepSeconds;
            var previous = Displacement;

            // Spring pulls toward neutral; exponential damping bleeds off velocity.
            Velocity += -Displacement * config.ReturnStrength * dt;
            Velocity *= Mathf.Exp(-config.Damping * dt);
            Displacement += Velocity * dt;

            if (Mathf.Abs(Displacement) > config.MaxAngle)
            {
                Displacement = Mathf.Clamp(Displacement, -config.MaxAngle, config.MaxAngle);
                Velocity *= config.LimitBounce;
            }

            TravelSinceInversion += Mathf.Abs(Displacement - previous);
            var speed = Mathf.Abs(Velocity);
            if (speed > PeakSpeedSinceInversion) PeakSpeedSinceInversion = speed;
        }
    }
}
