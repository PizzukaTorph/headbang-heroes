using System;
using System.Collections.Generic;
using HeadbangHeroes.Charts;

namespace HeadbangHeroes.Charts.Authoring
{
    public enum HhMidiSeverity { Error, Warning, Info }

    public readonly struct HhMidiDiagnostic
    {
        public readonly HhMidiSeverity Severity;
        public readonly string Code;
        public readonly string Track;
        public readonly long AbsoluteTick;
        public readonly string Message;
        public bool IsError => Severity == HhMidiSeverity.Error;

        public HhMidiDiagnostic(HhMidiSeverity severity, string code, string track, long tick, string message)
        { Severity = severity; Code = code; Track = track; AbsoluteTick = tick; Message = message; }

        public HhMidiDiagnostic(bool isError, string message)
            : this(isError ? HhMidiSeverity.Error : HhMidiSeverity.Warning, "HHMIDI_LEGACY", "", -1, message) { }

        public override string ToString()
            => $"{Severity.ToString().ToUpperInvariant()} {Code} track={Track} tick={AbsoluteTick}: {Message}";
    }

    public sealed class HhMidiImportResult
    {
        public ChartJsonData Chart;
        public readonly List<HhMidiDiagnostic> Diagnostics = new();
        public bool HasErrors
        {
            get { foreach (var diagnostic in Diagnostics) if (diagnostic.IsError) return true; return false; }
        }
    }

    /// <summary>Offline-only HH_MIDI_STANDARD_V1 parser/validator/compiler boundary.</summary>
    public static class HhMidiImporter
    {
        const int RestNote = 60;
        static readonly HashSet<string> KnownTracks = new(StringComparer.Ordinal)
        { "HH_CLASSIC", "HH_HALF", "HH_DEEP", "HH_WHIPLASH", "HH_WINDMILL", "HH_REST", "HH_MOD", "HH_META" };
        static readonly Dictionary<string, string> DirectionalTechniques = new(StringComparer.Ordinal)
        { ["HH_CLASSIC"] = "Classic", ["HH_HALF"] = "Half", ["HH_DEEP"] = "Deep" };

        struct MotionSource
        {
            public string Track;
            public MidiNote Note;
            public string Technique;
            public string Direction;
            public string Trajectory;
            public string Modifier;
            public bool Finisher;
        }

        public static HhMidiImportResult Import(MidiFileParser.MidiFile midi, string songId, string chartId,
            double approachTime = 0.9, int schemaVersion = 1, int rulesVersion = 1)
        {
            var result = new HhMidiImportResult();
            if (midi == null) { Error(result, "HHMIDI_NO_FILE", "", -1, "No MIDI file was supplied."); return result; }
            if (midi.TicksPerQuarter < 480) Warn(result, "HHMIDI_LOW_PPQ", "", -1, $"PPQ {midi.TicksPerQuarter} is below the preferred minimum of 480.");
            if (schemaVersion != 1) Error(result, "HHMIDI_SCHEMA_UNSUPPORTED", "", -1, $"Unsupported HH MIDI standard version {schemaVersion}; supported version is 1.");
            if (rulesVersion != 1) Error(result, "HHMIDI_RULES_UNSUPPORTED", "", -1, $"Unsupported chart rules version {rulesVersion}; supported version is 1.");
            if (string.IsNullOrWhiteSpace(songId) || string.IsNullOrWhiteSpace(chartId))
                Error(result, "HHMIDI_ID_MISSING", "", -1, "songId and chartId are required importer inputs.");

            var motions = new List<MotionSource>();
            var modifiers = new Dictionary<long, List<MidiNote>>();
            var rests = new List<ChartJsonRest>();
            var seenSemanticNotes = new HashSet<string>(StringComparer.Ordinal);

            foreach (var track in midi.Tracks)
            {
                var name = Canon(track.Name);
                if (name.Length == 0) continue;
                if (!name.StartsWith("HH_", StringComparison.Ordinal))
                { Info(result, "HHMIDI_REFERENCE_IGNORED", name, -1, "Non-HH reference track ignored."); continue; }
                if (!KnownTracks.Contains(name))
                { Error(result, "HHMIDI_TRACK_UNKNOWN", track.Name, -1, "Unknown HH_* semantic track."); continue; }
                if (name == "HH_MOD")
                {
                    foreach (var note in track.Notes)
                    {
                        if (!modifiers.TryGetValue(note.Tick, out var list)) { list = new List<MidiNote>(); modifiers[note.Tick] = list; }
                        list.Add(note);
                    }
                    continue;
                }
                if (name == "HH_META") continue;
                if (name == "HH_REST")
                {
                    foreach (var note in track.Notes)
                    {
                        if (note.Note != RestNote) { Error(result, "HHMIDI_REST_NOTE", name, note.Tick, "HH_REST accepts only note 60."); continue; }
                        if (note.DurationTicks <= 0) { Error(result, "HHMIDI_REST_DURATION", name, note.Tick, "Rest must have a positive note duration."); continue; }
                        rests.Add(new ChartJsonRest { id = $"rest-{rests.Count:0000}", time = midi.TickToSeconds(note.Tick), duration = midi.TickToSeconds(note.Tick + note.DurationTicks) - midi.TickToSeconds(note.Tick), settlingDuration = 0d });
                    }
                    continue;
                }
                if (name == "HH_WHIPLASH")
                {
                    foreach (var note in track.Notes)
                        Error(result, "HHMIDI_WHIPLASH_UNSUPPORTED", name, note.Tick, "Whiplash has no approved v1 note grammar.");
                    continue;
                }
                foreach (var note in track.Notes)
                {
                    var key = name + ":" + note.Tick + ":" + note.Note;
                    if (!seenSemanticNotes.Add(key)) Error(result, "HHMIDI_DUPLICATE_EVENT", name, note.Tick, "Duplicate semantic note at the same tick.");
                    if (name == "HH_WINDMILL")
                    {
                        if (note.Note != 36 && note.Note != 35) { Error(result, "HHMIDI_WINDMILL_DIRECTION", name, note.Tick, "Windmill accepts only 36 clockwise or 35 counter-clockwise."); continue; }
                        if (note.DurationTicks <= 0) { Error(result, "HHMIDI_WINDMILL_DURATION", name, note.Tick, "Windmill requires a positive duration."); continue; }
                        motions.Add(new MotionSource { Track = name, Note = note, Technique = "Windmill", Direction = note.Note == 36 ? "left" : "right", Trajectory = "Circular", Modifier = "None" });
                        continue;
                    }
                    if (!DirectionalTechniques.TryGetValue(name, out var technique)) continue;
                    if (note.Note != 36 && note.Note != 35 && note.Note != 41 && note.Note != 45)
                    { Error(result, "HHMIDI_DIRECTION_NOTE", name, note.Tick, "Motion tracks accept notes 36/35/41/45 only."); continue; }
                    var direction = note.Note == 36 ? "left" : note.Note == 35 ? "right" : note.Note == 41 ? "up" : "down";
                    motions.Add(new MotionSource { Track = name, Note = note, Technique = technique, Direction = direction, Trajectory = (note.Note == 36 || note.Note == 35) ? "Horizontal" : "Vertical", Modifier = "None" });
                }
            }

            var byTick = new Dictionary<long, List<int>>();
            for (var i = 0; i < motions.Count; i++)
            {
                if (!byTick.TryGetValue(motions[i].Note.Tick, out var list)) { list = new List<int>(); byTick[motions[i].Note.Tick] = list; }
                list.Add(i);
            }
            foreach (var pair in byTick) if (pair.Value.Count > 1)
                Error(result, "HHMIDI_EVENT_AMBIGUOUS", "HH_*", pair.Key, $"{pair.Value.Count} motion events share this tick; chart matching would be ambiguous.");

            foreach (var pair in modifiers)
            {
                if (!byTick.TryGetValue(pair.Key, out var targets))
                { foreach (var note in pair.Value) Error(result, "HHMIDI_MOD_ORPHAN", "HH_MOD", pair.Key, $"Modifier note {note.Note} has no motion event at the same tick."); continue; }
                if (targets.Count != 1)
                { Error(result, "HHMIDI_MOD_AMBIGUOUS", "HH_MOD", pair.Key, "Modifier has more than one compatible motion target at the same tick."); continue; }
                var primary = "None";
                var finisher = false;
                foreach (var note in pair.Value)
                {
                    string modifier;
                    if (note.Note == 60) modifier = "Accent";
                    else if (note.Note == 61) modifier = "Double";
                    else if (note.Note == 62) modifier = "Hold";
                    else if (note.Note == 63) modifier = "Burst";
                    else if (note.Note == 64) { finisher = true; continue; }
                    else { Error(result, "HHMIDI_MOD_NOTE", "HH_MOD", pair.Key, $"Unsupported modifier note {note.Note}; expected 60-64."); continue; }
                    if (primary != "None") Error(result, "HHMIDI_MOD_CONFLICT", "HH_MOD", pair.Key, "Multiple primary modifiers target one motion event.");
                    primary = modifier;
                }
                var target = motions[targets[0]];
                target.Modifier = primary;
                target.Finisher = finisher;
                motions[targets[0]] = target;
            }

            motions.Sort((a, b) => { var tick = a.Note.Tick.CompareTo(b.Note.Tick); return tick != 0 ? tick : string.CompareOrdinal(a.Track, b.Track); });
            rests.Sort((a, b) => a.time.CompareTo(b.time));
            for (var i = 1; i < rests.Count; i++) if (rests[i].time < rests[i - 1].time + rests[i - 1].duration)
                Error(result, "HHMIDI_REST_OVERLAP", "HH_REST", -1, "Authored Rest intervals overlap.");

            if (motions.Count == 0 && rests.Count == 0) Error(result, "HHMIDI_EMPTY", "", -1, "No valid HH motion or Rest events were found.");
            var chart = new ChartJsonData { songId = songId, chartId = chartId, version = 1, technique = "Classic", difficulty = "Authored", approachTime = approachTime, schemaVersion = schemaVersion, rulesVersion = rulesVersion, events = new List<ChartJsonEvent>(), rests = rests };
            for (var i = 0; i < motions.Count; i++)
            {
                var source = motions[i];
                var start = midi.TickToSeconds(source.Note.Tick);
                chart.events.Add(new ChartJsonEvent { id = $"event-{i:0000}", time = start, directionName = source.Direction, technique = source.Technique, trajectory = source.Trajectory, modifier = source.Modifier, intensity = source.Note.Velocity / 127f, duration = source.Note.DurationTicks > 0 ? midi.TickToSeconds(source.Note.Tick + source.Note.DurationTicks) - start : 0d, finisherCandidate = source.Finisher });
            }
            result.Chart = result.HasErrors ? null : chart;
            return result;
        }

        static string Canon(string value) => (value ?? "").Trim().ToUpperInvariant();
        static void Add(HhMidiImportResult r, HhMidiSeverity s, string c, string t, long tick, string m) => r.Diagnostics.Add(new HhMidiDiagnostic(s, c, t, tick, m));
        static void Error(HhMidiImportResult r, string c, string t, long tick, string m) => Add(r, HhMidiSeverity.Error, c, t, tick, m);
        static void Warn(HhMidiImportResult r, string c, string t, long tick, string m) => Add(r, HhMidiSeverity.Warning, c, t, tick, m);
        static void Info(HhMidiImportResult r, string c, string t, long tick, string m) => Add(r, HhMidiSeverity.Info, c, t, tick, m);
    }
}
