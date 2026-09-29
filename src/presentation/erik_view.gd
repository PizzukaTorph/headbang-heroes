class_name ErikView
extends TextureRect

const FRAME_COUNT := 16
const FRAME_ROOT := "res://game/assets/characters/erik/headbang/headbang_%02d.png"

var frames: Array[Texture2D] = []
var _playing: bool = false
var _frame_index: int = 0
var _frame_time: float = 0.0
var _frame_duration: float = 0.040

func _ready() -> void:
	expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	for i in FRAME_COUNT:
		var texture_resource := ResourceLoader.load(FRAME_ROOT % i)
		if texture_resource is Texture2D:
			frames.append(texture_resource as Texture2D)
	if not frames.is_empty():
		texture = frames[0]
	set_process(false)

func play_headbang(intensity: float = 1.0) -> void:
	if frames.is_empty():
		return
	_frame_duration = lerpf(0.052, 0.031, clampf(intensity, 0.0, 1.0))
	_frame_index = 0
	_frame_time = 0.0
	_playing = true
	texture = frames[0]
	set_process(true)

func _process(delta: float) -> void:
	if not _playing:
		return
	_frame_time += delta
	while _frame_time >= _frame_duration and _playing:
		_frame_time -= _frame_duration
		_frame_index += 1
		if _frame_index >= frames.size():
			_playing = false
			_frame_index = 0
			texture = frames[0]
			set_process(false)
		else:
			texture = frames[_frame_index]
