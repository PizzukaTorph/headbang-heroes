# Game Design Document v0.1

## 1. High concept

Headbang Heroes is a 2D mobile rhythm game in which the player's instrument is the avatar's head and neck.

The player reads timing cues, inputs directional/gesture commands and works with the simulated momentum of the head to perform increasingly complex headbang patterns in sync with metal music.

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

## 3. Core loop

Choose song → perform headbang chart → receive timing/technique judgments → build combo → finish song → receive score/rank → compare/improve → unlock/progress.

## 4. Moment-to-moment interaction

### Timing cue

The primary cue is a **closing/converging circle**. The player acts when the moving ring aligns with the target ring.

Initial timing judgments:

- PERFECT
- GREAT
- GOOD
- MISS

The implementation may internally track EARLY/LATE offsets even if they are not always shown.

### Movement

Input applies intent/impulse to the head system. Head state includes at minimum:

- direction
- angular position
- angular velocity
- momentum/inertia
- recovery/neutral tendency

Exact physics are a prototype question, not a fixed specification.

### Critical design constraint

A PERFECT tap at the wrong movement state should not necessarily look identical to a well-prepared motion. Timing and movement must interact enough to make mastery visible without making the game unreadable.

## 5. Headbang vocabulary

Prototype:
- Basic alternating/classic headbang
- Accent

Candidates after validation:
- Downbang
- Side-to-side
- Slow/held doom bang
- Windmill
- Hair whip
- Rapid/blast sequence
- Signature techniques

Complex moves may use tap sequences, directional swipes, holds/releases or chained patterns.

Do not expose many gestures before the basic motion is proven.

## 6. Combo system

Successful judgments build combo. Poor judgments break or degrade it.

Later, technique chains can create named combos/signature moves. These should reward execution rather than memorizing arbitrary fighting-game commands.

Potential feedback:
- increasing animation amplitude
- hair secondary motion
- crowd response
- stage/light intensity
- restrained screen feedback
- special finish/gesture

Accessibility settings must allow reduction of shake/flashes.

## 7. Genres

Initial identity remains within metal/hard music. Candidate genres:

- Heavy
- Thrash
- Death
- Black
- Doom
- Funeral Doom
- Groove
- Power
- Metalcore

Genres should affect chart language and pacing rather than functioning only as labels. A slow doom song and fast death-metal song should produce meaningfully different movement.

## 8. Avatar and cosmetics

Avatar is modular. Candidate slots:

- body/base
- face
- hair
- beard
- top
- bottom
- shoes
- accessories

Cosmetics are visual only; no pay-to-win stats.

Hair is a high-priority visual system because secondary motion communicates headbang quality.

Gesture categories may include intro, victory, failure, taunt and signature move.

## 9. Venues/backgrounds

Backgrounds should initially be static 2D compositions with optional lightweight parallax/particles/lighting.

Genre-themed examples:
- garage
- underground club
- old-school thrash club
- festival
- arena
- doom/cathedral environment
- extreme-metal environments

Do not introduce full 3D venues without a demonstrated gameplay need.

## 10. Progression — Road to the Pit

A deliberately lightweight, comedic horizontal campaign provides context and unlock pacing.

Example progression:
Garage → Pub → Underground Club → Festival → Arena → final championship.

Rivals/bosses are original fictional characters/archetypes. Real musicians, likenesses, names or trademarks require appropriate permission/licensing.

The campaign should never gate the fundamental rhythm experience behind grind.

## 11. Score and rankings

Per-song scoring is primary.

Candidate ranking surfaces:
- song global
- friends
- weekly
- genre aggregate
- overall aggregate

Candidate fun stats:
- total headbangs
- perfect bangs
- longest combo
- windmills completed
- cumulative head travel

Competitive scoring must include chart version so score validity survives chart updates.

## 12. Multiplayer

### Phase 1 — asynchronous challenges
A player completes a specific song/chart/version and challenges a friend or random opponent. The opponent plays the same content and scores are compared.

### Later — live battle
Two clients play the same song/chart concurrently. Do not synchronize head physics frame-by-frame unless proven necessary; exchange authoritative match/score state.

Realtime multiplayer is explicitly outside the prototype/MVP until retention justifies its cost.

## 13. Music strategy

Preferred order:
1. Original commissioned/community music
2. Direct licensing with small/underground bands
3. Properly licensed production/library music where appropriate

Potential Underground Platform program: open auditions/submissions for original tracks, followed by explicit written licensing agreements.

No assumption that possession of an audio file grants videogame synchronization/distribution rights.

## 14. Tone

Self-aware, exaggerated and affectionate toward metal culture. Humor should come from the premise, characters and escalation rather than mocking musicians or players.

## 15. Prototype success criteria

The prototype passes when:
- cue timing is immediately understandable
- input latency feels acceptable on target phones
- players can distinguish good from bad timing
- momentum improves the experience rather than frustrating it
- repeated attempts produce measurable improvement
- a 60–120 second session is fun without progression rewards

If these fail, iterate on the core before expanding scope.
