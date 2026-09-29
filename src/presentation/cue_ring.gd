class_name CueRing
extends Control

## POC/Easy readability profile:
## every semantic direction shares one central target.
## CURRENT owns the closing ring; NEXT is a faint, larger concentric preview.
## Future difficulty profiles may spatialize the cue anchor without changing chart semantics.

var current_active: bool = false
var next_active: bool = false

var current_progress: float = 0.0
var next_progress: float = 0.0

var current_direction: StringName = &""
var next_direction: StringName = &""

var target_radius: float = 42.0
var approach_radius: float = 104.0
var next_min_radius: float = 122.0
var next_max_radius: float = 142.0

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE

func set_cues(
	current_event: Dictionary,
	next_event: Dictionary,
	song_time: float,
	approach_time: float,
	preview_horizon: float
) -> void:
	current_active = not current_event.is_empty()
	next_active = not next_event.is_empty()

	if current_active:
		current_direction = StringName(current_event.get("direction", ""))
		var current_remaining := float(current_event.get("time", 0.0)) - song_time
		current_progress = clampf(
			1.0 - current_remaining / maxf(0.001, approach_time),
			0.0,
			1.0
		)
	else:
		current_direction = &""
		current_progress = 0.0

	if next_active:
		next_direction = StringName(next_event.get("direction", ""))
		var next_remaining := float(next_event.get("time", 0.0)) - song_time
		next_progress = clampf(
			1.0 - next_remaining / maxf(0.001, preview_horizon),
			0.0,
			1.0
		)
	else:
		next_direction = &""
		next_progress = 0.0

	queue_redraw()

func clear() -> void:
	current_active = false
	next_active = false
	current_direction = &""
	next_direction = &""
	queue_redraw()

func _draw() -> void:
	if not current_active and not next_active:
		return

	var center := size * 0.5

	# One semantic target, independent from direction. This is the intentionally easy/readable
	# profile. LEFT/RIGHT/UP/DOWN are still distinct inputs, only their visual target is shared.
	draw_arc(center, target_radius, 0.0, TAU, 64, Color(1.0, 1.0, 1.0, 0.34), 4.0, true)

	# NEXT never competes with CURRENT for the target. It stays outside as a low-alpha concentric
	# preview and communicates its direction primarily through color + the textual NEXT label.
	if next_active:
		var next_color := _direction_color(next_direction)
		next_color.a = 0.28
		var next_radius := lerpf(next_max_radius, next_min_radius, next_progress)
		draw_arc(center, next_radius, 0.0, TAU, 64, next_color, 4.0, true)

	if current_active:
		var current_color := _direction_color(current_direction)
		var current_radius := lerpf(approach_radius, target_radius, current_progress)
		draw_arc(center, current_radius, 0.0, TAU, 64, current_color, 7.0, true)
		draw_circle(center, 7.0, current_color)
		_draw_direction(center, current_color, current_direction)

func _direction_color(direction: StringName) -> Color:
	match direction:
		&"left": return Color(0.80, 0.48, 1.0, 1.0)
		&"right": return Color(0.35, 0.82, 1.0, 1.0)
		&"up": return Color(0.45, 1.0, 0.66, 1.0)
		&"down": return Color(1.0, 0.48, 0.42, 1.0)
		_: return Color.WHITE

func _draw_direction(center: Vector2, color: Color, direction: StringName) -> void:
	var d := Vector2.ZERO
	match direction:
		&"left": d = Vector2.LEFT
		&"right": d = Vector2.RIGHT
		&"up": d = Vector2.UP
		&"down": d = Vector2.DOWN
		_: return
	var start := center - d * 11.0
	var tip := center + d * 26.0
	draw_line(start, tip, color, 6.0, true)
	var side := d.rotated(PI * 0.5)
	draw_line(tip, tip - d * 13.0 + side * 10.0, color, 5.0, true)
	draw_line(tip, tip - d * 13.0 - side * 10.0, color, 5.0, true)
