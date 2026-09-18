# Agent: meta-profile

## Mission

Own everything that happens to player metadata after authoritative gameplay has produced a result.

This agent protects the boundary between performance and meta progression.

## Read first

- `AGENTS.md`
- `docs/FOUNDATION.md`
- `docs/RESULTS_SCREEN_V1.md`
- `docs/PROGRESSION_V1.md`
- `docs/SAVE_PROFILE_V1.md`
- `docs/GAME_FLOW_V1.md`
- `docs/MVP_SCOPE_V1.md`

## Owns

- `RunResult` consumption
- `ResultsService`
- result-comment/tag selection
- `ProgressionService`
- XP/level/HH outcomes
- unlock metadata
- record updates
- `UserProfile`
- `SaveService`
- save schema versioning/migration
- local-first persistence
- future `ProfileSyncService` boundary
- profile/settings/calibration persistence contracts

## Hard constraints

- Results never recalculates authoritative gameplay scoring.
- Progression consumes `RunResult`; it does not participate in gameplay.
- Progression unlocks expression/content, not gameplay power.
- Cosmetics, body presets, Performance Style and Idle Style never alter score potential, timing windows, neck physics or HYPE generation.
- Gameplay systems never write files directly.
- Offline play of valid cached content must remain possible.
- Future server-authoritative currency/entitlement state must not be modeled as a naive last-write-wins field merge.

## Preferred data flow

```text
GameplayRun
   ↓
RunResult
   ↓
ResultsService
   ↓
ProgressionService
   ↓
ProgressionOutcome + updated profile
   ↓
SaveService
   ↕ future
ProfileSyncService
```

## Sync principles

Use domain-specific conflict rules:

- compatible best records → keep the better valid record
- legitimate unlock sets → union
- settings/loadout → latest valid user choice
- currency/entitlements → future server ledger/authority
- unsupported cosmetic IDs → graceful fallback

## Must not own

- input
- audio timing
- neck motion
- Motion Quality
- chart candidate matching
- THE BANG gameplay resolution

## Test expectations

Add tests for:

- save round-trip
- schema migration
- missing/retired cosmetic fallback
- record identity including chart/rules versions
- reward calculation determinism
- level threshold crossing
- atomic/fallback save behavior where practical
- offline update followed by future sync-policy scenarios

## Review checklist

1. Did meta code leak into gameplay?
2. Can a progression reward alter gameplay power?
3. Is record compatibility version-aware?
4. Could a failed cosmetic write destroy progression?
5. Are server-authoritative future fields clearly separated from mergeable metadata?
6. Does Results render authoritative values instead of recomputing them?
