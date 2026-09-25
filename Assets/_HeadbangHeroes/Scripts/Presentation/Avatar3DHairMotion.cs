using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeadbangHeroes.Presentation
{
    /// <summary>
    /// Small presentation-only spring for existing Sidekick hair joint transforms.
    /// It never writes to gameplay state and is inert when a character has no hair_dyn_* joints.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Avatar3DHairMotion : MonoBehaviour
    {
        [Header("Resolved Sidekick joints")]
        [SerializeField] Transform hairRoot;
        [SerializeField] Transform[] dynamicJoints = new Transform[0];

        [Header("Presentation-only tuning")]
        [SerializeField, Range(0f, 1f)] float lag = 0.9f;
        [SerializeField, Min(0f)] float springStiffness = 18f;
        [SerializeField, Min(0f)] float springDamping = 4f;
        [SerializeField, Range(0.5f, 2f)] float whipGain = 1.6f;
        [SerializeField, Min(0f)] float maxSecondaryAngle = 48f;

        Quaternion[] restRotations = new Quaternion[0];
        float[] pitchAngles = new float[0];
        float[] rollAngles = new float[0];
        float[] pitchVelocities = new float[0];
        float[] rollVelocities = new float[0];
        bool initialized;

        /// <summary>Assigns the model root and discovers the package's existing hair joints.</summary>
        public void Configure(Transform modelRoot)
        {
            hairRoot = modelRoot;
            ResolveJoints();
        }

        /// <summary>
        /// Re-discovers joints so replacing the local Sidekick prefab with a bald character remains safe.
        /// </summary>
        public void ResolveJoints()
        {
            if (hairRoot == null) return;

            var found = new List<Transform>();
            foreach (var candidate in hairRoot.GetComponentsInChildren<Transform>(true))
            {
                if (candidate.name.StartsWith("hair_dyn_", StringComparison.OrdinalIgnoreCase))
                    found.Add(candidate);
            }

            dynamicJoints = found.ToArray();
            CaptureRestPose();
        }

        public void ResetMotion()
        {
            CaptureRestPose();
            for (var i = 0; i < dynamicJoints.Length; i++)
            {
                pitchAngles[i] = 0f;
                rollAngles[i] = 0f;
                pitchVelocities[i] = 0f;
                rollVelocities[i] = 0f;
                if (dynamicJoints[i] != null) dynamicJoints[i].localRotation = restRotations[i];
            }
        }

        /// <summary>
        /// Applies an under-damped inverse response to the already-computed visual head pose.
        /// </summary>
        public void Apply(Avatar3DPose pose, float deltaTime)
        {
            if (dynamicJoints == null || dynamicJoints.Length == 0) return;
            CaptureRestPose();

            var dt = Mathf.Clamp(deltaTime, 0f, 0.05f);
            if (dt <= 0f) return;

            var targetPitch = -pose.headPitch * lag * whipGain;
            var targetRoll = -pose.headRoll * lag * whipGain;
            for (var i = 0; i < dynamicJoints.Length; i++)
            {
                var joint = dynamicJoints[i];
                if (joint == null) continue;

                Step(ref pitchAngles[i], ref pitchVelocities[i], targetPitch, dt);
                Step(ref rollAngles[i], ref rollVelocities[i], targetRoll, dt);
                joint.localRotation = restRotations[i] * Quaternion.Euler(
                    pitchAngles[i], 0f, rollAngles[i]);
            }
        }

        void Step(ref float angle, ref float velocity, float target, float deltaTime)
        {
            velocity += (target - angle) * springStiffness * deltaTime;
            velocity *= Mathf.Exp(-springDamping * deltaTime);
            angle = Mathf.Clamp(angle + velocity * deltaTime, -maxSecondaryAngle, maxSecondaryAngle);
        }

        void CaptureRestPose()
        {
            if (initialized || dynamicJoints == null) return;

            restRotations = new Quaternion[dynamicJoints.Length];
            pitchAngles = new float[dynamicJoints.Length];
            rollAngles = new float[dynamicJoints.Length];
            pitchVelocities = new float[dynamicJoints.Length];
            rollVelocities = new float[dynamicJoints.Length];
            for (var i = 0; i < dynamicJoints.Length; i++)
                restRotations[i] = dynamicJoints[i] == null ? Quaternion.identity : dynamicJoints[i].localRotation;
            initialized = true;
        }

        void OnValidate()
        {
            lag = Mathf.Clamp01(lag);
            springStiffness = Mathf.Max(0f, springStiffness);
            springDamping = Mathf.Max(0f, springDamping);
            whipGain = Mathf.Clamp(whipGain, 0.5f, 2f);
            maxSecondaryAngle = Mathf.Max(0f, maxSecondaryAngle);
            if (!Application.isPlaying)
            {
                initialized = false;
                ResolveJoints();
            }
        }
    }
}
