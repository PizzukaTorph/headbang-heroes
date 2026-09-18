# POC / MVP Scope v1

## Core distinction

Headbang Heroes uses two milestones:

> **POC proves the mechanic. MVP proves the game.**

This keeps the first laboratory small without pretending one prototype song is already the full MVP.

---

# POC

The POC consists of **one complete playable song**, expected initially at Normal difficulty.

The POC is successful when the full loop works end-to-end:

```text
Home
→ Song Select
→ Pre-song
→ Gameplay
→ Results
→ Retry / Continue
```

The song must exercise enough of the actual system to validate the game, including:

- authoritative audio timing
- CURRENT / NEXT cues
- input always affecting neck motion
- Classic Bang inversion behavior
- timing judgments
- Motion Quality
- combo / multiplier
- HYPE
- THE BANG
- at least one valid Finisher path
- body reaction
- hair/secondary motion at a representative level
- Results
- save of relevant run/profile data
- immediate Retry

The decisive POC question is:

> **Is controlling the head/neck to music actually fun?**

Visual polish can remain incomplete.

---

# MVP

The MVP consists of **four complete real songs** that can be played end-to-end reliably.

The four-song requirement is intentionally more important than a large feature checklist.

Each song must:

- be represented through the real song/chart content model
- load through the intended content pipeline or a faithful MVP equivalent
- play from start to finish
- remain synchronized
- produce deterministic judgments/scoring
- support HYPE/THE BANG according to the chart
- reach Results correctly
- persist records/progression correctly
- retry correctly

The catalog may contain songs at different authored difficulty classifications.

The MVP does not require every song to exist at every difficulty.

---

## MVP supporting shell

The MVP should include enough shell to make the four-song build feel like a small real game:

- Home
- Song Select
- Avatar customization in its simple slide-based form
- Profile/progression persistence
- Settings/calibration entry point
- Results/rewards
- Quick access to replay

Full polish or breadth of secondary systems is not required.

---

## Not mandatory for POC

The POC does not require:

- four songs
- production backend
- online account sync
- Versus
- Story campaign structure
- Endurance
- Practice section controls
- community/mod browser
- external chart editor
- production economy balance
- full cosmetic catalog
- advanced venue catalog
- final tutorial presentation
- final art polish

---

## Not automatically mandatory for MVP

The MVP proves a complete small product, not every planned mode.

The following may remain post-MVP if they threaten the four-song goal:

- networked Versus implementation
- community content publishing
- external chart editor
- complex Story progression/narrative
- large-scale content CMS
- advanced social features
- leaderboards
- elaborate economy/rarity systems
- large avatar catalog

Architecture may anticipate them without requiring implementation.

---

## MVP acceptance mindset

Do not reject useful small additions merely because they were not listed here.

However, additions must not compromise the primary outcome:

> **Four real songs work completely and reliably as Headbang Heroes.**

When a proposed feature competes with that outcome, the four-song loop wins.
