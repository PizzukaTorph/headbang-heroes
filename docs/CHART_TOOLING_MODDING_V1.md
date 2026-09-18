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
- through canonical HH MIDI (`*.hh.mid`)
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
├── one or more ChartDefinition / compiled chart payloads
├── audio asset
├── optional artwork
├── optional background/presentation assets
└── package/provenance metadata
```

Authoring sources such as `.hh.mid` do not need to ship to the runtime client unless useful for tooling/debugging.

The important architectural rule is that Unity-specific scene/prefab state must **not** be required to describe a song chart.

---

# 2. Canonical HH MIDI authoring

`HH_MIDI_STANDARD_V1.md` defines the canonical MIDI interchange convention.

Core idea:

> **The HH MIDI describes the Headbang Heroes performance chart, not necessarily the original song instrumentation.**

A source song may have no MIDI at all.

When source/arrangement MIDI exists, it may be used as reference material while dedicated semantic tracks such as:

```text
HH_CLASSIC
HH_HALF
HH_DEEP
HH_WHIPLASH
HH_WINDMILL
HH_REST
HH_MOD
HH_META
```

encode the actual gameplay choreography.

Technique is primarily represented by track identity; direction/action by the standard note mapping; velocity by intensity; duration by interval semantics only where explicitly defined.

The compiler must ignore ordinary non-`HH_*` reference tracks for gameplay unless an explicit assist/import feature is invoked.

---

# 3. External Headbang Heroes Chart Editor

A future standalone tool can sit entirely outside the game runtime.

Its job would be to create and validate canonical song/chart data.

Conceptual workflow:

```text
IMPORT AUDIO
      ↓
OPTIONAL IMPORT SOURCE MIDI
      ↓
BPM / OFFSET / TIME SIGNATURE
      ↓
AUTHOR HH PERFORMANCE LAYER
      ↓
SECTIONS / PHRASES
      ↓
REST / MODIFIERS / FINISHER MARKERS
      ↓
VALIDATION
      ↓
PLAYTEST / PREVIEW
      ↓
EXPORT .hh.mid and/or canonical chart source
      ↓
COMPILER
      ↓
EXPORT SONG PACKAGE
```

The editor should not need intimate knowledge of Unity internals.

Its contract is the canonical HH semantics.

---

# 4. Editor responsibilities

A future editor may support:

- importing MP3/WAV/OGG or another supported audio source
- importing optional source/reference MIDI
- assigning title, artist, album, genre, subgenre, artwork and credits
- defining BPM and time signature
- defining audio/chart offset
- editing future tempo maps where needed
- defining sections and phrases
- authoring HH technique lanes/tracks
- placing MotionEvents visually
- placing Authored Rest intervals
- setting Technique
- setting Trajectory
- setting Direction
- setting Modifier
- marking Accent events
- marking Finisher candidates
- editing velocity/intensity
- editing meaningful note duration
- previewing CURRENT/NEXT gameplay cues
- displaying musical subdivisions
- snapping to useful grids without implying one input per subdivision
- validating event combinations
- checking duplicate/conflicting events
- checking invalid durations
- checking events outside song bounds
- previewing approximate gameplay flow
- importing/exporting canonical HH MIDI
- exporting canonical chart data
- incrementing chart versions

These are future capabilities, not all-or-nothing requirements for the first editor version.

---

# 5. Tooling must remain data-driven

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

The HH MIDI standard is itself a mapping onto these canonical semantics, not a separate gameplay language.

If Headbang Heroes adds a new gameplay concept, the schema/compiler/editor/MIDI convention should evolve together.

---

# 6. Compile / validation boundary

Authoring data and runtime data are deliberately different concerns.

Recommended pipeline:

```text
Audio + metadata + .hh.mid / editor source
        ↓
HH MIDI/import validation
        ↓
Canonical ChartDefinition
        ↓
Semantic validation
        ↓
Chart compiler
        ↓
Deterministic RuntimeChart
        ↓
Headbang Heroes
```

This allows authoring to remain readable/editable while the runtime receives precomputed, ordered, deterministic event timing.

The compiler/validator should be reusable outside Unity.

That makes it suitable for:

- DAW/Guitar Pro authoring workflows
- external editor validation
- CI validation of official content
- community mod validation
- automated packaging tools

---

# 7. DAW / Guitar Pro workflow

The MIDI convention intentionally uses ordinary MIDI primitives:

- named tracks
- notes
- velocity
- note duration
- tempo/time-signature data
- marker/text events

This allows authoring in DAWs and, where export behavior is adequate, Guitar Pro.

Tool-specific behavior is not canonical. Any exported `.hh.mid` must pass HH validation before it is considered publishable.

The validator should diagnose common destructive export behavior such as:

- lost/renamed semantic tracks
- quantized event positions
- dropped markers
- changed PPQ/timing resolution
- invalid note remapping
- stripped durations

---

# 8. Modding direction

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

Do not couple the chart schema or HH MIDI standard to any one storefront or workshop provider.

---

# 9. Official vs community content

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

---

# 10. Competitive / leaderboard isolation

If community content eventually exists, official competitive records must not blindly mix with arbitrary modified charts.

Future run identity should be able to reference:

```text
songId
chartId
chartVersion
rulesVersion
contentOrigin
contentHash / packageHash
```

Possible policy:

- official signed/known charts → official records
- curated charts → curated records
- modified/local charts → local/community records

The important rule is that modding must not destroy score comparability.

---

# 11. Audio and rights boundary

A modding-capable chart format does **not** imply that copyrighted audio may be redistributed by Headbang Heroes or its community tooling.

The architecture keeps these concerns separate:

```text
chart authoring capability != distribution rights
```

Possible future package modes may include:

- chart + redistributable audio
- chart referencing locally supplied audio
- officially licensed bundled songs

The exact legal/content distribution policy must be defined before public mod sharing exists.

---

# 12. Presentation assets

A future song package may optionally reference presentation content such as:

- cover artwork
- background layers
- venue/background preset
- lighting/presentation hints

These must remain secondary to chart validity.

A song should still be playable with safe/default presentation assets if optional visual content is missing or rejected.

Community presentation assets must never be allowed to redefine scoring or gameplay semantics.

---

# 13. Schema and MIDI-standard evolution

Canonical content carries versions for separate concerns.

Conceptually:

```text
schemaVersion
chartVersion
rulesVersion
hhMidiStandardVersion
```

Meaning:

- `schemaVersion` = canonical chart data shape/meaning
- `chartVersion` = revision of this authored chart
- `rulesVersion` = gameplay/scoring interpretation version
- `hhMidiStandardVersion` = MIDI authoring/interchange mapping version

Future tools should either read compatible older versions, migrate explicitly, or fail with a useful validation error.

Never silently reinterpret an old chart or old `.hh.mid` under incompatible semantics.

---

# 14. Stable IDs

Stable song/chart/event IDs support:

- editor round-tripping
- patching
- update detection
- diagnostics
- content dependency tracking
- score attribution
- future workshop references

MIDI authors are not required to manually name every note. The compiler may generate deterministic event IDs for v1; a future editor may embed explicit IDs for stronger round-trip behavior.

---

# 15. MVP implications

The MVP does **not** need:

- public mod loading
- workshop integration
- a standalone editor
- advanced round-trip metadata
- community browser
- moderation
- downloadable custom content

The MVP **should** preserve these architectural conditions:

1. song/chart data is externalizable and serializable
2. the chart schema is not Unity-scene-dependent
3. canonical HH MIDI can be authored outside Unity
4. IDs and versions are stable enough for run attribution
5. gameplay semantics are typed and documented
6. runtime compilation/validation has a clear boundary
7. one song can be represented completely through data + assets

---

# 16. North-star workflow

Short term:

```text
DAW / Guitar Pro
↓
author HH_* MIDI tracks against audio
↓
export .hh.mid
↓
validate / compile
↓
play in Headbang Heroes
```

Long term:

```text
creator opens HH Chart Editor
↓
loads audio / optional reference MIDI
↓
authors headbang choreography
↓
validates
↓
exports canonical HH source/package
↓
Headbang Heroes consumes the same gameplay schema used by official songs
```

---

## Final guardrails

> **Do not build a second gameplay language for modding.**

> **HH MIDI is an authoring convention, not the runtime gameplay model.**

> **Do not make Unity scene data part of the song definition.**

> **Do not turn source MIDI instrumental density into required input density automatically.**

> **Do not couple chart authoring to a future distribution platform.**

> **Do not let optional community presentation data affect gameplay/scoring.**

> **Build the official authoring pipeline so future community tooling extends it instead of replacing it.**
