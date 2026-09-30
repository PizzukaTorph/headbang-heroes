extends Control

const CHART_PATH := "res://game/assets/charts/lab-002-tempo-ramp.json"
const TECHNIQUE_CHART_PATH := "res://game/assets/charts/lab-003-technique-lab.json"
const AUDIO_PATH := "res://game/assets/audio/tempo_ramp.wav"
const PROFILE_ORDER := [&"easy", &"normal", &"hard", &"extreme"]

var calibration_ms_value: float = 0.0
var selected_tuning_profile: StringName = &"normal"
var gameplay_view: GameplayView
var gameplay_run: GameplayRun
var _start_button: Button
var _technique_button: Button

func _ready() -> void:
	set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	_show_home()

func _show_home() -> void:
	_clear_screen()
	var background := ColorRect.new()
	background.color = Color("#0b0910")
	background.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(background)

	var stack := VBoxContainer.new()
	stack.anchor_left = 0.08
	stack.anchor_right = 0.92
	stack.anchor_top = 0.11
	stack.anchor_bottom = 0.89
	stack.add_theme_constant_override("separation", 15)
	add_child(stack)

	var title := Label.new()
	title.text = "HEADBANG\nHEROES"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 48)
	stack.add_child(title)

	var subtitle := Label.new()
	subtitle.text = "Godot FLP migration POC\nAudio-clock authoritative · physics-driven Erik"
	subtitle.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	subtitle.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	subtitle.add_theme_font_size_override("font_size", 18)
	stack.add_child(subtitle)

	var profile_caption := Label.new()
	profile_caption.text = "TUNING PROFILE · same authored TempoRamp chart"
	profile_caption.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	profile_caption.modulate = Color(0.75, 0.73, 0.80)
	stack.add_child(profile_caption)

	var profile_row := HBoxContainer.new()
	profile_row.add_theme_constant_override("separation", 6)
	stack.add_child(profile_row)

	var profile_group := ButtonGroup.new()
	for profile in PROFILE_ORDER:
		var button := Button.new()
		button.text = str(profile).to_upper()
		button.toggle_mode = true
		button.button_group = profile_group
		button.button_pressed = profile == selected_tuning_profile
		button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		button.custom_minimum_size.y = 48.0
		button.pressed.connect(_select_tuning_profile.bind(profile))
		profile_row.add_child(button)

	_start_button = Button.new()
	_update_start_button()
	_start_button.custom_minimum_size.y = 72.0
	_start_button.add_theme_font_size_override("font_size", 22)
	_start_button.pressed.connect(_start_game)
	stack.add_child(_start_button)

	_technique_button = Button.new()
	_technique_button.text = "PLAY TECHNIQUE LAB · %s" % str(selected_tuning_profile).to_upper()
	_technique_button.custom_minimum_size.y = 58.0
	_technique_button.pressed.connect(_start_technique_lab)
	stack.add_child(_technique_button)

	var explanation := Label.new()
	explanation.text = "Profiles alter timing/readability only. Neck physics stays identical.\nProduction difficulty remains authored into each chart."
	explanation.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	explanation.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	explanation.add_theme_font_size_override("font_size", 13)
	explanation.modulate = Color(0.70, 0.68, 0.74)
	stack.add_child(explanation)

	var note := Label.new()
	note.text = "Touch: LEFT / UP / DOWN / RIGHT\nKeyboard: arrows/WASD · B THE BANG · [ ] calibration · F3 debug · F4 reload tuning"
	note.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	note.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	stack.add_child(note)

	var source := Label.new()
	source.text = "Included validation track: TempoRamp.wav\nBeyond the Pain chart + MIDI remain bundled as authoring fixtures; licensed/local song audio is intentionally not committed."
	source.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	source.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	source.modulate = Color(0.72, 0.70, 0.76)
	stack.add_child(source)

func _select_tuning_profile(profile: StringName) -> void:
	if not POCTuning.is_valid_profile(profile):
		return
	selected_tuning_profile = profile
	_update_start_button()

func _update_start_button() -> void:
	if _start_button != null:
		_start_button.text = "PLAY TEMPO RAMP · %s" % str(selected_tuning_profile).to_upper()

func _start_technique_lab() -> void:
	_start_game(TECHNIQUE_CHART_PATH)

func _start_game(chart_path: String = CHART_PATH) -> void:
	_clear_screen()
	gameplay_view = GameplayView.new()
	gameplay_view.name = "GameplayView"
	add_child(gameplay_view)

	gameplay_run = GameplayRun.new()
	gameplay_run.name = "GameplayRun"
	add_child(gameplay_run)

	gameplay_view.bang_requested.connect(gameplay_run.bang)
	gameplay_view.the_bang_requested.connect(_activate_the_bang)
	gameplay_view.pause_requested.connect(gameplay_run.toggle_pause)
	gameplay_view.calibration_delta_requested.connect(_change_calibration)
	gameplay_view.reload_tuning_requested.connect(_reload_tuning)
	gameplay_view.technique_gesture_sample.connect(gameplay_run.technique_gesture_sample)

	gameplay_run.cue_changed.connect(gameplay_view.update_cue)
	gameplay_run.hud_changed.connect(gameplay_view.update_hud)
	gameplay_run.judgment_resolved.connect(gameplay_view.show_judgment)
	gameplay_run.free_bang.connect(gameplay_view.show_free_bang)
	gameplay_run.technique_resolved.connect(gameplay_view.show_technique_result)
	gameplay_run.technique_failed.connect(gameplay_view.show_technique_result)
	gameplay_run.pause_changed.connect(gameplay_view.set_paused)
	gameplay_run.run_finished.connect(_show_results)

	if not gameplay_run.configure(chart_path, AUDIO_PATH, selected_tuning_profile):
		_show_error("Could not load the included POC content/profile.")
		return
	gameplay_view.apply_tuning(gameplay_run.presentation_tuning())
	gameplay_run.set_calibration_ms(calibration_ms_value)
	gameplay_run.start_run()

func _activate_the_bang() -> void:
	if gameplay_run != null:
		gameplay_run.try_activate_the_bang()

func _change_calibration(delta_ms: float) -> void:
	calibration_ms_value = clampf(calibration_ms_value + delta_ms, -250.0, 250.0)
	if gameplay_run != null:
		gameplay_run.set_calibration_ms(calibration_ms_value)

func _reload_tuning() -> void:
	if gameplay_run == null or gameplay_view == null:
		return
	if gameplay_run.reload_tuning():
		gameplay_view.apply_tuning(gameplay_run.presentation_tuning())

func _show_results(result: Dictionary) -> void:
	calibration_ms_value = float(result.get("calibration_ms", calibration_ms_value))
	_clear_screen()

	var background := ColorRect.new()
	background.color = Color("#0b0910")
	background.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(background)

	var stack := VBoxContainer.new()
	stack.anchor_left = 0.08
	stack.anchor_right = 0.92
	stack.anchor_top = 0.055
	stack.anchor_bottom = 0.945
	stack.add_theme_constant_override("separation", 9)
	add_child(stack)

	var title := Label.new()
	title.text = "RUN COMPLETE · %s" % str(result.get("tuning_profile", selected_tuning_profile)).to_upper()
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 30)
	stack.add_child(title)

	var score := Label.new()
	score.text = "%09d" % int(result.get("score", 0))
	score.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	score.add_theme_font_size_override("font_size", 42)
	stack.add_child(score)

	var diag: Dictionary = result.get("timing_diagnostics", {})
	var details := Label.new()
	details.text = "PERFECT  %d\nGREAT    %d\nGOOD     %d\nWELL     %d\nMISS     %d\n\nLONGEST COMBO  %d\nHYPE EARNED    %d\nTHE BANG       %d\nFINISHERS      %d\n\nTIMING BIAS   %+.1f ms\nMEAN ABS ERR  %.1f ms\nEARLY / LATE  %d / %d" % [
		int(result.get("perfect", 0)),
		int(result.get("great", 0)),
		int(result.get("good", 0)),
		int(result.get("well", 0)),
		int(result.get("miss", 0)),
		int(result.get("longest_combo", 0)),
		int(result.get("total_hype_earned", 0)),
		int(result.get("the_bang_activations", 0)),
		int(result.get("finishers", 0)),
		float(diag.get("mean_signed_ms", 0.0)),
		float(diag.get("mean_absolute_ms", 0.0)),
		int(diag.get("early", 0)),
		int(diag.get("late", 0))
	]
	details.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	details.add_theme_font_size_override("font_size", 17)
	stack.add_child(details)

	var retry := Button.new()
	retry.text = "RETRY · %s" % str(selected_tuning_profile).to_upper()
	retry.custom_minimum_size.y = 60.0
	retry.pressed.connect(_start_game)
	stack.add_child(retry)

	var home := Button.new()
	home.text = "HOME / CHANGE PROFILE"
	home.custom_minimum_size.y = 52.0
	home.pressed.connect(_show_home)
	stack.add_child(home)

func _show_error(message: String) -> void:
	_clear_screen()
	var label := Label.new()
	label.text = "HEADBANG HEROES\n\n%s" % message
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	label.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(label)

func _clear_screen() -> void:
	for child in get_children():
		remove_child(child)
		child.queue_free()
	_start_button = null
	gameplay_view = null
	gameplay_run = null
