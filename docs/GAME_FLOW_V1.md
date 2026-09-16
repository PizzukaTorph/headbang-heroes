# Game Flow v1

> Status: baseline shell/navigation contract for Headbang Heroes. The goal is to keep the route into gameplay fast while leaving room for future game modes, account customization and avatar customization.

## North-star rule

**The shell must not become the game. The game is headbanging.**

Two practical consequences:

- From **Home to Gameplay**: as few taps as practical.
- From **Results to Retry**: one tap.

The navigation should remain simple enough that players spend most of their time either playing, choosing content, or customizing their identity.

## High-level flow

```text
BOOT
  ↓
HOME
  ↓
PLAY
  ↓
[MODE SELECT — bypassed while only one mode exists]
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

Boot should be minimal.

Responsibilities:

- studio/game logo as appropriate
- load profile/save data
- initialize required services/content
- detect first-run state

Avoid long unskippable intros or redundant splash sequences.

## First-run flow

The first run should get the player to the core mechanic quickly.

Preferred baseline:

```text
BOOT
  ↓
CREATE / CHOOSE BASE AVATAR
  ↓
VERY SHORT TUTORIAL
  ↓
FIRST SONG
  ↓
RESULTS
  ↓
HOME
```

Do not force a deep avatar creator before the player has experienced the game.

The first-run avatar step should be fast enough to establish ownership/identity without delaying the first headbang session.

## Home

Home is the central shell node.

Baseline destinations:

- **PLAY**
- **AVATAR**
- **PROFILE**
- **SETTINGS**

PLAY is the dominant primary action.

### Avatar-centric Home

The player's own avatar should be the dominant visual element of the Home screen.

The avatar may appear large in a 3/4 presentation over a restrained background/backdrop and should use its selected idle style.

This gives customization visible value outside gameplay and makes the player's identity part of the shell itself.

Conceptual layout:

```text
+-----------------------------+
| profile / level / HH        |
|                             |
|                             |
|        PLAYER AVATAR        |
|         large 3/4           |
|         idle active         |
|                             |
|                             |
|          [ PLAY ]           |
|                             |
| AVATAR  PROFILE  SETTINGS   |
+-----------------------------+
```

The Home screen should show the player, not aggressively sell things to the player.

## Play and future Mode Select

The architecture should support multiple game modes from the beginning even if v1 ships with only one active mode.

Target structure:

```text
PLAY
  ↓
MODE SELECT
  ├─ Mode 1
  ├─ Mode 2
  ├─ Mode 3
  └─ Mode 4
```

There are already multiple future mode concepts in mind, so the shell should not hard-code PLAY directly to one permanent mode forever.

However:

- if only one mode exists, **bypass Mode Select**
- do not show an empty or fake mode-selection screen
- introduce the mode-selection step only when it creates a real player choice

This keeps v1 fast without blocking future expansion.

## Song Select

Song Select should combine the content choice and difficulty choice in one context wherever practical.

Show enough information to make a decision without turning the screen into a statistics dashboard.

Baseline information:

- song/title/artist metadata as available
- difficulty selector
- best score for selected difficulty
- best grade
- longest combo or another compact personal record if useful
- clear PLAY/START action

Avoid requiring separate screens for song choice and difficulty unless later UX testing proves it beneficial.

## Pre-Song

Pre-Song should be brief.

Purpose:

- confirm selected song
- confirm difficulty
- show current avatar/equipment state
- optionally preview notable technique vocabulary present in the chart

Examples of useful technique preview:

- Classic
- Vertical
- Windmill
- Deep

This is especially useful while players are still learning the visual grammar.

The pre-song screen should not become a loadout-management bottleneck.

Primary action: **START**.

## Gameplay

Gameplay follows the contracts defined in:

- `GAMEPLAY_UX_V1.md`
- `GAMEPLAY_SCREEN_V1.md`
- `SCORING_SYSTEM_V1.md`
- `BODY_SYSTEM_V1.md`
- `HAIR_SYSTEM_V1.md`

The shell must hand control over cleanly and avoid overlaying unrelated navigation or monetization UI during active play.

## Results

Results follows `RESULTS_SCREEN_V1.md`.

The results flow is presentation-first:

1. final grade / emotional impact
2. performance report
3. progression rewards

Baseline post-song actions:

- **RETRY**
- **CONTINUE**

## Retry

Retry must be deliberately fast.

From Results:

```text
RETRY
  ↓
brief countdown / pre-roll
  ↓
GAMEPLAY
```

Do not route the player back through Home, Song Select or a full Pre-Song screen when retrying the exact same configuration.

Retry preserves:

- song
- difficulty
- current avatar/equipment
- current game mode

This is important for score chasing and rhythm-game flow.

## Continue

CONTINUE returns the player to the nearest useful shell context.

Default behavior:

- after a normal song run: return to Song Select for the current mode
- if shell architecture later requires a different local context, preserve the principle of minimizing redundant navigation

Home remains available but does not need to be the destination after every run.

## Reward Reveal

Do not create a separate reward screen after every normal song.

Routine rewards such as:

- XP
- HH currency

should be included directly in Results.

Use a dedicated Reward Reveal only for meaningful events such as:

- level up
- new cosmetic unlock
- new song unlock
- new difficulty unlock
- major milestone reward

Conceptually:

```text
RESULTS
  ├─ normal rewards → CONTINUE / RETRY
  └─ meaningful unlock → REWARD REVEAL → CONTINUE / RETRY
```

This keeps ordinary runs fast while allowing major unlocks to feel special.

## Avatar

AVATAR is the character-customization area.

It owns presentation choices such as:

- face/body variants
- hair
- beard
- piercings
- makeup / corpse paint
- tattoos
- apparel
- special cosmetics
- performance style
- idle style

Avatar customization is separate from account/profile identity.

Avatar customization must not grant gameplay power.

## Profile

PROFILE represents the player's account/progression identity rather than the avatar's appearance.

Candidate content includes:

- player name
- level / XP
- HH balance
- best records
- aggregate performance stats
- badges/titles/profile flair in future versions

Keep Profile informational and identity-focused rather than mechanically powerful.

## Settings

SETTINGS owns system-level preferences such as:

- audio
- graphics/performance
- controls
- accessibility
- feedback intensity

Accessibility options should include the ability to reduce flashes, shake and intense presentation independently from scoring/gameplay state.

## Account vs Avatar

Keep these concepts distinct:

**Profile / Account**
- who the player is in the progression system
- level, stats, records, identity metadata

**Avatar**
- how the player's performer looks and presents
- cosmetics, performance style, idle style

This separation avoids mixing permanent profile identity with character loadout/customization.

## Monetization / shell guardrails

Current direction:

- no persistent ads during gameplay
- no aggressive monetization takeover on Home
- no forced store screen in the core loop
- do not interrupt Retry with monetization surfaces

Monetization, if introduced later, must remain separate from the core gameplay/navigation contract.

## Navigation guardrails

- PLAY is the primary Home action.
- Mode Select exists only when multiple active modes justify it.
- Song and difficulty selection should stay compact.
- Pre-Song must remain brief.
- Results carries routine rewards directly.
- Reward Reveal is exceptional, not mandatory after every run.
- Retry is one tap from Results.
- Do not return to Home unnecessarily between songs.
- Avatar customization is visible and valuable through the Home avatar presentation.
- The player's avatar, not promotional UI, should dominate Home visually.

## v1 flow acceptance criteria

The flow is behaving correctly when:

- a returning player can reach gameplay quickly
- a new player reaches the first playable song without a long setup process
- Retry does not require redundant navigation
- routine rewards do not add unnecessary screens
- meaningful unlocks still receive special presentation
- Home clearly prioritizes PLAY and the player's avatar
- future Mode Select can be inserted without redesigning the entire shell
- Profile and Avatar remain conceptually separate
- shell navigation never overshadows the headbang gameplay itself

## Scope boundary

This document defines the navigation/shell contract.

It intentionally does not fully define:

- the exact four future game modes
- detailed avatar creator UX
- detailed Profile information architecture
- exact visual styling of menu screens
- monetization systems

Those should be specified separately without breaking this flow contract.
