# Scoring System v1

> Status: confirmed scoring / combo / HYPE / THE BANG / end-of-song grade direction for Headbang Heroes. Exact thresholds and weights remain tuning variables and must be data-driven.

## Design goals

The scoring system must reward three distinct things without collapsing them into one number too early:

1. **Timing consistency** — represented mainly by combo and judgment streaks.
2. **Ongoing successful play** — represented by the scoring multiplier.
3. **High-quality performance intensity** — represented by HYPE.

A player should be able to understand why a run felt good or bad from the result sheet rather than seeing only one opaque score total.

The system must remain readable, deterministic and tunable.

---

## Judgment set

Active timing judgments:

- PERFECT
- GREAT
- GOOD
- MISS

Timing and Motion Quality remain separate signals.

A strong Motion Quality result does not convert a weak timing result into a better timing judgment, and a PERFECT timing result does not guarantee strong Motion Quality.

---

## Combo

Combo represents consecutive high-quality timing execution.

### Combo-building judgments

- PERFECT -> combo +1
- GREAT -> combo +1

For combo purposes, PERFECT and GREAT have equal value.

### Combo-breaking judgments

- GOOD -> ends the current combo
- MISS -> resets the current combo to zero

GOOD is intentionally more forgiving than MISS. It stops the streak but is not treated as a catastrophic failure.

### Why combo is elastic

The player does not need a pure-perfect streak to build combo.

A sustained mix of GREAT and PERFECT should still feel like a strong performance.

This keeps the system demanding without becoming excessively brittle.

### Metrics tracked

At minimum track:

- current combo
- longest combo
- number of completed combo streaks if useful for results/tuning

---

## Multiplier

Multiplier represents sustained successful play across valid chart events.

Every correctly hit event advances multiplier progress.

Candidate progression model:

- GOOD -> advances multiplier progress
- GREAT -> advances multiplier progress
- PERFECT -> advances multiplier progress
- MISS -> resets multiplier progress / multiplier state

Exact progression thresholds are tuning data.

Example prototype thresholds:

- x1 baseline
- 5 successful hits -> x2
- 10 successful hits -> x3
- 20 successful hits -> x4
- 30 successful hits -> x5

These values are placeholders only.

The multiplier system must be parameterized so thresholds and maximum multiplier can be changed without redesigning scoring code.

### Relationship to combo

Combo and multiplier are intentionally related but not identical:

- combo cares only about PERFECT/GREAT continuity
- multiplier rewards broader successful continuity, including GOOD

This gives GOOD a useful place in the scoring system without allowing it to maintain combo.

---

## HYPE generation

HYPE represents performance intensity / how hard the run is currently hitting.

Confirmed base judgment contribution:

- PERFECT = +2 HYPE
- GREAT = +1 HYPE
- GOOD = +0 HYPE
- MISS = +0 HYPE

For this purpose PERFECT is worth twice GREAT.

Additional HYPE sources may later include:

- strong Motion Quality
- technically demanding authored moments
- special technique execution
- sustained flow bonuses

Any additional contribution must remain parameterized and understandable.

### MISS behavior

MISS does **not** remove accumulated HYPE.

This is a deliberate rule.

A mistake destroys combo, but it does not erase all performance energy built during the song.

This separates the emotional role of HYPE from the punitive role of combo.

---

## HYPE MAX and THE BANG

When HYPE reaches maximum, the top-bar HYPE control enters its READY state.

The player manually activates THE BANG by tapping the HYPE control.

THE BANG uses the previously defined hybrid model:

1. activation starts a short enhanced-performance window
2. normal headbang gameplay continues
3. scoring/presentation are strongly amplified
4. the first suitable high-value moment inside the window becomes a Finisher / Special Bang payoff

---

## THE BANG scoring modifier

Confirmed prototype direction:

**THE BANG applies a 10x multiplier to its reward domain.**

This value must be fully parameterized.

Recommended tuning fields:

```text
TheBangScoreMultiplier = 10.0
TheBangMotionRewardMultiplier = 10.0
TheBangFinisherMultiplier = 10.0
```

The exact final values are not design constants.

The purpose of starting at 10x is to make THE BANG feel unmistakably like a limit-break state during prototype testing.

### HYPE generation during THE BANG

Do not automatically apply the same 10x multiplier to HYPE generation.

Reason: multiplying HYPE refill can create a runaway loop where THE BANG immediately refills itself and becomes effectively permanent.

Prototype-safe options:

- HYPE generation remains normal during THE BANG
- or HYPE generation is temporarily disabled during THE BANG

This behavior must also be parameterized.

---

## Finisher

The memorable Special Bang payoff inside THE BANG is formally tracked as a **Finisher**.

Conceptual flow:

```text
HYPE MAX
  -> player taps HYPE
  -> THE BANG active
  -> first suitable high-value moment
  -> FINISHER
```

A Finisher uses the existing neck action being performed rather than introducing a separate input language.

The exact deterministic trigger remains tuning/prototype work.

Candidate trigger rules include:

- first high-quality accented event after activation
- first authored strong beat / phrase accent
- first event above a defined Motion Quality threshold

The trigger must remain predictable once selected.

Track at minimum:

- Finishers executed
- possible Finishers available if the chart explicitly defines eligible opportunities
- Finisher score contribution

---

## Event scoring architecture

The current scoring model should remain modular and data-driven.

Conceptually:

```text
EventScore =
    BaseScore
  x TimingFactor
  x MotionQualityFactor
  x TechniqueFactor
  x CurrentMultiplier
  x TheBangModifier
```

Not every factor must start with complex tuning in the first implementation.

The important architectural rule is that each factor remains separable and independently configurable.

### Timing vs Motion Quality

Hard rule:

> Timing judges when the player acted. Motion Quality judges how well the neck actually moved.

They are separate signals.

This allows outcomes such as:

- PERFECT timing + weak motion = good timing but mediocre physical execution
- GREAT timing + excellent prepared motion = potentially stronger total performance value

Neither dimension should completely erase the other.

---

# End-of-song Performance Report

The result screen should feel like a **mission/performance report**, not just a score dump.

The player receives a full post-song "pagella".

Track/display at minimum:

- total score
- final grade
- current/best scoring multiplier if useful
- PERFECT count
- GREAT count
- GOOD count
- MISS count
- longest combo
- combo-related performance metric(s)
- total HYPE accumulated during the song
- THE BANG activations if useful
- Finishers executed

Example structure:

```text
HEADBANG REPORT

SCORE                  483,200
MAX COMBO                   87
COMBOS COMPLETED            14
TOTAL HYPE                 126
FINISHERS                     3
PERFECT                     74
GREAT                       31
GOOD                         8
MISS                         4

FINAL GRADE

             S
```

Exact visual layout belongs to the Results Screen specification.

---

## Final grade

Use the grade set:

- S
- A
- B
- C
- D

The final grade should be based on multiple performance dimensions rather than simple note accuracy alone.

Confirmed grade inputs:

- total score
- combo performance
- longest combo
- total HYPE accumulated
- Finishers executed

A player should not receive the best possible grade merely by being technically safe if the performance lacks intensity.

Likewise, raw aggression without consistency should not automatically produce the best grade.

### Candidate weighted model

Initial prototype hypothesis:

```text
Score contribution       40%
Longest Combo            20%
HYPE                      20%
Finishers                 15%
Other Combo metric         5%
```

These weights are placeholders and must be parameterized.

The grade computation should normalize metrics against song/chart expectations so different songs and difficulties remain comparable.

Do not hardcode one raw-score threshold across all songs.

---

## Grade philosophy

The result screen should answer:

- Did you hit the chart?
- Did you maintain control?
- Did you build meaningful combo?
- Did you generate HYPE?
- Did you use THE BANG effectively?
- Did you execute Finishers?

The final grade should summarize the **quality of the whole performance**, not only accuracy.

---

## End-of-song commentary

The result screen should include a short metal-themed line commenting on the performance.

This is part of Headbang Heroes' identity and should not be treated as generic flavor text.

Use pools of lines by grade and, later, contextual conditions.

### Example grade pools

S examples:

- `The venue may never structurally recover.`
- `Neck integrity: questionable. Performance: flawless.`

A examples:

- `Almost enough violence. Almost.`

B examples:

- `Respectable. The pit remains unconvinced.`

C examples:

- `Technically metal.`

D examples:

- `Your neck has filed for resignation.`

These examples establish tone, not final content count.

### Contextual commentary

Future commentary selection may also react to run characteristics such as:

- zero MISS
- unusually long combo
- very high PERFECT count
- high score but low HYPE
- high HYPE but poor consistency
- THE BANG never activated
- multiple Finishers executed
- perfect / near-perfect Finisher use
- excessive MISS count

The commentary system should select from authored pools rather than generate unpredictable live text.

This keeps tone consistent and localization manageable.

---

## Parameterization guardrail

All important scoring constants must live in configuration/data rather than scattered hardcoded values.

At minimum parameterize:

- judgment score factors
- HYPE values per judgment
- combo rules if later tuning changes them
- multiplier thresholds
- maximum multiplier
- THE BANG duration
- THE BANG score multiplier
- THE BANG motion reward multiplier
- THE BANG Finisher multiplier
- HYPE behavior while THE BANG is active
- grade contribution weights
- S/A/B/C/D thresholds
- song/chart normalization targets

The current document defines system semantics. Numbers are prototype defaults, not sacred values.

---

## Confirmed semantic rules summary

```text
PERFECT -> combo +1, multiplier progress, +2 HYPE
GREAT   -> combo +1, multiplier progress, +1 HYPE
GOOD    -> combo ends, multiplier progress, +0 HYPE
MISS    -> combo reset, multiplier reset, HYPE preserved
```

THE BANG:

```text
manual activation at HYPE MAX
short hybrid super window
normal gameplay continues
prototype reward multiplier = 10x
one suitable moment becomes a Finisher
```

Results:

```text
Performance Report
+ Score
+ Judgment breakdown
+ Combo metrics
+ Total HYPE
+ Finishers
+ Final Grade S/A/B/C/D
+ Metal-themed commentary
```

---

## Open tuning questions

The following are intentionally not locked yet:

- exact multiplier thresholds
- maximum multiplier
- exact BaseScore values
- exact timing factors
- exact Motion Quality factors
- whether GOOD merely ends combo or explicitly records a separate streak break statistic
- exact THE BANG duration
- exact Finisher trigger
- HYPE generation during active THE BANG
- normalization formula per chart/difficulty
- final grade weights and thresholds
- exact contextual commentary rules

These should be solved through prototype telemetry and playtesting without changing the semantic guardrails above.
