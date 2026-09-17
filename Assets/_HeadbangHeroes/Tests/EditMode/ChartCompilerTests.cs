using System.Collections.Generic;
using HeadbangHeroes.Charts;
using HeadbangHeroes.Charts.Runtime;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class ChartCompilerTests
    {
        static ChartJsonData Data(params ChartJsonEvent[] events)
        {
            return new ChartJsonData
            {
                chartId = "chart-1",
                songId = "song-1",
                version = 3,
                technique = "ClassicBang",
                difficulty = "normal",
                events = new List<ChartJsonEvent>(events),
                rests = new List<ChartJsonRest>()
            };
        }

        static ChartJsonEvent M(double time, int direction, float intensity = 1f) =>
            new ChartJsonEvent { time = time, direction = direction, intensity = intensity };

        // ---- Compilation / immutability ----

        [Test]
        public void Compile_ProducesOrderedImmutableChart()
        {
            var chart = ChartCompiler.Compile(Data(M(2.0, -1), M(1.0, 1), M(3.0, 2)));
            Assert.AreEqual(3, chart.MotionCount);
            Assert.AreEqual(1.0, chart.MotionEvents[0].Time, 1e-9);
            Assert.AreEqual(2.0, chart.MotionEvents[1].Time, 1e-9);
            Assert.AreEqual(3.0, chart.MotionEvents[2].Time, 1e-9);
        }

        [Test]
        public void SourceMutationAfterCompile_DoesNotAffectRuntimeChart()
        {
            var data = Data(M(1.0, -1));
            var chart = ChartCompiler.Compile(data);

            data.events.Add(M(5.0, 1));          // mutate authoring source after compile
            data.events[0] = M(9.0, 1);

            Assert.AreEqual(1, chart.MotionCount, "runtime chart is immutable to source mutation");
            Assert.AreEqual(1.0, chart.MotionEvents[0].Time, 1e-9);
        }

        [Test]
        public void EqualTimeEvents_HaveStableOrderById()
        {
            var chart = ChartCompiler.Compile(Data(
                new ChartJsonEvent { id = "b", time = 5.0, direction = 1 },
                new ChartJsonEvent { id = "a", time = 5.0, direction = -1 }));
            Assert.AreEqual("a", chart.MotionEvents[0].Id);
            Assert.AreEqual("b", chart.MotionEvents[1].Id);
        }

        // ---- Direction normalization ----

        [Test]
        public void LegacyIntegerDirections_NormalizeToCardinals()
        {
            var chart = ChartCompiler.Compile(Data(M(1.0, -1), M(2.0, 1), M(3.0, 2), M(4.0, 3)));
            Assert.AreEqual(BangDirection.Left, chart.MotionEvents[0].Direction);
            Assert.AreEqual(BangDirection.Right, chart.MotionEvents[1].Direction);
            Assert.AreEqual(BangDirection.Up, chart.MotionEvents[2].Direction);
            Assert.AreEqual(BangDirection.Down, chart.MotionEvents[3].Direction);
        }

        [Test]
        public void NamedDirection_OverridesInteger()
        {
            var chart = ChartCompiler.Compile(Data(
                new ChartJsonEvent { time = 1.0, direction = 1, directionName = "up" }));
            Assert.AreEqual(BangDirection.Up, chart.MotionEvents[0].Direction);
        }

        // ---- Identity / version ----

        [Test]
        public void IdentityAndVersions_Preserved()
        {
            var data = Data(M(1.0, -1));
            data.schemaVersion = 1;
            data.rulesVersion = 1;
            var chart = ChartCompiler.Compile(data);
            Assert.AreEqual("chart-1", chart.ChartId);
            Assert.AreEqual("song-1", chart.SongId);
            Assert.AreEqual(3, chart.ChartVersion);
            Assert.AreEqual(1, chart.SchemaVersion);
            Assert.AreEqual(1, chart.RulesVersion);
            Assert.AreEqual("normal", chart.Difficulty);
        }

        [Test]
        public void UnsupportedSchemaVersion_FailsBeforeGameplay()
        {
            var data = Data(M(1.0, -1));
            data.schemaVersion = 999;
            Assert.Throws<ChartValidationException>(() => ChartCompiler.Compile(data));
        }

        [Test]
        public void UnsupportedRulesVersion_FailsBeforeGameplay()
        {
            var data = Data(M(1.0, -1));
            data.rulesVersion = 999;
            Assert.Throws<ChartValidationException>(() => ChartCompiler.Compile(data));
        }

        // ---- Validation failures ----

        [Test]
        public void DuplicateEventId_Fails()
        {
            Assert.Throws<ChartValidationException>(() => ChartCompiler.Compile(Data(
                new ChartJsonEvent { id = "dup", time = 1.0, direction = -1 },
                new ChartJsonEvent { id = "dup", time = 2.0, direction = 1 })));
        }

        [Test]
        public void NegativeTime_Fails()
        {
            Assert.Throws<ChartValidationException>(() => ChartCompiler.Compile(Data(M(-1.0, -1))));
        }

        [Test]
        public void UnknownTechnique_Fails()
        {
            var data = Data(M(1.0, -1));
            data.technique = "PowerBang";
            Assert.Throws<ChartValidationException>(() => ChartCompiler.Compile(data));
        }

        [Test]
        public void UnknownDirectionCode_Fails()
        {
            Assert.Throws<ChartValidationException>(() => ChartCompiler.Compile(Data(M(1.0, 42))));
        }

        [Test]
        public void MissingChartId_Fails()
        {
            var data = Data(M(1.0, -1));
            data.chartId = "";
            Assert.Throws<ChartValidationException>(() => ChartCompiler.Compile(data));
        }

        // ---- Rest compilation ----

        [Test]
        public void InvalidRestSettling_Fails()
        {
            var data = Data(M(1.0, -1));
            data.rests.Add(new ChartJsonRest { time = 5.0, duration = 2.0, settlingDuration = 3.0 });
            Assert.Throws<ChartValidationException>(() => ChartCompiler.Compile(data));
        }

        [Test]
        public void ValidRest_CompilesWithPhases()
        {
            var data = Data(M(1.0, -1));
            data.rests.Add(new ChartJsonRest { id = "rest0", time = 5.0, duration = 2.0, settlingDuration = 0.8 });
            var chart = ChartCompiler.Compile(data);
            Assert.AreEqual(1, chart.RestCount);
            var r = chart.RestEvents[0];
            Assert.AreEqual(5.8, r.EvaluationStart, 1e-9);
            Assert.AreEqual(7.0, r.EndTime, 1e-9);
        }

        // ---- Query range ----

        [Test]
        public void QueryRange_ReturnsDeterministicOrderedSubset()
        {
            var chart = ChartCompiler.Compile(Data(M(1.0, -1), M(2.0, 1), M(3.0, -1), M(4.0, 1)));
            var buffer = new List<RuntimeMotionEvent>();
            chart.QueryRange(1.5, 3.5, buffer);
            Assert.AreEqual(2, buffer.Count);
            Assert.AreEqual(2.0, buffer[0].Time, 1e-9);
            Assert.AreEqual(3.0, buffer[1].Time, 1e-9);
        }

        [Test]
        public void NaturalRest_MotionGapCreatesNoRestEvent()
        {
            // A big gap between motion events (10s..30s) must NOT create any authored Rest.
            var chart = ChartCompiler.Compile(Data(M(10.0, -1), M(30.0, 1)));
            Assert.AreEqual(0, chart.RestCount, "a plain gap in motion is Natural Rest: no RestEvent, no requirement");
            Assert.IsFalse(chart.TryGetRestAt(20.0, out _), "no authored rest is active in a natural gap");
        }

        [Test]
        public void MissingDirection_Fails()
        {
            // direction 0 with no directionName is malformed/unset and must fail, not become Left.
            Assert.Throws<ChartValidationException>(() => ChartCompiler.Compile(Data(
                new ChartJsonEvent { time = 1.0, direction = 0 })));
        }

        [Test]
        public void NullEventsCollection_FailsClearly()
        {
            var data = new ChartJsonData { chartId = "c", songId = "s", events = null };
            Assert.Throws<ChartValidationException>(() => ChartCompiler.Compile(data));
        }

        [Test]
        public void OverlappingRests_Fail()
        {
            var data = Data(M(1.0, -1));
            data.rests.Add(new ChartJsonRest { id = "a", time = 5.0, duration = 3.0, settlingDuration = 0.5 });
            data.rests.Add(new ChartJsonRest { id = "b", time = 6.0, duration = 2.0, settlingDuration = 0.5 }); // starts before a ends (8.0)
            Assert.Throws<ChartValidationException>(() => ChartCompiler.Compile(data));
        }

        [Test]
        public void RuntimeChart_CannotBeMutatedViaArrayCast()
        {
            var chart = ChartCompiler.Compile(Data(M(1.0, -1)));
            // The exposed collection must not be the backing array (which could be written to).
            Assert.IsFalse(chart.MotionEvents is RuntimeMotionEvent[],
                "MotionEvents must be a read-only view, not the mutable backing array");
        }

        // ---- Long-song timing (no accumulation drift; times are precomputed absolutes) ----

        [Test]
        public void LongSongTimes_PreservedExactly()
        {
            var events = new List<ChartJsonEvent>();
            for (var i = 0; i < 500; i++) events.Add(M(i * 0.8, i % 2 == 0 ? -1 : 1));
            var data = Data(events.ToArray());
            var chart = ChartCompiler.Compile(data);
            Assert.AreEqual(499 * 0.8, chart.MotionEvents[499].Time, 1e-9, "absolute times are not accumulated/drifted");
        }
    }
}
