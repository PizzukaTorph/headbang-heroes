class_name TechniqueGestureGuide
extends Control

var technique: StringName = &""
var active: bool = false
var remaining_seconds: float = 0.0

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	queue_redraw()

func set_guide(next_technique: StringName, is_active: bool, remaining: float) -> void:
	technique = next_technique if next_technique in [&"half", &"deep"] else &""
	active = is_active
	remaining_seconds = remaining
	visible = technique != &""
	queue_redraw()

func clear() -> void:
	technique = &""
	active = false
	visible = false
	queue_redraw()

func _draw() -> void:
	if technique == &"":
		return
	var center := size * 0.5
	var color := Color("#ffd166") if active else Color(1.0, 0.82, 0.40, 0.50)
	var dim := Color(1.0, 1.0, 1.0, 0.24)
	if technique == &"half":
		var start := center + Vector2(-95.0, 0.0)
		var target := center + Vector2(95.0, 0.0)
		draw_line(start, target, dim, 5.0, true)
		draw_circle(start, 12.0, dim)
		draw_circle(target, 15.0, color, false, 5.0, true)
		draw_line(target, target + Vector2(-22.0, -15.0), color, 7.0, true)
		draw_line(target, target + Vector2(-22.0, 15.0), color, 7.0, true)
		draw_string(ThemeDB.fallback_font, center + Vector2(-50.0, 58.0), "HALF  →", HORIZONTAL_ALIGNMENT_CENTER, 100.0, 25, color)
	else:
		var start := center + Vector2(0.0, -105.0)
		var target := center + Vector2(0.0, 105.0)
		draw_line(start, target, dim, 5.0, true)
		draw_circle(start, 12.0, dim)
		draw_circle(target, 15.0, color, false, 5.0, true)
		draw_line(target, target + Vector2(-15.0, -22.0), color, 7.0, true)
		draw_line(target, target + Vector2(15.0, -22.0), color, 7.0, true)
		draw_string(ThemeDB.fallback_font, center + Vector2(-50.0, 145.0), "DEEP  ↓", HORIZONTAL_ALIGNMENT_CENTER, 100.0, 25, color)
