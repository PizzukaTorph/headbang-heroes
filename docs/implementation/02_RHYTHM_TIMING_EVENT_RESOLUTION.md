# Implementation Plan 02 — Rhythm Timing & Event Resolution

## Status

P0A / Work package 2. Execute after Implementation Plan 01 is accepted.

## 1. Goal

Make DSP-derived `AudioClock` song-time the single authoritative time domain for gameplay and replace M0 single-active-event assumptions with deterministic bounded candidate matching.

The result must let input affect the neck immediately while independently resolving whether that input consumes, hits or misses an authored event.

## 2. Canonical references

Read before implementation:
- `AGENTS.md`
- `docs/FOUNDATION.md`
- `docs/TECHNICAL_CONTRACTS_V1.md`
- `docs/SONG_CHART_MODEL_V1.md`
- `docs/SCORING_SYSTEM_V1.md` judgment semantics only
- `docs/GAMEPLAY_UX_V1.md`
- `docs/CONSOLIDATION_REVIEW_2026-09-16.md`
- `docs/ROADMAP.md` P0A WP2

Specialists:
- `agents/rhythm-audio.md` — primary timing authority
- `agents/gameplay-core.md`
- `agents/chart-content.md`
- `agents/qa-guardian.md`

## 3. Current-state investigation

Before editing, inspect:
- current audio start/scheduling and DSP use;
- song-time calculation;
- pause/resume/retry re-anchoring;
- calibration/offset handling;
- touch/Input System timestamp path;
- scheduler/current-event logic;
- cue scheduler dependencies;
- judgment window constants/config;
- event expiration/MISS logic;
- tests around timing, pause and scheduler.

Record assumptions that are M0-only and remove them where they conflict with canonical contracts.

## 4. Scope

- one authoritative `AudioClock`/song-time contract;
- scheduled playback preserved/strengthened;
- pause/resume/retry clock re-anchoring;
- calibration applied in one explicit place;
- physical/device input timestamps mapped into authoritative song-time before gameplay resolution;
- semantic `BangInput` carrying song-time;
- data-driven timing-window configuration;
- bounded lookup among unresolved candidate MotionEvents;
- deterministic candidate selection;
- explicit too-early / compatible / wrong-semantic / late-expired semantics;
- deterministic event-resolution ordering consistent with `FOUNDATION.md`;
- separation of TimingJudgment from Motion Quality;
- CURRENT/NEXT scheduler consuming chronology without defining judgment time;
- tests for dense/overlapping windows and clock lifecycle.

## 5. Out of scope

Do not implement:
- final RuntimeChart compiler/authoring importer (Plan 03);
- Authored Rest evaluation (Plan 03);
- scoring/combo/HYPE/THE BANG/Finisher (Plan 04);
- new cue art/presentation (Plan 05);
- external HH MIDI tooling;
- new gameplay techniques;
- save/profile/backend.

Temporary runtime fixtures/adapters are allowed until Plan 03 provides the final-shaped runtime chart.

## 6. Required architecture

### Time ownership

`AudioClock` owns authoritative song time. Judgment must not derive authority from `Time.time`, `Time.deltaTime`, animation/coroutine completion, frame count or cue animation.

Conceptually:

```text
songTime = dspTime - anchorDspTime + songStartOffset + calibration
```

Exact representation may differ, but offset/calibration ownership must be explicit and testable.

### Input normalization

Raw input timestamp -> canonical song-time must occur before candidate matching/judgment.

Conceptual domain input:

```text
BangInput
- semantic action/direction
- songTime
- optional diagnostic source timestamp/device metadata
```

Never compare unrelated clock domains directly.

### Resolution ordering

Preserve canonical order relevant to this package:

```text
physical input
→ semantic BangInput
→ canonical song-time
→ capture pre-inversion neck evidence
→ apply neck input immediately
→ candidate lookup
→ TimingJudgment
→ later MotionQuality/technique/context systems
```

Candidate failure must never undo the already-applied physical neck input.

### Candidate matching

Do not maintain “the next array item is the target” as gameplay law.

Search a bounded set of unresolved events around input song-time. Selection must be deterministic and documented. Consider timing proximity, eligibility and semantic compatibility without making semantic mismatch disappear.

Canonical outcomes:
- too early for all eligible windows -> no event consumed; neck still moves;
- compatible action in eligible window -> best candidate resolves;
- wrong semantic action inside best eligible event window -> consume that event as MISS unless the technique contract explicitly says otherwise;
- unresolved event passing its late boundary -> expires as MISS;
- one authored event resolves at most once.

Tie-breaking must be stable and test-covered, preferably using authored time then stable event identity/index after semantic/timing criteria.

### Cue scheduler

CURRENT/NEXT is presentation scheduling derived from `ChartRuntime + AudioClock`. Cue animation cannot shift event time or judgment windows.

## 7. Implementation tasks

### TIME-01 — Audit clocks and lifecycle
Characterize DSP/audio/input/chart clocks, scheduling, offsets, pause/resume/retry and current tests.

### TIME-02 — Canonical AudioClock
Create/refine the single song-time API; centralize anchor/offset/calibration; ensure scheduled start and explicit lifecycle transitions.

### TIME-03 — Input time-domain conversion
Convert raw input timestamp to song-time as early as practical; make domain `BangInput` independent of raw Input System time.

### TIME-04 — TimingConfig
Move judgment/candidate windows from scattered constants into explicit configuration/data. Preserve current values only as initial tuning defaults.

### TIME-05 — Unresolved candidate set
Replace permanent single-active-event assumptions with bounded unresolved-event lookup suitable for dense charts.

### TIME-06 — Deterministic matcher
Implement stable selection and consumption semantics for compatible, wrong, too-early and expired inputs/events.

### TIME-07 — Resolution pipeline
Wire pre-inversion snapshot + immediate neck application from Plan 01 to candidate/judgment resolution without introducing scoring/HYPE.

### TIME-08 — Cue boundary
Ensure CURRENT/NEXT observes authoritative chronology/time but does not own judgment state.

### TIME-09 — Lifecycle robustness
Clear/re-anchor candidates and clocks correctly on start, pause/resume and retry; prevent stale resolution state.

### TIME-10 — Verification/review
Run tests plus rhythm-audio, gameplay-core, chart-content and qa-guardian review; resolve package-blocking findings.

## 8. Data/API guidance

Conceptual APIs:

```text
AudioClock
- SongTime
- ScheduleStart(...)
- Pause()
- Resume()
- Restart(...)
- ToSongTime(inputTimestamp)

TimingConfig
- PerfectWindow
- GreatWindow
- GoodWindow
- candidate/early/late bounds as needed

ChartRuntime (temporary/final interface)
- GetUnresolvedCandidates(time, bounds)
- MarkResolved(eventId, resolution)
- ExpireEvents(upToTime)

EventMatcher
- Match(BangInput, candidates, TimingConfig)

TimingJudgment
- PERFECT | GREAT | GOOD | MISS
```

Keep matching/judgment domain-testable without UI/audio playback objects by injecting clock values/fixtures where appropriate.

## 9. Migration strategy

1. establish clock characterization tests;
2. centralize song-time authority without changing chart storage yet;
3. normalize input timestamps;
4. introduce configurable timing windows;
5. place candidate-query abstraction in front of legacy chart data;
6. implement deterministic matcher;
7. migrate scheduler/event consumption;
8. migrate CURRENT/NEXT to observer role;
9. remove obsolete single-active-event judgment authority;
10. leave the abstraction ready for Plan 03 RuntimeChart replacement.

## 10. Required tests

At minimum:

### Clock
- scheduled start has deterministic zero/reference;
- pause freezes logical song progress;
- resume continues without jump/drift;
- retry establishes a clean new anchor;
- calibration offset is applied exactly once;
- repeated pause/resume does not accumulate material drift.

### Input time conversion
- known raw/input timestamps map to expected song-time;
- no judgment path compares raw device time directly to chart time.

### Judgment boundaries
For every configured PERFECT/GREAT/GOOD/MISS edge, test exact boundary and immediately inside/outside behavior. Signed early/late error remains inspectable.

### Dense candidates
- two overlapping eligible windows resolve deterministically;
- nearest compatible candidate wins according to documented policy;
- stable tie-break behavior;
- already-resolved events cannot be consumed again.

### Early/wrong/expired
- too-early input moves neck but consumes nothing;
- wrong semantic input in best eligible window consumes MISS;
- late unresolved event expires exactly once;
- subsequent input cannot resurrect expired event.

### Lifecycle
- pause/resume with unresolved candidates creates no stale/duplicate judgments;
- retry clears prior run resolution state;
- frame hitch/cue delay does not move authored judgment time.

## 11. Acceptance criteria

- [ ] one canonical DSP-derived song-time API owns judgment time;
- [ ] raw input timestamps are normalized before matching;
- [ ] timing windows are data-driven;
- [ ] no permanent single-active-event assumption remains in authoritative matching;
- [ ] overlapping windows resolve deterministically;
- [ ] early input can move neck without consuming future event;
- [ ] wrong semantic action inside eligible best window follows canonical consumed-MISS rule;
- [ ] expired events MISS once;
- [ ] TimingJudgment remains separate from Motion Quality;
- [ ] CURRENT/NEXT presentation cannot alter authored judgment time;
- [ ] pause/resume/retry do not create clock drift/stale candidates;
- [ ] relevant automated tests pass;
- [ ] specialist reviews have no unresolved blockers.

## 12. Verification report

Return:
- files changed;
- clock/time-domain decisions;
- candidate-selection/tie-break policy;
- legacy assumptions removed;
- tests/results including boundary and dense-event cases;
- pause/resume/retry verification;
- specialist review findings;
- known out-of-scope issues;
- commit SHAs.

## 13. Deliverables

- authoritative AudioClock/time conversion;
- semantic timestamped BangInput path;
- TimingConfig;
- deterministic bounded matcher/resolution state;
- scheduler/cue boundary cleanup;
- timing/dense-event/lifecycle tests.

## 14. Commit strategy

Suggested sequence:

```text
refactor(timing): centralize authoritative song clock
refactor(input): normalize bang input to song time
refactor(timing): make judgment windows configurable
refactor(chart): add bounded deterministic event matching
test(timing): cover clock lifecycle and dense candidates
```

## 15. Do not

- do not let visuals/coroutines define timing;
- do not compare raw device and chart clocks;
- do not reject physical neck input because no event matched;
- do not silently skip wrong semantic input to find an easier later event;
- do not implement score/HYPE here;
- do not parse MIDI in gameplay;
- do not overbuild the Plan-03 RuntimeChart prematurely;
- do not proceed to Plan 04 automatically.
