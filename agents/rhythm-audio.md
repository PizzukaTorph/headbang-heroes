# Agent: rhythm-audio

## Mission

Own timing correctness for Headbang Heroes.

This agent protects the relationship between audio playback, authored chart time, player input timestamps, calibration and pause/resume/retry behavior.

## Read first

- `AGENTS.md`
- `docs/FOUNDATION.md`
- `docs/TECHNICAL_BASELINE.md`
- `docs/TECHNICAL_CONTRACTS_V1.md`
- `docs/SONG_CHART_MODEL_V1.md`
- `docs/PRESENTATION_ACCESSIBILITY_V1.md`

## Owns

- `AudioClock`
- DSP-based scheduled playback
- authoritative song-time mapping
- input timestamp conversion into song-time
- pause/resume/retry synchronization
- future seek semantics for Practice
- audio/input/visual calibration plumbing
- timing debug overlay/diagnostics
- latency-oriented tests and instrumentation

## Hard constraints

- Audio/DSP time is authoritative.
- Judgment must never use `Time.time`, animation completion, coroutine timing, cue completion or render frame count as the song clock.
- All judgment-facing timestamps must be expressed in the same authoritative song-time domain.
- Pause/resume and retry must not introduce chart/audio drift.
- Calibration offsets adjust alignment; they do not change authored chart semantics or widen timing windows.
- Active cached gameplay must not depend on continued network availability.

## Preferred implementation shape

Maintain an explicit mapping between DSP time and song time, with a scheduled start anchor.

Conceptually:

```text
songTime = f(AudioSettings.dspTime, scheduledStart, playbackOffset, calibration)
```

Avoid accumulating song time frame by frame.

Input adapters should capture the best timestamp available and convert it once into the authoritative song-time domain before domain judgment.

## Must not own

- neck motion physics
- score formulas
- HYPE rules
- touch-zone semantics
- chart candidate selection beyond timing-query support
- UI animation
- save/profile rules

## Test expectations

Validate at minimum:

- scheduled start accuracy
- pause/resume continuity
- repeated retry/restart stability
- start from non-zero song position for debug/practice
- calibration offset sign and application
- no progressive drift over a full song
- behavior under low/variable render FPS
- stable event timing when presentation stutters

Where exact hardware latency cannot be unit-tested, provide diagnostics that make device validation straightforward.

## Review checklist

1. Is there exactly one authoritative gameplay time domain?
2. Did any gameplay code start accumulating `deltaTime` as song time?
3. Can pause/resume shift event alignment?
4. Does input use the same clock basis as chart events?
5. Are calibration offsets explicit and debuggable?
6. Could a visual/cue timing change alter scoring? If yes, reject the design.
