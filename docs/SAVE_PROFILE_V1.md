# Save / Profile Data v1

## Goal

Persist user metadata required to reconstruct identity, progression, customization, records, settings, accessibility, and calibration.

> **Save data describes the player, not the song catalog.**

## High-level model

```text
UserProfile
├── identity
├── progression
├── avatar
├── unlocks / entitlements
├── records
├── settings / accessibility
├── calibration
└── schema/version metadata
```

Conceptual fields:

```text
playerId
displayName

level
xp
hhCurrency

avatarSelections
performanceStyleId
idleStyleId

ownedCosmetics[]
unlockedContent[]

records[]
  songId
  chartId
  chartVersion
  rulesVersion
  bestScore
  bestGrade
  longestCombo
  bestFinisherCount

settings
  musicVolume
  hapticsEnabled
  hapticIntensity
  reducedFlash
  reducedShake
  highContrastCues?

calibration
  audioOffsetMs
  inputOffsetMs
  visualOffsetMs

saveSchemaVersion
updatedAt
```

Exact serialization is implementation detail.

## Local-first behavior

The game must remain useful offline.

For POC/MVP:

```text
LOCAL SAVE = immediate authority for local play
SERVER = optional future sync / validation / economy authority by domain
```

Network outage must not block:
- launching with valid local profile
- playing cached songs
- earning local run results
- changing avatar/settings
- retrying

Pending synchronization can occur later.

## Do not use one generic sync rule

Different profile domains need different conflict/authority policies.

### Records

Compatible records reference:

```text
songId
chartId
chartVersion
rulesVersion
```

When both sides contain valid compatible records, preserve the better result according to explicit record semantics.

Do not compare scores across incompatible chart/rules versions as if identical.

### Unlocks

Legitimate non-consumable unlock sets may union when both sides are trusted/valid.

### Settings / avatar loadout

Use latest valid user choice where appropriate.

Missing/retired cosmetic IDs must fall back gracefully without invalidating the profile.

### Currency / purchases / entitlements

Once a production online economy exists, HH balance and paid entitlements must not be treated as ordinary last-write-wins fields.

Preferred production direction:
- server-authoritative transaction/ledger model
- client may show/use validated cached state offline according to product policy
- reconciliation is explicit
- no blind union/max/latest merge for currency

POC/MVP do not require the production ledger backend.

## Local provisional progression

Before server economy exists, XP/HH can be stored locally for MVP progression testing.

This is a product-development stage, not a claim that future competitive/economic state will trust arbitrary client values.

Keep the save schema and services structured so authority can move behind `ProfileSyncService` without gameplay systems knowing.

## Avatar persistence

Persist semantic selections, not baked images:

```text
presentation/body preset
faceId
hairId
beardId
makeupId
piercingIds[]
tattooIds[]
apparelIds[]
specialIds[]
performanceStyleId
idleStyleId
```

Body/avatar choices remain presentation-only.

## Settings vs scoring

Accessibility presentation options must not change scoring potential.

Calibration is different: it aligns perception/input timing and therefore is stored explicitly and auditable.

Calibration must not silently widen timing windows.

## Versioning and migration

Every profile has `saveSchemaVersion`.

Load flow:

```text
read
→ identify schema version
→ migrate forward if supported
→ validate
→ load
```

Migration should be deterministic, monotonic where practical, narrow, and unit-tested.

Never assume every user's save was written by the current exact app build.

## Failure handling

SaveService should support:
- atomic/transactional local write where practical
- previous-known-good/recovery strategy
- validation before replacing valid profile data
- development diagnostics
- isolation so a cosmetic/settings failure does not destroy progression

## Service boundary

```text
Gameplay/Progression
        ↓
    UserProfile
        ↓
    SaveService
        ↕
ProfileSyncService (future)
        ↕
      Backend
```

Gameplay systems never write files or HTTP state directly.

## MVP minimum

Persist at least:
- identity/display name if used
- XP / level / HH
- avatar selections
- unlocked/owned content used by MVP
- per-chart best score/grade/combo
- settings/accessibility
- calibration
- schema version

Online cross-device sync is architecture, not a blocker for the four-song MVP.
