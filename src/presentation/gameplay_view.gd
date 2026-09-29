class_name GameplayView
extends Control

signal bang_requested(direction: StringName)
signal the_bang_requested
signal pause_requested
signal calibration_delta_requested(delta_ms: float)
signal reload_tuning_requested

var erik: ErikView
var cue_ring: CueRing
var cue_label: Label
var score_label: Label
var combo_label: Label
var hype_label: Label
var time_label: Label
var calibration_label: Label
var feedback_label: Label
var debug_panel: PanelContainer
var debug_label: Label
var bang_button: Button
var pause_button: Button
var _feedback_tween: Tween
var _debug_visible: bool = true
var _current_cue_debug: Dictionary = {}
var _next_cue_debug: Dictionary = {}

func _ready() -> void:
	set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	_debug_visible = OS.is_debug_build()
	_build_ui()

func apply_tuning(values: Dictionary) -> void:
	if erik != null:
		erik.apply_tuning(values.get("flp", {}))
	if cue_ring != null:
		cue_ring.apply_tuning(values.get("cue", {}))

func update_cue(payload: Dictionary) -> void:
	if payload.is_empty():
		_current_cue_debug = {}
		_next_cue_debug = {}
		cue_label.text = ""
		cue_ring.clear()
		return

	var current_event: Dictionary = payload.get("current", {})
	var next_event: Dictionary = payload.get("next", {})
	var song_time := float(payload.get("song_time", 0.0))
	var approach_time := float(payload.get("approach_time", 1.0))
	var preview_horizon := float(payload.get("preview_horizon", approach_time * 2.0))

	_current_cue_debug = _cue_debug_payload(current_event, song_time)
	_next_cue_debug = _cue_debug_payload(next_event, song_time)

	var lines: Array[String] = []

	if not current_event.is_empty():
		var current_remaining := float(current_event.get("time", 0.0)) - song_time
		lines.append(
			"NOW  %s %s  %+.0f ms" % [
				_arrow_for_direction(StringName(current_event.get("direction", ""))),
				str(current_event.get("direction", "")).to_upper(),
				current_remaining * 1000.0
			]
		)

	if not next_event.is_empty():
		var next_remaining := float(next_event.get("time", 0.0)) - song_time
		lines.append(
			"NEXT %s %s  %+.0f ms" % [
				_arrow_for_direction(StringName(next_event.get("direction", ""))),
				str(next_event.get("direction", "")).to_upper(),
				next_remaining * 1000.0
			]
		)

	cue_label.text = "
".join(lines)
	cue_ring.set_cues(current_event, next_event, song_time, approach_time, preview_horizon)

func update_hud(state: Dictionary) -> void:
	score_label.text = "SCORE  %09d" % int(state.get("score", 0))
	combo_label.text = "COMBO  %d   x%d" % [int(state.get("combo", 0)), int(state.get("multiplier", 1))]
	hype_label.text = "HYPE  %d/%d%s" % [
		int(state.get("hype", 0)),
		int(state.get("hype_max", 100)),
		"   THE BANG!" if bool(state.get("the_bang", false)) else ("   READY" if bool(state.get("the_bang_ready", false)) else "")
	]
	time_label.text = "%06.2f s" % float(state.get("song_time", 0.0))
	calibration_label.text = "CAL %+.0f ms  ·  OUT %.0f ms" % [
		float(state.get("calibration_ms", 0.0)),
		float(state.get("audio_latency_ms", 0.0))
	]
	bang_button.disabled = not bool(state.get("the_bang_ready", false)) and not bool(state.get("the_bang", false))
	bang_button.text = "THE BANG ACTIVE" if bool(state.get("the_bang", false)) else ("ACTIVATE THE BANG" if bool(state.get("the_bang_ready", false)) else "THE BANG")

	var neck_state: Dictionary = state.get("neck", {})
	erik.apply_neck_state(neck_state, get_process_delta_time())
	_update_debug(state)

func show_judgment(outcome: Dictionary) -> void:
	var judgment := str(outcome.get("judgment", ""))
	var error_ms := float(outcome.get("error", 0.0)) * 1000.0
	var mq := float(outcome.get("motion_quality", 0.0)) * 100.0
	_show_feedback("%s  %+.0f ms
MOTION %.0f%%" % [judgment, error_ms, mq])

func show_free_bang(_direction: StringName) -> void:
	_show_feedback("BANG")

func _build_ui() -> void:
	var background := ColorRect.new()
	background.color = Color("#0b0910")
	background.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	background.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(background)
	move_child(background, 0)

	var top := VBoxContainer.new()
	top.anchor_left = 0.04
	top.anchor_right = 0.96
	top.anchor_top = 0.025
	top.anchor_bottom = 0.16
	top.add_theme_constant_override("separation", 4)
	add_child(top)

	var title := Label.new()
	title.text = "HEADBANG HEROES · GODOT POC"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 20)
	top.add_child(title)

	score_label = Label.new()
	score_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	score_label.add_theme_font_size_override("font_size", 26)
	top.add_child(score_label)

	combo_label = Label.new()
	combo_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	combo_label.add_theme_font_size_override("font_size", 18)
	top.add_child(combo_label)

	hype_label = Label.new()
	hype_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	top.add_child(hype_label)

	erik = ErikView.new()
	erik.anchor_left = 0.04
	erik.anchor_right = 0.96
	erik.anchor_top = 0.17
	erik.anchor_bottom = 0.59
	add_child(erik)

	debug_panel = PanelContainer.new()
	debug_panel.anchor_left = 0.025
	debug_panel.anchor_right = 0.61
	debug_panel.anchor_top = 0.17
	debug_panel.anchor_bottom = 0.335
	debug_panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	debug_panel.visible = _debug_visible
	var debug_style := StyleBoxFlat.new()
	debug_style.bg_color = Color(0.0, 0.0, 0.0, 0.72)
	debug_style.corner_radius_top_left = 8
	debug_style.corner_radius_top_right = 8
	debug_style.corner_radius_bottom_left = 8
	debug_style.corner_radius_bottom_right = 8
	debug_panel.add_theme_stylebox_override("panel", debug_style)
	add_child(debug_panel)

	debug_label = Label.new()
	debug_label.add_theme_font_size_override("font_size", 11)
	debug_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	debug_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	debug_panel.add_child(debug_label)

	cue_ring = CueRing.new()
	cue_ring.anchor_left = 0.5
	cue_ring.anchor_right = 0.5
	cue_ring.anchor_top = 0.54
	cue_ring.anchor_bottom = 0.54
	cue_ring.offset_left = -155.0
	cue_ring.offset_right = 155.0
	cue_ring.offset_top = -155.0
	cue_ring.offset_bottom = 155.0
	add_child(cue_ring)

	cue_label = Label.new()
	cue_label.anchor_left = 0.08
	cue_label.anchor_right = 0.92
	cue_label.anchor_top = 0.655
	cue_label.anchor_bottom = 0.715
	cue_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	cue_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	cue_label.add_theme_font_size_override("font_size", 18)
	add_child(cue_label)

	feedback_label = Label.new()
	feedback_label.anchor_left = 0.05
	feedback_label.anchor_right = 0.95
	feedback_label.anchor_top = 0.42
	feedback_label.anchor_bottom = 0.52
	feedback_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	feedback_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	feedback_label.add_theme_font_size_override("font_size", 26)
	feedback_label.modulate.a = 0.0
	feedback_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(feedback_label)

	var controls := HBoxContainer.new()
	controls.anchor_left = 0.035
	controls.anchor_right = 0.965
	controls.anchor_top = 0.73
	controls.anchor_bottom = 0.82
	controls.add_theme_constant_override("separation", 8)
	add_child(controls)
	_add_direction_button(controls, "◀
LEFT", &"left")
	_add_direction_button(controls, "▲
UP", &"up")
	_add_direction_button(controls, "▼
DOWN", &"down")
	_add_direction_button(controls, "▶
RIGHT", &"right")

	bang_button = Button.new()
	bang_button.anchor_left = 0.12
	bang_button.anchor_right = 0.88
	bang_button.anchor_top = 0.835
	bang_button.anchor_bottom = 0.89
	bang_button.text = "THE BANG"
	bang_button.disabled = true
	bang_button.pressed.connect(func(): the_bang_requested.emit())
	add_child(bang_button)

	var debug_row := HBoxContainer.new()
	debug_row.anchor_left = 0.04
	debug_row.anchor_right = 0.96
	debug_row.anchor_top = 0.91
	debug_row.anchor_bottom = 0.965
	debug_row.add_theme_constant_override("separation", 8)
	add_child(debug_row)

	var minus := Button.new()
	minus.text = "CAL -5"
	minus.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	minus.pressed.connect(func(): calibration_delta_requested.emit(-5.0))
	debug_row.add_child(minus)

	calibration_label = Label.new()
	calibration_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	calibration_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	calibration_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	debug_row.add_child(calibration_label)

	var plus := Button.new()
	plus.text = "CAL +5"
	plus.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	plus.pressed.connect(func(): calibration_delta_requested.emit(5.0))
	debug_row.add_child(plus)

	pause_button = Button.new()
	pause_button.text = "PAUSE"
	pause_button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	pause_button.pressed.connect(func(): pause_requested.emit())
	debug_row.add_child(pause_button)

	time_label = Label.new()
	time_label.anchor_left = 0.72
	time_label.anchor_right = 0.96
	time_label.anchor_top = 0.175
	time_label.anchor_bottom = 0.205
	time_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	time_label.add_theme_font_size_override("font_size", 13)
	add_child(time_label)

func _add_direction_button(parent: HBoxContainer, text_value: String, direction: StringName) -> void:
	var button := Button.new()
	button.text = text_value
	button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	button.size_flags_vertical = Control.SIZE_EXPAND_FILL
	button.add_theme_font_size_override("font_size", 17)
	button.pressed.connect(_on_direction_pressed.bind(direction))
	parent.add_child(button)

func _on_direction_pressed(direction: StringName) -> void:
	bang_requested.emit(direction)

func _input(event: InputEvent) -> void:
	if event is not InputEventKey:
		return
	var key := event as InputEventKey
	if not key.pressed or key.echo:
		return
	match key.keycode:
		KEY_LEFT, KEY_A:
			_on_direction_pressed(&"left")
		KEY_RIGHT, KEY_D:
			_on_direction_pressed(&"right")
		KEY_UP, KEY_W:
			_on_direction_pressed(&"up")
		KEY_DOWN, KEY_S:
			_on_direction_pressed(&"down")
		KEY_B:
			the_bang_requested.emit()
		KEY_BRACKETLEFT:
			calibration_delta_requested.emit(-5.0)
		KEY_BRACKETRIGHT:
			calibration_delta_requested.emit(5.0)
		KEY_P:
			pause_requested.emit()
		KEY_F3:
			_debug_visible = not _debug_visible
			debug_panel.visible = _debug_visible
		KEY_F4:
			reload_tuning_requested.emit()

func set_paused(value: bool) -> void:
	pause_button.text = "RESUME" if value else "PAUSE"

func _update_debug(state: Dictionary) -> void:
	if debug_label == null or not _debug_visible:
		return

	var neck: Dictionary = state.get("neck", {})
	var diag: Dictionary = state.get("timing_diagnostics", {})
	var flp := erik.debug_snapshot()

	var now_text := _cue_debug_text("NOW", _current_cue_debug)
	var next_text := _cue_debug_text("NEXT", _next_cue_debug)

	debug_label.text = (
		"DEBUG · F3 hide · F4 reload tuning
"
		+ "t %.3f  cal %+.0fms  out %.0fms
" % [
			float(state.get("song_time", 0.0)),
			float(state.get("calibration_ms", 0.0)),
			float(state.get("audio_latency_ms", 0.0))
		]
		+ "%s
%s
" % [now_text, next_text]
		+ "neck H %+.1f° @ %+.1f°/s   V %+.1f° @ %+.1f°/s
" % [
			float(neck.get("horizontal_angle", 0.0)),
			float(neck.get("horizontal_velocity", 0.0)),
			float(neck.get("vertical_angle", 0.0)),
			float(neck.get("vertical_velocity", 0.0))
		]
		+ "FLP %02d  phase %.2f  travel %.2f°  cap %.1ffps
" % [
			int(flp.get("frame", 0)),
			float(flp.get("phase", 0.0)),
			float(flp.get("angular_travel", 0.0)),
			float(flp.get("max_visual_fps", 0.0))
		]
		+ "timing n=%d  bias %+.1fms  abs %.1fms  E/L %d/%d  last %s %+.1fms" % [
			int(diag.get("samples", 0)),
			float(diag.get("mean_signed_ms", 0.0)),
			float(diag.get("mean_absolute_ms", 0.0)),
			int(diag.get("early", 0)),
			int(diag.get("late", 0)),
			str(diag.get("last_judgment", "")),
			float(diag.get("last_error_ms", 0.0))
		]
	)

func _cue_debug_payload(event: Dictionary, song_time: float) -> Dictionary:
	if event.is_empty():
		return {}
	return {
		"id": str(event.get("id", "")),
		"direction": str(event.get("direction", "")),
		"remaining_ms": (float(event.get("time", 0.0)) - song_time) * 1000.0
	}

func _cue_debug_text(prefix: String, cue: Dictionary) -> String:
	if cue.is_empty():
		return "%s —" % prefix
	return "%s %s %s %+.0fms" % [
		prefix,
		str(cue.get("id", "")),
		str(cue.get("direction", "")).to_upper(),
		float(cue.get("remaining_ms", 0.0))
	]

func _arrow_for_direction(direction: StringName) -> String:
	match direction:
		&"left": return "◀"
		&"right": return "▶"
		&"up": return "▲"
		&"down": return "▼"
		_: return "•"

func _show_feedback(text_value: String) -> void:
	if _feedback_tween != null and _feedback_tween.is_valid():
		_feedback_tween.kill()
	feedback_label.text = text_value
	feedback_label.modulate.a = 1.0
	_feedback_tween = create_tween()
	_feedback_tween.tween_interval(0.18)
	_feedback_tween.tween_property(feedback_label, "modulate:a", 0.0, 0.42)
