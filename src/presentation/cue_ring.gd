class_name CueRing
extends Control

## POC/Easy readability profile:
## every semantic direction shares one central target.
## CURRENT owns the closing ring; NEXT is a faint preview.
##
## layout_mode can be switched to "directional" from tuning data to spatialize the same
## semantic cues without touching chart/runtime rules.

const DEFAULT_TARGET_RADIUS := 42.0
const DEFAULT_APPROACH_RADIUS := 104.0
const DEFAULT_NEXT_MIN_RADIUS := 122.0
const DEFAULT_NEXT_MAX_RADIUS := 142.0
const DEFAULT_NEXT_ALPHA := 0.28
const DEFAULT_DIRECTIONAL_SPREAD_PX := 82.0

var current_active: bool = false
var next_active: bool = false

var current_progress: float = 0.0
var next_progress: float = 0.0

var current_direction: StringName = &""
var next_direction: StringName = &""

var target_radius: float = DEFAULT_TARGET_RADIUS
var approach_radius: float = DEFAULT_APPROACH_RADIUS
var next_min_radius: float = DEFAULT_NEXT_MIN_RADIUS
var next_max_radius: float = DEFAULT_NEXT_MAX_RADIUS
var next_alpha: float = DEFAULT_NEXT_ALPHA
var layout_mode: StringName = &"centered"
var directional_spread_px: float = DEFAULT_DIRECTIONAL_SPREAD_PX

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE

func apply_tuning(values: Dictionary) -> void:
	target_radius = maxf(8.0, float(values.get("targetRadius", DEFAULT_TARGET_RADIUS)))
	approach_radius = maxf(target_radius + 1.0, float(values.get("approachRadius", DEFAULT_APPROACH_RADIUS)))
	next_min_radius = maxf(approach_radius + 1.0, float(values.get("nextMinRadius", DEFAULT_NEXT_MIN_RADIUS)))
	next_max_radius = maxf(next_min_radius, float(values.get("nextMaxRadius", DEFAULT_NEXT_MAX_RADIUS)))
	next_alpha = clampf(float(values.get("nextAlpha", DEFAULT_NEXT_ALPHA)), 0.05, 0.9)
	var requested_mode := StringName(str(values.get("layoutMode", "centered")).to_lower())
	layout_mode = requested_mode if requested_mode in [&"centered", &"directional"] else &"centered"
	directional_spread_px = maxf(0.0, float(values.get("directionalSpreadPx", DEFAULT_DIRECTIONAL_SPREAD_PX)))
	queue_redraw()

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

	var base_center := size * 0.5
	var current_center := base_center + CueLayout.offset_for(current_direction, layout_mode, directional_spread_px)
	var next_center := base_center + CueLayout.offset_for(next_direction, layout_mode, directional_spread_px)

	# Draw the semantic target where CURRENT belongs. If only NEXT is visible, draw a dim target
	# at its future anchor so directional preview mode remains readable.
	if current_active:
		draw_arc(current_center, target_radius, 0.0, TAU, 64, Color(1.0, 1.0, 1.0, 0.34), 4.0, true)
	elif next_active:
		draw_arc(next_center, target_radius, 0.0, TAU, 64, Color(1.0, 1.0, 1.0, 0.18), 3.0, true)

	if next_active:
		var next_color := _direction_color(next_direction)
		next_color.a = next_alpha
		var next_radius := lerpf(next_max_radius, next_min_radius, next_progress)
		draw_arc(next_center, next_radius, 0.0, TAU, 64, next_color, 4.0, true)

	if current_active:
		var current_color := _direction_color(current_direction)
		var current_radius := lerpf(approach_radius, target_radius, current_progress)
		draw_arc(current_center, current_radius, 0.0, TAU, 64, current_color, 7.0, true)
		draw_circle(current_center, 7.0, current_color)
		_draw_direction(current_center, current_color, current_direction)

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
