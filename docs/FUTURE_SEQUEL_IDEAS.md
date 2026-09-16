# Headbang Heroes — Future / Sequel Ideas

> **STATUS: PARKED / NOT IN CURRENT SCOPE**
>
> This document is an idea parking lot only. Nothing here belongs to the current Headbang Heroes roadmap unless it is explicitly promoted into the GDD/ROADMAP later.
>
> **Current priority remains P0: Make The Headbang Fun.**

## Why this exists

During design discussion we explored ideas that could expand Headbang Heroes far beyond its current focused rhythm-game scope. Some are interesting, but implementing them now would add substantial systems, art, animation and content-production cost.

They are intentionally parked here so we can keep the ideas without contaminating HH1 scope.

A possible future framing could be a sequel/spinoff such as **Headbang Heroes 2: World Tour** (working title only).

## Parked concepts

### Band career fantasy

A broader progression built around growing a band:

**garage / rehearsal room → pub → club → larger venue → festival → tour**

The emphasis would be on the feeling of building *your* band rather than only progressing through rhythm charts.

Possible elements:
- create/name a band
- recruit/select bandmates
- band identity and visual evolution
- rehearsal before shows
- venue progression
- followers/reputation
- gig offers
- touring

### Band management

Potential lightweight or deeper management layer:
- bandmate traits
- musician roles
- relationships/chemistry
- upgrades
- equipment
- rehearsal choices
- setlists

This should remain separate from HH1 unless a very small element directly improves the core headbang loop.

### Systemic crowds

Crowds as more than background presentation.

Possible concepts:
- crowd energy propagation
- sections reacting differently
- synchronized headbang waves
- horns/headbang/pit escalation
- chain reactions caused by player performance
- increasingly absurd crowd states

One explored fantasy was effectively:

**make everybody headbang.**

Interesting, but not required for the current game's core validation.

### Modular venues

Instead of producing many bespoke environments, future versions could use reusable venue archetypes and modular dressing.

Possible parameters:
- capacity
- stage size
- crowd profile
- lighting
- props
- prestige
- visual theme

This could support many perceived gigs from a smaller reusable asset set.

### Generated / systemic bands and NPCs

Reuse a common modular character system to generate:
- bandmates
- rival bands
- fans
- background musicians

Possible band data model:

**members + genre + skill + popularity + look + personality + songs**

This could dramatically increase world variety without requiring bespoke characters for every encounter.

### Expanded career presentation

Potential mostly-UI systems:
- post-gig reports
- fake social feed
- fan comments
- photos/clips
- merch
- booking
- reputation
- show offers

These could sell a much larger world without requiring equivalent amounts of 3D/animation content.

### Persistent rehearsal-room hub

A band rehearsal space that evolves with progression:
- posters
- better equipment
- instruments
- photos
- trophies
- merch
- flight cases
- environmental clutter/history

The room itself could become a visual record of the player's career.

### Performance-driven animation architecture

For a larger sequel, songs should ideally drive reusable performance systems rather than require bespoke animation per track.

Potential inputs:
- BPM
- beat/bar
- MIDI/chart events
- section intensity
- authored accents

Goal:

**adding a song should primarily cost music/charting work, not a new animation production pass.**

### Absurd escalation

A future World Tour-style game could progressively abandon realism while keeping metal performance as the anchor.

Examples discussed:
- increasingly ridiculous stages
- huge pyro
- extreme festivals
- environmental chaos
- impossible venues
- intentionally over-the-top touring situations

The tone should remain affectionate toward metal culture rather than parodying its audience.

## Production principle worth remembering

The most useful idea from this exploration is broader than any individual feature:

> **Prefer systems that create reusable situations over bespoke content that must be produced repeatedly.**

If a future Headbang Heroes project becomes much larger, new content should preferably be data/assets feeding established systems rather than requiring unique code, environments and animation every time.

## Explicit boundary

None of the above changes the current project.

For **Headbang Heroes 1**, the authoritative scope remains the current GDD and ROADMAP.

In particular, do **not** use this document as justification to add:
- band management to P0/P1
- career simulation to P0/P1
- complex crowd simulation to P0/P1
- procedural venues
- NPC simulation
- touring systems
- social/merch/booking systems
- additional backend work

until the current core has passed its validation gates.

**First make the headbang fun. Then earn the right to make it bigger.**
