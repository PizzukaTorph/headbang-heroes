class_name TechniqueGestureGuide
extends Control

var technique: StringName = &""
var active: bool = false
var remaining_seconds: float = 0.0
var progress: float = 0.0
var half_target_min: float = 0.38
var half_target_max: float = 0.62

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	queue_redraw()

func set_guide(next_technique: StringName, is_active: bool, remaining: float) -> void:
	technique = next_technique if next_technique in [&"half", &"deep"] else &""
	active = is_active
	remaining_seconds = remaining
	progress = 0.0
	visible = technique != &""
	queue_redraw()

func apply_tuning(values: Dictionary) -> void:
	var half: Dictionary = values.get("half", {})
	half_target_min = clampf(float(half.get("targetMin", half_target_min)), 0.0, 1.0)
	half_target_max = clampf(float(half.get("targetMax", half_target_max)), half_target_min, 1.0)
	queue_redraw()

func set_progress(value: float, is_active: bool) -> void:
	progress = clampf(value, 0.0, 1.0)
	active = is_active
	queue_redraw()

func clear() -> void:
	technique = &""
	active = false
	progress = 0.0
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
		var band_start := start.lerp(target, half_target_min)
		var band_end := start.lerp(target, half_target_max)
		draw_rect(Rect2(Vector2(band_start.x, center.y - 22.0), Vector2(band_end.x - band_start.x, 44.0)), Color(1.0, 0.82, 0.40, 0.20))
		draw_line(Vector2(band_start.x, center.y - 28.0), Vector2(band_start.x, center.y + 28.0), color, 3.0, true)
		draw_line(Vector2(band_end.x, center.y - 28.0), Vector2(band_end.x, center.y + 28.0), color, 3.0, true)
		draw_circle(start, 12.0, dim)
		draw_circle(target, 15.0, color, false, 5.0, true)
		draw_line(target, target + Vector2(-22.0, -15.0), color, 7.0, true)
		draw_line(target, target + Vector2(-22.0, 15.0), color, 7.0, true)
		var marker := start.lerp(target, progress)
		draw_line(start, marker, color, 9.0, true)
		draw_circle(marker, 13.0, Color.WHITE if active else color)
		draw_string(ThemeDB.fallback_font, center + Vector2(-50.0, 58.0), "HALF  →", HORIZONTAL_ALIGNMENT_CENTER, 100.0, 25, color)
	else:
		var start := center + Vector2(0.0, -105.0)
		var target := center + Vector2(0.0, 105.0)
		draw_line(start, target, dim, 5.0, true)
		draw_circle(start, 12.0, dim)
		draw_circle(target, 15.0, color, false, 5.0, true)
		draw_line(target, target + Vector2(-15.0, -22.0), color, 7.0, true)
		draw_line(target, target + Vector2(15.0, -22.0), color, 7.0, true)
		var marker := start.lerp(target, progress)
		draw_line(start, marker, color, 9.0, true)
		draw_circle(marker, 13.0, Color.WHITE if active else color)
		draw_string(ThemeDB.fallback_font, center + Vector2(-50.0, 145.0), "DEEP  ↓", HORIZONTAL_ALIGNMENT_CENTER, 100.0, 25, color)
