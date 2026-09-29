# Gameplay Techniques v1 — Contract

## Status

GG1 in progress.

This document records only decisions already accepted for Headbang Heroes. Individual technique semantics are defined one at a time and are not considered canonical until explicitly accepted.

## Core split

Headbang Heroes has two different concepts:

### Classic

`Classic` is the core gameplay movement.

It is:
- always available;
- not a special skill;
- the baseline headbang language;
- driven by the standard semantic directional controls;
- continuously connected to NeckMotionState;
- valid without any progression unlock.

A complete HH song may use Classic alone.

### Technique Skills

The special techniques are:

- Half
- Deep
- Whiplash
- Windmill

They are **Technique Skills**, not ordinary note types and not alternate physics modes.

A Technique Skill is an authored special gameplay moment with its own recognizable touch gesture.

Conceptually:

```text
authored Technique Skill event
        ↓
dedicated cue / gesture prompt
        ↓
player performs touch gesture
        ↓
GestureRecognizer produces semantic TechniqueIntent
        ↓
neck responds through the authoritative gameplay model
        ↓
technique-specific execution can be evaluated
```

The skill does not execute automatically for the player.

## Gesture-first rule

Technique Skills are intentionally gesture-driven.

Accepted direction:

- Windmill uses a circular gesture;
- Deep uses a deliberate long/full-extension gesture;
- Half uses a deliberately reduced/partial gesture;
- Whiplash uses an explosive reversal-style gesture.

Exact gesture geometry, thresholds, duration and evaluation remain to be defined individually during GG1.

Hard rule:

> **Gesture recognition identifies player intent; it must not directly award success or bypass the neck/gameplay domain.**

The gesture can initiate/shape the special technique, but authoritative outcome remains gameplay-domain state.

## Authored-content rule

Technique Skills are never inserted procedurally by the runtime.

They are manually authored by the HH chart creator at musically intentional points.

Source authoring direction:

```text
custom HH MIDI / authoring data
        ↓
explicit Technique Skill markers/events
        ↓
chart compilation
        ↓
RuntimeChart
```

Hard rule:

> **Technique Skills are authored composition, not difficulty-generated decoration.**

The chart author decides:
- which technique appears;
- exact musical location;
- duration where relevant;
- required orientation/direction where relevant;
- any future technique-specific parameters.

The runtime must not:
- randomly inject skills;
- automatically add more skills because difficulty is higher;
- infer special techniques merely from BPM;
- convert dense MIDI subdivisions into Technique Skill spam.

Difficulty may determine whether a particular authored chart contains a broader technique vocabulary, but every Technique Skill occurrence remains explicit authored content.

## Dedicated MIDI technique track

Technique Skills are authored on a dedicated MIDI track/lane for each chart variant.

Conceptually:

```text
song-easy.mid
├── HH_CLASSIC / base chart data
└── HH_TECHNIQUES   (may be empty or absent)

song-normal.mid
├── HH_CLASSIC / base chart data
└── HH_TECHNIQUES   (authored where appropriate)

song-hard.mid
├── HH_CLASSIC / base chart data
└── HH_TECHNIQUES   (may contain many more authored skill moments)

song-extreme.mid
├── HH_CLASSIC / base chart data
└── HH_TECHNIQUES
```

The dedicated track keeps special-technique authoring explicit, inspectable and independent per difficulty chart.

Important distinction:

> **A harder chart may contain more Technique Skill events because the author placed them there, not because the runtime generated them.**

Therefore:
- Easy may legitimately contain zero Technique Skills;
- Normal may introduce one or two;
- Hard/Extreme may use them much more frequently where musically appropriate;
- there is still no automatic density rule or quota;
- every occurrence remains hand-authored in the chart source.

The exact MIDI encoding inside `HH_TECHNIQUES` (note numbers, marker values, duration semantics and optional parameters) is deferred to the authoring-format specification.

## Frequency / musical role

Technique Skills are special moments, not the default input stream.

Typical structure:

```text
Classic phrase
→ Classic phrase
→ authored Technique Skill
→ Classic phrase
→ later Technique Skill
```

They should support musical phrasing, riffs, breakdowns, transitions, climaxes and boss moments.

There is no target quota of Technique Skills per minute.

## Progression model

Classic is always available.

Technique Skills may be learned/unlocked through progression.

A playable chart must not require a Technique Skill that the player cannot legitimately know at that progression point.

Future content metadata may expose, conceptually:

```text
requiredTechniques:
- classic
- deep
- windmill
```

This metadata governs availability/tutorial/progression. It must not alter runtime physics or secretly perform the skill.

## Skill state

Initial progression vocabulary:

```text
LOCKED
LEARNED
MASTERED
```

Current intent:
- LOCKED: not yet available in progression;
- LEARNED: usable and charts may require it;
- MASTERED: milestone/status/achievement-style recognition.

MASTERED must not become an RPG stat that makes the physical execution automatically better.

## Presentation

Technique Skills may temporarily use the gameplay surface itself as a gesture area.

This must not require permanent extra skill buttons.

Standard HH cue visibility remains:

```text
CURRENT + NEXT
```

Technique-specific cue language is deferred to GG5.

## Architecture constraints

Technique Skills must preserve all existing HH invariants:

- AudioClock owns timing;
- neck physics remain continuous;
- base neck physics are not replaced per skill;
- presentation never determines success;
- gesture recognition maps intent, not correctness;
- Timing Quality remains separate from technique execution quality;
- no animation frame is authoritative;
- no skill automatically resets/snaps the neck;
- authored chart data remains deterministic.

## GG1 definition order

Define one skill at a time.

Current order:

1. Half
2. Deep
3. Whiplash
4. Windmill

For each technique, GG1 must freeze:
- player gesture;
- musical/performance meaning;
- authored parameters;
- gesture start/end semantics;
- relevant neck response;
- evidence required for later evaluation;
- failure/degradation semantics;
- interaction with CURRENT + NEXT;
- progression/tutorial expectations.

No TechniqueEvaluator implementation begins until all four Technique Skill contracts are accepted.
