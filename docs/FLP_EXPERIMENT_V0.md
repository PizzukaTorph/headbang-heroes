# FLP EXPERIMENT — Pixel / Frame-Based Avatar Direction

Status: exploratory
Branch: `develop-flp`
Base branch: `develop`
Canonical test character: Erik
Date: 2026-09-27

## Why this branch exists

Headbang Heroes currently has multiple avatar-rendering directions under evaluation:

- `develop` — current 2D avatar / puppet direction
- `develop-3d` — 3D avatar experiment
- `develop-flp` — frame-based low-poly/pixel-art-inspired 2D experiment

The purpose of `develop-flp` is not to replace the existing directions immediately. It is an isolated production experiment intended to answer whether a frame-based illustrated/pixel avatar pipeline can deliver:

1. strong visual identity,
2. expressive headbang animations,
3. low runtime complexity,
4. manageable customization,
5. and, critically, a production workflow that does not depend on an external art team.

The branch must stay isolated from the avatar implementation on `develop` and `develop-3d` until the experiment is proven useful.

---

## Core direction

The target visual language is a detailed pixel-art / pixel-illustration aesthetic rather than strict retro 8-bit or 16-bit art.

Characteristics:

- adult metal cartoon identity,
- detailed but readable pixel rendering,
- strong silhouette,
- upper-body / gameplay-focused framing,
- 3/4 presentation,
- expressive face and hair,
- exaggerated poses are allowed and encouraged,
- animation is authored as discrete frames rather than skeletal deformation,
- runtime remains fundamentally 2D.

The shorthand is:

> Erik canonical identity -> FLP reinterpretation -> frame-based animation -> Unity playback.

This is intentionally closer to “a controlled animated sprite sequence” than to a runtime puppet.

---

## Canonical character

Erik is the canonical character for the experiment.

Erik already exists as the approved 2D avatar reference for Headbang Heroes. Every FLP test must preserve the recognizable identity of Erik:

- face,
- hairstyle,
- proportions,
- attitude,
- metal aesthetic,
- overall silhouette.

The first approved neutral FLP rendering of Erik will become:

`ERIK_FLP_MASTER_v01`

Once approved, later animation experiments should derive from this master rather than freely re-generating Erik from scratch.

The goal is to avoid character drift between animations.

---

## What the first POC must prove

The first POC is deliberately small.

It must prove the complete pipeline:

`Erik reference -> FLP master -> animation frames -> cleanup -> export -> Unity -> gameplay`

Initial animation scope:

- Idle
- Headbang
- Horns

The first gameplay validation should use the current working song/chart setup, preferably Beyond the Pain, so the FLP avatar can be evaluated in the real gameplay context rather than as an isolated sprite sheet.

No customization system is required for the first POC.

No generic animation framework is required before the first motion test works.

No production atlas pipeline should be built until we know what actually needs automation.

---

## Experimental sequence

### Checkpoint 1 — Canonical neutral frame

Produce one neutral FLP rendering of Erik.

Acceptance criteria:

- clearly recognizable as Erik,
- correct gameplay framing,
- usable silhouette,
- stable proportions,
- style suitable for mobile,
- clean enough to normalize manually in GIMP.

This becomes `ERIK_FLP_MASTER_v01`.

Do not proceed until the master is approved.

### Checkpoint 2 — First headbang outside Unity

Create a short headbang animation from the master.

Target:

- approximately 6–8 frames,
- clear readable motion,
- no requirement for physically correct anatomy,
- consistent face, hair, outfit and body proportions across frames.

Suggested key-pose vocabulary:

- NEUTRAL
- FORWARD
- DOWN
- BOTTOM
- REBOUND
- UP
- OVERSHOOT
- NEUTRAL

The exact count may change after the first test.

### Checkpoint 3 — Cleanup cost

Before writing Unity infrastructure, measure the real manual cleanup burden.

Evaluate:

- facial drift,
- hair-length drift,
- outfit inconsistency,
- alignment,
- palette inconsistencies,
- scale changes,
- silhouette jumps,
- required pixel cleanup.

Success means the result can be normalized in GIMP with reasonable corrective work.

Failure means we change generation model/tool/workflow before scaling the system.

A visually attractive output alone is not sufficient.

### Checkpoint 4 — Unity gameplay test

Only after the frame sequence is visually coherent:

- import into `develop-flp`,
- temporarily replace/bypass the current avatar rendering path,
- play the animation in the existing gameplay loop,
- evaluate readability, timing and feel during Beyond the Pain.

Do not modify scoring, chart logic or timing architecture as part of the FLP experiment unless strictly necessary for display integration.

### Checkpoint 5 — Repeatability test

Create a second independent animation: `Horns`.

This is the real scalability test.

If Headbang and Horns can both be produced from the same Erik master while preserving identity and requiring acceptable cleanup, the pipeline is viable enough to formalize.

---

## Production philosophy

The target long-term workflow is:

`canonical reference -> generative model/tool -> rough sprites -> GIMP cleanup -> validation/export -> Unity`

The art pipeline should be model-agnostic.

ChatGPT may be used for concepting and sprite generation where suitable. If a different image model or specialized animation tool provides better frame consistency, it can replace that step without changing the surrounding production contract.

The project must not depend on one specific image generator.

---

## Hard production requirement

The core art pipeline must not require an external artist in order to continue development.

An artist may later be used for:

- polish,
- cleanup,
- art direction support,
- production acceleration,
- premium assets.

But the project must remain producible by the core team using tools, references and documented processes.

This is a deliberate constraint intended to minimize:

- scheduling dependency,
- communication overhead,
- revision loops,
- recurring outsourcing cost,
- loss of production capability when a collaborator is unavailable.

---

## Roles

### Project owner / game director

Responsibilities:

- approve or reject the visual direction,
- control scope,
- decide what belongs in HH,
- perform lightweight cleanup/normalization when practical,
- maintain final production authority.

The role does not require professional drawing ability.

### AI / design / technical bridge

Responsibilities:

- translate visual intent into explicit production specs,
- generate concepts and candidate sprites,
- define frame maps,
- define prompts and references,
- define naming and acceptance criteria,
- identify technical implications,
- help keep the pipeline reproducible.

### External artist, if ever used

Responsibilities should be bounded and optional.

The artist should receive explicit tasks and a production contract rather than being expected to invent the technical pipeline.

### Coding agents / implementation

Responsibilities:

- implement the asset contract,
- automate validation/import where useful,
- keep gameplay logic isolated from art-generation concerns,
- report invalid assets rather than making art-direction decisions.

---

## Guardrails for the art pipeline

These should be formalized only after the POC validates the direction, but the current working assumptions are:

### One canonical gameplay view

Use one fixed upper-body 3/4 gameplay presentation.

Avoid per-animation camera changes.

### Classic vs Technique framing

Frame-authored Technique Skills should be composed expecting a wider/full-body
presentation than the Classic gameplay crop.

Production expectation:
- Classic frames optimize head/neck/upper-body readability;
- Technique frames keep the complete silhouette usable;
- extreme hair motion must remain inside the Technique framing envelope;
- the stable lower-body / waist anchor remains mandatory even though the
  presentation camera pulls back;
- asset authors must not bake the camera transition into the sprite sequence.

The camera/framing transition belongs to runtime presentation. Sprite frames
describe character motion only.

### Stable canvas and alignment

Eventually define:

- fixed canvas size,
- fixed logical origin,
- stable torso anchor,
- stable character scale,
- predictable bounding box.

Animations must not appear to jump because frame geometry changes arbitrarily.

### Controlled palette

The style does not need to be strict 16-color retro pixel art.

However, HH should eventually define a controlled palette and rendering rules so separate assets look like part of the same game.

### Authored frame count by motion weight

Do not target 24/30 FPS character animation.

The style should benefit from authored keyframes, but frame count is subordinate
to the physical read of the move. A heavy Technique Skill must not be forced
into the same frame budget as a common action.

Working targets:

- Idle: around 4 frames
- Common / Classic actions: around 6–8 frames
- Heavy Technique Skills such as Half and Deep: around 12–16 frames
- Finishers / unusually complex specials: 12+ frames as required by the pose design

These are production starting points, not gameplay constants.

For heavy Technique Skills, prefer a phase-authored sequence:

```text
NEUTRAL
-> LOAD / ANTICIPATION
-> EXCURSION
-> COMPRESSION / TARGET
-> RELEASE / IMPACT
-> RECOVERY / SETTLE
-> NEUTRAL
```

The gesture-driven portion and post-release portion must remain distinguishable
in the frame map. During gameplay, loading/excursion/compression can be scrubbed
from continuous gesture progress, while release/impact/recovery can play as a
short authored continuation after the player releases.

Do not build the recovery by simply reversing the forward frames. Hair inertia,
shoulder lag, facial expression and torso settling should be allowed to follow a
different path so the move communicates weight.

### Pose vocabulary

Reusable key poses should be named and documented so later hair, outfit or accessory work can target the same motion states.

### Source preservation

Never keep only the exported PNG when a source file exists.

Retain:

- source image,
- GIMP/XCF or equivalent editable source,
- prompts/spec,
- palette reference,
- frame map,
- exported runtime asset.

The project must remain reproducible if tools change later.

---

## Customization direction

Customization is explicitly out of scope for the first POC.

If FLP proves viable, the preferred direction is layered compositing rather than pre-rendering every possible full-avatar combination.

Potential layer model:

- body / skin,
- outfit,
- head / face,
- back hair,
- front hair,
- facial hair,
- accessories.

Every layer participating in animation would need to support the same frame/pose contract.

Example concept:

`Headbang/04 = body_04 + outfit_04 + head_04 + hairBack_04 + hairFront_04 + accessory_04`

This must not be implemented until the single-character full-frame POC is proven.

Important risk: body types such as S/M/L/XL may require separate silhouette-aware asset sets rather than trivial reskins.

---

## Why frame-based animation may help HH

Compared with the current puppet direction, FLP allows motion to be authored for visual impact rather than constrained by runtime anatomy.

Examples:

- hair can occupy a large part of the frame during a violent headbang,
- faces can exaggerate on impact,
- boss animations can intentionally break normal proportions,
- special moves can use silhouettes that would be awkward with a skeletal rig,
- different headbang techniques can look genuinely different instead of being parameter variations of the same neck rotation.

Potential animation vocabulary later:

- Idle
- Headbang
- HeavyHeadbang
- Horns
- Scream
- Celebrate
- Miss
- Stunned
- Finisher
- boss-specific reactions

None of this is committed until the base pipeline proves repeatable.

---

## Main risks

### 1. Art-production dependency

Frame animation moves complexity from runtime code into asset creation.

Every new motion requires authored frames.

Mitigation:

- generative tooling,
- reusable master references,
- limited frame counts,
- repeatable cleanup process,
- automation of non-artistic steps.

### 2. Frame-to-frame consistency

Generative image tools may drift in:

- face,
- hair,
- proportions,
- clothing,
- accessories,
- perspective.

This is the single most important risk to test early.

### 3. Customization explosion

Animation count multiplied by hair/outfit/body/accessory variants can grow quickly.

Mitigation may include synchronized layers and constrained pose vocabularies.

### 4. Late style changes

Changing character proportions after many animations exist can create expensive rework.

The canonical master and style rules should be stabilized before scaling content.

### 5. Texture memory and build size

Large numbers of full-resolution frames can create unnecessary mobile memory and package cost.

Future mitigation:

- texture atlases,
- sensible sprite resolution,
- content loading strategy,
- asset reuse.

Do not optimize this before the POC.

### 6. Limited interpolation

Frame-authored animation cannot arbitrarily interpolate like a skeletal rig.

This is acceptable if the discrete animation style feels intentional.

### 7. Tool/model dependence

A specific generator may stop being useful or available.

Mitigation: keep the pipeline model-agnostic and preserve sources/specs.

---

## Decision criteria

The FLP experiment is successful enough to continue if all of these are true:

1. Erik remains recognizably the same character across multiple animations.
2. Manual cleanup is manageable.
3. A second animation can be produced without rebuilding the workflow.
4. Runtime integration is simpler than or competitive with the current avatar approaches.
5. The result is readable and attractive during actual gameplay.
6. The process can be maintained without depending on external personnel.

The experiment should be reconsidered if:

- every frame requires substantial redraw,
- identity drift cannot be controlled,
- customization immediately creates an unmanageable asset matrix,
- runtime memory cost becomes unreasonable,
- the visual result is attractive only as static concept art but weak in gameplay.

---

## Non-goals for the first POC

Do not build yet:

- final customization system,
- complete avatar editor,
- all six headbang techniques,
- final texture-atlas strategy,
- final import automation,
- shop integration,
- body-size variants,
- production-ready asset factory,
- artist-facing tools.

The purpose of the first chapter is discovery, not infrastructure.

---

## Immediate next action

Create and approve:

`ERIK_FLP_MASTER_v01`

Then produce the first coherent Headbang sequence outside Unity.

Only after that sequence passes visual and cleanup checks should implementation begin on `develop-flp`.

---

## Working principle

Do not optimize a pipeline that has not been proven.

First prove that one Erik can move.
Then prove that the same Erik can perform a second move.
Then automate what actually hurts.
