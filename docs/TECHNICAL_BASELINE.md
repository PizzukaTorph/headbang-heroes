# Technical Baseline — Godot Branch

## Authority

Read FOUNDATION.md, TECHNICAL_CONTRACTS_V1.md, and GODOT_PORT.md first.

This document defines the practical baseline for develop-godot.

## Engine

Godot 4.7.2 stable line.

- GDScript runtime
- no .NET requirement
- no third-party addons in the POC baseline
- use stable engine releases only

## Platforms

Product targets:
- Android
- iOS

Desktop is a development/validation target, not the product target.

## Presentation

- 2D
- portrait-first
- touch-first
- safe-area work required before production mobile release
- responsive phone aspect ratios
- 60 FPS presentation baseline
- FLP authored frame animation

Rendering can vary independently from gameplay simulation timing.

## Audio/rhythm timing

SongClock owns authoritative song time.

Canonical Godot formula:

~~~text
AudioStreamPlayer.get_playback_position()
+ AudioServer.get_time_since_last_mix()
- AudioServer.get_output_latency()
+ calibration
~~~

Clamp the exposed song clock monotonically so mix-thread jitter cannot move gameplay backwards.

Never base authoritative scoring on:
- process elapsed time
- animation callbacks
- Tween completion
- frame count
- visual cue completion

## Input

Physical touch/keyboard input maps to semantic intent before judgment.

Touch coordinates are never chart semantics. Domain matching receives a semantic direction plus authoritative song time.

## Neck simulation

The port keeps a deterministic fixed-step target-driven stroke simulation at
120 Hz baseline. Each Classic tap starts a new interruptible stroke from the
current pose; the render loop never owns the movement.

- render delta only causes the application layer to ask how much authoritative song-time elapsed;
- state advances in fixed ticks;
- Motion Quality uses evidence since the previous inversion boundary;
- first action from neutral remains setup/unprepared;
- MISS never resets physical state.

## Content pipeline

~~~text
audio + metadata + chart
→ validate/compile
→ runtime chart
→ gameplay
~~~

MIDI remains authoring/import material, not a required runtime parser.

Remote content comes later behind a content-provider boundary.

## Runtime layout

~~~text
src/domain/           engine-light gameplay rules/state
src/application/      run orchestration
src/infrastructure/   Godot/platform adapters (audio now)
src/presentation/     cues, HUD, FLP Erik
src/app/              shell / flow
game/assets/          runtime content
tests/                headless smoke/domain validation
~~~

## POC tuning

Current feel-sensitive values are centralized in `game/config/poc_tuning.json`.

This is prototype tuning data, not a new source of gameplay semantics. It may change numeric timing windows, neck coefficients and presentation parameters, but it must not bypass the ownership rules in FOUNDATION/TECHNICAL_CONTRACTS.

Debug telemetry may observe authoritative state but cannot feed back into judgment or scoring.

## Performance

- avoid per-frame allocations in hot paths where profiling shows impact;
- do not make rendering authoritative;
- validate audio/touch latency on real devices;
- FLP texture memory must be measured before multiplying characters/animations;
- prefer atlas/content optimization after the production asset contract stabilizes.

## Testing

Pure/domain smoke suite:

~~~bash
godot --headless --path . --script res://tests/run_tests.gd
~~~

Device validation remains mandatory for:
- touch feel
- audio latency
- calibration
- interruption/pause behavior
- iOS/Android export
- memory/performance

## Dependency policy

Stay vanilla Godot until a concrete requirement proves otherwise.

Do not add generic rhythm frameworks, DI frameworks, animation packages, networking SDKs, or analytics/ads/IAP plugins until that requirement enters current scope.

## POC exit gate

One complete committed fixture proves:
- synchronized audio/chart
- readable cue
- semantic directional input
- always-responsive deterministic neck state
- Timing + Motion Quality separation
- score/combo/multiplier
- HYPE/THE BANG code path
- FLP Erik feedback
- Results + Retry
- calibration control

Production readiness additionally requires real-device validation and a legal production song/content package.
