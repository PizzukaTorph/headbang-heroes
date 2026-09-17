# Implementation Plan 04 — Scoring, HYPE, THE BANG & Finisher

## Status

P0A / Work package 4. Execute only after Plans 01–03 are accepted and integrated.

## 1. Goal

Turn authoritative event resolution into deterministic `EventOutcome` and run state: score, combo, multiplier, HYPE, THE BANG, Finisher and authoritative `RunResult` data, without creating circular dependencies or allowing presentation to affect gameplay.

## 2. Canonical references

Read before implementation:
- `AGENTS.md`
- `docs/FOUNDATION.md`
- `docs/TECHNICAL_CONTRACTS_V1.md`
- `docs/SCORING_SYSTEM_V1.md`
- `docs/SONG_CHART_MODEL_V1.md`
- `docs/RESULTS_SCREEN_V1.md` for RunResult consumer requirements only
- `docs/CONSOLIDATION_REVIEW_2026-09-16.md`
- `docs/ROADMAP.md` P0A WP4

Specialists:
- `agents/gameplay-core.md` — primary
- `agents/qa-guardian.md`
- `agents/architecture-guardian.md` for dependency review
- `agents/chart-content.md` for Finisher-candidate semantics where needed

## 3. Current-state investigation

Inspect:
- current judgment/score/combo implementation;
- Motion Quality calculation/output after Plan 01;
- technique validation state;
- current multiplier behavior;
- any existing HYPE/Special Bang code;
- result/stat accumulation;
- current HUD dependencies on mutable score state;
- constants scattered in code;
- tests around score/combo/judgment.

Identify legacy semantics that conflict with `SCORING_SYSTEM_V1.md`.

## 4. Scope

- authoritative `EventOutcome` assembly from already-resolved timing + physical/semantic evidence;
- Motion Quality evaluator integration using pre-inversion event-local evidence;
- technique/context factor seam;
- parameterized event scoring;
- canonical combo rules;
- parameterized multiplier progression;
- canonical base HYPE generation;
- HypeSystem ownership of HYPE, READY, THE BANG and Finisher state;
- manual THE BANG activation only when ready;
- configurable THE BANG duration/reward modifiers;
- deterministic Finisher resolution on authored candidate events;
- prevention of recursive THE BANG -> amplified HYPE -> immediate refill loops;
- authoritative run statistics;
- immutable/final `RunResult` assembly for downstream Results;
- tests for deterministic outcomes and state transitions.

## 5. Out of scope

Do not implement:
- Results screen visual layout (Plan 06);
- XP/HH progression/rewards (Plan 06);
- save/profile persistence (Plan 06);
- THE BANG VFX/hair/body presentation (Plan 05);
- final balancing of open tuning values;
- telemetry backend;
- new Finisher QTE/input language;
- chart authoring/editor changes;
- monetization/economy.

## 6. Required architecture

### EventOutcome

Build one authoritative outcome per resolved scoring event from separable dimensions. Conceptually:

```text
EventOutcome
- EventId
- TimingJudgment + signed timing error
- MotionQuality result/evidence summary
- Technique/context result
- Finisher context/result
- score contribution
- combo/multiplier transition
- HYPE contribution
```

Do not let UI reconstruct this later.

### Scoring separation

Conceptual score:

```text
BaseScore
× TimingFactor
× MotionQualityFactor
× TechniqueFactor
× CurrentMultiplier
× TheBangModifier
```

Exact factors are configuration. Keep dimensions separately inspectable.

### Combo semantics

Canonical:
- PERFECT -> combo +1
- GREAT -> combo +1
- GOOD -> ends current combo
- MISS -> resets combo to zero

Track longest combo. Do not silently redefine GOOD to maintain combo.

### Multiplier semantics

Successful GOOD/GREAT/PERFECT advance multiplier progress; MISS resets multiplier state/progress. Thresholds/max are configurable.

### HYPE semantics

Base confirmed contribution:
- PERFECT +2
- GREAT +1
- GOOD +0
- MISS +0

MISS does not remove accumulated HYPE.

Additional Motion Quality/technique contributions may remain zero/configurable until playtesting justifies them.

### THE BANG

At HYPE max -> READY. Player explicitly activates THE BANG. Activation opens a temporary enhanced window while normal gameplay continues.

Prototype reward multiplier defaults may begin at documented 10x but must be configuration, not magic constants.

HYPE generation during THE BANG must be explicitly configured (normal or disabled initially) and must **not** accidentally receive THE BANG reward amplification.

### Finisher

`finisherCandidate` marks an ordinary authored MotionEvent as suitable. It is not a new input.

During active THE BANG, deterministic rules decide whether a suitable successfully performed candidate becomes the Finisher. Start with the simplest canonical predictable rule supported by the spec/configuration; do not invent cinematic QTE behavior.

Snapshot current HYPE/THE BANG/Finisher reward context before applying outcome rewards so scoring and HYPE cannot recursively change the context of the same event.

### RunResult

`RunResult` is authoritative output, not a request for Results UI to recalculate gameplay. It must contain sufficient stats for later Results/grade/progression consumers and stable song/chart/rules identity.

## 7. Implementation tasks

### SCORE-01 — Audit and config
Map current scoring state and move semantic/tuning constants into explicit config.

### SCORE-02 — MotionQualityEvaluator integration
Consume Plan-01 pre-inversion local evidence and produce a deterministic quality result. Preserve setup/unprepared first-bang semantics. Do not use presentation state.

### SCORE-03 — EventOutcome
Create authoritative separable event outcome assembly after timing/technique/context resolution.

### SCORE-04 — Score/combo/multiplier
Implement canonical combo and multiplier state transitions plus parameterized event score.

### HYPE-01 — HypeSystem
Introduce isolated HYPE ownership: accumulation, max/READY, activation request, active window, expiry.

### HYPE-02 — THE BANG context snapshot
Ensure event reward context is captured before outcome application; prevent circular Scoring/HYPE dependency.

### FIN-01 — Finisher resolution
Resolve authored `finisherCandidate` deterministically during THE BANG using normal event execution and configuration.

### RUN-01 — Run statistics
Accumulate judgment counts, score, longest combo, multiplier metrics, total HYPE generated, THE BANG activations, Finishers and other canonical result inputs.

### RUN-02 — RunResultBuilder
Produce final authoritative immutable/value-like RunResult with song/chart/version/rules identity.

### SCORE-05 — Presentation event seam
Emit semantic state/outcome notifications downstream as needed, without making presentation authoritative or introducing a generic global event bus.

### SCORE-06 — Verification/review
Run gameplay-core, qa-guardian and architecture review; resolve blockers.

## 8. Data/API guidance

Conceptual types:

```text
ScoringConfig
- BaseScore
- timing factors
- MotionQuality factors/curve
- technique factors
- multiplier thresholds/max

HypeConfig
- HYPE per judgment
- max HYPE
- THE BANG duration
- reward multipliers
- HYPE behavior while active
- Finisher eligibility thresholds/rules

EventOutcome
RunScoringState
HypeState
FinisherOutcome
RunResult
```

`ScoringSystem` should not own audio time or chart matching. `HypeSystem` should not recalculate score. `RunResultBuilder` should aggregate authoritative outcomes/state, not reinterpret them.

## 9. Required tests

### Event score determinism
Same event inputs/context -> identical EventOutcome/score contribution across repeated runs and render rates.

### Dimension separation
- PERFECT timing + weak motion remains PERFECT timing with weak MotionQuality;
- GREAT timing + strong motion remains GREAT timing with strong MotionQuality;
- no dimension rewrites another dimension's label.

### Combo
Test exact PERFECT/GREAT/GOOD/MISS transitions, longest combo and reset/end semantics.

### Multiplier
Test threshold advancement, GOOD contribution, MISS reset and configurable max.

### HYPE
- PERFECT +2 default;
- GREAT +1 default;
- GOOD/MISS +0 default;
- MISS preserves accumulated HYPE;
- max enters READY;
- activation below READY fails without side effects;
- valid activation starts THE BANG;
- expiry returns to normal state.

### Circularity guard
With large THE BANG score multiplier:
- HYPE contribution for the same event is not multiplied accidentally;
- THE BANG cannot immediately self-refill due to its reward multiplier;
- context used for an event cannot change halfway through resolving that same event.

### Finisher
- candidate outside THE BANG does not Finish;
- non-candidate during THE BANG does not Finish unless explicit config says otherwise;
- eligible candidate + qualifying execution during THE BANG resolves deterministically;
- same event cannot Finisher twice;
- Finisher uses existing event execution/no extra QTE.

### RunResult
- totals equal sum/state of authoritative outcomes;
- judgment counts/longest combo/HYPE/Finishers correct;
- identity/version fields preserved;
- building Results data does not recalculate score.

## 10. Acceptance criteria

- [ ] authoritative EventOutcome exists and preserves separate Timing/Motion/Technique/context signals;
- [ ] scoring constants are parameterized;
- [ ] combo follows canonical PERFECT/GREAT/GOOD/MISS semantics;
- [ ] multiplier is configurable and MISS-resettable;
- [ ] HYPE base generation matches canonical defaults;
- [ ] MISS does not erase HYPE;
- [ ] THE BANG is manually activated from READY and has configurable duration/rewards;
- [ ] THE BANG reward amplification cannot recursively amplify same-event HYPE context;
- [ ] Finisher resolves deterministically from authored candidate + normal gameplay execution;
- [ ] no separate Finisher QTE/input language exists;
- [ ] RunResult is authoritative and sufficient for Results consumers;
- [ ] presentation is downstream only;
- [ ] repeated equivalent runs produce equivalent outcomes;
- [ ] tests pass and specialist reviews have no blockers.

## 11. Verification report

Return:
- files changed;
- final EventOutcome/RunResult shape;
- config/default values used;
- Motion Quality integration method;
- combo/multiplier/HYPE state-transition evidence;
- Finisher deterministic rule selected and why it is canonical/predictable;
- circular-dependency guard implementation;
- tests/results;
- specialist findings;
- open tuning questions left intentionally configurable;
- commit SHAs.

## 12. Deliverables

- ScoringConfig/HypeConfig or equivalent;
- deterministic MotionQuality evaluation integration;
- EventOutcome;
- ScoringSystem combo/multiplier state;
- HypeSystem + THE BANG lifecycle;
- deterministic Finisher resolution;
- RunResultBuilder/RunResult;
- automated tests.

## 13. Commit strategy

Suggested:

```text
feat(scoring): add authoritative event outcomes
feat(scoring): implement combo and multiplier state
feat(hype): add HYPE and THE BANG lifecycle
feat(hype): add deterministic finisher resolution
feat(results): build authoritative run result data
test(gameplay): cover scoring hype and finisher contracts
```

## 14. Do not

- do not merge Timing and Motion Quality into one opaque judgment;
- do not let GOOD maintain combo;
- do not make MISS erase HYPE;
- do not hardcode tuning constants across gameplay classes;
- do not let THE BANG reward multiplier refill itself recursively;
- do not create a Finisher QTE;
- do not let Results UI recompute score;
- do not let presentation state affect scoring;
- do not implement progression/save/UI here;
- do not proceed to Plan 05 automatically.
