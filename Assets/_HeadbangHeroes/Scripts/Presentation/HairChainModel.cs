namespace HeadbangHeroes.Presentation
{
    /// <summary>
    /// Pure, deterministic multi-segment hair chain (HAIR_SYSTEM_V1). PRESENTATION ONLY: each segment
    /// is a damped spring that lags the segment above it (segment 0 lags the head), so a neck swing
    /// produces the metal sequence: HEAD INVERTS -> HAIR CONTINUES (lag) -> SNAPS/FOLLOWS -> OVERSHOOT
    /// -> SETTLE. It reads neck angle/velocity + an inversion signal + a modest HYPE factor and emits
    /// per-segment world-ish angles for a presenter to render. It NEVER touches gameplay/scoring/timing.
    ///
    /// Determinism: fixed formula, sub-stepped for stability at large/irregular frame deltas.
    /// </summary>
    public sealed class HairChainModel
    {
        HairMotionTier tier;
        float[] angle;      // per-segment angle (deg), index 0 = nearest the head
        float[] velocity;   // per-segment angular velocity (deg/s)
        float prevHeadVel;  // for inversion detection

        public HairChainModel(in HairMotionTier tier) => SetTier(tier);

        public int SegmentCount => angle?.Length ?? 0;
        public float SegmentAngle(int i) => (angle != null && i >= 0 && i < angle.Length) ? angle[i] : 0f;

        public void SetTier(in HairMotionTier value)
        {
            tier = value;
            var n = tier.Segments;
            angle = new float[n];
            velocity = new float[n];
            prevHeadVel = 0f;
        }

        public void Reset()
        {
            if (angle != null) for (var i = 0; i < angle.Length; i++) { angle[i] = 0f; velocity[i] = 0f; }
            prevHeadVel = 0f;
        }

        /// <summary>
        /// Advance the chain toward the head. <paramref name="headAngle"/>/<paramref name="headVel"/>
        /// are the neck's horizontal angle (deg) and angular velocity (deg/s). <paramref name="hype01"/>
        /// is 0..1; it applies a MODEST (~10-20%) exaggeration to bend + overshoot only. Deterministic.
        /// </summary>
        public void Update(float headAngle, float headVel, float hype01, float dt)
        {
            if (angle == null || angle.Length == 0 || dt <= 0f) return;

            // Inversion detection: head angular velocity changed sign with meaningful magnitude.
            var inverted = (headVel * prevHeadVel < 0f) && (System.Math.Abs(prevHeadVel) > 20f);
            prevHeadVel = headVel;

            // HYPE modestly widens bend and lowers damping (more overshoot). Clamp exaggeration 1.0..1.2.
            var h = hype01 < 0f ? 0f : (hype01 > 1f ? 1f : hype01);
            var exaggeration = 1f + 0.2f * h;                 // up to +20%
            var maxBend = tier.MaxBend * exaggeration;
            var damping = tier.Damping / exaggeration;        // less damping -> more overshoot

            // Inversion kick: inject velocity into the top segment in the head's NEW direction, so the
            // hair first trails (already lagging) then whips through and overshoots.
            if (inverted)
            {
                var dir = headVel >= 0f ? 1f : -1f;
                velocity[0] += dir * tier.InversionKick * exaggeration;
            }

            // Sub-stepped explicit springs, chained: segment i is pulled toward (i-1)'s angle
            // (or the head for i==0), clamped to +/- maxBend from its parent.
            var remaining = dt < 0.1f ? dt : 0.1f;
            const float maxStep = 1f / 120f;
            while (remaining > 0f)
            {
                var step = remaining < maxStep ? remaining : maxStep;
                for (var i = 0; i < angle.Length; i++)
                {
                    var parent = i == 0 ? headAngle : angle[i - 1];
                    var accel = (parent - angle[i]) * tier.Stiffness;
                    velocity[i] += accel * step;
                    velocity[i] *= Exp(-damping * step);
                    angle[i] += velocity[i] * step;

                    var min = parent - maxBend;
                    var max = parent + maxBend;
                    if (angle[i] < min) { angle[i] = min; if (velocity[i] < 0f) velocity[i] = 0f; }
                    else if (angle[i] > max) { angle[i] = max; if (velocity[i] > 0f) velocity[i] = 0f; }
                }
                remaining -= step;
            }
        }

        static float Exp(float x) => (float)System.Math.Exp(x);
    }
}
