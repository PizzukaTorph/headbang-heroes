class_name CueRing
extends Control

var active: bool = false
var progress: float = 0.0
var direction: StringName = &""
var target_radius: float = 42.0
var approach_radius: float = 102.0

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE

func set_cue(event: Dictionary, song_time: float, approach_time: float) -> void:
	if event.is_empty():
		active = false
		queue_redraw()
		return
	active = true
	direction = StringName(event.get("direction", ""))
	var remaining := float(event.get("time", 0.0)) - song_time
	progress = clampf(1.0 - remaining / maxf(0.001, approach_time), 0.0, 1.0)
	queue_redraw()

func _draw() -> void:
	if not active:
		return
	var center := size * 0.5
	var accent := _direction_color()
	var radius := lerpf(approach_radius, target_radius, progress)
	draw_arc(center, target_radius, 0.0, TAU, 64, Color(1.0, 1.0, 1.0, 0.42), 5.0, true)
	draw_arc(center, radius, 0.0, TAU, 64, accent, 7.0, true)
	draw_circle(center, 7.0, accent)
	_draw_direction(center, accent)

func _direction_color() -> Color:
	match direction:
		&"left": return Color(0.80, 0.48, 1.0, 1.0)
		&"right": return Color(0.35, 0.82, 1.0, 1.0)
		&"up": return Color(0.45, 1.0, 0.66, 1.0)
		&"down": return Color(1.0, 0.48, 0.42, 1.0)
		_: return Color.WHITE

func _draw_direction(center: Vector2, color: Color) -> void:
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
