namespace HeadbangHeroes.Gameplay.Rest
{
    /// <summary>
    /// Data-driven Authored Rest tuning. Values describe how still the neck must be during the
    /// evaluation phase (settling excluded). All thresholds are tuning data; the fairness rule is
    /// fixed (settling first, then evaluation), the numbers are not.
    ///
    /// Speed is in deg/s and displacement in deg, matching the neck domain units.
    /// </summary>
    public readonly struct RestConfig
    {
        /// <summary>Peak neck speed (deg/s) tolerated during evaluation before stillness fails.</summary>
        public readonly float MaxEvaluationSpeed;

        /// <summary>Peak absolute displacement (deg) tolerated during evaluation before stillness fails.</summary>
        public readonly float MaxEvaluationDisplacement;

        public RestConfig(float maxEvaluationSpeed, float maxEvaluationDisplacement)
        {
            MaxEvaluationSpeed = maxEvaluationSpeed;
            MaxEvaluationDisplacement = maxEvaluationDisplacement;
        }

        /// <summary>Reasonable M0 defaults; tune on device.</summary>
        public static RestConfig Default => new RestConfig(
            maxEvaluationSpeed: 35f,
            maxEvaluationDisplacement: 8f);
    }
}
