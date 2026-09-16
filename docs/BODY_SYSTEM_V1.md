# Body System v1

> Status: design contract for the current remote working branch.

## Core rule

**The player controls the neck. The body reacts to the neck.**

The body is not a second gameplay controller and must not require dedicated body inputs in HH1.

The runtime chain is:

`PLAYER INPUT -> NECK SIMULATION -> BODY RESPONSE -> VISUAL PRESENTATION`

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

## Response chain

The body is treated as a reaction chain:

`NECK -> SHOULDERS -> TORSO -> PELVIS -> KNEES/LEGS -> FEET`

General intent:
- **Neck/head**: gameplay source.
- **Shoulders**: closest follower; relatively direct response.
- **Torso**: follow-through, mass and delay.
- **Pelvis**: balance/weight compensation.
- **Knees/legs**: absorb and return energy.
- **Feet**: predominantly anchored; steps/stomps are secondary presentation, not direct neck copying.
- **Hair/clothing/accessories**: secondary motion that strongly communicates speed, amplitude and impact.

The chain must not be a rigid 1:1 mapping. Segments may have their own follow amount, delay, inertia, damping and recovery characteristics so the avatar reads as a body with mass rather than a single rigid puppet.

## Technique independence

Classic Horizontal, Classic Vertical, Half, Deep, Whiplash, Windmill and future neck mechanics do not require bespoke fundamental body-response systems.

Examples emerge from the neck state:
- slow, large vertical motion naturally creates heavier torso follow-through
- violent inversion naturally creates stronger shoulder/torso reaction
- rapid oscillation naturally creates more frequent balance/leg response
- circular X/Y neck motion naturally propagates multidirectional response

Technique-specific authored animation is therefore optional presentation, not the foundation of body motion.

## Performance Style

The standard body response is modified by the avatar's Performance Style.

Conceptually:

`BodyResponse = StandardBodyResponse x PerformanceStyle x HYPE`

Performance Style changes presentation parameters, not gameplay values.

Examples:
- **Doomer**: heavier/slower follow-through, stronger perceived mass and settling.
- **Thrasher**: quicker, more elastic/aggressive response; more active knees/bounce.
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

Small steps, stomps, stance shifts, gestures, idle behaviours and other signature actions may be layered above the standard body response.

These actions:
- are cosmetic/presentational
- may depend on Performance Style, HYPE or musical context
- must not become required player inputs
- must not replace the standard physical response chain

## Natural Rest

During Natural Rest, avatar-specific Idle Style may control presentational behaviour (stare, nod, sway, stretch, loose movement, style default, etc.).

This cosmetic motion does not generate gameplay input.

During an explicit Authored Rest, gameplay-required stillness takes precedence over cosmetic idle behaviour.

## Architectural constraint

Adding a new neck technique should normally require **zero new fundamental body mechanics**.

If a new technique produces sensible neck position, velocity, acceleration and momentum, the standard body system should already know how to respond.

Bespoke animation should be reserved for style/signature presentation where it materially improves character identity, not used as the default solution for every neck move.
