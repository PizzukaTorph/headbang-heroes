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

The first authored chart should prioritize readable alternating L/R events over literal transcription of every drum hit. The percussion MIDI informs timing and accents; it does not dictate one input per percussion note.

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
