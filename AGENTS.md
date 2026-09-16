# Headbang Heroes — Agent Rules

## Read this first

Before changing gameplay architecture or design, read:

1. `docs/FOUNDATION.md`
2. the relevant canonical `*_V1.md` subsystem specification
3. `docs/GDD.md` for high-level product context

If documents disagree, follow the authority order defined in `docs/FOUNDATION.md`.

Current prototype code is experimental evidence, not automatic specification.

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
