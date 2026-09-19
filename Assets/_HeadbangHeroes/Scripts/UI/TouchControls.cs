using HeadbangHeroes.Core;
using UnityEngine;
using UnityEngine.UI;

namespace HeadbangHeroes.UI
{
    /// <summary>
    /// On-screen touch controls for the POC loop (v0.0.2, P08). Option A UX: the whole screen stays
    /// an invisible bang zone; these controls live at the edges / in overlays and never cover the
    /// protected gameplay region (face / neck / CURRENT / head path). Buttons are UI GraphicRaycast
    /// targets, so <see cref="Input.HeadbangInput"/> suppresses a bang when a tap lands on them.
    ///
    /// PRESENTATION/INPUT-ADAPTER ONLY: it calls existing public entry points on
    /// <see cref="GameFlowController"/> and <see cref="PrototypeController"/>. It never touches
    /// scoring, timing, or the neck simulation.
    ///
    /// THE BANG button is shown only while HYPE is READY (per GAMEPLAY_SCREEN_V1: the player manually
    /// taps to activate the enhanced state). While hidden, its area reverts to a normal bang zone.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TouchControls : MonoBehaviour
    {
        [Header("Flow / gameplay (wired by the builder)")]
        [SerializeField] GameFlowController flow;
        [SerializeField] PrototypeController gameplay;

        [Header("In-gameplay controls")]
        [SerializeField] Button pauseButton;
        [SerializeField] Button theBangButton;          // shown only on HYPE READY
        [SerializeField] CanvasGroup theBangGroup;       // toggled for visibility/raycast

        [Header("Pause overlay")]
        [SerializeField] CanvasGroup pauseOverlay;
        [SerializeField] Button resumeButton;
        [SerializeField] Button pauseRetryButton;
        [SerializeField] Button quitButton;
        [SerializeField] Button calMinusButton;
        [SerializeField] Button calPlusButton;
        [SerializeField] Text calibrationText;

        [Header("Results actions")]
        [SerializeField] Button resultsRetryButton;
        [SerializeField] Button resultsContinueButton;

        bool wireDone;

        // --- Builder assignment helpers (SerializedObject can't call methods) ---
        public void SetFlow(GameFlowController value) => flow = value;
        public void SetGameplay(PrototypeController value) => gameplay = value;

        void OnEnable() => Wire();

        void Wire()
        {
            if (wireDone) return;
            wireDone = true;

            Bind(pauseButton, OnPause);
            Bind(theBangButton, OnTheBang);

            Bind(resumeButton, OnResume);
            Bind(pauseRetryButton, OnRetry);
            Bind(quitButton, OnQuit);
            Bind(calMinusButton, () => OnCalibrate(-1));
            Bind(calPlusButton, () => OnCalibrate(+1));

            Bind(resultsRetryButton, OnRetry);
            Bind(resultsContinueButton, OnContinue);

            SetPauseOverlay(false);
            SetTheBangVisible(false);
        }

        static void Bind(Button b, UnityEngine.Events.UnityAction action)
        {
            if (b == null) return;
            b.onClick.RemoveListener(action);
            b.onClick.AddListener(action);
        }

        void Update()
        {
            if (gameplay == null) return;

            // THE BANG button follows HYPE READY (and hides while paused / not running).
            SetTheBangVisible(gameplay.IsHypeReady);

            // Keep the pause overlay in sync with the actual paused state so keyboard Space and the
            // on-screen button agree.
            var paused = gameplay.IsPaused;
            if (paused != pauseOverlayShown) SetPauseOverlay(paused);
            if (paused && calibrationText != null) RefreshCalibrationText();
        }

        // --- Actions (route to existing public entry points) ---

        void OnPause()
        {
            if (gameplay == null || !gameplay.IsRunning) return;
            gameplay.Pause();                 // overlay follows via Update()
        }

        void OnResume() => gameplay?.Resume();

        void OnTheBang() => gameplay?.TryActivateTheBang();

        void OnRetry()
        {
            // Retry works from both the pause overlay and the results screen; the flow resets state.
            if (flow != null) flow.Retry();
        }

        void OnContinue()
        {
            if (flow != null) flow.ContinueToSelect();
        }

        void OnQuit()
        {
            // Abandon the run and return to Song Select (Option A). Resume first so the clock isn't
            // left paused, then abort.
            gameplay?.Resume();
            if (flow != null) flow.AbortToSongSelect();
        }

        void OnCalibrate(int steps)
        {
            gameplay?.NudgeCalibration(steps);
            RefreshCalibrationText();
        }

        // --- Visibility helpers ---

        bool pauseOverlayShown;

        void SetPauseOverlay(bool visible)
        {
            pauseOverlayShown = visible;
            if (pauseOverlay == null) return;
            pauseOverlay.alpha = visible ? 1f : 0f;
            pauseOverlay.interactable = visible;
            pauseOverlay.blocksRaycasts = visible;
            if (visible) RefreshCalibrationText();
        }

        void SetTheBangVisible(bool visible)
        {
            if (theBangGroup == null) return;
            theBangGroup.alpha = visible ? 1f : 0f;
            theBangGroup.interactable = visible;
            theBangGroup.blocksRaycasts = visible;   // when hidden, taps fall through to the bang zone
        }

        void RefreshCalibrationText()
        {
            if (calibrationText == null || gameplay == null) return;
            var offsetMs = gameplay.CalibrationSeconds * 1000.0;
            var latencyMs = gameplay.OutputLatencySeconds * 1000.0;
            calibrationText.text = $"CALIBRATION\noffset {offsetMs:+0;-0;0} ms\nlatency {latencyMs:0} ms";
        }
    }
}
