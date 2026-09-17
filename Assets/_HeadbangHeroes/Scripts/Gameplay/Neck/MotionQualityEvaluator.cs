namespace HeadbangHeroes.Gameplay.Neck
{
    /// <summary>Data-driven Motion Quality tuning (deg/s, deg). Prototype defaults; all tuning.</summary>
    public readonly struct MotionQualityConfig
    {
        /// <summary>Incoming speed (deg/s) at/above which the velocity component is maximal.</summary>
        public readonly float ReferenceSpeed;

        /// <summary>Travel-since-inversion (deg) at/above which the amplitude component is maximal.</summary>
        public readonly float ReferenceTravel;

        /// <summary>Weight of travel/amplitude evidence (the rest goes to incoming speed).</summary>
        public readonly float TravelWeight;

        /// <summary>Quality granted to a setup/unprepared first bang (no preceding travel to reward).</summary>
        public readonly float SetupQuality;

        public MotionQualityConfig(float referenceSpeed, float referenceTravel, float travelWeight, float setupQuality)
        {
            ReferenceSpeed = referenceSpeed > 1f ? referenceSpeed : 1f;
            ReferenceTravel = referenceTravel > 1f ? referenceTravel : 1f;
            TravelWeight = travelWeight < 0f ? 0f : (travelWeight > 1f ? 1f : travelWeight);
            SetupQuality = setupQuality < 0f ? 0f : (setupQuality > 1f ? 1f : setupQuality);
        }

        public static MotionQualityConfig Default => new MotionQualityConfig(
            referenceSpeed: 220f,
            referenceTravel: 24f,
            travelWeight: 0.6f,
            setupQuality: 0.4f);
    }

    /// <summary>Deterministic Motion Quality outcome with inspectable components.</summary>
    public readonly struct MotionQualityResult
    {
        public readonly float Quality;        // 0..1
        public readonly float TravelComponent; // 0..1
        public readonly float SpeedComponent;  // 0..1
        public readonly bool WasSetup;         // true for the first-bang/unprepared case

        public MotionQualityResult(float quality, float travelComponent, float speedComponent, bool wasSetup)
        {
            Quality = quality;
            TravelComponent = travelComponent;
            SpeedComponent = speedComponent;
            WasSetup = wasSetup;
        }
    }

    /// <summary>
    /// Canonical Motion Quality evaluator. Pure C#: consumes the immutable pre-inversion
    /// <see cref="NeckMotionSnapshot"/> (Plan 01 event-local evidence) and produces a deterministic
    /// 0..1 quality with inspectable components. It never reads presentation state and never
    /// integrates motion — it only judges the physical arrival into an inversion.
    ///
    /// Timing and Motion Quality stay separate signals (SCORING_SYSTEM_V1): this class knows
    /// nothing about timing tiers. The first bang from neutral is a setup/unprepared action with
    /// no preceding travel; it receives a configured setup quality rather than a fabricated one.
    /// This replaces the M0 <c>NeckMotionModel.ProvisionalMotionQuality</c> glue.
    /// </summary>
    public static class MotionQualityEvaluator
    {
        public static MotionQualityResult Evaluate(in NeckMotionSnapshot snapshot, in MotionQualityConfig config)
        {
            if (!snapshot.Prepared)
                return new MotionQualityResult(config.SetupQuality, 0f, 0f, wasSetup: true);

            var speed = snapshot.IncomingSpeed;
            var speedComponent = Clamp01(speed / config.ReferenceSpeed);
            var travelComponent = Clamp01(snapshot.TravelSinceInversion / config.ReferenceTravel);

            var quality = config.TravelWeight * travelComponent + (1f - config.TravelWeight) * speedComponent;
            return new MotionQualityResult(Clamp01(quality), travelComponent, speedComponent, wasSetup: false);
        }

        static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
    }
}
