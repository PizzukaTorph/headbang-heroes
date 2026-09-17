using System;
using UnityEngine;

namespace HeadbangHeroes.Charts
{
    /// <summary>
    /// Parses authoring chart JSON into <see cref="ChartJsonData"/>. Parsing happens once
    /// (song load / editor build), never in the gameplay hot path. Compilation into the immutable
    /// runtime chart is <see cref="Runtime.ChartCompiler"/>'s responsibility.
    /// </summary>
    public static class ChartJsonLoader
    {
        public static ChartJsonData Parse(TextAsset source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return Parse(source.text);
        }

        public static ChartJsonData Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException("Empty Headbang Heroes chart JSON.");
            var data = JsonUtility.FromJson<ChartJsonData>(json);
            if (data == null) throw new InvalidOperationException("Could not parse Headbang Heroes chart JSON.");
            data.events ??= new System.Collections.Generic.List<ChartJsonEvent>();
            data.rests ??= new System.Collections.Generic.List<ChartJsonRest>();
            return data;
        }
    }
}
