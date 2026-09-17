# Errors / Issues To Be Fixed

Running list of known problems deferred so implementation can continue. Each entry: what,
evidence, impact, and the intended fix / owning package. Do not silently close an item — resolve
it or move it to a package plan.

---

## 1. M0 chart is a synthetic grid, not aligned to the song (ring/cue not in sync)

**Status:** OPEN — deferred (does not block core-loop implementation).

**What:** `Content/Lab/lab-001-beyond-the-pain-classic-m0.json` is a hand-made metronomic grid:
76 events spaced at a fixed **+0.80 s** (23.34, 24.14, 24.94, …). It is NOT derived from the
song. So with a real MP3 the closing ring / ticks do not fall on the actual hits — the mismatch
is not a constant offset (calibration can't fix it), the rhythm itself is unrelated to the music.

**Evidence:**
- JSON event times increase by exactly 0.80 s for all 76 events.
- The reference MIDI `Content/Lab/06 - Beyond the Pain.mid` is a valid MIDI (format 1, 5 tracks,
  480 tpqn, has FF51 tempo + FF58 time-sig) but its tracks are `Traccia 1/2/3/Percussioni` —
  it has **no `HH_*` semantic tracks** required by `docs/HH_MIDI_STANDARD_V1.md`.
- There is **no MIDI importer in the runtime**; gameplay reads only the JSON. The `.mid` is inert
  reference material today.

**Impact:** Playing with the real audio "works" (audio + loop + input + neck + judgment) but the
choreography is not musically synced. Fine for testing whether the headbang *feels* good; not fine
for a real performance chart.

**Intended fix (options):**
- Short term: author a real chart JSON whose event times land on the song's actual hits (derive
  beats from the MIDI tempo map or tap them out). Fast, unblocks a musically-synced playtest.
- Canonical: build the HH MIDI importer/validator (`agents/hh-midi-validator.md`,
  `docs/HH_MIDI_STANDARD_V1.md`) that compiles `HH_*` tracks → `ChartDefinition` → `RuntimeChart`.
  Requires the source to be re-authored as a proper `.hh.mid` (the current `.mid` lacks `HH_*`
  tracks), OR the importer maps the `Percussioni` track (kick/snare) to L/R events.

**Owning package:** Chart Content / HH MIDI (post P0A core; see `docs/implementation/`), not the
current runtime packages.

---

## 2. RuntimeChart uses positional (index/slot) event identity, not a durable authored id

**Status:** OPEN — deferred to Chart Content / RuntimeChart hardening.

**What:** `MotionCandidate.Id` / resolver slots are array indices. Stable within a run, but not a
durable authored id that survives chart edits / cross-version score attribution. Flagged by the
chart-content review of Package 03.

**Intended fix:** carry a stable authored event id (string/hash) through the compiler → RuntimeChart
→ matcher; keep the slot index only as an internal resolve slot.

---

## 3. Deferred review findings (non-blocking) from Packages 01–03

Tracked so they are not lost:
- **Neck / timing:** gameplay simulation is advanced from `Time.deltaTime`; migrate to
  AudioClock-derived song-time delta when GameplayRun lands (Package 02/04 integration).
- **Motion Quality:** `NeckMotionModel.ProvisionalMotionQuality` is M0 glue on the MonoBehaviour;
  replace with a pure `MotionQualityEvaluator` consuming the pre-inversion snapshot (Package 04).
- **RestEvaluator wiring:** `Begin/Sample/Complete` has no runtime consumer yet; must be driven
  from the authoritative fixed sim tick (integration package). Fail-open at 0 samples is currently
  unreachable via valid authored data but is not fail-closed.
- **Architecture:** single asmdef; consider a `noEngineReferences` domain asmdef to make the
  Domain/Presentation boundary a build-time invariant. Rename `ChartDefinition.cs` → `BangDirection.cs`.
  `TimingConfig.CandidateLead` naming vs the actual GoodWindow early edge.
- **Input timestamp fidelity:** input is timestamped at frame time (`clock.DspNow` at
  `wasPressedThisFrame`), ~16 ms quantization at 60 fps. Use Input System event timestamps mapped
  into the DSP domain during device validation.
- **Rotational-direction seam:** Windmill/Circular have no CW/CCW field yet; add when those
  techniques are implemented (accepted-but-inert today).

---

## 4. Input path is single-touch and mouse-in-editor only

**Status:** OPEN — expected for M0; note for device validation.

**What:** `HeadbangInput` reads `Touchscreen.current.primaryTouch` (one finger) and mouse in the
Editor. Multi-touch / simultaneous L+R is not handled.

**Owning package:** Device validation / input hardening.

---

## 5. Package 05 (presentation) deferred items

**Status:** OPEN — non-blocking; presentation contract (read-only/downstream) holds.

- **NEXT cue missing (presentation-avatar H1):** only CURRENT (ClosingCircleCue) exists in the play
  field; NEXT preview (25–35% emphasis, momentum preparation) is spec'd in GAMEPLAY_SCREEN_V1 but
  deferred for the M0 placeholder pass. Add a second time-authority-free preview cue for the next
  event's direction. Owner: Presentation/Avatar.
- **Venue hue vs cue contrast (M1):** venue `intenseColor` shares the red channel with the CURRENT
  approach ring; at peak intensity (THE BANG floor + Finisher pulse) the background reddens toward
  the cue hue. Shift venue intense hue to purple/blue or enforce a luminance/hue separation budget.
- **Vertical (DOWN) cue occlusion (M2):** cue vertical offset ±185 + start scale 2.4 can transiently
  overlap the head/jaw on DOWN. Validate on a portrait device; clear the jaw, cap opening scale.
- **Top bar layout (M4):** currently a single concatenated Text line; canonical layout is segmented
  [PAUSE][SCORE][PROGRESS][MULTIPLIER][HYPE] with READY rising to ~45–55% peripheral emphasis. HUD polish.
- **Scorer read-only view (qa M1):** RunScorer exposes concrete mutable Scoring/Hype getters. Expose
  read-only interfaces (IScoringReadout/IHypeReadout) to presentation before more presenters are added.
- **Presentation dt hardening:** pure reaction models don't guard NaN/Inf dt (presentation-only, can't
  affect score); add IsFinite/clamp during device validation.
- **Device/readability manual checks (PRES-11):** CURRENT+head read as one system; NEXT usefulness;
  cardinal cue legibility; thumb comfort; long-hair silhouette clearance; reduced-flash/haptics behavior.
  Not verifiable without a real phone.
- **Art/polish debt:** placeholder avatar (knob head, flat rects), no face/expression channel; hardcoded
  verticalVisualScale/smoothing in NeckPresenter -> config later.
