# Avatar Customization UX v1

## Status

This document defines the intentionally simple avatar customization experience for Headbang Heroes.

The system is not intended to be a deep character creator.

Its primary purpose is to let the player quickly reach the feeling:

> **"Cazzo, sono io."**

That sense of personal recognition matters more than exhaustive customization depth.

---

## Core principle

Keep avatar customization simple, fast, visual, and immediately rewarding.

The customization flow should behave like a sequence of focused slides rather than a dense editor.

> **One category. One decision. Immediate preview.**

The player should always see the avatar while changing it.

---

# 1. Interaction model

The customization experience is organized as a linear sequence of slides.

Example flow:

```text
BODY
↓
FACE
↓
HAIR
↓
BEARD / MAKEUP / PIERCINGS
↓
APPAREL
↓
SPECIAL
↓
PERFORMANCE STYLE
↓
IDLE STYLE
↓
DONE
```

Each slide contains only one primary category.

The player changes options by:

- swipe left/right
- previous/next arrows
- direct tap on simple option controls where appropriate

Each change updates the avatar immediately.

---

# 2. Visual structure

The avatar must remain the dominant visual element.

Conceptual layout:

```text
<        LONG HAIR 03        >

          [ AVATAR ]

      BACK        NEXT
```

The exact layout may evolve, but the hierarchy should remain:

1. avatar
2. current category/option
3. previous/next interaction
4. navigation

Avoid interfaces that visually resemble:

- an e-commerce catalog
- a complex inventory
- a desktop asset browser
- a spreadsheet of cosmetic options

---

# 3. First-run creator

The first-run creator uses the complete slide flow.

Its purpose is not to make the player design a perfect character before playing.

Its purpose is to create immediate ownership and recognition quickly.

The player should be able to move through the creator rapidly and reach gameplay without excessive setup.

Do not block the first playable song behind a long customization ritual.

---

# 4. Avatar menu after first run

The `AVATAR` entry from Home reuses the same customization system.

The difference is navigation:

```text
First Run
→ complete slide sequence

Home > Avatar
→ choose/edit a category directly
```

The game should not maintain two independent customization UIs.

One system, two entry contexts.

---

# 5. v1 categories

The customization system should support the following conceptual categories.

## Body

Discrete body presets such as:

```text
S
M
L
XL
```

These are presentation presets only.

They must not affect gameplay, scoring, reach, timing windows, or neck physics.

## Face

A small set of distinct face archetypes.

The goal is recognizability and variety, not photorealistic facial construction.

## Hair

Hair is both a customization choice and a secondary-motion asset.

The selected hairstyle references the appropriate hair motion tier/profile defined by the hair system.

The customization UI chooses appearance.

It does not expose physics parameters to the player.

## Beard / Makeup / Piercings

These may be presented as one combined accessory/customization stage or split only if content volume eventually requires it.

Do not create deep sub-navigation for small v1 content sets.

## Apparel

Clothing selection should use the same simple preview-first interaction.

Apparel must not carry gameplay stats.

## Special

Reserved for strong identity options such as:

- corpse paint
- unusual face treatments
- future themed cosmetic features

`Special` is intentionally broad so unusual content does not require redesigning the creator.

---

# 6. Performance Style

Performance Style defines how the avatar visually performs the same gameplay.

Examples already defined include:

- Doomer
- Thrasher
- Death
- Black

The customization UI must preview the selected style immediately.

A player selecting a style should see a short representative movement or response loop rather than only reading a text label.

Performance Style must never affect:

- chart difficulty
- timing windows
- neck simulation semantics
- MotionQuality rules
- maximum scoring potential

It is expressive presentation only.

---

# 7. Idle Style

Idle Style controls non-gameplay presentation behavior during natural rest and relevant menu/home contexts.

Examples may include:

- Style Default
- Stare
- Nod
- Sway
- Stretch
- Loose

The selected Idle Style should preview immediately.

Authored gameplay Rest always overrides cosmetic idle behavior when stillness is required.

---

# 8. Immediate preview

Every customization change should be visible immediately.

Avoid a workflow where the player:

```text
selects options
↓
presses Apply
↓
finally sees the result
```

Prefer:

```text
change option
↓
avatar updates now
```

This is particularly important for:

- hair
- body
- apparel
- performance style
- idle style

The preview itself is part of the reward.

---

# 9. Home integration

The customized avatar is not only visible during gameplay.

The Home screen uses the player's avatar as a major visual element.

This increases the value of customization without adding mechanical complexity.

The player should repeatedly see the character they created in:

- Home
- Avatar customization
- pre-song presentation where appropriate
- gameplay
- results

Customization therefore supports identity across the whole game shell.

---

# 10. No gameplay power

Avatar customization is strictly non-power progression.

Cosmetics and appearance must not provide:

- score bonuses
- HYPE bonuses
- better timing windows
- easier techniques
- altered neck physics
- faster progression
- hidden gameplay advantages

This aligns with the progression rule:

> **Progression unlocks expression and content, not power.**

---

# 11. Scope guardrails

The avatar creator is deliberately not a headline gameplay system.

Do not turn v1 into:

- a deep RPG character creator
- facial morph sliders
- dozens of body measurements
- layered inventory management
- cosmetic rarity dashboards
- loot-box presentation
- complex dye systems
- outfit stat systems
- nested filtering and sorting tools

Content volume may grow later without changing the interaction philosophy.

---

# 12. Content scaling rule

If a category eventually becomes too large for simple left/right stepping, the UI may add a lightweight secondary selector.

However, the core rule should remain:

> **The avatar stays visible and the current choice remains the focus.**

Do not sacrifice preview clarity just because content volume grows.

---

# 13. v1 success criteria

The system is successful if:

- the player can create a recognizable personal avatar quickly
- the avatar remains visible through the process
- every change previews immediately
- the flow works comfortably on mobile
- the same customization system can be reused after first run
- Performance Style and Idle Style are previewable
- cosmetics remain presentation-only
- the creator does not delay the player from reaching gameplay

The emotional test is simple:

> **The player should look at the finished avatar and think: "yeah, that's me."**

That is enough for v1.
