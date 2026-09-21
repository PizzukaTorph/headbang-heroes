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
        public Sprite jaw;
        public Sprite hairBack;
        public Sprite hairFront;
    }

    /// <summary>
    /// Presentation-only 2D avatar puppet. It reads the authoritative NeckMotionModel and never
    /// accepts input or writes gameplay state. The neck model remains the source of impulse,
    /// damping, limits and return-to-center; this component supplies visual spring lag and hair
    /// secondary motion around the already simulated pose.
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

        [Header("Sprite layers")]
        [SerializeField] SpriteRenderer torso;
        [SerializeField] SpriteRenderer armLeft;
        [SerializeField] SpriteRenderer armRight;
        [SerializeField] SpriteRenderer neckSprite;
        [SerializeField] SpriteRenderer head;
        [SerializeField] SpriteRenderer jaw;
        [SerializeField] SpriteRenderer hairBack;
        [SerializeField] SpriteRenderer hairFront;

        [Header("Head presentation spring")]
        [SerializeField, Min(1f)] float headSpring = 140f;
        [SerializeField, Range(0.2f, 1.2f)] float headDamping = 0.62f;
        [SerializeField, Min(1f)] float visualMaxAngle = 42f;
        [SerializeField, Min(0f)] float verticalFollow = 1f;

        [Header("Hair secondary motion")]
        [SerializeField] HairTier hairTier = HairTier.MediumLong;
        [SerializeField, Range(0f, 1f)] float hairHype = 0f;

        float headRoll;
        float headPitch;
        float rollVelocity;
        float pitchVelocity;
        HairChainModel hairModel;

        public AvatarArtSet CurrentArtSet => artSet;
        public float HeadAngle => headRoll;
        public float HeadAngularVelocity => rollVelocity;
        public float HeadPitch => headPitch;

        void Awake()
        {
            if (neck == null) neck = FindAnyObjectByType<NeckMotionModel>();
            hairModel = new HairChainModel(HairMotionTier.For(hairTier));
            ApplyArtSet();
        }

        void OnValidate()
        {
            if (!Application.isPlaying) ApplyArtSet();
            if (hairModel == null) hairModel = new HairChainModel(HairMotionTier.For(hairTier));
        }

        public void SetSource(NeckMotionModel value) => neck = value;

        public void Configure(NeckMotionModel source, AvatarSpriteSet cleanSet, AvatarSpriteSet modularSet,
            SpriteRenderer torsoRenderer, SpriteRenderer leftRenderer, SpriteRenderer rightRenderer,
            SpriteRenderer neckRenderer, SpriteRenderer headRenderer, SpriteRenderer jawRenderer,
            SpriteRenderer backHairRenderer, SpriteRenderer frontHairRenderer)
        {
            neck = source;
            clean = cleanSet;
            modular = modularSet;
            torso = torsoRenderer;
            armLeft = leftRenderer;
            armRight = rightRenderer;
            neckSprite = neckRenderer;
            head = headRenderer;
            jaw = jawRenderer;
            hairBack = backHairRenderer;
            hairFront = frontHairRenderer;
            ApplyArtSet();
        }

        /// <summary>Presentation-only set switch. Both sets remain serialized in the prefab.</summary>
        public void SelectArtSet(AvatarArtSet value)
        {
            artSet = value;
            ApplyArtSet();
        }

        public void ResetPresentation()
        {
            headRoll = headPitch = 0f;
            rollVelocity = pitchVelocity = 0f;
            hairModel?.Reset();
            if (head != null) head.transform.localRotation = Quaternion.identity;
            if (hairBack != null) hairBack.transform.localRotation = Quaternion.identity;
            if (hairFront != null) hairFront.transform.localRotation = Quaternion.identity;
        }

        void LateUpdate()
        {
            if (neck == null) neck = FindAnyObjectByType<NeckMotionModel>();
            if (neck == null) return;

            var dt = Mathf.Clamp(Time.deltaTime, 0f, 0.1f);
            if (dt <= 0f) return;

            // NeckMotionModel owns the physical impulse, damping and hard limits. This spring is
            // deliberately downstream: it adds visual mass without creating a second gameplay
            // simulation or changing the authoritative angle.
            headRoll = StepAxis(headRoll, ref rollVelocity, neck.HorizontalAngle, dt);
            headPitch = StepAxis(headPitch, ref pitchVelocity, neck.VerticalAngle * verticalFollow, dt);
            if (head != null) head.transform.localRotation = Quaternion.Euler(headPitch, 0f, headRoll);

            if (hairModel == null) hairModel = new HairChainModel(HairMotionTier.For(hairTier));
            hairModel.Update(neck.HorizontalAngle, neck.HorizontalVelocity, hairHype, dt);
            ApplyHairAngles();
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

        void ApplyHairAngles()
        {
            if (hairModel == null) return;
            var parentAngle = headRoll;
            if (hairBack != null && hairModel.SegmentCount > 0)
            {
                var absolute = hairModel.SegmentAngle(0);
                hairBack.transform.localRotation = Quaternion.Euler(0f, 0f, absolute - parentAngle);
                parentAngle = absolute;
            }
            if (hairFront != null && hairModel.SegmentCount > 1)
            {
                var absolute = hairModel.SegmentAngle(1);
                hairFront.transform.localRotation = Quaternion.Euler(0f, 0f, absolute - parentAngle);
            }
        }

        void ApplyArtSet()
        {
            var set = artSet == AvatarArtSet.Modular ? modular : clean;
            if (torso != null) torso.sprite = set.torso;
            if (armLeft != null) armLeft.sprite = set.armLeft;
            if (armRight != null) armRight.sprite = set.armRight;
            if (neckSprite != null) neckSprite.sprite = set.neck;
            if (head != null) head.sprite = set.head;
            if (jaw != null) jaw.sprite = set.jaw;
            if (hairBack != null) hairBack.sprite = set.hairBack;
            if (hairFront != null) hairFront.sprite = set.hairFront;
        }
    }
}
