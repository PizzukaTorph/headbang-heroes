using HeadbangHeroes.Gameplay;
using UnityEngine;

namespace HeadbangHeroes.Presentation
{
    /// <summary>
    /// Drives the visible head transform from the authoritative <see cref="NeckMotionModel"/>.
    /// PRESENTATION ONLY: it READS the neck's public accessors and applies a smoothed rotation/
    /// offset in LateUpdate. It has no way to write neck state, so removing this component changes
    /// nothing about physics/scoring — the head simply stops moving visually.
    /// Interpolation here is a render nicety and is never fed back into the domain.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NeckPresenter : MonoBehaviour
    {
        [SerializeField] NeckMotionModel neck;   // read-only source
        [SerializeField] Transform head;
        [SerializeField] float verticalVisualScale = 0.45f;
        [SerializeField, Min(0f)] float smoothing = 25f;

        Vector2 neutralHeadPosition;
        float renderHorizontal, renderVertical;

        public void SetSource(NeckMotionModel value) => neck = value;

        void Awake()
        {
            if (head != null) neutralHeadPosition = head.localPosition;
        }

        /// <summary>Snaps the visual head back to neutral (run start / retry). Presentation only.</summary>
        public void ResetPresentation()
        {
            renderHorizontal = renderVertical = 0f;
            if (head != null)
            {
                head.localRotation = Quaternion.identity;
                head.localPosition = neutralHeadPosition;
            }
        }

        void LateUpdate()
        {
            if (neck == null || head == null) return;

            var t = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
            renderHorizontal = Mathf.Lerp(renderHorizontal, neck.HorizontalAngle, t);
            renderVertical = Mathf.Lerp(renderVertical, neck.VerticalAngle, t);

            head.localRotation = Quaternion.Euler(renderVertical, 0f, renderHorizontal);
            head.localPosition = neutralHeadPosition + Vector2.up * (renderVertical * verticalVisualScale);
        }
    }
}
