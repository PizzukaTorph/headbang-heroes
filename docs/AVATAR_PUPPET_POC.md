# Avatar Puppet POC

The first usable 2D avatar lives in `Assets/_HeadbangHeroes/Prefabs/Avatar/Avatar_Puppet.prefab`.

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
└── HeadPivot
    ├── Hair_Back
    ├── Head
    └── Hair_Front
```

Only these six sprites are active in the PoC: torso, both arms, complete head, and the two hair
layers. The `Torso` artwork already contains the upper neck/collar, and the `Head` artwork
contains the mouth, chin and beard; there is no separate neck, jaw,
mouth, eye, eyebrow or accessory overlay. All visible parts use `SpriteRenderer`. The
`HeadPivot` is positioned at the head/neck join, so the complete head and both hair layers rotate
as one attached visual unit. Torso and arms are static presentation layers. The two hair layers
receive a small delayed spring response relative to `HeadPivot`.

`Rest Pose Debug` forces every local rotation to zero and disables animation, smoothing and hair
lag so the base alignment can be inspected as one illustration before enabling movement.

## Runtime ownership

`AvatarPuppetController` is presentation-only. It reads `NeckMotionModel` and applies a visual
head spring by rotating only `HeadPivot`, plus `HairChainModel`; it does not receive input, alter the authoritative neck state,
or participate in audio, chart, timing, scoring or feedback resolution. The existing
`NeckMotionModel` remains the owner of impulse, damping, limits and return-to-center.

Enable `Debug Rotation Test` on `AvatarPuppetController` to slowly sweep `HeadPivot` from -20 to
20 degrees. This is a presentation alignment check only and does not change gameplay state.

Use `SelectArtSet(AvatarArtSet.Modular)` from a future presentation/loadout boundary when modular
switching is needed. This is deliberately not exposed as customization yet.
