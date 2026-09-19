# Changelog

All notable changes to Headbang Heroes are documented here.

## [0.0.2] — 2026-09-19 — "From validated toy to playable prototype"

Builds on the frozen `v0.0.1` core (feel validated). No changes to neck physics / timing windows /
scoring math — all gameplay-affecting numbers are unchanged; this milestone is UI, presentation,
content, hardening, and diagnostics.

### Input & UI (P08)
- On-screen **touch controls** (Option A UX): PAUSE, THE BANG (shown only on HYPE READY), pause
  overlay (RESUME/RETRY/QUIT + calibration ±), one-tap RETRY/CONTINUE on Results. A tap over a UI
  control no longer emits a bang.
- **Affordance**: a one-shot bang-zone hint (4 direction arrows) at run start, and a per-tap
  section flash so the whole tap quadrant reads as valid.

### Timing cue (ADR-0001)
- Replaced the closing-circle-on-head with a **pulse-in-sector**: a pill next to the avatar in the
  expected direction grows/brightens with a build-up that peaks on the event, then relaxes. Unifies
  WHEN + WHERE and matches the "feel the beat" philosophy. Decision recorded in
  `docs/DECISIONS/0001-pulse-in-sector-cue.md`; UX/Screen specs annotated.

### Results (P10)
- **Emotion-first** results: dominant grade (scaled S..D), score, a data-driven themed
  **ResultComment** line (rule set + text keys, English placeholder), performance card, and rewards.
  Renders the authoritative `RunResult` — no scoring recompute.

### Hair (P11)
- **Multi-segment hair chain** (HAIR_SYSTEM_V1): lag → snap → overshoot → settle, driven by neck
  motion + inversion whip + modest HYPE. Data-driven motion tiers (Bald/Short/Medium/MediumLong/
  Long, hero Long default) with a dev-only Inspector override for live tuning. Presentation-only.

### Feel polish (P12)
- **Micro screen-shake** on bang intensity and a **high-speed motion trail** behind the head. Both
  presentation-only, capped, and suppressed by the reduced-shake / reduced-effects accessibility
  toggles.

### Hardening (P13)
- **Atomic save**: `FileSaveStore` promotes via `File.Replace` so a mid-write crash keeps the
  last-known-good backup.
- Playtest/debug controls (R/Space/B/`[`/`]`/P/Z and keyboard A/D/W/S bangs) are compiled out of
  non-development builds (`UNITY_EDITOR || DEVELOPMENT_BUILD`).

### Diagnostics
- Dev-only **PlaytestDiagnostics** (failure taxonomy, timing-bias, direction-confusion matrix),
  a `ResolveZone` visualizer, input-source tagging, and AudioClock/SectorPulseCue diagnostics for
  the MISS investigation. Report in `docs/DECISIONS/0002-miss-investigation-report.md`.

### Content
- WISH added as a playable lab track (chart from `wish.mid`; audio/MIDI kept local, gitignored) with
  a dedicated build menu variant; Beyond the Pain chart regenerated at playable density.

### Known limitations / deferred
- iOS on-device validation (Package 07) still deferred (Apple signing). Hair/avatar art is
  placeholder (cube/quad segments); hair-style selection belongs to the future Avatar Customization
  flow, not in-game settings. Real HH-MIDI importer still future work. See `errors to be fix.md`.

## [0.0.1] — 2026-09-18 — POC: "Make The Headbang Fun" (core validated)

First frozen milestone. The prototype answers its one question — **controlling the head/neck to
music is fun** — validated through extensive playtesting. Do not modify this tag; future work
continues on a new branch.

### Core loop
- Complete repeatable loop: Home → Song Select → Pre-song → Gameplay → Results → Retry/Continue,
  driven by `GameFlowController` over pure services (Results/Progression/Save).

### Gameplay
- Deterministic, FPS-independent neck simulation (`NeckMotionState` / `NeckMotionModel`) advanced
  from total elapsed time (drift-free), separate from presentation.
- DSP-authoritative timing: `AudioClock` + `SongTimeMap` as the single calibration owner; input
  converted to song-time before judging.
- Bounded deterministic event matching (`RuntimeChart` / `ChartCompiler` / `CandidateResolver` /
  `EventMatcher`): resolve-once / expire-once, nearest-by-error with direction preference.
- Judgment tiers: Perfect / Great / Good / **Well** / Miss. A cue that is present but tapped
  sloppily scores a **Well** (1 point); a MISS is only a completely early/late tap, wrong
  direction, or no cue at all.
- Scoring / combo / multiplier, HYPE, THE BANG, and Finisher with anti-circular reward snapshot.
- Grade (S/A/B/C/D) and version-aware records + profile save/migration.

### Feel & presentation (render-only, domain untouched)
- Closing-circle cue with coherent approach (bounded to event spacing so rings start full and close
  at a consistent rate) and a hit-window rest/green feedback.
- Blue tap marker: a concentric ring sized to the *outer* approach ring at the tap's timing error
  (early = larger, dead-on = coincides with target, late = smaller) — a self-calibration map.
- Neck reads as having mass: overshoot spring + speed-driven squash & stretch on the head.
- Body lean, hair, venue intensity, and haptics as downstream presentation.

### Timing tuning
- Windows kept below half the chart's minimum event gap so adjacent events do not steal each
  other's taps (P±70 / G±130 / Good±190 / Well±240 ms on the tempo-ramp lab track).
- Audio output-latency compensation in `AudioClock` (measured from the DSP buffer config) plus
  manual `[` / `]` calibration on top.

### Tooling
- Mono-scene prototype built by `M0PrototypeBuilder` (standard + tempo-ramp variants).
- EditMode tests for timing, matching, scoring/HYPE, save/progression, and song-time mapping.
- Playtest controls (keyboard): A/D/W/S bang, F1 debug HUD, B THE BANG, Space pause, R restart,
  `[` / `]` calibration, Enter/Esc navigate.

### Known limitations (deferred — see `errors to be fix.md`)
- iOS on-device validation (Package 07) deferred: blocked on Apple signing, not on code.
- On-screen touch controls for pause/retry/THE BANG/calibration not yet built (keyboard-only).
- Chart authoring is a heuristic MIDI-derived grid, not authored choreography; no runtime HH-MIDI
  importer yet. Placeholder art. Results screen layout is a flat text block.
