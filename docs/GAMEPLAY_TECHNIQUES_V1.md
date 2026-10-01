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

The dedicated track uses long MIDI notes as the base Technique Skill encoding:

```text
NOTE ON  = authored Technique Skill start
NOTE OFF = authored Technique Skill end
PITCH    = Technique Skill identity
VELOCITY = optional authored intensity
```

The note duration describes the authored musical/gesture window, not an automatic success state and not merely an animation duration.

Per-technique semantics may interpret that window differently. For example, a sustained Windmill may require continuous circular input for most or all of the authored note duration, while Deep may use the same duration as the available time to complete its full-extension gesture.

Hard rule:

> **Holding the note window is never sufficient by itself; the expected gesture must still be recognized and evaluated.**

Exact pitch assignments for Half / Deep / Whiplash / Windmill are deferred to the authoring-format specification.

## Technique Skill: Half

### Status

**Accepted GG1 contract.**

`Half` is the first Technique Skill and represents a short, controlled headbang rather than a full excursion.

It is not a weak Classic input and it does not change the base neck physics. It asks the player to intentionally execute a reduced-range gesture.

### Performance meaning

```text
CLASSIC = normal headbang
HALF    = compact / short / controlled headbang
DEEP    = full-extension headbang
```

Half is especially useful in faster musical phrases where a full excursion would be inappropriate or unreadable.

### Player gesture

The player performs a short linear drag in the required direction.

Conceptually:

```text
START -------------------------- FULL
  ●------------[ HALF ]----------○
```

The player:

1. touches the gesture surface;
2. drags coherently toward the required direction;
3. reaches the authored/tuned Half depth band;
4. releases without continuing into a Deep/full-extension gesture.

The exact normalized Half band is tuning data, not hardcoded gameplay law. An initial implementation may test a band around the middle of the available gesture range, but GG1 does not freeze a numeric percentage.

### Timing ownership

Touch-down is **not** the Half timing event.

The authoritative timing sample is captured when the gesture first enters the valid Half depth band:

```text
touch down
    ↓
linear drag
    ↓
first entry into HALF depth band  ← authoritative technique timing point
    ↓
release
```

This prevents the player from starting the gesture early, parking at the target depth, and receiving an on-time result later.

If the player enters the Half band early, that entry is early. If the player reaches it late, that entry is late.

The MIDI Technique Skill note onset provides the authored target time; its note duration provides the authored gesture/completion window.

Timing Quality remains separate from Half execution quality.

### Direction ownership

The dedicated `HH_TECHNIQUES` track encodes **Half**, not duplicated variants such as:

```text
HALF_LEFT
HALF_RIGHT
HALF_UP
HALF_DOWN
```

Direction remains part of the associated/base chart movement context.

This keeps the Technique Skill lane focused on **how** the movement must be performed while the main chart continues to describe **where** the movement is directed.

The authoring/compiler contract for associating a Technique Skill note with its directional movement event will be defined in the authoring-format work; GG1 only freezes that direction is not duplicated into the Half skill identity.

### Required evidence for later evaluation

GG2 must expose enough gesture/motion evidence to evaluate at least:

- normalized gesture depth / travel;
- first Half-band crossing time;
- directional coherence;
- overshoot beyond the Half range;
- gesture completion/release;
- relevant neck travel generated by the TechniqueIntent.

Candidate diagnostics may also include gesture duration and speed, but Half is fundamentally a **range-control** skill rather than a speed skill.

### Success / degradation semantics

A valid Half should require:

- movement toward the required direction;
- entry into the valid Half depth band;
- no excessive overshoot into the Deep/full-extension range;
- a valid release/completion inside the authored Technique Skill window.

Failure to execute Half must never cancel the player's physical input. The neck still responds to what the player actually did.

Examples:

```text
too short                  → Half not completed / degraded
correct Half depth         → valid Half execution
overshoots toward full     → poor/failed Half; physical movement remains
wrong direction            → semantic failure according to event resolution
correct gesture, bad time  → technique may be valid while Timing Quality is poor
```

The exact pass/degrade thresholds are tuning data and belong to GG3, not this contract.

### Interaction with CURRENT + NEXT

Half remains part of the standard HH cue budget:

```text
CURRENT + NEXT
```

It does not add a third persistent cue or a permanent skill button.

GG5 will define the final visual vocabulary that distinguishes an upcoming Half from a Classic event while preserving direction readability.

### Progression / tutorial

Half is a learned Technique Skill.

The tutorial should teach one idea only:

> **Stop halfway on purpose.**

A training sequence should first demonstrate the Half depth target slowly, then require it in time, then mix it back into Classic gameplay.

Mastery remains recognition/progression metadata and must not make Half easier through hidden gameplay stats.

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

## Current implementation contract: HALF and DEEP

The current implementation restores the Half depth-band semantics described
above. The accepted gesture vocabulary is:

```text
CLASSIC     TAP
HALF        → short horizontal swipe
DEEP        ↓ long vertical downward swipe
WHIPLASH    ⇆ reserved / not implemented
WINDMILL    ⟳ reserved / not implemented
```

Half is a continuous controlled partial-range gesture. The recognizer exposes
current travel, peak travel, target-band entry time, overshoot, release and
directional coherence. Its target band and fail-overshoot threshold are
tuning data in `game/config/tuning/base.json`; the default Half direction is
configurable so a future handedness/accessibility mirror does not change the
semantic skill ID. Deep remains the current long downward-travel POC and uses
the shared continuous evidence path without receiving a full V2 redesign yet.

Gesture direction identifies the skill, not the authored neck direction. An
authored `DEEP` event can therefore resolve a left or right neck stroke. The
runtime representation keeps both fields on the same compiled event:

```json
{"id":"tech-002","technique":"deep","directionName":"left","time":5.5,"duration":1.0}
```

Technique events originate from the dedicated `HH_TECHNIQUES` lane in the
canonical MIDI model (or the equivalent compiled JSON fixture for this POC).
They are authored occurrences, not BPM/difficulty-generated events. Their
`time` starts the actionable window and their `duration` ends it. For Half, the
authoritative technique timing sample is the first entry into the target band,
measured in SongClock time; release is a separate completion signal with a
tuned grace period. CURRENT + NEXT remains the complete cue budget; the POC uses
`→ HALF` and `↓ DEEP` glyphs without adding a third cue.

The recognizer emits continuous evidence and a semantic intent; it does not
score it. The application continuously sends normalized technique progress to
the authoritative neck model, including failed or incomplete attempts. A
malformed or out-of-window gesture fails without snapping or resetting the
neck. Erik's sprite sequence follows live progress as presentation only.

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
