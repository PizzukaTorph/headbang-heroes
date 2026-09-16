# Headbang Heroes — Roadmap

## Principle

The roadmap exists to protect execution order, not to collect every future idea.

Primary milestone logic:

> **POC proves the mechanic. MVP proves the game.**

The POC must make the headbang fun.
The MVP must prove that the same foundation scales across four complete real songs.

Implementation work is expected to be done with **Kiro as the primary implementation assistant**, using the repository specialist agents as focused reviewers/guardians.

Canonical implementation loop:

```text
human decision / scoped task
        ↓
precise Kiro prompt
        ↓
Kiro implementation
        ↓
relevant specialist agent review
        ↓
QA Guardian adversarial review
        ↓
GitHub diff / test / device check
        ↓
accept or iterate
```

Do not ask Kiro to "fix the game" or refactor multiple unrelated systems at once.

---

# P0 — POC: Make The Headbang Fun

## Goal

Prove one complete song and the core neck/rhythm loop end-to-end.

## Required gameplay

- authoritative DSP/audio clock
- one complete authored chart, initially Normal
- CURRENT / NEXT cue flow
- semantic directional input
- Classic Bang Horizontal + Vertical sufficient to validate the core
- physical input always affects the neck
- continuous neck simulation
- timing judgment
- event-local Motion Quality
- combo / multiplier
- HYPE
- THE BANG
- at least one deterministic Finisher opportunity
- body response
- representative hair/secondary motion
- Results
- Retry

## Required technical validation

- song remains synchronized start-to-finish
- pause/resume and retry remain synchronized
- frame pacing does not define timing judgment
- gameplay-critical neck state uses an authoritative fixed-step or equivalent deterministic model before production scoring relies on it
- malformed chart/runtime data fails clearly
- real-device input latency can be inspected/calibrated

## Shell

Enough shell to exercise the loop:

```text
Home
→ Song Select
→ Pre-song
→ Gameplay
→ Results
→ Retry / Continue
```

No large campaign, backend, shop, realtime multiplayer, community system, or content CMS is required.

## Exit gate

A player can finish the song and wants to replay because controlling the neck itself is enjoyable, not because of progression rewards.

---

# P0A — Core Runtime Refactor

## Goal

Move the current M0 Unity prototype from experimental code to the canonical runtime contracts without broadening scope.

This is the immediate next implementation phase.

## Work package 1 — Neck runtime

Primary agents:
- `gameplay-core`
- `architecture-guardian`
- `qa-guardian`

Kiro tasks:
- replace/refactor legacy `HeadMotionModel` toward canonical `NeckMotionModel`
- remove render-frame-dependent gameplay-critical integration
- introduce authoritative fixed-step or equivalent deterministic simulation
- preserve continuous player ownership after MISS / wrong / early / late input
- formalize first-bang setup/unprepared semantics
- capture pre-inversion neck state/history
- replace run-global Motion Quality peak behavior with event/gesture-local evaluation
- keep tuning values configurable

Exit gate:
- equivalent authored input produces materially equivalent gameplay-domain state across different render FPS
- spam degrades movement quality through physics rather than arbitrary cooldowns
- MISS never snaps/resets/fakes neck state

## Work package 2 — Rhythm timing and event resolution

Primary agents:
- `rhythm-audio`
- `gameplay-core`
- `chart-content`
- `qa-guardian`

Kiro tasks:
- make AudioClock/song-time the only judgment time domain
- convert semantic input timestamps into authoritative song-time before matching/judgment
- preserve scheduled playback, pause/resume and retry synchronization
- replace permanent single-active-event assumptions with deterministic bounded candidate matching
- formalize early/wrong/late consumption semantics
- separate TimingJudgment from MotionQuality
- establish explicit event-resolution ordering

Exit gate:
- overlapping candidate windows resolve deterministically
- early physical input can move the neck without accidentally consuming a future event
- wrong-direction input in an eligible window follows the canonical MISS rule
- pause/resume/retry do not create clock drift or stale candidates

## Work package 3 — Runtime chart and Rest

Primary agents:
- `chart-content`
- `gameplay-core`
- `qa-guardian`

Kiro tasks:
- introduce production-shaped immutable/prevalidated RuntimeChart data
- keep raw MIDI/authoring formats out of gameplay runtime
- support MotionEvent and RestEvent
- implement Authored Rest evaluation with settling phase + stillness evaluation phase
- retain Natural Rest as absence of required events
- preserve stable chart/version/rules identity

Important scope rule:

The actual `.hh.mid` validator/compiler belongs to the future **external asset/chart management tool**. Unity only needs the runtime-side structures and temporary/dev fixtures necessary to test the game.

Exit gate:
- gameplay consumes runtime chart data rather than repeatedly parsing authoring sources
- Authored Rest behaves fairly after legitimate incoming momentum
- unsupported runtime semantics fail explicitly

## Work package 4 — Scoring / HYPE / THE BANG

Primary agents:
- `gameplay-core`
- `qa-guardian`

Kiro tasks:
- implement/align EventOutcome
- score from Timing + MotionQuality + technique/context
- canonical combo/multiplier behavior
- canonical HYPE gain
- THE BANG activation/state window
- deterministic Finisher eligibility/resolution
- build authoritative RunResult data
- avoid circular Scoring ↔ HYPE dependencies by snapshotting reward/context before outcome application

Exit gate:
- score is deterministic from authoritative event outcomes
- THE BANG cannot self-refill through its own multiplier
- Finisher uses normal gameplay execution, not a separate QTE

---

# P0B — POC Presentation Integration

## Goal

Make the canonical core readable and satisfying without allowing presentation to influence authoritative gameplay.

Primary agents:
- `presentation-avatar`
- `gameplay-core`
- `qa-guardian`

Kiro tasks:
- CURRENT / NEXT cue rendering
- head/neck-centered visual hierarchy
- body response driven only by neck motion
- hair secondary motion
- first venue/background reaction layer
- judgment feedback
- HYPE READY / THE BANG / Finisher presentation
- central haptics semantics
- compact topbar

Exit gate:
- player can primarily watch the avatar rather than stare at UI
- removing body/hair/venue presentation cannot change score
- cues remain readable on a real phone

---

# P0C — POC Product Loop

## Goal

Turn the core mechanic into one complete retryable song experience.

Primary agents:
- `meta-profile`
- `presentation-avatar`
- `qa-guardian`

Kiro tasks:
- Home → Song Select → Gameplay → Results
- authoritative RunResult → ResultsService
- S/A/B/C/D result presentation
- local best record
- minimum XP/HH progression integration
- save/profile round-trip
- Retry in one tap
- minimal settings/calibration

Exit gate:
- one song can be started, completed, scored, saved and retried repeatedly without manual repair/reset

---

# P0D — Device Validation

## Goal

Prove that the mechanic survives actual mobile timing and ergonomics.

Primary agent:
- `qa-guardian`

Support:
- `rhythm-audio`
- `gameplay-core`
- `presentation-avatar`

Validate at minimum:
- iOS real device
- Android real device when available
- 30/60/90/120 Hz render conditions where testable
- frame hitch during cue approach
- repeated pause/resume
- repeated retry
- large calibration offsets
- left/right thumb reach
- cardinal input ambiguity
- cue readability
- haptic usefulness/noise
- sustained play comfort

Exit gate:
- no major timing or control defect appears only outside Editor/Desktop testing

---

# P1 — Product Vertical Slice

## Goal

Turn the proven mechanic into a credible miniature Headbang Heroes experience before broadening content.

## Focus

- polished gameplay composition
- stronger avatar/body/hair presentation
- first convincing venue/background reaction profile
- simple avatar customization
- first-run/tutorial flow
- settings/calibration
- local profile/save
- progression/results integration
- server-first content path or faithful production-shaped equivalent
- validated content/package workflow

## Scope guidance

A second/third internal track may be useful for stress-testing tempo/style differences, but P1 is not defined by a fixed song count.

The important question is whether the complete product loop feels coherent around the proven mechanic.

## External asset/chart tool boundary

Begin the external tool only when manual asset/chart handling starts creating real friction.

Planned responsibilities:
- import audio
- manage song metadata
- author/import `.hh.mid`
- validate `HH_MIDI_STANDARD_V1`
- compile HH MIDI → canonical ChartDefinition/runtime payload
- inspect sections/phrases/events
- validate package assets, versions and provenance
- eventually package/publish official content

Do not turn this into a second game engine or an MVP blocker.

---

# P2 — MVP: Four Real Songs

## Goal

Prove that Headbang Heroes is a small complete game, not a one-song prototype.

## Definition

**Four complete real songs** work end-to-end reliably.

Each must:
- use the canonical Song/Chart model
- carry an authored difficulty classification
- load through the real/final-shaped content pipeline
- remain synchronized from start to finish
- resolve timing/motion/technique deterministically
- support HYPE/THE BANG/Finisher where authored
- produce correct Results
- persist records/progression
- retry cleanly
- remain playable offline once validly cached where licensing/product rules allow

The MVP does not require every song at every difficulty.

## Supporting MVP product surface

- Home
- Song Select
- simple Avatar customization
- Results / progression
- Profile/save
- Settings / calibration / accessibility basics
- official catalog filters
- Quick Headbang path

## Explicitly optional / deferrable beyond MVP

- networked Versus
- complete Story campaign
- advanced Practice tools
- Endurance
- leaderboards
- asynchronous challenges
- shop/large cosmetic catalog
- community publishing/browser
- full CMS
- elaborate economy balancing
- large venue library

If one of these threatens the four-song goal, it loses.

---

# P3 — Post-MVP Product Expansion

## Goal

Expand around the proven four-song product without redefining the core gameplay semantics.

Candidate workstreams:

### Story
- structured progression/narrative
- lightweight scenes/dialogue
- bosses/rivals
- authored challenges
- larger catalog usage

### Versus
- asynchronous competition first where appropriate
- exact chart/rules version references
- leaderboard/competitive integrity work
- realtime battle only after validation

### Practice
- phrase/section selection
- repeat/retry tooling
- technique-focused practice
- playback-speed experiments only if musically/technically acceptable

### Endurance
- longer sequences
- cumulative scoring/HYPE management
- session rules using existing gameplay systems

### Content scale
- larger official catalog
- improved publication tooling
- artist/band content workflows
- richer venues/presentation
- external asset/chart tool matured as content scale justifies it

### Community/modding
- local/community packages
- curated publishing/trust rules
- official/community separation
- rights/audio provenance constraints

---

# P4 — Mature v1 / Live Product

Possible mature-product capabilities after the core and expansion modes prove worthwhile:

- polished Story arc
- strong Versus offering
- larger official music catalog
- robust content release pipeline
- achievements/challenges
- social/leaderboards as justified
- store/platform compliance if monetization is introduced
- privacy-conscious analytics
- live content cadence
- curated community ecosystem if viable

These are not foundation requirements.

---

# Cross-cutting roadmap rules

## Gameplay before meta

No progression, economy, story, or online feature may be used to hide weak neck gameplay.

## Kiro is an implementer, not the design authority

Kiro must work from `AGENTS.md`, `docs/FOUNDATION.md`, the relevant canonical specs, and the assigned specialist-agent briefs.

If prototype code conflicts with canonical design, implementation changes; design is not silently redefined to preserve old code.

## Small scoped implementation passes

Prefer a sequence of narrow, testable Kiro tasks over one giant refactor request.

Each pass should state:
- files/systems in scope
- systems explicitly out of scope
- canonical contracts to preserve
- required tests
- acceptance criteria
- relevant specialist agents

## Content before complexity

Once the mechanic works, proving multiple real songs is more valuable than accumulating secondary systems.

## One gameplay language

Modes compose the same core chart/neck/scoring/HYPE systems. Do not fork separate gameplay implementations per mode.

## Server-first content, offline-capable play

Compatible official content can be delivered remotely and cached locally. Remote content never introduces unknown gameplay semantics.

## Authoring tool stays external

HH MIDI, asset validation, packaging and content authoring belong to an external toolchain boundary.

Unity/runtime consumes validated/compiled content and must not become the primary asset-authoring application.

## No power progression

Progression unlocks expression/content, not mechanical advantage.

## Monetization later

No gameplay ads, shop complexity, premium-currency design, or reward-loop optimization is required to prove POC/MVP. Monetization is a separate later design problem.

## Real-device validation

Audio/input latency, touch ergonomics, performance, and haptics must be validated on actual iOS/Android devices early and repeatedly.

---

# Immediate next action

Start **P0A / Work package 1 — Neck runtime** with Kiro.

Use:
- `agents/gameplay-core.md`
- `agents/architecture-guardian.md`
- `agents/qa-guardian.md`
- `docs/TECHNICAL_CONTRACTS_V1.md`
- `docs/FOUNDATION.md`

Do not move to the next work package until the neck simulation contract is demonstrably correct and testable.
