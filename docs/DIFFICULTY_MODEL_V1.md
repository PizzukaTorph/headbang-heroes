# Difficulty Model v1

## Status

Canonical difficulty design for Headbang Heroes.

Exact timing windows, density thresholds, and vocabulary limits remain playtest/tuning data.

## Core rule

> **Difficulty is a property of an authored chart.**

A song may have one chart or several.

The game must not assume every song exists in every difficulty.

Valid examples:

```text
Song A → Normal only
Song B → Hard only
Song C → Extreme only
Song D → Normal + Hard
```

The catalog itself may carry the difficulty curve.

## Difficulty tiers

Canonical labels:
- Easy
- Normal
- Hard
- Extreme

A chart has one authored difficulty classification.

## North-star rule

> **Higher difficulty does not make the neck artificially harder to control. It asks the player to control the same neck through harder choreography.**

Do not modify neck physics, Motion Quality formulas, damping, impulse, or physical limits merely because the chart is Hard or Extreme.

## What difficulty describes

Difficulty is informed by a combination of:
- event density
- performable rhythmic detail
- timing-window tolerance
- technique vocabulary
- trajectory changes
- modifier usage
- momentum-planning demand
- recovery demand
- phrase complexity
- authored Rest/stillness requirements

No single variable defines difficulty alone.

## Musical subdivision is not input subdivision

A chart may represent detailed musical information without requiring one discrete player input for every source subdivision.

Hard rule:

> **The chart follows the performable headbang rhythm, not every musical subdivision present in the source.**

For example, a drum passage containing 1/32 notes may be interpreted as:
- headbang pulse at 1/8
- headbang pulse at 1/16
- `Half + Burst`
- sustained Windmill continuity
- selected authored accents

The goal is to author plausible neck performance, not to transcribe a drum MIDI lane into touch spam.

Authoring concept:

```text
MUSIC LAYER       what happens in the song
PERFORMANCE LAYER what the neck should perform
CHART LAYER       how HH encodes it
```

## Performable density guidance

These are authoring expectations, not hard numeric ceilings.

### Easy
- sparse, obvious pulse
- large preparation windows
- mostly Classic
- simple Horizontal/Vertical movement
- few modifiers
- generous timing tolerance
- minimal momentum planning

### Normal
- primary musical groove
- moderate event density
- Classic + early Half/Deep exposure as appropriate
- readable trajectory changes
- simple modifiers
- standard timing tolerance
- momentum awareness useful

### Hard
- denser performable choreography
- broader vocabulary
- more frequent direction/trajectory changes
- stronger use of Half/Deep/Whiplash/Accent/Hold/Double where musically justified
- tighter timing tolerance
- momentum planning important

### Extreme
- full supported vocabulary where appropriate
- highly demanding but still human-readable choreography
- aggressive transitions
- tighter timing tolerance
- sustained momentum planning and recovery skill
- dense phrases may use continuous/compound gestures rather than impossible discrete tapping

Extreme means a highly demanding performance interpretation, not “copy every note into an input event.”

## Technique vocabulary by difficulty

This table is guidance, not a mandatory unlock law.

| Difficulty | Typical vocabulary |
|---|---|
| Easy | Classic, simple Rest |
| Normal | Classic, Half, introductory Deep, simple modifiers |
| Hard | Classic, Half, Deep, Whiplash, introductory Windmill, broader modifiers |
| Extreme | Full supported vocabulary, dense transitions, full modifier set |

A particular song may omit a technique even on Extreme if the music does not call for it.

## Timing windows

Timing-window tolerance and chart density are separate dimensions.

Exact millisecond values are not frozen in this document.

All judgment windows must be data-driven and validated on real devices.

Dense passages require careful event matching. The runtime must not assume a single obvious active event when multiple authored candidates can fall near the same input time.

## Candidate matching requirement

For dense charts, input should be resolved against a bounded set of unresolved compatible events around authoritative song-time.

Matching should consider at minimum:
- timing proximity
- event eligibility
- semantic compatibility (direction/trajectory/technique contract)

A future event should not be accidentally consumed merely because it is the next array element.

Wrong semantic action inside an eligible window may consume the matched event as MISS according to the canonical event-resolution contract.

Too-early input may still physically move the neck without consuming the future event.

## Motion demand

Motion demand emerges from choreography.

Sparse patterns naturally allow larger amplitude and easier preparation.
Dense patterns naturally require tighter control, partial excursions, rapid recovery, or continuous techniques.

Difficulty must never apply an invisible Motion Quality handicap.

## Rest

Rest exists at every difficulty when musically justified.

### Natural Rest
No authored event; player may continue moving or stop freely.

### Authored Rest
Explicit required stillness interval.

Preferred semantics:

```text
settling phase
→ player bleeds required preceding momentum
→ stillness evaluation phase
→ Rest outcome
```

This avoids punishing the player for carrying momentum that the immediately preceding chart legitimately required.

Exact settling/evaluation durations and stillness thresholds remain data-driven.

## THE BANG across difficulty

THE BANG uses the same control contract at every difficulty:
- build HYPE
- reach READY
- manually activate
- continue normal gameplay
- resolve Finisher when valid

A denser chart must not automatically become the best XP/HH farming strategy merely because it contains more events. Gameplay HYPE and meta rewards may be normalized separately.

## Catalog interaction

Difficulty can be used as a top-level catalog filter:

```text
ALL | EASY | NORMAL | HARD | EXTREME
```

If a song has only one chart, there is no redundant per-song difficulty choice.

If a song has multiple charts, the available alternatives may be selected locally in Song Select/Pre-song.

## Data-driven configuration

Keep configurable:
- judgment windows
- authoring density guidance
- technique/trajectory/modifier availability hints
- tutorial introduction gates
- HYPE/reward normalization
- Rest settling/evaluation profiles

## Acceptance criteria

Difficulty design is working when:
- each chart feels musically authored rather than mechanically generated
- Easy is readable without trivializing the physical model
- Normal expresses the core groove
- Hard demands active momentum planning
- Extreme is highly demanding but physically/ergonomically plausible
- dense source music does not devolve into absurd tap spam
- difficulty never changes neck physics merely to make control harder
- Rest remains musically meaningful
- songs are allowed to exist at only the difficulties that make sense for their authored content
