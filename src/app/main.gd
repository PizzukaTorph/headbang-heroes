extends Control

const CHART_PATH := "res://game/assets/charts/lab-002-tempo-ramp.json"
const AUDIO_PATH := "res://game/assets/audio/tempo_ramp.wav"

var calibration_ms_value: float = 0.0
var gameplay_view: GameplayView
var gameplay_run: GameplayRun

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
	stack.anchor_top = 0.18
	stack.anchor_bottom = 0.82
	stack.add_theme_constant_override("separation", 18)
	add_child(stack)

	var title := Label.new()
	title.text = "HEADBANG\nHEROES"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 48)
	stack.add_child(title)

	var subtitle := Label.new()
	subtitle.text = "Godot FLP migration POC\nAudio-clock authoritative · frame-based Erik"
	subtitle.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	subtitle.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	subtitle.add_theme_font_size_override("font_size", 18)
	stack.add_child(subtitle)

	var spacer := Control.new()
	spacer.custom_minimum_size.y = 34.0
	stack.add_child(spacer)

	var start := Button.new()
	start.text = "PLAY TEMPO RAMP"
	start.custom_minimum_size.y = 72.0
	start.add_theme_font_size_override("font_size", 22)
	start.pressed.connect(_start_game)
	stack.add_child(start)

	var note := Label.new()
	note.text = "Touch: LEFT / UP / DOWN / RIGHT\nKeyboard: arrows or WASD · B = THE BANG · [ ] = calibration"
	note.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	note.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	stack.add_child(note)

	var source := Label.new()
	source.text = "Included validation track: TempoRamp.wav\nBeyond the Pain chart + MIDI remain bundled as authoring fixtures; licensed/local song audio is intentionally not committed."
	source.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	source.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	source.modulate = Color(0.72, 0.70, 0.76)
	stack.add_child(source)

func _start_game() -> void:
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

	gameplay_run.cue_changed.connect(gameplay_view.update_cue)
	gameplay_run.hud_changed.connect(gameplay_view.update_hud)
	gameplay_run.judgment_resolved.connect(gameplay_view.show_judgment)
	gameplay_run.free_bang.connect(gameplay_view.show_free_bang)
	gameplay_run.pause_changed.connect(gameplay_view.set_paused)
	gameplay_run.run_finished.connect(_show_results)

	if not gameplay_run.configure(CHART_PATH, AUDIO_PATH):
		_show_error("Could not load the included POC content.")
		return
	gameplay_run.set_calibration_ms(calibration_ms_value)
	gameplay_run.start_run()

func _activate_the_bang() -> void:
	if gameplay_run != null:
		gameplay_run.try_activate_the_bang()

func _change_calibration(delta_ms: float) -> void:
	calibration_ms_value = clampf(calibration_ms_value + delta_ms, -250.0, 250.0)
	if gameplay_run != null:
		gameplay_run.set_calibration_ms(calibration_ms_value)

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
	stack.anchor_top = 0.08
	stack.anchor_bottom = 0.92
	stack.add_theme_constant_override("separation", 12)
	add_child(stack)

	var title := Label.new()
	title.text = "RUN COMPLETE"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 34)
	stack.add_child(title)

	var score := Label.new()
	score.text = "%09d" % int(result.get("score", 0))
	score.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	score.add_theme_font_size_override("font_size", 42)
	stack.add_child(score)

	var details := Label.new()
	details.text = "PERFECT  %d\nGREAT    %d\nGOOD     %d\nWELL     %d\nMISS     %d\n\nLONGEST COMBO  %d\nHYPE EARNED    %d\nTHE BANG       %d\nFINISHERS      %d" % [
		int(result.get("perfect", 0)),
		int(result.get("great", 0)),
		int(result.get("good", 0)),
		int(result.get("well", 0)),
		int(result.get("miss", 0)),
		int(result.get("longest_combo", 0)),
		int(result.get("total_hype_earned", 0)),
		int(result.get("the_bang_activations", 0)),
		int(result.get("finishers", 0))
	]
	details.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	details.add_theme_font_size_override("font_size", 18)
	stack.add_child(details)

	var retry := Button.new()
	retry.text = "RETRY"
	retry.custom_minimum_size.y = 64.0
	retry.pressed.connect(_start_game)
	stack.add_child(retry)

	var home := Button.new()
	home.text = "HOME"
	home.custom_minimum_size.y = 56.0
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
	gameplay_view = null
	gameplay_run = null
