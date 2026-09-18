# Agent: gameplay-core

## Mission

Protect and evolve the deterministic core of Headbang Heroes.

This agent owns the gameplay-domain interpretation of player intent and chart requirements. Its job is to make the headbang feel physical, readable, fair and testable without allowing presentation, frame pacing or meta systems to contaminate authoritative gameplay state.

## Read first

- `AGENTS.md`
- `docs/FOUNDATION.md`
- `docs/TECHNICAL_CONTRACTS_V1.md`
- `docs/SCORING_SYSTEM_V1.md`
- `docs/GAMEPLAY_UX_V1.md`
- `docs/SONG_CHART_MODEL_V1.md`

## Owns

- `NeckMotionModel`
- deterministic/fixed-step neck state
- semantic Bang application
- pre-inversion motion snapshots
- `MotionQualityEvaluator`
- setup/unprepared first-bang semantics
- `JudgmentSystem` domain behavior
- technique/trajectory/modifier validation
- event-resolution ordering
- wrong-direction semantics
- `ScoringSystem`
- combo/multiplier
- `HypeSystem`
- THE BANG state
- Finisher eligibility/resolution
- `GameplayRun` orchestration boundaries
- gameplay-domain tests

## Hard constraints

- Input always affects neck motion, even if early, late, wrong, or unmatched.
- MISS never freezes, snaps or resets the neck.
- Timing and Motion Quality remain separate signals.
- Motion Quality must use event/gesture-local state/history, never a run-global amplitude maximum.
- Scoring-critical simulation must not depend on render FPS.
- Presentation state must never feed authoritative gameplay outcomes.
- Difficulty must not alter neck physics simply to be harder.
- THE BANG is a state; Finisher is its payoff.
- Do not reinterpret existing chart semantics to make implementation easier.

## Preferred implementation shape

Use small pure C# state/data types wherever practical. Unity-facing MonoBehaviours should adapt scene/input/render concerns to the domain rather than contain the domain itself.

For a Bang, preserve this logical order unless a canonical spec explicitly changes it:

```text
semantic input arrives
→ timestamp already expressed in authoritative song-time
→ snapshot pre-inversion neck state/history
→ always apply physical neck input
→ find/receive eligible authored candidate
→ timing judgment
→ motion-quality evaluation from pre-inversion state/history
→ technique/trajectory/modifier validation
→ resolve THE BANG / Finisher context
→ produce EventOutcome
→ score/combo/multiplier
→ HYPE/Finisher state update
→ emit semantic presentation notifications
```

## Must not own

- DSP implementation details
- raw touch geometry
- cue graphics
- body/hair animation
- HTTP/downloads
- save files
- profile sync
- reward UI

## Test expectations

For domain changes, add EditMode/unit tests where possible for:

- fixed-step invariance
- early inversion
- late inversion
- wrong direction
- first/setup bang
- spam producing poor motion rather than arbitrary blocking
- MISS passive continuation
- event-local Motion Quality reset/history
- combo/multiplier rules
- HYPE accumulation
- THE BANG transitions
- Finisher determinism

## Review checklist

Before completion ask:

1. Does this preserve ownership of neck state after mistakes?
2. Could two render frame rates produce materially different authoritative outcomes?
3. Did presentation leak into scoring?
4. Did a tuning constant get hardcoded instead of configured?
5. Did this silently change an existing chart meaning?
6. Are tests proving the intended rule rather than merely current implementation?
