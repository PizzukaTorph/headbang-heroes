# v0.0.2 — MISS Investigation (diagnostic pass)

> Status: INSTRUMENTATION ONLY. No timing windows, scoring, judgment thresholds, neck physics,
> motion-quality, HYPE, THE BANG, Finisher math, chart event times, or difficulty were changed.
> Goal: make the cause of the high MISS count measurable before any fix.

## Findings (suspicious assumptions in the existing path)

Traced: physical input → Input System → screen coord → `HeadbangInput.ResolveZone` → DSP time
(`clock.DspNow`) → `AudioClock.ToSongTime` → `CandidateResolver` → `EventMatcher` → tier →
consume/expire → displayed judgment. Three concrete suspects:

1. **Portrait-skewed direction wedges (WHERE).** `ResolveZone` normalizes x and y *independently*
   by half-width/half-height, then compares `|nx| > |ny|`. On a tall portrait screen (1080×1920)
   the boundary is the 45° diagonal in *normalized* space, which in *pixels* is skewed toward
   Left/Right. A tap 200 px right **and** 200 px up of centre resolves **RIGHT** (nx 0.37 > ny 0.21),
   even though it is equidistant in pixels. Consequence: taps intended as Up/Down near the sides
   can resolve as Left/Right → **WRONG_DIRECTION** misses. (Documented in tests, not changed.)

2. **~1-frame input-timestamp quantization (WHEN).** Input is timestamped with `clock.DspNow` at
   `wasPressedThisFrame` (~16 ms at 60 fps). See "Input timestamp investigation" below — the Input
   System event timestamp is not safely mappable to `AudioSettings.dspTime` in-editor, so DSP-now is
   retained (authoritative) and the raw value is recorded for diagnostics only.

3. **SectorPulseCue hit-plateau, not a peak (perception).** In `Apply()`, while
   `|remaining| ≤ hitWindow` (hitWindow is passed the **GoodWindow, ±190 ms**), the pill is forced
   to `peakScale` + `hitColor` for the **entire ±190 ms window**. The strongest visual moment is a
   ~380 ms plateau, **not an instantaneous peak at eventSongTime**. `DiagTimingProfile()` confirms
   `inHit=Y` across −190…+190 ms with constant peak scale. This can make players commit anywhere in
   the plateau and read the cue as "late/early" even when timing is fine.

Also noted (not defects, but relevant): the `(no consume)` branch previously collapsed TooEarly and
NoActiveCandidate together; expired events are a separate MISS source with no input at all.

## Instrumentation added (what we can now measure)

- **`PlaytestDiagnostics`** (pure, engine-free): per-bang `BangRecord` + a **failure taxonomy**
  derived OUTSIDE the scoring path — `HIT / WRONG_DIRECTION / TOO_EARLY / TOO_LATE_OR_EXPIRED /
  NO_ACTIVE_CANDIDATE / EVENT_EXPIRED_WITHOUT_INPUT`. Records screen pos, normalized pos, resolved
  vs expected direction, DSP + song time, signed/abs timing error, judgment, source.
- **Session report** (`BuildReport`, printed at run end and on the **P** key): input/consumed
  counts, judgment breakdown, failure breakdown, **direction-confusion matrix**, signed/abs timing
  stats (median/mean/early/late), and clock offsets (output-latency, calibration, effective).
  Answers in one run: **are we missing because of WHERE or WHEN?**
- **Input source tagging** (Keyboard/Mouse/Touch) on every record → keyboard-vs-touch A/B.
- **`ZoneDebugOverlay`** (**Z** key, dev-gated, hidden by default): tints a screen grid by sampling
  `HeadbangInput.ResolveZone` itself, plus a live pointer and the resolved direction. It cannot
  disagree with gameplay because it calls the real resolver.
- **AudioClock diagnostics**: `DiagDspBufferLength / DiagDspBufferCount / DiagOutputSampleRate /
  OutputLatency / Calibration / DiagEffectiveOffset` (P-key dump).
- **SectorPulseCue diagnostics**: `DiagSampleAt` + `DiagTimingProfile` (P-key) to prove the plateau.
- **BangInput** carries diagnostic-only `SourceCode` + `ScreenPosition` (NaN sentinel), alongside the
  existing `RawDeviceTime`. Never read by matching/judgment.

## Input timestamp investigation (task 6)

- Current: `clock.DspNow` (`AudioSettings.dspTime`) captured at `wasPressedThisFrame`, converted via
  `ToSongTime`. Domain = DSP, authoritative.
- Unity's Input System exposes control/event timestamps in `Time.realtimeSinceStartup`-ish domain
  (`InputState.currentTime` / event `time`), **not** the DSP clock. There is no supported, stable
  mapping from that domain to `AudioSettings.dspTime`, and Editor vs device differ. Blindly using it
  could introduce a variable skew.
- Decision: **retain DSP-now** (authoritative), record `RawDeviceTime` for diagnostics. If we later
  want sub-frame precision, that is a deliberate device-validation experiment, not this pass.

## Audio latency compensation audit (task 7)

- `RefreshOutputLatency()` computes `bufferLength * numBuffers / sampleRate`. This represents the
  **DSP mixer buffering only**; it does NOT include OS/driver/Bluetooth/hardware output latency, so
  it is a lower bound on true output latency. Documented.
- Applied once as `SongTimeMap.OutputLatency`, added to song-time in the SAME place as calibration
  (`SongTimeAt`), so it shifts both the live clock and input conversion consistently (no double
  application). Sign: positive → treats input as later → compensates the player tapping "early"
  because sound arrives late. Pause/resume/restart: `SongTimeMapTests` (incl. the added
  `ResumeWithLatency_DoesNotDoubleCountOrJump`) confirm latency + calibration are not double-counted
  across pause/resume. No sign/double-apply bug found.

## Existing behavior changed

**None.** All additions are diagnostic/dev-gated. The authoritative input→judgment path is
byte-for-byte the same; diagnostics are appended after the outcome is computed and are compiled
inert outside `UNITY_EDITOR || DEVELOPMENT_BUILD` (and off when `enableDiagnostics=false`).

## Tests

- **Added** `PlaytestDiagnosticsTests` (6): empty session, all-hit, wrong-direction-heavy +
  confusion matrix, signed timing stats (median/mean/early/late), expired-without-input, report
  contains clock offsets.
- **Expanded** `HeadbangInputTests` (ResolveZone): exact centre, near-centre, **portrait skew**,
  either side of the normalized diagonal, landscape/square aspect ratios, lower-thumb positions,
  corners — documenting the wedge model as-is (not redesigning).
- Independent headless run of the two touched-file suites: **19/19 passed** (compiled against real
  Unity DLLs via NUnitLite). The heavier suites were not run headlessly but share no
  signature-breaking changes.

## Playtest procedure (next test)

1. Rebuild the prototype (Tools → Headbang Heroes → Build M0 Prototype, or the WISH variant).
2. Ensure `enableDiagnostics` is on (default in Editor).
3. Play ~30 events using the **keyboard** (A/D/W/S). Press **P** to print the summary; copy it.
4. Restart (R), play ~30 events using **mouse/touch** (tap the sectors). Press **P**; copy it.
5. Optionally press **Z** to see the real ResolveZone partition; press **P** to dump the cue timing
   profile + clock latency components.
6. Compare the two summaries: timing bias (median signed error) and the direction-confusion matrix.

## Decision rules (how to read the result)

- Many **WRONG_DIRECTION** (esp. Up/Down → Left/Right) → the touch geometry/portrait skew is the
  culprit → next fix targets `ResolveZone` wedges (pixel-space, not normalized).
- Consistent **positive/negative median signed error** → calibration/latency problem → adjust the
  offset (or improve latency estimate), not the windows.
- **Keyboard good, touch bad** on the same chart → the problem is touch mapping/input path, not
  timing.
- **Both keyboard and touch similarly shifted** → clock/audio/chart/cue alignment (e.g. MP3↔MIDI
  offset) → fix alignment, not windows.
- Signed errors **near zero** but player still feels the cue late/early → visual-cue perception →
  address the SectorPulseCue plateau (make the peak instantaneous at t=0).
- Many **EVENT_EXPIRED_WITHOUT_INPUT** → cue readability / chart density / input capture → not a
  timing-window problem.

The next commit should be a single, deliberate fix aimed by this evidence — not a window widen.
