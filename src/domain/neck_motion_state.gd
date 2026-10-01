class_name NeckMotionState
extends RefCounted

const DEFAULT_SIMULATION_HZ := 120.0
const DEFAULT_STROKE_DURATION := 0.32
const DEFAULT_TARGET_ANGLE := 36.0
const DEFAULT_MAX_ANGLE := 42.0
const DEFAULT_MAX_VELOCITY := 360.0
const MAX_ADVANCE_SECONDS := 0.25

class AxisState:
	var displacement: float = 0.0
	var velocity: float = 0.0
	var travel_since_inversion: float = 0.0
	var peak_speed_since_inversion: float = 0.0
	var stroke_target: float = 0.0
	var stroke_progress: float = 1.0
	var stroke_active: bool = false
	var technique_active: bool = false
	var technique_progress: float = 0.0
	var technique_direction: float = 1.0
	var _stroke_start: float = 0.0

	func reset() -> void:
		displacement = 0.0
		velocity = 0.0
		travel_since_inversion = 0.0
		peak_speed_since_inversion = 0.0
		stroke_target = 0.0
		stroke_progress = 1.0
		stroke_active = false
		technique_active = false
		technique_progress = 0.0
		technique_direction = 1.0
		_stroke_start = 0.0

	func begin_boundary() -> void:
		travel_since_inversion = 0.0
		peak_speed_since_inversion = 0.0

	func begin_stroke(sign_value: float, intensity: float, target_angle: float, stroke_duration: float, max_velocity: float) -> void:
		technique_active = false
		var sign_normalized := 1.0 if sign_value >= 0.0 else -1.0
		var effective_intensity := clampf(intensity, 0.0, 1.0)
		_stroke_start = displacement
		stroke_target = sign_normalized * target_angle * effective_intensity
		stroke_progress = 0.0
		stroke_active = true
		velocity = sign_normalized * minf(max_velocity, target_angle * effective_intensity / (stroke_duration * 0.5))
		peak_speed_since_inversion = maxf(peak_speed_since_inversion, absf(velocity))

	func begin_technique(sign_value: float) -> void:
		technique_active = true
		technique_progress = 0.0
		technique_direction = 1.0 if sign_value >= 0.0 else -1.0
		stroke_active = false

	func set_technique_progress(progress: float) -> void:
		technique_active = true
		technique_progress = clampf(progress, -1.0, 1.0)
		stroke_active = false

	func end_technique(stroke_duration: float) -> void:
		if not technique_active:
			return
		technique_active = false
		_stroke_start = displacement
		stroke_target = 0.0
		stroke_progress = 0.0
		stroke_active = true
		# Recovery begins from the current pose; there is no snap on release.
		velocity = 0.0

	func step(step_seconds: float, stroke_duration: float, max_angle: float, max_velocity: float) -> void:
		if technique_active:
			var previous_technique := displacement
			var desired_technique := technique_direction * max_angle * technique_progress
			displacement = move_toward(previous_technique, desired_technique, max_velocity * step_seconds)
			velocity = (displacement - previous_technique) / step_seconds
			travel_since_inversion += absf(displacement - previous_technique)
			peak_speed_since_inversion = maxf(peak_speed_since_inversion, absf(velocity))
			return
		if not stroke_active:
			velocity = 0.0
			return

		var previous := displacement
		stroke_progress = minf(1.0, stroke_progress + step_seconds / stroke_duration)
		var desired: float
		if stroke_progress <= 0.5:
			# First half: commit toward the direction of this tap.
			desired = lerpf(_stroke_start, stroke_target, stroke_progress * 2.0)
		else:
			# Second half: complete the stroke by recovering toward centre.
			desired = lerpf(stroke_target, 0.0, (stroke_progress - 0.5) * 2.0)

		desired = clampf(desired, -max_angle, max_angle)
		displacement = move_toward(previous, desired, max_velocity * step_seconds)
		velocity = (displacement - previous) / step_seconds
		travel_since_inversion += absf(displacement - previous)
		peak_speed_since_inversion = maxf(peak_speed_since_inversion, absf(velocity))

		if stroke_progress >= 1.0:
			stroke_active = false
			velocity = 0.0

var horizontal := AxisState.new()
var vertical := AxisState.new()
var elapsed: float = 0.0
var tick: int = 0
var prepared: bool = false

var simulation_hz: float = DEFAULT_SIMULATION_HZ
var step_seconds: float = 1.0 / DEFAULT_SIMULATION_HZ
var stroke_duration: float = DEFAULT_STROKE_DURATION
var target_angle: float = DEFAULT_TARGET_ANGLE
var max_angle: float = DEFAULT_MAX_ANGLE
var max_velocity: float = DEFAULT_MAX_VELOCITY

func configure(values: Dictionary) -> void:
	simulation_hz = maxf(30.0, float(values.get("simulationHz", DEFAULT_SIMULATION_HZ)))
	step_seconds = 1.0 / simulation_hz
	stroke_duration = maxf(0.05, float(values.get("strokeDuration", DEFAULT_STROKE_DURATION)))
	max_angle = maxf(1.0, float(values.get("maxAngle", DEFAULT_MAX_ANGLE)))
	target_angle = clampf(float(values.get("targetAngle", DEFAULT_TARGET_ANGLE)), 1.0, max_angle)
	max_velocity = maxf(1.0, float(values.get("maxVelocity", DEFAULT_MAX_VELOCITY)))

	# Keep the current tick aligned if tuning is hot-reloaded between runs.
	elapsed = tick * step_seconds

func reset() -> void:
	horizontal.reset()
	vertical.reset()
	elapsed = 0.0
	tick = 0
	prepared = false

func advance(authoritative_delta: float) -> int:
	if authoritative_delta <= 0.0:
		return 0
	elapsed += minf(authoritative_delta, MAX_ADVANCE_SECONDS)
	var target_tick := int(floor(elapsed / step_seconds + 0.000000001))
	var steps := 0
	while tick < target_tick:
		horizontal.step(step_seconds, stroke_duration, max_angle, max_velocity)
		vertical.step(step_seconds, stroke_duration, max_angle, max_velocity)
		tick += 1
		steps += 1
	return steps

func capture(direction: StringName) -> Dictionary:
	var axis: AxisState = horizontal if direction in [&"left", &"right"] else vertical
	return {
		"axis": &"horizontal" if direction in [&"left", &"right"] else &"vertical",
		"displacement": axis.displacement,
		"velocity": axis.velocity,
		"travel": axis.travel_since_inversion,
		"peak_speed": axis.peak_speed_since_inversion,
		"stroke_target": axis.stroke_target,
		"stroke_progress": axis.stroke_progress,
		"stroke_active": axis.stroke_active,
		"technique_active": axis.technique_active,
		"technique_progress": axis.technique_progress,
		"prepared": prepared,
		"tick": tick
	}

func apply_bang(direction: StringName, intensity: float = 1.0) -> Dictionary:
	var snapshot := capture(direction)
	var axis: AxisState
	var launch_sign := 1.0
	match direction:
		&"left":
			axis = horizontal
			launch_sign = 1.0
		&"right":
			axis = horizontal
			launch_sign = -1.0
		&"up":
			axis = vertical
			launch_sign = 1.0
		&"down":
			axis = vertical
			launch_sign = -1.0
		_:
			return snapshot
	axis.begin_boundary()
	axis.technique_active = false
	axis.begin_stroke(launch_sign, intensity, target_angle, stroke_duration, max_velocity)
	prepared = true
	return snapshot

func begin_technique(direction: StringName) -> void:
	var axis: AxisState = horizontal if direction in [&"left", &"right"] else vertical
	var sign_value := _direction_sign(direction)
	if sign_value == 0.0:
		return
	axis.begin_boundary()
	axis.begin_technique(sign_value)
	prepared = true

func set_technique_progress(direction: StringName, progress: float) -> void:
	var axis: AxisState = horizontal if direction in [&"left", &"right"] else vertical
	var sign_value := _direction_sign(direction)
	if sign_value == 0.0:
		return
	if not axis.technique_active:
		axis.begin_technique(sign_value)
	axis.set_technique_progress(progress)
	prepared = true

func begin_technique_pose() -> void:
	horizontal.begin_boundary()
	vertical.begin_boundary()
	horizontal.begin_technique(1.0)
	vertical.begin_technique(1.0)
	prepared = true

func set_technique_pose(pose: Vector2) -> void:
	if not horizontal.technique_active:
		horizontal.begin_technique(1.0)
	if not vertical.technique_active:
		vertical.begin_technique(1.0)
	horizontal.set_technique_progress(clampf(pose.x, -1.0, 1.0))
	vertical.set_technique_progress(clampf(pose.y, -1.0, 1.0))
	prepared = true

func end_technique(direction: StringName) -> void:
	var axis: AxisState = horizontal if direction in [&"left", &"right"] else vertical
	axis.end_technique(stroke_duration)

func end_technique_pose() -> void:
	horizontal.end_technique(stroke_duration)
	vertical.end_technique(stroke_duration)

func _direction_sign(direction: StringName) -> float:
	match direction:
		&"left", &"up": return 1.0
		&"right", &"down": return -1.0
		_: return 0.0

func presentation_state() -> Dictionary:
	return {
		"horizontal_angle": horizontal.displacement,
		"vertical_angle": vertical.displacement,
		"horizontal_velocity": horizontal.velocity,
		"vertical_velocity": vertical.velocity,
		"horizontal_stroke_active": horizontal.stroke_active,
		"vertical_stroke_active": vertical.stroke_active,
		"horizontal_technique_active": horizontal.technique_active,
		"vertical_technique_active": vertical.technique_active,
		"prepared": prepared,
		"tick": tick
	}

func tuning_snapshot() -> Dictionary:
	return {
		"simulation_hz": simulation_hz,
		"stroke_duration": stroke_duration,
		"target_angle": target_angle,
		"max_angle": max_angle,
		"max_velocity": max_velocity
	}
