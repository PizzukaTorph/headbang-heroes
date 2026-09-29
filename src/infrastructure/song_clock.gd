class_name SongClock
extends Node

signal playback_finished

var player: AudioStreamPlayer
var calibration_seconds: float = 0.0
var running: bool = false
var paused: bool = false
var _last_song_time: float = 0.0
var _start_offset: float = 0.0

func _init(audio_player: AudioStreamPlayer = null) -> void:
	player = audio_player

func bind(audio_player: AudioStreamPlayer) -> void:
	player = audio_player

func start(stream: AudioStream, start_song_time: float = 0.0) -> bool:
	if player == null or stream == null:
		return false
	if not player.finished.is_connected(_on_finished):
		player.finished.connect(_on_finished)
	_start_offset = maxf(0.0, start_song_time)
	_last_song_time = _start_offset
	player.stream = stream
	player.play(_start_offset)
	running = true
	paused = false
	return true

func stop() -> void:
	if player != null:
		player.stop()
	running = false
	paused = false
	_last_song_time = 0.0

func pause() -> void:
	if player == null or not running or paused:
		return
	player.stream_paused = true
	paused = true

func resume() -> void:
	if player == null or not running or not paused:
		return
	player.stream_paused = false
	paused = false

func song_time() -> float:
	if player == null or not running:
		return _last_song_time
	if paused:
		return _last_song_time
	var time := player.get_playback_position() + AudioServer.get_time_since_last_mix() - AudioServer.get_output_latency()
	time += calibration_seconds
	time = maxf(_start_offset, time)
	if time < _last_song_time:
		time = _last_song_time
	_last_song_time = time
	return time

func output_latency_seconds() -> float:
	return AudioServer.get_output_latency()

func set_calibration_ms(value_ms: float) -> void:
	calibration_seconds = value_ms / 1000.0

func calibration_ms() -> float:
	return calibration_seconds * 1000.0

func _on_finished() -> void:
	running = false
	playback_finished.emit()
