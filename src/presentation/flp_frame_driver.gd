class_name FLPFrameDriver
extends RefCounted

## Converts authoritative neck movement into a discrete FLP animation phase.
## It is presentation-only: gameplay never reads frame/phase state.

const DEFAULT_FRAME_COUNT := 16
const DEFAULT_FRAMES_PER_DEGREE := 0.12
const DEFAULT_MAX_VISUAL_FPS := 9.0
const DEFAULT_SETTLED_ANGLE_DEG := 1.5
const DEFAULT_SETTLED_SPEED_DEG_PER_SEC := 10.0

var frame_count: int = DEFAULT_FRAME_COUNT
var frames_per_degree: float = DEFAULT_FRAMES_PER_DEGREE
var max_visual_fps: float = DEFAULT_MAX_VISUAL_FPS
var settled_angle_deg: float = DEFAULT_SETTLED_ANGLE_DEG
var settled_speed_deg_per_sec: float = DEFAULT_SETTLED_SPEED_DEG_PER_SEC

var phase_frames: float = 0.0
var current_frame: int = 0

var _previous_angle := Vector2.ZERO
var _initialized: bool = false
var _last_angular_travel: float = 0.0

func _init(p_frame_count: int = DEFAULT_FRAME_COUNT) -> void:
	frame_count = maxi(1, p_frame_count)

func configure(values: Dictionary) -> void:
	frames_per_degree = maxf(0.001, float(values.get("framesPerDegree", DEFAULT_FRAMES_PER_DEGREE)))
	max_visual_fps = maxf(1.0, float(values.get("maxVisualFps", DEFAULT_MAX_VISUAL_FPS)))
	settled_angle_deg = maxf(0.0, float(values.get("settledAngleDeg", DEFAULT_SETTLED_ANGLE_DEG)))
	settled_speed_deg_per_sec = maxf(0.0, float(values.get("settledSpeedDegPerSec", DEFAULT_SETTLED_SPEED_DEG_PER_SEC)))

func reset() -> void:
	phase_frames = 0.0
	current_frame = 0
	_previous_angle = Vector2.ZERO
	_initialized = false
	_last_angular_travel = 0.0

func update(neck_state: Dictionary, delta: float) -> int:
	var angle := Vector2(
		float(neck_state.get("horizontal_angle", 0.0)),
		float(neck_state.get("vertical_angle", 0.0))
	)
	var velocity := Vector2(
		float(neck_state.get("horizontal_velocity", 0.0)),
		float(neck_state.get("vertical_velocity", 0.0))
	)

	if not _initialized:
		_previous_angle = angle
		_initialized = true

	if angle.length() <= settled_angle_deg and velocity.length() <= settled_speed_deg_per_sec:
		phase_frames = 0.0
		current_frame = 0
		_previous_angle = angle
		_last_angular_travel = 0.0
		return current_frame

	var angular_travel := (angle - _previous_angle).length()
	_last_angular_travel = angular_travel
	_previous_angle = angle

	var requested_advance := angular_travel * frames_per_degree
	var max_advance := max_visual_fps * maxf(0.0, delta)
	phase_frames += minf(requested_advance, max_advance)

	if phase_frames >= float(frame_count):
		phase_frames = fmod(phase_frames, float(frame_count))

	current_frame = clampi(int(floor(phase_frames)), 0, frame_count - 1)
	return current_frame

func debug_snapshot() -> Dictionary:
	return {
		"frame": current_frame,
		"phase": phase_frames,
		"angular_travel": _last_angular_travel,
		"frames_per_degree": frames_per_degree,
		"max_visual_fps": max_visual_fps
	}
