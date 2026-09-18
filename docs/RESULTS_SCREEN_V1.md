# Results Screen v1

> Status: confirmed post-song performance report direction for Headbang Heroes.

## North-star rule

**Emotion first, numbers second.**

The results screen must feel like the payoff to a performance, not like a spreadsheet dump.

The player should first understand how hard they crushed the song, then be able to inspect the performance details, then see rewards/progression.

## Overall flow

The results screen is presented in three visual acts:

1. **Impact** — final grade, score, themed line, avatar reaction.
2. **Performance Report** — detailed run statistics.
3. **Rewards / Progression** — XP, HH currency, unlocks, advancement, actions.

These acts may appear sequentially or progressively animate into the same screen, but the hierarchy must remain clear.

---

## Act 1 — Impact

The first thing the player sees is the final grade.

Supported grades:

- **S**
- **A**
- **B**
- **C**
- **D**

The grade should dominate the screen visually.

The score total is shown immediately with it.

A short contextual line appears as part of the payoff.

The avatar may react to the result through a short presentation animation or pose.

Examples of tone:

### S
- “The venue may never structurally recover.”
- “Neck integrity: questionable. Performance: flawless.”

### A
- “Almost enough violence. Almost.”

### B
- “Respectable. The pit remains unconvinced.”

### C
- “Technically metal.”

### D
- “Your neck has filed for resignation.”

The exact copy pool is content, not system logic. The results system must support multiple lines per grade and conditional lines based on performance details.

---

## Contextual result commentary

The result line should not depend only on the final letter grade.

The system may select from contextual categories such as:

- zero MISS
- extremely long combo
- high PERFECT count
- unusually high total HYPE
- multiple Finishers
- no Finisher executed
- THE BANG never activated
- low grade but one spectacular Finisher
- strong score with poor combo consistency
- strong combo with weak HYPE generation

This should make the game feel like it actually observed how the player performed.

Comment selection must remain lightweight and deterministic enough to debug.

A simple rule-based tag system is preferred over complex generated dialogue for v1.

---

## Act 2 — Performance Report

After the initial impact, the detailed report becomes readable.

Confirmed v1 metrics:

- **Score**
- **Longest Combo**
- **Combos Completed**
- **Total HYPE Earned**
- **Finishers Executed**
- **PERFECT** count
- **GREAT** count
- **GOOD** count
- **MISS** count

Suggested presentation:

```text
HEADBANG REPORT

SCORE                  483,200
LONGEST COMBO                87
COMBOS COMPLETED             14
TOTAL HYPE                  126
FINISHERS                     3
PERFECT                      74
GREAT                        31
GOOD                          8
MISS                          4
```

The layout should feel like a performance card / mission debrief rather than a generic table.

### Accuracy percentage

Do **not** make accuracy percentage a required visible v1 metric.

Accuracy can remain available internally for telemetry/debug and may be exposed later if it proves useful.

The current design intentionally avoids reducing the entire performance to one sterile rhythm-game percentage.

---

## Final grade calculation

The final grade is not based only on hit accuracy.

It should reflect the total performance identity of Headbang Heroes.

Confirmed grade inputs:

- total score
- combo performance
- longest combo
- total HYPE earned
- Finishers executed

Conceptually:

```text
FinalPerformanceScore =
    ScoreContribution
  + ComboContribution
  + MaxComboContribution
  + HypeContribution
  + FinisherContribution
```

Initial v1 weighting proposal:

```text
Score        40%
Max Combo    20%
HYPE         20%
Finishers    15%
Combos        5%
```

These are starting tuning values only.

All thresholds and weights must be data-driven / configurable.

The system should support adjusting grade boundaries and contribution weights without changing gameplay code.

---

## Grade philosophy

The grade should reward both:

- technical consistency
- expressive high-quality performance

A player should not automatically earn the highest grade merely by playing safely with weak physical movement.

Likewise, one spectacular moment should not completely erase a poor overall run.

The grade exists to summarize the whole performance, not one statistic.

---

## Act 3 — Rewards / Progression

After the performance report, show rewards and account progression.

Expected reward categories:

- XP
- HH currency
- unlocks
- progression toward next level / reward tier
- song / challenge progression where applicable

Exact reward economy belongs to the progression system specification.

The results screen should consume progression outcomes rather than hardcode progression rules.

---

## Actions

Primary v1 actions:

- **RETRY**
- **CONTINUE**

RETRY immediately repeats the song with the same relevant setup where possible.

CONTINUE returns to the appropriate progression/song flow.

Additional actions such as share, detailed stats, leaderboard or replay may be added later, but should not clutter the v1 result flow.

---

## Visual hierarchy

Preferred attention order:

1. final grade
2. final score
3. contextual line / avatar reaction
4. performance report
5. rewards / progression
6. RETRY / CONTINUE

The result screen must not present every statistic at equal visual weight.

---

## Presentation behavior by grade

The grade presentation may vary in intensity.

Examples:

- **S**: oversized impact, strong visual punch, confident avatar reaction.
- **A**: high-impact presentation, slightly restrained versus S.
- **B**: solid positive presentation.
- **C**: neutral / dry presentation.
- **D**: deliberately underwhelming or sarcastic presentation, without slowing navigation.

The purpose is personality, not punishment.

---

## Avatar result reaction

The avatar should reinforce the grade presentation.

Potential reaction families:

- triumph / dominant stance
- exhausted satisfaction
- neutral acknowledgment
- mild disappointment
- comedic collapse / neck regret

These should remain presentation-only and not affect rewards.

Avatar result reactions can later respect Performance Style / Idle Style where practical.

---

## Result-comment system

Recommended v1 implementation model:

```text
ResultComment
- id
- minGrade / maxGrade
- requiredTags[]
- excludedTags[]
- priority
- textKey
```

Example tags:

```text
ZERO_MISS
HUGE_COMBO
HIGH_HYPE
MANY_PERFECTS
MULTIPLE_FINISHERS
NO_FINISHER
NO_THE_BANG
CHAOTIC_RUN
ONE_HEROIC_FINISHER
```

This gives the game personality without coupling copy directly to scoring code.

Localization must use text keys rather than hardcoded English strings.

---

## Data contract

The gameplay session should provide the results screen a summary object equivalent to:

```text
RunResult
- score
- finalGrade
- perfectCount
- greatCount
- goodCount
- missCount
- longestCombo
- completedComboCount
- totalHypeEarned
- finishersExecuted
- theBangActivations
- derivedResultTags[]
- rewards
```

The results UI should render this data and must not recalculate authoritative gameplay scoring.

---

## Scope guardrails

The v1 results screen should NOT become:

- a dense analytics dashboard
- a leaderboard-first experience
- a mandatory social-sharing screen
- a monetization interruption
- a place where core score logic is recalculated
- a screen with dozens of secondary metrics

The core fantasy is:

**“The song ended. Here is your brutal performance report.”**

---

## Acceptance criteria

The results flow is successful when:

- the grade is understood immediately
- the score is visible without hunting
- the player receives a memorable themed reaction line
- detailed stats are available without overwhelming the first impression
- the player understands their strongest and weakest performance traits
- rewards/progression are clearly separated from performance scoring
- RETRY is fast and obvious
- CONTINUE is fast and obvious
- the screen feels like Headbang Heroes rather than a generic rhythm-game result table
