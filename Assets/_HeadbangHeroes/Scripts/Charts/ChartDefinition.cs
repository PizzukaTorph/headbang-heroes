using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeadbangHeroes.Charts
{
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

    [Serializable]
    public struct ChartEvent
    {
        [Min(0)] public double time;
        public BangDirection direction;
        [Range(0f, 1f)] public float intensity;
    }

    [CreateAssetMenu(menuName = "Headbang Heroes/Chart", fileName = "Chart_")]
    public sealed class ChartDefinition : ScriptableObject
    {
        public string chartId = "lab-001";
        public int version = 1;
        public List<ChartEvent> events = new();
    }
}
