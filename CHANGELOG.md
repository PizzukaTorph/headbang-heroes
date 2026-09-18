# Changelog

All notable changes to Headbang Heroes are documented here.

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
