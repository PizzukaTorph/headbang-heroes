# Headbang Heroes — Agent Rules (Godot branch)

## Read first

Before changing gameplay architecture or design, read:

1. docs/FOUNDATION.md
2. the relevant canonical subsystem specification
3. docs/GODOT_PORT.md
4. docs/GDD.md

The Godot branch preserves product/domain contracts while replacing Unity-specific adapters.

## Current runtime

- Engine: Godot 4.7.2
- Language: typed GDScript where it materially clarifies contracts
- Project root: repository root (project.godot)
- Active code: src/
- Runtime assets: game/assets/
- Entry scene: scenes/main.tscn
- Legacy Unity folders are reference-only and ignored by Godot with .gdignore.

## Non-negotiable gameplay contracts

- SongClock owns authoritative song time.
- Use the Godot audio hardware clock formula, never render delta/frame count, for judgment.
- Input always changes NeckMotionState, even early/late/wrong/no-event.
- MISS never snaps or resets neck state.
- Gameplay neck integration is fixed-step (120 Hz baseline), independent from render FPS.
- Timing Quality and Motion Quality remain separate.
- FLP/Erik animation is downstream presentation and never determines judgment/scoring.
- CandidateResolver owns per-run event consumption/expiry; authored chart data remains immutable.
- Wrong semantic input inside the eligible window consumes the best candidate as MISS.
- Results consume authoritative run state and never recalculate it.
- No generic God object. Keep domain, application, infrastructure and presentation boundaries explicit.

## Dependency policy

Do not add addons merely for convenience. The POC intentionally has zero external Godot dependencies.

Any new addon/native extension must solve a measured problem and be documented with why it is needed, supported mobile platforms, license, update strategy, and fallback behavior.

## Verification

For pure gameplay changes run:

~~~bash
godot --headless --path . --script res://tests/run_tests.gd
~~~

For timing/presentation/mobile changes also validate on a real device. Desktop correctness does not establish Android/iOS touch/audio latency.

## Asset/licensing rule

Never commit commercial/licensed song audio unless redistribution rights explicitly allow it. Local-only song files belong in ignored paths. The committed TempoRamp fixture exists specifically so the repository remains clone-and-run.
