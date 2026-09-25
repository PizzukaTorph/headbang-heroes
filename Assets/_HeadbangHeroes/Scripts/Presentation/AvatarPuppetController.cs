using System;
using HeadbangHeroes.Gameplay;
using UnityEngine;

namespace HeadbangHeroes.Presentation
{
    public enum AvatarArtSet
    {
        Clean,
        Modular
    }

    [Serializable]
    public struct AvatarSpriteSet
    {
        public Sprite torso;
        public Sprite armLeft;
        public Sprite armRight;
        public Sprite neck;
        public Sprite head;
    }

    /// <summary>
    /// Presentation-only avatar. It reads NeckMotionModel and rotates the complete Head sprite
    /// (including its full hairstyle) as one unit; it never changes gameplay state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AvatarPuppetController : MonoBehaviour
    {
        [Header("Authoritative source (read-only)")]
        [SerializeField] NeckMotionModel neck;

        [Header("Avatar art")]
        [SerializeField] AvatarArtSet artSet = AvatarArtSet.Clean;
        [SerializeField] AvatarSpriteSet clean;
        [SerializeField] AvatarSpriteSet modular;

        [Header("Static body layers")]
        [SerializeField] SpriteRenderer torso;
        [SerializeField] SpriteRenderer armLeft;
        [SerializeField] SpriteRenderer armRight;
        [SerializeField] SpriteRenderer neckSprite;

        [Header("Complete head and hair")]
        [SerializeField] SpriteRenderer head;
        [SerializeField] Transform headPivot;

        [Header("Head presentation spring")]
        [SerializeField, Min(1f)] float headSpring = 140f;
        [SerializeField, Range(0.2f, 1.2f)] float headDamping = 0.62f;
        [SerializeField, Min(1f)] float visualMaxAngle = 42f;
        [SerializeField, Min(0f)] float verticalFollow = 1f;

        [Header("Editor alignment test")]
        [SerializeField] bool restPoseDebug;
        [SerializeField] bool debugRotationTest;
        [SerializeField, Min(0.5f)] float debugRotationPeriod = 4f;
        [SerializeField, Range(0f, 20f)] float debugRotationAmplitude = 20f;

        float headRoll;
        float headPitch;
        float rollVelocity;
        float pitchVelocity;

        public AvatarArtSet CurrentArtSet => artSet;
        public float HeadAngle => headRoll;
        public float HeadAngularVelocity => rollVelocity;
        public float HeadPitch => headPitch;

        void Awake()
        {
            if (neck == null) neck = FindAnyObjectByType<NeckMotionModel>();
            ApplyArtSet();
        }

        void OnValidate()
        {
            if (!Application.isPlaying) ApplyArtSet();
        }

        public void SetSource(NeckMotionModel value) => neck = value;

        public void Configure(NeckMotionModel source, AvatarSpriteSet cleanSet, AvatarSpriteSet modularSet,
            SpriteRenderer torsoRenderer, SpriteRenderer leftRenderer, SpriteRenderer rightRenderer,
            SpriteRenderer neckRenderer, SpriteRenderer headRenderer, Transform headPivotTransform)
        {
            neck = source;
            clean = cleanSet;
            modular = modularSet;
            torso = torsoRenderer;
            armLeft = leftRenderer;
            armRight = rightRenderer;
            neckSprite = neckRenderer;
            head = headRenderer;
            headPivot = headPivotTransform;
            ApplyArtSet();
        }

        public void SelectArtSet(AvatarArtSet value)
        {
            artSet = value;
            ApplyArtSet();
        }

        public void ResetPresentation()
        {
            headRoll = headPitch = 0f;
            rollVelocity = pitchVelocity = 0f;
            if (headPivot != null) headPivot.localRotation = Quaternion.identity;
        }

        void LateUpdate()
        {
            if (restPoseDebug)
            {
                headRoll = headPitch = 0f;
                rollVelocity = pitchVelocity = 0f;
                if (headPivot != null) headPivot.localRotation = Quaternion.identity;
                return;
            }

            if (!debugRotationTest && neck == null) neck = FindAnyObjectByType<NeckMotionModel>();
            if (!debugRotationTest && neck == null) return;

            var dt = Mathf.Clamp(Time.deltaTime, 0f, 0.1f);
            if (dt <= 0f) return;

            var targetRoll = debugRotationTest
                ? Mathf.Sin(Time.time * Mathf.PI * 2f / debugRotationPeriod) * debugRotationAmplitude
                : neck.HorizontalAngle;
            var targetPitch = debugRotationTest ? 0f : neck.VerticalAngle * verticalFollow;

            // This visual spring is downstream of the authoritative neck simulation.
            headRoll = StepAxis(headRoll, ref rollVelocity, targetRoll, dt);
            headPitch = StepAxis(headPitch, ref pitchVelocity, targetPitch, dt);
            if (headPivot != null) headPivot.localRotation = Quaternion.Euler(headPitch, 0f, headRoll);
        }

        float StepAxis(float current, ref float velocity, float target, float dt)
        {
            var remaining = Mathf.Min(dt, 0.1f);
            const float maxStep = 1f / 120f;
            var omega = Mathf.Sqrt(Mathf.Max(1f, headSpring));
            var damping = 2f * headDamping * omega;
            while (remaining > 0f)
            {
                var step = Mathf.Min(remaining, maxStep);
                velocity += (headSpring * (target - current) - damping * velocity) * step;
                current += velocity * step;
                current = Mathf.Clamp(current, -visualMaxAngle, visualMaxAngle);
                if (Mathf.Abs(current) >= visualMaxAngle && Mathf.Sign(current) == Mathf.Sign(velocity)) velocity = 0f;
                remaining -= step;
            }
            return current;
        }

        void ApplyArtSet()
        {
            var set = artSet == AvatarArtSet.Modular ? modular : clean;
            if (torso != null) torso.sprite = set.torso;
            if (armLeft != null) armLeft.sprite = set.armLeft;
            if (armRight != null) armRight.sprite = set.armRight;
            if (neckSprite != null) neckSprite.sprite = set.neck;
            if (head != null) head.sprite = set.head;
        }
    }
}
