# Headbang Heroes Unity source

Prototype target: **M0 — One Head, One Song, One Circle**.

First chain:

AudioClock (DSP) -> chart event -> closing circle -> L/R touch -> signed timing error -> judgment -> head impulse -> combo/score -> feedback.

Current foundation contains the authoritative audio clock, Song/Chart ScriptableObjects, timing judgment, controlled head-motion model, touch input abstraction and initial combo/score model.

The next layer is ChartScheduler + closing-circle presentation + prototype scene wiring once the lab audio/chart is available.

Do not commit third-party or production audio unless redistribution rights are explicitly cleared.
