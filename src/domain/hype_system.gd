class_name HypeSystem
extends RefCounted

var hype: int = 0
var max_hype: int = 100
var the_bang_active: bool = false
var the_bang_end_song_time: float = 0.0
var finisher_taken_this_window: bool = false
var the_bang_activations: int = 0
var finishers_executed: int = 0

var the_bang_duration: float = 8.0
var score_multiplier: float = 10.0
var motion_reward_multiplier: float = 10.0
var finisher_multiplier: float = 10.0

func reset() -> void:
	hype = 0
	the_bang_active = false
	the_bang_end_song_time = 0.0
	finisher_taken_this_window = false
	the_bang_activations = 0
	finishers_executed = 0

func advance(song_time: float) -> void:
	if the_bang_active and song_time >= the_bang_end_song_time:
		the_bang_active = false
		finisher_taken_this_window = false

func is_ready() -> bool:
	return hype >= max_hype

func add_hype(judgment: StringName) -> int:
	var amount := 0
	match judgment:
		&"PERFECT": amount = 2
		&"GREAT": amount = 1
		_: amount = 0
	if amount <= 0:
		return 0
	var before := hype
	hype = mini(max_hype, hype + amount)
	return hype - before

func try_activate(song_time: float) -> bool:
	if the_bang_active or not is_ready():
		return false
	the_bang_active = true
	the_bang_end_song_time = song_time + the_bang_duration
	finisher_taken_this_window = false
	hype = 0
	the_bang_activations += 1
	return true

func reward_context() -> Dictionary:
	return {
		"during_the_bang": the_bang_active,
		"score_multiplier": score_multiplier,
		"motion_reward_multiplier": motion_reward_multiplier,
		"finisher_multiplier": finisher_multiplier,
		"finisher_available": the_bang_active and not finisher_taken_this_window
	}

func mark_finisher_executed() -> void:
	if the_bang_active and not finisher_taken_this_window:
		finisher_taken_this_window = true
		finishers_executed += 1
