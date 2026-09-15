# Headbang Heroes — Agent Rules

## Product principle

Headbang Heroes is a rhythm game about **headbanging**, not a generic note-tapping game with a metal skin.

Every implementation decision must protect the core fantasy: the player controls the timing, direction, momentum and technique of the avatar's head.

## Current priority

The only P0 goal is **Make The Headbang Fun**.

Do not add accounts, monetization, elaborate progression, realtime networking, production backend infrastructure, procedural content systems or large art pipelines before the prototype validates the core interaction.

## Engineering principles

- Prefer small, testable systems over large frameworks.
- Keep song/chart data deterministic and data-driven.
- Audio timing is authoritative; rendering must not be the timing clock.
- Separate chart events from visual presentation.
- Separate scoring from input presentation.
- Keep gameplay framerate-independent.
- Mobile performance and touch latency matter from day one.
- Build calibration/debug tooling early.
- Avoid third-party dependencies unless they materially reduce risk.
- Never commit licensed audio, fonts, art or other third-party assets without recording provenance and license.
- No copyrighted commercial music may be added merely for development convenience if the repository is public.

## Suggested Unity boundaries

When the Unity project is introduced, keep clear modules for:

- AudioClock
- Song/Chart
- Input
- HeadPhysics
- Judgment
- Combo/Score
- Feedback
- Avatar
- Progression
- Online

Core rhythm logic should be testable without production visuals.

## Definition of done for prototype

A new player can launch a test song, understand the closing-circle cue without explanation, headbang in rhythm, intentionally improve timing, perceive momentum, build/break a combo, receive reliable judgments, and want to retry for a better score.

If that is not true, do not solve the problem by adding metagame.
