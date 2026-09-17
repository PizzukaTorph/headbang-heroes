namespace HeadbangHeroes.Presentation
{
    /// <summary>
    /// Presentation-only accessibility flags. These reduce visual/haptic intensity; they must
    /// NEVER change authoritative scoring, timing windows, neck physics or chart resolution.
    /// A plain value type shared by presenters (no Unity dependency).
    /// </summary>
    public struct AccessibilitySettings
    {
        public bool ReducedFlash;   // damp bright flashes / colour swings
        public bool ReducedShake;   // suppress screen/camera shake
        public bool HapticsEnabled; // master haptics toggle
        public float HapticIntensity; // 0..1 scale

        public static AccessibilitySettings Default => new AccessibilitySettings
        {
            ReducedFlash = false,
            ReducedShake = false,
            HapticsEnabled = true,
            HapticIntensity = 1f
        };
    }
}
