# Implementation Plan 05 — POC Gameplay Presentation

## Status

P0B — POC Presentation Integration. Execute after P0A core runtime is accepted.

## 1. Goal

Make the deterministic core readable, satisfying and recognizably Headbang Heroes on a portrait mobile screen while keeping every presentation system strictly downstream of authoritative gameplay.

## 2. Canonical references

Read: `AGENTS.md`, `docs/FOUNDATION.md`, `docs/GAMEPLAY_SCREEN_V1.md`, `docs/GAMEPLAY_UX_V1.md`, `docs/PRESENTATION_ACCESSIBILITY_V1.md`, `docs/BODY_SYSTEM_V1.md`, `docs/HAIR_SYSTEM_V1.md`, `docs/VISUAL_DIRECTION.md`, `docs/TECHNICAL_CONTRACTS_V1.md`, `docs/ROADMAP.md` P0B.

Specialists: `agents/presentation-avatar.md` primary, `agents/gameplay-core.md`, `agents/qa-guardian.md`.

## 3. Current-state investigation

Inspect current gameplay scene, camera/canvas, head visual adapter, CURRENT/NEXT cues, HUD, avatar hierarchy, body/hair placeholders, background, haptics and any presentation code that currently mutates gameplay state.

## 4. Scope

- portrait gameplay composition;
- CURRENT/NEXT driven from ChartRuntime + AudioClock observer state;
- avatar head visual driven by authoritative neck state;
- representative body response driven from neck motion;
- representative hair/secondary motion;
- one lightweight multilayer venue/background reaction profile;
- judgment/combo feedback;
- compact top bar: pause, score, progress, multiplier, HYPE;
- HYPE READY, THE BANG and Finisher presentation states;
- centralized semantic haptic feedback with on/off/intensity seam;
- reduced-flash/reduced-shake compliance in implemented effects;
- real-phone readability verification.

## 5. Out of scope

No scoring/timing changes, chart changes, final avatar customization, large venue library, elaborate VFX, production art replacement, shop/economy, gameplay SFX layer, story scenes, backend, broad UI framework rewrite.

## 6. Required architecture

Presentation observes authoritative state/outcomes. Removing avatar body, hair, venue, VFX, HUD animation or haptics must not alter score, judgments, chart resolution, HYPE state or neck simulation.

Visual interpolation is allowed only downstream. Cue animation never owns event time. Body/hair react to authoritative neck snapshots and cannot feed physical values back into `NeckMotionModel`.

Song audio remains primary: no default PERFECT/GREAT/GOOD/MISS arcade sounds and no generic THE BANG sound layered over music.

## 7. Implementation tasks

### PRES-01 — Scene/presentation audit
Map current hierarchy and remove gameplay-authority leaks.

### PRES-02 — Gameplay composition
Establish portrait hierarchy: venue -> avatar -> CURRENT/NEXT -> compact HUD/local feedback. Protect face, neck, trajectory and cue target.

### PRES-03 — Cue presenter
Render CURRENT strongly at expected inversion zone and NEXT as restrained preview. Consume authoritative event chronology/time only.

### PRES-04 — Avatar adapter
Drive head visual from authoritative neck state with presentation-only interpolation where useful.

### PRES-05 — Body response
Implement minimal torso/shoulder reaction derived from neck motion. No independent scoring physics.

### PRES-06 — Hair response
Implement representative front/back secondary motion sufficient to prove readability and energy. Keep it cheap/mobile-shaped.

### PRES-07 — HUD/feedback
Wire score/progress/multiplier/HYPE, local judgment and combo feedback from semantic gameplay state/outcomes.

### PRES-08 — HYPE/BANG/Finisher
Present READY peripherally, manual HYPE control, active THE BANG and Finisher impact without obscuring gameplay.

### PRES-09 — Venue reaction
One static multilayer venue with restrained reactions to performance intensity/HYPE/THE BANG/Finisher.

### PRES-10 — Haptics/accessibility
Centralize semantic haptic patterns; implement off/intensity seam where platform supports it; reduced flash/shake must affect presentation only.

### PRES-11 — Verification/review
Presentation-avatar + gameplay-core + QA review and real-phone readability pass.

## 8. Data/API guidance

Prefer narrow presenters/adapters such as `NeckPresenter`, `CuePresenter`, `GameplayHudPresenter`, `BodyReactionPresenter`, `HairPresenter`, `VenuePresenter`, `HapticsService`. Consume snapshots/semantic events; do not introduce a generic global event bus merely for convenience.

## 9. Required tests/checks

Automated where useful:
- presentation can be absent while gameplay-domain tests still pass;
- cue target/time derives from authoritative state and visual delay cannot change judgment;
- body/hair presenter cannot mutate neck domain state;
- reduced flash/shake and haptics settings do not change authoritative outcomes.

Manual/device:
- CURRENT + head read as one system;
- NEXT materially helps preparation;
- cardinal cues remain legible;
- avatar is primary focus;
- top bar is peripheral;
- lower playfield remains comfortable for thumbs;
- THE BANG remains readable under intensified presentation;
- venue never obscures critical region;
- sustained representative hair motion does not destroy mobile performance/readability.

## 10. Acceptance criteria

- [ ] player can primarily watch avatar/head rather than stare at HUD;
- [ ] CURRENT/NEXT are readable and time-authority-free;
- [ ] body/hair react to authoritative motion but cannot influence gameplay;
- [ ] compact HUD reflects authoritative state;
- [ ] HYPE READY/THE BANG/Finisher are clear without hiding cues;
- [ ] no unnecessary gameplay SFX compete with song;
- [ ] haptics are optional/supplemental;
- [ ] accessibility presentation reductions do not alter scoring potential;
- [ ] removing presentation layers cannot change score;
- [ ] real-phone cue/readability check passes;
- [ ] tests/reviews have no blockers.

## 11. Verification report

Return files changed, hierarchy/data-flow changes, presentation-authority leaks removed, phone/device checked, readability findings, performance observations, tests, specialist findings, known art/polish debt and commit SHAs.

## 12. Commit strategy

```text
refactor(presentation): isolate gameplay presentation adapters
feat(cues): integrate current and next cue presentation
feat(avatar): add body and hair reaction layer
feat(ui): integrate gameplay hud and bang feedback
feat(venue): add lightweight reactive venue
feat(haptics): add semantic optional feedback
test(presentation): guard gameplay isolation
```

## 13. Do not

Do not let animation/cues/haptics determine judgment; do not obscure face/neck/CURRENT; do not add a note highway; do not over-polish temporary art; do not feed body/hair back into scoring; do not add gameplay audio clutter; do not proceed to Plan 06 automatically.
