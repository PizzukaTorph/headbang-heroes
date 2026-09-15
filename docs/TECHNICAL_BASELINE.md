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
- ChartDefinition
- ChartScheduler
- InputRouter
- JudgmentSystem
- HeadMotion/HeadPhysics
- ComboSystem
- ScoreSystem
- FeedbackSystem
- Calibration/debug overlay

The closing-circle cue is a visualization of an already-scheduled event.

## Content pipeline

Source concept:
audio master + MIDI gameplay data + metadata

MIDI should be imported/compiled into an internal Unity chart representation. Runtime gameplay must not depend on parsing arbitrary MIDI files unless there is a concrete benefit.

## Performance rules

- avoid per-frame allocations in gameplay
- pool repeated cue/feedback objects
- profile on real Android and iOS hardware early
- do not optimize blindly, but protect audio/touch latency
- gameplay logic must remain framerate-independent

## Repository layout target

```
Assets/
  _HeadbangHeroes/
    Art/
    Audio/
    Prefabs/
    Scenes/
    Scripts/
      Core/
      Audio/
      Charts/
      Input/
      Gameplay/
      UI/
      Debug/
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
audio -> scheduled chart event -> closing circle -> touch -> timing judgment -> head response -> combo/score -> feedback.

## Non-goals

No backend, accounts, ads, IAP, multiplayer networking, Addressables architecture, localization framework, elaborate DI framework, production save system or asset-store dependency unless the prototype specifically requires it.

## Dependency policy — build the core ourselves

Headbang Heroes should remain deliberately close to vanilla Unity during prototype and early production.

Default rule: **write the game-specific systems ourselves and add a package only when a concrete, measured problem justifies it.** Do not let a rhythm-game framework or asset dictate the game design.

Expected baseline dependencies are Unity's own standard packages/features where useful, especially Input System and the normal 2D/UI/audio stack.

The following are intentionally custom:
- DSP-clock rhythm scheduling
- chart/event model
- closing-circle cue timing
- judgment windows and early/late measurement
- combo and scoring
- head momentum/inertia model
- headbang gesture/technique vocabulary
- gameplay feedback orchestration

MIDI may use a small editor/import-time library if it clearly reduces work, but the preferred pipeline compiles MIDI into Headbang Heroes' internal chart format. Runtime gameplay should not depend on a general MIDI framework without a demonstrated need.

Avoid introducing, by default:
- generic rhythm-game frameworks/kits
- DOTween as a structural gameplay dependency
- FMOD/Wwise
- third-party physics frameworks
- networking SDKs
- dependency-injection frameworks
- large architecture/framework packages

A dependency is acceptable when all of the following are true:
1. there is a real current problem, not a hypothetical future one;
2. the package solves it materially better/safer than a small custom implementation;
3. mobile cost and maintenance burden are understood;
4. its license is compatible with the project;
5. it does not own or distort the core headbang/rhythm design.

Target mindset: roughly **90–95% Unity + Headbang Heroes code** for the core prototype.
