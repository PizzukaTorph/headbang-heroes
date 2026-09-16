# Gameplay Screen v1

## Status

Canonical active-gameplay screen direction.

Exact pixel placement, timing values, and touch ergonomics remain prototype/playtest work.

## Composition

Portrait screen with four visual layers:

1. static 2D multilayer background
2. 3/4 upper-body avatar foreground
3. CURRENT / NEXT cues close to head/neck
4. compact top status bar + local transient feedback

The avatar is the primary visual focus.

## Background

Use layered static 2D venue art with restrained parallax/lighting/crowd/haze reactions.

Background must never compete with cue/head readability.

## CURRENT / NEXT

- CURRENT = strong active inversion/commit cue
- NEXT = restrained preview for momentum preparation

CURRENT should sit in the actual expected inversion zone:
- LEFT: left of head
- RIGHT: right of head
- UP: above head
- DOWN: below jaw/neck without blocking torso

CURRENT and head/neck should read as one perceptual gameplay system.

## Top bar

Canonical layout:

```text
[PAUSE] [SCORE] [SONG PROGRESS] [MULTIPLIER] [HYPE]
```

Target roughly 8–10% total screen height.

Top bar is passive/global state only.

## Local feedback

- judgment near cue/head but not over face/neck
- combo near shoulder/torso-side area
- score/HYPE deltas only when useful

Keep low combo counts quiet; emphasize milestones more strongly.

## HYPE READY

When HYPE reaches maximum, the top-bar HYPE control may temporarily increase visual priority enough to communicate READY peripherally.

The player manually taps it to activate THE BANG.

## THE BANG / Finisher

THE BANG is the temporary enhanced-performance state.

Normal gameplay continues.

The first deterministic suitable authored/performed opportunity inside the window may resolve as a **Finisher**.

`Finisher` is the canonical payoff term. `Special Bang` is legacy terminology.

The screen may intensify body/hair/venue/UI response, but readability remains dominant.

## Visual hierarchy

1. HEAD / NECK + CURRENT
2. AVATAR BODY
3. NEXT
4. JUDGMENT
5. HYPE READY when available
6. COMBO
7. SCORE / PROGRESS / MULTIPLIER / PAUSE

## Starting emphasis targets

Tuning guidance only:
- Avatar: 100% reference
- CURRENT: 60–70%
- NEXT: 25–35%
- Judgment: 35–45%, transient
- Combo: 30–40% at meaningful moments
- HYPE normal: 20–25%
- HYPE READY: may rise to ~45–55%
- passive top-bar state: 10–20%

## Screen-space guidance

Approximate target:
- top 8–10%: status bar
- middle 55–60%: head/neck/CURRENT/NEXT/upper body
- lower 30–35%: torso/arms/foreground + invisible thumb regions

Avatar roughly 55–65% useful visual height, subject to hairstyle/device testing.

## Lower playfield

The lower screen is not a spare HUD strip.

Use it for:
- visible torso/arms
- foreground composition
- large comfortable invisible touch regions
- occasional transient feedback
- breathing room for body/hair movement

No persistent gameplay ads.

## Protected gameplay region

No persistent element may cover:
- face
- neck
- immediate head trajectory
- CURRENT target

Frequent thumb interactions should avoid covering critical visual timing information.

## Conceptual wireframe

```text
+------------------------------------------+
| [II] SCORE      SONG PROGRESS    xMULTI |
|                                    HYPE  |
|                                   [READY]|
+------------------------------------------+
|                 NEXT                     |
|                  .                       |
|                CURRENT                   |
|                   O                      |
|                [ HEAD ]                  |
|                 [NECK]                   |
|             [ SHOULDERS ]                |
|               [ TORSO ]                  |
|      PERFECT!              COMBO 26      |
|                                          |
|      lower playfield / thumb space       |
+------------------------------------------+
```

## Dense-chart readability

Do not visualize every source musical subdivision as a separate discrete cue.

The chart represents performable headbang rhythm. Dense phrases may use slower pulse, Half/Burst, Windmill continuity, or selected accents.

The screen must remain readable as choreography, not become a traditional note highway around the avatar.

## Validation focus

Validate:
- CURRENT/head read as one system
- NEXT helps preparation
- avatar/body response readable
- top bar peripheral
- judgments/combos non-obstructive
- HYPE READY noticeable but controlled
- lower playfield comfortable for thumbs
- cardinal cues readable on real phones
- dense passages remain visually performable
