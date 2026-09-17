using UnityEngine;

namespace HeadbangHeroes.Charts
{
    /// <summary>
    /// Canonical cardinal inversion vocabulary shared by input, neck and chart runtime.
    /// (The legacy M0 <c>ChartEvent</c> struct and <c>ChartDefinition</c> ScriptableObject were
    /// removed in Package 03: gameplay now consumes the immutable
    /// <see cref="Runtime.RuntimeChart"/> compiled from authoring data.)
    /// </summary>
    public enum BangDirection
    {
        Left = 0,
        Right = 1,
        Up = 2,
        Down = 3
    }

    public enum BangAxis
    {
        Horizontal,
        Vertical
    }

    public static class BangDirectionExtensions
    {
        public static BangAxis Axis(this BangDirection direction)
            => direction == BangDirection.Left || direction == BangDirection.Right
                ? BangAxis.Horizontal
                : BangAxis.Vertical;

        /// <summary>
        /// Direction the head launches after this inversion point.
        /// LEFT launches RIGHT, RIGHT launches LEFT, UP launches DOWN, DOWN launches UP.
        /// </summary>
        public static Vector2 LaunchVector(this BangDirection direction)
        {
            switch (direction)
            {
                case BangDirection.Left: return Vector2.right;
                case BangDirection.Right: return Vector2.left;
                case BangDirection.Up: return Vector2.down;
                case BangDirection.Down: return Vector2.up;
                default: return Vector2.zero;
            }
        }
    }
}
