# Erik Technique Skill sprites (POC)

Runtime presentation assets for Erik's authored Technique Skills.

## Contract

- transparent PNG
- common canvas: **360 x 348 px**
- common character scale
- common waist/lower-body anchor
- no per-frame tight cropping
- safety margin around hair/silhouette
- presentation-only: these images never determine gameplay outcomes

## Sequences

| Technique | Frames | Files |
|---|---:|---|
| Half | 6 | `half/half_00.png` … `half_05.png` |
| Deep | 8 | `deep/deep_00.png` … `deep_07.png` |
| Whiplash | 8 | `whiplash/whiplash_00.png` … `whiplash_07.png` |
| Windmill | 8 | `windmill/windmill_00.png` … `windmill/windmill_07.png` |

These are POC assets for validating timing, transitions and gesture-to-animation feel.
Production art may add in-between frames, but must preserve the same overlay/anchor contract.
