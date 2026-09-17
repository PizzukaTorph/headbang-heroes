namespace HeadbangHeroes.Presentation
{
    /// <summary>
    /// Pure, deterministic secondary-motion model for a hair strand. PRESENTATION ONLY: a damped
    /// spring lags behind the head angle so the hair whips with inertia. It reads the head angle
    /// and produces a strand angle for the hair adapter to apply. It never touches gameplay state.
    ///
    /// The strand under- and over-shoots the head with lag/overshoot governed by stiffness/damping,
    /// which is exactly what makes headbanging hair read as energetic without any authoritative role.
    /// </summary>
    public struct HairStrandModel
    {
        public float Angle;     // current strand angle (deg)
        public float Velocity;  // deg/s

        readonly float stiffness;  // spring pull toward the head angle
        readonly float damping;    // velocity damping
        readonly float maxLag;     // clamp how far the strand can trail the head (deg)

        public HairStrandModel(float stiffness, float damping, float maxLag)
        {
            this.stiffness = stiffness < 0f ? 0f : stiffness;
            this.damping = damping < 0f ? 0f : damping;
            this.maxLag = maxLag < 1f ? 1f : maxLag;
            Angle = 0f;
            Velocity = 0f;
        }

        public static HairStrandModel CreateDefault() => new HairStrandModel(90f, 9f, 60f);

        public void Reset()
        {
            Angle = 0f;
            Velocity = 0f;
        }

        /// <summary>
        /// Advances the damped spring toward <paramref name="headAngle"/> by <paramref name="dt"/>.
        /// Deterministic. The strand is clamped to stay within maxLag of the head so extreme swings
        /// cannot fling the hair off-screen.
        /// </summary>
        public void Update(float headAngle, float dt)
        {
            if (dt <= 0f) return;

            var accel = (headAngle - Angle) * stiffness;
            Velocity += accel * dt;
            Velocity *= Exp(-damping * dt);
            Angle += Velocity * dt;

            var min = headAngle - maxLag;
            var max = headAngle + maxLag;
            if (Angle < min) { Angle = min; if (Velocity < 0f) Velocity = 0f; }
            else if (Angle > max) { Angle = max; if (Velocity > 0f) Velocity = 0f; }
        }

        static float Exp(float x) => (float)System.Math.Exp(x);
    }
}
