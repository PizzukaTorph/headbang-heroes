# Body System v1

> Status: design contract for the current remote working branch.

## Core rule

**The player controls the neck. The body reacts to the neck.**

The body is not a second gameplay controller and must not require dedicated body inputs in HH1.

The runtime chain is:

`PLAYER INPUT -> NECK SIMULATION -> BODY RESPONSE -> VISUAL PRESENTATION`

## Camera-aware scope

HH1 frames the performer as a **3/4 upper-body avatar**. The body system should only simulate and author what the camera can meaningfully show.

> **Do not build body systems for anatomy the gameplay camera does not show.**

For the current framing, the important visible response chain is:

`NECK -> SHOULDERS -> TORSO -> ARMS`

with two additional presentation branches:

- `NECK -> HAIR / HEAD ACCESSORIES`
- `TORSO -> BODY ROOT` for subtle implicit weight/bounce

Pelvis, knees, legs and feet are not first-class HH1 body systems unless a later camera or feature explicitly requires them. Weight transfer that would physically involve the lower body may be suggested through torso/root translation, rotation and settling instead of actually rigging invisible legs.

This is both a visual and production constraint.

## Standard body response

Body motion should react to the physical state of the neck rather than to named neck techniques.

**Body motion reacts to neck motion, not to named neck techniques.**

The body system should consume physical signals such as:
- neck position / angular displacement
- neck velocity
- neck acceleration / change of momentum
- movement direction / trajectory components
- momentum / energy

It should not need special cases such as `if technique == Windmill` to produce the fundamental body response.

This means a new neck technique that produces valid neck motion should automatically produce a plausible body response.

## Visible response chain

General intent:
- **Neck/head**: gameplay source.
- **Shoulders**: closest follower; relatively direct response.
- **Torso**: follow-through, mass, delay and implied whole-body weight.
- **Arms**: maintain the avatar's pose while inheriting shoulder/body motion and limited secondary spring.
- **BodyRoot**: optional invisible presentation root for subtle bounce, lean and weight-shift cues.
- **Hair/clothing/accessories**: secondary motion that strongly communicates speed, amplitude and impact.

The chain must not be a rigid 1:1 mapping. Visible segments may have their own follow amount, delay, inertia, damping and recovery characteristics so the avatar reads as a body with mass rather than a single rigid puppet.

## Technique independence

Classic Horizontal, Classic Vertical, Half, Deep, Whiplash, Windmill and future neck mechanics do not require bespoke fundamental body-response systems.

Examples emerge from the neck state:
- slow, large vertical motion naturally creates heavier torso follow-through
- violent inversion naturally creates stronger shoulder/torso reaction
- rapid oscillation naturally creates tighter repeated upper-body response
- circular X/Y neck motion naturally propagates multidirectional shoulder/torso response

Technique-specific authored animation is therefore optional presentation, not the foundation of body motion.

## Arms and pose

Arms are primarily a presentation layer, not part of the core neck simulation.

The avatar may hold a base upper-body pose (for example arms down, horns, crossed/tense posture, etc.). Shoulder and torso motion then propagate into that pose through limited additive follow-through/secondary motion.

Avoid requiring separate authored headbang animations for every arm pose. Prefer:

`BASE POSE + BODY RESPONSE + SMALL ARM SECONDARY MOTION`

## Hair priority

Hair is a high-priority secondary-motion system because the 3/4 framing keeps head, hair, shoulders and torso in the player's visual focus.

Hair should react to head motion with visible lag, overshoot and settling. Long hair may use a short spring chain; shorter styles can use simpler response models.

Hair remains presentation-only and must never feed back into neck gameplay state.

## Performance Style

The standard body response is modified by the avatar's Performance Style.

Conceptually:

`BodyResponse = StandardBodyResponse x PerformanceStyle x HYPE`

Performance Style changes presentation parameters, not gameplay values.

Examples:
- **Doomer**: heavier/slower follow-through, stronger perceived mass and settling.
- **Thrasher**: quicker, more elastic/aggressive upper-body response.
- **Death**: compact, powerful response with strong shoulders and less visual dispersion.
- **Black**: more restrained/rigid baseline with stronger theatrical exaggeration when appropriate.

Additional styles can be added later without changing neck gameplay semantics.

## Gameplay neutrality

Performance Style and body response must not change:
- timing windows
- chart semantics
- neck simulation used for gameplay judgment
- Motion Quality values
- maximum scoring potential

The body visualizes gameplay; it does not secretly alter it.

## Secondary actions

Gestures, stance shifts, arm poses, idle behaviours and other signature actions may be layered above the standard body response.

These actions:
- are cosmetic/presentational
- may depend on Performance Style, HYPE or musical context
- must not become required player inputs
- must not replace the standard physical response chain

Lower-body actions such as explicit steps or stomps are outside the current 3/4-camera body scope unless they later become visible/relevant.

## Natural Rest

During Natural Rest, avatar-specific Idle Style may control presentational behaviour (stare, nod, sway, stretch, loose movement, style default, etc.).

This cosmetic motion does not generate gameplay input.

During an explicit Authored Rest, gameplay-required stillness takes precedence over cosmetic idle behaviour.

## Initial implementation target

The first body-response spike should remain deliberately small:

`HEAD/NECK -> SHOULDERS -> TORSO`

Optionally include an invisible `BodyRoot` for subtle global lean/bounce.

Do not add leg IK, foot anchoring or lower-body simulation to this spike. Arms and hair should be added only after the core upper-body spring response proves visually convincing.

Two deliberately different tuning presets are enough to validate the architecture, for example a heavy/slower profile and a fast/aggressive profile.

## Architectural constraint

Adding a new neck technique should normally require **zero new fundamental body mechanics**.

If a new technique produces sensible neck position, velocity, acceleration and momentum, the standard body system should already know how to respond.

Bespoke animation should be reserved for style/signature presentation where it materially improves character identity, not used as the default solution for every neck move.
