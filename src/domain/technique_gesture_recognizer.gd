class_name TechniqueGestureRecognizer
extends RefCounted

## Presentation/input samples become a semantic gesture intent here.
## This class never judges score and never knows authored chart timing.

var reference_pixels: float = 320.0
var half_direction: float = 1.0
var half_min_horizontal_travel: float = 0.25
var half_max_vertical_drift: float = 0.20
var half_min_coherence: float = 0.75
var deep_min_vertical_travel: float = 0.70
var deep_max_horizontal_drift: float = 0.20
var deep_min_coherence: float = 0.75

var _active: bool = false
var _technique: StringName = &""
var _started_at: float = 0.0
var _start_position := Vector2.ZERO
var _last_position := Vector2.ZERO
var _path_length: float = 0.0
var _positive_horizontal_progress: float = 0.0
var _positive_vertical_progress: float = 0.0
var _max_abs_vertical_drift: float = 0.0
var _max_abs_horizontal_drift: float = 0.0

func configure(values: Dictionary) -> void:
	reference_pixels = maxf(1.0, float(values.get("gestureReferencePx", reference_pixels)))
	half_direction = -1.0 if str(values.get("halfDirection", "right")).to_lower() == "left" else 1.0
	var half: Dictionary = values.get("half", {})
	var deep: Dictionary = values.get("deep", {})
	half_min_horizontal_travel = maxf(0.0, float(half.get("minHorizontalTravel", half_min_horizontal_travel)))
	half_max_vertical_drift = maxf(0.0, float(half.get("maxVerticalDrift", half_max_vertical_drift)))
	half_min_coherence = clampf(float(half.get("minCoherence", half_min_coherence)), 0.0, 1.0)
	deep_min_vertical_travel = maxf(0.0, float(deep.get("minVerticalTravel", deep_min_vertical_travel)))
	deep_max_horizontal_drift = maxf(0.0, float(deep.get("maxHorizontalDrift", deep_max_horizontal_drift)))
	deep_min_coherence = clampf(float(deep.get("minCoherence", deep_min_coherence)), 0.0, 1.0)

func reset() -> void:
	_active = false
	_technique = &""
	_started_at = 0.0
	_start_position = Vector2.ZERO
	_last_position = Vector2.ZERO
	_path_length = 0.0
	_positive_horizontal_progress = 0.0
	_positive_vertical_progress = 0.0
	_max_abs_vertical_drift = 0.0
	_max_abs_horizontal_drift = 0.0

func begin(technique: StringName, started_at: float, position: Vector2) -> bool:
	if technique not in [&"half", &"deep"]:
		reset()
		return false
	reset()
	_active = true
	_technique = technique
	_started_at = started_at
	_start_position = position
	_last_position = position
	return true

func update(position: Vector2) -> void:
	if not _active:
		return
	var delta := position - _last_position
	_path_length += delta.length()
	_positive_horizontal_progress += maxf(0.0, delta.x * half_direction)
	_positive_vertical_progress += maxf(0.0, delta.y)
	_max_abs_vertical_drift = maxf(_max_abs_vertical_drift, absf(position.y - _start_position.y))
	_max_abs_horizontal_drift = maxf(_max_abs_horizontal_drift, absf(position.x - _start_position.x))
	_last_position = position

func complete(completed_at: float, position: Vector2 = Vector2.INF) -> Dictionary:
	if not _active:
		return {}
	if position != Vector2.INF:
		update(position)

	var displacement := _last_position - _start_position
	var normalized_horizontal := displacement.x * half_direction / reference_pixels
	var normalized_vertical := displacement.y / reference_pixels
	var coherence := 0.0
	if _path_length > 0.0:
		coherence = _positive_horizontal_progress / _path_length if _technique == &"half" else _positive_vertical_progress / _path_length
	var result := {
		"technique": _technique,
		"started_at": _started_at,
		"completed_at": completed_at,
		"normalized_travel": normalized_horizontal if _technique == &"half" else normalized_vertical,
		"normalized_horizontal_travel": normalized_horizontal,
		"normalized_vertical_travel": normalized_vertical,
		"directional_coherence": coherence,
		"vertical_drift": _max_abs_vertical_drift / reference_pixels,
		"horizontal_drift": _max_abs_horizontal_drift / reference_pixels,
		"valid": false,
		"reason": ""
	}
	if _technique == &"half":
		result["valid"] = normalized_horizontal >= half_min_horizontal_travel and result["vertical_drift"] <= half_max_vertical_drift and coherence >= half_min_coherence
		result["reason"] = "recognized" if bool(result["valid"]) else "half_shape"
	else:
		result["valid"] = normalized_vertical >= deep_min_vertical_travel and result["horizontal_drift"] <= deep_max_horizontal_drift and coherence >= deep_min_coherence
		result["reason"] = "recognized" if bool(result["valid"]) else "deep_shape"
	reset()
	return result

func debug_state() -> Dictionary:
	return {
		"active": _active,
		"technique": _technique,
		"travel_x": (_last_position.x - _start_position.x) / reference_pixels if _active else 0.0,
		"travel_y": (_last_position.y - _start_position.y) / reference_pixels if _active else 0.0,
		"progress": _progress(),
		"coherence": (_positive_horizontal_progress / _path_length if _technique == &"half" else _positive_vertical_progress / _path_length) if _active and _path_length > 0.0 else 0.0
	}

func _progress() -> float:
	if not _active:
		return 0.0
	var displacement := _last_position - _start_position
	var normalized := displacement.x * half_direction / reference_pixels if _technique == &"half" else displacement.y / reference_pixels
	return clampf(normalized, 0.0, 1.0)
