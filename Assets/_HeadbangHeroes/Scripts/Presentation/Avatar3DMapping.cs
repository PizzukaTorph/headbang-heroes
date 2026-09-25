using System;
using UnityEngine;

namespace HeadbangHeroes.Presentation
{
    /// <summary>
    /// Presentation-only distribution of authoritative neck angles across 3D bones.
    /// It contains no simulation, timing, scoring, or scene references.
    /// </summary>
    [Serializable]
    public struct Avatar3DMappingConfig
    {
        [Range(0f, 1f)] public float neckShare;
        [Range(0f, 1f)] public float headShare;
        [Range(0f, 0.5f)] public float chestCompensation;
        [Min(0f)] public float maxVisualAngle;

        public static Avatar3DMappingConfig Default => new Avatar3DMappingConfig
        {
            neckShare = 0.35f,
            headShare = 0.65f,
            chestCompensation = 0.08f,
            maxVisualAngle = 42f
        };

        public Avatar3DMappingConfig Sanitized()
        {
            var result = this;
            result.neckShare = Mathf.Clamp01(result.neckShare);
            result.headShare = Mathf.Clamp01(result.headShare);
            result.chestCompensation = Mathf.Clamp(result.chestCompensation, 0f, 0.5f);
            result.maxVisualAngle = Mathf.Max(0f, result.maxVisualAngle);
            return result;
        }
    }

    public readonly struct Avatar3DPose
    {
        public readonly float neckPitch;
        public readonly float neckRoll;
        public readonly float headPitch;
        public readonly float headRoll;
        public readonly float chestPitch;
        public readonly float chestRoll;

        public Avatar3DPose(float neckPitch, float neckRoll, float headPitch, float headRoll,
            float chestPitch, float chestRoll)
        {
            this.neckPitch = neckPitch;
            this.neckRoll = neckRoll;
            this.headPitch = headPitch;
            this.headRoll = headRoll;
            this.chestPitch = chestPitch;
            this.chestRoll = chestRoll;
        }

        public static Avatar3DPose Neutral => new Avatar3DPose(0f, 0f, 0f, 0f, 0f, 0f);
    }

    public static class Avatar3DMapping
    {
        public static Avatar3DPose Map(float horizontalAngle, float verticalAngle,
            Avatar3DMappingConfig configuration)
        {
            var config = configuration.Sanitized();
            var horizontal = Mathf.Clamp(horizontalAngle, -config.maxVisualAngle, config.maxVisualAngle);
            var vertical = Mathf.Clamp(verticalAngle, -config.maxVisualAngle, config.maxVisualAngle);

            return new Avatar3DPose(
                vertical * config.neckShare,
                horizontal * config.neckShare,
                vertical * config.headShare,
                horizontal * config.headShare,
                vertical * config.chestCompensation,
                horizontal * config.chestCompensation);
        }
    }
}
