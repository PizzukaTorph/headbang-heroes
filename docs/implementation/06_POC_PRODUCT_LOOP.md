# Implementation Plan 06 — POC Product Loop

## Status

P0C — POC Product Loop. Execute after core and representative presentation are accepted.

## 1. Goal

Turn the proven gameplay core into one complete, repeatable song experience:

```text
Home → Song Select → Pre-song → Gameplay → Results → Retry / Continue
```

A run must start, finish, produce authoritative results, update local profile state and retry without manual repair.

## 2. Canonical references

Read: `AGENTS.md`, `docs/FOUNDATION.md`, `docs/GAME_FLOW_V1.md`, `docs/MVP_SCOPE_V1.md`, `docs/RESULTS_SCREEN_V1.md`, `docs/SAVE_PROFILE_V1.md`, `docs/PROGRESSION_V1.md`, `docs/PRESENTATION_ACCESSIBILITY_V1.md`, `docs/TECHNICAL_CONTRACTS_V1.md`, `docs/ROADMAP.md` P0C.

Specialists: `agents/meta-profile.md` primary, `agents/presentation-avatar.md`, `agents/gameplay-core.md`, `agents/qa-guardian.md`.

## 3. Current-state investigation

Inspect current bootstrap/scene flow, song launch path, result/debug screens, run completion, retry/reset behavior, existing save data, PlayerPrefs/serialization use, progression placeholders, calibration/settings and prototype assumptions requiring Editor intervention.

## 4. Scope

- minimal Boot/Home;
- Song Select for available POC content without assuming four difficulties;
- brief Pre-song confirmation;
- launch canonical GameplayRun;
- end-of-song authoritative RunResult handoff;
- ResultsService/Results presentation consuming rather than recalculating RunResult;
- S/A/B/C/D grade computation through configured authoritative results service where canonical implementation requires it;
- local best record by song/chart/chartVersion/rulesVersion;
- minimal XP/level/HH progression integration for loop validation;
- local UserProfile + SaveService round-trip;
- persisted settings/accessibility/calibration required by POC;
- one-tap Retry of same song/chart/avatar/settings with clean run state and deterministic preroll;
- Continue back toward selection/home;
- offline operation for valid local profile/content.

## 5. Out of scope

No account/backend sync, production currency ledger, shop, ads, Versus, Story campaign, Endurance, advanced Practice, community/mod browser, CMS, leaderboards, large avatar creator, production onboarding, economy optimization or four-song MVP content expansion.

## 6. Required architecture

Gameplay ends in `RunResult`. `ResultsService` consumes it. `ProgressionService` consumes validated result/grade/reward context. `SaveService` persists profile state. Gameplay systems do not write files or progression directly.

Save data describes player state, not catalog data. Profile records must carry songId/chartId/chartVersion/rulesVersion compatibility identity.

Local-first POC authority is acceptable. Keep boundaries so future sync can sit behind profile services without gameplay changes.

## 7. Implementation tasks

### LOOP-01 — Flow audit
Map scene/state transitions and remove manual/debug-only dependencies from the normal POC path.

### LOOP-02 — Boot/Home
Load/validate/migrate local profile, initialize required services/content, then present PLAY-dominant Home. Network must not be required for valid local POC.

### LOOP-03 — Song Select
List available songs/charts with authored difficulty; never fabricate Easy/Normal/Hard/Extreme slots.

### LOOP-04 — Pre-song
Confirm selected song/chart/difficulty/avatar and content readiness; START is primary.

### LOOP-05 — Run handoff
Create run from validated loaded song/runtime chart and transition to gameplay with clean authoritative state.

### LOOP-06 — Completion/RunResult
End run once, freeze authoritative result data, hand it to ResultsService.

### LOOP-07 — Results
Display canonical performance report data and grade without recomputing gameplay score. Provide Retry and Continue prominently.

### LOOP-08 — Progression
Apply minimal configured XP/HH progression after valid run result. Keep expression/content-only principle; no gameplay power.

### LOOP-09 — Records
Update compatible per-chart best record only according to explicit record semantics/version identity.

### LOOP-10 — SaveService
Persist profile atomically/recoverably where practical; schema-version and deterministic migration/validation seam required.

### LOOP-11 — Settings/calibration
Persist music volume, haptics, reduced flash/shake and audio/input/visual offsets needed by current POC. Calibration must not widen judgment windows.

### LOOP-12 — Retry
One tap restarts same song/chart/config with fresh AudioClock anchor, fresh ChartRuntime resolution, fresh neck/scoring/HYPE state and no stale presentation state.

### LOOP-13 — Verification/review
Meta-profile, gameplay, presentation and QA reviews plus repeated loop soak test.

## 8. Data/API guidance

Conceptual boundaries:

```text
RunResult -> ResultsService -> ProgressionService -> UserProfile -> SaveService
```

Use explicit `UserProfile`, `ChartRecord`, settings/calibration and save schema version. Do not expose serialization/filesystem operations to gameplay-domain classes.

## 9. Required tests

- clean install/no save creates valid profile/defaults;
- supported old schema migrates deterministically;
- malformed save fails safely/recovery strategy preserves usable profile where possible;
- completing a run creates exactly one result/progression application;
- Results values equal authoritative RunResult;
- compatible better record replaces previous; worse does not; incompatible chart/rules version is not blindly compared;
- save/reload preserves progression, record, settings, calibration and avatar selections in scope;
- Retry clears all per-run state but preserves selected setup/profile;
- 10+ repeated retries produce no stale judgments/HYPE/cues/audio drift;
- offline boot/play/results/retry work with local content/profile;
- calibration persistence does not modify authored chart or window width.

## 10. Acceptance criteria

- [ ] full Home → Song Select → Pre-song → Gameplay → Results flow works without Editor repair;
- [ ] one complete song finishes into authoritative Results;
- [ ] Results do not recalculate score;
- [ ] local record/progression update once per valid completion;
- [ ] profile survives app restart;
- [ ] settings/calibration survive restart;
- [ ] no four-difficulty assumption exists;
- [ ] Retry is one tap and starts a clean equivalent run;
- [ ] repeated retry does not accumulate stale state/drift;
- [ ] valid local content/profile remain playable offline;
- [ ] progression grants no gameplay power;
- [ ] tests/reviews have no blockers.

## 11. Verification report

Return flow implemented, files/services changed, save schema/migrations, reward defaults, record compatibility rule, retry reset inventory, repeated-retry results, offline check, tests, specialist findings, deferred product features and commit SHAs.

## 12. Commit strategy

```text
feat(flow): add minimal poc navigation loop
feat(results): consume authoritative run results
feat(profile): add local versioned profile persistence
feat(progression): integrate poc rewards and records
feat(settings): persist calibration and accessibility
feat(flow): implement clean one-tap retry
test(poc): cover persistence and repeated run loop
```

## 13. Do not

Do not build the shop/backend/campaign; do not let gameplay write save files; do not recalculate score in Results; do not compare incompatible records blindly; do not make progression stronger neck physics; do not require network for cached/local POC; do not turn the shell into the game; do not proceed to MVP expansion before Plan 07/device gate.
