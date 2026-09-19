namespace HeadbangHeroes.Presentation
{
    /// <summary>
    /// Pure, deterministic screen-shake magnitude model. PRESENTATION ONLY. A bang injects trauma
    /// (0..1, scaled by intensity); trauma decays over time and the rendered offset is trauma^2 *
    /// maxOffset (quadratic so small trauma is subtle). It emits only a scalar magnitude; the
    /// presenter turns it into a capped positional jitter. Never touches gameplay.
    /// </summary>
    public sealed class ShakeModel
    {
        float trauma;
        readonly float decayPerSecond;
        readonly float maxOffset;

        public ShakeModel(float decayPerSecond = 3.5f, float maxOffset = 18f)
        {
            this.decayPerSecond = decayPerSecond < 0.1f ? 0.1f : decayPerSecond;
            this.maxOffset = maxOffset < 0f ? 0f : maxOffset;
        }

        public float Trauma => trauma;

        /// <summary>Add trauma from a bang (0..1). Capped at 1.</summary>
        public void AddTrauma(float amount)
        {
            var a = amount < 0f ? 0f : (amount > 1f ? 1f : amount);
            trauma += a;
            if (trauma > 1f) trauma = 1f;
        }

        public void Reset() => trauma = 0f;

        /// <summary>Advance decay; returns the current shake offset magnitude in px (trauma^2 * maxOffset).</summary>
        public float Advance(float dt)
        {
            if (dt > 0f)
            {
                trauma -= decayPerSecond * dt;
                if (trauma < 0f) trauma = 0f;
            }
            return trauma * trauma * maxOffset;
        }
    }
}
