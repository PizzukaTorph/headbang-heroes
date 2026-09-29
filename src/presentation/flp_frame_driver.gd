class_name FLPFrameDriver
extends RefCounted

## Converts authoritative neck movement into a discrete FLP animation phase.
##
## The animation is NOT a gameplay clock and does not advance on an independent timer.
## Real angular travel advances the authored frame cycle; a visual-rate cap prevents a
## high neck velocity from turning the sequence into an unreadable GIF.
##
## Consequences:
## - stronger/faster physical motion advances the sequence faster;
## - the sequence naturally dwells near physical extrema as the neck slows;
## - when the neck physically settles near neutral, presentation returns to frame 0;
## - timing/scoring never depend on this presentation state.

const DEFAULT_FRAME_COUNT := 16
const FRAMES_PER_DEGREE := 0.12
const MAX_VISUAL_FPS := 9.0
const SETTLED_ANGLE_DEG := 1.5
const SETTLED_SPEED_DEG_PER_SEC := 10.0

var frame_count: int = DEFAULT_FRAME_COUNT
var phase_frames: float = 0.0
var current_frame: int = 0

var _previous_angle := Vector2.ZERO
var _initialized: bool = false

func _init(p_frame_count: int = DEFAULT_FRAME_COUNT) -> void:
	frame_count = maxi(1, p_frame_count)

func reset() -> void:
	phase_frames = 0.0
	current_frame = 0
	_previous_angle = Vector2.ZERO
	_initialized = false

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

	# Presentation may return to the neutral authored frame only when the physical neck
	# is actually close to neutral and has almost stopped. This never mutates gameplay.
	if angle.length() <= SETTLED_ANGLE_DEG and velocity.length() <= SETTLED_SPEED_DEG_PER_SEC:
		phase_frames = 0.0
		current_frame = 0
		_previous_angle = angle
		return current_frame

	var angular_travel := (angle - _previous_angle).length()
	_previous_angle = angle

	# Physical travel is the primary driver. The FPS term is only a readability ceiling:
	# even a large/hitch-delivered physical step cannot skip through the authored art too fast.
	var requested_advance := angular_travel * FRAMES_PER_DEGREE
	var max_advance := MAX_VISUAL_FPS * maxf(0.0, delta)
	phase_frames += minf(requested_advance, max_advance)

	if phase_frames >= float(frame_count):
		phase_frames = fmod(phase_frames, float(frame_count))

	current_frame = clampi(int(floor(phase_frames)), 0, frame_count - 1)
	return current_frame
