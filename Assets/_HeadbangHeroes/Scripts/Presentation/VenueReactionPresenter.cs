using UnityEngine;
using UnityEngine.UI;

namespace HeadbangHeroes.Presentation
{
    /// <summary>
    /// Lightweight reactive venue/background. PRESENTATION ONLY: the controller PUSHES already-
    /// authoritative performance signals (HYPE fraction, THE BANG active, Finisher pulse); this
    /// presenter maps them via the pure <see cref="VenueIntensityModel"/> to a restrained colour
    /// swing on a background image. It never reads or writes gameplay state and stays subordinate
    /// to head/cue readability (small colour delta, no coverage of the play region).
    /// Respects reduced-motion (accessibility) which caps intensity without touching gameplay.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VenueReactionPresenter : MonoBehaviour
    {
        [SerializeField] Image background;
        [SerializeField] Color baseColor = new Color(0.055f, 0.055f, 0.07f, 1f);
        [SerializeField] Color intenseColor = new Color(0.16f, 0.06f, 0.09f, 1f);

        VenueIntensityModel model = VenueIntensityModel.CreateDefault(false);
        float hypeFraction;
        bool theBangActive;

        public void ApplySettings(in AccessibilitySettings settings)
            => model = VenueIntensityModel.CreateDefault(settings.ReducedFlash);

        public void SetPerformanceSignals(float hype01, bool bangActive)
        {
            hypeFraction = hype01;
            theBangActive = bangActive;
        }

        public void PulseFinisher() => model.PulseFinisher();

        public void ResetPresentation()
        {
            model.Reset();
            hypeFraction = 0f;
            theBangActive = false;
            if (background != null) background.color = baseColor;
        }

        void Update()
        {
            if (background == null) return;
            model.Update(hypeFraction, theBangActive, Time.deltaTime);
            background.color = Color.Lerp(baseColor, intenseColor, model.Intensity);
        }
    }
}
