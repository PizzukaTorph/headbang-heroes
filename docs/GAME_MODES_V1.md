# Game Modes v1

## Current planned modes

Headbang Heroes currently anticipates five modes:

1. Story
2. Quick Headbang
3. Versus
4. Practice
5. Endurance

No additional modes are planned at this time.

---

## Core identity modes

The two modes with the strongest long-term product identity are:

- Story
- Versus

This does not mean both must exist in the POC/MVP.

---

# Story

Primary progression-oriented mode.

Expected responsibilities over time:

- structured song/content progression
- unlock flow
- player progression context
- performance milestones
- future narrative/presentation framing

Story should still use the same core headbang gameplay, scoring, HYPE, chart, and result systems.

Do not create a separate gameplay ruleset merely because the player is in Story.

---

# Quick Headbang

Fastest route from menu to song.

Conceptually:

```text
Quick Headbang
→ Song Select
→ Play
→ Results
→ Retry / choose another song
```

This is the natural POC-friendly mode and may initially be indistinguishable from the default play flow.

No progression/story framing is required beyond normal rewards/records.

---

# Versus

Competitive headbang mode.

Versus is considered a core long-term mode, but networking/competitive architecture can be significantly larger than the single-player loop.

The design should preserve:

- deterministic chart/scoring references
- comparable chart/rules versions
- clear competitive result data

The exact form is intentionally not frozen yet.

Possible future implementations may include synchronous or asynchronous competition, but v1 documentation must not assume one before technical validation.

Versus is not required to prove the POC and may be deferred beyond MVP if it threatens the four-song goal.

---

# Practice

A low-pressure learning/training mode.

Future capabilities may include:

- choose song
- choose section/phrase
- immediate retry
- repeat segment
- technique-focused practice
- optional playback-speed tools if technically/musically acceptable

Practice should reuse real chart data wherever possible rather than requiring special duplicate charts.

The MVP does not require advanced Practice tooling.

---

# Endurance

Long-form performance / score-attack mode.

Potential structure:

```text
song / sequence
→ continue performance
→ next segment/song
→ cumulative result
```

The core appeal is sustained consistency, HYPE management, and survival over a longer run.

Exact health/failure/continuation rules are not frozen.

Endurance should reuse existing gameplay systems rather than inventing a parallel combat layer.

---

## Mode Select integration

The Home/Game Flow architecture already supports a Mode Select stage.

When only one relevant mode is implemented, Mode Select may be bypassed.

When multiple modes are available:

```text
HOME
→ PLAY
→ MODE SELECT
   ├── Story
   ├── Quick Headbang
   ├── Versus
   ├── Practice
   └── Endurance
```

Locked/unimplemented modes must not appear as fake functional UI in production builds.

---

## Shared-system rule

> Modes should compose the same core gameplay systems instead of cloning them.

Shared systems include:

- Song/Chart runtime
- Audio clock
- Neck motion
- Scoring
- HYPE / THE BANG
- Body/Hair response
- Results
- Save/Profile

A mode may change entry/exit conditions, progression context, opponent logic, or session structure without redefining the core semantics of a headbang event.
