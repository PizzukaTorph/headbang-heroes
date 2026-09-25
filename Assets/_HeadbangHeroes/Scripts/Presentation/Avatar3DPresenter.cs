using HeadbangHeroes.Gameplay;
using UnityEngine;

namespace HeadbangHeroes.Presentation
{
    /// <summary>
    /// Presentation adapter for the experimental 3D avatar. It reads NeckMotionModel and writes
    /// only bone transforms. The smoothing here is visual and never feeds back into gameplay.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Avatar3DPresenter : MonoBehaviour
    {
        [Header("Authoritative source (read-only)")]
        [SerializeField] NeckMotionModel neck;

        [Header("3D bones")]
        [SerializeField] Transform neckBone;
        [SerializeField] Transform headBone;
        [SerializeField] Transform chestBone;

        [Header("Presentation mapping")]
        [SerializeField] Avatar3DMappingConfig mapping = new Avatar3DMappingConfig
        {
            neckShare = 0.35f,
            headShare = 0.65f,
            chestCompensation = 0.08f,
            maxVisualAngle = 42f
        };
        [SerializeField, Min(0f)] float visualSmoothing = 18f;
        [SerializeField] bool restPoseDebug;

        Avatar3DPose currentPose;

        public Avatar3DMappingConfig Mapping => mapping.Sanitized();
        public Avatar3DPose CurrentPose => currentPose;

        void Awake()
        {
            if (mapping.maxVisualAngle <= 0f) mapping = Avatar3DMappingConfig.Default;
            if (neck == null) neck = FindAnyObjectByType<NeckMotionModel>();
            ApplyPose(Avatar3DPose.Neutral);
        }

        void OnValidate()
        {
            mapping = mapping.Sanitized();
            if (!Application.isPlaying) ApplyPose(Avatar3DPose.Neutral);
        }

        public void SetSource(NeckMotionModel value) => neck = value;

        public void ResetPresentation()
        {
            currentPose = Avatar3DPose.Neutral;
            ApplyPose(currentPose);
        }

        void LateUpdate()
        {
            if (restPoseDebug)
            {
                currentPose = Avatar3DPose.Neutral;
                ApplyPose(currentPose);
                return;
            }

            if (neck == null) neck = FindAnyObjectByType<NeckMotionModel>();
            if (neck == null) return;

            var target = Avatar3DMapping.Map(neck.HorizontalAngle, neck.VerticalAngle, mapping);
            var blend = visualSmoothing <= 0f
                ? 1f
                : 1f - Mathf.Exp(-visualSmoothing * Mathf.Clamp(Time.deltaTime, 0f, 0.1f));
            currentPose = Blend(currentPose, target, blend);
            ApplyPose(currentPose);
        }

        static Avatar3DPose Blend(Avatar3DPose from, Avatar3DPose to, float amount)
        {
            return new Avatar3DPose(
                Mathf.Lerp(from.neckPitch, to.neckPitch, amount),
                Mathf.Lerp(from.neckRoll, to.neckRoll, amount),
                Mathf.Lerp(from.headPitch, to.headPitch, amount),
                Mathf.Lerp(from.headRoll, to.headRoll, amount),
                Mathf.Lerp(from.chestPitch, to.chestPitch, amount),
                Mathf.Lerp(from.chestRoll, to.chestRoll, amount));
        }

        void ApplyPose(Avatar3DPose pose)
        {
            if (neckBone != null) neckBone.localRotation = Quaternion.Euler(pose.neckPitch, 0f, pose.neckRoll);
            if (headBone != null) headBone.localRotation = Quaternion.Euler(pose.headPitch, 0f, pose.headRoll);
            if (chestBone != null) chestBone.localRotation = Quaternion.Euler(pose.chestPitch, 0f, pose.chestRoll);
        }
    }
}
