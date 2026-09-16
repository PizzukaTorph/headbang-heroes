# Headbang Heroes — Agent Rules

## Read this first

Before changing gameplay architecture or design, read:

1. `docs/FOUNDATION.md`
2. the relevant canonical `*_V1.md` subsystem specification
3. `docs/GDD.md` for high-level product context

If documents disagree, follow the authority order defined in `docs/FOUNDATION.md`.

Current prototype code is experimental evidence, not automatic specification.

## Specialist agent routing

Specialist briefs live in `agents/`.

Use the smallest relevant specialist set for a task. Cross-cutting changes should include the Architecture Guardian and QA Guardian.

- `agents/architecture-guardian.md` — ownership boundaries, dependency direction, naming, cross-cutting refactors.
- `agents/gameplay-core.md` — NeckMotionModel, Motion Quality, event resolution, scoring, HYPE/THE BANG/Finisher, GameplayRun boundaries.
- `agents/rhythm-audio.md` — AudioClock, DSP/song-time, scheduling, pause/resume/retry sync, calibration and timing diagnostics.
- `agents/chart-content.md` — song/chart schemas, RuntimeChart compilation, candidate matching, Rest semantics, validation, packaging and remote-content compatibility.
- `agents/presentation-avatar.md` — CURRENT/NEXT, avatar/body/hair/venue, haptics and presentation-only feedback.
- `agents/meta-profile.md` — Results integration, progression, save/profile, migrations, rewards/economy metadata and future sync boundaries.
- `agents/qa-guardian.md` — adversarial review for determinism, timing, chart semantics, device edge cases, persistence safety and documentation drift.

Routing examples:

- Refactor `AudioClock` + judgment timing → Rhythm Audio + Gameplay Core + QA Guardian.
- Replace `HeadMotionModel` → Gameplay Core + Architecture Guardian + QA Guardian.
- Implement THE BANG → Gameplay Core + Presentation Avatar + QA Guardian.
- Add/change chart semantics → Chart Content + Gameplay Core + Rhythm Audio as relevant + Architecture Guardian.
- Build remote song delivery → Chart Content + Architecture Guardian + QA Guardian.
- Save/profile migration → Meta Profile + QA Guardian.

Specialists do not override canonical docs. If a brief and a canonical spec disagree, the canonical spec wins and the brief must be updated.

## Product principle

Headbang Heroes is a rhythm game about **headbanging**, not a generic note-tapping game with a metal skin.

The player controls a continuous neck simulation. Timing, momentum, direction, technique, and movement quality matter.

## Current priority

P0 goal:

> **Make The Headbang Fun.**

Do not hide weak core interaction behind accounts, monetization, story, multiplayer, progression complexity, procedural systems, or large art pipelines.

## Canonical gameplay constraints

- Audio/DSP time is authoritative.
- Input always affects neck motion, even when early/late/wrong/no-event.
- A MISS never freezes/snaps/resets the neck.
- Timing Quality and Motion Quality are separate.
- Motion Quality uses physical neck state/history, not avatar animation.
- The first bang from neutral is setup/unprepared, not a normal completed-motion sample.
- Body, hair, venue, UI, and haptics are downstream presentation only.
- Difficulty changes authored choreography, not neck physics.
- Charts represent performable headbang rhythm, not every musical subdivision.
- THE BANG is the enhanced state; Finisher is the payoff.
- Progression unlocks expression/content, not power.

## Engineering principles

- Prefer small, testable systems over large frameworks.
- Keep song/chart data deterministic, versioned, and data-driven.
- Convert input to the authoritative song-time domain before judgment.
- Separate authoring data from immutable/prevalidated runtime chart data.
- Dense charts must use bounded candidate matching rather than assuming one active event forever.
- Gameplay-critical neck simulation must not depend on render frame pacing.
- Prefer an authoritative fixed simulation step or deterministic time-evaluated equivalent.
- Motion Quality history must be event/gesture-local, not run-global maxima.
- Separate domain outcomes from presentation events.
- Mobile performance and touch latency matter from day one.
- Build calibration/debug tooling early.
- Avoid third-party dependencies unless they materially reduce current risk.
- Never commit licensed audio, fonts, art, or other third-party assets without provenance/license records.
- No copyrighted commercial music may be added merely for development convenience if redistribution is not permitted.

## Primary module ownership

Canonical concepts include:

- AudioClock
- RuntimeChart / ChartRuntime
- CueScheduler
- InputRouter
- NeckMotionModel
- JudgmentSystem
- MotionQualityEvaluator
- RestEvaluator
- ScoringSystem
- HypeSystem
- GameplayRun
- RunResultBuilder
- ResultsService
- ProgressionService
- SaveService
- ContentCatalog / ContentProvider
- presentation controllers (Body/Hair/Venue/Haptics/UI)

Do not create a God `GameManager` that absorbs these responsibilities.

## Prototype-code rule

Existing M0 code may be refactored or replaced when it conflicts with canonical contracts.

In particular, do not preserve merely for compatibility:
- render-frame (`Time.deltaTime`) gameplay-critical simulation
- run-global Motion Quality peaks
- permanent single-active-event assumptions
- hardcoded tuning constants
- legacy naming such as `Special Bang`

## Definition of done for POC

A player can complete one song, understand CURRENT/NEXT cues, intentionally improve timing and movement quality, experience meaningful momentum, build HYPE, activate THE BANG, resolve a Finisher, receive coherent Results, and immediately want to retry because the **headbang itself** is fun.
