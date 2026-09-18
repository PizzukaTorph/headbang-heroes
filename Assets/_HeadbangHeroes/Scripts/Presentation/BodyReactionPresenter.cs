using HeadbangHeroes.Gameplay;
using UnityEngine;

namespace HeadbangHeroes.Presentation
{
    /// <summary>
    /// Torso/shoulder reaction driven by authoritative neck motion. PRESENTATION ONLY: reads the
    /// neck's public angles and applies a smoothed lean (via the pure <see cref="BodyLeanModel"/>)
    /// to a body transform. No scoring physics; it cannot feed anything back into the neck.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BodyReactionPresenter : MonoBehaviour
    {
        [SerializeField] NeckMotionModel neck;   // read-only source
        [SerializeField] Transform body;
        [SerializeField, Range(0f, 1f)] float followFraction = 0.35f;
        [SerializeField, Min(0f)] float responsiveness = 12f;

        BodyLeanModel model;

        public void SetSource(NeckMotionModel value) => neck = value;

        void Awake() => model = new BodyLeanModel(followFraction, responsiveness);

        void OnValidate() => model = new BodyLeanModel(followFraction, responsiveness);

        public void ResetPresentation()
        {
            model.Reset();
            if (body != null) body.localRotation = Quaternion.identity;
        }

        void LateUpdate()
        {
            if (neck == null || body == null) return;
            model.Update(neck.HorizontalAngle, neck.VerticalAngle, Time.deltaTime);
            body.localRotation = Quaternion.Euler(model.LeanX, 0f, model.LeanZ);
        }
    }
}
