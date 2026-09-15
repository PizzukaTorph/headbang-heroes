# Online Design v0.1

## Principle

Online features support replayability and competition. They must not dictate the prototype architecture beyond stable IDs, deterministic scoring and chart versioning.

## Leaderboards

Primary board: per song + chart/difficulty/version.

Views:
- Global
- Friends
- Weekly

Aggregates later:
- Genre
- Overall

A score record should eventually contain enough information to validate compatibility, including song/chart version and scoring rules version.

## Asynchronous challenge

Preferred first multiplayer mode.

Flow:
1. Player A completes a run.
2. A challenge references the exact song/chart/version and A's result.
3. Player B accepts and plays the same chart.
4. Server compares eligible results.
5. Both receive outcome/history.

Matchmaking variants:
- direct friend challenge
- random opponent near skill/rank
- daily/weekly challenge

This avoids realtime gameplay synchronization while providing social competition.

## Anti-cheat

Do not overbuild initially, but assume public leaderboards will be attacked.

Later candidates:
- server-issued run/session IDs
- sanity validation
- signed/hashed run summaries
- impossible-input/timing detection
- replay/event traces for top scores
- server-side score recomputation for competitive tiers

## Realtime battle — future

Potential presentation: two avatars performing the same chart concurrently with live score/combo comparison.

Prefer synchronizing compact competitive state rather than deterministic physics frames.

Questions to validate before building:
- does live play materially improve retention?
- acceptable latency model
- pause/disconnect behavior
- matchmaking population
- song ownership/licensing constraints
- backend/ops cost

## Privacy

Collect the minimum data needed. Avoid requiring accounts during prototype/vertical slice. Design identity/friends only when the online product is ready.
