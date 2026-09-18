# Headbang Heroes MIDI Authoring Standard v1

## Status

Canonical authoring/interchange convention for Headbang Heroes charts.

The HH MIDI file is **not required to be the MIDI performance of the original song**.

> **The HH MIDI is the MIDI representation of the Headbang Heroes performance chart.**

The source song may have no original MIDI at all. An existing band/DAW/Guitar Pro MIDI can be used as a reference layer, but Headbang Heroes gameplay authoring lives in dedicated `HH_*` tracks.

Runtime never depends on MIDI parsing during gameplay.

---

## 1. Core pipeline

```text
song.wav / song.mp3 / other approved audio
                +
             song.hh.mid
                +
          SongDefinition metadata
                ↓
         validator / compiler
                ↓
          ChartDefinition
                ↓
       immutable RuntimeChart
                ↓
             gameplay
```

MIDI is an authoring and interchange format.

The compiled chart/runtime format is the game contract.

This separation allows the MIDI convention to evolve without forcing installed clients to parse or reinterpret old authoring files.

---

## 2. Musical abstraction rule

The MIDI must describe the **performable headbang choreography**, not blindly copy the source instrumentation.

> **The chart follows the performable headbang rhythm, not every musical subdivision present in the source.**

Example:

```text
source drums:  1/32  K K K K K K K K K K K K K K K K
HH chart:      1/8   L       R       L       R
```

or the same passage may become:

- `Half` pulses
- `Burst`
- `Whiplash`
- sustained `Windmill`
- selected musical accents only

Conceptual authoring layers:

```text
MUSIC LAYER
what the song is doing

PERFORMANCE LAYER
what a human neck should perform

CHART LAYER
how HH encodes that performance
```

Original song MIDI, when available, belongs to the MUSIC/reference layer. `HH_*` tracks belong to the CHART authoring layer.

---

## 3. MIDI file baseline

Preferred authoring format:

- Standard MIDI File (SMF), Type 1
- PPQ/ticks-per-quarter-note: 480 minimum; 960 preferred where the authoring tool supports it cleanly
- tempo map permitted
- time-signature events permitted
- track names mandatory for semantic HH tracks
- note velocity retained
- note duration retained

Do not rely on General MIDI instrument meaning for HH semantic tracks.

The note numbers below are deliberately repurposed as HH commands.

HH semantic tracks should preferably use an ordinary non-drum MIDI channel so DAWs do not forcibly render them as a drum kit. The semantic meaning comes from **track name + note number**, never from instrument patch/channel identity.

---

## 4. Canonical track names

Initial canonical semantic tracks:

```text
HH_CLASSIC
HH_HALF
HH_DEEP
HH_WHIPLASH
HH_WINDMILL
HH_REST
HH_MOD
HH_META
```

Track-name matching is case-insensitive after trimming whitespace, but exporters should write the uppercase canonical names.

Unknown `HH_*` semantic tracks are validation errors unless explicitly supported by the declared schema version.

Non-HH tracks are allowed as authoring/reference material and are ignored by the HH compiler by default.

Examples:

```text
DRUMS
GUITAR
BASS
VOCALS
HH_CLASSIC
HH_MOD
HH_META
```

This lets an author keep useful reference material in the same project/file without turning it into gameplay.

---

## 5. Direction note mapping

For directional MotionEvent tracks (`HH_CLASSIC`, `HH_HALF`, `HH_DEEP`, and any future compatible directional technique), use the same four note numbers everywhere:

| MIDI note | Semantic direction / inversion point |
|---:|---|
| 36 | LEFT |
| 35 | RIGHT |
| 41 | UP |
| 45 | DOWN |

The direction names describe the authored **inversion/commit point**.

For Classic semantics:

```text
36 / LEFT  → invert/commit LEFT  → launch RIGHT
35 / RIGHT → invert/commit RIGHT → launch LEFT
41 / UP    → invert/commit UP    → launch DOWN
45 / DOWN  → invert/commit DOWN  → launch UP
```

The compiler derives trajectory where unambiguous:

```text
LEFT / RIGHT → Horizontal
UP / DOWN    → Vertical
```

A technique that requires additional trajectory semantics must define them explicitly in this standard before publication use.

---

## 6. Technique by track

Technique is primarily encoded by track identity.

Examples:

```text
HH_CLASSIC + note 36
→ Technique.Classic + Horizontal + Left

HH_HALF + note 35
→ Technique.Half + Horizontal + Right

HH_DEEP + note 41
→ Technique.Deep + Vertical + Up
```

This is intentionally preferable to assigning dozens of unrelated MIDI pitches to every `Technique × Direction` combination.

It keeps piano-roll authoring readable and preserves one shared directional vocabulary.

---

## 7. Event position

The MIDI note-on absolute tick determines the authored musical position.

The compiler converts tick position through the tempo map into canonical beat position and deterministic runtime song time.

Runtime audio remains authoritative. MIDI timing describes how musical beat-space maps onto that audio; it does not replace the audio clock.

Audio/chart alignment offset belongs to the SongDefinition/content metadata and must be applied explicitly by the compiler/runtime contract.

---

## 8. Velocity = intensity

For HH motion tracks, note velocity represents authored movement intensity.

Canonical normalization:

```text
MIDI velocity 1..127
        ↓
normalized intensity 0..1
```

Suggested initial mapping:

```text
intensity = velocity / 127.0
```

Velocity `0` is MIDI note-off semantics and must not create a MotionEvent.

Intensity is expressive/tuning metadata. It must not silently redefine technique or direction.

Typical authoring expectation:

- lower velocity → restrained/light movement
- medium velocity → normal authored commitment
- high velocity → strong/accented physical intent

An Accent modifier remains a distinct semantic marker; high velocity alone does not automatically mean `Modifier.Accent`.

---

## 9. Note duration

Note duration is retained and interpreted only when the semantic track/event supports duration.

Examples:

- Classic pulse: duration may be ignored except for tooling display
- Deep: duration may inform a long/committed authored gesture where supported
- Whiplash: duration meaning is technique-specific and must be validated
- Windmill: duration is significant and represents continuous action interval
- Rest: duration defines the authored Rest interval

The compiler must never invent duration-sensitive gameplay from note length for a technique whose current rules do not define it.

---

## 10. Authored Rest

`HH_REST` represents explicit Authored Rest intervals.

For v1, use note 60 as the canonical Rest interval note:

| Track | Note | Meaning |
|---|---:|---|
| HH_REST | 60 | Authored Rest interval |

Note-on = Rest start.

Note-off = Rest end.

Example:

```text
HH_REST
             60 [================]
                start           end
```

The compiler emits a `RestEvent` with duration.

Rest evaluation uses canonical runtime semantics:

```text
Rest begins
→ settling phase
→ stillness evaluation phase
→ RestOutcome
```

The MIDI interval defines the full authored Rest duration. Settling/evaluation split is provided by chart defaults, difficulty/rules configuration, or explicit future metadata; the authoring convention must not force the neck to snap to neutral.

Natural Rest is still represented by **no required HH event** and therefore needs no MIDI note.

---

## 11. Modifier track

`HH_MOD` carries modifiers/flags that attach to MotionEvents at the same authored musical position.

Initial mapping:

| MIDI note | Meaning |
|---:|---|
| 60 | Accent |
| 61 | Double |
| 62 | Hold |
| 63 | Burst |
| 64 | Finisher Candidate |

Example:

```text
HH_CLASSIC     36
HH_MOD         60
               ↑ same tick

→ Classic + Left + Accent
```

Example:

```text
HH_DEEP        45
HH_MOD         64
               ↑ same tick

→ Deep + Down + finisherCandidate = true
```

### Attachment rule

A modifier note attaches to a compatible MotionEvent at the same absolute MIDI tick.

If no compatible event exists at that tick, validation fails.

If more than one compatible MotionEvent exists at the same tick and the modifier cannot be assigned unambiguously, validation fails rather than relying on track order.

### Multiple modifiers

The canonical chart model currently exposes one primary `Modifier` value plus independent `finisherCandidate` metadata.

Therefore:

- note 64 may coexist with one primary modifier
- two conflicting primary modifier notes on the same event are invalid unless a later schema explicitly defines composition

Do not silently choose one based on note order.

---

## 12. Windmill

`HH_WINDMILL` represents continuous circular neck performance.

Initial mapping:

| MIDI note | Meaning |
|---:|---|
| 36 | Clockwise |
| 35 | Counter-clockwise |

Note duration defines the required continuous interval.

Example:

```text
HH_WINDMILL
36 [========================]

→ Windmill + Circular + Clockwise for authored duration
```

UP/DOWN notes are not valid on `HH_WINDMILL` in v1.

Windmill should not be encoded as dozens of fake cardinal hit events.

---

## 13. Whiplash

`HH_WHIPLASH` is reserved for canonical Whiplash authoring.

The final detailed gesture encoding may evolve during gameplay validation.

Until a specific Whiplash sub-grammar is approved, the following rule applies:

> Do not overload arbitrary note numbers with undocumented Whiplash meanings.

The validator/compiler may support only the currently implemented Whiplash subset and must reject unsupported encodings clearly.

This protects existing charts from semantic reinterpretation later.

---

## 14. Sections and phrases — HH_META

Musical structure should use Standard MIDI meta text/marker events where supported by the authoring tool.

Canonical strings:

```text
SECTION:<id-or-name>
PHRASE:<id-or-name>
```

Examples:

```text
SECTION:INTRO
SECTION:VERSE_1
SECTION:BREAKDOWN
PHRASE:RIFF_A
PHRASE:BLAST_01
PHRASE:DOOM_BREAK
```

Preferred location: `HH_META` track.

The compiler may accept markers from the global conductor/meta track if the authoring tool cannot place them on `HH_META`, but official exports should normalize them where practical.

Structure markers do not directly change score.

---

## 15. Optional explicit IDs

Stable event IDs are important in compiled chart data, but authors should not be forced to manually name every MIDI note.

For v1:

- compiler generates deterministic event IDs from chart identity + semantic source position + an unambiguous ordinal/key
- editing a chart and moving events may legitimately produce new event IDs under a new `chartVersion`
- competitive/save identity relies on chart/version/rules identity, not on preserving every event ID across arbitrary chart revisions

A future HH editor may embed explicit IDs using supported text metadata for round-trip editing, but this is not required for MVP authoring.

---

## 16. Tempo and time signature

HH MIDI may carry tempo and time-signature events.

Rules:

- audio playback remains authoritative at runtime
- tempo/time signature define musical beat-space mapping
- content metadata and MIDI timing must not silently disagree
- compiler validates or explicitly imports/normalizes the timing map

For MVP songs using one BPM and one time signature, the MIDI and SongDefinition values should match.

If they differ beyond configured tolerance, compilation fails with a useful diagnostic rather than guessing.

Future tempo-map songs may designate the validated MIDI tempo map as the authoring source from which canonical timing metadata is generated.

---

## 17. Audio offset

The `.hh.mid` does not encode device calibration.

Keep these concepts separate:

```text
Song audio/chart offset → content metadata
Device audio/input/visual offsets → UserProfile calibration
```

The compiler aligns beat-space to the song using the authored content offset.

Runtime calibration then adjusts perception/input alignment without modifying chart semantics.

---

## 18. Reference MIDI tracks

If an original/arrangement MIDI exists, it may coexist in the authoring file/project.

Examples:

```text
DRUMS_REFERENCE
GUITAR_REFERENCE
BASS_REFERENCE
VOCALS_REFERENCE
```

The HH compiler ignores non-`HH_*` tracks unless a future import tool explicitly consumes them as reference/assist data.

Reference tracks must never automatically become gameplay events merely because note numbers overlap HH semantic numbers.

Semantic meaning requires the canonical `HH_*` track name.

---

## 19. Validation rules

At minimum, HH MIDI validation must detect:

- required HH track names malformed/unknown
- unsupported note values on semantic tracks
- duplicate/conflicting events at one semantic position
- modifiers without a target event
- ambiguous modifier attachment
- invalid Rest duration
- invalid Windmill duration/direction
- events outside audio/chart bounds after offset resolution
- unsupported technique semantics for declared schema/rules version
- tempo/time-signature disagreement with content metadata
- physically implausible discrete event density warnings
- ambiguous candidate-window density warnings

Warnings may remain warnings where advanced authoring is intentional, but semantic ambiguity must never be silently compiled.

---

## 20. Compiler output

The compiler translates HH MIDI into the canonical chart model.

Example source:

```text
track: HH_CLASSIC
beat: 32.0
note: 36
velocity: 110
modifier at same tick: 60
```

Conceptual output:

```text
MotionEvent
- technique: Classic
- trajectory: Horizontal
- direction: Left
- modifier: Accent
- intensity: 110 / 127
- beat: 32.0
```

MIDI note numbers do not survive as gameplay semantics unless useful for diagnostics/provenance.

Runtime consumes typed `MotionEvent` / `RestEvent` data, not raw MIDI notes.

---

## 21. File naming

Recommended authoring filename:

```text
<song-id>.<chart-id>.hh.mid
```

Examples:

```text
beyond-the-pain.normal-v1.hh.mid
song-001.extreme-v2.hh.mid
```

The filename is convenience only. Canonical song/chart identity comes from content metadata/compiler inputs, not filename parsing alone.

---

## 22. Authoring examples

### Basic Classic horizontal

```text
HH_CLASSIC
36        35        36        35
LEFT      RIGHT     LEFT      RIGHT
```

### Classic vertical with accent

```text
HH_CLASSIC
41                  45
UP                  DOWN

HH_MOD
60
ACCENT
```

### Blast-beat abstraction

```text
SOURCE DRUMS
K K K K K K K K K K K K K K K K   (1/32)

HH_HALF
36      35      36      35           (slower performable pulse)
```

### Continuous Windmill

```text
HH_WINDMILL
36 [===============================]
CW
```

### Stop / Authored Rest

```text
HH_CLASSIC
36        35

HH_REST
              60 [===========]
```

---

## 23. DAW / Guitar Pro friendliness

The convention is deliberately based on ordinary MIDI concepts:

- named tracks
- note number
- velocity
- note duration
- tempo/time-signature map
- text/marker events

This keeps charts editable in tools such as DAWs and, where MIDI import/export behavior is sufficient, Guitar Pro.

Tool-specific MIDI limitations must not become canonical HH semantics. If a tool drops markers, renames tracks, changes PPQ, rewrites channels, or quantizes notes unexpectedly, the exported file must be revalidated before publication.

The future HH Chart Editor should import/export this convention or a semantically equivalent canonical source without requiring Unity.

---

## 24. Versioning

The standard itself is versioned.

Initial value:

```text
hhMidiStandardVersion = 1
```

The version may be supplied alongside the MIDI in chart build metadata/package configuration rather than requiring a proprietary MIDI header event.

A future metadata marker may be standardized if round-trip tooling benefits from it.

Never reinterpret an old `.hh.mid` under incompatible new note semantics without explicit migration/version handling.

---

## 25. Guardrails

- MIDI is authoring/interchange, not gameplay runtime.
- HH MIDI does not need to reproduce or contain the original song MIDI.
- Non-HH tracks never become gameplay automatically.
- Track name identifies technique family; note identifies direction/action within that family.
- Direction mapping is shared wherever semantics permit.
- Velocity is intensity, not technique.
- Duration only has gameplay meaning where explicitly defined.
- Modifiers must attach deterministically.
- Charts encode performable neck choreography, not instrumental transcription.
- Audio/DSP remains runtime timing authority.
- Compiler/validator is the boundary between flexible authoring and deterministic gameplay.

---

## MVP authoring subset

The first production-ready subset may be deliberately small:

```text
HH_CLASSIC
HH_REST
HH_MOD (Accent + Finisher Candidate)
HH_META
```

with:

- one BPM
- one time signature
- fixed direction mapping
- velocity → intensity
- note-on → beat
- Rest note duration → Rest duration

`HH_HALF`, `HH_DEEP`, `HH_WINDMILL`, `HH_WHIPLASH`, and advanced modifier behavior can become publishable as their gameplay contracts pass validation.

This lets the format be comprehensive without forcing unfinished mechanics into the first song.
