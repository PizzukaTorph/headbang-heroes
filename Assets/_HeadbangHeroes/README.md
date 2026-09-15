# Headbang Heroes — M0 prototype

Milestone: **M0 — One Head, One Song, One Circle**.

Core loop under test:

```
AudioClock (DSP) -> chart event -> closing circle -> L/R input
    -> signed timing error -> judgment -> head motion (inertia) -> motion quality
    -> EventPerformance (Timing x Motion) -> combo/score -> feedback
```

Audio timing is authoritative: the clock is derived from `AudioSettings.dspTime` via
`AudioClock`, never from frame count, animation duration or coroutine waits. Rendering
(the closing ring, the head) only interpolates against that clock.

## What M0 implements

- **URP orthographic Main Camera** created by the builder (fixes `No Cameras Rendering`).
- **Idempotent builder**: `Tools > Headbang Heroes > Build M0 Prototype` regenerates the
  scene from scratch, (re)creates/updates the `SongDefinition`, assigns the chart JSON,
  wires every serialized field, registers the scene in Build Settings, and logs
  `HH M0 prototype ready.` on success (or a readable warning if the audio is missing).
- **Closing-circle cue** driven every frame by `songTime` and the absolute event time:
  `progress = clamp01(1 - (eventTime - songTime) / approachTime)`.
- **Head motion model** (no `Rigidbody2D`): deterministic spring-damper with angle,
  angular velocity, peak-amplitude tracking, inversion detection and a normalized
  `0..1` motion-quality estimate that rewards moving coherently with the requested direction.
- **Judgment pipeline** with `TimingQuality` per tier and
  `EventPerformance = TimingQuality x MotionQuality`; score is driven by performance.
- **Debug HUD** in the bottom-left corner: song time, next event time, last timing error (ms),
  judgment, motion, performance, combo, score, head angle, angular velocity, peak, calibration
  offset. Toggle with **F1**.
- **Chart/input correctness**: left screen half = Left, right half = Right (mouse in the
  Editor replicates touch); wrong direction = Miss; an early tap outside the window does
  **not** consume the note; a late tap does not block later events; expired events auto-miss.
- **Playtest controls** and a calibration offset applied to the judgment time only.
- **EditMode unit tests** for the judgment windows, the scheduler decision logic and the
  pure head-motion math.

## Where to put the audio

Place the supplied track here (create the `LocalAudio` folder if needed):

```
Assets/_HeadbangHeroes/Content/Lab/LocalAudio/BeyondThePain.mp3
```

The audio file is intentionally **not** committed (see licensing note below). The prototype
starts the M0 segment at song time ~22 s.

## How to build & run the prototype

1. `git pull` the `main` branch.
2. Open the project in **Unity 6.6 (6000.6.0f1)**.
3. Copy `BeyondThePain.mp3` to the path above.
4. Run **`Tools > Headbang Heroes > Build M0 Prototype`**. Wait for the
   `HH M0 prototype ready.` log line.
5. Open `Assets/_HeadbangHeroes/Scenes/Prototype_Headbang.unity`.
6. Press **Play**. You should see a rendered portrait scene (not `No Cameras Rendering`),
   hear Beyond the Pain from the M0 segment, see the closing ring converge on the head,
   and be able to tap/click Left/Right to judge, move the head with inertia, and watch the
   HUD update judgment, timing error, motion, performance, combo and score.

## Editor / playtest controls

| Key            | Action                                             |
| -------------- | -------------------------------------------------- |
| Left click     | Bang Left (left screen half) / Right (right half)  |
| Touch          | Same left/right split as mouse                     |
| `R`            | Restart the prototype (re-arms clock, head, cue)   |
| `Space`        | Pause / resume (audio + clock freeze together)     |
| `[`            | Calibration offset −5 ms (judgment time only)      |
| `]`            | Calibration offset +5 ms (judgment time only)      |
| `F1`           | Toggle the debug HUD                                |

The calibration offset is applied to the judged input time; it never edits the audio file.
The offset handling lives in `PrototypeController`/`ChartScheduler` so a future calibration
screen can reuse it.

## Running the tests

Open **Window > General > Test Runner > EditMode** and run all tests. They cover:

- `JudgmentSystemTests`: window boundaries (±35 / ±70 / ±120 ms and just outside), timing
  quality tiers, `Performance = Timing x Motion`, and `ResolveInput` (early → keep,
  valid → consume, late-in-window → Good, wrong direction → Miss).
- `HeadMotionStateTests`: motion quality range/coherence, still vs moving, peak tracking,
  determinism, spring settling and reset.

## Assembly layout

- `HeadbangHeroes.Runtime` (`Scripts/`) — gameplay runtime code.
- `HeadbangHeroes.Editor` (`Editor/`) — the M0 builder (Editor-only, references URP runtime).
- `HeadbangHeroes.Tests.EditMode` (`Tests/EditMode/`) — EditMode unit tests.

## Known limits / TODO (M0)

- Motion quality is sampled from the head's momentum **before** the tap's impulse, so the
  very first hit of a run scores low until the head is actually moving. This is intentional
  for M0 (it rewards real headbanging over cold taps) but is a tuning candidate.
- Placeholder visuals only (dark background, bust, knob-sprite head, procedural rings).
  No real art, shaders or animation.
- `MotionQuality` is a coarse heuristic; the full `Timing x Motion x Technique` model is
  out of scope for M0.
- Pause/resume re-anchors the DSP clock on resume; extremely long pauses have not been
  stress-tested.
- No account system, store, campaign, online, Addressables, DOTween, FMOD/Wwise or
  rhythm-game frameworks — deliberately out of scope for M0.

## Licensing

Do not commit third-party or production audio, fonts or art without recorded provenance and
license. `BeyondThePain.mp3` is kept out of the repository on purpose.
