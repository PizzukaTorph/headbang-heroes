# Tutorial + Song Select UX v1

## Tutorial principle

The tutorial should feel like playing a very easy Headbang Heroes song, not reading a manual.

> **Teach one thing, make the player do it immediately, then keep the music moving.**

Use a super-Easy authored song/chart or a specially authored onboarding track.

## Teaching gates

For v1, avoid real-time audio time-stretch as a dependency.

Preferred instructional flow:

```text
approach a safe authored teaching point
→ controlled pause/freeze gate or musically safe stop
→ short overlay
→ player performs/acknowledges the concept
→ deterministic resume / pre-roll
```

If later testing proves true slowdown useful, it can be evaluated as a separate audio feature. It is not required for onboarding.

The authored song timeline must remain deterministic; tutorial presentation must never create accidental drift.

## Initial teaching order

Suggested first-run sequence:
1. LEFT / RIGHT input
2. inversion idea: tap where the cue closes; neck launches away
3. CURRENT / NEXT
4. PERFECT / GREAT / GOOD / MISS
5. simple Vertical action
6. Natural Rest and/or simple Authored Rest
7. HYPE accumulation
8. HYPE READY + THE BANG activation
9. obvious Finisher opportunity

Do not teach the complete advanced vocabulary immediately.

Half, Deep, Whiplash, Windmill, Burst, and dense combinations belong in later content/contextual onboarding/Practice.

## Tutorial controls

Use the real gameplay mapping and real neck/body response.

Do not substitute fake tutorial-only controls.

Failure should produce immediate retry/re-explanation rather than punitive run failure.

The first bang from neutral may be explained simply as the setup that starts the swing; the tutorial does not need to expose Motion Quality implementation terminology.

## Replay / skip

After first completion:
- tutorial can be replayed
- experienced players may skip prompts
- normal songs should not repeatedly interrupt with basic onboarding

---

# Song Select

Song Select exists to get the player into playable authored content quickly.

Conceptual card:

```text
[COVER]
ARTIST
SONG TITLE
HARD
Best Grade: A
Best Score: 1,284,530
```

Recommended information:
- artwork
- artist
- title
- authored chart difficulty
- duration
- best grade
- best score
- lock state when relevant
- download/cache state
- compact genre/subgenre where useful

Optional later:
- preview audio
- longest combo
- development-only chart/version data

Do not turn the list into a statistics dashboard.

## Difficulty browsing

Official catalog filters:

```text
ALL | EASY | NORMAL | HARD | EXTREME
```

Difficulty is primarily a chart/content classification.

A song does not need all difficulty variants.

## Multiple charts on one song

If the selected song has exactly one playable chart:
- show its authored difficulty
- do not add a pointless difficulty-selection step

If it has multiple playable charts:
- expose the available chart/difficulty alternatives in the detail/pre-song context
- preserve the short path to PLAY

## Official content first

MVP browsing focuses on official content.

Future split may be:

```text
OFFICIAL | COMMUNITY
```

Do not implement community navigation before community publishing exists.

## Interaction

Primary path:

```text
Song Select
→ tap playable song/chart
→ short detail/pre-song
→ PLAY
```

Downloaded valid songs remain playable offline.

Locked/unavailable content should explain the reason without routing the player through unrelated menus.
