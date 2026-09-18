# Headbang Heroes — Foundation

## Purpose

This document defines the canonical product and engineering foundation for Headbang Heroes.

It exists to prevent contradictory design notes, prototype behavior, or outdated roadmap assumptions from becoming accidental specification.

If two documents disagree, use the authority order below.

## Document authority

1. Explicit subsystem `*_V1.md` specifications
2. This `FOUNDATION.md`
3. `GDD.md` high-level product design
4. `ROADMAP.md`
5. Lab/prototype notes such as `LAB_TRACK_001.md`
6. Current prototype implementation

Prototype code proves hypotheses; it does not silently redefine design.

## Product definition

Headbang Heroes is a portrait-first 2D mobile rhythm game in which the player's instrument is the avatar's head and neck.

The player does not tap notes in a detached lane. The player controls a continuous neck simulation, reads authored timing/movement cues, and works with momentum to perform headbang choreography in sync with metal music.

North star:

> Easy to understand, difficult to perfect.

POC mantra:

> Make The Headbang Fun.

## Non-negotiable gameplay rules

- Audio time is authoritative.
- Player input changes neck motion; it never teleports the head between authored poses.
- Every valid semantic physical input affects neck state, even if early, late, wrong, or outside a scoring event.
- A MISS never freezes, snaps, or resets the neck.
- Timing Quality and Motion Quality are separate signals.
- Motion Quality is evaluated from physical neck state/history, not from presentation animation.
- Body, hair, venue, VFX, UI, and haptics are presentation-only and never determine scoring.
- Difficulty changes authored choreography, not neck physics.
- Progression unlocks expression/content, not gameplay power.
- Results consume authoritative run data and never recalculate scoring.

## Canonical neck vocabulary

Technique:
- Classic
- Half
- Deep
- Whiplash
- Windmill

Trajectory:
- Horizontal
- Vertical
- Circular
- CenterEdge

Modifier:
- None
- Double
- Hold
- Accent
- Burst

Rest is represented as its own authored `RestEvent`, not a MotionEvent technique.

Extension rule:

> Adding a new neck mechanic must not change the semantics of existing mechanics.

## Classic Bang contract

A tap identifies the inversion point, not the destination.

- LEFT -> invert/commit LEFT -> launch RIGHT
- RIGHT -> invert/commit RIGHT -> launch LEFT
- UP -> invert/commit UP -> launch DOWN
- DOWN -> invert/commit DOWN -> launch UP

Inversion can happen anywhere in travel. Early inversion produces weak travel; prepared inversion produces stronger motion; late inversion travels farther toward limits. Anti-spam should emerge from poor physical travel/momentum rather than arbitrary cooldowns.

The first action from neutral is a setup action. It can be rhythmically valid, but there is no preceding travel to judge with normal Motion Quality semantics.

## Musical abstraction rule

Charts represent a performable headbang interpretation of the music, not a transcription of every musical subdivision.

> The chart follows the performable headbang rhythm, not every subdivision present in the source.

A drum passage in 1/32 does not imply head input at 1/32. Depending on the phrase, the performer may headbang at 1/8 or 1/16, use Half/Burst semantics, sustain Windmill continuity, or accent only musically important pulses.

Authoring should conceptually distinguish:

```text
MUSIC LAYER       what happens in the song
PERFORMANCE LAYER what a human neck should perform
CHART LAYER       how HH encodes that performance
```

## Difficulty contract

Difficulty is a property of an authored chart.

A song may have one chart or several. The catalog may carry the difficulty curve.

Valid examples:
- Song A -> Normal only
- Song B -> Hard only
- Song C -> Extreme only
- Song D -> Normal + Hard

No system may assume every song has four difficulty variants.

## THE BANG terminology

Canonical terminology:

- `HYPE` = accumulated performance-energy resource
- `THE BANG` = manually activated temporary enhanced state
- `Finisher` = deterministic payoff resolved on a suitable authored/performed event while THE BANG is active

`Special Bang` is legacy terminology and should not be used in canonical docs/code going forward.

## Authored Rest contract

Natural Rest = no required event.

Authored Rest = explicit stillness requirement that never snaps/reset motion.

Preferred model:

```text
REST START
  -> settling phase: player may bleed existing momentum
  -> stillness evaluation phase: residual movement is measured
  -> Rest outcome
```

The exact thresholds/durations are tuning data. The important fairness rule is that the player must be given a musically reasonable opportunity to dissipate momentum created by preceding required actions.

## Runtime event resolution order

Canonical MotionEvent resolution:

```text
1. physical input arrives
2. InputRouter maps it to semantic BangInput
3. convert timestamp to authoritative song-time
4. snapshot pre-inversion NeckMotionState/history
5. apply physical input to neck immediately
6. find eligible unresolved authored candidate
7. resolve timing judgment
8. evaluate Motion Quality from the pre-inversion snapshot/history
9. validate technique / trajectory / modifier semantics
10. resolve current THE BANG / Finisher context
11. build authoritative EventOutcome
12. apply score / combo / multiplier
13. update HYPE / THE BANG / Finisher state
14. emit semantic presentation events
```

The ordering of steps 4 and 5 is intentional: the run judges the movement arriving into the inversion while still preserving immediate player ownership of the neck.

## Dense-event matching

Runtime matching must not rely forever on one mutable `activeEvent` assumption.

For dense charts, the runtime should search a bounded set of unresolved candidate events around input song-time and choose the best compatible candidate using timing proximity plus semantic compatibility.

Wrong semantic action inside an eligible window is treated as a consumed MISS unless a specific technique defines another contract. Too-early input may still move the neck without consuming the future event.

## Motion simulation determinism

Gameplay-critical neck simulation must not depend on render frame pacing.

Production direction:
- fixed authoritative simulation step (target to tune, e.g. 120 Hz), or an analytically time-evaluated equivalent
- rendering may interpolate independently
- Motion Quality uses event-local history, not run-global maxima

The current M0 `Time.deltaTime` integrator is prototype behavior and must not be treated as the final scoring simulation contract.

## Content and server direction

Official content is server-first with local cache.

Remote content may add songs, charts, artwork, and venue/background assets using semantics already supported by the installed client.

> Content can change remotely. Runtime rules cannot.

Cached valid content remains playable offline.

MVP trust model: HTTPS + version/hash validation. Production publication should additionally use a trusted/signed release-manifest strategy or equivalent authenticity control.

## Save/profile direction

The user profile is user metadata:
- identity
- progression
- HH
- avatar selections
- unlocks
- records
- settings/accessibility
- calibration
- schema/version metadata

Local save is immediate/offline-capable. Future online authority is domain-specific:
- records can merge using validated best-result rules
- unlock sets may union where legitimate
- settings/loadout may use latest valid user choice
- currency/entitlements require server-authoritative ledger/transactions once production economy exists

## Modes

Current planned modes:
1. Story
2. Quick Headbang
3. Versus
4. Practice
5. Endurance

Story and Versus are the strongest long-term identity modes. POC/MVP do not require all modes.

## Milestones

POC:
- one complete song
- proves the mechanic and full loop

MVP:
- four complete real songs
- proves a small complete game

POC proves the mechanic. MVP proves the game.

## Explicit anti-spaghetti rules

- AudioClock owns time.
- ChartRuntime owns authored chronology.
- InputRouter describes intent; it does not judge.
- NeckMotionModel owns physical neck state.
- Judgment owns timing quality.
- MotionQualityEvaluator owns movement quality.
- Scoring owns score/combo/multiplier.
- HypeSystem owns HYPE/THE BANG/Finisher state.
- Presentation never determines gameplay state.
- Progression consumes RunResult; it does not participate in gameplay.
- SaveService persists profile state; gameplay systems do not write files.
- No generic God Object may become the place where unrelated responsibilities accumulate.
