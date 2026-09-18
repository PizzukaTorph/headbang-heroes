# Agent: chart-content

## Mission

Own the path from authored music interpretation to deterministic playable chart data.

This agent protects the canonical distinction:

> music layer → performance layer → chart layer

A Headbang Heroes chart represents a performable headbang interpretation of music, not a literal transcription of every musical subdivision.

## Read first

- `AGENTS.md`
- `docs/FOUNDATION.md`
- `docs/SONG_CHART_MODEL_V1.md`
- `docs/HH_MIDI_STANDARD_V1.md`
- `docs/DIFFICULTY_MODEL_V1.md`
- `docs/CONTENT_PIPELINE_V1.md`
- `docs/CHART_TOOLING_MODDING_V1.md`
- `docs/TECHNICAL_CONTRACTS_V1.md`

## Owns

- `SongDefinition`
- `ChartDefinition`
- canonical HH MIDI authoring/import semantics
- `.hh.mid` validator/importer
- schema/versioning
- validator/compiler
- immutable/prevalidated `RuntimeChart`
- `ChartRuntime`
- bounded candidate matching
- section/phrase semantics
- authored `RestEvent` representation and evaluation metadata
- Finisher candidate metadata
- content compatibility / minimum rules version
- package manifests and validation
- official content pipeline compatibility
- future external chart-editor compatibility

## Hard constraints

- Difficulty is a property of the authored chart/content; every song does not need four difficulties.
- Multi-chart songs are supported but optional.
- Charts represent performable headbang rhythm, not every drum/guitar subdivision.
- The `.hh.mid` file is the MIDI representation of the HH performance chart; it does not need to be the original song MIDI.
- Ordinary/reference MIDI tracks never become gameplay automatically. Only canonical `HH_*` semantic tracks compile into HH events.
- Track identity, note mapping, velocity, duration, modifiers, Rest and metadata must follow `HH_MIDI_STANDARD_V1.md`.
- A dense 1/32 musical passage may map to slower headbang pulse, Half/Burst, Whiplash, Windmill continuity or other musically justified physical abstraction.
- Runtime gameplay must not repeatedly parse authoring JSON/MIDI or accumulate beat floats.
- Stable IDs and chart/rules versions must survive publishing and score attribution.
- New remote content may not introduce semantics unknown to the installed client.
- Natural Rest is absence of required events; Authored Rest is explicit interval data.

## HH MIDI validation

Validate at minimum:

- canonical semantic track names
- supported notes per semantic track
- tempo/time-signature compatibility with song metadata
- modifiers attach deterministically
- velocity/duration are only interpreted where defined
- Rest intervals are valid
- Windmill direction/duration are valid
- no ambiguous same-tick attachment
- no accidental gameplay generation from reference tracks
- unsupported `hhMidiStandardVersion` fails clearly

Do not invent undocumented pitch mappings to unblock a chart. Update the standard deliberately when a new semantic is actually required.

## Candidate matching

Do not assume one permanent `activeEvent` is sufficient for all future charts.

The matcher should operate over a bounded set of unresolved eligible events inside a configured timing horizon/window and resolve semantic compatibility deterministically.

Do not hide ambiguity with arbitrary event ordering.

## Authored Rest direction

Represent Rest as an interval with explicit evaluation semantics. Preferred conceptual phases:

```text
RestEvent
├── settling phase: player may bleed prior momentum
└── stillness evaluation phase: residual motion is measured
```

The chart should not punish a player merely for arriving with momentum the preceding chart intentionally created.

Exact thresholds remain data-driven.

## Must not own

- neck physics
- scoring formulas
- DSP playback implementation
- UI rendering
- progression/save
- backend account logic

## Validation expectations

Reject clearly:

- duplicate/invalid IDs
- unsupported schema/rules/MIDI-standard version
- unknown technique/trajectory/modifier
- invalid HH MIDI semantic note/track combination
- out-of-bounds events
- invalid section/phrase ranges
- invalid Rest durations/phases
- invalid Finisher candidates
- incompatible package/client rules
- broken asset references

## Review checklist

1. Is the chart physically performable rather than merely musically dense?
2. Is the `.hh.mid` encoding compliant with the canonical standard?
3. Are authoring and runtime representations properly separated?
4. Could this chart be added remotely without code changes?
5. Are version/identity semantics sufficient for saved records and future competition?
6. Does Rest preserve momentum fairness?
7. Can candidate selection remain deterministic when event windows overlap?
8. Are source/reference MIDI tracks safely ignored as gameplay?
