namespace HeadbangHeroes.Gameplay.Neck
{
    /// <summary>
    /// Immutable tuning for the deterministic neck simulation. Pure gameplay-domain data:
    /// no Unity object references, no static mutable state. A fresh config is passed into the
    /// model so tuning is explicit and testable rather than hidden in scattered constants.
    ///
    /// Angles are in degrees, velocities in degrees/second, times in seconds.
    /// </summary>
    public readonly struct NeckMotionConfig
    {
        /// <summary>Authoritative fixed simulation step (seconds). ~120 Hz is a tuning candidate.</summary>
        public readonly double StepSeconds;

        /// <summary>Launch impulse applied to velocity on a full-intensity bang (deg/s).</summary>
        public readonly float Impulse;

        /// <summary>Exponential velocity damping coefficient (per second).</summary>
        public readonly float Damping;

        /// <summary>Spring strength pulling the head back toward neutral (per second^2 scale).</summary>
        public readonly float ReturnStrength;

        /// <summary>Absolute travel limit on each axis (deg).</summary>
        public readonly float MaxAngle;

        /// <summary>Absolute velocity clamp (deg/s).</summary>
        public readonly float MaxVelocity;

        /// <summary>Velocity retained (and reflected) when hitting the physical limit.</summary>
        public readonly float LimitBounce;

        /// <summary>Reference incoming velocity at/above which the velocity evidence is maximal (deg/s).</summary>
        public readonly float ReferenceVelocity;

        /// <summary>Reference travel-since-inversion at/above which amplitude evidence is maximal (deg).</summary>
        public readonly float ReferenceTravel;

        public NeckMotionConfig(
            double stepSeconds,
            float impulse,
            float damping,
            float returnStrength,
            float maxAngle,
            float maxVelocity,
            float limitBounce,
            float referenceVelocity,
            float referenceTravel)
        {
            StepSeconds = stepSeconds > 0d ? stepSeconds : 1d / 120d;
            Impulse = impulse;
            Damping = damping;
            ReturnStrength = returnStrength;
            MaxAngle = maxAngle > 1f ? maxAngle : 1f;
            MaxVelocity = maxVelocity > 1f ? maxVelocity : 1f;
            LimitBounce = limitBounce;
            ReferenceVelocity = referenceVelocity > 1f ? referenceVelocity : 1f;
            ReferenceTravel = referenceTravel > 1f ? referenceTravel : 1f;
        }

        /// <summary>Canonical default tuning (120 Hz authoritative step).</summary>
        public static NeckMotionConfig Default => new NeckMotionConfig(
            stepSeconds: 1d / 120d,
            impulse: 190f,
            damping: 5.5f,
            returnStrength: 9f,
            maxAngle: 42f,
            maxVelocity: 360f,
            limitBounce: -0.15f,
            referenceVelocity: 220f,
            referenceTravel: 24f);
    }
}
