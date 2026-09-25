# Avatar Puppet POC

The first usable 2D avatar lives in `Assets/_HeadbangHeroes/Prefabs/Avatar/Avatar_Puppet.prefab`.

## Approved visual reference — corrected P01 (2026-09-22)

The current `Avatar_Puppet.prefab` and `Prototype_Headbang` scene pose are the user-approved
corrected `P01` reference. Older P01/P02 iterations are historical; do not restore their transforms
over this baseline. Preserve this composition when making subsequent targeted revisions.

Reference state: clean set, source sprites at 100 PPU and local scale 1, scene avatar scale 0.62,
centered face/torso, `HeadPivot` at y=2.0, and static arms at x=±1.85, y=-0.65 behind the torso.
`Hair_Front`, `Hair_Back`, and the Neck renderer are disabled; the complete `Head` artwork is used
as one face-and-hair unit. No source PNG artwork was changed for this pose.

Known follow-up observation, not part of the baseline correction: a small flesh-colored arm edge
is visible at the shirt flank. It comes from the separate arm sprite silhouette meeting the edge
of `Torso`; diagnose/correct only the arm-to-torso join in a later revision. Keep this P01 state as
the comparison reference and do not move the torso, head, or other avatar parts as part of that
targeted fix.

## Source sets

- `Assets/sprites/clean/` is the default PoC set: black T-shirt avatar.
- `Assets/sprites/modular/` is serialized into the same prefab for a later art-set switch; there is
  intentionally no customization UI yet.
- `Assets/_HeadbangHeroes/Editor/AvatarPuppetBuilder.cs` configures every PNG as a single Sprite
  with 100 Pixels Per Unit, alpha transparency, no mipmaps and uncompressed texture data, then
  builds the prefab and installs it into `Prototype_Headbang`.

Run `Headbang Heroes > Avatar > Build Clean Puppet + Prototype` after importing or changing the
source artwork. The builder does not edit the PNG files.

## Hierarchy

```text
Avatar
├── Torso
├── Arm_L
├── Arm_R
├── NeckPivot
│   └── Neck (static; renderer disabled in corrected P01)
└── HeadPivot
    └── Head
```

The clean `Head.png` is used as one complete face-and-hairstyle sprite. It already contains the
full hair silhouette around the face, as well as the mouth, chin and beard. `Hair_Front` and
`Hair_Back` remain disabled in the prefab and scene; no separate hair sprite is rendered or
animated. This avoids using the back-view silhouette as a false rear-hair mass and prevents gaps
or duplicate strands caused by independently aligned layers. There are no jaw, mouth, eye,
eyebrow or accessory overlays. The complete `Head` sprite is the only animated visual element;
torso, arms and neck remain static.

The `HeadPivot` is a direct child of `Avatar` and is positioned at y=2.0 in corrected P01. The
static neck hierarchy does not inherit head rotation.
`Rest Pose Debug` holds the complete head sprite at zero rotation. The debug rotation test sweeps
that whole sprite together; there is deliberately no secondary hair motion in this PoC.

## Runtime ownership

`AvatarPuppetController` is presentation-only. It reads `NeckMotionModel` and applies a visual
head spring by rotating only `HeadPivot`; it does not receive input, alter the authoritative neck
state, or participate in audio, chart, timing, scoring or feedback resolution. The existing
`NeckMotionModel` remains the owner of impulse, damping, limits and return-to-center.

Enable `Debug Rotation Test` on `AvatarPuppetController` to slowly sweep `HeadPivot` from -20 to
20 degrees. This is a presentation alignment check only and does not change gameplay state.

Use `SelectArtSet(AvatarArtSet.Modular)` from a future presentation/loadout boundary when modular
switching is needed. This is deliberately not exposed as customization yet.
