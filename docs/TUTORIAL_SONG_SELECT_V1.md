# Tutorial + Song Select UX v1

## Tutorial principle

The tutorial should feel like playing a very easy Headbang Heroes song, not reading a manual.

> **Teach one thing, make the player do it immediately, then keep the music moving.**

The preferred tutorial vehicle is a super-Easy song or specially authored onboarding chart.

---

## Tutorial presentation

When introducing a new concept:

```text
music approaches teaching moment
→ controlled slowdown / near-pause
→ short overlay
→ player performs the shown action
→ music resumes
```

Do not leave long instruction screens between gameplay moments.

The slowdown is instructional presentation; the authored musical timeline/runtime must remain deterministic and explicitly handle the tutorial state rather than drifting accidentally.

---

## Initial teaching order

Suggested first-run sequence:

1. LEFT / RIGHT input
2. inversion idea: tap on the side where the cue closes, neck launches away
3. CURRENT / NEXT visual grammar
4. PERFECT / GREAT / GOOD / MISS
5. first simple Vertical command
6. Natural Rest and/or one simple Authored Rest
7. HYPE accumulation
8. HYPE READY + THE BANG activation
9. Finisher payoff on an intentionally obvious candidate

Do not teach the full advanced vocabulary in the first tutorial.

Half, Deep, Whiplash, Windmill, Burst and advanced combinations can be learned through later content, contextual onboarding, or Practice.

---

## Tutorial input philosophy

Do not replace the real controls with fake tutorial controls.

The player should learn the actual gameplay mapping and see the actual neck/body response.

Failure during the tutorial should prefer immediate retry/re-explanation over punitive run failure.

---

## Replaying / skipping

After first completion:

- tutorial can be replayed from an appropriate menu/help/practice entry
- tutorial prompts can be skipped by experienced players
- normal songs should not repeatedly interrupt with basic onboarding

---

# Song Select

Song Select should be visually simple and optimized for getting into a song quickly.

Conceptual song card/list item:

```text
[COVER]

ARTIST
SONG TITLE

NORMAL / HARD / EXTREME

Best Grade: A
Best Score: 1,284,530
```

Recommended information:

- artwork/cover
- artist
- song title
- authored difficulty
- duration
- best grade
- best score
- locked/unlocked state
- download/cache state when remote content is used
- small genre/subgenre label where useful

Optional future detail:

- audio preview
- longest combo
- chart version/debug info in development builds

Do not turn the main list into a statistics dashboard.

---

## Difficulty browsing

For the official catalog, preferred top-level filters:

```text
ALL | EASY | NORMAL | HARD | EXTREME
```

The system must not assume every song has all difficulty variants.

Difficulty is primarily a property of the available authored chart/content.

If a song later has multiple charts, the available alternatives can be shown in its detail/pre-song state.

---

## Official content first

v1/MVP song browsing focuses on official content only.

Community content is a future catalog dimension and should not complicate the initial Song Select.

Potential future split:

```text
OFFICIAL | COMMUNITY
```

Do not implement it before community publishing exists.

---

## Interaction

Primary path:

```text
Song Select
→ tap song
→ short pre-song/detail state
→ PLAY
```

The number of required taps should remain small.

Locked songs should clearly communicate lock state and unlock requirement without pushing the player through unrelated menus.

Downloaded songs should remain directly playable offline.
