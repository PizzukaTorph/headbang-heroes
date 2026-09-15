# Roadmap

## P0 — Prototype: Make The Headbang Fun

Goal: validate the core interaction.

Deliverables:
- Unity mobile project baseline
- one original/test song
- deterministic audio clock
- audio + MIDI/event chart ingestion
- closing-circle cue
- basic directional/tap input
- custom head momentum/inertia model
- timing judgment windows
- first Motion Quality metric
- first Technique validation
- score + combo
- HYPE/crowd feedback stub
- calibration/debug overlay
- placeholder avatar
- basic hit/miss feedback
- Android/iOS device test

Exit gate: a short song is fun enough to replay for score without metagame.

## P1 — Vertical Slice

Goal: demonstrate the complete product fantasy.

- 3–5 tracks spanning different tempos/styles
- 2–3 headbang techniques
- improved head/hair animation
- first polished cartoon avatar
- cosmetic customization sample
- 2–3 static genre venues
- results/rank screen
- basic THE PIT song select
- tutorial
- local XP/level progression stub
- local HH economy stub
- first named combo chain
- production-quality UX direction

Exit gate: external testers understand the game, perceive skill growth, and ask to replay/try another song.

## P2 — MVP

- 6 initial genre archetypes
- at least 1 representative track per genre
- Road to the Pit first 6-world campaign framework
- 6 core headbang techniques
- modular avatar customization
- apparel/cosmetic unlock economy
- persistent profile
- XP + levels
- HH soft currency
- shop foundation
- THE PIT catalog mode
- multiple chart difficulties
- leaderboard backend
- global/friends/weekly per-song boards
- asynchronous friend/random challenges
- chart versioning
- licensing ledger
- accessibility and latency calibration
- initial backend hosted on M0THER
- cross-platform account model foundation

## P3 — v1

- complete/polished Road to the Pit arc
- 6 worlds × 6 levels structure
- 6 world bosses
- THE NECK final boss encounter
- richer technique/combo vocabulary
- large and expandable song catalog
- meaningful genre differentiation
- expanded venues/backgrounds
- achievements/challenges
- live operations/content pipeline
- store/platform compliance
- privacy-conscious analytics
- monetization hooks ready for controlled rollout

## Content scale strategy

The long-term goal is a **large catalog**, potentially hundreds of songs.

Architecture rules:
- Song, Chart, Artist, Genre and License are independent entities
- Campaign references catalog content; it does not own it
- New songs can ship without new narrative content
- Multiple charts/difficulties can exist per song
- Catalog browsing lives in THE PIT
- Content ingestion/licensing/chart authoring must scale without touching core gameplay code

## Online roadmap

### Phase A — asynchronous
- friend challenge
- random challenge
- song/chart/version locked comparisons
- result history

### Phase B — realtime Headbang Battle
- synchronized local song playback
- compact live score/combo/HYPE state
- no audio streaming
- no frame-by-frame physics sync
- crowd allegiance / CROWD CHOICE presentation

Realtime should be added only after the solo/asynchronous loop proves retention.

## Backend roadmap

Prototype: none required.

MVP:
- account/profile
- XP/level
- HH wallet
- inventory
- score submission
- leaderboards
- challenge service

Initial hosting target: M0THER.

Scale later by separating API, database, realtime coordination and other services if actual load requires it.

Server authority is mandatory for economy and competitive state.

## Monetization roadmap

Planned model: freemium/free-to-play.

Possible future monetization:
- apparel/cosmetics
- gestures
- cosmetic packs
- song/band packs where licensing supports it
- rewarded ads
- optional remove-ads/support purchase

Rules:
- never interrupt songs with ads
- no pay-to-win gameplay stats
- exact ad/IAP placement deferred until retention is understood
- avoid a second premium currency unless there is a demonstrated need

## Post-launch candidates

- seasonal events
- community competitions
- band spotlights
- artist submission/audition programs
- optional future Underground Platform integrations
- official guest musicians
- advanced cosmetics and gestures
- new campaign worlds/bosses
- new techniques
- new genre packs

## Explicitly not now

- realtime multiplayer in prototype
- full 3D environments
- procedural chart generation as a dependency
- AI beat detection as a dependency
- complex backend before online features
- large inventory/store implementation before core fun is validated
- famous-musician likenesses without agreements
