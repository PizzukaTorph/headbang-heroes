# Save / Profile Data v1

## Goal

Persist all user metadata required to reconstruct progression, identity, customization, records, settings, and calibration across sessions and updates.

> Save data describes the player, not the song catalog.

---

## High-level model

```text
UserProfile
├── identity
├── progression
├── avatar
├── unlocks
├── records
├── settings
├── calibration
└── schema/version metadata
```

Suggested conceptual shape:

```text
playerId
displayName

level
xp
hhCurrency

avatarDefinition / equipped cosmetics
performanceStyle
idleStyle

ownedCosmetics[]
unlockedSongs[]
unlockedContent[]

records[]
  songId
  chartId
  chartVersion
  bestScore
  bestGrade
  longestCombo
  bestPerfectCount
  bestGreatCount
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

Exact serialization format is an implementation detail; the logical contract is what matters here.

---

## Local-first behavior

The game should remain usable without a live connection.

Preferred direction:

```text
LOCAL SAVE = immediate gameplay authority
SERVER PROFILE = sync/backup/cross-device authority when available
```

The client writes locally immediately, then synchronizes when online.

A network outage must not prevent:

- launching the game
- playing downloaded songs
- earning local run results
- changing avatar/settings
- retrying

Pending progression can synchronize later.

---

## Conflict principles

Do not invent a complex generic merge engine if domain-specific rules are clearer.

Candidate rules:

- best score/grade/combo records: keep the better valid record
- unlock sets: union when both sides are valid
- settings/avatar selection: latest valid user choice
- currency/progression: server-authoritative once a production backend exists, with explicit transaction handling
- unsupported/corrupt fields: preserve recoverable profile state and surface diagnostics

Exact production sync semantics can be implemented later, but IDs and versioning must support them from the start.

---

## Record identity

Records must reference the exact playable content contract:

```text
songId
chartId
chartVersion
rulesVersion where required
```

Do not compare scores blindly across incompatible competitive chart/rules versions.

---

## Avatar persistence

Save the avatar as semantic selections, not as baked visuals.

Examples:

```text
presentation/body choice
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

Missing/retired cosmetic IDs must fail gracefully to a valid fallback rather than making the whole profile unloadable.

---

## Settings vs gameplay state

User settings must not change scoring potential unless explicitly defined as calibration.

Accessibility options such as reduced flash/shake are presentation-only.

Calibration offsets affect synchronization of perception/input and therefore must be stored explicitly and auditable.

---

## Versioning and migration

Every save requires `saveSchemaVersion`.

On load:

```text
read version
→ migrate forward if supported
→ validate
→ load profile
```

Never rely on the assumption that all installed users always run a profile written by the current exact version.

Migration should be deterministic and preferably monotonic.

Keep migrations narrow and testable.

---

## Failure handling

The save system should support:

- atomic/transactional local writes where practical
- previous-known-good backup or equivalent recovery strategy
- validation before replacing a valid profile
- clear diagnostics in development builds

A failed cosmetic/settings write must not destroy progression.

---

## MVP minimum

POC may use a minimal local profile.

MVP should persist at least:

- identity/display name if used
- XP / level / HH
- avatar selections
- owned/unlocked content
- per-chart best score/grade/combo
- settings
- calibration
- schema version

Online account/cross-device sync is desirable architecture, not a blocker for proving the four-song MVP.
