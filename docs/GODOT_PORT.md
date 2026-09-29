# Godot Port — develop-godot

Status: first complete native POC port
Source branch: develop-flp at 0b6fb3a79290c6029ecd25d1047c066a6ec1f37f
Engine target: Godot 4.7.2
Language: GDScript

## Goal

Make the FLP direction a smaller, mobile-first Godot project without throwing away the product/domain work already proven in Unity.

This is a port of contracts and behavior, not a line-by-line C# translation.

## What is carried over

### Content

- Erik FLP 16-frame headbang sequence
- TempoRamp WAV validation fixture
- TempoRamp chart
- Beyond the Pain chart fixture
- Beyond the Pain MIDI authoring fixture
- all existing canonical project/design documentation

### Domain behavior

- authoritative audio-time judgment
- PERFECT / GREAT / GOOD / WELL / MISS windows
- bounded unresolved candidate matching
- wrong-direction consumed MISS
- event expiry
- fixed 120 Hz spring/damper neck simulation
- first-bang setup semantics
- event-local Motion Quality evidence
- combo/multiplier
- score
- HYPE
- THE BANG
- Finisher-ready scoring path
- authoritative result assembly

### Product loop in this port

~~~text
Home
→ Play Tempo Ramp
→ gameplay/audio/chart
→ FLP Erik feedback
→ Results
→ Retry / Home
~~~

The committed chart has no Finisher candidates, so the Finisher code path exists but the included TempoRamp fixture does not force a Finisher demonstration.

## Unity → Godot mapping

| Unity source concept | Godot port |
|---|---|
| AudioClock.cs / DSP time | src/infrastructure/song_clock.gd |
| ChartCompiler.cs | src/domain/chart_compiler.gd |
| CandidateResolver.cs + EventMatcher.cs | src/domain/candidate_resolver.gd |
| TimingConfig.cs | src/domain/timing_config.gd |
| NeckMotionState.cs | src/domain/neck_motion_state.gd |
| MotionQualityEvaluator.cs | src/domain/motion_quality.gd |
| ScoringConfig.cs | src/domain/scoring_config.gd |
| HypeSystem.cs | src/domain/hype_system.gd |
| RunScorer.cs | src/domain/run_scorer.gd |
| PrototypeController.cs orchestration | src/application/gameplay_run.gd |
| Unity Canvas/HUD | src/presentation/gameplay_view.gd |
| FLP frame playback | src/presentation/erik_view.gd |
| closing-circle cue | src/presentation/cue_ring.gd |
| scene/product shell | src/app/main.gd + scenes/main.tscn |

## Authoritative clock

The Godot port follows the engine's documented rhythm-game timing approach:

~~~text
song_time =
  AudioStreamPlayer.get_playback_position()
  + AudioServer.get_time_since_last_mix()
  - AudioServer.get_output_latency()
  + calibration
~~~

Song time is prevented from moving backwards. Rendering and animation never become timing authority.

Input is sampled into the same song-time domain at the instant the semantic bang is resolved.

## FLP contract

The 16 Erik PNGs are imported as independent textures. The sequence is now physically driven rather than timer-driven.

Presentation flow:

```text
NeckMotionState
    ↓
actual angular travel
    ↓
FLPFrameDriver
    ↓
authored frame phase
    ↓
ErikView
```

The frame driver advances from real neck travel and is capped at 9 visual frames/second so high physical velocity cannot make the art unreadably fast. Near physical extrema, angular travel naturally falls and the displayed pose dwells longer. Once the neck is physically settled near neutral, presentation returns to frame 0.

Important:

- FLP frames observe neck state; they never mutate it;
- they do not drive event completion;
- they do not drive score;
- timing/judgment never use FLP phase;
- future Idle/Horns sequences can be added without modifying the domain.

## Cue readability profile

The current POC uses one shared central cue location for all four semantic directions. CURRENT is the strong closing ring; NEXT is a faint larger concentric preview. Direction remains explicit through arrow/color and the four input buttons stay distinct.

This intentionally creates a low-search-cost baseline. A future difficulty profile may move/spatialize cue anchors without changing chart semantics or neck physics.

## Included content and local audio

The Unity source branch intentionally ignores licensed/local audio, including Beyond the Pain. Therefore this Godot branch does not fabricate or redistribute that file.

The clone-and-run POC uses:

- game/assets/audio/tempo_ramp.wav
- game/assets/charts/lab-002-tempo-ramp.json

Authoring/reference fixtures also include:

- game/assets/charts/lab-001-beyond-the-pain-classic-m0.json
- game/assets/midi/06 - Beyond the Pain.mid

A later content manifest can bind a local/remote legal audio asset to that chart without changing gameplay code.

## Validation gates

Before calling the engine migration production-ready:

1. Import branch in Godot 4.7.2 and run the included test script.
2. Play full TempoRamp on macOS.
3. Export/run on at least one Android device.
4. Export/run on iPhone.
5. Record timing error distribution with wired/speaker audio.
6. Repeat with Bluetooth and store a separate calibration.
7. Verify pause/resume and focus interruption behavior on mobile.
8. Profile texture memory for FLP animation growth.
9. Add an authored fixture with HYPE → THE BANG → Finisher so that path is demonstrated end-to-end.
10. Add final mobile export presets only once signing/package identifiers are decided.

## Why legacy Unity files remain in this branch

They are migration evidence and regression reference. Removing them immediately would make it harder to compare behavior and recover details that are not yet encoded in canonical docs.

Godot ignores Assets/, Packages/, and ProjectSettings/ through .gdignore. Once the Godot port is validated on real devices, a later cleanup commit can remove the legacy engine tree.

## Explicitly not ported yet

These are outside the current FLP POC or require device/product decisions:

- final avatar customization
- Idle/Horns art sequences not present in the remote develop-flp commit
- remote content/CDN
- account/backend
- ads/IAP
- leaderboard
- production save/profile UI
- platform signing/export presets

The architecture keeps those concerns outside the gameplay domain so they can be added without rewriting the rhythm core.


## Tuning and diagnostics

POC feel values are intentionally centralized in `game/config/poc_tuning.json` rather than scattered through GDScript constants. The current file covers timing, neck physics, FLP presentation and cue presentation.

A debug run supports:
- `F3` — toggle tuning telemetry;
- `F4` — reload the JSON tuning file without restarting the run.

Timing telemetry is observational only. It records consumed player inputs and exposes:
- signed mean error (calibration bias);
- mean absolute error;
- early/late counts;
- last judgment/error.

Expired events are not inserted as synthetic zero-error samples, and the diagnostics never participate in judgment, score or progression.
