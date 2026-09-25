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

Run `Headbang Heroes > Avatar 3D > Build Placeholder Prefab + Scene` in the Unity Editor. The
builder creates:

- `Assets/_HeadbangHeroes/Prefabs/Avatar/Avatar3D_Prototype.prefab`
- `Assets/_HeadbangHeroes/Scenes/Prototype_Headbang_3D.unity`
- inexpensive URP-compatible materials under `Assets/_HeadbangHeroes/Content/Art/Generated3D/`

The builder copies the current gameplay scene, disables only the 2D avatar instance, and inserts
the 3D prefab. `Prototype_Headbang.unity`, `Avatar_Puppet.prefab`, and the existing sprite art are
not deleted or rewritten.

## Hierarchy

```text
Avatar3D
└── Root
    └── BodyRoot
        └── Spine
            └── Chest
                ├── Neck
                │   └── NeckMesh
                ├── Head
                │   ├── HeadMesh
                │   └── HairCap
                ├── Shoulder_L
                │   └── Arm_L
                └── Shoulder_R
                    └── Arm_R
```

The mannequin uses Unity primitives with generated materials. Colliders are removed and no
Rigidbody2D, Rigidbody, or Animator is added. `Neck` and `Head` are explicit independent bones for
the presentation mapping.

## Mapping

`Avatar3DMapping` is a small pure mapping layer covered by EditMode tests. By default it clamps
each gameplay axis to ±42 degrees and distributes it as:

- Neck: 35%
- Head: 65%
- Chest compensation: 8% in the same direction

Horizontal gameplay angle maps to local Z rotation and vertical gameplay angle maps to local X
rotation. `Avatar3DPresenter` applies the result in `LateUpdate`; its optional smoothing is visual
only. It consumes the same `NeckMotionModel` instance used by the gameplay controller in the copied
scene, so input, audio timing, charts, judgment, scoring, and Motion Quality remain unchanged.

## Intentionally not implemented

- no final character model, humanoid rig, or external asset/package;
- no Unity Animator authority or animation state machine;
- no final toon/cel shader or post-processing dependency;
- no final hair physics or revival of the old 2D hair layers;
- no body choreography, windmill/circular technique mapping, or customization UI;
- no changes to the 2D fallback scene or authoritative gameplay systems.

The intended future hair direction is `Head -> hair root bones -> optional secondary bones`, always
presentation-only and optionally driven by neck angular velocity/momentum.

## Next milestones

1. Import and validate a real character model.
2. Decide between a humanoid/custom rig boundary.
3. Replace placeholder materials with a mobile-friendly toon material.
4. Add procedural body follow-through.
5. Add hair secondary motion.
6. Extend mapping for vertical and circular/windmill expression.
7. Add avatar customization after the presentation contract is stable.
8. Profile the result on target mobile hardware.
