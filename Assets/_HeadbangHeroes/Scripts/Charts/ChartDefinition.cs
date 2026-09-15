using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeadbangHeroes.Charts
{
    public enum BangDirection { Left = -1, Right = 1 }

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
