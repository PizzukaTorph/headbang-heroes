# Progression v1

> Status: intentionally simple meta-progression for Headbang Heroes. The purpose is to reward replay, unlock expression/content, and avoid power creep or RPG-style stat systems.

## North-star rule

**Progression unlocks expression and content, not power.**

Nothing obtained through progression may directly improve timing windows, Motion Quality, neck simulation, HYPE gain, scoring potential, or any other gameplay-performance variable.

A cosmetic must never make the player mechanically stronger.

## Core loop

```text
PLAY SONG
  ↓
RESULTS
  ↓
XP + HH
  ↓
LEVEL UP? → reward / unlock
  ↓
COSMETICS / SONGS / DIFFICULTIES
  ↓
REPLAY
```

The system should remain readable enough that players immediately understand what they earned and why.

## Progression currencies

Headbang Heroes v1 uses two basic progression values:

1. **XP** — profile/account progression.
2. **HH** — earnable soft currency for cosmetics and selected unlocks.

Do not add additional currencies unless a later design problem genuinely requires them.

## XP

XP advances the player profile level.

The initial model should be deliberately simple and fully data-driven:

```text
XP Earned = BaseSongXP × GradeModifier
```

Initial candidate grade modifiers:

| Grade | XP modifier |
| --- | ---: |
| D | 0.70 |
| C | 0.85 |
| B | 1.00 |
| A | 1.20 |
| S | 1.50 |

These values are tuning defaults, not hard-coded balance law.

`BaseSongXP` may vary by song/difficulty if needed, but the model should remain transparent and predictable.

## Player Level

Player Level is primarily a long-term profile/progression marker.

Levels may unlock:

- cosmetic items
- avatar customization categories/items
- Performance Styles
- Idle Styles
- songs
- difficulties
- titles/badges/profile flair

Player Level must not unlock gameplay power.

Examples of forbidden level rewards:

- wider timing windows
- easier Motion Quality thresholds
- extra score multiplier
- faster HYPE generation
- stronger neck movement
- automatic combo protection

## HH currency

HH is the standard earnable soft currency.

Its primary use is purchasing cosmetic expression.

Candidate sources:

- normal song completion
- grade/performance reward
- first clear bonus
- new personal best
- first S on a song/difficulty
- progression milestones
- selected challenge/milestone rewards

Avoid turning HH rewards into noisy slot-machine-style payout presentation.

HH rewards should be understandable and mostly deterministic.

## Cosmetics

Primary HH spending targets may include:

- hairstyles
- facial hair
- makeup / corpse paint
- tattoos
- piercings
- apparel
- special cosmetics
- profile flair
- future presentation-only customization

Cosmetics must remain mechanically neutral.

The player should be able to build identity through appearance without changing scoring potential.

## Songs and difficulties

Songs and difficulty levels may unlock through progression, milestones, or explicit campaign/content rules.

The exact unlock graph is not fixed by this document.

Guardrail:

**content gating should create progression, not grind walls.**

Players should spend most of their time playing songs because they want to improve, explore content, or earn expression — not repeating trivial content solely to fill a meter.

## Performance Styles and Idle Styles

Performance Styles and Idle Styles are valid progression rewards because they affect presentation rather than gameplay power.

Examples:

- Doomer
- Thrasher
- Death
- Black
- future genre/performance identities

Idle options may include:

- Style Default
- Stare
- Nod
- Sway
- Stretch
- Loose

These remain cosmetic/presentational and must not affect chart semantics, scoring, or Motion Quality.

## Personal records

For every song/difficulty combination, persist at minimum:

- best score
- best grade
- longest combo

Additional useful historical records may include:

- best HYPE earned
- best Finisher count
- first-clear state
- first-S state

Only store/display extra records if they provide useful player feedback rather than clutter.

## Rewards presentation

The Results flow should present progression after the performance report.

Suggested order:

```text
FINAL GRADE / SCORE
↓
PERFORMANCE REPORT
↓
XP EARNED
↓
LEVEL PROGRESS / LEVEL UP
↓
HH EARNED
↓
UNLOCKS / REWARDS
↓
RETRY / CONTINUE
```

Rewards should never overpower the grade/performance identity of the Results screen.

## Level-up behavior

When a level threshold is crossed:

- clearly communicate the new level
- show any newly unlocked content
- keep the sequence short
- do not interrupt the player with multiple nested reward screens

One compact reward reveal is preferred over a chain of modal dialogs.

## First-clear and milestone bonuses

Small milestone bonuses are allowed to encourage exploration and mastery.

Candidate milestones:

- first clear
- first A
- first S
- new high score
- new longest combo
- first successful Finisher
- song/difficulty mastery milestones

The exact reward values should remain configurable.

## No power progression

The following systems are explicitly out of scope for v1:

- skill trees
- stat points
- gear rarity with gameplay bonuses
- equipment power
- neck strength stats
- timing-window upgrades
- HYPE-generation upgrades
- combo shields
- passive score bonuses from cosmetics
- pay-to-win progression

If a later feature proposes one of these, it should be treated as a major design change rather than a natural extension of this progression system.

## Monetization boundary

Current progression design assumes normal play can earn the primary soft currency and progression rewards.

No gameplay ads are required by this system.

Any future monetization design must preserve the core rule that purchased appearance does not grant gameplay power.

Monetization should be designed separately rather than embedded into the progression formulas.

## Data-driven configuration

All numerical values must be data-driven, including:

- BaseSongXP
- grade XP modifiers
- level thresholds
- HH base rewards
- first-clear bonuses
- milestone bonuses
- song/difficulty unlock requirements

Avoid scattering progression constants through gameplay code.

## Minimal v1 acceptance criteria

The progression system is sufficient for v1 when the game can:

- award XP after a completed song
- advance Player Level
- award HH
- show progress/rewards in Results
- unlock at least one cosmetic/content reward
- persist best score, best grade and longest combo per song/difficulty
- preserve identical gameplay potential regardless of level or equipped cosmetics

Anything beyond this should justify its complexity before entering scope.
