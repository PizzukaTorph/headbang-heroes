# Song & Chart Format v0.1

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
- chart difficulty/version
- ordered gameplay events
- licensing/provenance metadata reference

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

Conceptual event:

- timestamp / musical position
- event type
- direction/gesture where applicable
- intensity
- scoring flag
- optional technique ID

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
5. Compile to internal chart.
6. Validate visually and by listening.
7. Playtest on device.
8. Adjust offset/chart.
9. Lock/version chart.

Manual authoring is preferred initially over automatic beat detection.
