class_name ErikView
extends TextureRect

const FRAME_COUNT := 16
const FRAME_ROOT := "res://game/assets/characters/erik/headbang/headbang_%02d.png"

var frames: Array[Texture2D] = []
var frame_driver := FLPFrameDriver.new(FRAME_COUNT)
var _last_frame: int = -1

func _ready() -> void:
	expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	mouse_filter = Control.MOUSE_FILTER_IGNORE

	for i in FRAME_COUNT:
		var texture_resource := ResourceLoader.load(FRAME_ROOT % i)
		if texture_resource is Texture2D:
			frames.append(texture_resource as Texture2D)

	reset_pose()

func apply_tuning(values: Dictionary) -> void:
	frame_driver.configure(values)

func reset_pose() -> void:
	frame_driver.reset()
	_last_frame = -1
	_set_frame(0)

func apply_neck_state(neck_state: Dictionary, delta: float) -> void:
	if frames.is_empty():
		return
	var frame := frame_driver.update(neck_state, delta)
	_set_frame(frame)

func debug_snapshot() -> Dictionary:
	return frame_driver.debug_snapshot()

func _set_frame(frame: int) -> void:
	if frames.is_empty():
		return
	var safe_frame := clampi(frame, 0, frames.size() - 1)
	if safe_frame == _last_frame:
		return
	_last_frame = safe_frame
	texture = frames[safe_frame]
