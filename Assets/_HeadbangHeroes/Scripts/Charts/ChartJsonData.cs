using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeadbangHeroes.Charts
{
    /// <summary>
    /// Authoring-side JSON model for M0/dev fixtures. This is NOT the runtime shape: it is parsed
    /// once (never in the gameplay hot path) and compiled into an immutable
    /// <see cref="Runtime.RuntimeChart"/> by <see cref="Runtime.ChartCompiler"/>.
    ///
    /// Fields beyond the original M0 set are optional; older fixtures that only specify
    /// {time, direction, intensity} events still parse and compile (Classic/Horizontal/None).
    /// </summary>
    [Serializable]
    public sealed class ChartJsonData
    {
        public string chartId;
        public int version;               // legacy chart version
        public string songId;
        public string technique;          // chart-level default technique (e.g. "ClassicBang"/"Classic")
        public string difficulty;
        public double approachTime = 1.0;

        // Optional explicit version identity (defaults applied by the compiler when 0).
        public int schemaVersion;
        public int rulesVersion;

        public List<ChartJsonEvent> events = new();
        public List<ChartJsonRest> rests = new();
    }

    /// <summary>Authoring motion event. Only <c>time</c>/<c>direction</c> are required.</summary>
    [Serializable]
    public struct ChartJsonEvent
    {
        public string id;                 // optional; compiler generates a stable id if empty
        public double time;               // seconds (current subset authored directly in seconds)
        public int direction;             // M0 encoding: -1 = Left, +1 = Right; 2 = Up, 3 = Down
        public string directionName;      // optional explicit "left"/"right"/"up"/"down"
        public float intensity;
        public string technique;          // optional per-event override
        public string trajectory;         // optional
        public string modifier;           // optional
        public double durationBeats;      // reserved; not used by current subset
        public double duration;           // seconds; 0 if instantaneous
        public bool finisherCandidate;
    }

    /// <summary>Authoring Rest interval.</summary>
    [Serializable]
    public struct ChartJsonRest
    {
        public string id;
        public double time;               // start (seconds)
        public double duration;           // total interval (seconds)
        public double settlingDuration;   // portion allowed to bleed momentum (seconds)
    }
}
