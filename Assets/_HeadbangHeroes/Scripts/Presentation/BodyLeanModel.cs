namespace HeadbangHeroes.Presentation
{
    /// <summary>
    /// Pure, deterministic torso/shoulder lean derived from neck displacement. PRESENTATION ONLY:
    /// it takes read-only neck angles and produces a smoothed lean the body adapter applies to a
    /// transform. It has no reference to the neck domain and cannot feed anything back into it.
    ///
    /// The body under-follows the head (a fraction of the neck angle) and eases toward the target,
    /// so the torso reads as reacting to the head rather than being rigidly attached.
    /// </summary>
    public struct BodyLeanModel
    {
        public float LeanZ;   // current smoothed roll (deg), follows horizontal neck angle
        public float LeanX;   // current smoothed pitch (deg), follows vertical neck angle

        readonly float followFraction; // how much of the neck angle the body adopts (0..1)
        readonly float responsiveness;  // easing rate (per second)

        public BodyLeanModel(float followFraction, float responsiveness)
        {
            this.followFraction = followFraction < 0f ? 0f : (followFraction > 1f ? 1f : followFraction);
            this.responsiveness = responsiveness < 0f ? 0f : responsiveness;
            LeanZ = 0f;
            LeanX = 0f;
        }

        public static BodyLeanModel CreateDefault() => new BodyLeanModel(0.35f, 12f);

        public void Reset()
        {
            LeanZ = 0f;
            LeanX = 0f;
        }

        /// <summary>
        /// Eases the lean toward followFraction * neck angle over <paramref name="dt"/> seconds.
        /// Deterministic given the same inputs; uses an exponential smoothing factor.
        /// </summary>
        public void Update(float horizontalNeckAngle, float verticalNeckAngle, float dt)
        {
            var targetZ = horizontalNeckAngle * followFraction;
            var targetX = verticalNeckAngle * followFraction;
            var t = Smoothing(dt);
            LeanZ += (targetZ - LeanZ) * t;
            LeanX += (targetX - LeanX) * t;
        }

        float Smoothing(float dt)
        {
            if (dt <= 0f) return 0f;
            // 1 - e^(-k*dt): framerate-independent easing, clamped to [0,1].
            var f = 1f - Exp(-responsiveness * dt);
            return f < 0f ? 0f : (f > 1f ? 1f : f);
        }

        // Local exp to keep the type engine-free (unit-testable without UnityEngine).
        static float Exp(float x) => (float)System.Math.Exp(x);
    }
}
