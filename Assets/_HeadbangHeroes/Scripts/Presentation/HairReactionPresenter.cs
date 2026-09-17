using HeadbangHeroes.Gameplay;
using UnityEngine;

namespace HeadbangHeroes.Presentation
{
    /// <summary>
    /// Representative hair secondary motion. PRESENTATION ONLY: a damped-spring strand (pure
    /// <see cref="HairStrandModel"/>) lags behind the authoritative neck angle so the hair whips
    /// with inertia. Reads the neck; never writes gameplay state. Cheap enough for mobile (one
    /// spring integration per frame).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HairReactionPresenter : MonoBehaviour
    {
        [SerializeField] NeckMotionModel neck;   // read-only source
        [SerializeField] Transform strand;
        [SerializeField, Min(0f)] float stiffness = 90f;
        [SerializeField, Min(0f)] float damping = 9f;
        [SerializeField, Min(1f)] float maxLag = 60f;

        HairStrandModel model;

        public void SetSource(NeckMotionModel value) => neck = value;

        void Awake() => model = new HairStrandModel(stiffness, damping, maxLag);

        void OnValidate() => model = new HairStrandModel(stiffness, damping, maxLag);

        public void ResetPresentation()
        {
            model.Reset();
            if (strand != null) strand.localRotation = Quaternion.identity;
        }

        void LateUpdate()
        {
            if (neck == null || strand == null) return;
            // Hair follows the horizontal head swing (the dominant headbang axis) with lag.
            model.Update(neck.HorizontalAngle, Time.deltaTime);
            strand.localRotation = Quaternion.Euler(0f, 0f, model.Angle);
        }
    }
}
