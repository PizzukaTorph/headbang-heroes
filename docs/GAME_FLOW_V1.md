# Game Flow v1

## Status

Canonical shell/navigation contract for Headbang Heroes.

## North-star rule

> **The shell must not become the game. The game is headbanging.**

Practical consequences:
- Home → Gameplay: as few taps as practical
- Results → Retry: one tap

## High-level flow

```text
BOOT
  ↓
HOME
  ↓
PLAY
  ↓
[MODE SELECT — bypassed when only one relevant mode exists]
  ↓
SONG SELECT
  ↓
PRE-SONG
  ↓
GAMEPLAY
  ↓
RESULTS
  ↓
[OPTIONAL REWARD REVEAL]
  ↓
CONTINUE / RETRY
```

## Boot

Responsibilities:
- studio/game splash as appropriate
- load local profile/save
- initialize required services
- load local catalog/cache
- refresh remote catalog opportunistically when online
- detect first-run state

Do not require network availability merely to launch with valid local profile/content.

## First-run

Preferred first-run flow:

```text
BOOT
→ CREATE / CHOOSE BASE AVATAR
→ VERY SHORT TUTORIAL
→ FIRST SONG
→ RESULTS
→ HOME
```

Do not force a deep character creator before first play.

The avatar step should create enough ownership for the player to think “that’s me” and move on.

## Home

Baseline destinations:
- PLAY
- AVATAR
- PROFILE
- SETTINGS

PLAY is dominant.

The avatar is the dominant visual object on Home and uses the selected idle presentation where appropriate.

The Home screen should show the player, not aggressively sell to the player.

## Modes

Current planned modes:
1. Story
2. Quick Headbang
3. Versus
4. Practice
5. Endurance

When multiple implemented modes exist:

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

If only one relevant mode exists, bypass Mode Select.

Do not show fake/unimplemented functional modes in production UI.

Detailed mode contracts live in `GAME_MODES_V1.md`.

## Song Select

Song Select selects playable authored content.

Important model:

> **The player selects a song/chart; the game does not assume every song has four difficulty variants.**

Baseline information:
- artwork
- title / artist
- authored difficulty
- duration
- best score
- best grade
- download/cache state if relevant
- lock/unlock state if relevant
- compact genre/subgenre metadata where useful

Official catalog filters:

```text
ALL | EASY | NORMAL | HARD | EXTREME
```

### Multiple charts for one song

If a song has more than one authored chart, available alternatives may be selected in the song detail/pre-song context.

Do not show a redundant difficulty selector for songs that have only one playable chart.

## Pre-song

Keep brief.

Purpose:
- confirm song/chart
- show authored difficulty
- show current avatar
- optionally preview notable technique vocabulary
- ensure/download required content before gameplay

Primary action: START.

Do not turn Pre-song into a loadout-management screen.

## Gameplay

Gameplay follows:
- `GAMEPLAY_UX_V1.md`
- `GAMEPLAY_SCREEN_V1.md`
- `SCORING_SYSTEM_V1.md`
- `BODY_SYSTEM_V1.md`
- `HAIR_SYSTEM_V1.md`
- `TECHNICAL_CONTRACTS_V1.md`

No unrelated shell/monetization UI overlays active play.

## Results

Results follows `RESULTS_SCREEN_V1.md`.

Flow:
1. grade / emotional impact
2. performance report
3. progression rewards

Actions:
- RETRY
- CONTINUE

## Retry

One tap from Results.

```text
RETRY
→ brief deterministic pre-roll/countdown
→ GAMEPLAY
```

Retry preserves:
- song
- chart
- avatar
- mode
- relevant settings/calibration

No route back through Home/Song Select is required.

## Continue

Return to the nearest useful context.

Default:
- normal run → Song Select for current mode
- mode-specific runs → appropriate mode context

Do not bounce through Home unnecessarily.

## Reward Reveal

Routine XP/HH belongs in Results.

Dedicated reward reveal is optional and reserved for meaningful events such as:
- level up
- new cosmetic/content unlock
- meaningful milestone

Do not stack nested reward modals.

## Avatar

AVATAR owns presentation choices:
- body/face
- hair
- beard/makeup/piercings/tattoos
- apparel/special
- Performance Style
- Idle Style

Avatar choices never grant gameplay power.

The UI contract lives in `AVATAR_CUSTOMIZATION_UX_V1.md`.

## Profile

PROFILE owns player identity/progression information:
- display name
- level / XP
- HH
- records
- aggregate stats
- future badges/titles

Profile is separate from avatar appearance.

## Settings

SETTINGS owns:
- audio
- graphics/performance
- controls
- calibration
- accessibility
- haptics

Reduced flashes/shake/haptics options must not change scoring potential.

## Offline behavior

With valid local profile and downloaded content, network loss must not block:
- launch
- song play
- retry
- local record/progression update
- avatar/settings changes

Remote sync/catalog refresh may resume later.

## Guardrails

- PLAY is primary Home action.
- Mode Select exists only when it creates a real choice.
- Song Select is chart-aware and does not assume four difficulties per song.
- Pre-song stays short.
- Routine rewards remain inside Results.
- Retry is one tap.
- Avatar dominates Home more than promotional UI.
- Profile and Avatar remain separate concepts.
- Shell navigation never overshadows gameplay.

## v1 acceptance criteria

The flow is behaving correctly when:
- returning player reaches gameplay quickly
- first-run player reaches a real song quickly
- songs with one chart do not show pointless difficulty navigation
- songs with multiple charts can expose those choices without redesigning Song Select
- retry is immediate
- local/offline content remains usable
- future Mode Select can expand to all five planned modes without changing gameplay semantics
