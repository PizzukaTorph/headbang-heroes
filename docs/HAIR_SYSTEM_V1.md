# Hair System v1

> Status: design contract for the current remote working branch.

## Core rule

**Hair motion amplifies neck motion; it does not replace it.**

Hair is not gameplay. It is secondary motion used to make speed, inversion, amplitude and flow visually readable and dramatically metal.

The runtime relationship is:

`NECK MOTION -> HAIR RESPONSE -> VISUAL AMPLIFICATION`

Hair must never feed back into scoring, timing windows, Motion Quality or the neck simulation.

## Design target

The visual target is exaggerated adult-metal cartoon energy rather than realistic salon-grade hair physics.

Priorities:
- readable silhouette
- delayed violence after neck inversion
- controlled overshoot
- strong cause/effect relationship with the neck
- deliberately exaggerated motion when it improves readability

The system should feel closer to animated metal performance than to physically exact strand simulation.

## Motion inputs

The hair solver should primarily consume existing neck/presentation signals:
- neck angular position / trajectory
- angular velocity
- angular acceleration / momentum change
- inversion events
- HYPE as a limited presentation multiplier

Conceptually:

`HairResponse = NeckMotion + InversionKick + GravityBias + HypeExaggeration`

Exact implementation details can evolve, but named neck techniques should not require bespoke hair systems.

## Motion tiers

Hair length is separated from individual hairstyle artwork. Each hairstyle selects a reusable motion tier.

| Motion Tier | Typical segments | Intent |
| --- | ---: | --- |
| **Bald / Shaved** | 0 | no hair secondary motion; useful baseline and base for future head specials |
| **Short** | 0-1 | minimal lag, dry/snappy response |
| **Medium** | 1-2 | moderate lag and readable secondary motion |
| **MediumLong** | 2-3 | clearly delayed, strongly metal motion |
| **Long** | 3-4 | hero hair; large silhouette, high overshoot and longer settling |
| **Special** | custom/reused chains | mohawks, dreadlocks, braids, ponytails and other exceptional silhouettes |

These are **motion tiers**, not hairstyles. Multiple visual hairstyles should reuse the same tier/solver configuration wherever possible.

## Tier character

Increasing tier length should not only add segments. It also changes motion character.

Longer tiers generally use:
- lower stiffness
- higher inertia
- larger allowed bend
- stronger inversion overshoot
- longer settling time

Shorter tiers remain tighter and more directly attached to head movement.

## Hair weight

Length and weight are independent dimensions.

Initial weight profiles:
- **Light**
- **Normal**
- **Heavy**

Weight changes motion character without requiring a new length tier.

Examples:
- long straight/light hair: large fast spread and lively recovery
- long thick/heavy hair: slower, weightier follow-through
- dreadlocks: long/heavy with higher damping and more restrained recovery

**The length tier decides rig complexity; hair weight decides movement character.**

## Front and back separation

For the 3/4 gameplay camera, complex hairstyles should support separate motion treatment for:
- **BackMass** - main visible mass; freer motion, larger overshoot and silhouette expansion
- **FrontStrands** - shorter/more controlled chains used to preserve face readability

Front strands should not repeatedly obscure the face during ordinary play unless a specific hairstyle intentionally uses that silhouette.

A Long hairstyle might therefore use 3-4 back segments but only 1-2 more constrained front segments.

## Inversion behaviour

Neck inversion is the key visual event.

When the head changes direction, longer hair should briefly continue along the previous trajectory before recovering and following the new motion.

This creates the intended sequence:

`HEAD INVERTS -> HAIR CONTINUES -> HAIR SNAPS/FOLLOWS -> OVERSHOOT -> SETTLE`

The delay and overshoot sell the violence of a successful bang.

Hair motion must remain controlled rather than random. The player should always be able to visually connect hair motion to the preceding neck motion.

## HYPE

HYPE may increase hair presentation moderately, for example:
- slightly higher max bend
- slightly more overshoot
- slightly larger visual spread
- slightly more recovery bounce

HYPE must not radically change the underlying hair behaviour. Suggested initial exaggeration range is modest (approximately 10-20% around the base profile) and should be tuned visually.

## Special hair / head content

Special content should reuse the same principles where practical:
- **Mohawk** - mostly rigid/minimal spring
- **Dreadlocks** - multiple heavier damped chains
- **Braids** - a small number of independent chains
- **Ponytail** - one primary chain
- **Bald/Shaved** - zero hair solver; clean base for headgear and other specials
- **Headgear / horns / attachments** - separate attachment response when needed, not forced into normal hair physics

Special content may use custom configuration, but should reuse existing solver components before introducing another physics system.

## Initial validation set

The first useful prototype only needs a small reusable set:
1. **Bald / Shaved** - validates neck/body without hair assistance
2. **Short** - validates minimal secondary response
3. **Medium** - validates 1-2 segment chain
4. **MediumLong** - validates stronger delayed motion
5. **Long Metal** - validates 3-4 segment hero hair and the intended exaggerated metal silhouette

The Long Metal case is the primary stress/feel test. If inversion, lag, overshoot and settling read clearly there, the architecture is likely viable for the remaining tiers.

## Architectural constraints

1. Hair never changes gameplay state.
2. Hair responds to neck motion, not to named neck techniques.
3. New hairstyles should primarily be new artwork plus reusable motion profiles, not new solver code.
4. New length tiers should only be introduced when existing segment complexity cannot represent the desired silhouette.
5. Realistic physics is subordinate to readable, exaggerated metal performance.
6. A bald avatar must still communicate a satisfying headbang. Hair improves a good system; it must not conceal a weak neck/body system.
