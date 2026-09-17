using HeadbangHeroes.Charts;
using HeadbangHeroes.Gameplay.Neck;
using UnityEngine;

namespace HeadbangHeroes.Gameplay
{
    /// <summary>
    /// Unity adapter for the canonical deterministic neck simulation.
    ///
    /// This MonoBehaviour owns a pure <see cref="NeckMotionState"/> domain object and:
    ///  - advances it with a fixed authoritative step (independent of render FPS),
    ///  - forwards semantic Classic bangs to the domain,
    ///  - drives the visual head transform DOWNSTREAM only (presentation reads domain state;
    ///    it never writes back into it).
    ///
    /// Gameplay-critical state lives entirely in the domain object. Motion Quality is evaluated
    /// separately by the pure MotionQualityEvaluator, which consumes the pre-inversion snapshot
    /// returned by <see cref="Bang"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NeckMotionModel : MonoBehaviour
    {
        [SerializeField] Transform head;
        [SerializeField] float verticalVisualScale = 0.45f;

        [Header("Authoritative simulation")]
        [Tooltip("Fixed simulation rate in Hz. Gameplay physics is stepped at this rate regardless of render FPS.")]
        [SerializeField, Min(30f)] float simulationHz = 120f;

        [Header("Physical tuning (deg, deg/s)")]
        [SerializeField] float impulse = 190f;
        [SerializeField] float damping = 5.5f;
        [SerializeField] float returnStrength = 9f;
        [SerializeField] float maxAngle = 42f;
        [SerializeField] float maxVelocity = 360f;
        [SerializeField] float limitBounce = -0.15f;

        [Header("Motion-quality references (provisional M0 glue)")]
        [SerializeField] float referenceVelocity = 220f;
        [SerializeField] float referenceTravel = 24f;

        NeckMotionState state;
        Vector2 neutralHeadPosition;

        // Presentation-only interpolation between the last two authoritative samples.
        float renderHorizontal, renderVertical;

        NeckMotionState State
        {
            get
            {
                if (state == null) state = new NeckMotionState(BuildConfig());
                return state;
            }
        }

        NeckMotionConfig BuildConfig() => new NeckMotionConfig(
            stepSeconds: 1d / Mathf.Max(30f, simulationHz),
            impulse: impulse,
            damping: damping,
            returnStrength: returnStrength,
            maxAngle: maxAngle,
            maxVelocity: maxVelocity,
            limitBounce: limitBounce,
            referenceVelocity: referenceVelocity,
            referenceTravel: referenceTravel);

        // --- Debug/telemetry accessors (read-only view of authoritative state) ---
        public float Angle => State.HorizontalDisplacement;
        public float Velocity => State.HorizontalVelocity;
        public float HorizontalAngle => State.HorizontalDisplacement;
        public float VerticalAngle => State.VerticalDisplacement;
        public float HorizontalVelocity => State.HorizontalVelocity;
        public float VerticalVelocity => State.VerticalVelocity;
        public bool Prepared => State.Prepared;

        void Awake()
        {
            if (head != null) neutralHeadPosition = head.localPosition;
            state = new NeckMotionState(BuildConfig());
        }

        void OnValidate()
        {
            if (state != null) state.SetConfig(BuildConfig());
        }

        /// <summary>Full reset to neutral/unprepared (run start / retry). Snaps only here.</summary>
        public void ResetMotion()
        {
            State.SetConfig(BuildConfig());
            State.Reset();
            renderHorizontal = renderVertical = 0f;
            if (head != null)
            {
                head.localRotation = Quaternion.identity;
                head.localPosition = neutralHeadPosition;
            }
        }

        /// <summary>
        /// Captures immutable pre-inversion evidence for this bang, then applies the impulse.
        /// Returns the snapshot describing the motion that arrived into the inversion.
        /// </summary>
        public NeckMotionSnapshot Bang(BangDirection inversionPoint, float intensity = 1f)
            => State.ApplyBang(inversionPoint, intensity);

        /// <summary>Captures pre-inversion evidence without applying an impulse.</summary>
        public NeckMotionSnapshot CapturePreInversionSnapshot(BangDirection inversionPoint)
            => State.CapturePreInversionSnapshot(inversionPoint);

        void Update()
        {
            // Advance authoritative simulation with a fixed step. Render delta only feeds the
            // authoritative elapsed clock; ticks are derived from total elapsed time, so the number
            // of physics ticks depends on elapsed time, not on how many render frames delivered it.
            // (Package 02 will replace Time.deltaTime with AudioClock-derived song-time delta.)
            State.Advance(Time.deltaTime);
        }

        void LateUpdate()
        {
            if (head == null) return;

            // Presentation-only smoothing toward authoritative state. Never writes back to domain.
            var t = 1f - Mathf.Exp(-25f * Time.deltaTime);
            renderHorizontal = Mathf.Lerp(renderHorizontal, State.HorizontalDisplacement, t);
            renderVertical = Mathf.Lerp(renderVertical, State.VerticalDisplacement, t);

            head.localRotation = Quaternion.Euler(renderVertical, 0f, renderHorizontal);
            head.localPosition = neutralHeadPosition + Vector2.up * (renderVertical * verticalVisualScale);
        }
    }
}
