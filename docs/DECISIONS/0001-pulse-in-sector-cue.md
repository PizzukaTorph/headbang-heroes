# ADR 0001 — Timing cue: pulse-in-sector with build-up (supersedes closing-circle-on-head)

- Status: ACCEPTED (v0.0.2, 2026-09-18)
- Supersedes: parts of `docs/GAMEPLAY_SCREEN_V1.md` and `docs/GAMEPLAY_UX_V1.md`
- Deciders: project owner + implementer (evening playtest session)

## Context

v0.0.1 shipped the canonical **closing circle** timing cue centred on the avatar's head: an outer
ring shrinks toward a target ring; you hit when they coincide. Playtesting v0.0.2 surfaced two
problems:

1. **Philosophy mismatch.** A ring that shrinks toward a target is the visual language of generic
   note-tapping games (Osu!-style). `AGENTS.md` is explicit: Headbang Heroes is *"a game about
   headbanging, not a generic note-tapping game with a metal skin."* The shrink reads as "aim at
   the dot", not "feel the beat".
2. **Split attention & weak progression.** Timing lived on the head while the tap happens anywhere
   on-screen (the input is a full-screen 4-quadrant map). The player looked at the head but tapped
   elsewhere; and when they did not tap, the next cue's shrink animation started late (single active
   cue freed only on resolve/expire), so it often read as a static red→green with no visible
   approach.

The owner explored a radical simplification (single 360° gesture, timing-only) — analysed and
**deferred to a future "Freestyle" mode**: it would make direction (a core pillar) decorative. See
"Considered alternatives".

## Decision

Replace the closing-circle-on-head with a **pulse-in-sector** timing cue:

- The timing signal is a **pulse with a build-up of ~0.5–1.0 s that culminates exactly on the event
  time**, then relaxes. Growth = anticipation; peak = "hit now".
- The pulse renders **in the screen sector/quadrant of the expected direction** (Left/Right/Up/Down),
  the same full-screen regions the input maps to. This unifies **WHEN** (pulse build-up/peak) and
  **WHERE** (which sector) into one signal, where the thumb already acts.
- Colour rises toward the "hit" tone as it approaches; full/peak on the event.

The avatar/head remains the expressive focus (neck motion, hair, body) but is **no longer the
carrier of the primary timing signal**.

## Why this is allowed to deviate from the specs

`AGENTS.md`: specialists/plans may deviate from canonical specs only if the deviation is declared
and the spec updated (no silent contradiction). This ADR is that declaration. `GAMEPLAY_SCREEN_V1`
and `GAMEPLAY_UX_V1` get a header note pointing here.

## Consequences

- **Positive:** cue matches the "feel the beat" philosophy; WHEN+WHERE unified where the player
  taps; anticipation build-up fixes the "no visible progression without tapping" issue; still not a
  scrolling note-highway (one sector pulses at a time, transient).
- **Guardrails kept:** must remain "readable as choreography, not a note highway" — **one sector
  pulses per active event**, transient, no queued lanes. Timing authority is unchanged (AudioClock/
  RuntimeChart); this is presentation only and never shifts authored time or scoring.
- **Neutral/again-open:** whether the head keeps a small identity indicator alongside the sector
  pulse is a tuning choice (default: no head ring; head reacts via motion/hair only).
- **Negative:** diverges from the shipped v0.0.1 visual; the `ClosingCircleCue` blue tap-marker
  (early=larger/late=smaller) is retired or reworked for the pulse.

## Considered alternatives

- **Keep closing-circle (status quo):** rejected — philosophy mismatch, split attention.
- **Circle fill / radial timer:** viable but still head-centric and dot-aim flavoured.
- **Single 360° timing-only gesture (no direction):** deferred to a future **Freestyle** mode —
  removing direction would make the neck simulation decorative and contradicts the core pillar that
  timing, momentum AND direction matter.

## Implementation note

Presentation-only. Replace/augment the cue presenter to drive a per-sector pulse from
`AudioClock.SongTime` and the scheduler's active/next event (build-up over the approach window,
peak on the event time). No change to matching, windows, scoring, HYPE, or the neck domain.
