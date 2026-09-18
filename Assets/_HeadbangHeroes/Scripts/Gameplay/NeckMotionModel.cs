using HeadbangHeroes.Charts;
using HeadbangHeroes.Gameplay.Neck;
using UnityEngine;

namespace HeadbangHeroes.Gameplay
{
    /// <summary>
    /// Unity adapter for the canonical deterministic neck simulation. It owns a pure
    /// <see cref="NeckMotionState"/> domain object, advances it with a fixed authoritative step
    /// (independent of render FPS) and forwards semantic Classic bangs to the domain.
    ///
    /// This component holds NO presentation: the visible head transform is driven downstream by a
    /// separate <c>NeckPresenter</c> that only READS the accessors below. Motion Quality is
    /// evaluated separately by the pure MotionQualityEvaluator from the snapshot returned by
    /// <see cref="Bang"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NeckMotionModel : MonoBehaviour
    {
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

        NeckMotionState state;

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
            referenceVelocity: 220f,   // retained for config completeness; MQ uses its own config
            referenceTravel: 24f);

        // --- Read-only view of authoritative state (consumed downstream by NeckPresenter/HUD) ---
        public float Angle => State.HorizontalDisplacement;
        public float Velocity => State.HorizontalVelocity;
        public float HorizontalAngle => State.HorizontalDisplacement;
        public float VerticalAngle => State.VerticalDisplacement;
        public float HorizontalVelocity => State.HorizontalVelocity;
        public float VerticalVelocity => State.VerticalVelocity;
        public bool Prepared => State.Prepared;

        void Awake() => state = new NeckMotionState(BuildConfig());

        void OnValidate()
        {
            if (state != null) state.SetConfig(BuildConfig());
        }

        /// <summary>Full reset to neutral/unprepared (run start / retry). Snaps only here.</summary>
        public void ResetMotion()
        {
            State.SetConfig(BuildConfig());
            State.Reset();
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
            // (A later package will replace Time.deltaTime with AudioClock-derived song-time delta.)
            State.Advance(Time.deltaTime);
        }
    }
}
