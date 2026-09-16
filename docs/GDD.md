# Headbang Heroes — Game Design Document

## Status

This is the high-level product design document.

Detailed subsystem behavior lives in the dedicated `*_V1.md` specifications. If this file and a canonical subsystem spec disagree, the subsystem spec wins. See `FOUNDATION.md` for authority order.

## 1. High concept

Headbang Heroes is a portrait-first 2D mobile rhythm game in which the player's instrument is the avatar's head and neck.

The player reads timing/movement cues and controls a continuous simulated neck through authored headbang choreography synchronized to metal music.

The design target is:

> **Easy to understand, difficult to perfect.**

The game should feel physical, musical, readable, fair, replayable, and unmistakably metal.

## 2. Pillars

### Feel the music
Inputs should feel connected to musical structure rather than to an arbitrary detached note lane.

### Control movement, not notes
The player controls neck motion and momentum. Inputs influence a continuous simulation rather than teleporting the avatar between authored poses.

### Metal identity
Music, avatar performance, hair/body response, venues, humor, presentation, progression, and content discovery all reinforce metal culture without mocking the player or the music.

### Visible skill
A strong run should visibly look stronger: better preparation, larger/cleaner motion, smoother flow, stronger body/hair response, and more intense HYPE presentation.

### Fair deterministic scoring
Timing, movement quality, chart identity, and scoring rules must be explainable and versionable.

### Scalable catalog
Songs and charts are content, not hard-coded app structure. New compatible songs should be deliverable remotely without redefining gameplay code.

## 3. Core loop

```text
Choose content
→ perform authored headbang chart
→ timing + motion + technique resolution
→ combo / multiplier / HYPE
→ THE BANG / Finisher opportunities
→ finish song
→ score + grade + performance report
→ XP + HH + records/unlocks
→ retry / continue
```

## 4. Core gameplay grammar

The neck is always a continuous simulated object.

Input changes motion; it does not directly place the head.

The player owns the neck even when wrong:

> **The player misses the beat, not ownership of the neck simulation.**

A MISS never freezes, snaps, or resets the neck.

Timing Quality and Motion Quality are distinct.

The same chart event can therefore produce combinations such as:
- PERFECT timing + weak physical execution
- GREAT timing + excellent prepared movement

The exact scoring architecture is defined in `SCORING_SYSTEM_V1.md`.

## 5. Canonical neck vocabulary

### Technique
- Classic
- Half
- Deep
- Whiplash
- Windmill

### Trajectory
- Horizontal
- Vertical
- Circular
- CenterEdge

### Modifier
- None
- Double
- Hold
- Accent
- Burst

Rest is its own authored event family rather than a MotionEvent technique.

New gameplay mechanics should extend this vocabulary without changing the semantics of existing charts.

## 6. Classic Bang

Classic Bang establishes the basic inversion/launch grammar.

- LEFT input = invert/commit on LEFT and launch RIGHT
- RIGHT input = invert/commit on RIGHT and launch LEFT
- UP input = invert/commit on UP and launch DOWN
- DOWN input = invert/commit on DOWN and launch UP

The authored beat corresponds to the inversion/commit point.

Inversion may occur anywhere in travel. Early inversion naturally produces small/weak movement, well-prepared inversion produces stronger flow, and late inversion allows travel toward physical limits.

Rapid spam should fail primarily because repeated early inversions prevent useful amplitude/momentum from developing, not because of an arbitrary cooldown.

The first input from neutral is a setup action because no preceding travel exists to evaluate with normal Motion Quality semantics.

## 7. Musical abstraction

Headbang Heroes charts are not literal transcriptions of every note or drum subdivision.

> **The chart follows the performable headbang rhythm, not every musical subdivision present in the source.**

A 1/32 drum passage may still be performed by the neck at 1/8 or 1/16, through Half/Burst, continuous Windmill motion, or selected accents.

The authoring model should distinguish:

```text
MUSIC LAYER
PERFORMANCE LAYER
CHART LAYER
```

This preserves musicality without turning dense metal passages into absurd tap spam.

## 8. Difficulty

Difficulty is a property of an authored chart.

A song may have one playable chart or several. The product must never assume that every song exists in Easy/Normal/Hard/Extreme.

The catalog itself may carry the difficulty curve.

Higher difficulty changes choreography, density, timing tolerance, vocabulary, transitions, and momentum planning. It must not make the neck physics artificially harsher.

See `DIFFICULTY_MODEL_V1.md` and `SONG_CHART_MODEL_V1.md`.

## 9. HYPE, THE BANG, Finisher

HYPE represents performance intensity.

Baseline judgment contribution:
- PERFECT +2
- GREAT +1
- GOOD +0
- MISS +0

MISS does not remove accumulated HYPE.

When full, HYPE enters READY. The player manually activates **THE BANG**, a short enhanced-performance state in which normal gameplay continues.

A suitable strong authored/performed moment inside THE BANG can resolve as a **Finisher**.

Canonical terminology:
- HYPE = resource
- THE BANG = temporary state
- Finisher = payoff

No separate QTE or unrelated control scheme is introduced.

## 10. Avatar and body fantasy

The player controls the neck. The body reacts to the neck.

The avatar is a 3/4 upper-body performer with strong head/neck readability, expressive body follow-through, and high-priority hair secondary motion.

Body and hair are presentation systems. They never determine scoring.

Performance Style and Idle Style change how the same gameplay is expressed visually, never gameplay power.

Avatar customization exists primarily to create immediate personal recognition:

> **"Cazzo, sono io."**

The creator should remain simple and preview-first rather than becoming a deep RPG character editor.

## 11. Active gameplay UX

The player should be able to play by looking at the avatar rather than a detached interface.

Primary visual hierarchy:
1. Head/Neck + CURRENT
2. Avatar body
3. NEXT
4. Judgment feedback
5. HYPE READY
6. Combo
7. Passive top-bar state

Persistent top bar:

```text
[PAUSE] [SCORE] [SONG PROGRESS] [MULTIPLIER] [HYPE]
```

Touch targets and visual target locations are separate concerns. Touch ergonomics must be validated on real phones.

No gameplay sound effects are layered over the music by default; visual feedback and haptics are preferred.

## 12. Song and chart model

Canonical structure:

```text
SongDefinition
└── charts[]
    └── ChartDefinition
        ├── sections[]
        ├── phrases[]
        └── events[]
            ├── MotionEvent
            └── RestEvent
```

Audio playback is authoritative.

Author-facing data is expressed in musical time; runtime data may be validated/compiled into deterministic precomputed timing.

Songs/charts carry stable IDs and versions so records and competitive results can remain attributable.

See `SONG_CHART_MODEL_V1.md`.

## 13. Content pipeline

Official content is server-first with local caching.

High-level path:

```text
AUTHORING
→ VALIDATE
→ PACKAGE
→ PUBLISH
→ SERVER CATALOG / CDN
→ CLIENT DOWNLOAD / CACHE
→ PLAY
```

New songs using known runtime semantics should not require a new app release.

New gameplay semantics require a compatible client update.

Cached valid content remains playable offline.

See `CONTENT_PIPELINE_V1.md`.

## 14. Progression

North-star rule:

> **Progression unlocks expression and content, not power.**

Core progression values:
- XP / Player Level
- HH soft currency

Progression may unlock cosmetics, presentation styles, songs/content, badges/titles, and other non-power expression.

No level/cosmetic may improve timing windows, neck simulation, Motion Quality rules, HYPE generation, multiplier protection, or maximum scoring potential.

See `PROGRESSION_V1.md`.

## 15. Results

Post-song results are presented as a brutal performance report.

Canonical grades:
- S
- A
- B
- C
- D

Flow:
1. Impact — grade, score, contextual line, avatar reaction
2. Performance Report
3. Rewards / Progression

Accuracy percentage may exist internally but is not required as a visible v1 metric.

Results render an authoritative `RunResult`; they do not recalculate gameplay scoring.

See `RESULTS_SCREEN_V1.md`.

## 16. Game flow

Baseline shell:

```text
BOOT
→ HOME
→ PLAY
→ [MODE SELECT when relevant]
→ SONG SELECT
→ PRE-SONG
→ GAMEPLAY
→ RESULTS
→ RETRY / CONTINUE
```

From Home to gameplay: as few taps as practical.

From Results to Retry: one tap.

Home is avatar-centric and prioritizes PLAY rather than promotional UI.

## 17. Modes

Current planned modes:
1. Story
2. Quick Headbang
3. Versus
4. Practice
5. Endurance

Story and Versus are the strongest long-term identity modes.

POC/MVP do not require every mode.

All modes must reuse the same core chart, neck, scoring, HYPE, body/hair, results, and profile systems rather than cloning gameplay rules.

See `GAME_MODES_V1.md`.

## 18. First-run/tutorial

First-run should establish ownership quickly, teach the core mechanic through real gameplay, and get the player into music fast.

The tutorial uses an intentionally easy authored song/chart and introduces one concept at a time.

For v1, instructional moments should prefer controlled authored pause/freeze gates or safe musical teaching points rather than real-time audio time-stretch complexity.

Advanced techniques are learned later through content/contextual onboarding/Practice.

## 19. Save/profile

The profile persists user metadata:
- identity
- XP/level/HH
- avatar selections
- unlocks
- records
- settings/accessibility
- calibration
- schema/version metadata

The game should remain usable offline with local data and downloaded songs.

Future server authority is domain-specific: currency/entitlements require transactional server authority, while settings/records/unlocks follow their own merge policies.

See `SAVE_PROFILE_V1.md`.

## 20. Presentation and venues

Backgrounds are lightweight static 2D multilayer compositions with restrained parallax, crowd/light/haze response, and HYPE/THE BANG/Finisher reactions.

Presentation must never obstruct gameplay-critical information.

Visual direction:

**dark adult metal cartoon × readable mobile game × extreme caricature**

The world treats headbanging with absurd seriousness.

## 21. Online / Versus direction

Online features are long-term product capabilities, not core prototype dependencies.

Stable IDs, deterministic scoring references, chart/rules versioning, and explicit run data should support future asynchronous and realtime competition.

Realtime play should synchronize competitive state rather than streaming audio or head-physics frames.

See `ONLINE.md` and `GAME_MODES_V1.md`.

## 22. POC and MVP

### POC
One complete song proving the full loop and, above all, that controlling the neck to music is fun.

### MVP
Four complete real songs working end-to-end through the real content/game loop.

> **POC proves the mechanic. MVP proves the game.**

See `MVP_SCOPE_V1.md`.

## 23. Explicit non-goals for the foundation stage

Do not allow these to distract from the playable core:
- large campaign implementation
- realtime multiplayer before the solo loop is proven
- complex monetization
- elaborate shop/rarity economy
- large CMS
- community publishing browser
- full external chart editor
- full 3D venues
- gameplay-affecting gear/stats
- giant avatar creator

These may become later product work without changing the core gameplay contract.

## 24. Canonical specification map

Gameplay:
- `GAMEPLAY_UX_V1.md`
- `GAMEPLAY_SCREEN_V1.md`
- `SCORING_SYSTEM_V1.md`
- `DIFFICULTY_MODEL_V1.md`

Character:
- `BODY_SYSTEM_V1.md`
- `HAIR_SYSTEM_V1.md`
- `AVATAR_CUSTOMIZATION_UX_V1.md`

Content:
- `SONG_CHART_MODEL_V1.md`
- `CONTENT_PIPELINE_V1.md`
- `CHART_TOOLING_MODDING_V1.md`

Product:
- `GAME_FLOW_V1.md`
- `GAME_MODES_V1.md`
- `RESULTS_SCREEN_V1.md`
- `PROGRESSION_V1.md`
- `SAVE_PROFILE_V1.md`
- `MVP_SCOPE_V1.md`
- `TUTORIAL_SONG_SELECT_V1.md`
- `PRESENTATION_ACCESSIBILITY_V1.md`

Engineering:
- `TECHNICAL_BASELINE.md`
- `TECHNICAL_CONTRACTS_V1.md`

Foundation/authority:
- `FOUNDATION.md`
