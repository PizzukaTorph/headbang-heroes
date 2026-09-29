# Gameplay Grammar v1 — Execution Roadmap

## Status

Planned post-POC gameplay milestone for Headbang Heroes.

This document records the sequence only. Each package is implemented and accepted independently before work begins on the next one.

Canonical architectural authority remains:
- `FOUNDATION.md`
- `TECHNICAL_CONTRACTS_V1.md`
- `DIFFICULTY_MODEL_V1.md`

## Goal

Move Headbang Heroes from direction + timing rhythm input toward a real headbang performance grammar.

Target flow:

```text
Authored MotionEvent
- time
- direction
- technique
- trajectory
- modifier
- intensity
        ↓
authoritative player neck movement
        ↓
Timing Quality
+ Motion Quality
+ Technique / semantic validation
        ↓
authoritative EventOutcome
```

The neck remains continuously physical and player-owned. Presentation remains downstream.

## Execution rule

> **One package at a time.**

For every package:

1. define the contract;
2. add/adjust canonical documentation;
3. add pure-domain tests;
4. implement the smallest complete runtime change;
5. run CI;
6. playtest where the package has experiential consequences;
7. tune data, not algorithms, where possible;
8. accept the package before starting the next one.

Do not pre-implement later packages merely because their future interface is obvious.

---

## GG1 — Technique Contract

### Purpose

Freeze the gameplay contract for Classic and the four authored Technique Skills before writing recognizers/evaluators.

Canonical split:

```text
Classic
= core movement
= always available
= not a special skill

Technique Skills
├── Half
├── Deep
├── Whiplash
└── Windmill
```

Technique Skills are manually authored special moments in the HH custom MIDI/chart source. They are not automatically injected from BPM, density or difficulty.

Each Technique Skill uses a recognizable on-screen gesture. Gesture recognition expresses player intent; it does not directly award success or bypass authoritative neck/gameplay evaluation.

See `docs/GAMEPLAY_TECHNIQUES_V1.md`.

### Authoring rule

```text
custom HH MIDI
        ↓
explicit technique marker/event
        ↓
chart compilation
        ↓
RuntimeChart
```

Every occurrence is chosen by the chart author for a musical reason.

Hard rule:

> **Do not spam or procedurally generate Technique Skills.**

Higher difficulty may use a broader authored vocabulary, but the runtime never adds skills merely because a chart is harder.

### Hard constraints

- Classic remains the normal/base HH gameplay;
- Half / Deep / Whiplash / Windmill are Technique Skills;
- Technique Skills are gesture-driven;
- no permanent extra skill buttons are required;
- no skill changes base neck physics;
- no skill auto-succeeds from gesture recognition alone;
- no technique is judged from animation frames;
- timing and technique execution remain separate;
- thresholds are data-driven;
- standard gameplay remains CURRENT + NEXT;
- no TechniqueEvaluator implementation until this contract is accepted.

### Exit gate

A canonical contract exists for Classic and all four Technique Skills, defined one at a time, including gesture semantics and required runtime evidence.

---

## GG2 — Rich Motion Evidence

### Purpose

Extend event-local neck evidence only with the measurements required by GG1.

Candidate evidence includes:

- travel;
- peak speed;
- peak amplitude / displacement;
- incoming velocity;
- inversion quality;
- axis balance;
- directional coherence;
- trajectory path;
- circularity;
- gesture duration / elapsed travel window.

This package does not score techniques.

### Hard constraints

- evidence is event/gesture-local, never run-global;
- fixed simulation remains authoritative;
- evidence collection must not modify physical response;
- no presentation dependency.

### Exit gate

Deterministic tests prove that the required evidence can be captured independently of render rate.

---

## GG3 — Technique Evaluator

### Purpose

Evaluate authored technique against physical evidence.

Conceptual output:

```text
TechniqueEvaluation
- technique
- quality 0..1
- passed / degraded
- diagnostic components
```

Exact shape is decided from GG1/GG2 evidence.

### Hard constraints

- pure domain component;
- no score ownership;
- no timing ownership;
- no presentation ownership;
- diagnostic components remain inspectable for tuning.

### Exit gate

All canonical techniques have deterministic evaluator coverage and a dedicated Technique Lab fixture can prove them.

---

## GG4 — Modifier v1

### Purpose

Implement authored performance modifiers after the base technique grammar works.

Initial order:

1. Accent
2. Double
3. Hold
4. Burst

The exact implementation model must preserve performability rather than translating musical subdivisions into tap spam.

### Hard constraints

- modifier semantics compose with technique/trajectory rather than replacing them;
- no modifier changes base neck physics;
- modifier-specific timing/state must remain deterministic;
- continuous modifiers must not rely on animation completion.

### Exit gate

Each supported modifier has explicit authored/runtime semantics and deterministic tests.

---

## GG5 — Cue Language

### Purpose

Communicate technique / trajectory / modifier requirements without turning HH into a dense note highway.

Current invariant remains:

> **Standard gameplay shows CURRENT + NEXT only.**

Direction remains readable through the established cue language. Technique/modifier information should use the minimum additional visual vocabulary necessary.

Candidate channels:

- cue shape;
- outline/pattern;
- compact glyph;
- motion/rotation;
- controlled color accents where they do not conflict with direction.

### Hard constraints

- cue visuals never determine timing;
- CURRENT + NEXT remains the standard visibility cap;
- readability must be validated on mobile;
- difficulty may alter presentation/readability without changing authored semantics.

### Exit gate

A player can identify the required action from CURRENT/NEXT without excessive text or visual clutter.

---

## GG6 — Outcome / Scoring Integration

### Purpose

Integrate the richer grammar into authoritative outcomes while preserving signal separation.

Target conceptual signals:

```text
Timing Quality
Motion Quality
Technique Quality
Modifier / semantic result
        ↓
EventOutcome
        ↓
ScoringSystem
```

The exact score formula remains tuning/configuration and must be established only after the three quality signals are observable independently.

### Hard constraints

- Timing Quality is never hidden inside Motion/Technique Quality;
- UI never recomputes score;
- diagnostic values remain available for playtest/tuning;
- difficulty does not inject a hidden Motion/Technique penalty.

### Exit gate

Score, combo, HYPE and Results consume one deterministic EventOutcome containing the accepted grammar evidence.

---

## GG7 — Technique Lab

### Purpose

Create an authored validation fixture specifically for gameplay grammar.

The fixture should expose clean, inspectable phrases for:

- Classic;
- Half;
- Deep;
- Whiplash;
- Windmill;
- accepted modifiers once GG4 lands.

It is a validation chart, not production music content.

### Hard constraints

- deterministic authored timing;
- readable isolated test phrases before mixed phrases;
- usable with telemetry;
- reusable in CI/domain tests where practical.

### Exit gate

Every grammar feature can be reproduced and tuned without depending on a licensed song or production chart.

---

## After Gameplay Grammar v1

Only after GG1–GG7 are accepted:

```text
real song
→ authored sections / techniques / modifiers
→ complete HH performance chart
→ first representative vertical slice
```

That vertical slice is where the project should begin answering the product question beyond the laboratory:

> Does this feel like performing a headbang, rather than pressing directional rhythm inputs?

## Non-goals for this milestone

Do not pull these into Gameplay Grammar v1:

- account/backend;
- ads/IAP;
- leaderboards;
- avatar customization;
- production progression/economy;
- remote content;
- generic editor/tooling beyond what the Technique Lab strictly requires.

A future chart-authoring/modding tool can consume the grammar once the runtime semantics are stable.
