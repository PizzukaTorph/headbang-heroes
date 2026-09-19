using HeadbangHeroes.Charts;
using HeadbangHeroes.Input;
using UnityEngine;
using UnityEngine.UI;

namespace HeadbangHeroes.UI
{
    /// <summary>
    /// DEV/PLAYTEST-ONLY visualization of the real touch-zone partition produced by
    /// <see cref="HeadbangInput.ResolveZone"/>. It does NOT re-implement the geometry: it samples the
    /// actual resolver on a grid of screen cells and tints each cell by the direction it resolves to,
    /// so the picture can never silently disagree with gameplay. Also shows the live pointer and the
    /// direction the latest pointer position resolves to. Toggle with a key; hidden by default.
    ///
    /// Presentation/diagnostic only; never touches gameplay state. Non-raycast.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZoneDebugOverlay : MonoBehaviour
    {
        [SerializeField] CanvasGroup group;
        [SerializeField] RectTransform gridRoot;     // cells are added here (fills screen)
        [SerializeField] RectTransform pointer;       // marker following the mouse/touch
        [SerializeField] Text readout;                // "resolved: RIGHT" etc.
        [SerializeField] int cellsX = 18;
        [SerializeField] int cellsY = 32;             // portrait: more rows

        static readonly Color ColLeft  = new Color(0.20f, 0.45f, 1f, 0.28f);
        static readonly Color ColRight = new Color(1f, 0.55f, 0.20f, 0.28f);
        static readonly Color ColUp    = new Color(0.30f, 1f, 0.45f, 0.28f);
        static readonly Color ColDown  = new Color(1f, 0.30f, 0.55f, 0.28f);

        bool visible;
        Image[] cells;
        int lastW, lastH;

        void Awake()
        {
            if (group != null) group.alpha = 0f;
        }

        public void Toggle() => SetVisible(!visible);

        public void SetVisible(bool v)
        {
            visible = v;
            if (group != null) group.alpha = v ? 1f : 0f;
            if (v) Rebuild();
        }

        void Update()
        {
            if (!visible) return;
            if (Screen.width != lastW || Screen.height != lastH) Rebuild();

            // Live pointer: mouse in editor, primary touch otherwise.
            Vector2 pos; bool has = false;
#if UNITY_EDITOR
            if (UnityEngine.InputSystem.Mouse.current != null)
            { pos = UnityEngine.InputSystem.Mouse.current.position.ReadValue(); has = true; }
            else pos = default;
#else
            pos = default;
            if (UnityEngine.InputSystem.Touchscreen.current != null)
            { pos = UnityEngine.InputSystem.Touchscreen.current.primaryTouch.position.ReadValue(); has = true; }
#endif
            if (has)
            {
                if (pointer != null) pointer.position = pos;
                var dir = HeadbangInput.ResolveZone(pos, Screen.width, Screen.height);
                if (readout != null) readout.text = $"resolved: {dir}\n{Screen.width}x{Screen.height}";
            }
        }

        void Rebuild()
        {
            if (gridRoot == null) return;
            lastW = Screen.width; lastH = Screen.height;

            // (Re)create the cell grid once.
            if (cells == null || cells.Length != cellsX * cellsY)
            {
                for (var i = gridRoot.childCount - 1; i >= 0; i--) Destroy(gridRoot.GetChild(i).gameObject);
                cells = new Image[cellsX * cellsY];
                for (var gy = 0; gy < cellsY; gy++)
                for (var gx = 0; gx < cellsX; gx++)
                {
                    var go = new GameObject($"cell_{gx}_{gy}", typeof(RectTransform), typeof(Image));
                    var rt = go.GetComponent<RectTransform>();
                    rt.SetParent(gridRoot, false);
                    rt.anchorMin = new Vector2(gx / (float)cellsX, gy / (float)cellsY);
                    rt.anchorMax = new Vector2((gx + 1) / (float)cellsX, (gy + 1) / (float)cellsY);
                    rt.offsetMin = rt.offsetMax = Vector2.zero;
                    var img = go.GetComponent<Image>();
                    img.raycastTarget = false;
                    cells[gy * cellsX + gx] = img;
                }
            }

            // Tint each cell by the REAL resolver at the cell centre (screen pixels).
            for (var gy = 0; gy < cellsY; gy++)
            for (var gx = 0; gx < cellsX; gx++)
            {
                var px = (gx + 0.5f) / cellsX * Screen.width;
                var py = (gy + 0.5f) / cellsY * Screen.height;
                var dir = HeadbangInput.ResolveZone(new Vector2(px, py), Screen.width, Screen.height);
                cells[gy * cellsX + gx].color = ColorFor(dir);
            }
        }

        static Color ColorFor(BangDirection d) => d switch
        {
            BangDirection.Left => ColLeft,
            BangDirection.Right => ColRight,
            BangDirection.Up => ColUp,
            _ => ColDown,
        };
    }
}
