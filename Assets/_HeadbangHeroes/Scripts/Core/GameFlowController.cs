using System.Text;
using HeadbangHeroes.Gameplay.Scoring;
using HeadbangHeroes.Meta;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HeadbangHeroes.Core
{
    /// <summary>
    /// Minimal POC product loop: Home -> Song Select -> Pre-song -> Gameplay -> Results ->
    /// Retry/Continue. Thin application orchestrator — it consumes the authoritative
    /// <see cref="RunResult"/> from <see cref="PrototypeController"/>, runs it through the meta
    /// services (Results -> Progression -> Save), and shows panels. It NEVER recalculates score
    /// and NEVER writes gameplay state; gameplay never writes files (SaveService does).
    ///
    /// Panels are CanvasGroups toggled by state. Enter/Space advances the primary action, Esc goes
    /// back, so the loop is fully drivable on desktop even before UI buttons are wired.
    /// </summary>
    public sealed class GameFlowController : MonoBehaviour
    {
        public enum FlowState { Boot, Home, SongSelect, PreSong, Gameplay, Results }

        [SerializeField] PrototypeController gameplay;

        [Header("Panels (CanvasGroups)")]
        [SerializeField] CanvasGroup homePanel;
        [SerializeField] CanvasGroup songSelectPanel;
        [SerializeField] CanvasGroup preSongPanel;
        [SerializeField] CanvasGroup resultsPanel;

        [Header("Texts")]
        [SerializeField] Text homeText;
        [SerializeField] Text songSelectText;
        [SerializeField] Text preSongText;
        [SerializeField] Text resultsText;

        [Header("Results — emotion-first acts (P10)")]
        [SerializeField] Text resultsGradeText;    // Act 1: dominant grade
        [SerializeField] Text resultsScoreText;     // Act 1: score
        [SerializeField] Text resultsCommentText;   // Act 1: themed contextual line
        [SerializeField] Text resultsReportText;    // Act 2: performance card
        [SerializeField] Text resultsRewardsText;   // Act 3: rewards/progression

        [Header("Content identity (POC single song)")]
        [SerializeField] string songTitle = "Beyond the Pain";
        [SerializeField] string songArtist = "Asidie";
        [SerializeField] string chartDifficulty = "Prototype";

        readonly GradeConfig gradeConfig = GradeConfig.Default;
        readonly ProgressionConfig progressionConfig = ProgressionConfig.Default;
        // Normalization targets for this POC chart (tuning; keeps grade comparable per chart).
        readonly GradeNormalization norm = new GradeNormalization(
            targetScore: 200000, targetLongestCombo: 60, targetHype: 120, targetFinishers: 2);

        SaveService save;
        UserProfile profile;
        FlowState state = FlowState.Boot;
        readonly StringBuilder sb = new(256);

        void Awake()
        {
            // Boot: local-first profile load (offline). SaveService owns file I/O, not gameplay.
            save = new SaveService(new FileSaveStore());
            profile = save.Load();
        }

        void OnEnable()
        {
            if (gameplay != null) gameplay.RunCompleted += OnRunCompleted;
        }

        void OnDisable()
        {
            if (gameplay != null) gameplay.RunCompleted -= OnRunCompleted;
        }

        void Start() => GoHome();

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            var advance = kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame;
            var back = kb.escapeKey.wasPressedThisFrame;

            switch (state)
            {
                case FlowState.Home:
                    if (advance) GoSongSelect();
                    break;
                case FlowState.SongSelect:
                    if (advance) GoPreSong();
                    else if (back) GoHome();
                    break;
                case FlowState.PreSong:
                    if (advance) StartRun();
                    else if (back) GoSongSelect();
                    break;
                case FlowState.Results:
                    if (advance) Retry();            // primary = RETRY
                    else if (back) ContinueToSelect(); // secondary = CONTINUE
                    break;
            }
        }

        // ---- Navigation ----

        public void GoHome()
        {
            state = FlowState.Home;
            Show(homePanel); Hide(songSelectPanel); Hide(preSongPanel); Hide(resultsPanel);
            if (homeText != null)
                homeText.text = $"HEADBANG HEROES\n\nLv {profile.level}   XP {profile.xp}   HH {profile.hhCurrency}\n\n[Enter] PLAY";
        }

        public void GoSongSelect()
        {
            state = FlowState.SongSelect;
            Hide(homePanel); Show(songSelectPanel); Hide(preSongPanel); Hide(resultsPanel);
            if (songSelectText != null)
            {
                var rec = FindBestRecordText();
                songSelectText.text = $"SONG SELECT\n\n{songArtist} — {songTitle}\nDifficulty: {chartDifficulty}\n{rec}\n\n[Enter] Select   [Esc] Back";
            }
        }

        public void GoPreSong()
        {
            state = FlowState.PreSong;
            Hide(homePanel); Hide(songSelectPanel); Show(preSongPanel); Hide(resultsPanel);
            if (preSongText != null)
                preSongText.text = $"{songTitle}\n{chartDifficulty}\n\nReady?\n\n[Enter] START   [Esc] Back";
        }

        public void StartRun()
        {
            state = FlowState.Gameplay;
            Hide(homePanel); Hide(songSelectPanel); Hide(preSongPanel); Hide(resultsPanel);
            if (gameplay == null) return;

            gameplay.ApplyProfileSettings(
                profile.calibration.CombinedSeconds,
                profile.settings.reducedFlash, profile.settings.reducedShake,
                profile.settings.hapticsEnabled, profile.settings.hapticIntensity);
            gameplay.StartPrototype();
        }

        public void Retry() => StartRun();   // one-tap clean retry (StartPrototype resets all run state)

        public void ContinueToSelect() => GoSongSelect();

        /// <summary>
        /// Abandon the current run (QUIT from the pause overlay): stop gameplay without finalizing a
        /// RunResult and return to Song Select. Guarded to the Gameplay state so it can't fire twice.
        /// </summary>
        public void AbortToSongSelect()
        {
            if (state != FlowState.Gameplay) return;
            if (gameplay != null) gameplay.AbortRun();
            GoSongSelect();
        }

        // ---- Run completion -> meta pipeline ----

        void OnRunCompleted(RunResult run)
        {
            // Defend against a duplicate/re-entrant completion: only apply the meta pipeline once,
            // when transitioning out of the Gameplay state.
            if (state != FlowState.Gameplay) return;

            // Results consume the authoritative result; they never recalculate score.
            var grade = ResultsService.Grade(run, gradeConfig, norm);
            var tags = ResultsService.DeriveTags(run);

            // Progression consumes result+grade and updates the profile; then persist locally.
            var outcome = ProgressionService.Apply(profile, run, grade, progressionConfig);
            save.Save(profile);

            ShowResults(run, grade, outcome, tags);
        }

        void ShowResults(in RunResult run, in GradeResult grade, in ProgressionOutcome outcome,
            System.Collections.Generic.IReadOnlyList<ResultTag> tags)
        {
            state = FlowState.Results;
            Hide(homePanel); Hide(songSelectPanel); Hide(preSongPanel); Show(resultsPanel);

            // Emotion-first layout (RESULTS_SCREEN_V1 / ADR-aligned): grade dominant, then score,
            // then a themed contextual line, then the report, then rewards. Renders the authoritative
            // result only — never recomputes score/grade.
            var comment = ResultCommentSelector.SelectLine(grade.Grade, tags);

            if (resultsGradeText != null)
            {
                // Act 1 — Impact. Grade dominates; scale emphasis by grade (S biggest .. D smallest).
                resultsGradeText.text = grade.Grade.ToString();
                resultsGradeText.fontSize = GradeFontSize(grade.Grade);
                resultsGradeText.color = GradeColor(grade.Grade);
                if (resultsScoreText != null) resultsScoreText.text = run.Score.ToString("N0");
                if (resultsCommentText != null) resultsCommentText.text = comment;

                // Act 2 — Performance report (card).
                if (resultsReportText != null)
                {
                    sb.Clear();
                    sb.Append("HEADBANG REPORT\n\n");
                    sb.Append("LONGEST COMBO   ").Append(run.LongestCombo).Append('\n');
                    sb.Append("COMBOS          ").Append(run.CompletedCombos).Append('\n');
                    sb.Append("TOTAL HYPE      ").Append(run.TotalHypeEarned).Append('\n');
                    sb.Append("FINISHERS       ").Append(run.FinishersExecuted).Append("\n\n");
                    sb.Append("PERFECT ").Append(run.PerfectCount)
                      .Append("   GREAT ").Append(run.GreatCount)
                      .Append("   GOOD ").Append(run.GoodCount)
                      .Append("   WELL ").Append(run.WellCount)
                      .Append("   MISS ").Append(run.MissCount);
                    resultsReportText.text = sb.ToString();
                }

                // Act 3 — Rewards / progression.
                if (resultsRewardsText != null)
                {
                    sb.Clear();
                    sb.Append("XP +").Append(outcome.XpEarned).Append("    HH +").Append(outcome.HhEarned);
                    if (outcome.LeveledUp) sb.Append("\nLEVEL UP -> ").Append(outcome.LevelAfter);
                    if (outcome.FirstClear) sb.Append("\nFIRST CLEAR");
                    if (outcome.FirstS) sb.Append("\nFIRST S!");
                    resultsRewardsText.text = sb.ToString();
                }
                return;
            }

            // Fallback: single-text block (if the act fields are not wired).
            if (resultsText == null) return;
            sb.Clear();
            sb.Append("HEADBANG REPORT\n\n");
            sb.Append("GRADE   ").Append(grade.Grade).Append('\n');
            sb.Append("SCORE   ").Append(run.Score.ToString("N0")).Append('\n');
            if (!string.IsNullOrEmpty(comment)) sb.Append(comment).Append('\n');
            sb.Append("MAX COMBO   ").Append(run.LongestCombo).Append('\n');
            sb.Append("TOTAL HYPE  ").Append(run.TotalHypeEarned).Append('\n');
            sb.Append("FINISHERS   ").Append(run.FinishersExecuted).Append('\n');
            sb.Append("PERFECT ").Append(run.PerfectCount)
              .Append("  GREAT ").Append(run.GreatCount)
              .Append("  GOOD ").Append(run.GoodCount)
              .Append("  WELL ").Append(run.WellCount)
              .Append("  MISS ").Append(run.MissCount).Append("\n\n");
            sb.Append("XP +").Append(outcome.XpEarned).Append("   HH +").Append(outcome.HhEarned);
            if (outcome.LeveledUp) sb.Append("   LEVEL UP -> ").Append(outcome.LevelAfter);
            if (outcome.FirstClear) sb.Append("   FIRST CLEAR");
            if (outcome.FirstS) sb.Append("   FIRST S!");
            resultsText.text = sb.ToString();
        }

        // Grade-scaled emphasis (RESULTS_SCREEN_V1: S oversized .. D underwhelming). Presentation only.
        static int GradeFontSize(Grade g) => g switch
        {
            Grade.S => 220, Grade.A => 190, Grade.B => 160, Grade.C => 140, _ => 120
        };

        static Color GradeColor(Grade g) => g switch
        {
            Grade.S => new Color(1f, 0.85f, 0.2f, 1f),   // gold
            Grade.A => new Color(0.6f, 1f, 0.4f, 1f),    // green
            Grade.B => new Color(0.5f, 0.8f, 1f, 1f),    // blue
            Grade.C => new Color(0.8f, 0.8f, 0.8f, 1f),  // grey
            _ => new Color(1f, 0.4f, 0.4f, 1f),          // red-ish for D
        };

        string FindBestRecordText()
        {
            if (profile.records == null) return "";
            foreach (var r in profile.records)
                if (r.longestCombo > 0 || r.bestScore > 0)
                    return $"Best: {r.bestScore:N0}  ({(Grade)r.bestGrade})  x{r.longestCombo}";
            return "Best: —";
        }

        static void Show(CanvasGroup g)
        {
            if (g == null) return;
            g.alpha = 1f; g.interactable = true; g.blocksRaycasts = true;
        }

        static void Hide(CanvasGroup g)
        {
            if (g == null) return;
            g.alpha = 0f; g.interactable = false; g.blocksRaycasts = false;
        }
    }
}
