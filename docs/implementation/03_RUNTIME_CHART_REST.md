# Implementation Plan 03 — Runtime Chart & Authored Rest

## Status

P0A / Work package 3. Plan 01 must be accepted first. This package may be implemented before or after Plan 02, but integration acceptance requires the timing/candidate contracts from Plan 02.

## 1. Goal

Introduce production-shaped immutable/prevalidated runtime chart data that gameplay can consume cheaply and deterministically, and implement the canonical Authored Rest lifecycle using physical neck evidence.

Unity must consume compiled semantics, not become the primary HH MIDI authoring application.

## 2. Canonical references

Read before implementation:
- `AGENTS.md`
- `docs/FOUNDATION.md`
- `docs/TECHNICAL_CONTRACTS_V1.md`
- `docs/SONG_CHART_MODEL_V1.md`
- `docs/HH_MIDI_STANDARD_V1.md` for authoring/runtime boundary only
- `docs/CHART_TOOLING_MODDING_V1.md` for tooling boundary
- `docs/CONTENT_PIPELINE_V1.md` where runtime package identity is relevant
- `docs/CONSOLIDATION_REVIEW_2026-09-16.md`
- `docs/ROADMAP.md` P0A WP3

Specialists:
- `agents/chart-content.md` — primary
- `agents/gameplay-core.md`
- `agents/qa-guardian.md`
- `agents/architecture-guardian.md` where data ownership boundaries change

## 3. Current-state investigation

Inspect:
- current ScriptableObject/song/chart structures;
- current event representation/time-in-seconds assumptions;
- M0 chart fixture/builders;
- MIDI parsing/import code, if any;
- scheduler data access patterns;
- IDs/version fields currently available;
- section/phrase support;
- any existing Rest-like behavior;
- validation/error behavior for malformed chart data;
- tests and lab assets depending on prototype structures.

## 4. Scope

- canonical runtime-side `SongDefinition`/`ChartDefinition` boundary as needed by Unity;
- semantic validation sufficient for runtime/dev fixtures;
- compilation/conversion into immutable/prevalidated `RuntimeChart`;
- deterministic precomputed event timings/ticks suitable for gameplay hot path;
- stable song/chart/event identity and schema/chart/rules versions;
- ordered MotionEvent and RestEvent runtime representation;
- sections/phrases retained in runtime where useful for queries/context;
- chart query API compatible with Plan-02 bounded candidate matching;
- explicit failure for unsupported semantics/versions;
- Natural Rest = absence of required events;
- Authored Rest = explicit interval with settling then stillness evaluation;
- `RestEvaluator` using Plan-01 authoritative neck state/history;
- tests for compilation, ordering, validation, identity and Rest fairness.

## 5. Out of scope

Do not build:
- the external `.hh.mid` editor/validator/compiler product;
- a Unity MIDI authoring UI;
- community browser/publishing;
- remote catalog/CDN implementation;
- full Addressables/content backend;
- scoring consequences for Rest beyond producing authoritative RestOutcome/evidence seam for Plan 04;
- HYPE/THE BANG/Finisher;
- new techniques;
- speculative tempo-map complexity not needed by current songs, though schema/API must not make future support impossible.

A small dev/test converter or fixture builder inside Unity is allowed only to create canonical runtime data for testing.

## 6. Required architecture

### Authoring vs runtime

Canonical pipeline boundary:

```text
SongDefinition + ChartDefinition
        ↓ validation/compilation
immutable RuntimeChart
        ↓
ChartRuntime / gameplay
```

HH MIDI is authoring/interchange. Runtime must not parse MIDI notes/tracks during gameplay.

### RuntimeChart properties

`RuntimeChart` must be:
- immutable during a run;
- ordered deterministically;
- prevalidated;
- cheap to query;
- resolved to deterministic runtime timing;
- identified by song/chart/schema/chart/rules versions;
- composed of typed known semantics.

Unsupported semantics/version combinations must fail explicitly before gameplay rather than silently ignore events.

### Event families

v1 runtime families:
- MotionEvent
- RestEvent

Do not create a generic bag of loosely typed event dictionaries when typed semantics are available.

MotionEvent carries canonical technique/trajectory/direction/modifier semantics plus duration/intensity/tags/Finisher-candidate data where authored.

RestEvent is not a MotionEvent technique.

### Time representation

Authoring may remain beat-space. Runtime event time must be precomputed deterministically from the chart musical timing model.

For the current subset, single BPM/time signature is acceptable. Do not repeatedly convert beats through accumulated floating-point calculations in the gameplay hot path.

Choose a runtime representation with sufficient precision and explicit conversion tests. Preserve author-facing beat positions separately only if useful for diagnostics/tooling.

### Rest lifecycle

```text
Rest begins
→ settling phase
→ stillness evaluation phase
→ RestOutcome
```

Settling exists so legitimate incoming momentum can dissipate. Rest never cancels velocity, snaps to neutral or changes neck physics.

Stillness must be evaluated from authoritative neck state/history, not avatar animation.

Thresholds and phase proportions/durations are configurable data.

## 7. Implementation tasks

### CHART-01 — Audit prototype data
Map current song/chart/event types, builders, IDs, time assumptions and scheduler dependencies.

### CHART-02 — Define runtime semantic types
Introduce/refine typed immutable runtime MotionEvent/RestEvent plus stable IDs and version identity.

### CHART-03 — Runtime compiler/validator boundary
Create a small runtime/dev validation+compilation boundary from canonical definitions/fixtures to RuntimeChart. Do not implement HH MIDI authoring tool responsibilities.

### CHART-04 — Deterministic timing/order
Precompute runtime event timings; define stable ordering for equal-time events; reject invalid ordering/duplicates/unsupported semantics as appropriate.

### CHART-05 — Sections/phrases/metadata
Carry required structural ranges/markers and identity without allowing metadata to affect gameplay accidentally.

### CHART-06 — Query interface
Expose efficient time-range/unresolved-candidate data access needed by Plan 02 without embedding judgment logic in RuntimeChart.

### REST-01 — Rest semantic model
Represent explicit authored Rest intervals, settling configuration and stillness profile/config.

### REST-02 — RestEvaluator
Evaluate lifecycle against authoritative neck state/history. Produce a deterministic RestOutcome/evidence object without scoring it here.

### REST-03 — Natural Rest separation
Prove that ordinary gaps in MotionEvents do not create implicit scored Rest requirements.

### CHART-07 — M0 fixture migration
Migrate the current playable chart fixture/ScriptableObject path to feed canonical runtime structures or a clearly temporary adapter. Keep the prototype playable.

### CHART-08 — Verification/review
Run chart-content, gameplay-core, qa-guardian and architecture review as applicable; resolve blockers.

## 8. Data/API guidance

Conceptual shape:

```text
RuntimeChart
- SongId
- ChartId
- SchemaVersion
- ChartVersion
- RulesVersion
- Difficulty
- Events (immutable ordered collection)
- Sections
- Phrases
- QueryRange(...)

RuntimeMotionEvent
- Id
- Time/Tick
- Technique
- Trajectory
- Direction
- Modifier
- Duration
- Intensity
- FinisherCandidate

RuntimeRestEvent
- Id
- StartTime/Tick
- Duration
- SettlingDuration/Profile
- StillnessProfile

RestEvaluator
- Begin(rest, currentNeckEvidence)
- Advance(authoritativeTime, neckEvidence)
- Complete() -> RestOutcome
```

Exact APIs may differ. Runtime data must not require mutable ScriptableObject state during a run.

## 9. Validation requirements

Fail clearly for at least:
- missing/duplicate stable event IDs where uniqueness is required;
- unsupported schema/rules semantics;
- invalid event times/durations;
- invalid Rest interval configuration;
- unknown technique/trajectory/modifier values;
- chart/song identity mismatch;
- non-deterministic/ambiguous ordering that cannot be resolved by canonical ordering rules.

Warnings versus hard failures should follow canonical authoring intent: runtime-incompatible data is a hard failure; merely difficult but valid choreography belongs to external authoring validation/review.

## 10. Required tests

### Compilation/immutability
- same canonical input compiles to equivalent RuntimeChart;
- source mutation after compile cannot mutate active RuntimeChart;
- runtime collections/state cannot be casually modified during a run.

### Timing/order
- beat positions map to expected runtime times for current BPM subset;
- long-song conversion does not accumulate event-to-event drift;
- equal-time ordering is stable;
- query range returns deterministic ordered results.

### Identity/version
- song/chart/event IDs preserved;
- schema/chart/rules versions preserved;
- unsupported versions fail before gameplay.

### Validation
Cover malformed IDs, invalid durations/times, unknown semantics and chart/song mismatch with explicit errors.

### Rest fairness
- Rest starts with settling, not immediate stillness punishment;
- incoming momentum remains physically present;
- neck can naturally damp/recover during settling;
- stillness is measured only in evaluation phase;
- changing render FPS does not change RestOutcome for equivalent authoritative neck history;
- Rest never writes/snaps neck state.

### Natural Rest
- an event gap alone creates no RestOutcome/scored stillness requirement.

### Integration
- Plan-02 matcher can query runtime MotionEvents without parsing authoring data;
- resolved/unresolved run state lives in ChartRuntime/run state, not by mutating immutable RuntimeChart definitions.

## 11. Acceptance criteria

- [ ] gameplay consumes immutable/prevalidated RuntimeChart semantics;
- [ ] raw MIDI/authoring parsing is absent from gameplay hot path;
- [ ] runtime events have stable identity/version context;
- [ ] event timing is precomputed/deterministic;
- [ ] RuntimeChart provides efficient deterministic range/candidate access without judging input;
- [ ] MotionEvent and RestEvent are distinct typed semantics;
- [ ] unsupported runtime semantics fail explicitly;
- [ ] Natural Rest remains simple absence of required events;
- [ ] Authored Rest has settling + stillness evaluation phases;
- [ ] RestEvaluator reads neck evidence but never mutates/snaps neck state;
- [ ] equivalent authoritative histories produce equivalent Rest outcomes independent of render FPS;
- [ ] current POC chart can run through the new runtime data path;
- [ ] relevant tests pass;
- [ ] specialist reviews have no unresolved blockers.

## 12. Verification report

Return:
- files/types introduced or migrated;
- chosen runtime time representation and precision rationale;
- validation/failure rules;
- legacy ScriptableObject/MIDI runtime assumptions removed or temporarily adapted;
- Rest lifecycle/configuration;
- tests/results;
- specialist findings;
- out-of-scope issues;
- commit SHAs.

## 13. Deliverables

- immutable RuntimeChart model;
- typed runtime MotionEvent/RestEvent;
- runtime/dev compiler-validator seam;
- deterministic timing/order/query implementation;
- stable identity/version handling;
- RestEvaluator and RestOutcome seam;
- migrated POC fixture;
- automated tests.

## 14. Commit strategy

Suggested:

```text
feat(chart): introduce immutable runtime chart semantics
feat(chart): add validation and deterministic event timing
refactor(chart): migrate POC fixture to runtime chart
feat(rest): implement authored rest evaluation
test(chart): cover runtime validation timing and rest
```

## 15. Do not

- do not build the external chart editor inside Unity;
- do not parse `.hh.mid` in gameplay;
- do not mutate RuntimeChart to track run resolution;
- do not make Rest cancel momentum;
- do not treat every musical gap as Authored Rest;
- do not add scoring/HYPE consequences here;
- do not silently ignore unknown gameplay semantics;
- do not require four difficulty variants per song;
- do not proceed to Plan 04 until Plans 01–03 integrate cleanly.
