# HH Lab Track #001 — Beyond the Pain

## Source

User-supplied Asidie material:
- MP3: `2 - Beyond the Pain.mp3`
- MIDI: `06 - Beyond the Pain.mid`

The source binaries are intentionally **not committed**. They can be placed locally for development after rights/storage policy is decided.

## MIDI analysis

- MIDI type: 1
- PPQ: 480
- tracks: conductor + 3 pitched/instrument tracks + Percussioni
- initial tempo: 150 BPM
- tempo change: ~258.8 MIDI seconds -> ~140 BPM
- meter: 4/4
- brief 3/4 section: ~158.4–159.6 MIDI seconds
- returns to 4/4 immediately after

The percussion track provides a useful authoritative musical grid for chart authoring.

## MP3/MIDI alignment

The MP3 is ~282.456 s. MIDI musical content is shorter and does not share the same zero point.

Cross-correlation between the MIDI percussion impulses and the decoded MP3 onset envelope produced a strongest alignment near:

**audioTime ~= midiTime + 10.54 s**

The MIDI percussion entrance at ~12.80 s therefore corresponds to approximately 23.34 s in the audio.

This value is **provisional**. It must be verified by ear/in-editor before becoming production metadata. The pipeline must support explicit per-song audio/MIDI offset; it must never assume both files start at the same musical zero.

## M0 section

First prototype window:

- audio: ~23.34–83.34 s
- MIDI: ~12.80–72.80 s
- duration: 60 s
- technique: Classic Bang only

The first authored chart prioritizes readable alternating L/R events over literal transcription of every drum hit. The percussion MIDI informs timing and accents; it does not dictate one input per percussion note.

### First playable chart

The committed M0 chart contains **76 alternating Classic Bang events** at half-time relative to the 150 BPM source: approximately **75 headbangs per minute**, one event every 0.8 seconds.

Why half-time for M0:
- readable on first contact
- enough time to perceive head inertia and reversal
- suitable for testing closing-circle timing
- avoids confusing rhythm-density problems with motion-model problems

Direction alternates Left / Right. Intensity is derived from the percussion grid: ordinary kick/snare anchors use medium-high intensity, while crash/accent positions use maximum prototype intensity.

Chart file:
`Assets/_HeadbangHeroes/Content/Lab/lab-001-beyond-the-pain-classic-m0.json`

## Unity M0 setup

Local source audio is ignored by Git.

Place the supplied MP3 at:

`Assets/_HeadbangHeroes/Content/Lab/LocalAudio/BeyondThePain.mp3`

Then in Unity run:

`Tools > Headbang Heroes > Build M0 Prototype`

The builder creates/updates:
- the lab SongDefinition
- `Prototype_Headbang.unity`
- DSP AudioClock wiring
- JSON chart wiring
- touch/mouse L/R input
- placeholder torso/head
- head-centered target + approach rings
- automatic playback starting at song time 22.0 s

The first chart cue appears roughly one second before the first event at 23.34 s.

The closing ring is driven from absolute DSP song time rather than animation elapsed time, so visual drift cannot become the scoring clock.

## Architecture consequence

Song metadata requires:
- stable song ID
- audio asset reference
- explicit sync offset
- tempo map
- meter map
- sections
- one or more authored charts

Charts remain independent from source MIDI so future difficulty levels can simplify or intensify the same song without modifying the musical source.
