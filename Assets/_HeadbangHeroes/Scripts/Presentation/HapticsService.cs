using UnityEngine;

namespace HeadbangHeroes.Presentation
{
    /// <summary>Semantic gameplay moments a presenter may translate into haptics.</summary>
    public enum HapticEvent
    {
        Perfect,
        Great,
        Miss,
        HypeReady,
        TheBangActivated,
        Finisher
    }

    /// <summary>
    /// Centralized, optional, semantic haptic feedback. Presenters call <see cref="Play"/> with a
    /// semantic <see cref="HapticEvent"/>; the service maps it to a platform vibration behind a
    /// single seam that honours an on/off toggle and an intensity scale (accessibility/settings).
    ///
    /// This is supplemental feedback only: successful play never requires it, and it NEVER layers
    /// an arcade sound over the song. On desktop/editor it is a safe no-op with a debug trace, so
    /// the seam is fully exercised without a device. Platform vibration wiring is intentionally
    /// deferred to device validation.
    /// </summary>
    public sealed class HapticsService : MonoBehaviour
    {
        [SerializeField] bool enabledHaptics = true;
        [SerializeField, Range(0f, 1f)] float intensity = 1f;
        [SerializeField] bool logInEditor = false;

        public void ApplySettings(in AccessibilitySettings settings)
        {
            enabledHaptics = settings.HapticsEnabled;
            intensity = settings.HapticIntensity;
        }

        public void SetEnabled(bool value) => enabledHaptics = value;
        public void SetIntensity(float value) => intensity = Mathf.Clamp01(value);

        /// <summary>
        /// Plays a semantic haptic. No-ops when disabled or at zero intensity. This is the ONLY
        /// entry point, so haptics can be globally disabled in one place. Never affects gameplay.
        /// </summary>
        public void Play(HapticEvent semanticEvent)
        {
            if (!enabledHaptics || intensity <= 0f) return;

            // Platform vibration goes here (Handheld.Vibrate / per-platform amplitude API) during
            // device validation. Kept a no-op seam for editor/desktop so nothing is fabricated.
#if UNITY_EDITOR
            if (logInEditor)
                Debug.Log($"[Haptics] {semanticEvent} @ {intensity:0.00}");
#endif
        }
    }
}
