using System;
using System.Collections.Generic;
using System.Text;

namespace HeadbangHeroes.Charts.Authoring
{
    public readonly struct MidiNote
    {
        public readonly long Tick;
        public readonly int Note;
        public readonly int Velocity;
        public readonly long DurationTicks;

        public MidiNote(long tick, int note, int velocity, long durationTicks)
        { Tick = tick; Note = note; Velocity = velocity; DurationTicks = durationTicks; }
    }

    public sealed class MidiTrack
    {
        public string Name = "";
        public readonly List<MidiNote> Notes = new();
        public readonly List<MidiTextEvent> TextEvents = new();
    }

    public readonly struct MidiTextEvent
    {
        public readonly long Tick;
        public readonly string Text;
        public MidiTextEvent(long tick, string text) { Tick = tick; Text = text; }
    }

    public readonly struct MidiTempo
    {
        public readonly long Tick;
        public readonly int MicrosPerQuarter;
        public MidiTempo(long tick, int microsPerQuarter) { Tick = tick; MicrosPerQuarter = microsPerQuarter; }
    }

    public readonly struct MidiTimeSignature
    {
        public readonly long Tick;
        public readonly int Numerator;
        public readonly int Denominator;
        public MidiTimeSignature(long tick, int numerator, int denominator)
        { Tick = tick; Numerator = numerator; Denominator = denominator; }
    }

    /// <summary>Strict, engine-free SMF reader used only by the offline HH-MIDI importer.</summary>
    public static class MidiFileParser
    {
        public sealed class MidiParseException : Exception
        { public MidiParseException(string message) : base(message) { } }

        public sealed class MidiFile
        {
            public int Format;
            public int TicksPerQuarter;
            public readonly List<MidiTrack> Tracks = new();
            public readonly List<MidiTempo> Tempos = new();
            public readonly List<MidiTimeSignature> TimeSignatures = new();

            public double TickToSeconds(long tick)
            {
                if (tick < 0 || TicksPerQuarter <= 0) return 0d;
                var tempos = new List<MidiTempo>(Tempos);
                tempos.Sort((a, b) => a.Tick.CompareTo(b.Tick));
                double seconds = 0d;
                long lastTick = 0;
                var micros = 500000;
                foreach (var tempo in tempos)
                {
                    if (tempo.Tick > tick) break;
                    if (tempo.Tick < lastTick) continue;
                    seconds += (tempo.Tick - lastTick) / (double)TicksPerQuarter * micros / 1000000d;
                    lastTick = tempo.Tick;
                    micros = tempo.MicrosPerQuarter;
                }
                seconds += (tick - lastTick) / (double)TicksPerQuarter * micros / 1000000d;
                return seconds;
            }
        }

        public static MidiFile Parse(byte[] data)
        {
            if (data == null || data.Length < 14) throw new MidiParseException("MIDI file is too small.");
            var p = 0;
            Expect(data, ref p, "MThd");
            var headerLength = ReadU32(data, ref p, "header length");
            if (headerLength < 6 || headerLength > data.Length - p)
                throw new MidiParseException($"Invalid MThd length {headerLength}.");
            var format = ReadU16(data, ref p, "format");
            var trackCount = ReadU16(data, ref p, "track count");
            var division = ReadU16(data, ref p, "division");
            if (format > 1) throw new MidiParseException($"Unsupported SMF format {format}; only Type 0/1 is supported.");
            if ((division & 0x8000) != 0) throw new MidiParseException("SMPTE MIDI division is unsupported; use PPQ timing.");
            if (division == 0) throw new MidiParseException("MIDI PPQ division cannot be zero.");
            p = 14 + (int)headerLength - 6;

            var file = new MidiFile { Format = format, TicksPerQuarter = division };
            for (var trackIndex = 0; trackIndex < trackCount; trackIndex++)
            {
                Expect(data, ref p, "MTrk");
                var length = ReadU32(data, ref p, $"track {trackIndex} length");
                if (length > data.Length - p) throw new MidiParseException($"Track {trackIndex} exceeds the file boundary.");
                var end = p + (int)length;
                ParseTrack(data, p, end, file, trackIndex);
                p = end;
            }
            if (file.Tracks.Count != trackCount)
                throw new MidiParseException($"Expected {trackCount} tracks but parsed {file.Tracks.Count}.");
            return file;
        }

        static void ParseTrack(byte[] data, int p, int end, MidiFile file, int trackIndex)
        {
            var track = new MidiTrack();
            long tick = 0;
            var runningStatus = 0;
            var pending = new Dictionary<int, Queue<int>>();
            var sawEndOfTrack = false;
            while (p < end)
            {
                tick += ReadVlq(data, ref p, end, $"track {trackIndex} delta");
                if (p >= end) throw new MidiParseException($"Track {trackIndex} ends inside an event.");
                var status = (int)data[p];
                if ((status & 0x80) != 0) { p++; runningStatus = status; }
                else if (runningStatus < 0x80 || runningStatus == 0xFF)
                    throw new MidiParseException($"Track {trackIndex} uses running status without a prior channel event.");
                status = runningStatus;

                if (status == 0xFF)
                {
                    if (p >= end) throw new MidiParseException($"Track {trackIndex} has an incomplete meta event.");
                    var meta = data[p++];
                    var length = ReadVlq(data, ref p, end, $"track {trackIndex} meta length");
                    if (length > end - p) throw new MidiParseException($"Track {trackIndex} meta event exceeds its chunk.");
                    if (meta == 0x03) track.Name = Decode(data, p, length).Trim();
                    else if (meta == 0x01 || meta == 0x05 || meta == 0x06 || meta == 0x07)
                        track.TextEvents.Add(new MidiTextEvent(tick, Decode(data, p, length)));
                    else if (meta == 0x51 && length == 3)
                        file.Tempos.Add(new MidiTempo(tick, (data[p] << 16) | (data[p + 1] << 8) | data[p + 2]));
                    else if (meta == 0x58 && length >= 4)
                    {
                        var denominator = 1 << data[p + 1];
                        if (data[p] == 0 || denominator <= 0)
                            throw new MidiParseException($"Track {trackIndex} has an invalid time signature.");
                        file.TimeSignatures.Add(new MidiTimeSignature(tick, data[p], denominator));
                    }
                    p += (int)length;
                    if (meta == 0x2F) { sawEndOfTrack = true; break; }
                    runningStatus = 0;
                    continue;
                }
                if (status == 0xF0 || status == 0xF7)
                {
                    var length = ReadVlq(data, ref p, end, $"track {trackIndex} sysex length");
                    if (length > end - p) throw new MidiParseException($"Track {trackIndex} sysex exceeds its chunk.");
                    p += (int)length;
                    runningStatus = 0;
                    continue;
                }

                var high = status & 0xF0;
                if (high < 0x80 || high > 0xE0)
                    throw new MidiParseException($"Track {trackIndex} has unsupported status 0x{status:X2}.");
                var channel = status & 0x0F;
                var first = ReadByte(data, ref p, end, $"track {trackIndex} event");
                var second = (high == 0xC0 || high == 0xD0) ? 0 : ReadByte(data, ref p, end, $"track {trackIndex} event");
                if (first > 127 || second > 127) throw new MidiParseException($"Track {trackIndex} has an invalid MIDI data byte.");
                if (high == 0x90 && second > 0)
                {
                    var key = channel * 128 + first;
                    track.Notes.Add(new MidiNote(tick, first, second, 0));
                    if (!pending.TryGetValue(key, out var queue)) { queue = new Queue<int>(); pending[key] = queue; }
                    queue.Enqueue(track.Notes.Count - 1);
                }
                else if (high == 0x80 || (high == 0x90 && second == 0))
                {
                    var key = channel * 128 + first;
                    if (pending.TryGetValue(key, out var queue) && queue.Count > 0)
                    {
                        var index = queue.Dequeue();
                        var note = track.Notes[index];
                        track.Notes[index] = new MidiNote(note.Tick, note.Note, note.Velocity, tick - note.Tick);
                    }
                }
            }
            if (!sawEndOfTrack) throw new MidiParseException($"Track {trackIndex} is missing the End-of-Track meta event.");
            file.Tracks.Add(track);
        }

        static string Decode(byte[] data, int offset, long length)
            => Encoding.UTF8.GetString(data, offset, (int)length);

        static void Expect(byte[] data, ref int p, string value)
        {
            if (p + value.Length > data.Length) throw new MidiParseException($"Missing {value} chunk.");
            for (var i = 0; i < value.Length; i++) if (data[p + i] != value[i]) throw new MidiParseException($"Expected {value} chunk.");
            p += value.Length;
        }

        static int ReadByte(byte[] data, ref int p, int end, string where)
        { if (p >= end) throw new MidiParseException($"Unexpected end of {where}."); return data[p++]; }

        static int ReadU16(byte[] data, ref int p, string where)
        { return (ReadByte(data, ref p, data.Length, where) << 8) | ReadByte(data, ref p, data.Length, where); }

        static uint ReadU32(byte[] data, ref int p, string where)
        { return ((uint)ReadByte(data, ref p, data.Length, where) << 24) | ((uint)ReadByte(data, ref p, data.Length, where) << 16) | ((uint)ReadByte(data, ref p, data.Length, where) << 8) | (uint)ReadByte(data, ref p, data.Length, where); }

        static long ReadVlq(byte[] data, ref int p, int end, string where)
        {
            long value = 0;
            for (var i = 0; i < 4; i++)
            {
                var b = ReadByte(data, ref p, end, where);
                value = (value << 7) | (uint)(b & 0x7F);
                if ((b & 0x80) == 0) return value;
            }
            throw new MidiParseException($"{where} uses an invalid variable-length quantity.");
        }
    }
}
