using System;
using System.Collections.Generic;
using HeadbangHeroes.Charts.Authoring;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class HhMidiImporterTests
    {
        static MidiFileParser.MidiFile File(params MidiTrack[] tracks)
        {
            var midi = new MidiFileParser.MidiFile { Format = 1, TicksPerQuarter = 480 };
            midi.Tempos.Add(new MidiTempo(0, 500000));
            midi.TimeSignatures.Add(new MidiTimeSignature(0, 4, 4));
            midi.Tracks.AddRange(tracks);
            return midi;
        }

        static MidiTrack Track(string name, params MidiNote[] notes)
        {
            var track = new MidiTrack { Name = name };
            track.Notes.AddRange(notes);
            return track;
        }

        static MidiNote Note(long tick, int pitch, int velocity = 100, long duration = 0)
            => new MidiNote(tick, pitch, velocity, duration);

        [Test]
        public void ValidClassicWithModifier_CompilesDeterministically()
        {
            var result = HhMidiImporter.Import(File(
                Track(" drums ", Note(0, 36)),
                Track("HH_CLASSIC", Note(480, 36, 110)),
                Track("HH_MOD", Note(480, 60)),
                Track("HH_META")), "song", "normal");

            Assert.IsFalse(result.HasErrors, string.Join("\n", result.Diagnostics));
            Assert.NotNull(result.Chart);
            Assert.AreEqual(1, result.Chart.events.Count);
            Assert.AreEqual(1.0, result.Chart.events[0].time, 1e-9);
            Assert.AreEqual("left", result.Chart.events[0].directionName);
            Assert.AreEqual("Accent", result.Chart.events[0].modifier);
            Assert.AreEqual(110f / 127f, result.Chart.events[0].intensity, 1e-5f);
        }

        [Test]
        public void ReferenceTracksNeverBecomeGameplay()
        {
            var result = HhMidiImporter.Import(File(Track("DRUMS", Note(0, 36))), "song", "chart");
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(Contains(result, "HHMIDI_EMPTY"));
        }

        [Test]
        public void OrphanModifier_IsHardErrorWithStableCode()
        {
            var result = HhMidiImporter.Import(File(Track("HH_MOD", Note(480, 60))), "song", "chart");
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(Contains(result, "HHMIDI_MOD_ORPHAN"));
            Assert.IsNull(result.Chart);
        }

        [Test]
        public void ConflictingModifiers_AreRejected()
        {
            var result = HhMidiImporter.Import(File(
                Track("HH_CLASSIC", Note(480, 36)),
                Track("HH_MOD", Note(480, 60), Note(480, 63))), "song", "chart");
            Assert.IsTrue(Contains(result, "HHMIDI_MOD_CONFLICT"));
            Assert.IsNull(result.Chart);
        }

        [Test]
        public void RestAndWindmill_RequirePositiveDurations()
        {
            var result = HhMidiImporter.Import(File(
                Track("HH_REST", Note(480, 60, 100, 960)),
                Track("HH_WINDMILL", Note(1920, 36, 100, 960))), "song", "chart");
            Assert.IsFalse(result.HasErrors, string.Join("\n", result.Diagnostics));
            Assert.AreEqual(1, result.Chart.rests.Count);
            Assert.AreEqual(1, result.Chart.events.Count);
            Assert.AreEqual("Windmill", result.Chart.events[0].technique);
            Assert.AreEqual("Circular", result.Chart.events[0].trajectory);
        }

        [Test]
        public void UnsupportedWhiplashGrammar_IsRejected()
        {
            var result = HhMidiImporter.Import(File(Track("HH_WHIPLASH", Note(0, 36, 100, 480))), "song", "chart");
            Assert.IsTrue(Contains(result, "HHMIDI_WHIPLASH_UNSUPPORTED"));
            Assert.IsNull(result.Chart);
        }

        [Test]
        public void MalformedSmf_IsRejectedInsteadOfPartiallyParsed()
        {
            var bytes = new byte[] { (byte)'M', (byte)'T', (byte)'h', (byte)'d', 0, 0, 0, 6, 0, 1, 0, 1, 0x01, 0xE0 };
            Assert.Throws<MidiFileParser.MidiParseException>(() => MidiFileParser.Parse(bytes));
        }

        static bool Contains(HhMidiImportResult result, string code)
        {
            foreach (var diagnostic in result.Diagnostics) if (diagnostic.Code == code) return true;
            return false;
        }
    }
}
