using HeadbangHeroes.Audio;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class SongTimeMapTests
    {
        [Test]
        public void OutputLatency_AppliedExactlyOnce()
        {
            var m = SongTimeMap.Idle;
            m.Schedule(100.0, 0.0);
            m.OutputLatency = 0.090;
            Assert.AreEqual(1.090, m.SongTimeAt(101.0), 1e-9);
        }

        [Test]
        public void OutputLatencyPlusCalibration_BothAppliedOnce()
        {
            var m = SongTimeMap.Idle;
            m.Schedule(100.0, 0.0);
            m.OutputLatency = 0.090;
            m.Calibration = 0.030;
            Assert.AreEqual(1.120, m.SongTimeAt(101.0), 1e-9);
        }

        [Test]
        public void ResumeWithLatency_DoesNotDoubleCountOrJump()
        {
            var m = SongTimeMap.Idle;
            m.Schedule(100.0, 0.0);
            m.OutputLatency = 0.090;
            m.Calibration = 0.030;
            m.Pause(102.0);                 // frozen = 2.120 (2 + 0.09 + 0.03)
            Assert.AreEqual(2.120, m.SongTimeAt(150.0), 1e-9);
            m.Resume(200.0);
            Assert.AreEqual(2.120, m.SongTimeAt(200.0), 1e-9);   // no jump; latency+cal not double-counted
            Assert.AreEqual(3.120, m.SongTimeAt(201.0), 1e-9);
        }

        [Test]
        public void Idle_ReturnsZero()
        {
            var m = SongTimeMap.Idle;
            Assert.AreEqual(0d, m.SongTimeAt(123.4));
        }

        [Test]
        public void ScheduledStart_HasDeterministicReference()
        {
            var m = SongTimeMap.Idle;
            m.Schedule(anchorDsp: 100.0, startSongTime: 22.0);
            Assert.AreEqual(22.0, m.SongTimeAt(100.0), 1e-9);   // at anchor => start offset
            Assert.AreEqual(23.0, m.SongTimeAt(101.0), 1e-9);   // +1s dsp => +1s song
        }

        [Test]
        public void BeforeAnchor_DoesNotGoNegative()
        {
            var m = SongTimeMap.Idle;
            m.Schedule(100.0, 0.0);
            Assert.AreEqual(0d, m.SongTimeAt(99.0)); // lead-in before scheduled start
        }

        [Test]
        public void Calibration_AppliedExactlyOnce()
        {
            var m = SongTimeMap.Idle;
            m.Schedule(100.0, 0.0);
            m.Calibration = 0.030;
            Assert.AreEqual(1.030, m.SongTimeAt(101.0), 1e-9);
        }

        [Test]
        public void Pause_FreezesSongTime()
        {
            var m = SongTimeMap.Idle;
            m.Schedule(100.0, 0.0);
            m.Pause(102.0);
            var frozen = m.SongTimeAt(105.0); // dsp advanced, but paused
            Assert.AreEqual(2.0, frozen, 1e-9);
        }

        [Test]
        public void Resume_ContinuesWithoutJump()
        {
            var m = SongTimeMap.Idle;
            m.Schedule(100.0, 0.0);
            m.Pause(102.0);              // frozen at 2.0
            m.Resume(200.0);            // resume much later in dsp
            Assert.AreEqual(2.0, m.SongTimeAt(200.0), 1e-9);   // no jump at resume instant
            Assert.AreEqual(3.0, m.SongTimeAt(201.0), 1e-9);   // continues at 1x
        }

        [Test]
        public void RepeatedPauseResume_NoDrift()
        {
            var m = SongTimeMap.Idle;
            m.Schedule(100.0, 0.0);
            var dsp = 100.0;
            var song = 0.0;
            for (var i = 0; i < 50; i++)
            {
                dsp += 0.5; song += 0.5;
                m.Pause(dsp);
                Assert.AreEqual(song, m.SongTimeAt(dsp + 999.0), 1e-9); // frozen regardless of dsp
                m.Resume(dsp);
            }
            dsp += 1.0; song += 1.0;
            Assert.AreEqual(song, m.SongTimeAt(dsp), 1e-9); // no accumulated drift after 50 cycles
        }

        [Test]
        public void ResumePreservesCalibrationWithoutDoubleCount()
        {
            var m = SongTimeMap.Idle;
            m.Schedule(100.0, 0.0);
            m.Calibration = 0.040;
            m.Pause(102.0);             // frozen song-time = 2.040 (incl calibration)
            m.Resume(200.0);
            Assert.AreEqual(2.040, m.SongTimeAt(200.0), 1e-9); // calibration not double-applied
            Assert.AreEqual(3.040, m.SongTimeAt(201.0), 1e-9);
        }

        [Test]
        public void Restart_EstablishesCleanAnchor()
        {
            var m = SongTimeMap.Idle;
            m.Schedule(100.0, 50.0);
            m.SongTimeAt(120.0);
            m.Schedule(300.0, 0.0);     // "restart" = fresh schedule
            Assert.AreEqual(0.0, m.SongTimeAt(300.0), 1e-9);
            Assert.AreEqual(2.0, m.SongTimeAt(302.0), 1e-9);
        }

        [Test]
        public void CalibrationChangedWhilePaused_DoesNotCorruptResumeAlignment()
        {
            var m = SongTimeMap.Idle;
            m.Schedule(100.0, 0.0);
            m.Calibration = 0.010;
            m.Pause(102.0);                 // frozen song-time = 2.010 (with cal 0.010)
            Assert.AreEqual(2.010, m.SongTimeAt(150.0), 1e-9);

            // Player opens settings during the pause and changes calibration.
            m.Calibration = 0.050;
            m.Resume(200.0);

            // Resume continues from the frozen position; the NEW calibration applies going forward
            // without a spurious jump from the (newCal - oldCal) delta being double-applied.
            Assert.AreEqual(2.050, m.SongTimeAt(200.0), 1e-9);   // frozen base + new calibration
            Assert.AreEqual(3.050, m.SongTimeAt(201.0), 1e-9);
        }
    }
}
