class_name GameplayRun
extends Node

signal cue_changed(cue: Dictionary)
signal hud_changed(state: Dictionary)
signal judgment_resolved(outcome: Dictionary)
signal free_bang(direction: StringName)
signal run_finished(result: Dictionary)
signal pause_changed(paused: bool)

var chart: Dictionary = {}
var resolver: CandidateResolver
var timing := TimingConfig.new()
var neck := NeckMotionState.new()
var scorer := RunScorer.new()
var clock: SongClock
var audio_player: AudioStreamPlayer

var _stream: AudioStream
var _active: bool = false
var _finished: bool = false
var _last_song_time: float = 0.0
var _audio_finished: bool = false

func _ready() -> void:
	set_process(false)

func configure(chart_path: String, audio_path: String) -> bool:
	chart = HHChartCompiler.load_chart(chart_path)
	if chart.is_empty():
		return false
	var resource := ResourceLoader.load(audio_path)
	if resource == null or not resource is AudioStream:
		push_error("HH audio resource could not be loaded: %s" % audio_path)
		return false
	_stream = resource as AudioStream

	audio_player = AudioStreamPlayer.new()
	audio_player.name = "SongPlayer"
	add_child(audio_player)
	clock = SongClock.new(audio_player)
	clock.name = "SongClock"
	add_child(clock)
	clock.playback_finished.connect(_on_audio_finished)

	resolver = CandidateResolver.new(chart["events"])
	return true

func start_run() -> void:
	if chart.is_empty() or _stream == null or clock == null:
		return
	resolver.reset()
	neck.reset()
	scorer.reset()
	_finished = false
	_audio_finished = false
	_last_song_time = 0.0
	_active = clock.start(_stream)
	set_process(_active)
	_emit_hud()

func stop_run() -> void:
	_active = false
	set_process(false)
	if clock != null:
		clock.stop()

func set_calibration_ms(value: float) -> void:
	if clock != null:
		clock.set_calibration_ms(value)
	_emit_hud()

func calibration_ms() -> float:
	return clock.calibration_ms() if clock != null else 0.0

func toggle_pause() -> void:
	if not _active or clock == null:
		return
	if clock.paused:
		clock.resume()
	else:
		clock.pause()
	pause_changed.emit(clock.paused)

func bang(direction: StringName) -> void:
	if not _active or _finished or clock == null:
		return
	var now := clock.song_time()

	# Physical ownership comes first. Every valid bang changes the neck, regardless of judgment.
	var snapshot := neck.apply_bang(direction)
	var motion := MotionQuality.evaluate(snapshot)
	var match := resolver.resolve(direction, now, timing)
	if not bool(match.get("consumed", false)):
		free_bang.emit(direction)
		_emit_hud()
		return

	var event: Dictionary = match["event"]
	var outcome := scorer.resolve(
		event,
		StringName(match["judgment"]),
		float(match["error"]),
		float(motion["quality"]),
		bool(motion["was_setup"])
	)
	judgment_resolved.emit(outcome)
	_emit_hud()

func try_activate_the_bang() -> bool:
	if not _active or clock == null:
		return false
	var activated := scorer.try_activate_the_bang(clock.song_time())
	_emit_hud()
	return activated

func _process(_delta: float) -> void:
	if not _active or _finished:
		return

	var now := clock.song_time()
	var authoritative_delta := maxf(0.0, now - _last_song_time)
	_last_song_time = now
	neck.advance(authoritative_delta)
	scorer.advance(now)

	for expired_event in resolver.expire(now, timing):
		var outcome := scorer.resolve_expired(expired_event)
		judgment_resolved.emit(outcome)

	var cue := resolver.earliest_unresolved_within(now, float(chart.get("approach_time", 1.0)))
	if cue.is_empty():
		cue_changed.emit({})
	else:
		var payload: Dictionary = cue.duplicate(true)
		payload["song_time"] = now
		payload["approach_time"] = float(chart.get("approach_time", 1.0))
		cue_changed.emit(payload)

	_emit_hud()

	var last_event_time := _last_authored_event_time()
	if resolver.all_resolved() and (now >= last_event_time + 0.75 or _audio_finished):
		_finish()

func _emit_hud() -> void:
	if scorer == null:
		return
	hud_changed.emit({
		"song_time": clock.song_time() if clock != null else 0.0,
		"score": scorer.score,
		"combo": scorer.combo,
		"multiplier": scorer.config.multiplier_for_hits(scorer.multiplier_hits),
		"hype": scorer.hype.hype,
		"hype_max": scorer.hype.max_hype,
		"the_bang": scorer.hype.the_bang_active,
		"the_bang_ready": scorer.hype.is_ready(),
		"calibration_ms": calibration_ms(),
		"audio_latency_ms": (clock.output_latency_seconds() * 1000.0) if clock != null else 0.0,
		"neck": neck.presentation_state()
	})

func _last_authored_event_time() -> float:
	var events: Array = chart.get("events", [])
	if events.is_empty():
		return 0.0
	return float((events[events.size() - 1] as Dictionary)["time"])

func _on_audio_finished() -> void:
	_audio_finished = true
	if resolver != null and resolver.all_resolved():
		_finish()

func _finish() -> void:
	if _finished:
		return
	_finished = true
	_active = false
	set_process(false)
	var result := scorer.result(chart)
	result["calibration_ms"] = calibration_ms()
	run_finished.emit(result)
