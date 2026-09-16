# Gameplay UX v1

> Status: design specification for the current Headbang Heroes branch. Touch ergonomics still require device playtesting before merge to `main`.

## North-star rule

**The player should be able to play by watching the avatar, not by staring at a detached interface.**

The head is part of the interface. Timing information should remain visually attached to the avatar/head area whenever possible.

## Gameplay composition

Portrait mobile remains the baseline.

The avatar/head occupies the central visual focus. LEFT / RIGHT / UP / DOWN interaction zones are large and forgiving, but they do not need to appear as permanent visible buttons.

The active target is communicated near the head using the closing-circle timing language.

Permanent D-pad-style UI should be avoided unless playtesting proves it necessary.

## Current and next cue readability

Because gameplay depends on preparing momentum, showing only the exact current beat may be insufficient.

The preferred visual model is:

- **CURRENT** target: strong/high-contrast closing cue
- **NEXT** target: restrained ghost/preview cue

The NEXT cue exists to support preparation, not to compete with the current judgment target.

For example, during Classic Horizontal the player may see the current RIGHT inversion target strongly while the following LEFT inversion point is visible as a subtle preview.

Cue timing remains chart/audio-clock authoritative. Previewing the next target must never allow player input to move authored song timing.

## Technique visual grammar

Different neck techniques should be readable through consistent visual language rather than explanatory text during active play.

Candidate language:

- **Classic**: standard closing-circle inversion target
- **Half**: visually communicates reduced excursion / target closer to the avatar centre
- **Deep**: slower/heavier approach language emphasizing larger committed movement
- **Whiplash**: stronger directional/flick-oriented visual treatment
- **Windmill**: circular trajectory/rotation language rather than isolated cardinal hits
- **Rest**: stillness/settling language, potentially converging toward the neutral/centre state

Exact graphic treatment remains an art/UX task, but the rule is fixed: technique differences should become learnable visual grammar.

## Feedback hierarchy

The avatar is the primary feedback surface.

A successful high-quality bang should read through:

neck motion -> body follow-through -> hair/secondary motion -> presentation punch

A weak bang with excellent timing should still look physically weak. This allows players to understand the difference between Timing and Motion Quality without relying only on numbers.

Text feedback such as PERFECT / GREAT / GOOD / MISS should remain compact and near the gameplay focus.

Avoid flooding active play with simultaneous score deltas, percentages, motion telemetry and reward text.

Detailed telemetry belongs primarily in debug/dev presentation and post-song results.

## HYPE presentation

HYPE should be readable as a changing state of the whole performance, not only as a meter.

As HYPE rises, presentation may escalate through:

- stronger avatar/body performance
- more expressive hair/secondary motion
- restrained camera punch
- stronger crowd/background response
- lighting and VFX intensity
- slightly more energetic UI treatment

A compact HYPE indicator may remain for explicit readability, but players should increasingly perceive HYPE without needing to inspect the meter.

Accessibility settings must be able to reduce camera shake, flashes and other intense feedback independently of gameplay state.

## Touch ergonomics

Visual target position and physical touch region are separate concerns.

A target may appear above, below, left or right of the avatar while its corresponding physical touch region remains large enough to reach comfortably with thumbs on a portrait phone.

Spatial precision is not intended to be a major skill test. The core skill tests are timing, momentum, technique and motion quality.

The current implementation uses four large cardinal screen regions. This is a prototype hypothesis, not a permanently locked ergonomic solution.

Before finalizing input layout, test at least these candidates on real phones:

1. **Four cardinal wedges** around screen centre.
2. **Expanded thumb-reach regions** where visual cardinal targets map to larger lower-screen touch areas.
3. **Technique-contextual two-thumb mapping**, especially for Vertical play, where the same comfortable thumb regions may change semantic meaning based on the active technique/trajectory.

The winning layout should minimize reach fatigue, accidental direction changes and screen obstruction while preserving immediate learnability.

## Hands should not cover the game

Touch regions should be designed so frequent play does not require the player's thumbs to obscure the avatar's head or active timing cue.

This is especially important for UP/DOWN interactions in portrait orientation.

## UX validation gate

The visual language may be designed ahead of implementation, but touch ergonomics must be validated on real devices.

Before merging a finalized gameplay-input UX into `main`, confirm that:

- CURRENT target is immediately readable
- NEXT preview helps rather than distracts
- LEFT/RIGHT play is comfortable
- UP/DOWN play is comfortable
- hands do not obscure critical timing information
- technique changes are understandable without large instructional overlays
- avatar feedback communicates weak vs strong movement
- a player can complete a short run without needing to watch peripheral HUD elements

## Scope boundary

This document covers active gameplay UX only.

Song select, campaign navigation, avatar creator, store, results and other shell/menu UX should be designed separately so they do not force compromises into the core playfield.
