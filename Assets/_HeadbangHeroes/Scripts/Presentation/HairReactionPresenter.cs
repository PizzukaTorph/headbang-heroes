using HeadbangHeroes.Gameplay;
using UnityEngine;

namespace HeadbangHeroes.Presentation
{
    /// <summary>
    /// Multi-segment hair secondary motion (HAIR_SYSTEM_V1). PRESENTATION ONLY: a pure
    /// <see cref="HairChainModel"/> chain lags the authoritative neck so the hair whips with the
    /// metal sequence continue -> snap -> overshoot -> settle. Reads the neck (angle + velocity) and
    /// an optional HYPE fraction (modest exaggeration); never writes gameplay state. Renders a chain
    /// of segment <see cref="Transform"/>s, each rotated by its solved angle.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HairReactionPresenter : MonoBehaviour
    {
        [SerializeField] NeckMotionModel neck;        // read-only source
        [SerializeField] Transform[] segments;         // chain segments, root-to-tip (root nearest head)
        [SerializeField] HairTier tier = HairTier.Long;

        HairChainModel model;
        float hype01;   // pushed by the controller (presentation signal); default 0

        public void SetSource(NeckMotionModel value) => neck = value;

        /// <summary>Presentation signal: current HYPE fraction 0..1 (modest exaggeration only).</summary>
        public void SetHype(float fraction01) => hype01 = fraction01;

        void Awake() => Rebuild();
        void OnValidate() => Rebuild();

        void Rebuild()
        {
            var cfg = HairMotionTier.For(tier);
            model = new HairChainModel(cfg);
        }

        public void ResetPresentation()
        {
            model?.Reset();
            hype01 = 0f;
            if (segments != null)
                foreach (var s in segments)
                    if (s != null) s.localRotation = Quaternion.identity;
        }

        void LateUpdate()
        {
            if (neck == null || model == null || segments == null || segments.Length == 0) return;

            // Hair follows the horizontal head swing (dominant headbang axis) with lag + inversion whip.
            model.Update(neck.HorizontalAngle, neck.HorizontalVelocity, hype01, Time.deltaTime);

            // Each segment's transform is rotated relative to its PARENT (previous segment / head),
            // so the chain composes into a whipping curve. The model stores absolute-ish angles; the
            // relative delta parent->segment gives the local rotation.
            var n = Mathf.Min(segments.Length, model.SegmentCount);
            var parentAngle = neck.HorizontalAngle;
            for (var i = 0; i < n; i++)
            {
                var seg = segments[i];
                if (seg == null) { parentAngle = model.SegmentAngle(i); continue; }
                var local = model.SegmentAngle(i) - parentAngle;
                seg.localRotation = Quaternion.Euler(0f, 0f, local);
                parentAngle = model.SegmentAngle(i);
            }
        }
    }
}
