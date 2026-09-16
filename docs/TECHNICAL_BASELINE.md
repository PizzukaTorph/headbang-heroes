# Technical Baseline

## Engine

**Unity 6.6 Supported**, starting from a current 6000.6.x patch.

Why:
- new project, not production-locked
- Unity recommends Supported Update releases for new/mid-cycle projects
- latest platform support and fixes are valuable for a mobile-first rhythm game
- migrate to Unity 6.7 LTS when it is available and validated

Do not use alpha/beta editor builds.

## Platforms

Mobile only:
- Android
- iOS

Desktop editor play mode exists for development only and is not a supported product target.
No WebGL requirement.

## Presentation

- 2D
- portrait-first
- touch-first
- safe-area aware
- responsive across common phone aspect ratios
- 60 FPS baseline target

## Rendering

Start with Unity's 2D workflow and URP only if/where it provides clear value. Avoid expensive rendering features. Static backgrounds and lightweight layered/parallax effects are preferred.

## Input

Use Unity Input System.
Gameplay API must model abstract headbang actions rather than platform-specific touch coordinates so gestures can evolve independently.

## Audio/rhythm architecture

Audio timing is authoritative.

Use DSP/audio-clock timing for gameplay synchronization. Never base scoring on Update(), animation callbacks, coroutines, frame count, or visual ring completion.

Required systems:
- AudioClock
- SongDefinition
- ChartDefinition / RuntimeChart
- ChartRuntime
- CueScheduler
- InputRouter
- NeckMotionModel
- JudgmentSystem
- MotionQualityEvaluator
- ScoringSystem
- HypeSystem
- RunResultBuilder
- Calibration/debug overlay

The closing-circle cue is a visualization of an already-scheduled event.

Detailed ownership rules are defined in `docs/TECHNICAL_CONTRACTS_V1.md`.

## Content pipeline

Source concept:
audio master + MIDI/gameplay authoring data + metadata

MIDI should be imported/compiled into an internal Headbang Heroes chart representation. Runtime gameplay must not depend on parsing arbitrary MIDI files unless there is a concrete benefit.

Authoring data should be validated/compiled into immutable runtime-ready chart data before active gameplay.

The POC may use local bundled content only.

Production direction is server-first content delivery with local cache and offline playback of already-valid content. Remote songs/charts/art/backgrounds may be delivered without a client rebuild **only when they use gameplay semantics already supported by the installed client**.

Do not couple gameplay code directly to one remote hosting provider. Addressables may be introduced when remote asset delivery becomes useful, behind the content boundary described in `TECHNICAL_CONTRACTS_V1.md`.

## Persistence

The POC does not require production-grade profile synchronization.

Production direction:
- versioned local-first UserProfile
- explicit SaveService
- schema migrations
- future ProfileSyncService/backend boundary

Gameplay systems must not write persistence files directly.

## Performance rules

- avoid per-frame allocations in gameplay
- pool repeated cue/feedback objects where useful
- profile on real Android and iOS hardware early
- do not optimize blindly, but protect audio/touch latency
- gameplay logic must remain framerate-independent
- do not parse/compile remote authoring content in the active gameplay hot path

## Repository layout target

```
Assets/
  _HeadbangHeroes/
    Core/
      Gameplay/
      Chart/
      Scoring/
      Hype/
      Models/
    Content/
      Catalog/
      Loading/
      Validation/
    Presentation/
      Avatar/
      Hair/
      Venue/
      Cues/
      Haptics/
    UI/
      Home/
      SongSelect/
      Gameplay/
      Results/
      Avatar/
    Progression/
    Persistence/
    Settings/
    Tests/
Packages/
ProjectSettings/
docs/
```

Production licensed content may require local ignored locations depending on redistribution rights.

## Prototype scene

One scene: `Prototype_Headbang`

It must prove:
audio -> scheduled chart event -> closing circle -> touch -> timing judgment -> neck response -> motion quality -> combo/score/HYPE -> feedback.

## POC non-goals

The first one-song POC does not require:
- backend accounts
- production profile sync
- ads
- IAP
- multiplayer networking
- production remote catalog/download UI
- localization framework
- elaborate DI framework
- final content economy
- asset-store architecture dependencies

These are POC non-goals, not permanent product exclusions. Remote content, persistence and future online systems are allowed by the production architecture when their corresponding design milestone requires them.

## Dependency policy — build the core ourselves

Headbang Heroes should remain deliberately close to vanilla Unity during prototype and early production.

Default rule: **write the game-specific systems ourselves and add a package only when a concrete, measured problem justifies it.** Do not let a rhythm-game framework or asset dictate the game design.

Expected baseline dependencies are Unity's own standard packages/features where useful, especially Input System, Test Framework and the normal 2D/UI/audio stack. Addressables may be adopted when remote delivery provides concrete value.

The following are intentionally custom:
- DSP-clock rhythm scheduling
- chart/event model
- chart runtime/compiler contracts
- closing-circle cue timing
- judgment windows and early/late measurement
- motion-quality evaluation
- combo and scoring
- neck momentum/inertia model
- headbang gesture/technique vocabulary
- HYPE/THE BANG/Finisher orchestration
- gameplay feedback orchestration

MIDI may use a small editor/import-time library if it clearly reduces work, but the preferred pipeline compiles MIDI into Headbang Heroes' internal chart format. Runtime gameplay should not depend on a general MIDI framework without a demonstrated need.

Avoid introducing, by default:
- generic rhythm-game frameworks/kits
- DOTween as a structural gameplay dependency
- FMOD/Wwise
- third-party physics frameworks
- networking SDKs before Versus actually requires them
- dependency-injection frameworks
- large architecture/framework packages

A dependency is acceptable when all of the following are true:
1. there is a real current problem, not a hypothetical future one;
2. the package solves it materially better/safer than a small custom implementation;
3. mobile cost and maintenance burden are understood;
4. its license is compatible with the project;
5. it does not own or distort the core headbang/rhythm design.

Target mindset: roughly **90–95% Unity + Headbang Heroes code** for the core prototype.

## Architecture rule

The project must avoid a God `GameManager`.

Core deterministic flow should remain explicit and testable. Presentation may react through semantic events, but gameplay ownership must remain traceable.

See `docs/TECHNICAL_CONTRACTS_V1.md` for the canonical system boundaries.
