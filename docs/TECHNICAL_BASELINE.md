# Technical Baseline

## Authority

Read `FOUNDATION.md` and `TECHNICAL_CONTRACTS_V1.md` first for canonical ownership/runtime rules.

This document defines the practical Unity baseline.

## Engine

Unity 6.6 Supported, current validated 6000.6.x patch.

- no alpha/beta editor builds
- migrate to a later LTS/Supported release only after validation

## Platforms

Product targets:
- Android
- iOS

Desktop Editor play mode is development-only.

No WebGL requirement.

## Presentation

- 2D
- portrait-first
- touch-first
- safe-area aware
- responsive phone aspect ratios
- 60 FPS presentation baseline

Rendering can vary independently from gameplay simulation timing.

## Rendering

Use Unity 2D workflow/URP where useful.

Prefer:
- static layered backgrounds
- lightweight parallax
- restrained lighting/particles
- mobile profiling early

Avoid expensive visual features without demonstrated value.

## Input

Use Unity Input System.

Physical controls map to semantic gameplay intent through InputRouter.

Touch coordinates/regions are never chart semantics.

Convert input timestamps into authoritative song-time before domain judgment.

## Audio/rhythm timing

Audio/DSP clock is authoritative.

Use scheduled playback where appropriate.

Never base authoritative scoring on:
- Update timing
- animation callbacks
- coroutines
- frame count
- visual cue completion

Required architecture includes:
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
- calibration/debug tools

## Neck simulation

Prototype may iterate quickly, but production scoring must not depend on render-frame pacing.

Direction:
- authoritative fixed simulation step (target such as 120 Hz to validate), or deterministic time-evaluated equivalent
- render interpolation independent of simulation
- event/gesture-local Motion Quality history

Current M0 `Time.deltaTime` neck integration is experimental and not the final contract.

## Content pipeline

Canonical flow:

```text
audio + metadata + chart
→ validate
→ compile RuntimeChart
→ package
→ publish
→ server/CDN
→ client cache
```

MIDI may be used for authoring/import, not required runtime parsing.

Compatible official content can be delivered remotely.

Remote content cannot introduce gameplay semantics unknown to the installed client.

## Content hosting

Use a provider abstraction rather than binding gameplay to one vendor.

Possible implementations:
- local
- Addressables-backed
- custom object storage/CDN

MVP trust baseline:
- HTTPS
- explicit versions
- hashes/checksums

Production publication should use trusted/signed release-manifest semantics or equivalent.

## Persistence

POC/MVP may use a simple versioned local profile.

SaveService owns serialization/migrations; gameplay systems never write files directly.

Future sync/economy authority stays behind ProfileSyncService/backend boundaries.

## Performance rules

- avoid per-frame allocations in gameplay hot paths
- pool repeated cue/feedback objects where useful
- profile on real Android/iOS hardware early
- protect audio/touch latency
- render frame drops must not redefine authored timing or gameplay-critical neck physics
- do not optimize blindly

## Testing

Use Unity Test Framework.

EditMode/pure tests:
- chart compile/validation
- candidate event matching
- timing boundaries
- neck simulation
- Motion Quality history
- Rest evaluation
- scoring/HYPE/progression
- save migrations

PlayMode/device:
- audio scheduling/sync
- pause/resume/retry
- touch/Input System
- calibration
- content cache/offline
- haptics
- presentation readability

## Repository layout target

```text
Assets/
  _HeadbangHeroes/
    Art/
    Audio/
    Content/
    Prefabs/
    Scenes/
    Scripts/
      Core/
      Audio/
      Charts/
      Input/
      Gameplay/
      Presentation/
      UI/
      Progression/
      Persistence/
      Content/
      Debug/
    Settings/
    Tests/
Packages/
ProjectSettings/
docs/
```

Do not reorganize purely for aesthetics; migrate toward this structure as production systems replace M0 prototype classes.

## Dependency policy

Core target remains mostly vanilla Unity + Headbang Heroes code.

Build game-specific core systems ourselves:
- DSP rhythm timing
- chart/runtime event model
- cue scheduling
- candidate matching/judgment
- neck simulation
- Motion Quality
- scoring/HYPE
- gameplay presentation orchestration

Avoid by default:
- generic rhythm-game frameworks
- structural DOTween dependence
- FMOD/Wwise before measured need
- third-party physics frameworks
- networking SDKs before online work
- dependency-injection frameworks
- large architecture packages

A dependency is justified only when it solves a current concrete problem materially better than a small maintained implementation.

## POC technical exit gate

One song proves:
- synchronized audio/chart
- readable cues
- always-responsive neck input
- meaningful momentum
- deterministic-enough scoring path moving toward authoritative fixed simulation
- timing vs Motion Quality separation
- HYPE/THE BANG/Finisher
- Results/Retry
- real-device feel worth replaying
