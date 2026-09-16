# Gameplay UX v1

> Status: design specification for the current Headbang Heroes branch. Touch ergonomics still require device playtesting before merge to `main`.

## North-star rule

**The player should be able to play by watching the avatar, not by staring at a detached interface.**

The head is part of the interface. Timing information should remain visually attached to the avatar/head area whenever possible.

## Gameplay composition

Portrait mobile remains the baseline.

The active gameplay screen is built from four visual layers:

1. **Static 2D multilayer background**
2. **3/4 upper-body avatar in the foreground**
3. **CURRENT / NEXT gameplay cues near the avatar**
4. **Persistent top status bar plus local transient feedback**

The avatar/head occupies the central visual focus. LEFT / RIGHT / UP / DOWN interaction zones are large and forgiving, but they do not need to appear as permanent visible buttons.

The active target is communicated near the head using the closing-circle timing language.

Permanent D-pad-style UI should be avoided unless playtesting proves it necessary.

## Background presentation

The gameplay environment should be a **static 2D background assembled from multiple layers**, not a fully simulated 3D scene.

Typical layers may include:

- distant venue / wall / stage backdrop
- mid-ground speakers, banners, lights or crowd elements
- restrained foreground silhouettes, haze, lighting or framing elements

The multilayer setup may support subtle parallax and HYPE-driven presentation changes while remaining cheap to produce and predictable on mobile.

Background response must remain secondary to gameplay readability. HYPE may increase crowd motion, light intensity, subtle pulse/shake, parallax or effects, but the environment must never obscure the avatar or timing cues.

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

## Top status bar

The top bar contains **persistent/global state only**. Immediate performance feedback remains local to the avatar.

Preferred v1 layout:

`[PAUSE]   [SCORE]   [SONG PROGRESS]   [MULTIPLIER]   [HYPE]`

### Pause

A compact pause affordance remains visible at the upper edge of the screen.

### Score

Score is persistent information and belongs in the top bar rather than competing with moment-to-moment judgments around the avatar.

### Song progress

Use a small, low-noise progress indicator. It exists to answer “how much of the song remains?” without becoming a focal element.

### Multiplier

The current score multiplier may remain visible as compact persistent state. The large combo count itself remains local and transient.

### HYPE

HYPE occupies a prominent but compact position in the top bar and is also an interaction target once full.

Do not permanently place judgment text, combo counts, CURRENT/NEXT cues or score-delta spam inside the top bar.

## Local feedback

Moment-to-moment feedback appears **in loco**, close to the action that caused it.

Examples:

- PERFECT / GREAT / GOOD / MISS near the head/cue region
- combo count near a shoulder or torso-side region
- small score or HYPE gain popups near the avatar when useful

These elements should appear briefly and disappear quickly. They must never accumulate into a dense information cloud.

The combo should feel alive rather than like a static dashboard number. It may gain emphasis as the streak becomes notable, while still remaining subordinate to the avatar and cue readability.

## Feedback hierarchy

The avatar is the primary feedback surface.

A successful high-quality bang should read through:

`neck motion -> body follow-through -> hair/secondary motion -> presentation punch`

A weak bang with excellent timing should still look physically weak. This allows players to understand the difference between Timing and Motion Quality without relying only on numbers.

Text feedback such as PERFECT / GREAT / GOOD / MISS should remain compact and near the gameplay focus.

Avoid flooding active play with simultaneous score deltas, percentages, motion telemetry and reward text.

Detailed telemetry belongs primarily in debug/dev presentation and post-song results.

## HYPE generation

HYPE represents **how hard the performance is currently hitting**, not merely note completion.

HYPE may be generated by:

- successful moves
- strong Motion Quality
- maintained combo / flow
- technically demanding authored moments
- clean execution across successive events

Exact numerical weighting remains a tuning task.

The important design rule is that good rhythmic play and convincing physical performance both contribute to the feeling of building HYPE.

## HYPE presentation

HYPE should be readable as a changing state of the whole performance, not only as a meter.

As HYPE rises, presentation may escalate through:

- stronger avatar/body performance
- more expressive hair/secondary motion
- restrained camera punch
- stronger crowd/background response
- lighting and VFX intensity
- slightly more energetic UI treatment

A compact HYPE indicator remains in the top bar for explicit readability.

When HYPE reaches maximum, its top-bar treatment changes significantly enough to communicate **READY** without requiring the player to read text. This can include pulse, glow, animation or other controlled emphasis.

Accessibility settings must be able to reduce camera shake, flashes and other intense feedback independently of gameplay state.

## HYPE MAX — THE BANG

Working name: **THE BANG**.

THE BANG is the current direction for the HYPE-max payoff. It should feel analogous to a short limit-break state while remaining inside the existing headbang gameplay vocabulary.

### Activation

- HYPE reaches maximum.
- The HYPE control in the top bar enters a strong READY visual state.
- The player manually activates THE BANG by tapping the HYPE control.
- Activation is never automatic in the current design.

Manual activation creates a small strategic decision: the player may save the full meter for a favorable phrase or difficult/high-value moment.

### Core behavior

THE BANG creates a short enhanced-performance window while normal gameplay continues.

The player does **not** enter a separate minigame and does not learn a new control language.

Candidate effects during the active window:

- increased scoring value / multiplier opportunity
- stronger reward for good Motion Quality
- maximum avatar performance intensity
- stronger body and hair presentation
- intensified background/crowd/light response
- more energetic but still readable UI feedback

Exact duration and numerical bonuses remain tuning variables. Initial prototype target: a short window on the order of several seconds rather than a long alternate mode.

### Special Bang payoff

THE BANG should contain a memorable **special-move payoff** without requiring an unrelated quick-time event.

Current preferred direction:

- activation starts the enhanced window
- the first suitable strong authored/performed moment inside that window becomes a **Special Bang**
- the Special Bang uses the existing neck action being performed, but receives an exceptional audiovisual presentation and score payoff

This preserves the normal rhythm flow while still giving the player a recognizable “limit break” moment.

The exact trigger rule for the Special Bang is intentionally not finalized yet. Candidates include:

- first high-quality accented event after activation
- first authored strong beat / phrase accent
- first event exceeding a Motion Quality threshold

The rule must be deterministic and understandable once selected.

### Scope constraints

THE BANG should not introduce:

- a new unrelated input scheme
- a detached QTE
- invulnerability as the primary reward
- a long alternate game mode
- automatic activation
- chart semantics that only exist during HYPE mode unless later testing strongly justifies them

The super state should communicate **“the same performance, now pushed beyond the normal limit”**, not “a different game has started.”

## Gameplay screen wireframe

Conceptual portrait layout:

```text
+------------------------------------------+
| [II] SCORE      SONG PROGRESS    xMULTI |
|                                    HYPE  |
|                                   [READY]|
+------------------------------------------+
|                                          |
|                 NEXT                     |
|                  .                       |
|                                          |
|                CURRENT                   |
|                   O                      |
|                                          |
|                [ HEAD ]                  |
|                 [NECK]                   |
|             [ SHOULDERS ]                |
|               [ TORSO ]                  |
|                                          |
|      PERFECT!              COMBO 26      |
|                                          |
|        +score                  +HYPE      |
|                                          |
|       multilayer 2D venue/background     |
|                                          |
+------------------------------------------+
```

This is a hierarchy specification, not pixel-perfect placement.

The intended attention order is:

1. avatar/head
2. CURRENT cue
3. NEXT cue
4. local judgment/combo feedback
5. peripheral top-bar state

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

The HYPE control is an intentional exception: it is a discrete low-frequency top-bar action and therefore may be explicitly tapped when READY.

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
- top-bar information can be read peripherally
- local combo/judgment feedback does not obscure cues
- HYPE READY state is obvious without demanding attention
- tapping HYPE is comfortable without breaking rhythm flow
- a player can complete a short run without needing to stare at peripheral HUD elements

## Scope boundary

This document covers active gameplay UX only.

Song select, campaign navigation, avatar creator, store, results and other shell/menu UX should be designed separately so they do not force compromises into the core playfield.
