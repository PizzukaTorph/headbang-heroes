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

        public void Configure(ChartDefinition value) => chart = value;

        public void ResetScheduler()
        {
            nextIndex = 0;
            activeIndex = -1;
        }

        void Update()
        {
            if (clock == null || chart == null || !clock.IsScheduled) return;
            var now = clock.SongTime;

            if (activeIndex >= 0 && now - ActiveEvent.time > JudgmentSystem.Good)
            {
                EventMissed?.Invoke(ActiveEvent);
                activeIndex = -1;
            }

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

        public bool TryJudge(BangDirection inputDirection, out JudgmentResult result)
        {
            result = default;
            if (activeIndex < 0 || clock == null) return false;

            var ev = ActiveEvent;
            var error = clock.SongTime - ev.time;
            result = JudgmentSystem.Evaluate(error);

            if (result.judgment == Judgment.Miss) return false;
            if (ev.direction != inputDirection) result = new JudgmentResult(Judgment.Miss, error);

            activeIndex = -1;
            return true;
        }
    }
}
