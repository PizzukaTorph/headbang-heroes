using System.Text;
using HeadbangHeroes.Audio;
using HeadbangHeroes.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HeadbangHeroes.UI
{
    /// <summary>
    /// Prototype HUD: large score/combo/judgment readouts plus a corner debug block that
    /// shows live timing/motion telemetry. The debug block can be toggled off entirely
    /// (F1 by default, or via <see cref="SetDebugVisible"/>) so it is trivial to disable later.
    /// Uses legacy uGUI Text to avoid adding TextMeshPro as a dependency for M0.
    /// </summary>
    public sealed class PrototypeHud : MonoBehaviour
    {
        [SerializeField] Text scoreText;
        [SerializeField] Text comboText;
        [SerializeField] Text judgmentText;
        [SerializeField] Text debugText;

        [Header("Live telemetry sources (optional)")]
        [SerializeField] AudioClock clock;
        [SerializeField] ChartScheduler scheduler;
        [SerializeField] HeadMotionModel head;

        [SerializeField] bool debugVisible = true;
        [SerializeField] bool allowToggleKey = true;

        readonly StringBuilder sb = new(256);

        // Last judged event snapshot for the debug block.
        string lastJudgment = "-";
        double lastErrorMs;
        float lastMotion;
        float lastPerformance;
        int combo;
        long score;
        double calibrationOffsetMs;

        public void BindSources(AudioClock audioClock, ChartScheduler chartScheduler, HeadMotionModel headMotion)
        {
            clock = audioClock;
            scheduler = chartScheduler;
            head = headMotion;
        }

        public void SetDebugVisible(bool visible)
        {
            debugVisible = visible;
            if (debugText != null) debugText.enabled = visible;
        }

        public void SetCalibrationOffset(double offsetSeconds) => calibrationOffsetMs = offsetSeconds * 1000.0;

        public void ResetHud()
        {
            score = 0;
            combo = 0;
            lastJudgment = "-";
            lastErrorMs = 0;
            lastMotion = 0;
            lastPerformance = 0;
            if (scoreText != null) scoreText.text = "0";
            if (comboText != null) comboText.text = "x0";
            if (judgmentText != null) judgmentText.text = "";
        }

        public void Show(JudgmentResult result, int comboValue, long scoreValue)
        {
            combo = comboValue;
            score = scoreValue;
            lastJudgment = result.judgment.ToString();
            lastErrorMs = result.error * 1000.0;
            lastMotion = result.motionQuality;
            lastPerformance = result.Performance;

            if (scoreText != null) scoreText.text = score.ToString("N0");
            if (comboText != null) comboText.text = $"x{combo}";
            if (judgmentText != null)
                judgmentText.text = $"{result.judgment}\n{lastErrorMs:+0;-0;0} ms";
        }

        public void ShowMiss(int comboValue, long scoreValue)
        {
            combo = comboValue;
            score = scoreValue;
            lastJudgment = "MISS";
            lastMotion = 0f;
            lastPerformance = 0f;
            if (scoreText != null) scoreText.text = score.ToString("N0");
            if (comboText != null) comboText.text = $"x{combo}";
            if (judgmentText != null) judgmentText.text = "MISS";
        }

        void Start()
        {
            SetDebugVisible(debugVisible);
        }

        void Update()
        {
            if (allowToggleKey && Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
                SetDebugVisible(!debugVisible);

            if (!debugVisible || debugText == null) return;

            var songTime = clock != null ? clock.SongTime : 0d;
            var nextEvent = scheduler != null ? scheduler.NextEventTime : -1d;
            var angle = head != null ? head.Angle : 0f;
            var angVel = head != null ? head.Velocity : 0f;
            var peak = head != null ? head.PeakAmplitude : 0f;
            var paused = clock != null && clock.IsPaused;

            sb.Clear();
            sb.Append("HH M0 DEBUG");
            if (paused) sb.Append("  [PAUSED]");
            sb.Append('\n');
            sb.Append("song t : ").Append(songTime.ToString("0.000")).Append(" s\n");
            sb.Append("next ev: ").Append(nextEvent >= 0 ? nextEvent.ToString("0.000") + " s" : "-").Append('\n');
            sb.Append("err    : ").Append(lastErrorMs.ToString("+0;-0;0")).Append(" ms\n");
            sb.Append("judge  : ").Append(lastJudgment).Append('\n');
            sb.Append("motion : ").Append(lastMotion.ToString("0.00")).Append('\n');
            sb.Append("perf   : ").Append(lastPerformance.ToString("0.00")).Append('\n');
            sb.Append("combo  : ").Append(combo).Append('\n');
            sb.Append("score  : ").Append(score.ToString("N0")).Append('\n');
            sb.Append("angle  : ").Append(angle.ToString("0.0")).Append("\u00B0\n");
            sb.Append("ang vel: ").Append(angVel.ToString("0")).Append("\u00B0/s\n");
            sb.Append("peak   : ").Append(peak.ToString("0.0")).Append("\u00B0\n");
            sb.Append("offset : ").Append(calibrationOffsetMs.ToString("+0;-0;0")).Append(" ms");

            debugText.text = sb.ToString();
        }
    }
}
