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

        [Header("Overshoot spring (render-only, gives the head visible mass)")]
        [Tooltip("Spring stiffness. Higher = snappier, less overshoot.")]
        [SerializeField, Min(1f)] float springStiffness = 140f;
        [Tooltip("Damping ratio. 1 = critically damped (no overshoot); <1 overshoots and settles.")]
        [SerializeField, Range(0.2f, 1.2f)] float springDamping = 0.6f;

        [Header("Squash & stretch (render-only)")]
        [Tooltip("How strongly speed stretches the head along its motion. 0 disables.")]
        [SerializeField, Range(0f, 1f)] float squashAmount = 0.35f;
        [Tooltip("Neck speed (deg/s) that maps to full stretch.")]
        [SerializeField, Min(1f)] float squashReferenceSpeed = 320f;
        [Tooltip("How fast the squash follows/relaxes.")]
        [SerializeField, Min(1f)] float squashResponse = 18f;

        Vector2 neutralHeadPosition;
        Vector3 baseHeadScale = Vector3.one;
        float renderHorizontal, renderVertical;   // spring positions
        float velHorizontal, velVertical;         // spring velocities
        float renderStretch;                      // smoothed 0..1 stretch factor

        public void SetSource(NeckMotionModel value) => neck = value;

        void Awake()
        {
            if (head != null)
            {
                neutralHeadPosition = head.localPosition;
                baseHeadScale = head.localScale;
            }
        }

        /// <summary>Snaps the visual head back to neutral (run start / retry). Presentation only.</summary>
        public void ResetPresentation()
        {
            renderHorizontal = renderVertical = 0f;
            velHorizontal = velVertical = 0f;
            renderStretch = 0f;
            if (head != null)
            {
                head.localRotation = Quaternion.identity;
                head.localPosition = neutralHeadPosition;
                head.localScale = baseHeadScale;
            }
        }

        void LateUpdate()
        {
            if (neck == null || head == null) return;

            var dt = Time.deltaTime;
            if (dt <= 0f) return;

            // --- Overshoot spring toward the authoritative angles (render-only). ---
            // A critically/under-damped spring gives the visible head mass: it slightly overshoots
            // the target and settles, instead of the old exponential Lerp that eased in flatly.
            renderHorizontal = Spring(renderHorizontal, ref velHorizontal, neck.HorizontalAngle, dt);
            renderVertical = Spring(renderVertical, ref velVertical, neck.VerticalAngle, dt);

            head.localRotation = Quaternion.Euler(renderVertical, 0f, renderHorizontal);
            head.localPosition = neutralHeadPosition + Vector2.up * (renderVertical * verticalVisualScale);

            // --- Squash & stretch driven by neck speed (render-only, volume ~conserved). ---
            var speed = Mathf.Abs(neck.HorizontalVelocity) + Mathf.Abs(neck.VerticalVelocity);
            var targetStretch = squashAmount * Mathf.Clamp01(speed / squashReferenceSpeed);
            var s = 1f - Mathf.Exp(-squashResponse * dt);
            renderStretch = Mathf.Lerp(renderStretch, targetStretch, s);

            // Stretch mostly along the horizontal swing (the dominant headbang axis); squash the
            // perpendicular so the head keeps roughly constant area.
            var stretchX = 1f + renderStretch;
            var squashY = 1f - renderStretch * 0.6f;
            head.localScale = new Vector3(
                baseHeadScale.x * stretchX,
                baseHeadScale.y * squashY,
                baseHeadScale.z);
        }

        /// <summary>
        /// One step of a damped spring toward <paramref name="target"/>. Returns the new position;
        /// updates <paramref name="velocity"/> by ref. springDamping &lt; 1 overshoots (mass feel),
        /// = 1 is critically damped. Render-only; never fed back into the domain.
        /// </summary>
        float Spring(float current, ref float velocity, float target, float dt)
        {
            // Sub-step so an explicit integrator stays stable at large/irregular frame deltas
            // (a frame hitch must not make the head fling). Cap total dt as a safety net.
            var remaining = Mathf.Min(dt, 0.1f);
            const float maxStep = 1f / 120f;
            var omega = Mathf.Sqrt(springStiffness);
            var damp = 2f * springDamping * omega;
            while (remaining > 0f)
            {
                var step = Mathf.Min(remaining, maxStep);
                var force = springStiffness * (target - current) - damp * velocity;
                velocity += force * step;
                current += velocity * step;
                remaining -= step;
            }
            return current;
        }
    }
}
