class_name GameplayView
extends Control

signal bang_requested(direction: StringName)
signal the_bang_requested
signal pause_requested
signal calibration_delta_requested(delta_ms: float)
signal reload_tuning_requested
signal technique_gesture_sample(phase: StringName, position: Vector2)

var erik_stage: Control
var erik: ErikView
var cue_ring: CueRing
var technique_guide: TechniqueGestureGuide
var cue_label: Label
var technique_prompt_label: Label
var title_label: Label
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

var _feedback_tuning: Dictionary = {}
var _technique_camera_tuning: Dictionary = {}
var _feedback_tween: Tween
var _erik_tween: Tween
var _hype_tween: Tween
var _technique_camera_tween: Tween
var _debug_visible: bool = true
var _current_cue_debug: Dictionary = {}
var _next_cue_debug: Dictionary = {}
var _last_hype: int = -1
var _mouse_gesture_active: bool = false
var _technique_framing_active: bool = false

func _ready() -> void:
	set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	_debug_visible = OS.is_debug_build()
	_build_ui()

func apply_tuning(values: Dictionary) -> void:
	if erik != null:
		erik.apply_tuning(values)
	if cue_ring != null:
		cue_ring.apply_tuning(values.get("cue", {}))
	if technique_guide != null:
		technique_guide.apply_tuning(values.get("techniques", {}))
	_feedback_tuning = (values.get("feedback", {}) as Dictionary).duplicate(true)
	_technique_camera_tuning = (values.get("techniqueCamera", {}) as Dictionary).duplicate(true)

func update_cue(payload: Dictionary) -> void:
	if payload.is_empty():
		_current_cue_debug = {}
		_next_cue_debug = {}
		cue_label.text = ""
		technique_prompt_label.text = ""
		cue_ring.clear()
		technique_guide.clear()
		return

	var current_event: Dictionary = payload.get("current", {})
	var next_event: Dictionary = payload.get("next", {})
	var song_time := float(payload.get("song_time", 0.0))
	var approach_time := float(payload.get("approach_time", 1.0))
	var preview_horizon := float(payload.get("preview_horizon", approach_time * 2.0))

	_current_cue_debug = _cue_debug_payload(current_event, song_time)
	_next_cue_debug = _cue_debug_payload(next_event, song_time)

	var lines: Array[String] = []
	technique_prompt_label.text = ""
	var guide_event: Dictionary = current_event if _is_technique_event(current_event) else next_event if _is_technique_event(next_event) else {}
	if guide_event.is_empty():
		technique_guide.clear()
		cue_ring.visible = true
	else:
		cue_ring.visible = false
		var guide_time := float(guide_event.get("time", 0.0)) - song_time
		technique_guide.set_guide(StringName(guide_event.get("technique", "")), guide_event == current_event, guide_time)

	if not current_event.is_empty():
		var current_remaining := float(current_event.get("time", 0.0)) - song_time
		lines.append(_cue_line("NOW", current_event, current_remaining))
		_set_technique_prompt(current_event)

	if not next_event.is_empty():
		var next_remaining := float(next_event.get("time", 0.0)) - song_time
		lines.append(_cue_line("NEXT", next_event, next_remaining))
		if not _is_technique_event(current_event) and _is_technique_event(next_event):
			_set_technique_prompt(next_event)

	cue_label.text = "\n".join(lines)
	cue_ring.set_cues(current_event, next_event, song_time, approach_time, preview_horizon)

func update_hud(state: Dictionary) -> void:
	var profile := str(state.get("tuning_profile", "normal")).to_upper()
	title_label.text = "HEADBANG HEROES · %s" % profile

	score_label.text = "SCORE  %09d" % int(state.get("score", 0))
	combo_label.text = "COMBO  %d   x%d" % [int(state.get("combo", 0)), int(state.get("multiplier", 1))]

	var hype := int(state.get("hype", 0))
	hype_label.text = "HYPE  %d/%d%s" % [
		hype,
		int(state.get("hype_max", 100)),
		"   THE BANG!" if bool(state.get("the_bang", false)) else ("   READY" if bool(state.get("the_bang_ready", false)) else "")
	]
	if _last_hype >= 0 and hype > _last_hype:
		_pulse_hype()
	_last_hype = hype

	time_label.text = "%06.2f s" % float(state.get("song_time", 0.0))
	calibration_label.text = "CAL %+.0f ms  ·  OUT %.0f ms" % [
		float(state.get("calibration_ms", 0.0)),
		float(state.get("audio_latency_ms", 0.0))
	]
	bang_button.disabled = not bool(state.get("the_bang_ready", false)) and not bool(state.get("the_bang", false))
	bang_button.text = "THE BANG ACTIVE" if bool(state.get("the_bang", false)) else ("ACTIVATE THE BANG" if bool(state.get("the_bang_ready", false)) else "THE BANG")

	var neck_state: Dictionary = state.get("neck", {})
	erik.apply_neck_state(neck_state, get_process_delta_time())
	var gesture: Dictionary = state.get("gesture", {})
	var technique_state: Dictionary = state.get("technique", {})
	var special_framing := (
		bool(gesture.get("active", false))
		or bool(technique_state.get("active", false))
		or bool(technique_state.get("active_window", false))
		or erik.is_playing_technique()
	)
	_set_technique_framing(special_framing)
	if technique_guide != null and technique_guide.visible:
		technique_guide.set_progress(float(gesture.get("progress", 0.0)), bool(gesture.get("active", false)))
	if bool(gesture.get("active", false)) and StringName(gesture.get("technique", "")) in [&"half", &"deep"]:
		erik.scrub_technique(StringName(gesture.get("technique", "")), float(gesture.get("progress", 0.0)))
	elif not bool(technique_state.get("active", false)):
		erik.end_technique()
	_update_debug(state)

func show_judgment(outcome: Dictionary) -> void:
	var judgment := str(outcome.get("judgment", ""))
	var error_ms := float(outcome.get("error", 0.0)) * 1000.0
	var mq := float(outcome.get("motion_quality", 0.0)) * 100.0
	_show_feedback("%s  %+.0f ms\nMOTION %.0f%%" % [judgment, error_ms, mq], judgment)
	_punch_erik(judgment)

func show_free_bang(_direction: StringName) -> void:
	_show_feedback("BANG", "FREE")

func show_technique_result(result: Dictionary) -> void:
	var event: Dictionary = result.get("event", {})
	var intent: Dictionary = result.get("intent", {})
	var technique := str(event.get("technique", intent.get("technique", "technique"))).to_upper()
	if bool(result.get("valid", false)):
		# HALF/DEEP are scrubbed by live gesture evidence; future techniques may autoplay here.
		if technique not in ["HALF", "DEEP"]:
			erik.play_technique(StringName(event.get("technique", "")))
		_show_feedback("%s  OK" % technique, "GREAT")
	else:
		_show_feedback("%s  FAIL\n%s" % [technique, str(result.get("reason", "gesture"))], "MISS")

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

	title_label = Label.new()
	title_label.text = "HEADBANG HEROES · NORMAL"
	title_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title_label.add_theme_font_size_override("font_size", 20)
	top.add_child(title_label)

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

	erik_stage = Control.new()
	erik_stage.anchor_left = 0.04
	erik_stage.anchor_right = 0.96
	erik_stage.anchor_top = 0.17
	erik_stage.anchor_bottom = 0.59
	erik_stage.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(erik_stage)

	erik = ErikView.new()
	erik.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	erik_stage.add_child(erik)

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

	technique_guide = TechniqueGestureGuide.new()
	technique_guide.anchor_left = 0.5
	technique_guide.anchor_right = 0.5
	technique_guide.anchor_top = 0.54
	technique_guide.anchor_bottom = 0.54
	technique_guide.offset_left = -160.0
	technique_guide.offset_right = 160.0
	technique_guide.offset_top = -160.0
	technique_guide.offset_bottom = 160.0
	technique_guide.visible = false
	add_child(technique_guide)

	cue_label = Label.new()
	cue_label.anchor_left = 0.08
	cue_label.anchor_right = 0.92
	cue_label.anchor_top = 0.655
	cue_label.anchor_bottom = 0.715
	cue_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	cue_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	cue_label.add_theme_font_size_override("font_size", 18)
	add_child(cue_label)

	technique_prompt_label = Label.new()
	technique_prompt_label.anchor_left = 0.12
	technique_prompt_label.anchor_right = 0.88
	technique_prompt_label.anchor_top = 0.595
	technique_prompt_label.anchor_bottom = 0.645
	technique_prompt_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	technique_prompt_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	technique_prompt_label.add_theme_font_size_override("font_size", 24)
	technique_prompt_label.modulate = Color("#ffd166")
	technique_prompt_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(technique_prompt_label)

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
	_add_direction_button(controls, "◀\nLEFT", &"left")
	_add_direction_button(controls, "▲\nUP", &"up")
	_add_direction_button(controls, "▼\nDOWN", &"down")
	_add_direction_button(controls, "▶\nRIGHT", &"right")

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
	if event is InputEventMouseButton:
		var mouse_button := event as InputEventMouseButton
		if mouse_button.button_index == MOUSE_BUTTON_LEFT and technique_guide != null and technique_guide.visible:
			var viewport_size := get_viewport_rect().size
			var in_gameplay_area := mouse_button.position.y >= viewport_size.y * 0.18 and mouse_button.position.y <= viewport_size.y * 0.72
			if mouse_button.pressed and in_gameplay_area:
				_mouse_gesture_active = true
				technique_gesture_sample.emit(&"start", mouse_button.position)
				get_viewport().set_input_as_handled()
			elif not mouse_button.pressed and _mouse_gesture_active:
				_mouse_gesture_active = false
				technique_gesture_sample.emit(&"complete", mouse_button.position)
				get_viewport().set_input_as_handled()
			return
	if event is InputEventMouseMotion and _mouse_gesture_active:
		technique_gesture_sample.emit(&"update", (event as InputEventMouseMotion).position)
		get_viewport().set_input_as_handled()
		return
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

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventScreenTouch:
		var touch := event as InputEventScreenTouch
		technique_gesture_sample.emit(&"start" if touch.pressed else &"complete", touch.position)
	elif event is InputEventScreenDrag:
		technique_gesture_sample.emit(&"update", (event as InputEventScreenDrag).position)

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
	var technique: Dictionary = state.get("technique", {})
	var gesture: Dictionary = state.get("gesture", {})

	debug_label.text = (
		"DEBUG · %s profile · chart %s · F3 hide · F4 reload\n" % [
			str(state.get("tuning_profile", "normal")).to_upper(),
			str(state.get("chart_difficulty", ""))
		]
		+ "t %.3f  cal %+.0fms  out %.0fms\n" % [
			float(state.get("song_time", 0.0)),
			float(state.get("calibration_ms", 0.0)),
			float(state.get("audio_latency_ms", 0.0))
		]
		+ "%s\n%s\n" % [now_text, next_text]
		+ "tech %s %s  gesture %s  X %.2f Y %.2f C %.2f\n" % [
			str(technique.get("current", "—")),
			str(technique.get("current_id", "")),
			str(gesture.get("active", false)),
			float(gesture.get("travel_x", 0.0)),
			float(gesture.get("travel_y", 0.0)),
			float(gesture.get("coherence", 0.0))
		]
		+ "neck H %+.1f° @ %+.1f°/s   V %+.1f° @ %+.1f°/s\n" % [
			float(neck.get("horizontal_angle", 0.0)),
			float(neck.get("horizontal_velocity", 0.0)),
			float(neck.get("vertical_angle", 0.0)),
			float(neck.get("vertical_velocity", 0.0))
		]
		+ "FLP %02d  phase %.2f  travel %.2f°  cap %.1ffps\n" % [
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
		"technique": str(event.get("technique", "classic")),
		"remaining_ms": (float(event.get("time", 0.0)) - song_time) * 1000.0
	}

func _cue_debug_text(prefix: String, cue: Dictionary) -> String:
	if cue.is_empty():
		return "%s —" % prefix
	return "%s %s %s %+.0fms" % [
		prefix,
		str(cue.get("id", "")),
		str(cue.get("technique", "classic")).to_upper() + " " + str(cue.get("direction", "")).to_upper(),
		float(cue.get("remaining_ms", 0.0))
	]

func _cue_line(prefix: String, event: Dictionary, remaining: float) -> String:
	var technique := StringName(event.get("technique", "classic"))
	if technique in [&"half", &"deep"]:
		var glyph := "→" if technique == &"half" else "↓"
		return "%s  %s %s · %s  %+.0f ms" % [prefix, glyph, str(technique).to_upper(), str(event.get("direction", "")).to_upper(), remaining * 1000.0]
	return "%s  %s %s  %+.0f ms" % [prefix, _arrow_for_direction(StringName(event.get("direction", ""))), str(event.get("direction", "")).to_upper(), remaining * 1000.0]

func _set_technique_prompt(event: Dictionary) -> void:
	var technique := StringName(event.get("technique", "classic"))
	if technique == &"half":
		technique_prompt_label.text = "GESTURE  →  HALF  ·  SWIPE RIGHT"
	elif technique == &"deep":
		technique_prompt_label.text = "GESTURE  ↓  DEEP  ·  SWIPE DOWN"

func _is_technique_event(event: Dictionary) -> bool:
	var technique := StringName(event.get("technique", "classic"))
	return technique in [&"half", &"deep"]

func _arrow_for_direction(direction: StringName) -> String:
	match direction:
		&"left": return "◀"
		&"right": return "▶"
		&"up": return "▲"
		&"down": return "▼"
		_: return "•"

func _set_technique_framing(active: bool) -> void:
	if erik_stage == null or _technique_framing_active == active:
		return
	_technique_framing_active = active

	if _technique_camera_tween != null and _technique_camera_tween.is_valid():
		_technique_camera_tween.kill()

	erik_stage.pivot_offset = erik_stage.size * 0.5
	var special_scale := clampf(float(_technique_camera_tuning.get("specialScale", 0.82)), 0.60, 1.0)
	var target_scale := Vector2.ONE * special_scale if active else Vector2.ONE
	var duration := (
		maxf(0.05, float(_technique_camera_tuning.get("pullBackSeconds", 0.13)))
		if active
		else maxf(0.05, float(_technique_camera_tuning.get("returnSeconds", 0.20)))
	)

	_technique_camera_tween = create_tween()
	_technique_camera_tween.tween_property(erik_stage, "scale", target_scale, duration).set_trans(Tween.TRANS_QUAD).set_ease(
		Tween.EASE_OUT if active else Tween.EASE_IN_OUT
	)


func _show_feedback(text_value: String, kind: String) -> void:
	if _feedback_tween != null and _feedback_tween.is_valid():
		_feedback_tween.kill()

	feedback_label.text = text_value
	feedback_label.pivot_offset = feedback_label.size * 0.5
	feedback_label.scale = Vector2.ONE * _feedback_punch(kind)
	feedback_label.modulate = _feedback_color(kind)
	feedback_label.modulate.a = 1.0

	var hold := maxf(0.0, float(_feedback_tuning.get("holdSeconds", 0.16)))
	var fade := maxf(0.05, float(_feedback_tuning.get("fadeSeconds", 0.36)))

	_feedback_tween = create_tween()
	_feedback_tween.tween_property(feedback_label, "scale", Vector2.ONE, 0.10).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	_feedback_tween.tween_interval(hold)
	_feedback_tween.tween_property(feedback_label, "modulate:a", 0.0, fade)

func _punch_erik(judgment: String) -> void:
	if erik == null:
		return
	if _erik_tween != null and _erik_tween.is_valid():
		_erik_tween.kill()

	var punch := _feedback_punch(judgment)
	erik.pivot_offset = erik.size * 0.5
	erik.scale = Vector2.ONE * punch
	if judgment == "MISS":
		erik.modulate = Color(1.0, 0.72, 0.72, 1.0)
	else:
		erik.modulate = Color.WHITE

	var duration := maxf(0.05, float(_feedback_tuning.get("erikPunchSeconds", 0.12)))
	_erik_tween = create_tween().set_parallel(true)
	_erik_tween.tween_property(erik, "scale", Vector2.ONE, duration).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	_erik_tween.tween_property(erik, "modulate", Color.WHITE, duration)

func _pulse_hype() -> void:
	if hype_label == null:
		return
	if _hype_tween != null and _hype_tween.is_valid():
		_hype_tween.kill()
	var pulse := maxf(1.0, float(_feedback_tuning.get("hypePulse", 1.06)))
	hype_label.pivot_offset = hype_label.size * 0.5
	hype_label.scale = Vector2.ONE * pulse
	_hype_tween = create_tween()
	_hype_tween.tween_property(hype_label, "scale", Vector2.ONE, 0.14).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

func _feedback_punch(kind: String) -> float:
	match kind:
		"PERFECT": return float(_feedback_tuning.get("perfectPunch", 1.08))
		"GREAT": return float(_feedback_tuning.get("greatPunch", 1.055))
		"GOOD": return float(_feedback_tuning.get("goodPunch", 1.03))
		"WELL": return float(_feedback_tuning.get("wellPunch", 1.015))
		"MISS": return float(_feedback_tuning.get("missPunch", 0.985))
		_: return 1.0

func _feedback_color(kind: String) -> Color:
	match kind:
		"PERFECT": return Color("#f5d76e")
		"GREAT": return Color("#78ddff")
		"GOOD": return Color("#8ce99a")
		"WELL": return Color("#d4d0dc")
		"MISS": return Color("#ff7272")
		_: return Color(0.88, 0.85, 0.92, 1.0)
