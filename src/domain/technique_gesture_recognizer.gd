class_name TechniqueGestureRecognizer
extends RefCounted

## Continuous gesture evidence. It identifies intent and shape; it never scores.

var reference_pixels: float = 320.0
var half_direction: float = 1.0
var half_target_min: float = 0.38
var half_target_max: float = 0.62
var half_fail_overshoot: float = 0.78
var half_min_coherence: float = 0.72
var half_release_grace_seconds: float = 0.25
var deep_min_vertical_travel: float = 0.70
var deep_max_horizontal_drift: float = 0.20
var deep_min_coherence: float = 0.75

var _active: bool = false
var _technique: StringName = &""
var _started_at: float = 0.0
var _last_sample_at: float = 0.0
var _start_position := Vector2.ZERO
var _last_position := Vector2.ZERO
var _path_length: float = 0.0
var _positive_horizontal_progress: float = 0.0
var _positive_vertical_progress: float = 0.0
var _max_abs_vertical_drift: float = 0.0
var _max_abs_horizontal_drift: float = 0.0
var _peak_travel: float = 0.0
var _target_entered: bool = false
var _target_entered_at: float = -1.0

func configure(values: Dictionary) -> void:
	reference_pixels = maxf(1.0, float(values.get("gestureReferencePx", reference_pixels)))
	half_direction = -1.0 if str(values.get("halfDirection", "right")).to_lower() == "left" else 1.0
	var half: Dictionary = values.get("half", {})
	var deep: Dictionary = values.get("deep", {})
	half_target_min = clampf(float(half.get("targetMin", half_target_min)), 0.0, 1.0)
	half_target_max = clampf(float(half.get("targetMax", half_target_max)), half_target_min, 1.0)
	half_fail_overshoot = maxf(half_target_max, float(half.get("failOvershoot", half_fail_overshoot)))
	half_min_coherence = clampf(float(half.get("minCoherence", half_min_coherence)), 0.0, 1.0)
	half_release_grace_seconds = maxf(0.0, float(half.get("releaseGraceMs", half_release_grace_seconds * 1000.0)) / 1000.0)
	deep_min_vertical_travel = maxf(0.0, float(deep.get("minVerticalTravel", deep_min_vertical_travel)))
	deep_max_horizontal_drift = maxf(0.0, float(deep.get("maxHorizontalDrift", deep_max_horizontal_drift)))
	deep_min_coherence = clampf(float(deep.get("minCoherence", deep_min_coherence)), 0.0, 1.0)

func reset() -> void:
	_active = false
	_technique = &""
	_started_at = 0.0
	_last_sample_at = 0.0
	_start_position = Vector2.ZERO
	_last_position = Vector2.ZERO
	_path_length = 0.0
	_positive_horizontal_progress = 0.0
	_positive_vertical_progress = 0.0
	_max_abs_vertical_drift = 0.0
	_max_abs_horizontal_drift = 0.0
	_peak_travel = 0.0
	_target_entered = false
	_target_entered_at = -1.0

func begin(technique: StringName, started_at: float, position: Vector2) -> bool:
	if technique not in [&"half", &"deep"]:
		reset()
		return false
	reset()
	_active = true
	_technique = technique
	_started_at = started_at
	_last_sample_at = started_at
	_start_position = position
	_last_position = position
	return true

func update(position: Vector2, sampled_at: float = -1.0) -> void:
	if not _active:
		return
	var delta := position - _last_position
	_path_length += delta.length()
	_positive_horizontal_progress += maxf(0.0, delta.x * half_direction)
	_positive_vertical_progress += maxf(0.0, delta.y)
	_max_abs_vertical_drift = maxf(_max_abs_vertical_drift, absf(position.y - _start_position.y))
	_max_abs_horizontal_drift = maxf(_max_abs_horizontal_drift, absf(position.x - _start_position.x))
	_last_position = position
	if sampled_at >= 0.0:
		_last_sample_at = sampled_at
	var travel := _travel()
	_peak_travel = maxf(_peak_travel, travel)
	if _technique == &"half" and not _target_entered and travel >= half_target_min and travel <= half_target_max:
		_target_entered = true
		_target_entered_at = sampled_at if sampled_at >= 0.0 else _started_at

func complete(completed_at: float, position: Vector2 = Vector2.INF) -> Dictionary:
	if not _active:
		return {}
	if position != Vector2.INF:
		update(position, completed_at)

	var travel := _travel()
	var coherence := _coherence()
	var overshoot := maxf(0.0, _peak_travel - half_target_max)
	var valid := false
	var reason := ""
	if _technique == &"half":
		valid = _target_entered and _peak_travel < half_fail_overshoot and coherence >= half_min_coherence
		if not _target_entered:
			reason = "target_not_reached"
		elif _peak_travel >= half_fail_overshoot:
			reason = "overshoot"
		elif coherence < half_min_coherence:
			reason = "low_coherence"
		else:
			reason = "recognized"
	else:
		valid = travel >= deep_min_vertical_travel and (_max_abs_horizontal_drift / reference_pixels) <= deep_max_horizontal_drift and coherence >= deep_min_coherence
		reason = "recognized" if valid else "deep_shape"

	var result := {
		"technique": _technique,
		"started_at": _started_at,
		"completed_at": completed_at,
		"travel": travel,
		"peak_travel": _peak_travel,
		"target_entered": _target_entered,
		"target_entered_at": _target_entered_at,
		"overshoot": overshoot,
		"directional_coherence": coherence,
		"normalized_horizontal_travel": (_last_position.x - _start_position.x) * half_direction / reference_pixels,
		"normalized_vertical_travel": (_last_position.y - _start_position.y) / reference_pixels,
		"vertical_drift": _max_abs_vertical_drift / reference_pixels,
		"horizontal_drift": _max_abs_horizontal_drift / reference_pixels,
		"duration": completed_at - _started_at,
		"released": true,
		"release_grace_seconds": half_release_grace_seconds,
		"valid": valid,
		"reason": reason
	}
	reset()
	return result

func current_evidence() -> Dictionary:
	return {
		"active": _active,
		"technique": _technique,
		"travel": _travel(),
		"peak_travel": _peak_travel,
		"target_entered": _target_entered,
		"target_entered_at": _target_entered_at,
		"overshoot": maxf(0.0, _peak_travel - half_target_max),
		"directional_coherence": _coherence(),
		"started_at": _started_at,
		"duration": _last_sample_at - _started_at if _active else 0.0
	}

func debug_state() -> Dictionary:
	var evidence := current_evidence()
	evidence["travel_x"] = (_last_position.x - _start_position.x) / reference_pixels if _active else 0.0
	evidence["travel_y"] = (_last_position.y - _start_position.y) / reference_pixels if _active else 0.0
	evidence["progress"] = clampf(_travel(), 0.0, 1.0)
	return evidence

func _travel() -> float:
	if not _active:
		return 0.0
	var displacement := _last_position - _start_position
	return displacement.x * half_direction / reference_pixels if _technique == &"half" else displacement.y / reference_pixels

func _coherence() -> float:
	if _path_length <= 0.0:
		return 0.0
	return (_positive_horizontal_progress if _technique == &"half" else _positive_vertical_progress) / _path_length
