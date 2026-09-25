# 3D Avatar Spike

## Purpose

This is a parallel presentation experiment. It proves that the existing Headbang Heroes gameplay
loop can drive a skeletal 3D avatar without replacing the approved 2D prototype.

The authoritative chain remains:

```text
input / chart / AudioClock
    -> NeckMotionModel
    -> judgment / Motion Quality / scoring
    -> Avatar3DPresenter
    -> Neck / Head / Chest transforms
```

`NeckMotionModel` remains the only owner of gameplay neck state. The presenter reads its angles and
may smooth the visual result with render time, but no bone transform, Animator state, or visual
velocity is used by timing, judgment, Motion Quality, chart resolution, or scoring.

## Generated prototype

Run `Headbang Heroes > Avatar 3D > Build Sidekick Host + Scene` in the Unity Editor. The builder
discovers the local Humanoid prefab below `Assets/Synty/SidekickCharacters/Characters` with the
most existing `hair_dyn_*` joints,
instantiates it as a scene dependency, and creates:

- `Assets/_HeadbangHeroes/Prefabs/Avatar/Avatar3D_Prototype.prefab`
- `Assets/_HeadbangHeroes/Scenes/Prototype_Headbang_3D.unity`
The builder copies the current gameplay scene, disables only the 2D avatar instance, and inserts
the HH-owned host plus the local Sidekick prefab. No Synty mesh, texture, material or script is
copied into `_HeadbangHeroes`; `Assets/Synty/` remains a local gitignored dependency.
`Prototype_Headbang.unity`, `Avatar_Puppet.prefab`, and the existing sprite art are not deleted or
rewritten.

## Hierarchy

```text
HH_Avatar_3D                         (HH-owned host + Avatar3DPresenter)
└── SidekickCharacter_LocalDependency (local Sidekick prefab instance)
    └── Animator (Humanoid Avatar)
        ├── ... Neck ...
        ├── ... Head ...
        └── ... Chest / UpperChest ...
```

The presenter resolves `HumanBodyBones.Neck` and `HumanBodyBones.Head`, then prefers
`HumanBodyBones.UpperChest` with `Chest` as fallback. It also retains explicit Transform slots as a
fallback for future non-Humanoid prototypes. No Rigidbody or animation clip is required.

## Mapping

`Avatar3DMapping` is a small pure mapping layer covered by EditMode tests. By default it clamps
each gameplay axis to ±42 degrees and distributes it as:

- Neck: 35%
- Head: 65%
- Chest compensation: 8% in the same direction

Horizontal gameplay angle maps to local Z rotation and vertical gameplay angle maps to local X
rotation. `Avatar3DPresenter` applies the result in `LateUpdate`; its optional smoothing is visual
only. It captures each imported bone's rest local rotation and composes the visual rotation on top
of it, so the Sidekick import orientation is preserved. It consumes the same `NeckMotionModel`
instance used by the gameplay controller in the copied scene, so input, audio timing, charts,
judgment, scoring, and Motion Quality remain unchanged. The Animator is not the headbang authority
and no visual state is sent back to gameplay.

## Sidekick long-hair spike

The local Sidekick user guide has a `Dynamic Joints` section, but this installed package does not
ship a runtime dynamic-joint component or hair-physics API. Its runtime `hair_dyn_*` objects are
ordinary imported `Transform` joints. The guide points to optional external solutions such as
UniVRM SpringBone; no such package is added here.

The builder currently selects the local `HumanSpecies_04` long-hair variant because it exposes the
largest set of those existing joints. The exact asset remains a local Synty dependency. The
HH-owned `Avatar3DHairMotion` adapter discovers those names and applies a small under-damped,
inverse spring to the imported rest rotations after the presenter has applied the head pose. The
default response is intentionally readable: lag, direction inversion, whip, overshoot, and settle.
Its only tuning controls are lag, spring stiffness/damping, whip gain, and maximum secondary
angle. The adapter is inert for a bald character with no `hair_dyn_*` joints.

The prototype camera is orthographic and currently generated at `orthographicSize = 1.65` so the
character reads at mobile-style HH scale; this remains a single builder setting for quick tuning.

## Intentionally not implemented

- no final character model decision or external package dependency;
- no Unity Animator authority or animation state machine;
- no final toon/cel shader or post-processing dependency;
- no external hair-physics package, Animator authority, or revival of the old 2D hair layers;
- no body choreography, windmill/circular technique mapping, or customization UI;
- no changes to the 2D fallback scene or authoritative gameplay systems.

The intended future hair direction is `Head -> hair root bones -> optional secondary bones`, always
presentation-only and optionally driven by neck angular velocity/momentum. This spike only proves
the smallest useful adapter against the existing Sidekick joints; it is not a generalized HH hair
framework.

## Next milestones

1. Import and validate a real character model.
2. Decide between a humanoid/custom rig boundary.
3. Replace placeholder materials with a mobile-friendly toon material.
4. Add procedural body follow-through.
5. Replace the spike hair spring with artist-authored joint tuning or a selected runtime solution.
6. Extend mapping for vertical and circular/windmill expression.
7. Add avatar customization after the presentation contract is stable.
8. Profile the result on target mobile hardware.
