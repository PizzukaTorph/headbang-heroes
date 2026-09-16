# Foundation Consolidation Review — 2026-09-16

## Purpose

Record the result of the full design/architecture review and distinguish resolved documentation issues from implementation work still required.

This is a review record, not a higher-authority specification. Canonical authority is defined in `FOUNDATION.md`.

## Consolidated decisions

### Product / scope
- POC = one complete song proving the mechanic/full loop.
- MVP = four complete real songs proving a small complete game.
- Story, Quick Headbang, Versus, Practice, Endurance are the five planned modes.
- Story + Versus are core long-term identity modes; POC/MVP do not require all modes.

### Gameplay
- Continuous neck simulation is the gameplay source.
- Input always affects neck state.
- MISS never resets/snaps/freeze neck motion.
- Timing Quality and Motion Quality are separate.
- First bang from neutral is setup/unprepared.
- Classic Bang direction names denote inversion points; launch occurs away from the tapped side.

### Vocabulary
- Techniques: Classic, Half, Deep, Whiplash, Windmill.
- Trajectories: Horizontal, Vertical, Circular, CenterEdge.
- Modifiers: None, Double, Hold, Accent, Burst.
- Rest is a separate event family.
- THE BANG = enhanced state.
- Finisher = payoff.
- `Special Bang` is legacy wording.

### Musical density
- Source rhythmic subdivision does not directly dictate input subdivision.
- Charts represent performable headbang interpretation.
- Dense source music can be abstracted into slower pulse, Half/Burst, Windmill continuity, or accents.

### Difficulty
- Difficulty belongs to a chart.
- A song can have one or many charts.
- The catalog can carry difficulty progression.
- Higher difficulty never changes neck physics just to become harder.

### Event matching
- Production runtime must support bounded unresolved candidate matching around authoritative song-time.
- Single-active-event behavior is not a permanent architecture assumption.
- Too-early input may move neck without consuming a future event.
- Wrong semantic action inside a matched eligible window consumes MISS unless technique semantics explicitly say otherwise.

### Motion determinism
- Render frame pacing cannot define scoring physics.
- Production direction: fixed authoritative simulation step or deterministic time-evaluated equivalent.
- Motion Quality uses event/gesture-local history, not run-global peak amplitude.

### Authored Rest
- Rest interval has a settling phase followed by stillness evaluation.
- Existing momentum is never forcibly cancelled.
- Exact thresholds/proportions are tuneable data.

### Content
- Official content is server-first with local cache.
- Compatible content can update remotely; gameplay rules cannot.
- POC/MVP security: HTTPS + version/hash validation.
- Production direction: trusted/signed release manifest or equivalent authenticity mechanism.

### Save/profile
- Local-first for offline play.
- Sync authority is domain-specific.
- Records/unlocks/settings and currency/entitlements do not use one generic merge strategy.
- Production currency/entitlements require server-authoritative transaction/ledger semantics.

### Presentation
- Body/hair/venue/UI/haptics remain downstream of gameplay.
- No default gameplay SFX over the music.
- Haptics are optional supplemental feedback.

## Legacy concepts removed from canonical foundation

The following may still appear in historical commits/lab experiments but are not canonical unless reintroduced explicitly:
- Downbang as a core standalone technique family
- Doom Hold as a core standalone technique family
- Sidebang as a core standalone technique family
- Neckbreaker difficulty label
- SS grade
- mandatory six-technique progression ladder
- mandatory six-world campaign as MVP requirement
- THE PIT as required catalog naming
- Special Bang terminology
- gameplay power unlocked by player level
- every song requiring four difficulty charts

## Current implementation mismatches to address during refactor

### `HeadMotionModel`
Current M0 code integrates with render `Time.deltaTime`.

Required production direction:
- move gameplay-critical simulation to authoritative fixed step / deterministic equivalent
- rename concept toward `NeckMotionModel` when refactoring
- keep render transform application downstream/interpolated

### Motion Quality
Current M0 `peakAmplitude` can accumulate for the whole run.

Required:
- bounded event/gesture-local history
- explicit reset/rollover semantics
- pre-inversion snapshot for arrival evaluation

### Scheduler / event matching
Current prototype largely assumes one active event.

Required:
- bounded unresolved candidate search
- deterministic matching policy
- dense-event tests

### Timing config
Current prototype judgment windows are hardcoded constants.

Required:
- data-driven TimingConfig
- real-device tuning/calibration

### Rest
No complete production RestEvaluator exists yet.

Required:
- settling/evaluation interval handling
- stillness metrics
- tests

### HYPE/THE BANG/Finisher
Prototype coverage is incomplete relative to the canonical full loop.

Required:
- HypeSystem ownership
- deterministic Finisher candidate resolution
- no recursive HYPE refill trap

### Runtime chart
Prototype ScriptableObject/time-in-seconds chart structures remain M0 conveniences.

Required:
- canonical authoring model
- validator/compiler
- immutable RuntimeChart
- stable IDs/versions

## Gate before broad implementation

Before adding broad product features, complete/refactor enough of the core to validate:

```text
AudioClock
→ RuntimeChart
→ candidate matching
→ semantic input
→ deterministic neck simulation
→ Timing + Motion Quality
→ Scoring
→ HYPE / THE BANG / Finisher
→ Results
→ Retry
```

Then verify on a real phone.

The purpose of the foundation work is not to over-engineer the prototype. It is to ensure every next implementation step has one clear source of truth and does not accidentally rebuild an obsolete version of the game.
