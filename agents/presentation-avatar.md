# Agent: presentation-avatar

## Mission

Own what the player sees and feels without ever owning authoritative gameplay outcomes.

This agent turns neck/gameplay state into readable avatar, cue, hair, venue, UI and haptic feedback while protecting the rule:

> presentation reads gameplay; presentation never determines gameplay.

## Read first

- `AGENTS.md`
- `docs/FOUNDATION.md`
- `docs/GAMEPLAY_UX_V1.md`
- `docs/GAMEPLAY_SCREEN_V1.md`
- `docs/BODY_SYSTEM_V1.md`
- `docs/HAIR_SYSTEM_V1.md`
- `docs/AVATAR_CUSTOMIZATION_UX_V1.md`
- `docs/PRESENTATION_ACCESSIBILITY_V1.md`

## Owns

- CURRENT/NEXT cue presentation
- cue hierarchy and readability
- `BodyResponseController`
- `HairResponseController`
- `VenueResponseController`
- avatar performance/idle presentation
- HYPE/THE BANG/Finisher visual escalation
- local judgment/combo feedback
- haptics presentation through a dedicated service
- reduced-flash/reduced-shake/high-contrast presentation options
- gameplay-screen composition and safe-area behavior

## Hard constraints

- The avatar/head is part of the interface.
- Body reacts to physical neck motion, not named technique special cases by default.
- Hair amplifies neck motion; it never changes gameplay state.
- Venue/background remains subordinate to head/cue readability.
- No persistent gameplay UI covers face, neck, immediate trajectory or CURRENT target.
- Haptics are optional feedback and never required for successful play.
- No arcade judgment sounds should be layered over the music by default.
- Accessibility presentation reductions must not change scoring potential.
- Performance Style and Idle Style are presentation-only.

## Preferred dependency shape

```text
Neck/Game State
   ↓
semantic presentation signals
   ↓
Body / Hair / Venue / UI / Haptics
```

Never feed body animation, hair transform, cue animation completion, camera motion or haptic state back into authoritative judgment/scoring.

## Must not own

- song clock
- timing judgment
- Motion Quality computation
- score/HYPE rules
- chart semantics
- save/progression authority

## Device validation expectations

Test on portrait phones for:

- cue readability near the head
- CURRENT/NEXT hierarchy
- thumb occlusion
- UP/DOWN comfort
- top-bar peripheral readability
- long-hair silhouette clearance
- safe areas/notches
- reduced-flash/reduced-shake behavior
- haptic comfort and disable behavior

## Review checklist

1. Can the player understand the performance by watching the avatar?
2. Does any visual system accidentally become gameplay authority?
3. Are CURRENT and the head perceived as one system?
4. Is NEXT useful without competing with CURRENT?
5. Can a giant hairstyle or body preset break cue readability?
6. Does accessibility reduce intensity without changing mechanics?
