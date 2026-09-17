# POC Implementation Master Plan

## Status

Execution plan for the Headbang Heroes POC.

This document does not redefine gameplay or architecture. Canonical authority remains the subsystem `*_V1.md` specifications and `FOUNDATION.md`.

## Goal

Turn the existing M0 prototype into one complete, deterministic, retryable song experience that proves the core question:

> Is controlling the head/neck to music actually fun?

## Execution model

Each implementation package gets its own plan under `docs/implementation/`.

An implementation agent must:
1. read `AGENTS.md`;
2. read the package plan and all canonical references it names;
3. inspect current code before changing it;
4. stay inside Scope / Out of Scope;
5. implement in small reviewable passes;
6. run the required automated verification;
7. use the named specialist agents for review;
8. stop at the package exit gate rather than silently starting later packages.

Prototype behavior is not authority when it conflicts with canonical documentation.

## Dependency graph

```text
P0A CORE RUNTIME

01 Neck Runtime
      │
      ├──────────────┐
      ↓              ↓
02 Rhythm Timing   03 Runtime Chart + Rest
      └──────┬───────┘
             ↓
04 Scoring + HYPE + THE BANG + Finisher
             ↓

P0B PRESENTATION

05 Gameplay Presentation Integration
             ↓

P0C PRODUCT LOOP

06 Results + Retry + Save/Profile + Minimal Shell
             ↓

P0D VALIDATION

07 Real Device Validation
             ↓
          POC GATE
```

Packages may prepare interfaces needed by later packages, but they must not implement later feature scope prematurely.

## Plans

| ID | Package | Primary outcome | Roadmap mapping |
|---|---|---|---|
| 01 | Neck Runtime | deterministic continuous neck simulation + event-local motion history | P0A WP1 |
| 02 | Rhythm Timing & Event Resolution | one authoritative time domain + deterministic candidate matching | P0A WP2 |
| 03 | Runtime Chart & Rest | immutable/prevalidated runtime chart + authored Rest evaluation | P0A WP3 |
| 04 | Scoring / HYPE / THE BANG / Finisher | deterministic authoritative event/run outcomes | P0A WP4 |
| 05 | Gameplay Presentation | readable CURRENT/NEXT + avatar/body/hair/HYPE feedback downstream of gameplay | P0B |
| 06 | POC Product Loop | complete Home → Song → Results → Retry loop with local persistence | P0C |
| 07 | Device Validation | real-phone timing, ergonomics, calibration and robustness evidence | P0D |

P1/P2/post-MVP plans should be authored only when the POC packages expose real implementation constraints. Do not prematurely prescribe them from prototype assumptions.

## Cross-package invariants

Every package must preserve these rules:
- `AudioClock` owns authoritative song time.
- Input changes neck state even when it does not earn a successful judgment.
- MISS never freezes/snaps/resets the neck.
- gameplay-critical neck simulation does not depend on render frame pacing.
- Timing Quality and Motion Quality remain separate signals.
- Motion Quality is based on event/gesture-local physical history.
- presentation cannot affect authoritative score/state.
- `ChartRuntime` owns authored chronology; it does not judge or render.
- `InputRouter` maps physical interaction to semantic intent; it does not judge.
- Results consume `RunResult`; they do not recalculate gameplay.
- no new gameplay semantics may be invented to preserve legacy M0 code.

## Shared verification expectations

At each package boundary:
- project compiles cleanly;
- relevant EditMode tests pass;
- relevant PlayMode tests pass where scene/runtime integration is involved;
- deterministic behavior is tested independently of render FPS where applicable;
- malformed/unsupported data fails explicitly rather than degrading silently;
- existing tests are updated only when canonical behavior changed, never merely to make failures disappear;
- specialist review findings are resolved or recorded explicitly before acceptance.

## Commit strategy

Prefer small semantic commits such as:

```text
refactor(neck): introduce deterministic neck state model
refactor(neck): move integration off render delta time
test(neck): add render-rate invariance coverage
```

Do not combine unrelated roadmap packages in one commit.

## POC completion gate

The POC is complete only when one complete authored song can repeatedly execute:

```text
Home
→ Song Select
→ Pre-song
→ Gameplay
→ Results
→ Retry / Continue
```

and demonstrates:
- synchronized authoritative audio timing;
- deterministic chart/event resolution;
- continuous enjoyable neck control;
- Timing + Motion Quality;
- combo/multiplier;
- HYPE;
- THE BANG;
- deterministic Finisher path;
- representative body/hair response;
- authoritative Results;
- local persistence;
- immediate Retry;
- real-device calibration/latency validation.

The final acceptance criterion is experiential as well as technical: repeated play must be desirable because the headbang mechanic itself is satisfying.
