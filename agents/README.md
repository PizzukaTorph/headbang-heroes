# Headbang Heroes — Specialized Agents

These agent profiles are intended for Kiro/Codex-style delegated work inside this repository.

They do not replace `AGENTS.md` or the canonical documentation hierarchy.

## Mandatory read order

Every agent must read, in this order, before changing implementation or design:

1. `AGENTS.md`
2. `docs/FOUNDATION.md`
3. the relevant canonical `docs/*_V1.md` specifications for its task
4. `docs/GDD.md` for product context
5. current code only after the contract is understood

If code conflicts with the canonical contracts, the code is the thing to refactor.

## Shared operating rules

- Work on the current requested branch; do not create branches unless explicitly requested.
- Do not silently redefine gameplay semantics.
- Do not broaden scope to adjacent systems unless required to make the assigned change correct.
- Prefer pure/testable domain logic over MonoBehaviour coupling.
- Keep deterministic gameplay state separate from presentation.
- Add or update tests for gameplay-domain changes.
- Report assumptions, touched contracts, tests run, and remaining risks.
- Prototype code is evidence, not authority.

## Agent roster

### `gameplay-core`
Owns neck simulation, motion quality, event resolution, judgment, scoring, HYPE/THE BANG/Finisher semantics and run orchestration boundaries.

### `rhythm-audio`
Owns DSP timing, scheduling, pause/resume/retry synchronization, input time-domain conversion, calibration and timing diagnostics.

### `chart-content`
Owns Song/Chart schemas, RuntimeChart compilation, candidate matching, RestEvent semantics, validation, content packaging and remote-content compatibility.

### `hh-midi-validator`
Owns strict validation of `.hh.mid` authoring files against `HH_MIDI_STANDARD_V1.md`, including track/note semantics, modifiers, Rest/Windmill intervals, tempo metadata, ambiguity checks and golden-sample regression.

### `presentation-avatar`
Owns gameplay cues, body response, hair, venue/background, haptics and avatar presentation while preserving strict downstream-only behavior.

### `meta-profile`
Owns Results integration, progression, save/profile, schema migration, economy metadata and future sync boundaries without leaking into gameplay rules.

### `qa-guardian`
Acts as an adversarial reviewer for deterministic behavior, contract regressions, device/rhythm edge cases, documentation drift and test coverage.

## Recommended collaboration pattern

For a gameplay change:

```text
rhythm-audio / chart-content (when relevant)
        ↓
gameplay-core
        ↓
presentation-avatar
        ↓
meta-profile (only if run result/progression changes)
        ↓
qa-guardian
```

For HH MIDI authoring/import work:

```text
hh-midi-validator
        ↓
chart-content
        ↓
rhythm-audio / gameplay-core when semantics require it
        ↓
qa-guardian
```

Agents should remain narrow. The goal is not to build a bureaucracy; the goal is to stop one generalist implementation pass from accidentally changing timing, chart semantics, physics, UI and persistence at once.
