class_name CueLayout
extends RefCounted

## Pure presentation mapping from semantic direction to a cue-anchor offset.
## It never changes chart semantics, timing, matching or input meaning.

static func offset_for(direction: StringName, mode: StringName, spread_px: float) -> Vector2:
	if mode != &"directional":
		return Vector2.ZERO
	var spread := maxf(0.0, spread_px)
	match direction:
		&"left": return Vector2(-spread, 0.0)
		&"right": return Vector2(spread, 0.0)
		&"up": return Vector2(0.0, -spread)
		&"down": return Vector2(0.0, spread)
		_: return Vector2.ZERO
