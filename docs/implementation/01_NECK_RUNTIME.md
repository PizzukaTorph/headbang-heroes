# Implementation Plan 01 — Neck Runtime

## Status

First executable implementation package for P0A / Core Runtime Refactor.

Roadmap mapping: **P0A — Work package 1: Neck runtime**.

Do not begin package 02 until this package's acceptance criteria are satisfied.

## 1. Goal

Refactor the M0 head-motion experiment into the canonical deterministic `NeckMotionModel` gameplay-domain system without changing the intended player fantasy.

The resulting system must preserve continuous player ownership of neck motion while making gameplay-critical state independent from render frame pacing and suitable for deterministic Motion Quality evaluation.

## 2. Canonical references

Read before implementation, in authority order:
- `AGENTS.md`
- `docs/FOUNDATION.md`
- `docs/TECHNICAL_CONTRACTS_V1.md`
- `docs/SCORING_SYSTEM_V1.md` for the Motion Quality boundary only
- `docs/GAMEPLAY_UX_V1.md` where input/motion semantics are referenced
- `docs/CONSOLIDATION_REVIEW_2026-09-16.md`
- `docs/ROADMAP.md` P0A / Work package 1

Specialist agents:
- `agents/gameplay-core.md` — primary
- `agents/architecture-guardian.md` — architecture review
- `agents/qa-guardian.md` — adversarial verification

The current M0 implementation is evidence/prototype material, not design authority.

## 3. Current-state investigation

Before editing code, inspect and record the actual current implementation of:
- `HeadMotionModel` and all callers;
- the gameplay update/tick path;
- how `Time.deltaTime` currently enters gameplay-critical integration;
- current motion state fields and tuning constants;
- current inversion/impulse application;
- current first-bang behavior;
- current `peakAmplitude` / Motion Quality history behavior;
- presentation transforms that currently read or mutate head state;
- tests covering head motion, judgments and scheduler interactions.

Do not assume file names or class boundaries from this document if the repository differs. Adapt the migration to the actual code while preserving the contracts below.

## 4. Scope

Implement/refactor only what is necessary to establish the production-shaped neck gameplay domain:

- canonical `NeckMotionModel` ownership of gameplay-critical neck state;
- deterministic authoritative simulation stepping independent of render FPS;
- configurable simulation/tuning values;
- semantic inversion/launch input for the currently supported Classic Bang directions;
- inversion allowed anywhere in travel;
- continuous passive motion/recovery after input and after MISS/no-event periods;
- physical limits without snapping authored poses;
- explicit first-bang setup/unprepared state;
- pre-inversion snapshot support;
- bounded event/gesture-local motion history needed by later Motion Quality evaluation;
- render/presentation reading/interpolating authoritative state downstream;
- migration/removal of obsolete run-global motion-quality state where it belongs to this package;
- tests proving the neck contract.

Classic Horizontal and Vertical are sufficient for this package unless existing canonical runtime structures make all cardinal directions trivial without scope growth.

## 5. Out of scope

Do **not** implement or redesign:
- RuntimeChart compilation;
- dense candidate matching;
- new timing judgment windows;
- scoring formulas;
- HYPE;
- THE BANG;
- Finisher;
- Authored Rest evaluation;
- body/hair/VFX polish;
- new techniques beyond what is required to preserve existing interfaces;
- backend/content delivery;
- save/profile;
- external `.hh.mid` tooling;
- broad scene/UI refactors.

If an interface seam is required for a later package, create the smallest explicit seam and stop there.

## 6. Required architecture

### Ownership

`NeckMotionModel` owns authoritative physical gameplay state.

Presentation may consume snapshots but must not mutate authoritative neck state.

Judgment/MotionQuality may inspect snapshots/history but must not integrate motion.

### Authoritative step

Gameplay-critical simulation must use a deterministic step independent of render `Update()` cadence.

Preferred direction from the foundation is a fixed authoritative step, initially configurable around a high enough rate for rhythm gameplay (the documentation cites ~120 Hz as a tuning candidate, not a hardcoded permanent rule).

The implementation must support advancing from authoritative elapsed/song time without losing or double-applying simulation steps during variable render frames.

Do not use Unity `FixedUpdate()` merely because it is named "fixed" without first proving that its lifecycle/time ownership fits the audio-authoritative architecture. A domain-owned fixed-step accumulator/advance API is acceptable and likely preferable.

### Conceptual state

Use the minimum state actually required, but the domain model should be able to represent:
- displacement/angle or trajectory components;
- velocity;
- inversion/launch direction/state;
- damping/recovery;
- physical limits;
- prepared/unprepared/setup status;
- event-local motion history.

Do not expose Unity scene transforms as canonical state.

### Input contract

For Classic Bang:
- LEFT means inversion/commit at LEFT and launch RIGHT;
- RIGHT means inversion/commit at RIGHT and launch LEFT;
- UP means inversion/commit at UP and launch DOWN;
- DOWN means inversion/commit at DOWN and launch UP.

An inversion request is accepted anywhere in travel. Do not gate it on reaching an expected pose/side.

Every valid semantic physical input changes neck motion, including inputs that later become early/late/wrong/MISS/no-match in other systems.

### First bang

The first relevant input from neutral is setup/unprepared:
- it launches motion normally;
- there is no preceding travel to evaluate with ordinary Motion Quality;
- the model exposes enough state/context for downstream systems to distinguish this condition deterministically.

Do not fake a perfect/zero motion-quality sample inside the physics model.

### Pre-inversion snapshot

The runtime must be able to capture the arriving state/history immediately **before** applying the new inversion impulse.

This is required by canonical event resolution ordering:

```text
snapshot pre-inversion state/history
→ apply physical input immediately
→ downstream matching/judgment
→ Motion Quality evaluates arrival from pre-inversion evidence
```

The snapshot must be immutable/value-like enough that applying the new input cannot mutate the evidence being judged.

### Event-local history

Remove the M0 assumption that a run-global peak amplitude is valid Motion Quality evidence.

Provide bounded/local history sufficient for later evaluation of concepts such as:
- travel/amplitude since relevant inversion/setup boundary;
- incoming velocity/momentum;
- continuity;
- inversion preparation.

Do not implement the final scoring formula here. This package provides trustworthy physical evidence.

## 7. Implementation tasks

### NECK-01 — Audit and characterization
- locate current motion implementation/call graph;
- document current M0 behavior relevant to migration in code comments/tests only where useful;
- identify render-time coupling and presentation mutation risks;
- run existing tests before refactor and establish baseline failures if any.

### NECK-02 — Introduce canonical domain state/config
- introduce/refine `NeckMotionState` as gameplay-domain data;
- introduce configurable `NeckMotionConfig` or equivalent;
- separate gameplay state from Unity transform/presentation state;
- avoid static/global mutable tuning state.

### NECK-03 — Deterministic stepping
- replace gameplay-critical `Time.deltaTime` integration with explicit deterministic stepping;
- make step size/configuration explicit;
- ensure variable render cadence can advance zero/one/many simulation ticks correctly;
- define/reset accumulator semantics for run start/retry/pause as needed without implementing package-02 audio logic.

### NECK-04 — Inversion/launch semantics
- implement canonical Classic inversion meaning;
- accept inversion anywhere in travel;
- preserve momentum/inertia semantics rather than teleporting to authored positions;
- enforce physical limits through physical model constraints, not pose snapping.

### NECK-05 — Setup and pre-inversion evidence
- formalize neutral/setup/unprepared semantics;
- expose pre-inversion snapshot capture;
- ensure first bang creates motion but cannot claim ordinary preceding-travel evidence.

### NECK-06 — Local motion history
- replace/remove run-global peak accumulation used as physical-quality evidence;
- track bounded gesture/event-local evidence;
- define clear rollover/reset boundaries tied to physical actions, not render frames;
- ensure history remains deterministic.

### NECK-07 — Presentation boundary migration
- make current visual head transform consume authoritative neck state;
- interpolation/smoothing may be presentation-only;
- prove presentation changes cannot alter gameplay state;
- avoid broad body/hair work.

### NECK-08 — Compatibility cleanup
- migrate callers from legacy `HeadMotionModel` naming/contract where safe;
- delete obsolete compatibility code when no longer referenced;
- if a temporary adapter is necessary, mark it clearly and keep it one-way toward the canonical model.

### NECK-09 — Verification and specialist review
- run all relevant tests;
- run gameplay-core review;
- run architecture-guardian review;
- run qa-guardian adversarial review;
- address findings within scope;
- report remaining unrelated findings without expanding scope.

## 8. Data/API contract guidance

Exact C# API naming may adapt to the repository, but the architecture should make operations equivalent to these concepts explicit:

```text
NeckMotionModel
- State
- Advance(authoritativeDelta / targetTime)
- CapturePreInversionSnapshot()
- ApplyBang(semanticDirection/action)
- Reset(initialState)

NeckMotionState
- physical trajectory/displacement
- velocity/momentum
- current launch/inversion context
- setup/prepared context

NeckMotionSnapshot
- immutable physical state
- bounded local history/evidence
- simulation timestamp/tick if useful

NeckMotionConfig
- simulation step
- impulse/launch tuning
- damping/recovery tuning
- physical limits
- other physical tuning values
```

Do not make `MonoBehaviour` the only usable API for the deterministic model. Core physics should be testable without a scene/render loop.

## 9. Migration strategy

Prefer incremental replacement over a big-bang scene rewrite:

1. characterize current M0 tests/behavior;
2. create deterministic domain state/model;
3. migrate current gameplay driver to advance the model explicitly;
4. migrate semantic input to call the new inversion API;
5. migrate presentation to read the new state;
6. migrate Motion Quality evidence boundary;
7. remove obsolete legacy state/calls;
8. keep the playable M0 scene functional throughout where practical.

Do not preserve a legacy behavior solely because tests encoded an obsolete prototype assumption. Update such tests to canonical behavior and state why.

## 10. Required tests

At minimum add/retain automated coverage for:

### Determinism / render-rate invariance
Given the same initial state, semantic input sequence and authoritative elapsed time:
- simulate under representative render delivery patterns corresponding to ~30, 60, 90 and 120 FPS;
- authoritative neck state at comparison points must be equal within explicitly justified numeric tolerance;
- number/order of authoritative simulation ticks must not depend on render FPS.

### Classic inversion semantics
- LEFT launches RIGHT;
- RIGHT launches LEFT;
- UP launches DOWN;
- DOWN launches UP;
- inversion before reaching a side is accepted and reverses/redirects motion;
- no authored pose snap occurs.

### Anti-spam through physics
Compare properly paced alternating inputs with very rapid micro-inversions:
- rapid spam remains accepted physically;
- it produces materially lower travel/amplitude/preparation evidence;
- no arbitrary cooldown is required to manufacture the penalty.

### MISS/passive continuity
With no subsequent input:
- existing motion continues;
- physical limits/recovery behave naturally;
- no reset/snap occurs because an expected gameplay event was missed elsewhere.

### First bang
- neutral first bang launches motion;
- snapshot/context reports unprepared/setup state;
- no fake preceding-travel history is created.

### Pre-inversion snapshot
- snapshot captures state before impulse;
- applying bang cannot mutate the captured snapshot/history;
- downstream evaluation can distinguish incoming motion from outgoing motion.

### Local history
- evidence rolls over at defined physical boundaries;
- old run history cannot inflate a later event's amplitude/quality evidence;
- repeated deterministic runs produce equivalent evidence.

### Presentation isolation
Where feasible in EditMode/domain tests, demonstrate that authoritative state evolves without any presentation object. Add PlayMode coverage only for the adapter/transform wiring that actually needs Unity scene lifecycle.

## 11. Acceptance criteria

Package 01 is complete only when all are true:

- [ ] gameplay-critical neck integration no longer depends on render `Time.deltaTime`;
- [ ] deterministic domain model is testable without presentation;
- [ ] equivalent authored input produces materially equivalent authoritative state across different render FPS delivery patterns;
- [ ] Classic inversion semantics match `FOUNDATION.md`;
- [ ] inversion is accepted anywhere in travel;
- [ ] spam degrades physical travel/quality evidence without arbitrary input cooldowns;
- [ ] no MISS/no-input path snaps or resets neck state;
- [ ] first bang has explicit setup/unprepared semantics;
- [ ] immutable pre-inversion evidence can be captured before applying an impulse;
- [ ] run-global peak amplitude is no longer the basis for later Motion Quality;
- [ ] presentation is downstream from authoritative neck state;
- [ ] relevant EditMode/PlayMode tests pass;
- [ ] gameplay-core, architecture-guardian and qa-guardian reviews have no unresolved package-blocking findings;
- [ ] current playable prototype still runs sufficiently to proceed to package 02.

## 12. Verification report required from implementation agent

At completion, return a concise report containing:
- files changed;
- architecture decisions made;
- legacy behavior removed/replaced;
- automated tests added/updated and results;
- determinism test methodology and observed result;
- specialist agents invoked and their findings;
- manual Unity verification performed;
- known issues explicitly outside package scope;
- commit SHAs created.

Do not report the package complete if acceptance criteria are not met.

## 13. Deliverables

Expected deliverables are implementation-dependent but should include:
- deterministic neck domain model/state/config;
- explicit stepping/advance mechanism;
- canonical semantic inversion handling;
- pre-inversion snapshot/local-history mechanism;
- presentation adapter/migration as needed;
- automated deterministic behavior tests;
- removal or containment of obsolete M0 motion state;
- no unrelated feature work.

## 14. Commit strategy

Recommended logical sequence, adapted to actual code:

```text
refactor(neck): introduce canonical neck domain state
refactor(neck): make simulation stepping deterministic
refactor(neck): implement canonical inversion semantics
refactor(neck): add local pre-inversion motion evidence
test(neck): cover deterministic motion contracts
refactor(neck): migrate presentation and remove legacy head model
```

Fewer commits are acceptable when changes cannot safely compile independently. Do not create artificial commits that leave the project broken.

## 15. Do not

- do not ask Unity animation to define gameplay motion;
- do not use render frame timing for scoring physics;
- do not snap the head to LEFT/RIGHT/UP/DOWN poses on input;
- do not reject early inversion because an expected side was not reached;
- do not add an anti-spam cooldown as a substitute for motion quality;
- do not let presentation mutate the domain model;
- do not calculate score/HYPE/Finisher here;
- do not redesign the chart format here;
- do not introduce a DI framework/event-bus architecture for this package;
- do not preserve obsolete M0 semantics merely to minimize diff size;
- do not proceed to implementation package 02 automatically.
