# Song / Chart Data Model v1

## Status

Canonical song/chart contract for Headbang Heroes.

The model must support MVP simplicity, future catalog scale, external tooling, and community/mod content without creating incompatible formats.

## Core direction

> **Content difficulty first. Multi-chart support optional.**

A song may have one chart or several.

A chart has exactly one authored difficulty classification.

The game must never assume that every song exists at Easy/Normal/Hard/Extreme.

## High-level model

```text
SongDefinition
├── identity / metadata
├── audio
├── musical timing
├── licensing / provenance reference
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

Song = musical asset/content identity.
Chart = playable interpretation.

## SongDefinition

Required v1 fields:
- `id`
- `title`
- `artist`
- `audioAsset`
- `durationMs`
- `defaultBpm`
- `defaultTimeSignature`
- `audioOffsetMs`
- `charts[]`

Recommended/optional:
- album
- year
- genre
- subgenre
- artwork
- credits
- preview offset
- provenance/rights reference
- tags

Metadata must not affect gameplay unless explicitly consumed by a system.

## Musical timing

Audio playback is authoritative.

Authoring happens in musical time; runtime resolves against deterministic audio time.

MVP may assume:
- one BPM
- one time signature
- one global audio offset

The schema should support future tempo/time-signature maps when a real song requires them.

Author-facing event position should remain readable in beat-space.

Examples:
- `32.0`
- `32.5`
- `32.25`
- `32.125`

Compiled runtime data should use deterministic precomputed timings/ticks rather than accumulated floating-point song-clock math.

## Musical abstraction rule

A chart is a **performance interpretation**, not a note-for-note transcription.

> **The chart follows the performable headbang rhythm, not every musical subdivision present in the source.**

A 1/32 drum phrase may be represented by 1/8 or 1/16 headbang pulses, a Half/Burst phrase, continuous Windmill motion, or selected accents.

Authoring should conceptually separate:

```text
MUSIC LAYER       source musical detail
PERFORMANCE LAYER intended human neck performance
CHART LAYER       HH event encoding
```

This is a core authoring rule.

## ChartDefinition

Required:
- `id`
- `songId`
- `difficulty`
- `version`
- `events[]`

Recommended:
- `schemaVersion`
- `rulesVersion`
- `author`
- `sections[]`
- `phrases[]`
- `notes`
- `tags[]`

Every competitive/run record should be attributable to song/chart/version and rules version when relevant.

## Difficulty

Canonical vocabulary:

```text
easy
normal
hard
extreme
```

Valid structures:

```text
Song A → Normal
Song B → Hard
Song C → Extreme
Song D → Normal + Hard
```

Difficulty is authored classification, not automatically inferred from event count.

Tooling may compute diagnostics, but the author owns the final label.

## Event families

v1 runtime event families:

```text
MotionEvent
RestEvent
```

Do not invent unrelated event types when the concept can be expressed with the canonical movement grammar.

## MotionEvent

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

Required:
- stable `id`
- `type = motion`
- beat/time position
- technique
- trajectory
- direction/action semantics
- modifier

Optional:
- duration
- finisherCandidate
- intensity
- tags
- non-scoring presentation hints

## Canonical movement vocabulary

Technique:
- Classic
- Half
- Deep
- Whiplash
- Windmill

Trajectory:
- Horizontal
- Vertical
- Circular
- CenterEdge

Modifier:
- None
- Double
- Hold
- Accent
- Burst

Rotational direction and other technique-specific data may exist where required without redefining the base vocabulary.

Extension rule:

> **Adding a new neck mechanic must not change the semantics of existing mechanics.**

## RestEvent

Natural Rest is represented by absence of required events.

Authored Rest is explicit.

Conceptual schema:

```text
RestEvent
- id
- type = rest
- beat
- durationBeats
- settlingBeats or evaluationProfile
- stillnessProfile / thresholds
- tags[]
```

Canonical runtime intent:

```text
rest begins
→ settling phase: player may bleed existing momentum
→ stillness evaluation phase
→ RestOutcome
```

The Rest event never snaps/reset the neck.

Exact stillness thresholds and score/combo consequences are tuning/configuration.

## Sections

Sections represent large musical structure such as:
- Intro
- Verse
- Chorus
- Breakdown
- Bridge
- Outro

Useful for tooling, debugging, Practice, and future mode/session structure.

Sections do not directly imply scoring behavior.

## Phrases

Phrases represent smaller musically meaningful units/riffs.

Useful for:
- chart authoring
- Practice loops
- THE BANG/Finisher context
- analytics/debugging
- future challenge modes

Do not require a phrase marker every bar merely for completeness.

## Finisher candidate

`finisherCandidate` marks a normal MotionEvent as musically suitable for Finisher resolution.

It does not:
- activate THE BANG
- guarantee a Finisher
- introduce a new input type

HypeSystem resolves whether THE BANG is active and whether execution/context satisfies deterministic Finisher rules.

## First/setup event

The first relevant motion from neutral may be a setup action with no preceding travel.

Chart data does not need a separate “fake MotionQuality” value.

Runtime can classify the player state as unprepared/setup and apply the canonical first-bang semantics defined in `FOUNDATION.md` / `TECHNICAL_CONTRACTS_V1.md`.

## Dense event semantics

Charts may contain close events, but authoring should prefer physically performable compound/continuous gestures over impossible discrete tap spam.

Runtime matching must support bounded candidate search around input time rather than assuming the next event is always the only eligible target.

The authoring validator should warn about:
- overlapping/ambiguous discrete input windows
- physically implausible direction reversals
- event density inconsistent with the selected technique vocabulary

Warnings need not automatically reject intentionally advanced content, but impossible/ambiguous charts should fail publication review.

## Stable IDs and versioning

Stable identity is required for:
- songs
- charts
- events
- sections
- phrases

Use separate versions for concepts that evolve independently:
- `schemaVersion`
- `chartVersion`
- `rulesVersion`
- package/content versions outside the chart as appropriate

Do not compare competitive results blindly across incompatible chart/rules versions.

## Canonical HH MIDI authoring/interchange

Headbang Heroes adopts `docs/HH_MIDI_STANDARD_V1.md` as the canonical MIDI convention for chart authoring/interchange.

Important distinction:

> **The HH MIDI is the MIDI of the Headbang Heroes performance chart, not necessarily the MIDI of the original song.**

An original song MIDI may be absent. If available, it may be retained as reference material, but only dedicated `HH_*` semantic tracks compile into gameplay.

The v1 MIDI grammar uses named technique tracks plus shared semantic note mappings. For example:

```text
HH_CLASSIC + note 36 → Classic / LEFT
HH_CLASSIC + note 35 → Classic / RIGHT
HH_CLASSIC + note 41 → Classic / UP
HH_CLASSIC + note 45 → Classic / DOWN
```

Technique lives primarily in the track name; direction/action lives in the note number; velocity carries intensity; note duration carries duration only for semantics that explicitly define it.

Modifiers, Authored Rest, Windmill intervals, Finisher candidates and structural markers are defined by the MIDI standard.

The MIDI convention is an authoring contract, not the runtime schema itself.

## Authoring / runtime pipeline

Canonical direction:

```text
song audio
+ SongDefinition metadata
+ HH authoring MIDI (`*.hh.mid`)
+ optional source/reference MIDI
        ↓
HH MIDI validator/importer
        ↓
canonical ChartDefinition
        ↓
semantic validator/compiler
        ↓
immutable RuntimeChart
        ↓
gameplay
```

Alternative future authoring tools may emit the same canonical ChartDefinition directly, but they must preserve the same gameplay semantics.

Runtime gameplay must not parse MIDI in the hot path.

MIDI note/channel/instrument details are authoring concerns and must be compiled into typed `MotionEvent` / `RestEvent` semantics before gameplay.

## Official and community compatibility

Official and future community content should share the same core schema wherever possible.

Community distribution/trust/rights rules may differ, but gameplay semantics should not require a second incompatible chart language.

## MVP subset

MVP may use:
- one chart per song
- single BPM
- single time signature
- small vocabulary subset
- canonical HH MIDI authoring using only the approved semantic tracks for implemented mechanics

This is a subset of the canonical schema, not a disposable temporary format.
