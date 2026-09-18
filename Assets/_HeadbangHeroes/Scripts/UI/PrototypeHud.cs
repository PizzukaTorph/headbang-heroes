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
        [SerializeField] Text topBarText;

        [Header("Live telemetry sources (optional)")]
        [SerializeField] AudioClock clock;
        [SerializeField] ChartScheduler scheduler;
        [SerializeField] NeckMotionModel head;

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
        double latencyMs;
        [SerializeField] float flashSeconds = 0.6f;
        float flashUntil;
        int hype;
        int maxHype = 100;
        bool hypeReady;
        bool theBangActive;
        int finishers;

        public void SetHype(int value, int max, bool ready, bool bangActive, int finishersExecuted)
        {
            hype = value;
            maxHype = max < 1 ? 1 : max;
            hypeReady = ready;
            theBangActive = bangActive;
            finishers = finishersExecuted;
        }

        readonly StringBuilder topBar = new(96);

        /// <summary>
        /// Renders the compact, always-visible top bar: score, song progress, multiplier and HYPE.
        /// Pure display of authoritative values pushed by the controller (no gameplay authority).
        /// </summary>
        public void SetTopBar(long scoreValue, double songTime, double songLength, int multiplier)
        {
            if (topBarText == null) return;
            var progress = songLength > 0.01 ? System.Math.Min(1.0, songTime / songLength) : 0.0;

            topBar.Clear();
            topBar.Append("\u2016  ")                       // pause glyph
                  .Append(scoreValue.ToString("N0")).Append("   ")
                  .Append((progress * 100.0).ToString("0")).Append("%   x")
                  .Append(multiplier).Append("   HYPE ")
                  .Append(hype).Append('/').Append(maxHype);
            if (theBangActive) topBar.Append("  THE BANG");
            else if (hypeReady) topBar.Append("  READY");
            topBarText.text = topBar.ToString();
        }

        public void BindSources(AudioClock audioClock, ChartScheduler chartScheduler, NeckMotionModel headMotion)
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
        public void SetLatencyOffset(double latencySeconds) => latencyMs = latencySeconds * 1000.0;

        public void ResetHud()
        {
            score = 0;
            combo = 0;
            hype = 0;
            hypeReady = false;
            theBangActive = false;
            finishers = 0;
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
            flashUntil = Time.unscaledTime + flashSeconds;
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
            flashUntil = Time.unscaledTime + flashSeconds;
        }

        void Start()
        {
            SetDebugVisible(debugVisible);
        }

        void Update()
        {
            // Time out the big centre judgment flash so it does not freeze on the last event
            // (notably an expired MISS between taps), which read as a permanent "MISS 0ms".
            if (judgmentText != null && flashUntil > 0f && Time.unscaledTime >= flashUntil)
            {
                judgmentText.text = "";
                flashUntil = 0f;
            }

            if (allowToggleKey && Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
                SetDebugVisible(!debugVisible);

            if (!debugVisible || debugText == null) return;

            var songTime = clock != null ? clock.SongTime : 0d;
            var nextEvent = scheduler != null ? scheduler.NextEventTime : -1d;
            var angle = head != null ? head.HorizontalAngle : 0f;
            var angVel = head != null ? head.HorizontalVelocity : 0f;
            var vAngle = head != null ? head.VerticalAngle : 0f;
            var prepared = head != null && head.Prepared;
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
            sb.Append("hype   : ").Append(hype).Append('/').Append(maxHype);
            if (theBangActive) sb.Append("  [THE BANG]");
            else if (hypeReady) sb.Append("  [READY - press B]");
            sb.Append('\n');
            sb.Append("finish : ").Append(finishers).Append('\n');
            sb.Append("h ang  : ").Append(angle.ToString("0.0")).Append("\u00B0\n");
            sb.Append("v ang  : ").Append(vAngle.ToString("0.0")).Append("\u00B0\n");
            sb.Append("h vel  : ").Append(angVel.ToString("0")).Append("\u00B0/s\n");
            sb.Append("prep   : ").Append(prepared ? "yes" : "setup").Append('\n');
            sb.Append("offset : ").Append(calibrationOffsetMs.ToString("+0;-0;0")).Append(" ms\n");
            sb.Append("latency: ").Append(latencyMs.ToString("0")).Append(" ms");

            debugText.text = sb.ToString();
        }
    }
}
