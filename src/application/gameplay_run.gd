class_name GameplayRun
extends Node

signal cue_changed(cue: Dictionary)
signal hud_changed(state: Dictionary)
signal judgment_resolved(outcome: Dictionary)
signal free_bang(direction: StringName)
signal technique_resolved(result: Dictionary)
signal technique_failed(result: Dictionary)
signal run_finished(result: Dictionary)
signal pause_changed(paused: bool)

const DEFAULT_NEXT_CUE_HORIZON_MULTIPLIER := 2.25

var chart: Dictionary = {}
var resolver: CandidateResolver
var technique_resolver: TechniqueEventResolver
var technique_gesture := TechniqueGestureRecognizer.new()
var timing := TimingConfig.new()
var neck := NeckMotionState.new()
var scorer := RunScorer.new()
var diagnostics := RunDiagnostics.new()
var tuning := POCTuning.new()
var clock: SongClock
var audio_player: AudioStreamPlayer

var _stream: AudioStream
var _active: bool = false
var _finished: bool = false
var _last_song_time: float = 0.0
var _audio_finished: bool = false
var _next_cue_horizon_multiplier: float = DEFAULT_NEXT_CUE_HORIZON_MULTIPLIER

func _ready() -> void:
	set_process(false)

func configure(
	chart_path: String,
	audio_path: String,
	tuning_profile: StringName = POCTuning.DEFAULT_PROFILE
) -> bool:
	if not tuning.load_profile(tuning_profile):
		push_error("HH tuning profile could not be loaded: %s" % str(tuning_profile))
		return false
	_apply_tuning()

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

	var classic_events: Array = []
	var technique_events: Array = []
	for event in chart["events"]:
		if StringName((event as Dictionary).get("technique", "classic")) == &"classic":
			classic_events.append(event)
		else:
			technique_events.append(event)
	resolver = CandidateResolver.new(classic_events)
	technique_resolver = TechniqueEventResolver.new(technique_events)
	technique_gesture.configure(tuning.section("techniques"))
	return true

func active_tuning_profile() -> StringName:
	return tuning.profile_id

func presentation_tuning() -> Dictionary:
	return {
		"flp": tuning.section("flp"),
		"cue": tuning.section("cue"),
		"feedback": tuning.section("feedback"),
		"techniques": tuning.section("techniques")
	}

func reload_tuning() -> bool:
	var loaded := tuning.reload()
	if not loaded:
		return false
	_apply_tuning()
	_emit_hud()
	return true

func start_run() -> void:
	if chart.is_empty() or _stream == null or clock == null:
		return
	resolver.reset()
	technique_resolver.reset()
	technique_gesture.reset()
	neck.reset()
	scorer.reset()
	diagnostics.reset()
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
	var judgment := StringName(match["judgment"])
	var signed_error := float(match["error"])

	# Diagnostics observe resolved player input but never participate in scoring.
	diagnostics.record(judgment, signed_error)

	var outcome := scorer.resolve(
		event,
		judgment,
		signed_error,
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

func technique_gesture_sample(phase: StringName, position: Vector2) -> void:
	if not _active or _finished or clock == null or technique_resolver == null:
		return
	var now := clock.song_time()
	match phase:
		&"start":
			var event := technique_resolver.active_event(now)
			if not event.is_empty():
				technique_gesture.begin(StringName(event.get("technique", "")), now, position)
			_emit_hud()
		&"update":
			technique_gesture.update(position)
			_emit_hud()
		&"complete":
			var intent := technique_gesture.complete(now, position)
			if intent.is_empty():
				return
			var result := technique_resolver.resolve(intent)
			result["song_time"] = now
			if bool(result.get("valid", false)):
				var event: Dictionary = result["event"]
				result["neck_snapshot"] = neck.apply_bang(StringName(event["direction"]), float(event.get("intensity", 1.0)))
				technique_resolved.emit(result)
			else:
				technique_failed.emit(result)
			_emit_hud()

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
	for expired_technique in technique_resolver.expire(now):
		technique_failed.emit({
			"consumed": true,
			"valid": false,
			"kind": &"expired",
			"reason": "window_expired",
			"event": expired_technique,
			"song_time": now
		})

	_emit_cues(now)
	_emit_hud()

	var last_event_time := _last_authored_event_time()
	if resolver.all_resolved() and technique_resolver.all_resolved() and (now >= last_event_time + 0.75 or _audio_finished):
		_finish()

func _apply_tuning() -> void:
	var timing_values := tuning.section("timing")
	timing.perfect_window = maxf(0.001, float(timing_values.get("perfectWindowMs", 70.0)) / 1000.0)
	timing.great_window = maxf(timing.perfect_window, float(timing_values.get("greatWindowMs", 130.0)) / 1000.0)
	timing.good_window = maxf(timing.great_window, float(timing_values.get("goodWindowMs", 190.0)) / 1000.0)
	timing.well_window = maxf(timing.good_window, float(timing_values.get("wellWindowMs", 240.0)) / 1000.0)
	timing.candidate_lead = maxf(0.0, float(timing_values.get("candidateLeadMs", 1000.0)) / 1000.0)
	timing.late_expiry = maxf(0.0, float(timing_values.get("lateExpiryMs", 240.0)) / 1000.0)

	# North-star difficulty rule: the same neck is configured from base tuning for every profile.
	# Difficulty overrides deliberately contain no neck section.
	neck.configure(tuning.section("neck"))

	var cue_values := tuning.section("cue")
	_next_cue_horizon_multiplier = maxf(
		1.0,
		float(cue_values.get("nextHorizonMultiplier", DEFAULT_NEXT_CUE_HORIZON_MULTIPLIER))
	)

func _upcoming_events(now: float, lead_seconds: float, count: int) -> Array[Dictionary]:
	var candidates: Array[Dictionary] = []
	for item in resolver.upcoming_unresolved(now, lead_seconds, count):
		candidates.append(item)
	for item in technique_resolver.upcoming_unresolved(now, lead_seconds, count):
		candidates.append(item)
	candidates.sort_custom(_cue_before)
	if candidates.size() > count:
		candidates.resize(count)
	return candidates

static func _cue_before(a: Dictionary, b: Dictionary) -> bool:
	var at := float((a["event"] as Dictionary)["time"])
	var bt := float((b["event"] as Dictionary)["time"])
	if not is_equal_approx(at, bt):
		return at < bt
	return str((a["event"] as Dictionary).get("id", "")) < str((b["event"] as Dictionary).get("id", ""))

func _emit_cues(now: float) -> void:
	var approach_time := float(chart.get("approach_time", 1.0))
	var preview_horizon := approach_time * _next_cue_horizon_multiplier
	var upcoming := _upcoming_events(now, preview_horizon, 2)

	if upcoming.is_empty():
		cue_changed.emit({})
		return

	var current_event: Dictionary = {}
	var next_event: Dictionary = {}

	var first: Dictionary = upcoming[0]
	var first_event: Dictionary = first["event"]
	var first_remaining := float(first_event["time"]) - now

	if first_remaining <= approach_time:
		current_event = first_event
		if upcoming.size() > 1:
			next_event = (upcoming[1] as Dictionary)["event"]
	else:
		next_event = first_event

	cue_changed.emit({
		"current": current_event,
		"next": next_event,
		"song_time": now,
		"approach_time": approach_time,
		"preview_horizon": preview_horizon
	})

func _emit_hud() -> void:
	if scorer == null:
		return
	hud_changed.emit({
		"tuning_profile": str(tuning.profile_id),
		"chart_difficulty": str(chart.get("difficulty", "")),
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
		"neck": neck.presentation_state(),
		"neck_tuning": neck.tuning_snapshot(),
		"timing_diagnostics": diagnostics.snapshot(),
		"technique": technique_resolver.debug_state(clock.song_time() if clock != null else 0.0),
		"gesture": technique_gesture.debug_state()
	})

func _last_authored_event_time() -> float:
	var events: Array = chart.get("events", [])
	if events.is_empty():
		return 0.0
	var latest := 0.0
	for event in events:
		var item: Dictionary = event
		latest = maxf(latest, float(item["time"]) + float(item.get("duration", 0.0)))
	return latest

func _on_audio_finished() -> void:
	_audio_finished = true
	if resolver != null and technique_resolver != null and resolver.all_resolved() and technique_resolver.all_resolved():
		_finish()

func _finish() -> void:
	if _finished:
		return
	_finished = true
	_active = false
	set_process(false)
	var result := scorer.result(chart)
	result["tuning_profile"] = str(tuning.profile_id)
	result["chart_difficulty"] = str(chart.get("difficulty", ""))
	result["calibration_ms"] = calibration_ms()
	result["timing_diagnostics"] = diagnostics.snapshot()
	run_finished.emit(result)
