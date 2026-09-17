# Implementation Plan 07 — POC Real-Device Validation

## Status

P0D — final POC validation gate. This is a validation/fix package, not a feature-expansion milestone.

## 1. Goal

Prove that Headbang Heroes' core mechanic survives actual mobile audio/input latency, render conditions, touch ergonomics, interruptions and sustained play. Editor/Desktop success is insufficient.

## 2. Canonical references

Read: `AGENTS.md`, `docs/FOUNDATION.md`, `docs/MVP_SCOPE_V1.md`, `docs/PRESENTATION_ACCESSIBILITY_V1.md`, `docs/GAMEPLAY_SCREEN_V1.md`, `docs/GAMEPLAY_UX_V1.md`, `docs/TECHNICAL_CONTRACTS_V1.md`, `docs/ROADMAP.md` P0D and all accepted implementation plans 01–06.

Primary specialist: `agents/qa-guardian.md`. Support: `agents/rhythm-audio.md`, `agents/gameplay-core.md`, `agents/presentation-avatar.md`.

## 3. Scope

- iOS real-device validation mandatory for POC;
- Android real-device validation when hardware is available, otherwise explicitly record as pending before broader MVP claims;
- timing diagnostics using signed timing error;
- audio/input/visual calibration behavior;
- 30/60/90/120 Hz render conditions where hardware/test harness permits;
- deliberate frame hitch during cue approach;
- repeated pause/resume;
- repeated retry;
- large positive/negative calibration offsets;
- left/right thumb reach and lower-playfield comfort;
- cardinal input ambiguity;
- CURRENT/NEXT readability;
- HYPE control reach/readability;
- haptic usefulness/noise;
- reduced flash/shake behavior;
- thermal/performance observation during sustained play;
- regression fixes strictly necessary to pass POC contracts.

## 4. Out of scope

No new modes, content expansion, final optimization campaign, device-lab matrix, backend, store compliance, monetization, art overhaul or speculative UX redesign unrelated to observed device defects.

## 5. Validation principle

Do not tune around one lucky phone by corrupting canonical semantics. Device-specific latency belongs in calibration/platform adaptation, not chart mutation or hidden judgment-window widening.

Every discovered defect must be classified as one of:
- gameplay-domain defect;
- audio/time-domain defect;
- input mapping/latency defect;
- presentation/readability defect;
- performance/device defect;
- calibration/configuration defect;
- non-blocking polish.

Fix the owning layer.

## 6. Test matrix

For each available device record model/OS/build and test at minimum:

```text
A. full song clean run
B. full song intentionally mixed timing
C. 10 repeated retries
D. 10 pause/resume cycles across runs
E. deliberate frame hitch near cue
F. calibration: zero / large positive / large negative
G. rapid alternating spam
H. early inversion / late inversion
I. THE BANG + Finisher path
J. reduced flash/shake + haptics off/on
K. sustained session / thermal observation
```

Where refresh-rate control exists, repeat representative timing cases at available 30/60/90/120 Hz modes.

## 7. Validation tasks

### DEV-01 — Diagnostic build
Expose development-only diagnostics for authoritative song time, signed timing error, current calibration, render FPS/frame hitch and enough event identity to investigate failures. Do not ship debug clutter into normal presentation.

### DEV-02 — iOS baseline
Run full matrix on at least one physical iPhone. Capture concrete failures/reproduction steps.

### DEV-03 — Android baseline
Run on physical Android when available; otherwise create explicit pending gate item rather than assuming parity.

### DEV-04 — Refresh/frame pacing
Verify authoritative outcomes remain stable under available refresh caps/variable pacing and induced hitch. Visual smoothness may degrade; authored timing must not shift.

### DEV-05 — Pause/retry soak
Repeated pause/resume/retry must not accumulate drift, stale candidates, stale HYPE, duplicate results or stale cues.

### DEV-06 — Calibration extremes
Confirm offsets shift alignment exactly as designed and are persisted. They must not alter chart event definitions or widen timing windows.

### DEV-07 — Ergonomics
Validate portrait thumb regions, left/right reach, UP/DOWN ambiguity, HYPE tap, face/neck visibility and sustained comfort.

### DEV-08 — Haptics/accessibility
Judge whether haptics help without becoming noise; verify disabling them has no gameplay consequence. Verify reduced flash/shake retain information and scoring potential.

### DEV-09 — Performance/thermal
Observe sustained FPS/frame pacing, GC spikes, audio continuity and thermal behavior. Optimize only POC-blocking problems.

### DEV-10 — Adversarial QA
QA Guardian attempts to break timing/control contracts using edge sequences and interruption patterns.

### DEV-11 — POC feel gate
After technical defects are addressed, play the complete song repeatedly without progression motivation and explicitly evaluate the north-star question: is controlling the neck itself satisfying enough to replay?

## 8. Required evidence

Keep a concise validation record containing:
- build/commit SHA;
- device + OS;
- refresh condition where known;
- calibration values;
- pass/fail per matrix case;
- observed signed timing anomalies;
- reproduction steps for failures;
- fix commit where applicable;
- remaining limitations.

A markdown validation report under `docs/validation/` is sufficient for POC. Do not build telemetry infrastructure solely for this gate.

## 9. Acceptance criteria

- [ ] full POC song remains synchronized start-to-finish on physical iOS device;
- [ ] frame pacing/hitch does not shift authoritative event timing;
- [ ] available refresh-rate changes do not materially change authoritative results for equivalent scripted inputs;
- [ ] pause/resume does not create accumulating desync;
- [ ] 10 repeated retries start cleanly without stale state;
- [ ] calibration offsets behave predictably and persist;
- [ ] input always produces physical neck response;
- [ ] spam remains physically accepted but produces poor motion evidence naturally;
- [ ] MISS never snaps/freezes/resets neck;
- [ ] cardinal controls and HYPE activation are usable on a real phone;
- [ ] CURRENT/NEXT remain readable without hiding avatar/head;
- [ ] haptics are optional and non-authoritative;
- [ ] accessibility reductions preserve scoring potential;
- [ ] no POC-blocking thermal/performance/audio defect appears in sustained test;
- [ ] Android status is explicitly tested or recorded pending;
- [ ] QA Guardian has no unresolved POC-blocking finding;
- [ ] human feel gate concludes the neck mechanic is sufficiently enjoyable to justify P1/MVP work.

## 10. Failure policy

If a gate fails, return to the owning implementation package and fix the smallest root cause. Add a regression test where technically possible, then rerun the affected device cases plus the baseline full-song case.

Do not waive deterministic/timing defects as “mobile weirdness”. Do not compensate for systematic latency by editing chart event times per device.

## 11. Verification report

At completion provide device matrix, build SHA, calibration used, pass/fail evidence, defects/fixes, regression tests, QA findings, Android status, remaining non-blocking polish and explicit POC gate result.

## 12. Commit strategy

Validation documentation and fixes should remain attributable, e.g.:

```text
test(device): add poc timing diagnostics
fix(audio): correct pause resume clock reanchor
fix(input): correct mobile timestamp conversion
fix(ui): improve cardinal cue readability on phone
docs(validation): record poc device matrix
```

Do not combine unrelated polish discovered during device testing.

## 13. Do not

Do not declare POC from Editor testing; do not widen windows to hide latency; do not mutate chart timing per phone; do not add features during validation; do not ignore repeat/retry drift; do not make haptics required; do not move to P1 merely because the software is technically stable if the core headbang still is not fun.
