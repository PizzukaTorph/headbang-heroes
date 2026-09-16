# Content Pipeline v1

## Goal

Define how official Headbang Heroes content moves from authoring to a playable client without hard-wiring the catalog into app builds.

Preferred direction:

> **Server-first content delivery with local caching.**

Core rule:

> **Content can change remotely. Runtime rules cannot.**

A new song using mechanics already understood by the installed client should not require a new app release.

A new mechanic/event semantic/runtime contract requires a compatible client build.

## High-level flow

```text
AUTHORING
  audio + metadata + HH MIDI/chart source + artwork/background
        ↓
HH MIDI / SOURCE VALIDATION
        ↓
CANONICAL CHART IMPORT
        ↓
SEMANTIC VALIDATION
        ↓
COMPILATION
        ↓
PACKAGE
        ↓
PUBLICATION
        ↓
SERVER CATALOG / CDN
        ↓
CLIENT MANIFEST REFRESH
        ↓
DOWNLOAD / VERIFY / CACHE
        ↓
PLAY
```

The MVP does not need a complex CMS.

A small manifest/API plus object storage/CDN is sufficient.

## Canonical authoring source

For MIDI-based authoring, use `docs/HH_MIDI_STANDARD_V1.md`.

Important distinction:

> **The HH MIDI is the authored Headbang Heroes performance chart, not necessarily the MIDI of the original song.**

A song may have no original MIDI.

Optional source/arrangement MIDI can be retained as authoring reference, but only canonical `HH_*` semantic tracks compile into gameplay.

The first practical workflow can therefore be:

```text
song audio
+ song/chart metadata
+ <song>.<chart>.hh.mid
        ↓
validator/importer
        ↓
ChartDefinition
        ↓
compiler
```

Future HH Chart Editor output must remain semantically compatible with this pipeline.

## Content package

An official package may contain:
- SongDefinition
- one or more ChartDefinitions / compiled RuntimeChart payloads
- audio
- artwork/cover
- optional venue/background assets
- package manifest
- version metadata
- hashes/checksums
- provenance reference

Authoring `.hh.mid` files are source assets and do not need to ship in the runtime package.

The same logical schema should be usable whether authored through DAW/Guitar Pro MIDI, internal tooling, or the future external chart editor.

## Server catalog

The client should fetch a lightweight catalog/manifest sufficient for browsing and cache decisions.

Candidate fields:

```text
songId
title
artist
availableChartIds[]
authored difficulty labels
packageVersion
chartVersion
audioVersion
artworkVersion
backgroundVersion
schemaVersion
minimumClientRulesVersion
contentStatus
asset URLs
asset hashes/checksums
```

Do not download heavy audio/art merely to display the catalog.

## Client compatibility gate

Before download/play, validate at minimum:
- schema version supported
- `minimumClientRulesVersion` supported
- event semantics supported
- required assets resolvable

Unsupported content should be hidden/marked incompatible rather than partially loaded into gameplay.

## Cache behavior

Expected behavior:
- download only missing/outdated assets
- retain valid local packages
- verify integrity before activation
- keep valid downloaded content playable offline
- never interrupt an active cached run because network disappears

Server outage prevents refresh/new download; it does not invalidate valid cached content.

## Validation before publication

Validate at minimum:
- stable unique IDs
- required metadata
- referenced audio/assets exist
- song/chart linkage correct
- supported schema/rules versions
- known techniques/trajectories/modifiers
- event ordering/bounds
- valid sections/phrases
- valid Rest duration/evaluation profile
- valid Finisher candidate placement
- candidate timing ambiguity warnings
- physically implausible discrete density warnings
- package hash generation
- provenance/rights publication state

If the source is `.hh.mid`, also validate:
- canonical HH track names
- supported note mappings
- valid modifier attachment
- supported velocity/duration semantics
- tempo/time-signature consistency with content metadata
- no silent conversion of ordinary reference tracks into gameplay
- `hhMidiStandardVersion` compatibility where tracked

Invalid content must fail loudly before gameplay.

## Performable-rhythm validation

Validator/tooling should not assume musical subdivision equals required input density.

It should warn when a chart appears to transcribe impossible discrete input rates instead of using the movement vocabulary appropriately.

A dense source phrase may be encoded with slower headbang pulse, Half/Burst, Windmill continuity, or authored accents.

## Publication lifecycle

Suggested lifecycle:

```text
draft
→ validated
→ internal
→ cleared
→ published
→ retired
```

Technical validity is not publication permission.

Prototype/internal content must never become public merely because packaging succeeded.

## Integrity vs authenticity

Hashes/checksums provide integrity detection; they do not by themselves prove trusted publication if both manifest and payload could be replaced.

### POC/MVP baseline
- HTTPS transport
- explicit package/version IDs
- hashes/checksums
- known catalog endpoint

### Production direction
Add a trusted release mechanism such as:
- signed manifest/catalog, or
- platform/trusted backend release channel with equivalent authenticity guarantees

The gameplay/domain layer must not care which publication security mechanism is used.

## Licensing / provenance boundary

Rights metadata is separate from gameplay semantics.

Candidate backend fields:

```text
contentStatus
rightsReference
distributionAllowed
territories[]
expiryDate?
notes
```

Publication tooling must prevent uncleared/prototype content from reaching production catalogs.

Community/mod rights are a separate future policy and must not weaken official-content controls.

## External tooling compatibility

Preferred future editor output:

```text
Audio
+ SongDefinition
+ canonical HH chart source (.hh.mid and/or editor-native source)
+ optional presentation assets
        ↓
shared validator/importer/compiler
        ↓
HH package
```

Do not invent a second gameplay language for mods unless a hard constraint requires it.

## Provider abstraction

Client architecture should depend on `IContentProvider` / `ContentCatalog`, not directly on one vendor.

Possible implementations:
- local provider
- Unity Addressables-backed provider
- custom CDN/object-storage provider

Unity CCD may be used but is not architectural destiny.

## MVP implementation target

Acceptable MVP:
- DAW/Guitar Pro-friendly HH MIDI authoring
- command-line/editor validator/importer
- compiled canonical chart data
- static/tiny API-hosted manifest
- object storage/CDN
- manual publication process
- client download/cache
- HTTPS + hashes/version checks

Do not build a full CMS or standalone chart editor before content volume demands it.

## Guardrails

- no permanent hard-coded catalog architecture
- no mandatory network dependency after valid cache
- no server-defined unknown gameplay semantics
- no silent schema incompatibility
- no unvalidated publication
- no content authoring coupled to Unity scene internals
- no assumption that source musical density equals discrete headbang input density
- no runtime dependency on MIDI parsing
- no automatic gameplay generation from non-HH reference MIDI tracks
