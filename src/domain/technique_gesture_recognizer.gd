class_name TechniqueGestureRecognizer
extends RefCounted

## Continuous gesture evidence. It identifies intent and shape; it never scores.

var reference_pixels: float = 320.0

var half_direction: float = 1.0
var half_target_min: float = 0.38
var half_target_max: float = 0.62
var half_fail_overshoot: float = 0.78
var half_max_vertical_drift: float = 0.20
var half_min_coherence: float = 0.72
var half_release_grace_seconds: float = 0.25

var deep_min_vertical_travel: float = 0.70
var deep_max_horizontal_drift: float = 0.20
var deep_min_coherence: float = 0.75

var whiplash_min_outbound: float = 0.34
var whiplash_min_return: float = 0.32
var whiplash_reversal_deadzone: float = 0.06
var whiplash_max_vertical_drift: float = 0.22

var windmill_min_turns: float = 0.85
var windmill_min_span: float = 0.34
var windmill_min_path_travel: float = 1.30
var windmill_min_angular_coherence: float = 0.62

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

var _whiplash_initial_sign: float = 0.0
var _whiplash_peak_signed_px: float = 0.0
var _whiplash_outbound_peak: float = 0.0
var _whiplash_reversal_detected: bool = false
var _whiplash_reversal_at: float = -1.0
var _whiplash_return_travel: float = 0.0

var _windmill_points: Array[Vector2] = []
var _windmill_turns: float = 0.0
var _windmill_angular_coherence: float = 0.0
var _windmill_span_x: float = 0.0
var _windmill_span_y: float = 0.0
var _windmill_completed_at: float = -1.0
var _windmill_motion := Vector2.ZERO
var _windmill_rotation_direction: StringName = &""

func configure(values: Dictionary) -> void:
	reference_pixels = maxf(1.0, float(values.get("gestureReferencePx", reference_pixels)))
	half_direction = -1.0 if str(values.get("halfDirection", "right")).to_lower() == "left" else 1.0

	var half: Dictionary = values.get("half", {})
	var deep: Dictionary = values.get("deep", {})
	var whiplash: Dictionary = values.get("whiplash", {})
	var windmill: Dictionary = values.get("windmill", {})

	half_target_min = clampf(float(half.get("targetMin", half_target_min)), 0.0, 1.0)
	half_target_max = clampf(float(half.get("targetMax", half_target_max)), half_target_min, 1.0)
	half_fail_overshoot = maxf(half_target_max, float(half.get("failOvershoot", half_fail_overshoot)))
	half_max_vertical_drift = maxf(0.0, float(half.get("maxVerticalDrift", half_max_vertical_drift)))
	half_min_coherence = clampf(float(half.get("minCoherence", half_min_coherence)), 0.0, 1.0)
	half_release_grace_seconds = maxf(0.0, float(half.get("releaseGraceMs", half_release_grace_seconds * 1000.0)) / 1000.0)

	deep_min_vertical_travel = maxf(0.0, float(deep.get("minVerticalTravel", deep_min_vertical_travel)))
	deep_max_horizontal_drift = maxf(0.0, float(deep.get("maxHorizontalDrift", deep_max_horizontal_drift)))
	deep_min_coherence = clampf(float(deep.get("minCoherence", deep_min_coherence)), 0.0, 1.0)

	whiplash_min_outbound = maxf(0.05, float(whiplash.get("minOutboundTravel", whiplash_min_outbound)))
	whiplash_min_return = maxf(0.05, float(whiplash.get("minReturnTravel", whiplash_min_return)))
	whiplash_reversal_deadzone = maxf(0.01, float(whiplash.get("reversalDeadzone", whiplash_reversal_deadzone)))
	whiplash_max_vertical_drift = maxf(0.0, float(whiplash.get("maxVerticalDrift", whiplash_max_vertical_drift)))

	windmill_min_turns = maxf(0.25, float(windmill.get("minTurns", windmill_min_turns)))
	windmill_min_span = maxf(0.05, float(windmill.get("minSpan", windmill_min_span)))
	windmill_min_path_travel = maxf(0.25, float(windmill.get("minPathTravel", windmill_min_path_travel)))
	windmill_min_angular_coherence = clampf(float(windmill.get("minAngularCoherence", windmill_min_angular_coherence)), 0.0, 1.0)

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
	_whiplash_initial_sign = 0.0
	_whiplash_peak_signed_px = 0.0
	_whiplash_outbound_peak = 0.0
	_whiplash_reversal_detected = false
	_whiplash_reversal_at = -1.0
	_whiplash_return_travel = 0.0
	_windmill_points.clear()
	_windmill_turns = 0.0
	_windmill_angular_coherence = 0.0
	_windmill_span_x = 0.0
	_windmill_span_y = 0.0
	_windmill_completed_at = -1.0
	_windmill_motion = Vector2.ZERO
	_windmill_rotation_direction = &""

func begin(technique: StringName, started_at: float, position: Vector2) -> bool:
	if technique not in [&"half", &"deep", &"whiplash", &"windmill"]:
		reset()
		return false
	reset()
	_active = true
	_technique = technique
	_started_at = started_at
	_last_sample_at = started_at
	_start_position = position
	_last_position = position
	if _technique == &"windmill":
		_windmill_points.append(position)
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

	match _technique:
		&"half":
			var travel := _linear_travel()
			_peak_travel = maxf(_peak_travel, travel)
			if not _target_entered and travel >= half_target_min and travel <= half_target_max:
				_target_entered = true
				_target_entered_at = sampled_at if sampled_at >= 0.0 else _started_at
		&"deep":
			_peak_travel = maxf(_peak_travel, _linear_travel())
		&"whiplash":
			_update_whiplash(sampled_at)
		&"windmill":
			if delta.length() >= 1.0:
				_windmill_points.append(position)
			_update_windmill(sampled_at)

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

	match _technique:
		&"half":
			var vertical_drift := _max_abs_vertical_drift / reference_pixels
			valid = _target_entered and _peak_travel < half_fail_overshoot and vertical_drift <= half_max_vertical_drift and coherence >= half_min_coherence
			if not _target_entered:
				reason = "target_not_reached"
			elif _peak_travel >= half_fail_overshoot:
				reason = "overshoot"
			elif vertical_drift > half_max_vertical_drift:
				reason = "vertical_drift"
			elif coherence < half_min_coherence:
				reason = "low_coherence"
			else:
				reason = "recognized"
		&"deep":
			valid = travel >= deep_min_vertical_travel and (_max_abs_horizontal_drift / reference_pixels) <= deep_max_horizontal_drift and coherence >= deep_min_coherence
			reason = "recognized" if valid else "deep_shape"
		&"whiplash":
			var vertical_drift := _max_abs_vertical_drift / reference_pixels
			valid = _whiplash_outbound_peak >= whiplash_min_outbound and _whiplash_reversal_detected and _whiplash_return_travel >= whiplash_min_return and vertical_drift <= whiplash_max_vertical_drift
			if _whiplash_outbound_peak < whiplash_min_outbound:
				reason = "whiplash_outbound_short"
			elif not _whiplash_reversal_detected:
				reason = "whiplash_no_reversal"
			elif _whiplash_return_travel < whiplash_min_return:
				reason = "whiplash_return_short"
			elif vertical_drift > whiplash_max_vertical_drift:
				reason = "vertical_drift"
			else:
				reason = "recognized"
		&"windmill":
			var path_travel := _path_length / reference_pixels
			valid = _windmill_turns >= windmill_min_turns and _windmill_span_x >= windmill_min_span and _windmill_span_y >= windmill_min_span and path_travel >= windmill_min_path_travel and _windmill_angular_coherence >= windmill_min_angular_coherence
			if _windmill_span_x < windmill_min_span or _windmill_span_y < windmill_min_span:
				reason = "windmill_too_small"
			elif _windmill_turns < windmill_min_turns:
				reason = "windmill_incomplete_turn"
			elif path_travel < windmill_min_path_travel:
				reason = "windmill_path_short"
			elif _windmill_angular_coherence < windmill_min_angular_coherence:
				reason = "windmill_low_coherence"
			else:
				reason = "recognized"

	var result := current_evidence()
	result["completed_at"] = completed_at
	result["duration"] = completed_at - _started_at
	result["released"] = true
	result["release_grace_seconds"] = half_release_grace_seconds if _technique == &"half" else 0.0
	result["valid"] = valid
	result["reason"] = reason
	result["normalized_horizontal_travel"] = (_last_position.x - _start_position.x) * half_direction / reference_pixels
	result["normalized_vertical_travel"] = (_last_position.y - _start_position.y) / reference_pixels
	result["vertical_drift"] = _max_abs_vertical_drift / reference_pixels
	result["horizontal_drift"] = _max_abs_horizontal_drift / reference_pixels
	result["overshoot"] = overshoot
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
		"directional_coherence": _coherence(),
		"started_at": _started_at,
		"duration": _last_sample_at - _started_at if _active else 0.0,
		"whiplash_outbound": _whiplash_outbound_peak,
		"whiplash_reversal": _whiplash_reversal_detected,
		"whiplash_reversal_at": _whiplash_reversal_at,
		"whiplash_return": _whiplash_return_travel,
		"motion_progress": _whiplash_motion_progress(),
		"windmill_turns": _windmill_turns,
		"windmill_angular_coherence": _windmill_angular_coherence,
		"windmill_span_x": _windmill_span_x,
		"windmill_span_y": _windmill_span_y,
		"windmill_completed_at": _windmill_completed_at,
		"rotation_direction": _windmill_rotation_direction,
		"motion_x": _windmill_motion.x,
		"motion_y": _windmill_motion.y
	}

func debug_state() -> Dictionary:
	var evidence := current_evidence()
	evidence["travel_x"] = (_last_position.x - _start_position.x) / reference_pixels if _active else 0.0
	evidence["travel_y"] = (_last_position.y - _start_position.y) / reference_pixels if _active else 0.0
	evidence["progress"] = _progress()
	evidence["coherence"] = _coherence()
	return evidence

func _travel() -> float:
	match _technique:
		&"half", &"deep":
			return _linear_travel()
		&"whiplash", &"windmill":
			return _progress()
		_:
			return 0.0

func _linear_travel() -> float:
	if not _active:
		return 0.0
	var displacement := _last_position - _start_position
	return displacement.x * half_direction / reference_pixels if _technique == &"half" else displacement.y / reference_pixels

func _coherence() -> float:
	if _technique == &"windmill":
		return _windmill_angular_coherence
	if _path_length <= 0.0:
		return 0.0
	if _technique == &"half":
		return _positive_horizontal_progress / _path_length
	if _technique == &"deep":
		return _positive_vertical_progress / _path_length
	return 1.0

func _progress() -> float:
	match _technique:
		&"half", &"deep":
			return clampf(_linear_travel(), 0.0, 1.0)
		&"whiplash":
			if not _whiplash_reversal_detected:
				return 0.5 * clampf(_whiplash_outbound_peak / whiplash_min_outbound, 0.0, 1.0)
			return 0.5 + 0.5 * clampf(_whiplash_return_travel / whiplash_min_return, 0.0, 1.0)
		&"windmill":
			return clampf(_windmill_turns / windmill_min_turns, 0.0, 1.0)
		_:
			return 0.0

func _update_whiplash(sampled_at: float) -> void:
	var displacement_x := _last_position.x - _start_position.x
	var abs_normalized := absf(displacement_x) / reference_pixels
	if _whiplash_initial_sign == 0.0 and abs_normalized >= 0.08:
		_whiplash_initial_sign = 1.0 if displacement_x >= 0.0 else -1.0
	if _whiplash_initial_sign == 0.0:
		return
	var outbound := displacement_x * _whiplash_initial_sign / reference_pixels
	if not _whiplash_reversal_detected and outbound > _whiplash_outbound_peak:
		_whiplash_outbound_peak = outbound
		_whiplash_peak_signed_px = displacement_x
	var return_travel := (_whiplash_peak_signed_px - displacement_x) * _whiplash_initial_sign / reference_pixels
	if _whiplash_outbound_peak >= whiplash_min_outbound and return_travel >= whiplash_reversal_deadzone:
		if not _whiplash_reversal_detected:
			_whiplash_reversal_detected = true
			_whiplash_reversal_at = sampled_at if sampled_at >= 0.0 else _started_at
		_whiplash_return_travel = maxf(_whiplash_return_travel, return_travel)

func _whiplash_motion_progress() -> float:
	if _technique != &"whiplash" or _whiplash_initial_sign == 0.0:
		return 0.0
	if not _whiplash_reversal_detected:
		return clampf(_whiplash_outbound_peak / whiplash_min_outbound, 0.0, 1.0)
	var return_ratio := clampf(_whiplash_return_travel / whiplash_min_return, 0.0, 1.0)
	return lerpf(1.0, -1.0, return_ratio)

func _update_windmill(sampled_at: float) -> void:
	if _windmill_points.size() < 2:
		return
	var min_point := _windmill_points[0]
	var max_point := _windmill_points[0]
	for point in _windmill_points:
		min_point.x = minf(min_point.x, point.x)
		min_point.y = minf(min_point.y, point.y)
		max_point.x = maxf(max_point.x, point.x)
		max_point.y = maxf(max_point.y, point.y)
	var center := (min_point + max_point) * 0.5
	_windmill_span_x = (max_point.x - min_point.x) / reference_pixels
	_windmill_span_y = (max_point.y - min_point.y) / reference_pixels
	var signed_angle := 0.0
	var abs_angle := 0.0
	for i in range(1, _windmill_points.size()):
		var from_center := _windmill_points[i - 1] - center
		var to_center := _windmill_points[i] - center
		if from_center.length() < 4.0 or to_center.length() < 4.0:
			continue
		var delta_angle := wrapf(to_center.angle() - from_center.angle(), -PI, PI)
		signed_angle += delta_angle
		abs_angle += absf(delta_angle)
	_windmill_turns = absf(signed_angle) / TAU
	_windmill_angular_coherence = absf(signed_angle) / abs_angle if abs_angle > 0.0001 else 0.0
	_windmill_rotation_direction = &"clockwise" if signed_angle > 0.0 else &"counterclockwise" if signed_angle < 0.0 else &""
	var half_span := maxf(8.0, maxf(max_point.x - min_point.x, max_point.y - min_point.y) * 0.5)
	_windmill_motion = Vector2(
		clampf((_last_position.x - center.x) / half_span, -1.0, 1.0),
		clampf((_last_position.y - center.y) / half_span, -1.0, 1.0)
	)
	if _windmill_completed_at < 0.0 and _windmill_turns >= windmill_min_turns and _windmill_span_x >= windmill_min_span and _windmill_span_y >= windmill_min_span:
		_windmill_completed_at = sampled_at if sampled_at >= 0.0 else _started_at
