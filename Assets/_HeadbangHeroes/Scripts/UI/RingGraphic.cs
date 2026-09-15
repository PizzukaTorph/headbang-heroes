using UnityEngine;
using UnityEngine.UI;

namespace HeadbangHeroes.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RingGraphic : MaskableGraphic
    {
        [SerializeField, Range(8, 128)] int segments = 64;
        [SerializeField, Min(1f)] float thickness = 6f;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect = rectTransform.rect;
            var outer = Mathf.Min(rect.width, rect.height) * 0.5f;
            var inner = Mathf.Max(0f, outer - thickness);

            for (var i = 0; i <= segments; i++)
            {
                var a = (float)i / segments * Mathf.PI * 2f;
                var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                vh.AddVert(dir * outer, color, Vector2.zero);
                vh.AddVert(dir * inner, color, Vector2.zero);
            }

            for (var i = 0; i < segments; i++)
            {
                var root = i * 2;
                vh.AddTriangle(root, root + 1, root + 2);
                vh.AddTriangle(root + 2, root + 1, root + 3);
            }
        }
    }
}
