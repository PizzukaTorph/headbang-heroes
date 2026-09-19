# Gameplay UX v1

## Status

Canonical active-gameplay UX direction for Headbang Heroes.

Touch ergonomics still require real-device validation.

> **v0.0.2 amendment — see [ADR 0001](DECISIONS/0001-pulse-in-sector-cue.md).** The primary timing
> cue moves from a closing circle on the head to a **pulse-in-sector with build-up** (in the
> direction's screen quadrant, culminating on the event). The head stays the expressive focus but no
> longer carries the primary timing signal. "Play by watching the avatar" is relaxed: WHEN+WHERE are
> unified in the tap sector. Still not a note-highway (one sector pulses at a time, transient).

## North-star rule

> **The player should be able to play by watching the avatar, not by staring at a detached interface.**

The head/neck is part of the interface.

## Gameplay composition

Portrait mobile baseline:

1. static 2D multilayer background
2. large 3/4 upper-body avatar
3. CURRENT / NEXT cues near the head
4. compact persistent top bar + transient local feedback

The avatar/head is the primary visual focus.

## CURRENT / NEXT

- CURRENT: strong active cue
- NEXT: restrained ghost preview

NEXT exists because momentum preparation matters.

Cue timing remains AudioClock/ChartRuntime authoritative. Visual preview never shifts authored time.

CURRENT should read spatially as the expected inversion/commit point.

## Technique visual grammar

Technique differences should become learnable visual language rather than instruction text during normal play.

Examples:
- Classic: standard closing inversion cue
- Half: reduced-excursion language
- Deep: wider/slower/heavier commitment language
- Whiplash: strong flick/directional treatment
- Windmill: continuous circular/phase language
- Authored Rest: settling/stillness language

Exact art treatment remains iterative.

## Top bar

Canonical layout:

```text
[PAUSE] [SCORE] [SONG PROGRESS] [MULTIPLIER] [HYPE]
```

Only persistent/global state belongs here.

Do not permanently put CURRENT/NEXT, judgment spam, or large combo counters in the top bar.

## Local feedback

Near the avatar/action area:
- PERFECT / GREAT / GOOD / MISS
- combo milestones
- small useful score/HYPE deltas

Feedback must be brief and must not obscure face, neck, trajectory, or active cue.

## Avatar as feedback

Primary feedback chain:

```text
neck motion
→ body follow-through
→ hair/secondary motion
→ presentation punch
```

A PERFECT-timed weak movement should still look physically weak.

This visually teaches the separation between Timing Quality and Motion Quality.

## HYPE

HYPE represents performance intensity.

Baseline numerical generation lives in `SCORING_SYSTEM_V1.md`.

As HYPE rises, presentation may intensify through avatar/body/hair response, venue/crowd/light reaction, restrained camera response, and UI emphasis.

Accessibility settings can reduce intense presentation independently from scoring.

## THE BANG

When HYPE reaches maximum, the HYPE control enters READY.

The player manually activates THE BANG by tapping HYPE.

THE BANG:
- is temporary
- keeps normal gameplay active
- may amplify configured scoring/reward/presentation
- introduces no unrelated control scheme or detached QTE

## Finisher

Canonical payoff name: **Finisher**.

Flow:

```text
HYPE FULL
→ player activates THE BANG
→ normal gameplay continues
→ deterministic suitable authored/performed moment
→ FINISHER
```

A Finisher uses the existing neck action being performed.

`Special Bang` is legacy wording and should not be used in new UX/content terminology.

## Touch ergonomics

Visual target and physical touch region are separate concerns.

Spatial precision is not the primary skill test. Timing, momentum, movement preparation, and technique are.

The current four-cardinal-wedge mapping is a prototype hypothesis.

Validate at least:
1. four large cardinal wedges
2. expanded lower thumb-reach regions mapped to cardinal semantics
3. contextual two-thumb mappings where useful, especially for Vertical content

The winning mapping should minimize fatigue, accidental direction changes, obstruction, and reach difficulty.

## Physical input semantics

Any valid semantic gameplay input changes neck motion, even if it is:
- too early
- too late
- wrong direction
- outside an active scoring event

Judgment is a separate layer.

Wrong semantic input within a matched eligible event window may consume that event as MISS according to the canonical runtime contract.

Too-early input may move the neck without consuming a future event.

## Dense passages

Gameplay UX must not become a visualization of every musical source subdivision.

> **The chart follows the performable headbang rhythm, not every musical subdivision.**

Dense metal passages should use physically readable/performable headbang interpretation rather than impossible discrete cue spam.

## Hands should not cover gameplay

Frequent touch regions should not require thumbs over the head/CURRENT cue.

The HYPE top-bar tap is a low-frequency intentional exception.

## UX validation gate

Before finalizing input UX, validate on phones:
- CURRENT immediately readable
- NEXT useful but subordinate
- LEFT/RIGHT comfortable
- UP/DOWN comfortable
- hands do not obscure timing information
- technique changes readable
- weak vs strong motion visually obvious
- top-bar state readable peripherally
- HYPE READY obvious without distraction
- HYPE tap does not break rhythm flow
- dense passages remain human-readable rather than note-lane spam

## Scope boundary

This document covers active gameplay UX only. Shell, Song Select, Avatar, Results, and other flows are specified separately.
