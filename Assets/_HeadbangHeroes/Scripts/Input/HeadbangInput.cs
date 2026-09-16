using System;
using HeadbangHeroes.Charts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HeadbangHeroes.Input
{
    /// <summary>
    /// Four large invisible tap zones around screen centre. The active chart cue tells the
    /// player which zone matters; input itself stays spatially forgiving.
    ///
    /// Portrait layout uses diagonals through screen centre rather than four tiny buttons:
    /// top wedge = UP, bottom = DOWN, left = LEFT, right = RIGHT.
    /// </summary>
    public sealed class HeadbangInput : MonoBehaviour
    {
        public event Action<BangDirection> Bang;

        void Update()
        {
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.wasPressedThisFrame)
                    Emit(touch.position.ReadValue());
            }

#if UNITY_EDITOR
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                Emit(Mouse.current.position.ReadValue());
#endif
        }

        void Emit(Vector2 screenPosition) => Bang?.Invoke(ResolveZone(screenPosition, Screen.width, Screen.height));

        /// <summary>
        /// Resolves the nearest cardinal zone without requiring visible buttons.
        /// Normalizing by half-screen extents keeps the four wedges useful on portrait devices.
        /// </summary>
        public static BangDirection ResolveZone(Vector2 position, float width, float height)
        {
            var halfWidth = Mathf.Max(1f, width * 0.5f);
            var halfHeight = Mathf.Max(1f, height * 0.5f);
            var x = (position.x - halfWidth) / halfWidth;
            var y = (position.y - halfHeight) / halfHeight;

            if (Mathf.Abs(x) > Mathf.Abs(y))
                return x < 0f ? BangDirection.Left : BangDirection.Right;

            return y < 0f ? BangDirection.Down : BangDirection.Up;
        }
    }
}
