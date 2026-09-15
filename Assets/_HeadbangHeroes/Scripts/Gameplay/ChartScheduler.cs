using System;
using HeadbangHeroes.Audio;
using HeadbangHeroes.Charts;
using UnityEngine;

namespace HeadbangHeroes.Gameplay
{
    public sealed class ChartScheduler : MonoBehaviour
    {
        [SerializeField] AudioClock clock;
        [SerializeField] ChartDefinition chart;
        [SerializeField, Min(0.25f)] double approachTime = 1.0;

        int nextIndex;
        int activeIndex = -1;

        public event Action<ChartEvent, double> CueActivated;
        public event Action<ChartEvent> EventMissed;

        public bool HasActiveEvent => activeIndex >= 0;
        public ChartEvent ActiveEvent => chart.events[activeIndex];
        public double ApproachTime => approachTime;

        /// <summary>Song time (offset-corrected) of the next event that has not yet been activated, or -1.</summary>
        public double NextEventTime
        {
            get
            {
                if (chart == null) return -1;
                if (activeIndex >= 0) return chart.events[activeIndex].time;
                if (nextIndex < chart.events.Count) return chart.events[nextIndex].time;
                return -1;
            }
        }

        public void Configure(ChartDefinition value) => chart = value;

        public void ConfigureApproachTime(double value) => approachTime = Math.Max(0.25, value);

        public void ResetScheduler()
        {
            nextIndex = 0;
            activeIndex = -1;
        }

        void Update()
        {
            if (clock == null || chart == null || !clock.IsScheduled) return;
            var now = clock.SongTime;

            // Expire the active event once it is later than the Good window: auto-miss.
            if (activeIndex >= 0 && now - ActiveEvent.time > JudgmentSystem.Good)
            {
                var missed = ActiveEvent;
                activeIndex = -1;
                EventMissed?.Invoke(missed);
            }

            // Activate the next event once it enters the approach window.
            if (activeIndex < 0 && nextIndex < chart.events.Count)
            {
                var candidate = chart.events[nextIndex];
                var until = candidate.time - now;
                if (until <= approachTime)
                {
                    activeIndex = nextIndex++;
                    CueActivated?.Invoke(candidate, approachTime);
                }
            }
        }

        /// <summary>
        /// Attempts to judge the active event against an input.
        /// <paramref name="calibrationOffset"/> (seconds) is added to the raw song time so
        /// positive offset means "the player is treated as slightly later" — it never edits audio.
        ///
        /// Returns false (and leaves the event ACTIVE) when the input is earlier than the Good
        /// window: an early panic-tap must not consume the note. Late-but-within-window and
        /// wrong-direction inputs DO consume the event.
        /// </summary>
        public bool TryJudge(BangDirection inputDirection, float motionQuality, double calibrationOffset, out JudgmentResult result)
        {
            result = default;
            if (activeIndex < 0 || clock == null) return false;

            var ev = ActiveEvent;
            var error = (clock.SongTime + calibrationOffset) - ev.time;
            var directionMatches = ev.direction == inputDirection;

            var outcome = JudgmentSystem.ResolveInput(error, directionMatches, motionQuality, out result);
            if (outcome == JudgmentSystem.JudgeOutcome.TooEarlyKeep)
                return false; // early panic-tap: event stays active.

            activeIndex = -1;
            return true;
        }
    }
}
