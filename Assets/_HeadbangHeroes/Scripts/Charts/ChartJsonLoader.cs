using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeadbangHeroes.Charts
{
    [Serializable]
    public sealed class ChartJsonData
    {
        public string chartId;
        public int version;
        public string songId;
        public string technique;
        public string difficulty;
        public double approachTime = 1.0;
        public List<ChartEvent> events = new();
    }

    public static class ChartJsonLoader
    {
        public static ChartJsonData Parse(TextAsset source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var data = JsonUtility.FromJson<ChartJsonData>(source.text);
            if (data == null) throw new InvalidOperationException("Could not parse Headbang Heroes chart JSON.");
            data.events ??= new List<ChartEvent>();
            return data;
        }

        public static ChartDefinition CreateRuntimeChart(TextAsset source)
        {
            var data = Parse(source);
            var chart = ScriptableObject.CreateInstance<ChartDefinition>();
            chart.chartId = data.chartId;
            chart.version = data.version;
            chart.events = data.events;
            return chart;
        }
    }
}
