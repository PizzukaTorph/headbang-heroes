class_name NeckMotionState
extends RefCounted

const DEFAULT_SIMULATION_HZ := 120.0
const DEFAULT_IMPULSE := 190.0
const DEFAULT_DAMPING := 5.5
const DEFAULT_RETURN_STRENGTH := 9.0
const DEFAULT_MAX_ANGLE := 42.0
const DEFAULT_MAX_VELOCITY := 360.0
const DEFAULT_LIMIT_BOUNCE := -0.15
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

	func apply_impulse(sign_value: float, intensity: float, impulse: float, max_velocity: float) -> void:
		var sign_normalized := 1.0 if sign_value >= 0.0 else -1.0
		velocity += sign_normalized * impulse * clampf(intensity, 0.0, 1.0)
		velocity = clampf(velocity, -max_velocity, max_velocity)
		peak_speed_since_inversion = maxf(peak_speed_since_inversion, absf(velocity))

	func step(
		step_seconds: float,
		return_strength: float,
		damping: float,
		max_angle: float,
		limit_bounce: float
	) -> void:
		var previous := displacement
		velocity += -displacement * return_strength * step_seconds
		velocity *= exp(-damping * step_seconds)
		displacement += velocity * step_seconds
		if absf(displacement) > max_angle:
			displacement = clampf(displacement, -max_angle, max_angle)
			velocity *= limit_bounce
		travel_since_inversion += absf(displacement - previous)
		peak_speed_since_inversion = maxf(peak_speed_since_inversion, absf(velocity))

var horizontal := AxisState.new()
var vertical := AxisState.new()
var elapsed: float = 0.0
var tick: int = 0
var prepared: bool = false

var simulation_hz: float = DEFAULT_SIMULATION_HZ
var step_seconds: float = 1.0 / DEFAULT_SIMULATION_HZ
var impulse: float = DEFAULT_IMPULSE
var damping: float = DEFAULT_DAMPING
var return_strength: float = DEFAULT_RETURN_STRENGTH
var max_angle: float = DEFAULT_MAX_ANGLE
var max_velocity: float = DEFAULT_MAX_VELOCITY
var limit_bounce: float = DEFAULT_LIMIT_BOUNCE

func configure(values: Dictionary) -> void:
	simulation_hz = maxf(30.0, float(values.get("simulationHz", DEFAULT_SIMULATION_HZ)))
	step_seconds = 1.0 / simulation_hz
	impulse = maxf(1.0, float(values.get("impulse", DEFAULT_IMPULSE)))
	damping = maxf(0.0, float(values.get("damping", DEFAULT_DAMPING)))
	return_strength = maxf(0.0, float(values.get("returnStrength", DEFAULT_RETURN_STRENGTH)))
	max_angle = maxf(1.0, float(values.get("maxAngle", DEFAULT_MAX_ANGLE)))
	max_velocity = maxf(1.0, float(values.get("maxVelocity", DEFAULT_MAX_VELOCITY)))
	limit_bounce = clampf(float(values.get("limitBounce", DEFAULT_LIMIT_BOUNCE)), -1.0, 1.0)

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
		horizontal.step(step_seconds, return_strength, damping, max_angle, limit_bounce)
		vertical.step(step_seconds, return_strength, damping, max_angle, limit_bounce)
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
	axis.apply_impulse(launch_sign, intensity, impulse, max_velocity)
	prepared = true
	return snapshot

func presentation_state() -> Dictionary:
	return {
		"horizontal_angle": horizontal.displacement,
		"vertical_angle": vertical.displacement,
		"horizontal_velocity": horizontal.velocity,
		"vertical_velocity": vertical.velocity,
		"prepared": prepared,
		"tick": tick
	}

func tuning_snapshot() -> Dictionary:
	return {
		"simulation_hz": simulation_hz,
		"impulse": impulse,
		"damping": damping,
		"return_strength": return_strength,
		"max_angle": max_angle,
		"max_velocity": max_velocity,
		"limit_bounce": limit_bounce
	}
