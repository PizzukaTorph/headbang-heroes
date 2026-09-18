namespace HeadbangHeroes.Presentation
{
    /// <summary>
    /// Pure, deterministic venue/background reaction intensity. PRESENTATION ONLY: it maps
    /// semantic performance signals (HYPE fraction, THE BANG active, a transient Finisher pulse)
    /// into a single 0..1 intensity the venue adapter uses for restrained colour/parallax/pulse.
    /// It never reads or writes gameplay state; it only receives already-authoritative signals.
    ///
    /// A <see cref="reducedMotion"/> flag caps the reactive range (accessibility) WITHOUT changing
    /// any of the inputs — presentation reductions never alter scoring.
    /// </summary>
    public struct VenueIntensityModel
    {
        public float Intensity;   // current smoothed 0..1

        float finisherPulse;      // transient 0..1, decays over time
        readonly float responsiveness;
        readonly float pulseDecayPerSecond;
        readonly bool reducedMotion;
        readonly float reducedMotionCap;

        public VenueIntensityModel(float responsiveness, float pulseDecayPerSecond, bool reducedMotion, float reducedMotionCap = 0.35f)
        {
            this.responsiveness = responsiveness < 0f ? 0f : responsiveness;
            this.pulseDecayPerSecond = pulseDecayPerSecond < 0f ? 0f : pulseDecayPerSecond;
            this.reducedMotion = reducedMotion;
            this.reducedMotionCap = reducedMotionCap < 0f ? 0f : (reducedMotionCap > 1f ? 1f : reducedMotionCap);
            Intensity = 0f;
            finisherPulse = 0f;
        }

        public static VenueIntensityModel CreateDefault(bool reducedMotion) =>
            new VenueIntensityModel(6f, 2f, reducedMotion);

        public void Reset()
        {
            Intensity = 0f;
            finisherPulse = 0f;
        }

        /// <summary>Registers a transient Finisher impact pulse (decays over the next moments).</summary>
        public void PulseFinisher() => finisherPulse = 1f;

        /// <summary>
        /// Advances toward a target built from HYPE (0..1), a THE BANG floor, and the decaying
        /// Finisher pulse. Reduced-motion caps the final intensity. Deterministic.
        /// </summary>
        public void Update(float hypeFraction, bool theBangActive, float dt)
        {
            var hype = Clamp01(hypeFraction);

            // Decay the transient pulse.
            if (finisherPulse > 0f)
            {
                finisherPulse -= pulseDecayPerSecond * (dt < 0f ? 0f : dt);
                if (finisherPulse < 0f) finisherPulse = 0f;
            }

            var target = hype * 0.6f;
            if (theBangActive) target = target > 0.7f ? target : 0.7f; // THE BANG raises the floor
            target += finisherPulse * 0.4f;
            target = Clamp01(target);
            if (reducedMotion && target > reducedMotionCap) target = reducedMotionCap;

            var t = Smoothing(dt);
            Intensity += (target - Intensity) * t;
            Intensity = Clamp01(Intensity);
        }

        float Smoothing(float dt)
        {
            if (dt <= 0f) return 0f;
            var f = 1f - Exp(-responsiveness * dt);
            return f < 0f ? 0f : (f > 1f ? 1f : f);
        }

        static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
        static float Exp(float x) => (float)System.Math.Exp(x);
    }
}
