class_name ErikView
extends TextureRect

const FRAME_COUNT := 16
const FRAME_ROOT := "res://game/assets/characters/erik/headbang/headbang_%02d.png"
const TECHNIQUE_ROOT := "res://game/assets/characters/erik/techniques"
const TECHNIQUE_FOLDERS := {
	&"half": "half",
	&"deep": "deep",
	&"whiplash": "whiplash",
	&"windmill": "windmill"
}
const DEFAULT_TECHNIQUE_FPS := 12.0

var frames: Array[Texture2D] = []
var technique_frames: Dictionary = {}
var technique_fps: Dictionary = {}
var technique_playing: StringName = &""
var _technique_scrubbed: bool = false
var frame_driver := FLPFrameDriver.new(FRAME_COUNT)
var _last_frame: int = -1
var _technique_elapsed: float = 0.0
var _last_neck_state: Dictionary = {}

func _ready() -> void:
	expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	mouse_filter = Control.MOUSE_FILTER_IGNORE

	for i in FRAME_COUNT:
		var texture_resource := ResourceLoader.load(FRAME_ROOT % i)
		if texture_resource is Texture2D:
			frames.append(texture_resource as Texture2D)
	_load_technique_frames()
	set_process(true)

	reset_pose()

func apply_tuning(values: Dictionary) -> void:
	var flp_values: Dictionary = values.get("flp", values)
	frame_driver.configure(flp_values)
	var animation_values: Dictionary = values.get("techniqueAnimation", {})
	for technique in TECHNIQUE_FOLDERS.keys():
		technique_fps[technique] = maxf(1.0, float(animation_values.get("%sFps" % str(technique), DEFAULT_TECHNIQUE_FPS)))

func reset_pose() -> void:
	frame_driver.reset()
	technique_playing = &""
	_technique_scrubbed = false
	scale = Vector2.ONE
	_last_frame = -1
	_set_frame(0)

func apply_neck_state(neck_state: Dictionary, delta: float) -> void:
	if frames.is_empty():
		return
	_last_neck_state = neck_state.duplicate(true)
	var frame := frame_driver.update(neck_state, delta)
	if technique_playing == &"":
		_set_frame(frame)

func play_technique(technique: StringName) -> bool:
	var selected: Array = technique_frames.get(technique, [])
	if selected.is_empty():
		return false
	technique_playing = technique
	_technique_scrubbed = false
	_technique_elapsed = 0.0
	_last_frame = -1
	_set_technique_frame(0)
	return true

func scrub_technique(technique: StringName, progress: float) -> bool:
	var selected: Array = technique_frames.get(technique, [])
	if selected.is_empty():
		return false
	technique_playing = technique
	_technique_scrubbed = true
	_technique_elapsed = 0.0
	_last_frame = -1
	_set_technique_frame(roundi(clampf(progress, 0.0, 1.0) * float(selected.size() - 1)))
	return true

func end_technique() -> void:
	if technique_playing == &"" or not _technique_scrubbed:
		return
	technique_playing = &""
	_technique_scrubbed = false
	_technique_elapsed = 0.0
	_last_frame = -1
	_set_frame(frame_driver.current_frame)

func is_playing_technique() -> bool:
	return technique_playing != &""

func debug_snapshot() -> Dictionary:
	return frame_driver.debug_snapshot()

func _set_frame(frame: int) -> void:
	if frames.is_empty():
		return
	var safe_frame := clampi(frame, 0, frames.size() - 1)
	if safe_frame == _last_frame:
		return
	_last_frame = safe_frame
	scale = Vector2.ONE
	texture = frames[safe_frame]

func _process(delta: float) -> void:
	if technique_playing == &"" or _technique_scrubbed:
		return
	var selected: Array = technique_frames.get(technique_playing, [])
	var fps := float(technique_fps.get(technique_playing, DEFAULT_TECHNIQUE_FPS))
	_technique_elapsed += maxf(0.0, delta)
	var frame := int(floor(_technique_elapsed * fps))
	if frame >= selected.size():
		technique_playing = &""
		_technique_scrubbed = false
		_technique_elapsed = 0.0
		_last_frame = -1
		_set_frame(frame_driver.current_frame)
		return
	_set_technique_frame(frame)

func _load_technique_frames() -> void:
	for technique in TECHNIQUE_FOLDERS.keys():
		var folder := str(TECHNIQUE_FOLDERS[technique])
		var directory_path := "%s/%s" % [TECHNIQUE_ROOT, folder]
		var file_names := DirAccess.get_files_at(directory_path)
		file_names.sort()
		var loaded: Array[Texture2D] = []
		for file_name in file_names:
			if not file_name.to_lower().ends_with(".png"):
				continue
			var resource := ResourceLoader.load("%s/%s" % [directory_path, file_name])
			if resource is Texture2D:
				loaded.append(resource as Texture2D)
		if not loaded.is_empty():
			technique_frames[technique] = loaded
			technique_fps[technique] = DEFAULT_TECHNIQUE_FPS

func _set_technique_frame(frame: int) -> void:
	var selected: Array = technique_frames.get(technique_playing, [])
	if selected.is_empty():
		return
	var safe_frame := clampi(frame, 0, selected.size() - 1)
	_apply_technique_scale(selected[safe_frame])
	texture = selected[safe_frame]

func _apply_technique_scale(technique_texture: Texture2D) -> void:
	if frames.is_empty() or technique_texture == null:
		return
	var classic_size := frames[0].get_size()
	var technique_size := technique_texture.get_size()
	if classic_size.y <= 0.0 or technique_size.x <= 0.0 or technique_size.y <= 0.0:
		return
	# Keep the technique's visible width comparable to the tall Classic artwork.
	var classic_aspect := classic_size.x / classic_size.y
	var technique_aspect := technique_size.x / technique_size.y
	var visual_scale := clampf(classic_aspect / technique_aspect, 0.55, 1.0)
	scale = Vector2.ONE * visual_scale
