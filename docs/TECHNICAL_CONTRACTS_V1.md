# Technical Contracts v1

## Status

Canonical architectural contract for Headbang Heroes.

The goal is clear ownership without enterprise ceremony:

> Gameplay logic, presentation, content delivery, and persistence must evolve largely independently.

No DI framework, global event bus, or strict Clean Architecture implementation is required. Explicit responsibilities and deterministic gameplay boundaries are required.

See `FOUNDATION.md` for document authority.

---

# 1. Conceptual layers

```text
DOMAIN
- chart semantics
- neck simulation
- timing judgment
- motion quality
- scoring
- HYPE / THE BANG / Finisher
- authored Rest evaluation

APPLICATION
- GameplayRun orchestration
- RunResult assembly
- Results
- Progression

INFRASTRUCTURE
- content provider/catalog/cache
- audio asset loading
- save persistence
- future profile sync

PRESENTATION
- cues
- avatar/body/hair
- venue/background
- UI
- haptics
```

The Domain must not depend on UI, scene hierarchy, HTTP, save files, artwork, haptics, or render frame timing.

---

# 2. Canonical runtime flow

```text
ContentCatalog
   ↓
SongLoader
   ↓
RuntimeChart + Audio
   ↓
GameplayRun
   ├── AudioClock
   ├── ChartRuntime
   ├── InputRouter
   ├── NeckMotionModel
   ├── JudgmentSystem
   ├── MotionQualityEvaluator
   ├── RestEvaluator
   ├── ScoringSystem
   └── HypeSystem
          ↓
   RunResultBuilder
          ↓
   ResultsService
          ↓
   ProgressionService
          ↓
   SaveService
```

Presentation observes semantic state/events and reacts downstream.

---

# 3. AudioClock owns time

Hard rule:

> **AudioClock owns authoritative song time.**

Use Unity DSP/audio timing as the source of truth.

Conceptually:

```text
songTime = dspTime - songStartDspTime + songStartOffset + calibration
```

Scheduled playback should be used so start timing is not bound to an arbitrary `Update()` frame.

Judgment must never use as authority:
- `Time.time`
- animation completion
- coroutine completion
- frame count
- visual cue completion

Responsibilities:
- scheduled start
- authoritative song time
- pause/resume/restart re-anchoring
- calibration application
- future Practice seek support

---

# 4. One canonical time domain for inputs

Raw device/input timestamps must be converted as early as practical into authoritative `songTime`.

`BangInput` entering gameplay-domain resolution should conceptually contain:

```text
BangInput
- semantic direction/action
- songTime
- optional source/device metadata for diagnostics
```

Do not compare an Input System timestamp directly to a chart timestamp unless both have first been mapped into the same authoritative time domain.

---

# 5. Authoring data vs RuntimeChart

Preferred pipeline:

```text
SongDefinition + ChartDefinition
        ↓
validation
        ↓
compilation
        ↓
RuntimeChart
```

`RuntimeChart` should be:
- immutable during a run
- ordered
- prevalidated
- cheap to query
- resolved to deterministic event timing
- tagged with song/chart/rules versions

Do not repeatedly parse JSON/MIDI/beat fractions/tempo maps in the gameplay hot path.

---

# 6. ContentCatalog / ContentProvider

`ContentCatalog` owns playable-content discovery and local availability.

Recommended provider boundary:

```text
ContentCatalog
   ↓
IContentProvider
   ├── LocalContentProvider
   ├── AddressablesContentProvider
   └── RemoteCdnContentProvider
```

Responsibilities:
- local manifest/cache
- remote manifest refresh
- version comparison
- hashes/checksums
- download state
- offline availability
- client/rules compatibility

Rule:

> **Content can change remotely. Runtime rules cannot.**

Remote content may add known songs/charts/art/venue assets. Unsupported semantics require a client update.

---

# 7. SongLoader

Input:

```text
songId
chartId
```

Output:

```text
LoadedSong
- SongDefinition
- RuntimeChart
- audio reference
- presentation/venue references
```

SongLoader prepares validated run-ready inputs. It does not judge, score, grant progression, or mutate profile state.

---

# 8. ChartRuntime owns authored chronology

Responsibilities:
- ordered unresolved/resolved events
- candidate lookup around song-time
- sections/phrases
- chart/version identity
- efficient time-range queries

It does not judge input, move the neck, or render cues.

---

# 9. Dense-event candidate matching

Do not rely on one mutable `activeEvent` forever.

For an input at authoritative song-time, search a bounded set of unresolved candidate events around that time.

Candidate selection should consider:
- timing proximity
- whether the event is eligible
- direction/action compatibility
- technique/trajectory context

Desired semantics:
- input too early for all candidates: physical neck response still occurs, no event consumed
- compatible input inside a window: best candidate is resolved
- wrong semantic input inside the best eligible window: matched event is consumed as MISS unless a technique explicitly defines otherwise
- expired unresolved event: resolves MISS according to scheduler/runtime rules

This is required for dense charts and prevents “next array item” behavior from becoming gameplay law.

---

# 10. CueScheduler

```text
ChartRuntime + AudioClock
        ↓
CueScheduler
        ↓
CURRENT / NEXT presentation state
```

Hard rule:

> **The cue visualizes time. It does not define time.**

Changing cue animation/easing/rendering must never move authored judgment timing.

---

# 11. InputRouter

Maps physical touch/gesture interaction to semantic intent.

It owns ergonomics/mapping, not correctness.

Hard rule:

> **InputRouter describes player intent; it does not judge it.**

Touch zones are not chart data.

This allows four wedges, lower thumb zones, contextual mappings, and future accessibility mappings without changing chart semantics.

---

# 12. NeckMotionModel owns physical neck state

Authoritative gameplay state includes as needed:
- displacement/angle
- velocity
- acceleration/change of momentum
- inversion state
- damping/recovery
- physical limits
- trajectory components
- event-local motion history

Every valid semantic BangInput affects neck state, including early/late/wrong/no-event input.

A MISS never freezes/snaps/resets motion.

The model does not own score, XP, results, avatar art, or UI.

---

# 13. Gameplay-critical simulation step

The final scoring simulation must not depend on render frame pacing.

The current M0 `Time.deltaTime` integrator is prototype behavior only.

Production direction:

```text
AUTHORITATIVE FIXED SIMULATION
(e.g. target 120 Hz, to validate)
        ↓
NeckMotionState
        ↓
RENDER INTERPOLATION
```

An analytically time-evaluated deterministic solution is also acceptable if it proves cleaner.

Requirements:
- identical input/chart/config should produce materially equivalent gameplay state across render frame rates
- rendering may run independently
- dropped render frames must not silently change scoring physics

---

# 14. Motion history is event-local

Do not use run-global maxima such as a `peakAmplitude` that only resets at song restart as authoritative Motion Quality evidence.

MotionQualityEvaluator should consume a bounded recent history or per-gesture/per-event accumulator.

Useful signals may include:
- amplitude developed since relevant preparation/inversion
- incoming velocity/energy
- direction coherence
- inversion quality
- continuity from preceding movement

History reset/rollover semantics must be explicit and testable.

---

# 15. First Bang / setup state

The first bang from neutral has no preceding travel to judge normally.

Canonical state:

```text
UNPREPARED / SETUP
→ first physical launch
→ subsequent inversion can become first complete Bang
```

The setup input may still receive timing feedback where useful, but normal Motion Quality derived from preceding travel is not required.

Exact score treatment remains configurable; the system must not fabricate normal incoming-motion quality from nothing.

---

# 16. Event resolution order

Canonical MotionEvent resolution:

```text
1. physical input arrives
2. InputRouter maps semantic BangInput
3. convert to authoritative song-time
4. snapshot PRE-INVERSION NeckMotionState/history
5. apply physical neck input immediately
6. find eligible unresolved authored candidate
7. resolve TimingJudgment
8. evaluate MotionQuality from PRE-INVERSION snapshot/history
9. validate technique / trajectory / modifier semantics
10. read current THE BANG / Finisher context
11. build authoritative EventOutcome
12. ScoringSystem applies score / combo / multiplier
13. HypeSystem updates HYPE / THE BANG / Finisher state
14. semantic presentation events are emitted
```

Steps 4→5 are deliberate: judge the movement arriving into the inversion while preserving immediate physical ownership of the neck.

Presentation cannot reorder or alter this outcome.

---

# 17. JudgmentSystem

Answers:

> When did the player act relative to the matched authored event?

Output:

```text
TimingJudgment
- PERFECT | GREAT | GOOD | MISS
- signedTimingError
- matchedEventId
```

Timing windows are configuration.

Judgment does not own Motion Quality.

---

# 18. MotionQualityEvaluator

Answers:

> How well did the neck physically arrive/perform for this authored action?

Input:
- matched event
- pre-inversion NeckMotionState
- bounded motion history

Output:
- normalized MotionQuality
- optional diagnostic components

Timing Quality and Motion Quality remain separate signals.

Difficulty never applies a hidden Motion Quality penalty.

---

# 19. RestEvaluator

Natural Rest requires no event/evaluator.

Authored `RestEvent` is interval-based.

Preferred contract:

```text
RestEvent
- start/end or beat + duration
- settling portion/profile
- stillness evaluation portion/profile
- stillness thresholds
```

Runtime semantics:

```text
REST START
→ settling phase: player may bleed existing momentum
→ evaluation phase: residual movement is measured
→ RestOutcome
```

The evaluator must never snap the neck to neutral.

Candidate stillness signals may include velocity, displacement, energy, and stability over the evaluation interval.

Exact thresholds and score/combo consequences are tuning data.

---

# 20. ScoringSystem

Owns:
- event score
- total score
- combo
- longest combo
- completed combo metric
- multiplier/progress
- judgment counts

Inputs include authoritative EventOutcome, current multiplier state, and THE BANG scoring context.

Hard rule:

> **Scoring owns score/combo/multiplier.**

UI never calculates score.

---

# 21. HypeSystem

Owns:
- HYPE amount
- READY
- THE BANG activation/window
- Finisher eligibility/resolution

Baseline HYPE:

```text
PERFECT +2
GREAT   +1
GOOD     0
MISS     0
```

MISS does not remove accumulated HYPE.

Canonical terminology:
- THE BANG = state
- Finisher = payoff

HYPE refill behavior during THE BANG must be independently configurable to prevent accidental infinite loops.

---

# 22. GameplayRun

Application-level orchestrator for one attempt.

Responsibilities:
- start/end/pause/resume/abort/retry lifecycle
- connect already-defined services
- retain run identity/state references
- finalize RunResult

It must not absorb domain formulas, JSON parsing, HTTP, avatar animation, or save serialization.

---

# 23. Presentation contract

Hard rule:

> **Presentation reads gameplay state; presentation never determines gameplay state.**

Semantic events may include:
- JudgmentOccurred
- ComboChanged
- HypeChanged
- HypeReady
- TheBangStarted
- FinisherTriggered
- RunCompleted

Prefer direct typed calls/interfaces for deterministic core flow and semantic events for downstream UI/presentation.

Avoid an opaque universal event bus.

---

# 24. Body / Hair / Venue

`BodyResponseController` consumes NeckMotionState + PerformanceStyle + presentation intensity.

`HairResponseController` consumes NeckMotionState + inversion/energy + HairProfile + presentation intensity.

`VenueResponseController` consumes semantic presentation signals such as HYPE/THE BANG/Finisher.

All are downstream only.

No body/hair/venue animation state feeds scoring or neck physics.

---

# 25. HapticsService

No default gameplay SFX are layered over the song.

Haptics may react to semantic events such as Perfect, Miss, HypeReady, TheBangActivated, Finisher.

Platform vibration APIs stay behind `HapticsService` and obey accessibility/settings.

---

# 26. RunResultBuilder / ResultsService

One authoritative `RunResult` is finalized at end of run.

Contains at minimum:
- songId
- chartId/version
- rulesVersion
- score
- final grade
- judgment counts
- longest/completed combo metrics
- total HYPE earned
- THE BANG activations
- Finishers executed
- result tags

Hard rule:

> **Results renders RunResult; it does not rebuild it.**

ResultsService may select commentary/presentation data but does not recalculate score.

---

# 27. ProgressionService

Consumes:

```text
RunResult + UserProfile
```

Produces:

```text
ProgressionOutcome
- XP
- HH
- level change
- unlocks
- milestones
- record updates
```

Progression does not participate in active gameplay.

---

# 28. SaveService / ProfileSyncService

SaveService owns local versioned persistence and migration.

Gameplay systems do not write files directly.

Future online sync:

```text
SaveService ↔ ProfileSyncService ↔ Backend
```

Conflict/authority is domain-specific:
- validated best records: keep best compatible record
- legitimate unlock sets: union where appropriate
- settings/loadout: latest valid choice
- HH/paid entitlements/economy: server-authoritative ledger once backend economy exists

Do not treat currency as a last-write-wins field.

---

# 29. Configuration

Tunable values belong in configuration rather than scattered code constants.

Examples:
- TimingConfig
- ScoringConfig
- HypeConfig
- NeckPhysicsConfig
- MotionQualityConfig
- RestEvaluationConfig
- DifficultyConfig
- EconomyConfig
- PerformanceStyleConfig
- HairProfile
- VenueProfile

ScriptableObjects are suitable where Unity editor workflow adds value, but are not the default mutable runtime state and are not authoritative save files.

---

# 30. Testing contract

EditMode/pure-domain tests should cover:
- chart validation/compilation
- timing judgment boundaries
- candidate matching
- wrong-direction consumption
- setup-first-bang semantics
- neck integrator behavior at authoritative step
- Motion Quality history/reset semantics
- Rest settling/evaluation
- scoring/combo/multiplier
- HYPE/THE BANG/Finisher state
- grade/progression calculations
- save migrations

PlayMode/device tests should cover:
- DSP start alignment
- pause/resume/retry sync
- Input System integration
- cue scheduling
- real-device calibration/latency
- haptics
- background/foreground readability
- content download/cache/offline behavior

---

# 31. Prototype status

Current M0 classes such as the existing `PrototypeController`, `HeadMotionModel`, single-active-event scheduler behavior, hardcoded timing constants, and render-frame integration are experimental proof-of-concept code.

They may demonstrate valid design ideas, but they do not outrank this contract.

Refactor production code toward these ownership boundaries rather than extending the prototype indefinitely.

Canonical naming going forward:
- gameplay simulation concept: `NeckMotionModel`
- visual head transform: presentation concern

---

# 32. Anti-spaghetti rules

- AudioClock owns time.
- RuntimeChart/ChartRuntime owns authored chronology.
- InputRouter maps intent, not correctness.
- NeckMotionModel owns physical neck state.
- JudgmentSystem owns timing quality.
- MotionQualityEvaluator owns movement quality.
- RestEvaluator owns authored stillness resolution.
- ScoringSystem owns score/combo/multiplier.
- HypeSystem owns HYPE/THE BANG/Finisher state.
- Presentation never determines gameplay outcomes.
- Progression consumes RunResult.
- SaveService persists; gameplay never writes files.
- No class becomes a generic dumping ground because responsibility is unclear.
