# Chart Tooling & Modding Direction v1

## Status

This document captures the future-facing tooling and modding direction enabled by the canonical Headbang Heroes song/chart data model.

This is **not an MVP implementation requirement**.

The purpose is to protect the architecture now so that future tooling, user-authored songs, and community charts do not require a second incompatible format later.

---

## Core principle

> **Official and community charts should use the same core schema whenever possible.**

Headbang Heroes should not invent a separate "mod format" if the canonical `SongDefinition` + `ChartDefinition` contract is already capable of describing playable content.

The game runtime should consume validated chart data independently from whether that data was authored:

- internally by the development team
- through an official external editor
- through community tooling
- through future import/conversion pipelines

The provenance/trust level may differ, but the gameplay contract should remain shared.

---

# 1. Long-term package concept

A playable song package can conceptually resolve to:

```text
Song Package
├── SongDefinition
├── one or more ChartDefinition files
├── audio asset
├── optional artwork
├── optional background/presentation assets
└── package/provenance metadata
```

The MVP does not need a formal distributable package format yet.

The important architectural rule is that Unity-specific scene/prefab state must **not** be required to describe a song chart.

---

# 2. External Headbang Heroes Chart Editor

A future standalone tool can sit entirely outside the game runtime.

Its job would be to create and validate canonical song/chart data.

Conceptual workflow:

```text
IMPORT AUDIO
      ↓
SONG METADATA
      ↓
BPM / OFFSET / TIME SIGNATURE
      ↓
TIMELINE AUTHORING
      ↓
SECTIONS / PHRASES
      ↓
GAMEPLAY EVENTS
      ↓
REST / ACCENT / FINISHER MARKERS
      ↓
VALIDATION
      ↓
PLAYTEST / PREVIEW
      ↓
EXPORT SONG PACKAGE
```

The editor should not need intimate knowledge of Unity internals.

Its contract is the data schema.

---

# 3. Editor responsibilities

A future editor may support:

- importing MP3/WAV/OGG or another supported audio source
- assigning title, artist, album, genre, subgenre, artwork and credits
- defining BPM and time signature
- defining audio/chart offset
- editing future tempo maps where needed
- defining sections and phrases
- placing MotionEvents on a musical timeline
- placing Authored Rest events
- setting Technique
- setting Trajectory
- setting Direction
- setting Modifier
- marking Accent events
- marking Finisher candidates
- previewing CURRENT/NEXT gameplay cues
- displaying musical subdivisions
- snapping to 1/4, 1/8, 1/16, 1/32 and other supported grids
- validating event combinations
- checking duplicate/conflicting events
- checking invalid durations
- checking events outside song bounds
- previewing approximate gameplay flow
- exporting canonical chart data
- incrementing chart versions

These are future capabilities, not all-or-nothing requirements for the first editor version.

---

# 4. Tooling must remain data-driven

The editor should derive available authoring vocabulary from typed gameplay definitions where practical.

Examples:

```text
Technique
- Classic
- Half
- Deep
- Whiplash
- Windmill

Trajectory
- Horizontal
- Vertical
- Circular
- CenterEdge

Modifier
- None
- Double
- Hold
- Accent
- Burst
```

The editor must not invent a parallel semantic vocabulary that then needs translation into the game.

If Headbang Heroes adds a new gameplay concept, the schema/compiler/editor pipeline should evolve together.

---

# 5. Compile / validation boundary

Authoring data and runtime data may differ.

Recommended long-term pipeline:

```text
Canonical authoring files
        ↓
Schema validation
        ↓
Semantic validation
        ↓
Chart compiler
        ↓
Deterministic runtime chart
        ↓
Headbang Heroes
```

This allows the authoring format to remain readable while the runtime receives precomputed, ordered, deterministic event timing.

The compiler/validator should eventually be reusable outside Unity.

That makes it suitable for:

- external editor validation
- CI validation of official content
- community mod validation
- automated packaging tools

---

# 6. Modding direction

Modding is a plausible future extension, not an MVP commitment.

Potential content tiers:

```text
Official Content
Community Local Mods
Curated / Workshop-style Content
```

The exact distribution platform is intentionally undefined.

Possible future workflows could include:

- local sideloaded song packages
- import folders
- community repositories
- curated in-game browser
- workshop-style distribution

Do not couple the chart schema to any one storefront or workshop provider.

---

# 7. Official vs community content

Official and community content can share the same gameplay schema while remaining distinguishable by metadata/trust.

Conceptually:

```text
ContentOrigin
- official
- local
- community
- curated
```

This is metadata, not gameplay difficulty.

It can later drive:

- UI labeling
- trust warnings
- leaderboard eligibility
- integrity checks
- update behavior
- moderation/curation flows

The MVP does not need to implement `ContentOrigin` yet unless useful.

---

# 8. Competitive / leaderboard isolation

If community content eventually exists, official competitive records must not blindly mix with arbitrary modified charts.

Future run identity should be able to reference:

```text
songId
chartId
chartVersion
rulesVersion
contentOrigin
contentHash / packageHash (future)
```

Possible policy:

- official signed/known charts → official records
- curated charts → curated records
- modified/local charts → local/community records

Exact leaderboard design is future work.

The important rule is that modding must not destroy score comparability.

---

# 9. Audio and rights boundary

A modding-capable chart format does **not** imply that copyrighted audio may be redistributed by Headbang Heroes or its community tooling.

The architecture should keep these concerns separate:

```text
chart authoring capability != distribution rights
```

Possible future package modes may include:

- chart + redistributable audio
- chart referencing locally supplied audio
- officially licensed bundled songs

The exact legal/content distribution policy must be defined before public mod sharing exists.

---

# 10. Presentation assets

A future song package may optionally reference presentation content such as:

- cover artwork
- background layers
- venue/background preset
- lighting/presentation hints

These must remain secondary to chart validity.

A song should still be playable with safe/default presentation assets if optional visual content is missing or rejected.

Community presentation assets must never be allowed to redefine scoring or gameplay semantics.

---

# 11. Schema evolution

Modding makes backward compatibility more important.

Canonical content should eventually carry a schema version separate from chart version.

Conceptually:

```text
schemaVersion: 1
chartVersion: 4
rulesVersion: 2
```

Meaning:

- `schemaVersion` = shape/meaning of the data format
- `chartVersion` = revision of this authored chart
- `rulesVersion` = gameplay/scoring interpretation version

Future tools should either:

- read compatible older schema versions
- migrate them explicitly
- or fail with a useful validation error

Never silently reinterpret an old chart under incompatible semantics.

---

# 12. Stable IDs matter even more with mods

Stable song/chart/event IDs support:

- editor round-tripping
- patching
- update detection
- diagnostics
- content dependency tracking
- score attribution
- future workshop references

Community tooling should generate valid stable IDs rather than relying solely on filenames.

---

# 13. Future package manifest

A formal package manifest may eventually look conceptually like:

```json
{
  "schemaVersion": 1,
  "packageId": "community.example.songpack",
  "origin": "community",
  "songDefinition": "song.json",
  "charts": ["chart.normal.json"],
  "audio": "audio/song.ogg",
  "artwork": "art/cover.png"
}
```

This is illustrative only.

Do not freeze the package manifest before actual tooling/mod distribution requirements exist.

---

# 14. MVP implications

The MVP does **not** need:

- public mod loading
- workshop integration
- a standalone editor
- package signing
- package hashes
- community browser
- moderation
- downloadable custom content

The MVP **should** preserve these architectural conditions:

1. song/chart data is externalizable and serializable
2. the chart schema is not Unity-scene-dependent
3. IDs and versions are stable
4. gameplay semantics are typed and documented
5. runtime compilation/validation has a clear boundary
6. one song can be represented completely through data + assets

If those conditions hold, future tooling becomes an extension instead of a rewrite.

---

# 15. North-star workflow

The long-term ideal is simple:

```text
creator opens HH Chart Editor
↓
loads a song
↓
maps musical structure
↓
authors headbang choreography
↓
validates
↓
exports
↓
Headbang Heroes loads the same core format used by official songs
```

That is the architectural target.

---

## Final guardrails

> **Do not build a second chart language for modding.**

> **Do not make Unity scene data part of the song definition.**

> **Do not couple chart authoring to a future distribution platform.**

> **Do not let optional community presentation data affect gameplay/scoring.**

> **Do not promise public mod distribution before rights, integrity, and moderation requirements are designed.**

> **Build the canonical data contract so the official pipeline can become the foundation of future community tooling.**
