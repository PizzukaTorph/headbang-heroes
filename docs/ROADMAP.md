# Headbang Heroes — Roadmap

## Principle

The roadmap exists to protect execution order, not to collect every future idea.

Primary milestone logic:

> **POC proves the mechanic. MVP proves the game.**

The POC must make the headbang fun.
The MVP must prove that the same foundation scales across four complete real songs.

---

# P0 — POC: Make The Headbang Fun

## Goal

Prove one complete song and the core neck/rhythm loop end-to-end.

## Required gameplay

- authoritative DSP/audio clock
- one complete authored chart, initially Normal
- CURRENT / NEXT cue flow
- semantic directional input
- Classic Bang Horizontal + Vertical sufficient to validate the core
- physical input always affects the neck
- continuous neck simulation
- timing judgment
- event-local Motion Quality
- combo / multiplier
- HYPE
- THE BANG
- at least one deterministic Finisher opportunity
- body response
- representative hair/secondary motion
- Results
- Retry

## Required technical validation

- song remains synchronized start-to-finish
- pause/resume and retry remain synchronized
- frame pacing does not define timing judgment
- gameplay-critical neck state moves toward an authoritative fixed-step/otherwise deterministic model before production scoring relies on it
- malformed chart fails clearly
- real-device input latency can be inspected/calibrated

## Shell

Enough shell to exercise the loop:

```text
Home
→ Song Select
→ Pre-song
→ Gameplay
→ Results
→ Retry / Continue
```

No large campaign, backend, shop, realtime multiplayer, community system, or content CMS is required.

## Exit gate

A player can finish the song and wants to replay because controlling the neck itself is enjoyable, not because of progression rewards.

---

# P1 — Product Vertical Slice

## Goal

Turn the proven mechanic into a credible miniature Headbang Heroes experience before broadening content.

## Focus

- polished gameplay composition
- stronger avatar/body/hair presentation
- first convincing venue/background reaction profile
- simple avatar customization
- first-run/tutorial flow
- settings/calibration
- local profile/save
- progression/results integration
- server-first content path or faithful production-shaped equivalent
- validated content/package workflow

## Scope guidance

A second/third internal track may be useful for stress-testing tempo/style differences, but P1 is not defined by a fixed song count.

The important question is whether the complete product loop feels coherent around the proven mechanic.

---

# P2 — MVP: Four Real Songs

## Goal

Prove that Headbang Heroes is a small complete game, not a one-song prototype.

## Definition

**Four complete real songs** work end-to-end reliably.

Each must:
- use the canonical Song/Chart model
- carry an authored difficulty classification
- load through the real/final-shaped content pipeline
- remain synchronized from start to finish
- resolve timing/motion/technique deterministically
- support HYPE/THE BANG/Finisher where authored
- produce correct Results
- persist records/progression
- retry cleanly
- remain playable offline once validly cached where licensing/product rules allow

The MVP does not require every song at every difficulty.

## Supporting MVP product surface

- Home
- Song Select
- simple Avatar customization
- Results / progression
- Profile/save
- Settings / calibration / accessibility basics
- official catalog filters
- Quick Headbang path

## Explicitly optional / deferrable beyond MVP

- networked Versus
- complete Story campaign
- advanced Practice tools
- Endurance
- leaderboards
- asynchronous challenges
- shop/large cosmetic catalog
- community publishing/browser
- external chart editor
- full CMS
- elaborate economy balancing
- large venue library

If one of these threatens the four-song goal, it loses.

---

# P3 — Post-MVP Product Expansion

## Goal

Expand around the proven four-song product without redefining the core gameplay semantics.

Candidate workstreams:

### Story
- structured progression/narrative
- lightweight scenes/dialogue
- bosses/rivals
- authored challenges
- larger catalog usage

### Versus
- asynchronous competition first where appropriate
- exact chart/rules version references
- leaderboard/competitive integrity work
- realtime battle only after validation

### Practice
- phrase/section selection
- repeat/retry tooling
- technique-focused practice
- playback-speed experiments only if musically/technically acceptable

### Endurance
- longer sequences
- cumulative scoring/HYPE management
- session rules using existing gameplay systems

### Content scale
- larger official catalog
- improved publication tooling
- artist/band content workflows
- richer venues/presentation
- external chart tooling when authoring scale justifies it

### Community/modding
- local/community packages
- curated publishing/trust rules
- official/community separation
- rights/audio provenance constraints

---

# P4 — Mature v1 / Live Product

Possible mature-product capabilities after the core and expansion modes prove worthwhile:

- polished Story arc
- strong Versus offering
- larger official music catalog
- robust content release pipeline
- achievements/challenges
- social/leaderboards as justified
- store/platform compliance if monetization is introduced
- privacy-conscious analytics
- live content cadence
- curated community ecosystem if viable

These are not foundation requirements.

---

# Cross-cutting roadmap rules

## Gameplay before meta

No progression, economy, story, or online feature may be used to hide weak neck gameplay.

## Content before complexity

Once the mechanic works, proving multiple real songs is more valuable than accumulating secondary systems.

## One gameplay language

Modes compose the same core chart/neck/scoring/HYPE systems. Do not fork separate gameplay implementations per mode.

## Server-first content, offline-capable play

Compatible official content can be delivered remotely and cached locally. Remote content never introduces unknown gameplay semantics.

## No power progression

Progression unlocks expression/content, not mechanical advantage.

## Monetization later

No gameplay ads, shop complexity, premium-currency design, or reward-loop optimization is required to prove POC/MVP. Monetization is a separate later design problem.

## Real-device validation

Audio/input latency, touch ergonomics, performance, and haptics must be validated on actual iOS/Android devices early and repeatedly.
