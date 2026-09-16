# Song & Chart Format v0.1

> Historical authoring/pipeline notes. The canonical gameplay data contract is now defined in [`SONG_CHART_MODEL_V1.md`](./SONG_CHART_MODEL_V1.md).

## Principle

Audio playback is authoritative. Gameplay events are authored against musical time and resolved against a high-precision audio clock.

The initial pipeline uses:

**audio master + MIDI/event track + song metadata**

MIDI is an authoring/interchange format, not necessarily a runtime dependency. Import may compile MIDI into an internal deterministic chart asset.

## Package concept

A song package should eventually resolve to:

- stable song ID
- title
- artist
- genre/subgenre
- audio asset reference
- BPM map
- time signatures
- offset/calibration metadata
- one or more authored charts
- chart difficulty/version
- ordered gameplay events
- licensing/provenance metadata reference

A song is not required to provide Easy/Normal/Hard/Extreme variants. Difficulty belongs to each authored chart. The catalog may instead use different songs as its primary difficulty progression.

## MIDI convention proposal

Reserve named tracks/channels for gameplay semantics.

Candidate tracks:
- HH_BEAT — ordinary timing events
- HH_ACCENT — emphasized impacts
- HH_MOVE — technique/movement commands
- HH_SECTION — section markers
- HH_FX — non-scoring presentation hints

Exact note-number mapping must be documented only after the prototype proves which inputs are necessary.

## Event model

Canonical event semantics are defined in `SONG_CHART_MODEL_V1.md`.

At authoring/runtime boundary, each compiled event must resolve to:

- stable event ID
- musical position / deterministic timestamp
- event family
- direction/gesture where applicable
- technique / trajectory / modifier where applicable
- optional duration
- optional Finisher-candidate metadata

Runtime chart data should use precomputed event times suitable for deterministic comparison with the audio clock.

## Timing judgments

Initial conceptual windows:
- PERFECT
- GREAT
- GOOD
- MISS

Do **not** freeze millisecond values yet. Tune on real devices and provide calibration.

Store signed timing error for diagnostics:
- negative = early
- positive = late

## Calibration

Required early:
- global audio/input offset
- device testing
- debug display of measured timing error
- optional user calibration flow before competitive release

Visual cue timing must derive from the event timestamp and approach duration, not from chained animation callbacks.

## Chart versioning

Every leaderboard-eligible run must reference:
- song ID
- chart ID/difficulty
- chart version
- gameplay/scoring rules version if necessary

Changing competitive event timing should invalidate/separate incompatible leaderboard scores.

## Authoring workflow v0

1. Obtain cleared audio master.
2. Establish BPM/time-signature map.
3. Import/create MIDI gameplay track.
4. Place gameplay events manually.
5. Annotate sections, phrases, accents, rests and Finisher candidates as needed.
6. Compile to internal chart.
7. Validate visually and by listening.
8. Playtest on device.
9. Adjust offset/chart.
10. Lock/version chart.

Manual authoring is preferred initially over automatic beat detection.
