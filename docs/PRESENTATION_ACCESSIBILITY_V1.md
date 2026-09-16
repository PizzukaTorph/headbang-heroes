# Presentation / Accessibility v1

## Audio feedback

Headbang Heroes treats the song as the primary audio experience.

> **Do not add unnecessary gameplay sounds over the music.**

By default:

- no PERFECT chime
- no GOOD/GREAT click
- no MISS buzzer
- no arcade combo sounds
- no generic THE BANG sound layered over the music

Gameplay feedback should primarily use:

- avatar motion
- body/hair reaction
- UI feedback
- haptics
- venue/background reaction

---

## Haptics

Haptics are the preferred non-visual supplemental feedback channel.

Candidate patterns:

```text
PERFECT → short clean pulse
GREAT → lighter pulse
MISS → distinct optional pulse
HYPE READY → alert pulse
THE BANG activation → stronger impact
Finisher → dedicated impact pattern
```

These patterns must remain configurable and may be simplified after device testing.

Required user options:

- haptics on/off
- optional intensity level where platform support is reliable

Haptics must never be required for successful play.

---

## Calibration

Rhythm-game calibration must be planned early.

Persist at least:

```text
audioOffsetMs
inputOffsetMs
visualOffsetMs
```

Initial builds need development diagnostics for signed timing error.

A production calibration flow may be introduced when real-device testing proves what is required.

Calibration changes perception/alignment only; it must not alter authored chart timing or secretly widen judgment windows.

---

## Accessibility / presentation options

Initial accessibility-oriented settings should include:

- reduced flashes
- reduced camera shake
- haptics on/off
- optional haptic intensity

Potential future option:

- high-contrast gameplay cues

Presentation reduction options must not reduce scoring potential or change chart semantics.

---

# Background / Venue presentation

Gameplay uses a static 2D multilayer venue/background system with restrained reactive presentation.

Conceptual model:

```text
VenueDefinition
├── backdrop
├── midLayer
├── foreground
└── reactionProfile
```

Typical visual layers:

```text
backdrop   → stage / wall / venue environment
midLayer   → amps / banners / crowd / lights
foreground → silhouettes / haze / restrained FX
```

The system should remain light enough for mobile and must never compete with head/cue readability.

---

## Venue reaction profile

A venue can react to shared performance state rather than requiring bespoke animation for every chart event.

Suggested inputs:

```text
performanceIntensity 0..1
HYPE value/state
THE BANG active
Finisher event
```

Suggested profile parameters:

```text
baseMovement
hypeMovement
lightResponse
theBangImpact
finisherImpact
cameraPunch
foregroundIntensity
```

Exact numeric values are tuning data.

---

## Presentation hierarchy

The venue remains subordinate to gameplay.

Priority remains:

```text
Head/Neck + CURRENT
Avatar
NEXT
Judgment / local feedback
Venue/background
```

No venue effect may obscure the face, neck, cue target, or required touch-readability zone.

---

## Example venue families

The same lightweight system can later support:

- rehearsal room
- small pub
- underground club
- theater/stage
- festival
- deliberately absurd/special metal environments

These are presentation/content variants, not gameplay modes.

---

# Economy initial tuning placeholders

Economy values are implementation defaults for testing, not final balance.

Suggested initial XP model:

```text
Base clear XP = 100
D = ×0.70
C = ×0.85
B = ×1.00
A = ×1.20
S = ×1.50
```

Suggested HH rewards:

```text
clear       = 50 HH
first clear = +100 HH
new best    = +25 HH
first A     = +50 HH
first S     = +100 HH
```

Suggested cosmetic price bands for tuning:

```text
small/basic cosmetic   ≈ 250 HH
larger/special cosmetic ≈ 500 HH
major/special item      ≈ 1000 HH
```

Do not formalize rarity tiers solely because these price bands exist.

Suggested early level curve may be linear/simple, for example increasing required XP by a fixed amount per level. Exact values must be tuned after real playtime exists.

---

## Economy principles

- progression unlocks expression/content, not power
- rewards should be understandable
- avoid slot-machine presentation
- no gameplay stat bonuses from cosmetics
- do not optimize a freemium economy before the core loop is fun
- all values must be data-driven

---

# Official catalog structure

Initial catalog browsing focuses on official content.

Recommended organization:

```text
OFFICIAL
├── ALL
├── EASY
├── NORMAL
├── HARD
└── EXTREME
```

Genre/subgenre filters may be introduced when the catalog becomes large enough to justify them.

Community content remains a future separate catalog dimension.

---

# QA acceptance contract

The first serious vertical slice should verify at minimum:

- song remains synchronized from start to finish
- frame drops do not shift authored event timing
- pause/resume does not desynchronize playback and chart
- restart/retry does not desynchronize playback and chart
- input always produces physical neck response
- timing judgment remains separate from physical response
- PERFECT/GREAT/GOOD/MISS resolve consistently
- spam naturally produces poor micro-motion rather than being blocked by arbitrary cooldown
- MISS does not snap/freeze/reset the neck
- Motion Quality reflects actual prepared motion
- HYPE accumulates correctly
- THE BANG activates correctly
- Finisher resolves deterministically on valid candidates
- body and hair never feed back into gameplay scoring
- venue/background never obscures gameplay-critical information
- Results reflect authoritative run data
- Retry returns quickly to the same song/setup
- valid save/progression survives restart
- malformed chart/package is rejected clearly
- loss of network does not interrupt an already cached active run
- accessibility presentation options do not alter scoring potential

These are acceptance checks, not a substitute for feel testing.

The final POC criterion remains:

> **Make The Headbang Fun.**
