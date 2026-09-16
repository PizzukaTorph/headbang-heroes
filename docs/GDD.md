# Game Design Document v0.4

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

Input applies intent/impulse to the head system. It does **not** directly position the head.

Head state includes at minimum:
- direction
- angular position
- angular velocity
- momentum/inertia
- recovery/neutral tendency
- physical movement limit / neck range

The preferred model is a deterministic/custom motion model rather than Rigidbody2D-driven gameplay.

The head is therefore always a continuous simulated object. Player input changes its motion; it does not teleport it between authored poses.

### Classic Bang — inversion/launch model

Classic Bang is the baseline expression of the movement model and establishes the grammar for the prototype.

The screen has LEFT and RIGHT tap zones. A tap represents an **inversion/launch point**, not the direction the head should move toward.

Therefore:
- tap LEFT → commit/invert on the left side → head launches toward RIGHT
- tap RIGHT → judge the right-side arrival/inversion → head launches toward LEFT
- tap LEFT → judge the left-side arrival/inversion → head launches toward RIGHT
- repeat

The beat is associated with the **inversion/launch**, not with the middle of the travel.

Conceptually:

`TAP LEFT → travel right → TAP RIGHT / BANG → travel left → TAP LEFT / BANG → ...`

The first tap from a neutral/rest state is a setup impulse. Because there is no preceding travel to evaluate, it should not receive ordinary Motion Quality judgment. The following opposite-side tap is the first complete Classic Bang event.

### Cue handoff

For Classic Bang, the previous input initiates both physical travel and anticipation of the next target.

Example:
1. player taps LEFT
2. the head starts travelling RIGHT
3. the RIGHT timing target/cue begins its countdown/convergence toward the authored beat
4. player taps RIGHT
5. timing and head state are judged
6. the head inverts/launches LEFT
7. the LEFT cue becomes the next target

The exact cue scheduling remains chart/audio-clock authoritative; player input must not be allowed to shift authored song timing. Visually, however, the handoff should read as one bang preparing the next bang.

### Anti-spam emerges from motion

Rapidly alternating LEFT/RIGHT taps must not produce a full-quality headbang merely because the input sequence is technically correct.

Every tap requests another inversion/impulse. If the player taps LEFT/RIGHT/LEFT/RIGHT too quickly, the head repeatedly reverses before it can build useful travel and amplitude. The visible motion becomes small and weak.

This is intentional.

Spam protection should therefore emerge primarily from the movement model and Motion Quality rather than from an arbitrary input cooldown.

Correctly paced play gives the head time to travel, build momentum and reach a strong inversion state. Over-fast play produces reduced amplitude/poor flow even when some taps happen to land inside timing windows.

### Miss and passive continuation

A MISS must **not** freeze, snap, reset or otherwise artificially stop the head.

If the expected opposite-side tap does not occur:
1. the head continues according to its current velocity and inertia
2. it approaches/reaches the physical neck movement limit
3. the motion naturally loses energy
4. the recovery/neutral tendency begins to influence the head
5. the player can attempt to recover on a later event

The player misses the beat, not ownership of the neck simulation.

This continuity is important both visually and mechanically: mistakes should disturb flow and make recovery harder without turning the avatar into a binary hit/miss animation state machine.

### Critical design constraint

A PERFECT tap with poor head state should not score identically to a perfectly prepared motion. Timing and motion quality are both meaningful.

The intended feel is:

prepare movement → acquire speed → arrive near beat → invert/commit → hit judgment → carry momentum into next beat.

For Classic Bang specifically, the opposite-side tap should evaluate at least:
- authored timing error
- expected side/direction
- amplitude / how far the head actually travelled
- useful angular velocity/momentum at arrival
- inversion quality
- continuity/flow from the preceding movement

These movement components feed Motion Quality rather than replacing Timing.

Example: a PERFECT-timed tap after frantic micro-inversions may have excellent Timing but poor Motion Quality because the head barely travelled. A slightly less accurate GREAT with a large, controlled, well-prepared swing may look and feel substantially better while still retaining the lower Timing component in deterministic scoring.

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


## 24. Level progression

The player level cap is **666**. This number is part of the game's identity, not a traditional slow RPG progression curve.

Principles:
- early levels should arrive quickly and frequently
- progression gradually slows but remains visible
- not every level needs a unique unlock
- XP is never spent; HH remains the spendable soft currency
- reaching level 666 is a prestige achievement, not a competitive advantage
- XP may continue to be tracked statistically after the cap, but displayed level remains 666

Candidate milestone levels include 6, 66, 100, 200, 333, 500, 600 and 666.

Level rewards may include HH grants, apparel access, cosmetics, gestures, techniques, combo chains and increasingly spectacular movement variants. Core competitive power must not be purchasable.

Level 666 should award an exceptional prestige cosmetic/title/animation rather than stronger scoring.

## 25. Campaign world structure

**Road to the Pit** consists of five genre-focused training/conquest worlds followed by a sixth mixed-genre endgame world: Hell.

Each world contains six levels and culminates in its own Headbang Master/boss. The first five bosses embody their world's musical/gameplay identity. Hell combines everything learned previously and culminates in THE NECK.

Initial structure:

1. **Heavy World** — foundational timing and Classic Bang
2. **Thrash World** — aggression/accents and Downbang
3. **Doom World** — slow weight, stored momentum and Doom Hold
4. **Death World** — precision/directional complexity and Sidebang
5. **Black World** — endurance/continuous motion and Windmill
6. **HELL** — mixed genres, mastery chains and Whiplash

The exact world/boss names remain open for creative development.

### Hell

Hell deliberately has no single genre. Its songs/charts mix the established gameplay languages and demand combinations of previously learned techniques. Whiplash is the final fundamental/master technique.

The design progression is therefore:

**five fundamental schools → Hell mastery → THE NECK**.

THE NECK does not need a secret seventh technique. The fantasy is that THE NECK has absolute mastery of the same language the player has spent the campaign learning.

### Boss encounters

Bosses should not be limited to ordinary high-difficulty charts. A low-cost signature mechanic is **Call & Response**:

boss performs a move/pattern → player reads/responds → pattern escalates → encounter transitions into or interleaves with normal chart play.

This reuses the core rhythm/headbang systems while making bosses feel authored and distinct. THE NECK can reuse, combine and mutate pattern languages learned from all previous bosses.

## 26. Catalog access and progression gating

The long-term target is a very large song library. Campaign progression must therefore remain separate from music access.

Default philosophy: **do not lock ordinary song listening/play behind arbitrary player level.**

Levels primarily unlock techniques, combos, cosmetics, apparel, gestures, campaign progression and advanced challenges. A song may remain playable at an accessible chart difficulty even when its advanced charts use techniques the player has not yet learned.

Candidate difficulty naming remains open; the current conceptual ladder is Easy / Normal / Hard / Neckbreaker.

Charts should support authored sections so future modes such as 60–90 second Quick Bang, daily challenges, practice segments and multiplayer excerpts can reuse song content without destructive editing.

Fail-state philosophy: prefer allowing the player to finish the song and receive a poor rank/reduced rewards rather than ejecting them mid-track. This supports music discovery and reduces mobile-session frustration.

## 27. Practice mode — Neck Gym

A future **NECK GYM** mode can teach and practice unlocked techniques independently of campaign failure/rewards.

It can serve as:
- onboarding/tutorial
- technique practice
- newly unlocked move preview
- timing/momentum training
- calibration-friendly low-pressure environment

This is not required for the first core prototype.

## 28. Visual direction

### North star

The desired tone is inspired by the **serious, dark, exaggerated adult-metal-animation energy associated with Metalocalypse**, but Headbang Heroes must develop its own original characters, silhouettes, rendering language, UI and world design. This is a tonal/reference north star, not an instruction to imitate protected character designs or distinctive artwork.

Working shorthand:

**dark adult metal cartoon × readable mobile game × extreme caricature**.

### Tone principle

The world treats headbanging with absurd seriousness.

The comedy should come from treating ridiculous stakes as completely legitimate: headbang technique can be discussed like elite athletic training; Headbang Masters are culturally important figures; THE NECK is treated as an intimidating legendary presence rather than a wink-at-the-camera joke.

### Character principles

- stylized human anatomy
- strong, immediately readable silhouettes
- exaggerated shoulders, hands, heads, necks and hair where useful
- enough structural consistency to support modular apparel
- expressive faces capable of escalating dramatically during performance
- hair is a major gameplay-feedback surface
- characters may deform beyond idle proportions at high HYPE

Avatar presentation states should conceptually escalate:

**IDLE → BANG → COMBO → HIGH HYPE → POSSESSED**

Skill should therefore alter not only score but the visual intensity of the avatar.

### Mobile readability

The player/avatar should occupy a large portion of the portrait playfield. The head and timing cue must remain readable on a phone screen. Background detail must never compete with rhythm information.

The timing UI may use clearer/brighter values than the darker world art so gameplay remains instantly legible.

### Backgrounds

Genre/world backgrounds remain primarily static 2D artwork with restrained parallax, particles, lighting and crowd reactions. This supports visual variety without turning the project into a 3D environment pipeline.

### Boss silhouettes

Each Headbang Master should exaggerate the visual language of their world. Examples of direction, not final designs:
- Heavy: iconic/classic imposing metal frontman energy
- Thrash: lean, kinetic, aggressive silhouette
- Doom: monumental, heavy, slow visual mass
- Death: dense physicality/technical menace
- Black: severe, elongated or spectral silhouette
- THE NECK: deliberately breaks the established anatomy scale through impossible neck/trapezius mass

### Art discovery

Before production art, create multiple original Headbang Heroes concept directions using the same gameplay composition. Evaluate at least:
- clean/readable cartoon
- darker metal-comic treatment
- grotesque/exaggerated cartoon

The likely target is a hybrid that preserves mobile readability while allowing extreme character deformation and adult-metal tone.


## 29. World identity lineup — working names

Each campaign world is a materialization of its reference metal genre. Environment, boss silhouette, chart language and taught technique should reinforce the same identity. The campaign progressively moves from recognizable real-world metal spaces into increasingly impossible mythic environments.

### World I — IRON ROOTS
- Genre: Heavy Metal
- Environment: large old-school metal club; walls of amplifiers, denim/leather visual language, classic stage energy
- Boss: **THE KING** — archetypal heavy-metal sovereign; imposing but still recognizably human
- Technique: Classic Bang

### World II — MOSH DISTRICT
- Genre: Thrash Metal
- Environment: battered warehouse/club, bent barriers, graffiti, huge mosh-pit energy
- Boss: **THE RIOT** — lean, nervous, hyper-kinetic and aggressively fast
- Technique: Downbang

### World III — THE ABYSS
- Genre: Doom Metal
- Environment: cyclopean ruined cathedral, fog, candles and monumental amplifier stacks
- Boss: **THE MONOLITH** — enormous and extremely slow; every movement should feel impossibly heavy
- Technique: Doom Hold

### World IV — THE CATACOMBS
- Genre: Death Metal
- Environment: underground catacombs converted into an oppressive extreme-metal venue; macabre/cartoon rather than explicit splatter
- Boss: **THE BUTCHER** — dense, physical and technically threatening; increasingly exaggerated neck anatomy foreshadows the endgame
- Technique: Sidebang

### World V — FROZEN VOID
- Genre: Black Metal
- Environment: frozen outdoor stage / forest / mountain darkness, snow and fire; an original reinterpretation of black-metal visual language
- Boss: **THE WRAITH** — tall, spectral, near-inhuman silhouette with extreme hair motion and supernatural-looking Windmill mastery
- Technique: Windmill

### World VI — HELL
- Genre: All Metal / mixed mastery
- Environment: an impossible infernal metal arena built around/above an endless pit; no longer grounded in reality
- Boss: **THE NECK** — final master and deliberate breaking point of the game's established anatomy scale
- Technique: Whiplash / mastery of all previous techniques

### Escalation

Environmental escalation:

**Club → Warehouse → Cathedral → Catacombs → Frozen Wilderness → Literal Hell**

Boss escalation:

**THE KING → THE RIOT → THE MONOLITH → THE BUTCHER → THE WRAITH → THE NECK**

The first boss remains plausibly human. Each subsequent world pushes anatomy, movement and environment further into myth until THE NECK requires no literal explanation. The game should treat THE NECK as a legendary fact rather than over-explaining the character.

All world and boss names above are working names until final naming/brand review.


## 30. Avatar customization model v0.1

The avatar is a player-created modular character rather than a fixed protagonist.

Identity presentation options: Male, Female, Other. These are customization starting points, not gameplay classes.

Target face system: 5 distinct face archetypes per presentation set.

Target body system: 4 controlled archetypes: S / Slim, M / Medium, L / Large, XL / Extra Large. Prefer a shared skeleton and controlled body families so apparel remains scalable.

Customization categories include facial hair, piercings, makeup, tattoos and a Special category for more extreme original metal-themed face/body treatments. The underlying system should remain flexible even where curated presets differ.

Hair is a major customization and feedback category: short through very long, multiple textures, mohawks, undercuts, shaved/bald and fantasy/extreme styles, with natural and unnatural colors. Hair choice never affects scoring.

Apparel categories: headwear, tops/shirts, jackets/vests, bottoms, footwear and accessories. Apparel is intended as a primary HH soft-currency sink and long-term content surface. Future licensed band collaborations may include digital band apparel where rights permit.

## 31. Gameplay screen composition v0.1

Portrait mobile is the baseline. The avatar should occupy roughly 50–60% of useful gameplay height in a near-frontal three-quarter presentation, preserving face/apparel readability while making head movement readable.

The closing-circle timing target lives around or immediately adjacent to the avatar's head. The head is effectively part of the interface; avoid separating rhythm gameplay into a traditional note highway.

Active-song HUD remains minimal: score, combo and HYPE/Crowd meter. XP, HH and progression information remain outside active performance.

The timing cue stays geometrically simple: fixed target ring plus approaching/converging ring. Judgment feedback can be expressive, but ornament must never compromise sub-second readability.

## 32. Visual style synthesis v0.1

The approved direction combines three explored treatments:

- **A — Clean Dark Cartoon:** gameplay readability baseline, strong silhouettes and controlled detail.
- **B — Metal Comic:** texture, grit, typography and aggressive attitude for the wider brand and selected environments.
- **C — Grotesque Cartoon:** increasing deformation for HIGH/MAX HYPE, finishers and bosses rather than constant maximum distortion.

Working synthesis: **A for baseline readability + B for metal identity + C for performance escalation.**

Performance ladder: **IDLE → BANG → COMBO → HIGH HYPE → MAX HYPE / POSSESSED**.

World art can remain dark/muted/gritty while critical rhythm cues use stronger contrast and clarity. Gameplay UI stays restrained; menus may lean harder into concert-poster, backstage, flyer and merch-booth visual language.

The first visual exploration is approved as **Visual Direction v0.1 reference**, not production-ready art. It validates portrait composition, large near-3/4 avatar, head-centered timing ring, minimal HUD, IRON ROOTS readability, the A/B/C synthesis, modular avatar categories and hair/secondary-motion emphasis.


## 34. Narrative tone and campaign premise v0.1

### Core narrative rule

The world is absurd, but everyone who lives in it treats it as completely normal.

Competitive headbanging, Masters, rankings, training, specialized techniques, Neck Gyms and the road toward Hell do not require elaborate lore explanations. The comedy comes from absolute sincerity toward ridiculous stakes.

Avoid over-explaining the setting. Headbang Heroes does not need a mythology that justifies every joke.

### The protagonist

The player avatar is **not a Chosen One** and does not begin with a mission to save the world.

They are an ordinary metal fan who discovers that they are naturally very good at headbanging and decides to see how far that ability can take them. The player and protagonist discover the competitive headbanging world together.

### Act 0 — The Bedroom

Working opening premise:
- the avatar is listening to metal and headbanging alone in their room
- another character notices the performance and is shocked by the natural ability
- this character reveals that competitive headbanging is an established discipline
- the protagonist reacts as the relatively normal observer of an otherwise completely sincere absurd world
- this encounter becomes the entry point to IRON ROOTS

### Coach / manager

A coach/manager character is planned as the player's guide through the circuit. Working concept: a former promising headbanger whose own competitive career ended because of a neck injury. This is intentionally simple and comedic and remains open to revision.

The coach provides onboarding, context and recurring dialogue without becoming an exposition machine.

### Campaign escalation

The campaign should focus on characters, rivalries, competitions and increasingly extreme situations rather than dense world lore.

IRON ROOTS establishes the circuit and THE KING. Subsequent worlds escalate the player's reputation, technique and the strangeness of the Masters. References to THE NECK can gradually appear, but the character should remain unexplained and legendary.

The first five Masters may ultimately function as guardians/preparation for HELL without requiring a large cosmological explanation.

### THE NECK

Do not over-explain THE NECK.

THE NECK is treated by the world as an established legendary fact. The final encounter should gain power from restraint: reduced comedy/exposition, an intimidating reveal, and very little dialogue before the final performance.

Working principle: the player is not fighting THE NECK to save reality. They have reached the end because they want to prove they can beat the greatest headbanger alive.

### Campaign writing workflow

Develop narrative incrementally rather than writing the entire campaign before gameplay validation:

**Act 0 → Coach → entry into the circuit → IRON ROOTS → THE KING**

Then expand world by world. Preserve unanswered questions when explaining them would make the joke weaker.
