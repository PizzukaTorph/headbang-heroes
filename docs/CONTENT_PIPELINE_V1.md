# Content Pipeline v1

## Goal

Define how official Headbang Heroes content moves from authoring into a playable client without hard-wiring the catalog into app builds.

The preferred direction is **server-first content delivery** with local caching.

> Content can change remotely. Gameplay rules cannot.

A new song that uses mechanics the installed client already understands should not require a new app release.

A new gameplay mechanic, event semantic, or runtime contract still requires a compatible client build.

---

## High-level flow

```text
AUTHORING
  audio + song metadata + chart + artwork/background
        ↓
VALIDATION
        ↓
PACKAGE
        ↓
UPLOAD
        ↓
SERVER CATALOG / CDN
        ↓
CLIENT MANIFEST REFRESH
        ↓
DOWNLOAD / CACHE
        ↓
PLAY
```

The MVP does not need a complex CMS. A manifest, object storage/CDN, and a small API are sufficient.

---

## Content package

An official playable song package may contain:

- SongDefinition
- one or more ChartDefinitions
- audio asset
- artwork / cover
- optional venue/background assets
- package manifest
- checksums / hashes
- version metadata
- provenance reference

The core schema should remain the same whether authored manually, through an internal tool, or later through the external Headbang Heroes Chart Editor.

---

## Server catalog

The client should be able to retrieve a lightweight content manifest containing enough data to discover and cache content.

Candidate manifest fields:

```text
songId
title
artist
availableChartIds[]
difficulty labels
packageVersion
chartVersion
audioVersion
artworkVersion
backgroundVersion
contentStatus
asset URLs
asset hashes/checksums
minimumClientRulesVersion
```

Do not require the client to download heavy audio/artwork merely to display the catalog.

---

## Client cache behavior

The client compares the remote manifest with locally cached package metadata.

Expected behavior:

- download assets only when missing or outdated
- retain already valid content locally
- verify downloaded content before use
- keep downloaded playable content usable when temporarily offline
- do not interrupt an active run because the server becomes unavailable

An unavailable server should prevent new downloads/refreshes, not invalidate already cached playable content.

---

## Validation

Content must pass validation before publication.

At minimum validate:

- IDs are unique and well-formed
- referenced audio exists
- chart schema version is supported
- event types / techniques / trajectories / modifiers are known
- events are ordered and inside song bounds
- section/phrase ranges are valid
- authored rests do not have invalid duration
- chart references correct song ID
- Finisher candidate flags occur on valid gameplay events
- required metadata exists
- package hashes can be generated

Invalid content must fail loudly during authoring/publishing rather than being discovered in gameplay.

---

## Publication states

Suggested content lifecycle:

```text
draft
→ validated
→ internal
→ cleared
→ published
→ retired
```

Exact backend implementation may differ, but publication must never implicitly promote prototype/internal content to production.

---

## Licensing / provenance boundary

Official content must carry provenance/rights metadata separately from gameplay data.

Candidate backend fields:

```text
contentStatus
rightsReference
distributionAllowed
territories[]
expiryDate?
notes
```

`prototype` or `internal` content must not become public merely because its package is technically valid.

Community/mod content is a separate future publishing problem and must not weaken official-content rights checks.

---

## External tooling compatibility

The future external chart editor should produce the same logical song/chart contract consumed by the official pipeline.

Preferred tool output:

```text
Audio file
+ SongDefinition
+ ChartDefinition
+ optional artwork/background
        ↓
validator/compiler
        ↓
HH song package
```

Do not invent a second incompatible mod/chart format unless a hard technical constraint later requires one.

---

## MVP implementation target

For POC/MVP the pipeline may be intentionally small:

- static or tiny API-hosted manifest
- files in object storage/CDN
- manual upload process
- client download + cache
- local schema validation/debug tools

Do not build a full content-management platform before the game loop is proven.

---

## Guardrails

- no content hard-coded as a permanent catalog architecture
- no mandatory network dependency once a song is cached
- no server-controlled gameplay semantics unknown to the client
- no silent chart/schema incompatibility
- no publication of unvalidated packages
- no coupling content authoring to Unity scene internals
