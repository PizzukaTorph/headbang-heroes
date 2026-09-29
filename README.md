# Headbang Heroes — Godot

**A mobile rhythm game where you do not play the music — you play the neck.**

develop-godot is the native Godot port of the working FLP POC. The branch deliberately preserves the original Unity folders as migration/reference material, but Godot ignores them through .gdignore. The active project starts at the repository root with project.godot.

## Run it

Requirements:

- Godot 4.7.2
- no addons or external packages

Steps:

1. Clone the repository and checkout develop-godot.
2. Import the repository root in Godot Project Manager (project.godot).
3. Let Godot import the PNG/WAV assets.
4. Press Play Project.
5. Press PLAY TEMPO RAMP.

The repository contains a redistributable validation fixture (TempoRamp.wav) so the POC works immediately after clone. Beyond the Pain chart/MIDI fixtures are also retained, but the song audio is intentionally local-only/licensed and is not committed.

## Controls

Mobile/touch:
- LEFT / UP / DOWN / RIGHT buttons
- THE BANG button when HYPE is ready
- calibration +/- controls

Desktop:
- arrows or WASD = semantic bang directions
- B = THE BANG
- [ / ] = calibration -/+ 5 ms
- P = pause/resume

## Runtime architecture

~~~text
AudioStreamPlayer
      ↓
SongClock (authoritative audio time)
      ↓
Runtime chart + CandidateResolver
      ↓
NeckMotionState (120 Hz fixed simulation)
      ↓
Timing + MotionQuality
      ↓
RunScorer + HypeSystem
      ↓
GameplayView / Erik FLP animation
~~~

Hard rules retained from the canonical docs:

- audio time owns judgment timing;
- input always changes neck motion, even with no matching cue;
- a MISS never snaps/reset the neck;
- Timing Quality and Motion Quality are independent;
- FLP animation is presentation only;
- chart authoring data is compiled before the gameplay hot path;
- wrong-direction input inside an eligible window consumes that event as MISS;
- score/HYPE/results consume deterministic domain outcomes.

## Audio timing

Song time uses the Godot rhythm-game clock recommended by the engine documentation:

~~~text
AudioStreamPlayer.get_playback_position()
+ AudioServer.get_time_since_last_mix()
- AudioServer.get_output_latency()
+ user calibration
~~~

The value is monotonic-clamped to prevent mix jitter from moving gameplay time backwards.

## Tests

From a terminal with Godot available:

~~~bash
godot --headless --path . --script res://tests/run_tests.gd
~~~

The smoke/domain suite covers chart compilation, timing boundaries, candidate matching, deterministic neck evidence and scoring transitions.

## Important paths

~~~text
project.godot
scenes/
src/
  app/
  application/
  domain/
  infrastructure/
  presentation/
game/assets/
  audio/
  charts/
  characters/erik/headbang/
tests/
docs/
~~~

See docs/GODOT_PORT.md for the migration map and known validation gates.


## POC tuning lab

Feel-sensitive values are layered instead of duplicated:

~~~text
game/config/tuning/base.json
        +
game/config/tuning/profiles/easy.json
game/config/tuning/profiles/normal.json
game/config/tuning/profiles/hard.json
game/config/tuning/profiles/extreme.json
        +
user/device calibration in SongClock
~~~

The base file owns shared neck physics, FLP presentation, cue defaults and feedback feel. Difficulty/readability profiles override only the values that actually differ.

The POC Home screen can run the same TempoRamp fixture through EASY / NORMAL / HARD / EXTREME tuning profiles. This is a tuning harness, not automatic chart generation: production difficulty remains an authored chart property.

Current profile intent:

- EASY: wider timing windows, centered cue, longer preview;
- NORMAL: original Godot baseline;
- HARD: tighter timing, directional cue layout;
- EXTREME: tighter timing again, wider directional layout.

Neck physics is intentionally identical across all four profiles.

In a debug run:
- **F3** toggles the telemetry HUD;
- **F4** reloads the selected profile plus base tuning without restarting the run.

The debug HUD shows authoritative song time, calibration/output latency, tuning profile vs chart difficulty, CURRENT/NEXT event ids, neck angle/velocity, FLP frame/phase, and timing bias statistics.

Results include mean signed timing bias, mean absolute timing error, and early/late counts.
