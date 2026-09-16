# Song / Chart Data Model v1

## Status

This document defines the canonical song and chart data contract for Headbang Heroes.

It is intentionally complete enough to support the planned game, while keeping the first MVP free to ship with a single authored chart per song.

The model must support future expansion without forcing the MVP to implement every capability immediately.

---

## Core direction

Headbang Heroes treats **difficulty primarily as a property of an authored chart/content item**, not as a requirement that every song exists in every difficulty.

A song may therefore ship with only one chart:

- a simpler song may be authored as `Normal`
- a more demanding song may be authored as `Hard`
- an extremely dense/technical song may be authored as `Extreme`

The catalog itself can therefore carry the difficulty curve.

A song *may* have multiple charts later, but this is optional.

> **Content difficulty first. Multi-chart support optional.**

The MVP is expected to use one `Normal` chart for the first playable song.

---

# 1. High-level model

```text
SongDefinition
├── identity / metadata
├── audio
├── musical timing
├── licensing / provenance metadata
└── charts[]
      └── ChartDefinition
          ├── identity / version
          ├── difficulty
          ├── sections[]
          ├── phrases[]
          └── events[]
                ├── MotionEvent
                └── RestEvent
```

A song owns audio and musical metadata.

A chart owns gameplay interpretation.

This separation is important: changing gameplay authoring must not require duplicating the audio/song metadata.

---

# 2. SongDefinition

A `SongDefinition` describes the musical asset independently from any one gameplay chart.

Conceptual schema:

```json
{
  "id": "song-001",
  "title": "Example Song",
  "artist": "Example Artist",
  "album": "Example Album",
  "genre": "Death Metal",
  "subgenre": "Technical Death Metal",
  "audioAsset": "Audio/song-001.ogg",
  "durationMs": 248000,
  "defaultBpm": 180,
  "defaultTimeSignature": "4/4",
  "audioOffsetMs": 0,
  "charts": [
    "Charts/song-001.normal.json"
  ],
  "provenanceId": "song-001-rights"
}
```

## Required v1 fields

- `id`
- `title`
- `artist`
- `audioAsset`
- `durationMs`
- `defaultBpm`
- `defaultTimeSignature`
- `audioOffsetMs`
- `charts[]`

## Optional metadata

- album
- year
- genre
- subgenre
- artwork
- credits
- licensing/provenance reference
- preview/start offset
- content tags

These fields must not affect gameplay unless explicitly consumed by another system.

---

# 3. Musical timing

Audio playback is authoritative.

Gameplay is authored in **musical time** and resolved against the high-precision audio clock.

The runtime relationship is:

```text
musical position
      ↓
tempo/time-signature map
      ↓
audio timestamp
      ↓
judgment + cue scheduling
```

The chart must not drift because of frame rate, animation state, player input, or chained callbacks.

## MVP timing

The MVP may assume:

- one BPM
- one time signature
- one global audio offset

This is sufficient for the first song.

## Full v1-capable model

The data model should permit a future tempo map:

```text
TempoChange
- beat
- bpm

TimeSignatureChange
- beat
- numerator
- denominator
```

The MVP does not need a tempo-map editor before a song actually requires it.

---

# 4. Musical position representation

Author-facing chart data should be readable in beat-space.

Example:

```json
"beat": 32.5
```

This corresponds naturally to musical subdivisions and is significantly easier to author/debug than raw milliseconds.

Typical positions include:

- `32.0`
- `32.5`
- `32.25`
- `32.125`

The authoring/import pipeline may internally normalize these positions to integer ticks or another deterministic representation.

Recommended implementation direction:

- human/editor representation: beat-space
- compiled/runtime representation: deterministic precomputed timing

Do not let floating-point accumulation become the runtime song clock.

---

# 5. ChartDefinition

A chart is one playable interpretation of a song.

Conceptual schema:

```json
{
  "id": "song-001-normal-v1",
  "songId": "song-001",
  "difficulty": "normal",
  "version": 1,
  "author": "pizzu",
  "rulesVersion": 1,
  "sections": [],
  "phrases": [],
  "events": []
}
```

## Required fields

- `id`
- `songId`
- `difficulty`
- `version`
- `events[]`

## Recommended fields

- `author`
- `rulesVersion`
- `sections[]`
- `phrases[]`
- `notes`
- `tags[]`

---

# 6. Difficulty

Supported difficulty vocabulary:

```text
easy
normal
hard
extreme
```

A song does **not** need all four.

Examples of valid catalog structures:

```text
Song A → Normal only
Song B → Hard only
Song C → Extreme only
Song D → Normal + Hard
```

The game must therefore never assume:

```text
song == four difficulty variants
```

Instead:

```text
song == one or more authored charts
chart == one difficulty classification
```

This preserves the option to add alternate difficulty charts later without making duplication mandatory.

---

# 7. Difficulty and song selection

Difficulty may be exposed as a content filter/category rather than only as a per-song selector.

Conceptually:

```text
NORMAL
├── Song A
├── Song B
└── Song C

HARD
├── Song D
└── Song E

EXTREME
└── Song F
```

If a song has multiple charts, the UI may expose those available alternatives locally.

The data model should support both approaches.

---

# 8. Event families

The v1 runtime requires two explicit event families:

```text
MotionEvent
RestEvent
```

Do not encode every gameplay concept as an unrelated event type.

Motion uses the gameplay vocabulary already defined elsewhere:

```text
Technique + Trajectory + Direction + Modifier
```

Rest represents authored stillness intervals.

Natural rest requires no event at all.

---

# 9. MotionEvent

Conceptual schema:

```json
{
  "id": "evt-0042",
  "type": "motion",
  "beat": 32.0,
  "technique": "classic",
  "trajectory": "horizontal",
  "direction": "left",
  "modifier": "none",
  "durationBeats": 0,
  "finisherCandidate": false
}
```

## Required fields

- `id`
- `type = motion`
- `beat`
- `technique`
- `trajectory`
- `direction`
- `modifier`

## Optional fields

- `durationBeats`
- `finisherCandidate`
- `intensity`
- `tags[]`
- presentation hints that do not affect scoring

---

# 10. Technique vocabulary

Initial technique vocabulary comes from the neck gameplay contract:

```text
Classic
Half
Deep
Whiplash
Windmill
```

`Rest` is intentionally represented as `RestEvent`, not as a normal MotionEvent technique.

Additional techniques may be introduced later only when they cannot be expressed cleanly as an existing technique + trajectory + modifier.

Extension rule:

> Adding a new neck mechanic must not change the semantics of existing mechanics.

---

# 11. Trajectory vocabulary

Initial trajectory vocabulary:

```text
Horizontal
Vertical
Circular
CenterEdge
```

Trajectory expresses the shape/path family of the commanded motion.

It is distinct from technique.

Example:

```text
Classic + Horizontal
Classic + Vertical
Windmill + Circular
```

---

# 12. Direction

Direction expresses the authored inversion/target direction where applicable.

Initial cardinal vocabulary:

```text
left
right
up
down
```

Technique-specific extensions may later include values such as clockwise/counter-clockwise where required by circular motion, but these should be introduced deliberately rather than overloaded into unrelated fields.

Direction is gameplay data.

The physical touch region used to trigger it is **not** chart data.

---

# 13. Modifier vocabulary

Initial modifiers:

```text
none
double
hold
accent
burst
```

Modifiers alter how a technique/trajectory event should be interpreted without creating a whole new technique family.

Examples:

```text
Classic + Horizontal + Double
Deep + Vertical + Hold
Half + CenterEdge + Burst
```

---

# 14. Accent semantics

`Accent` remains a modifier, not a technique.

It represents an emphasized musical/gameplay impact.

Example:

```json
{
  "id": "evt-0071",
  "type": "motion",
  "beat": 64,
  "technique": "classic",
  "trajectory": "vertical",
  "direction": "down",
  "modifier": "accent"
}
```

Accent may influence presentation and configured scoring behavior, but must not silently redefine timing semantics.

---

# 15. Finisher candidate markers

A Finisher candidate is metadata on an otherwise normal authored gameplay event.

Example:

```json
{
  "id": "evt-0096",
  "type": "motion",
  "beat": 96,
  "technique": "deep",
  "trajectory": "vertical",
  "direction": "down",
  "modifier": "accent",
  "finisherCandidate": true
}
```

It means:

> This is a musically appropriate place for THE BANG to resolve into a Finisher.

It does **not** mean:

- the player must activate THE BANG here
- a Finisher is guaranteed
- this is a separate technique
- normal gameplay is suspended

Runtime Finisher resolution still depends on THE BANG being active and the configured execution requirements being satisfied.

The trigger remains deterministic.

---

# 16. Natural Rest

A natural rest is simply a gap with no required gameplay event.

During natural rest:

- the player may move or stop
- neck simulation continues naturally
- damping/inertia continue
- no penalty is applied for movement
- idle presentation may appear if appropriate

Natural Rest therefore needs **no chart object**.

---

# 17. Authored RestEvent

An authored rest explicitly asks the player to reduce/stabilize movement.

Conceptual schema:

```json
{
  "id": "rest-001",
  "type": "rest",
  "beat": 128,
  "durationBeats": 4,
  "stillnessRequired": true
}
```

## Required fields

- `id`
- `type = rest`
- `beat`
- `durationBeats`

## Optional/configurable fields

- `stillnessRequired`
- future tolerance/profile reference
- tags

The rest event must never snap or reset the neck.

The player must bleed momentum through the same simulation used everywhere else.

---

# 18. Sections

Sections describe large-scale song structure.

Example:

```json
{
  "id": "verse-1",
  "label": "Verse 1",
  "startBeat": 32,
  "endBeat": 64
}
```

Typical examples:

- Intro
- Verse
- Chorus
- Bridge
- Breakdown
- Solo
- Outro

Sections are primarily authoring/tooling metadata.

Potential consumers include:

- chart editor
- debug tools
- analytics
- future challenge modes
- practice/restart features
- presentation logic

Sections must not automatically modify scoring unless an explicit system consumes them.

---

# 19. Phrases

Phrases describe smaller musically meaningful spans inside or across sections.

Example:

```json
{
  "id": "verse1-riff-a",
  "startBeat": 32,
  "endBeat": 40,
  "label": "Riff A"
}
```

A phrase may represent:

- a riff
- a 4/8-bar musical unit
- a blastbeat passage
- a breakdown cell
- a buildup
- a climax unit

Phrase markers are useful for tooling, THE BANG authoring, future challenge logic, and performance analysis.

They are not mandatory for every bar of the song.

---

# 20. Tags

Generic `tags[]` may be attached to non-critical metadata objects where useful.

Examples:

```json
["chorus", "blast", "climax"]
```

Tags are intended for:

- tooling
- search/filtering
- author notes
- future mode/content selection
- analytics

Gameplay-critical semantics must **not** depend on arbitrary free-text tags.

If a concept becomes required for gameplay, promote it to a typed field.

---

# 21. Stable IDs

Gameplay events, rests, sections, phrases, songs, and charts should have stable IDs.

Examples:

```text
song-001
song-001-normal-v1
evt-0042
rest-001
verse-1
verse1-riff-a
```

IDs are useful for:

- debugging
- editor selection
- telemetry
- validation errors
- chart patches
- result diagnostics

IDs should be stable but do not need semantic cleverness.

---

# 22. Chart versioning

Every chart must be versioned.

A run should be attributable to at least:

```text
songId
chartId
chartVersion
rulesVersion
```

Changing event timing, event meaning, or scoring-relevant chart content may require a new chart version.

This keeps future best-score/leaderboard data comparable.

The MVP does not need online leaderboards to benefit from correct versioning now.

---

# 23. Runtime compilation

Authoring representation and runtime representation do not have to be identical.

Recommended pipeline:

```text
Song metadata
+ authored chart / MIDI
+ tempo information
        ↓
validator/compiler
        ↓
compiled deterministic chart
        ↓
runtime
```

Runtime should receive ordered, prevalidated events with timing information suitable for direct comparison against the audio clock.

Avoid reparsing complicated authoring semantics every frame.

---

# 24. MIDI relationship

MIDI remains useful as an authoring/interchange format.

It is **not required to be the runtime data format**.

Possible authoring flow:

```text
MIDI/event track
      ↓
import
      ↓
Headbang Heroes chart representation
      ↓
manual correction/annotation
      ↓
validation
      ↓
compiled runtime chart
```

Candidate semantic tracks may include:

```text
HH_BEAT
HH_ACCENT
HH_MOVE
HH_SECTION
HH_FX
```

Exact note-number/channel mapping should only be frozen when the prototype/editor workflow proves what is convenient.

---

# 25. Audio authority and calibration

The audio clock remains authoritative.

Input must never shift authored song timing.

Required system-level calibration support:

- global audio/input offset
- device testing
- signed timing-error diagnostics
- future user calibration flow where necessary

Visual cues derive from event time and cue-approach configuration.

They must not be driven by animation-complete callbacks.

---

# 26. Difficulty is authored, not automatically inferred

The system may eventually calculate diagnostics such as:

- event density
- maximum subdivision
- technique diversity
- direction-change rate
- rest ratio

But the declared difficulty remains an authored content classification.

Do not automatically relabel a chart because one numerical metric crosses a threshold.

Difficulty is the combined result of:

- musical density
- subdivisions
- technique mix
- trajectory changes
- modifier usage
- momentum demands
- timing-window profile
- overall choreography

---

# 27. Multi-chart support without multi-chart obligation

The schema deliberately keeps `charts[]` on `SongDefinition`.

This allows future examples such as:

```text
Song A
├── Normal
└── Hard
```

without requiring every content item to follow that pattern.

For MVP:

```text
charts.length = 1
```

is completely valid and expected.

No runtime or UI system should assume that missing difficulty variants are an error.

---

# 28. Full example

```json
{
  "song": {
    "id": "song-001",
    "title": "Example Song",
    "artist": "Example Artist",
    "audioAsset": "Audio/song-001.ogg",
    "durationMs": 248000,
    "defaultBpm": 180,
    "defaultTimeSignature": "4/4",
    "audioOffsetMs": 0,
    "charts": ["Charts/song-001.normal.json"]
  },
  "chart": {
    "id": "song-001-normal-v1",
    "songId": "song-001",
    "difficulty": "normal",
    "version": 1,
    "rulesVersion": 1,
    "author": "pizzu",
    "sections": [
      {
        "id": "intro",
        "label": "Intro",
        "startBeat": 0,
        "endBeat": 16
      },
      {
        "id": "verse-1",
        "label": "Verse 1",
        "startBeat": 16,
        "endBeat": 48
      }
    ],
    "phrases": [
      {
        "id": "verse1-riff-a",
        "label": "Riff A",
        "startBeat": 16,
        "endBeat": 24
      }
    ],
    "events": [
      {
        "id": "evt-001",
        "type": "motion",
        "beat": 16,
        "technique": "classic",
        "trajectory": "horizontal",
        "direction": "left",
        "modifier": "none"
      },
      {
        "id": "evt-002",
        "type": "motion",
        "beat": 17,
        "technique": "classic",
        "trajectory": "horizontal",
        "direction": "right",
        "modifier": "accent",
        "finisherCandidate": true
      },
      {
        "id": "rest-001",
        "type": "rest",
        "beat": 24,
        "durationBeats": 4,
        "stillnessRequired": true
      }
    ]
  }
}
```

The example combines song and chart only for readability. Production assets may store them separately.

---

# 29. MVP implementation subset

The first MVP uses the **full contract conceptually**, but only needs to implement the subset required for the first playable song.

Expected first MVP:

```text
1 song
1 Normal chart
constant BPM
4/4
MotionEvent
RestEvent
sections optional but supported
phrases optional but supported
accent modifier
finisherCandidate
```

This is a subset, not a different format.

Do not create a throwaway `MvpSongFormat` that later needs migration.

---

# 30. Guardrails

1. Audio playback is authoritative.
2. Song metadata and gameplay chart data remain separated conceptually.
3. A song may have one or many charts.
4. Missing difficulty variants are valid.
5. Difficulty belongs to the chart, not automatically to the audio asset.
6. Catalog difficulty may be driven by different songs rather than easier/harder copies of one song.
7. MotionEvent uses Technique + Trajectory + Direction + Modifier.
8. RestEvent is explicit only for authored stillness; natural rest is event absence.
9. Finisher candidate is metadata, not a new technique.
10. Sections/phrases describe music; they do not implicitly modify scoring.
11. Free-text tags never carry gameplay-critical meaning.
12. Runtime timing is deterministic and driven by the audio clock.
13. Input never moves authored event timing.
14. The MVP implements a subset of this schema, not a disposable alternate schema.
15. Do not build a DAW before the gameplay needs one.

---

# 31. Acceptance criteria

The model is sufficient when all of the following are possible without schema redesign:

- author one Normal MVP song
- assign a different song directly to Hard or Extreme
- add a second chart to an existing song later
- represent Classic/Horizontal and Classic/Vertical events
- represent future Half/Deep/Whiplash/Windmill events
- represent modifiers such as Hold/Accent/Burst
- mark deterministic Finisher candidates
- represent authored stillness intervals
- represent natural rests through event absence
- mark song sections and phrases
- support constant-BPM MVP content
- add tempo changes later without replacing the whole model
- version chart changes safely
- compile authoring data into deterministic runtime events

---

## North-star rule

> **The data model describes the musical performance we want the player to execute; it must not force every song into the same difficulty structure.**
