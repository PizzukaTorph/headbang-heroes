# Technical Contracts v1

## Status

This document defines the architectural contracts for Headbang Heroes.

It translates the game design into technical ownership boundaries without introducing unnecessary enterprise complexity.

The goal is simple:

> Gameplay logic, presentation, content delivery and persistence must be able to evolve largely independently.

This document does **not** require a dependency-injection framework, a global event bus, dozens of assemblies, or a strict Clean Architecture implementation.

It does require clear ownership, one-way dependencies where practical, and explicit boundaries between deterministic gameplay state and presentation/infrastructure.

---

# 1. Architectural shape

Headbang Heroes is divided conceptually into four layers:

```text
DOMAIN
- chart semantics
- neck simulation
- timing judgment
- motion quality
- scoring
- HYPE / THE BANG / Finisher rules

APPLICATION
- gameplay run orchestration
- results assembly
- progression application

INFRASTRUCTURE
- remote/local content
- cache
- audio asset loading
- save persistence
- profile sync

PRESENTATION
- cues
- avatar/body/hair
- venue/background
- UI
- haptics
```

Preferred dependency direction:

```text
Presentation
     ↓
Application
     ↓
Domain

Infrastructure → Application
```

The Domain must not depend on:

- Unity UI
- remote servers
- HTTP
- save files
- artwork
- haptics
- scene hierarchy
- frame timing

---

# 2. Primary runtime flow

Conceptually:

```text
ContentCatalog
      ↓
SongLoader ──────────────┐
      │                  │
      ▼                  ▼
 ChartRuntime         AudioClock
      │                  │
      └────────┬─────────┘
               ▼
           GameplayRun
               │
         ┌─────┴─────┐
         ▼           ▼
    InputRouter   CueScheduler
         │
         ▼
  NeckMotionModel
         │
   ┌─────┴─────────────┐
   ▼                   ▼
JudgmentSystem   MotionQualityEvaluator
   │                   │
   └─────────┬─────────┘
             ▼
       ScoringSystem
             │
             ▼
        HypeSystem
             │
             ▼
     RunResultBuilder
             │
             ▼
       ResultsService
             │
             ▼
    ProgressionService
             │
             ▼
         SaveService
```

Presentation systems observe semantic state/events and react to them. They do not own authoritative gameplay outcomes.

---

# 3. AudioClock

## Ownership

`AudioClock` is the authoritative gameplay clock.

Hard rule:

> **AudioClock owns time.**

Gameplay judgment must never use `Time.time`, animation completion, coroutine timing, frame counts, or cue completion as the authoritative song position.

## Required implementation direction

Use Unity's DSP/audio clock as the basis of gameplay time.

A run should establish a scheduled start point such as:

```text
songStartDspTime
```

Conceptually:

```text
songTime = dspTime - songStartDspTime + calibrationOffset
```

Unity's scheduled audio playback should be used where appropriate so playback start is not dependent on the frame in which `Update()` happens to execute.

## Responsibilities

- establish precise song start
- expose authoritative song time
- pause/resume/restart semantics
- calibration offsets
- support deterministic cue/judgment scheduling
- future seek support for Practice mode

## Does not own

- score
- chart semantics
- input
- visual cue animation
- progression

---

# 4. Authoring data vs RuntimeChart

Human/editor-facing data and gameplay-runtime data are not required to be identical.

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
- already resolved to deterministic timing information suitable for the AudioClock

Do not repeatedly parse authoring JSON, MIDI semantics, beat fractions or tempo maps inside the hot gameplay loop.

---

# 5. ContentCatalog

`ContentCatalog` owns discovery and availability of playable content.

## Responsibilities

- read local content manifest/cache
- fetch remote manifest when available
- compare package/chart/art/audio versions
- validate hashes/checksums where applicable
- expose download/availability state
- ensure required content is locally available before a run
- allow offline use of valid cached content

## Does not own

- chart judgment
- gameplay rules
- progression
- Unity scene flow

Remote content may provide new songs, charts, art and backgrounds.

Remote content must never silently introduce gameplay semantics unsupported by the installed client.

Rule:

> **Content can change remotely. Runtime rules cannot.**

---

# 6. ContentProvider boundary

Do not hardwire the game to one hosting technology.

Recommended abstraction:

```text
ContentCatalog
      ↓
IContentProvider
      ├── LocalContentProvider
      ├── AddressablesContentProvider
      └── RemoteCdnContentProvider
```

The exact implementation may evolve.

Unity Addressables are compatible with this direction because remote catalogs/assets can be hosted outside the application build. Unity CCD is optional, not architectural destiny.

Headbang Heroes should be able to move between custom CDN/object storage, Addressables hosting, or another provider without rewriting gameplay systems.

---

# 7. SongLoader

`SongLoader` prepares content for a run.

## Input

```text
songId
chartId
```

## Output

Conceptually:

```text
LoadedSong
- SongDefinition
- RuntimeChart
- audio reference
- venue/background definition
```

## Responsibilities

- resolve package references
- load validated metadata
- resolve audio and presentation assets
- verify chart/client compatibility
- produce run-ready immutable inputs

## Does not own

- score
- timing judgment
- profile data
- HYPE

---

# 8. ChartRuntime

`ChartRuntime` owns authored gameplay chronology for the active run.

Hard rule:

> **ChartRuntime owns authored gameplay chronology.**

## Responsibilities

- ordered runtime events
- next/current/upcoming event lookup
- section/phrase lookup
- chart IDs and version metadata
- efficient time-range queries

It does not judge player input.

It does not move the neck.

It does not render cues.

---

# 9. CueScheduler

`CueScheduler` converts upcoming chart events into presentation timing.

Conceptually:

```text
ChartRuntime + AudioClock
           ↓
      CueScheduler
           ↓
       CURRENT/NEXT
```

## Responsibilities

- decide cue spawn/approach timing
- promote NEXT to CURRENT
- expose normalized cue progress
- retire expired cues

## Contract

Cue visuals represent already-authored timing.

Changing closing-circle animation, easing, color or rendering must not change judgment timing.

> **The cue visualizes time. It does not define time.**

---

# 10. InputRouter

`InputRouter` translates physical interaction into semantic gameplay intent.

Conceptually:

```text
screen interaction
      ↓
touch mapping
      ↓
BangInput
```

Example:

```text
BangInput
- direction
- inputTimestamp
- optional device/source metadata
```

Hard rule:

> **InputRouter describes player intent; it does not judge it.**

The physical touch region is not chart data.

This allows the game to change four-wedge mapping, thumb-zone mapping or future accessibility layouts without changing charts or neck semantics.

Unity Input System is the preferred platform input layer.

---

# 11. NeckMotionModel

`NeckMotionModel` owns the authoritative physical neck state.

Hard rule:

> **NeckMotionModel owns physical neck state.**

## Responsibilities

- position/displacement
- velocity
- acceleration/momentum
- inversion
- damping
- travel/limits
- trajectory-relevant state
- energy/amplitude behavior

Every valid semantic BangInput affects the neck, including input that is:

- early
- late
- wrong
- outside a scoring event

This preserves the core gameplay contract:

> **The player misses the beat, not ownership of the neck simulation.**

## Does not own

- PERFECT/GREAT/GOOD/MISS
- score
- XP
- results
- avatar art

Prefer a deterministic/custom movement model over gameplay-critical Rigidbody2D behavior.

---

# 12. JudgmentSystem

`JudgmentSystem` answers one question:

> When did the player act relative to the authored event?

Input:

```text
BangInput
+ candidate ChartEvent
+ AudioClock
```

Output concept:

```text
TimingJudgment
- PERFECT | GREAT | GOOD | MISS
- signedTimingError
- matchedEventId
```

Negative error = early.
Positive error = late.

Hard rule:

> **Judgment owns timing quality.**

It does not own motion quality.

---

# 13. MotionQualityEvaluator

`MotionQualityEvaluator` evaluates how well the requested movement was physically executed.

Input:

```text
ChartEvent
+ NeckMotionState/history
```

Output:

```text
MotionQuality
```

Potential factors:

- useful amplitude
- preparation
- inversion quality
- velocity/energy
- direction consistency
- technique-specific requirements

Hard rule:

> **Timing Quality and Motion Quality are separate signals.**

A PERFECT timing input can still have weak motion quality.

A GREAT can have excellent movement quality.

Difficulty must not artificially change the neck simulation itself; higher motion demand should emerge from harder authored choreography.

---

# 14. ScoringSystem

`ScoringSystem` owns score, combo and multiplier state for the current run.

Inputs include:

- TimingJudgment
- MotionQuality
- technique/modifier data
- current multiplier
- THE BANG scoring modifier where configured

Outputs include:

- event score
- total score
- combo changes
- longest combo
- completed combos
- multiplier state
- PERFECT/GREAT/GOOD/MISS counts

Hard rule:

> **Scoring owns score/combo/multiplier.**

UI never calculates score.

Presentation never changes score.

---

# 15. HypeSystem

`HypeSystem` owns:

- HYPE amount
- READY state
- THE BANG activation
- THE BANG active window
- Finisher eligibility/resolution state

Baseline HYPE event contribution remains:

```text
PERFECT = +2
GREAT   = +1
GOOD    = +0
MISS    = +0
```

MISS does not remove accumulated HYPE.

THE BANG may apply configured scoring/presentation amplification.

HYPE generation during THE BANG must be separately configured so a self-sustaining activation loop cannot occur accidentally.

Finisher candidate markers remain chart metadata; Finisher resolution remains deterministic.

Hard rule:

> **HypeSystem owns HYPE/THE BANG/Finisher state.**

---

# 16. GameplayRun

`GameplayRun` is the application-level orchestrator for one song attempt.

It is not a God Object.

## Responsibilities

- coordinate run start/end
- connect already-defined services
- pause/resume/abort/retry lifecycle
- own run-level state references
- trigger result finalization

It delegates domain work to domain systems.

Avoid putting scoring formulas, chart parsing, save serialization, avatar animation or HTTP code inside `GameplayRun`.

---

# 17. Presentation contract

Hard rule:

> **Presentation reads gameplay state; presentation never determines gameplay state.**

Presentation systems may subscribe to semantic events/state such as:

- judgment occurred
- combo changed
- HYPE changed
- HYPE ready
- THE BANG started
- Finisher triggered
- run completed

They may not feed authoritative visual state back into score/timing/neck simulation.

---

# 18. BodyResponseController

Input:

```text
NeckMotionState
+ PerformanceStyle
+ HYPE presentation state
```

Output:

- shoulder response
- torso follow-through
- arm response
- optional BodyRoot response

One-way dependency:

```text
Gameplay → Neck → Body
```

Never:

```text
Body animation → gameplay result
```

---

# 19. HairResponseController

Input:

```text
NeckMotionState
+ inversion impulses
+ HYPE presentation state
+ HairProfile
```

Output:

- hair segment transforms
- delayed follow-through
- overshoot/recovery

Hair is presentation-only.

Hair physics, clipping or animation failure must not alter judgment or score.

---

# 20. VenueResponseController

Input semantic presentation signals:

```text
PerformanceIntensity
HYPE
THE BANG
Finisher
```

Possible outputs:

- parallax strength
- crowd response
- lights
- haze
- camera impulse

Venue/background presentation is explicitly downstream of gameplay.

---

# 21. HapticsService

Headbang Heroes does not add gameplay sound effects over the music by default.

Gameplay feedback may use visual response and haptics.

`HapticsService` centralizes platform-specific vibration behavior.

Semantic inputs may include:

- Perfect
- Miss
- HypeReady
- TheBangActivated
- Finisher

Settings control whether haptics are enabled and, where supported, their intensity/profile.

No gameplay system should call low-level device vibration APIs directly.

---

# 22. RunResultBuilder

At the end of a run, one authoritative object is built:

```text
RunResult
```

It contains the finalized values required by Results and Progression, including:

- score
- final grade
- PERFECT/GREAT/GOOD/MISS counts
- longest combo
- completed combo count
- total HYPE earned
- Finishers executed
- THE BANG activations
- derived result tags
- song/chart/version identity

Hard rule:

> **Results renders RunResult; it does not rebuild it.**

---

# 23. ResultsService

`ResultsService` consumes an authoritative `RunResult` and produces presentation-ready result information.

Responsibilities may include:

- contextual result comment selection
- grade presentation data
- stat ordering
- reward summary integration

It does not recalculate score.

---

# 24. ProgressionService

`ProgressionService` consumes:

```text
RunResult
+ current UserProfile
```

and produces:

```text
ProgressionOutcome
- XP earned
- HH earned
- level change
- unlocks
- milestone rewards
- record updates
```

Hard rule:

> **Progression consumes RunResult; it does not participate in gameplay.**

The gameplay scene must not grant currency directly.

---

# 25. SaveService

`SaveService` persists user metadata and progression state.

Conceptual ownership:

```text
UserProfile
- identity
- level / XP / HH
- avatar loadout
- owned/unlocked cosmetics
- unlocked songs/content
- best records
- settings
- accessibility
- calibration
- save schema version
```

Responsibilities:

- local persistence
- schema versioning
- migration
- atomic/defensive write behavior
- corruption fallback strategy
- future sync hooks

Hard rule:

> **Gameplay systems do not write files.**

For early versions, a simple versioned JSON representation is acceptable provided the save contract is not coupled to one serializer implementation.

Do not use ScriptableObjects as the authoritative persistent user save.

---

# 26. ProfileSyncService

Future online synchronization belongs behind a separate boundary:

```text
SaveService
    ↕
ProfileSyncService
    ↕
backend
```

The game should remain usable offline with valid local data/content.

Conflict policy may differ by data class:

- best records → preserve better valid record
- unlock sets → union where legitimate
- settings/loadout → latest valid update
- server-governed economy → server-authoritative when online economy exists

Exact backend policy can evolve without changing gameplay domain code.

---

# 27. ScriptableObject policy

ScriptableObjects are encouraged for static/configuration data and reusable profiles, for example:

```text
TimingConfig
ScoringConfig
HypeConfig
NeckPhysicsConfig
DifficultyConfig
EconomyConfig
PerformanceStyleConfig
HairProfile
VenueProfile
```

They are not the default answer for mutable runtime state.

They are not the authoritative user save.

They are not required for every architecture boundary.

Use them where Unity's inspector/data workflow provides actual value.

---

# 28. Event policy

Events are useful primarily for downstream presentation and cross-system notifications.

Examples:

```text
JudgmentOccurred
ComboChanged
HypeChanged
HypeReady
TheBangStarted
FinisherTriggered
RunCompleted
```

Do not build an opaque universal event bus where core domain flow becomes impossible to trace.

Preferred rule:

- direct typed calls/interfaces for core deterministic gameplay flow
- semantic events for UI/presentation/reactive systems

ScriptableObject Event Channels may be used selectively where they improve Unity-side decoupling, but are not a mandatory global pattern.

---

# 29. Configuration policy

Tuneable values belong in configuration, not scattered constants.

Examples:

```text
TimingConfig
ScoringConfig
HypeConfig
NeckPhysicsConfig
DifficultyConfig
EconomyConfig
BodyStyleConfig
HairProfile
```

Avoid gameplay formulas hidden across MonoBehaviours.

Data-driven does not mean every value must be remote-configurable.

Core gameplay rule changes still require compatible client code/versioning.

---

# 30. Unity project layout target

Recommended organization:

```text
Assets/_HeadbangHeroes/
├── Core/
│   ├── Gameplay/
│   ├── Chart/
│   ├── Scoring/
│   ├── Hype/
│   └── Models/
│
├── Content/
│   ├── Catalog/
│   ├── Loading/
│   └── Validation/
│
├── Presentation/
│   ├── Avatar/
│   ├── Hair/
│   ├── Venue/
│   ├── Cues/
│   └── Haptics/
│
├── UI/
│   ├── Home/
│   ├── SongSelect/
│   ├── Gameplay/
│   ├── Results/
│   └── Avatar/
│
├── Progression/
├── Persistence/
├── Settings/
└── Tests/
```

Do not create dozens of assembly definitions before a concrete need exists.

---

# 31. Testing contract

Use Unity Test Framework with a bias toward testing pure domain logic outside scene-heavy Play Mode where possible.

## Edit Mode candidates

- chart validation/compilation
- timing-window math
- JudgmentSystem
- MotionQuality calculations
- scoring
- combo/multiplier
- HYPE state transitions
- Finisher resolution
- progression calculations
- save migrations

## Play Mode/device candidates

- DSP/audio synchronization
- scheduled playback
- touch mapping
- pause/resume
- restart
- cue synchronization
- scene/service integration
- actual mobile latency

Critical behavior must still be validated on real Android/iOS hardware.

---

# 32. Performance contract

Gameplay logic must be framerate-independent.

Protect the audio/input path from unnecessary work.

Baseline rules:

- avoid avoidable per-frame allocations
- prevalidate/compile charts
- pool repeated cue/feedback objects where useful
- avoid parsing remote/authoring data during active gameplay
- profile on real devices
- do not optimize speculative non-problems

60 FPS remains the baseline presentation target, but scoring/timing correctness must not depend on hitting exactly 60 FPS.

---

# 33. Anti-God-Object rule

A central lifecycle/orchestration object may exist.

A generic dumping-ground manager may not.

Hard rule:

> **No system becomes “the place where we put stuff because we do not know where else to put it.”**

If a proposed responsibility does not clearly belong to a system, define its owner before adding it.

Avoid a `GameManager` that simultaneously owns:

- audio
- charts
- scoring
- saves
- UI
- progression
- HTTP
- avatar animation

---

# 34. Dependency policy

Stay close to vanilla Unity for the core game.

Use Unity standard packages/features where they clearly fit, especially:

- Unity Input System
- Unity audio/DSP scheduling
- Unity Test Framework
- Addressables if/when remote asset delivery becomes necessary

Third-party packages are justified only by a current measured need.

Do not adopt a framework that owns or distorts the headbang/rhythm design.

---

# 35. Research-backed implementation notes

The following Unity platform facts reinforce these contracts:

- DSP/audio timing and scheduled playback are appropriate for precise musical scheduling.
- Unity Input System separates logical actions from physical bindings/devices.
- ScriptableObjects are useful for separating reusable/static data from logic, but should not become the default mutable runtime state container.
- Event-channel patterns are useful for decoupled notifications when applied selectively.
- Addressables support remote content catalogs/assets and do not require Unity CCD specifically.
- Remote asset/content delivery cannot replace client code when new gameplay semantics require code changes.
- Unity Test Framework supports Edit Mode and Play Mode test strategies that match HH's pure-domain versus scene/device split.

These platform features support the architecture; they do not define the game architecture by themselves.

---

# 36. Canonical ownership rules

These rules summarize the technical contract:

> **AudioClock owns time.**

> **ChartRuntime owns authored gameplay chronology.**

> **InputRouter describes player intent; it does not judge it.**

> **NeckMotionModel owns physical neck state.**

> **JudgmentSystem owns timing quality.**

> **MotionQualityEvaluator owns movement quality.**

> **ScoringSystem owns score/combo/multiplier.**

> **HypeSystem owns HYPE/THE BANG/Finisher state.**

> **Presentation reads gameplay state; presentation never determines gameplay state.**

> **Results renders RunResult; it does not rebuild it.**

> **Progression consumes RunResult; it does not participate in gameplay.**

> **SaveService persists profile state; gameplay systems do not write files.**

> **Remote content may add content, but may not introduce runtime rules unknown to the installed client.**

---

# 37. Acceptance criteria for the architecture

The architecture is considered healthy when all of the following are true:

- scoring logic can be unit-tested without a Unity scene
- changing touch-zone ergonomics does not require changing chart files
- changing cue visuals does not change judgment timing
- changing body/hair art does not affect score
- a new song using existing mechanics can be added as content without changing gameplay code
- a new mechanic that requires new runtime semantics requires an explicit client/version change
- Results can render exclusively from RunResult/ProgressionOutcome
- SaveService can change serialization details without modifying scoring/gameplay systems
- offline cached songs remain playable without the content server
- pause/resume/retry do not move authored event timing relative to the authoritative audio clock
- no single manager accumulates unrelated responsibilities

---

## Final principle

Headbang Heroes should be architecturally simple to follow even as the content becomes more complex.

The target is not maximum abstraction.

The target is:

> **Explicit ownership, deterministic gameplay, replaceable infrastructure, presentation downstream of gameplay, and no spaghetti.**
