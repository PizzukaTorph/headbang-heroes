# Game Design Document v0.2

## 1. High concept

Headbang Heroes is a 2D mobile rhythm game in which the player's instrument is the avatar's head and neck.

The player reads timing cues, inputs directional/gesture commands and works with simulated momentum to perform increasingly complex headbang patterns in sync with metal music.

The design target is **easy to understand, difficult to perfect**.

## 2. Pillars

### Feel the music
Input must feel coupled to the song, not to an arbitrary UI sequence.

### Control movement, not notes
A successful input influences head motion. The game must not collapse into simple note tapping.

### Metal identity
Genres, venues, animation, humor, progression and music discovery all reinforce metal/hard-music culture.

### Skill is visible
A skilled run should visibly look smoother, heavier and more spectacular than a weak run.

### Fair scoring
The same chart and calibrated device conditions must produce deterministic, explainable judgments.

### Long-term content scale
The game must be designed from the beginning to support a large catalog of songs and charts without coupling every new track to narrative content.

## 3. Core loop

Choose song → perform headbang chart → receive timing/motion/technique judgments → build combo and HYPE → finish song → receive score/rank → earn XP + HH → unlock progression/cosmetics/moves → compare/improve → repeat.

## 4. Moment-to-moment interaction

### Timing cue

The primary cue is a **closing/converging circle**. The player acts when the moving ring aligns with the target ring.

Initial timing judgments:
- PERFECT
- GREAT
- GOOD
- MISS

Internally, the system tracks signed timing error:
- negative = early
- positive = late

### Movement

Input applies intent/impulse to the head system. Head state includes at minimum:
- direction
- angular position
- angular velocity
- momentum/inertia
- recovery/neutral tendency

The preferred model is a deterministic/custom motion model rather than Rigidbody2D-driven gameplay.

### Critical design constraint

A PERFECT tap with poor head state should not score identically to a perfectly prepared motion. Timing and motion quality are both meaningful.

The intended feel is:

prepare movement → acquire speed → arrive near beat → invert/commit → hit judgment → carry momentum into next beat.

## 5. Headbang progression

Six core techniques define the first major progression arc.

### 1. Classic Bang
Input: alternating left/right taps.

Teaches timing, inversion and basic momentum control.

### 2. Downbang
Input: downward gesture / accented directional action.

Teaches heavy accents and decisive beat commitment.

### 3. Doom Hold
Input: hold → release.

Teaches stored tension, slow timing and release control.

### 4. Sidebang
Input: left/right lateral gesture.

Introduces a second movement axis and more complex directional reading.

### 5. Windmill
Input: circular gesture / momentum-maintenance input.

The player initiates and sustains rotational flow. This should reward continuity rather than merely drawing circles.

### 6. Whiplash
Input: fast multi-directional sequence / advanced chained movement.

Endgame technique combining speed, inversion and directional precision.

## 6. Move chains and named combos

Moves are atomic techniques. Combos are authored chains of moves.

Examples:
- Classic → Classic → Downbang = HAMMER
- Windmill → Windmill → Downbang = HELLSLAM
- Doom Hold → Downbang → Whiplash = NECK DESTROYER

Progression may unlock both individual moves and named combo chains.

Combo design should reward execution and flow rather than arbitrary memorization.

## 7. Scoring model

Scoring is based on three dimensions:

### Timing
Measures distance from the authored chart event.

Example starting values for tuning only:
- PERFECT: ±35 ms → 1.00
- GREAT: ±70 ms → 0.85
- GOOD: ±120 ms → 0.60
- MISS: outside window → 0

These values are not final and must be tuned on real devices.

### Motion Quality
Measures how well the head movement matches a satisfying/valid physical state.

Candidate inputs:
- direction correctness
- angular velocity
- amplitude
- inversion quality
- continuity from previous event

Normalized range: 0.0–1.0.

### Technique
Measures whether the required move or compatible movement pattern was executed correctly.

Typical range:
- correct technique: 1.0
- partially compatible execution: reduced multiplier
- wrong technique: 0 or severe penalty depending on chart design

### Event score

Initial model:

`EventScore = BaseScore × Timing × MotionQuality × Technique`

With BaseScore initially set to a simple constant such as 1000 for tuning.

The multiplicative structure is intentional: strong movement should not compensate for terrible timing, and vice versa.

## 8. Combo and HYPE

Successful events build combo. Poor events reduce or break it.

Combo score multiplier should remain bounded so one early mistake does not destroy an entire run.

Candidate progression:
- x1.0
- x1.1
- x1.25
- x1.5
- max x2.0

Combo also feeds a **HYPE / Crowd** meter.

Higher HYPE increases presentation intensity:
- animation amplitude
- hair secondary motion
- crowd reaction
- stage/light intensity
- particles/FX
- special gestures/finishers

Accessibility options must allow reduced shake/flashes.

## 9. XP, levels and HH economy

### XP
XP represents permanent progression and is not spent.

Higher levels unlock access to:
- apparel tiers
- cosmetics
- gestures
- new headbang techniques
- combo chains
- larger/more spectacular moves
- harder charts/content where appropriate

### HH
HH is the soft in-game currency.

Players earn HH primarily by playing/winning/completing objectives.

HH is spent mostly on:
- apparel
- cosmetics
- accessories
- gestures

The competitive core remains skill-based. Cosmetics must not provide pay-to-win advantages.

## 10. Business model

Headbang Heroes is planned as freemium/free-to-play.

IAP and ads are expected as future monetization systems, but their exact placement is intentionally deferred.

Principles:
- never interrupt a song with an ad
- avoid gameplay advantages for money
- rewarded ads may be evaluated later
- cosmetics, apparel, gesture packs and possibly song/band packs are candidate IAPs
- do not introduce a second premium currency unless there is a demonstrated need

## 11. Avatar and cosmetics

The player uses a customizable modular avatar.

Candidate slots:
- body/base
- face
- hair
- beard
- top
- bottom
- shoes
- accessories

Hair is a high-priority visual system because secondary motion communicates headbang quality.

Gesture categories may include:
- intro
- victory
- failure
- taunt
- signature move

Visual direction: stylized/cartoon, exaggerated enough to make large head/neck/hair motion readable and funny.

## 12. Genres

Initial launch target: six broad metal gameplay archetypes.

Proposed first six:
- Heavy
- Thrash
- Death
- Black
- Doom
- Metalcore

Funeral Doom, Groove, Power and other subgenres can expand the catalog later.

Genres should affect chart language and pacing, not just metadata.

## 13. Music catalog architecture

The catalog and campaign are separate systems.

Core entities should be independent:
- Song
- Chart
- Artist
- Genre
- License/rights metadata

The campaign references songs/charts but does not own them.

This allows the catalog to grow to dozens, hundreds or more tracks without requiring new narrative structure for every song.

### THE PIT

A standalone catalog/play mode provides direct access to the full music library.

Example navigation:
Genre → Artist → Song → Difficulty → Play

Possible chart difficulties:
- Easy
- Normal
- Hard
- Neckbreaker

Different difficulties should use meaningfully different charts, not only looser timing windows.

## 14. Music strategy

Preferred sources:
1. Original commissioned/community music
2. Direct licensing with small/underground bands
3. Properly licensed production/library music where appropriate

Prototype/test content may use original music supplied directly by the team.

A future audition/submission program could source original tracks from underground artists. Submission alone does not grant usage rights; selected tracks require explicit agreements.

Underground Platform integration is not a current product dependency.

## 15. Venues/backgrounds

Backgrounds are initially static 2D compositions with optional lightweight parallax/particles/lighting.

Candidate venue progression:
- bedroom/garage
- pub
- underground club
- festival
- arena
- final championship / THE PIT

Do not introduce full 3D venues without a demonstrated gameplay need.

## 16. Campaign — Road to the Pit

The campaign is intentionally comedic but long enough to support retention and progression.

Framework:
- 6 worlds
- 6 levels per world
- 6 world bosses total
- final endgame confrontation with **THE NECK**

Each world introduces or emphasizes a gameplay technique and genre identity.

The structure is content-friendly: new tracks/challenges can be inserted later without rebuilding the whole progression.

A world level does not have to equal one unique song. Levels may include:
- song performance
- challenge variant
- score target
- technique challenge
- rival battle
- boss performance

This allows early versions to reuse a small catalog intelligently while the song library grows.

### Narrative premise draft

Headbanging is fading from the world. The player is discovered as someone who still possesses **THE BANG** and begins a ridiculous climb from tiny venues to the ultimate pit.

The story is delivered lightly through short cartoon scenes/dialogue rather than long narrative sequences.

THE NECK is the canonical final boss concept.

## 17. Score results and rewards

Post-song results should expose enough information to support mastery.

Candidate result panel:
- total score
- accuracy
- motion quality
- longest combo
- perfect/great/good/miss counts
- rank
- XP earned
- HH earned

Example rank ladder may later include familiar rhythm-game tiers such as C/B/A/S/SS.

## 18. Leaderboards

Per-song scoring is primary.

Candidate ranking surfaces:
- song global
- friends
- weekly
- genre aggregate
- overall aggregate

Fun statistics may include:
- total headbangs
- perfect bangs
- longest combo
- windmills completed
- cumulative head travel

Competitive scoring must include chart version so score validity survives chart updates.

## 19. Multiplayer

### Asynchronous challenge
A player completes a specific song/chart/version and challenges a friend or random opponent. The opponent plays the same content and results are compared.

### Realtime Headbang Battle
Two players perform the same song/chart concurrently.

The realtime fantasy is a performance battle, not direct combat.

Presentation ideas:
- both avatars visible
- live score/combo comparison
- shared CROWD/HYPE visualization
- crowd allegiance shifts toward the stronger performer
- final CROWD CHOICE / winner presentation

Networking should exchange compact match state rather than audio streams or head physics frames.

The song runs locally on both clients. The server coordinates identity, match, song/chart version, synchronized start and competitive state.

This is conceptually similar to lightweight synchronized mobile competitive play: very little data must be exchanged continuously.

## 20. Backend ownership

Backend is not required for the core prototype, but the product architecture assumes server authority for online/economic systems.

Server-owned domains include:
- account/profile
- XP/level
- HH balance
- inventory
- purchases
- progression
- scores
- leaderboards
- friends
- asynchronous challenges
- realtime match coordination

The client must not be trusted as the source of truth for currency or competitive scores.

Initial backend hosting can run on the existing M0THER infrastructure. If the game grows, services/databases/realtime components can be separated and scaled later.

## 21. Login and platform identity

Expected mobile login approach:
- Sign in with Apple
- Google / Google Play Games where appropriate
- optional guest-first flow

Headbang Heroes should maintain its own stable cross-platform account ID if cross-device progression/friends/economy are required.

Platform services such as Game Center / Google Play Games may be used for useful native capabilities, but should not become the only identity model if cross-platform features are required.

## 22. Tone

Self-aware, exaggerated and affectionate toward metal culture. Humor should come from the premise, characters and escalation rather than mocking musicians or players.

## 23. Prototype success criteria

The prototype passes when:
- cue timing is immediately understandable
- input latency feels acceptable on target phones
- players can distinguish good from bad timing
- momentum improves the experience rather than frustrating it
- motion quality meaningfully differentiates good/bad execution
- repeated attempts produce measurable improvement
- a 60–120 second session is fun without progression rewards

If these fail, iterate on the core before expanding scope.
