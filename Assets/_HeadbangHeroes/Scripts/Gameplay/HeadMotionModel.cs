using HeadbangHeroes.Charts;
using UnityEngine;

namespace HeadbangHeroes.Gameplay
{
    /// <summary>
    /// Deterministic, tunable head/neck motion model for M0.
    /// No Rigidbody2D: a simple spring-damper integrator drives a single angle.
    /// Exposes enough data (angle, angular velocity, normalized motion quality,
    /// inversion detection, peak amplitude, direction coherence) to start
    /// measuring whether the player is really "headbanging" and not just tapping.
    ///
    /// The numeric integration and the quality heuristics live in the pure
    /// <see cref="HeadMotionState"/> struct so they can be unit-tested without a MonoBehaviour.
    /// </summary>
    public sealed class HeadMotionModel : MonoBehaviour
    {
        [SerializeField] Transform head;
        [SerializeField] float verticalVisualScale = 0.45f;

        [Header("Motion (deterministic spring-damper)")]
        [SerializeField] float impulse = 190f;
        [SerializeField] float damping = 5.5f;
        [SerializeField] float returnStrength = 9f;
        [SerializeField] float maxAngle = 42f;
        [SerializeField] float maxVelocity = 360f;

        [Header("Motion quality tuning")]
        [Tooltip("Angular velocity (deg/s) at or above which velocity contribution to motion quality is maximal.")]
        [SerializeField] float qualityReferenceVelocity = 220f;
        [Tooltip("Peak amplitude (deg) at or above which amplitude contribution to motion quality is maximal.")]
        [SerializeField] float qualityReferenceAmplitude = 24f;

        HeadMotionState horizontalState = HeadMotionState.CreateDefault();
        HeadMotionState verticalState = HeadMotionState.CreateDefault();
        Vector2 neutralHeadPosition;

        // Legacy/debug aliases remain horizontal for the current HUD.
        public float Angle => horizontalState.angle;
        public float Velocity => horizontalState.velocity;
        public float HorizontalAngle => horizontalState.angle;
        public float VerticalAngle => verticalState.angle;
        public float HorizontalVelocity => horizontalState.velocity;
        public float VerticalVelocity => verticalState.velocity;

        /// <summary>Largest absolute angle observed since the last <see cref="ResetMotion"/>.</summary>
        public float PeakAmplitude => Mathf.Max(horizontalState.peakAmplitude, verticalState.peakAmplitude);

        /// <summary>True on the frame the head crosses the neutral axis (a real "swing" happened).</summary>
        public bool InvertedThisFrame => horizontalState.invertedThisFrame || verticalState.invertedThisFrame;

        void Awake()
        {
            if (head != null) neutralHeadPosition = head.localPosition;
            ApplyTuning();
        }

        void OnValidate() => ApplyTuning();

        void ApplyTuning()
        {
            var tuning = new HeadMotionTuning(
                impulse, damping, returnStrength, maxAngle, maxVelocity,
                qualityReferenceVelocity, qualityReferenceAmplitude);
            horizontalState.tuning = tuning;
            verticalState.tuning = tuning;
        }

        public void ResetMotion()
        {
            ApplyTuning();
            horizontalState.ResetDynamics();
            verticalState.ResetDynamics();
            if (head != null)
            {
                head.localRotation = Quaternion.identity;
                head.localPosition = neutralHeadPosition;
            }
        }

        public void Bang(BangDirection inversionPoint, float intensity = 1f)
        {
            // A tap is an inversion/launch. It is accepted anywhere in travel.
            // The named zone is where the inversion happens; motion launches to the opposite side.
            var launch = inversionPoint.LaunchVector();
            if (inversionPoint.Axis() == BangAxis.Horizontal)
                horizontalState.ApplyImpulse(launch.x, intensity);
            else
                verticalState.ApplyImpulse(launch.y, intensity);
        }

        /// <summary>
        /// Normalized 0..1 estimate of how "good" the current head motion is for a
        /// bang in <paramref name="requestedDirection"/> (-1 left, +1 right).
        /// Combines direction coherence, angular velocity and recent peak amplitude.
        /// </summary>
        public float SampleMotionQuality(BangDirection inversionPoint)
        {
            // Judge the motion arriving INTO the inversion point, before applying the new launch.
            switch (inversionPoint)
            {
                case BangDirection.Left: return horizontalState.EvaluateMotionQuality(-1f);
                case BangDirection.Right: return horizontalState.EvaluateMotionQuality(1f);
                case BangDirection.Up: return verticalState.EvaluateMotionQuality(1f);
                case BangDirection.Down: return verticalState.EvaluateMotionQuality(-1f);
                default: return 0f;
            }
        }

        void Update()
        {
            horizontalState.Integrate(Time.deltaTime);
            verticalState.Integrate(Time.deltaTime);
            if (head != null)
            {
                head.localRotation = Quaternion.Euler(verticalState.angle, 0f, horizontalState.angle);
                head.localPosition = neutralHeadPosition + Vector2.up * (verticalState.angle * verticalVisualScale);
            }
        }
    }

    /// <summary>Immutable tuning parameters for the head motion model.</summary>
    public readonly struct HeadMotionTuning
    {
        public readonly float impulse;
        public readonly float damping;
        public readonly float returnStrength;
        public readonly float maxAngle;
        public readonly float maxVelocity;
        public readonly float referenceVelocity;
        public readonly float referenceAmplitude;

        public HeadMotionTuning(float impulse, float damping, float returnStrength,
            float maxAngle, float maxVelocity, float referenceVelocity, float referenceAmplitude)
        {
            this.impulse = impulse;
            this.damping = damping;
            this.returnStrength = returnStrength;
            this.maxAngle = Mathf.Max(1f, maxAngle);
            this.maxVelocity = Mathf.Max(1f, maxVelocity);
            this.referenceVelocity = Mathf.Max(1f, referenceVelocity);
            this.referenceAmplitude = Mathf.Max(1f, referenceAmplitude);
        }
    }

    /// <summary>
    /// Pure, deterministic head motion integrator + quality heuristics.
    /// Kept free of UnityEngine object references so it is unit-testable.
    /// </summary>
    public struct HeadMotionState
    {
        public HeadMotionTuning tuning;
        public float angle;
        public float velocity;
        public float peakAmplitude;
        public bool invertedThisFrame;

        public static HeadMotionState CreateDefault()
        {
            return new HeadMotionState
            {
                tuning = new HeadMotionTuning(190f, 5.5f, 9f, 42f, 360f, 220f, 24f),
                angle = 0f,
                velocity = 0f,
                peakAmplitude = 0f,
                invertedThisFrame = false
            };
        }

        public void ResetDynamics()
        {
            angle = 0f;
            velocity = 0f;
            peakAmplitude = 0f;
            invertedThisFrame = false;
        }

        public void ApplyImpulse(float direction, float intensity)
        {
            var sign = Mathf.Sign(direction);
            if (sign == 0f) sign = 1f;
            velocity += sign * tuning.impulse * Mathf.Clamp01(intensity);
            velocity = Mathf.Clamp(velocity, -tuning.maxVelocity, tuning.maxVelocity);
        }

        /// <summary>Advances the simulation by <paramref name="dt"/> seconds (framerate independent).</summary>
        public void Integrate(float dt)
        {
            if (dt <= 0f)
            {
                invertedThisFrame = false;
                return;
            }

            var previousAngle = angle;

            // Spring pulls the head back to neutral; exponential damping bleeds off velocity.
            velocity += -angle * tuning.returnStrength * dt;
            velocity *= Mathf.Exp(-tuning.damping * dt);
            angle += velocity * dt;

            if (Mathf.Abs(angle) > tuning.maxAngle)
            {
                angle = Mathf.Clamp(angle, -tuning.maxAngle, tuning.maxAngle);
                velocity *= -0.15f;
            }

            // Crossing the neutral axis marks a genuine swing/inversion.
            invertedThisFrame = (previousAngle < 0f && angle >= 0f) || (previousAngle > 0f && angle <= 0f);

            var absAngle = Mathf.Abs(angle);
            if (absAngle > peakAmplitude) peakAmplitude = absAngle;
        }

        /// <summary>
        /// 0..1 motion quality for a bang in <paramref name="requestedDirection"/>.
        /// direction coherence (is the head moving the requested way) is weighted most,
        /// then angular velocity magnitude, then recent peak amplitude.
        /// </summary>
        public float EvaluateMotionQuality(float requestedDirection)
        {
            var reqSign = Mathf.Sign(requestedDirection);
            if (reqSign == 0f) reqSign = 1f;

            // Coherence: +1 when moving strongly in the requested direction, ~0 when still/opposite.
            var normalizedVel = Mathf.Clamp(velocity / tuning.referenceVelocity, -1f, 1f);
            var coherence = Mathf.Clamp01(normalizedVel * reqSign);

            var velMagnitude = Mathf.Clamp01(Mathf.Abs(velocity) / tuning.referenceVelocity);
            var amplitude = Mathf.Clamp01(peakAmplitude / tuning.referenceAmplitude);

            // Weighted blend, then a small floor so a correct on-beat tap is never zero.
            var quality = 0.55f * coherence + 0.30f * velMagnitude + 0.15f * amplitude;
            return Mathf.Clamp01(0.2f + 0.8f * quality);
        }
    }
}
