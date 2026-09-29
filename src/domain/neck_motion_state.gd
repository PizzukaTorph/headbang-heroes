class_name NeckMotionState
extends RefCounted

const STEP_SECONDS := 1.0 / 120.0
const IMPULSE := 190.0
const DAMPING := 5.5
const RETURN_STRENGTH := 9.0
const MAX_ANGLE := 42.0
const MAX_VELOCITY := 360.0
const LIMIT_BOUNCE := -0.15
const MAX_ADVANCE_SECONDS := 0.25

class AxisState:
	var displacement: float = 0.0
	var velocity: float = 0.0
	var travel_since_inversion: float = 0.0
	var peak_speed_since_inversion: float = 0.0

	func reset() -> void:
		displacement = 0.0
		velocity = 0.0
		travel_since_inversion = 0.0
		peak_speed_since_inversion = 0.0

	func begin_boundary() -> void:
		travel_since_inversion = 0.0
		peak_speed_since_inversion = 0.0

	func apply_impulse(sign_value: float, intensity: float) -> void:
		var sign_normalized := 1.0 if sign_value >= 0.0 else -1.0
		velocity += sign_normalized * IMPULSE * clampf(intensity, 0.0, 1.0)
		velocity = clampf(velocity, -MAX_VELOCITY, MAX_VELOCITY)
		peak_speed_since_inversion = maxf(peak_speed_since_inversion, absf(velocity))

	func step() -> void:
		var previous := displacement
		velocity += -displacement * RETURN_STRENGTH * STEP_SECONDS
		velocity *= exp(-DAMPING * STEP_SECONDS)
		displacement += velocity * STEP_SECONDS
		if absf(displacement) > MAX_ANGLE:
			displacement = clampf(displacement, -MAX_ANGLE, MAX_ANGLE)
			velocity *= LIMIT_BOUNCE
		travel_since_inversion += absf(displacement - previous)
		peak_speed_since_inversion = maxf(peak_speed_since_inversion, absf(velocity))

var horizontal := AxisState.new()
var vertical := AxisState.new()
var elapsed: float = 0.0
var tick: int = 0
var prepared: bool = false

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
	var target_tick := int(floor(elapsed / STEP_SECONDS + 0.000000001))
	var steps := 0
	while tick < target_tick:
		horizontal.step()
		vertical.step()
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
	axis.apply_impulse(launch_sign, intensity)
	prepared = true
	return snapshot

func presentation_state() -> Dictionary:
	return {
		"horizontal_angle": horizontal.displacement,
		"vertical_angle": vertical.displacement,
		"horizontal_velocity": horizontal.velocity,
		"vertical_velocity": vertical.velocity,
		"prepared": prepared
	}
