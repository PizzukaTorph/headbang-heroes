extends SceneTree

var failures := 0

func _init() -> void:
	call_deferred("_run")

func _expect(condition: bool, message: String) -> void:
	if condition:
		return
	failures += 1
	push_error("SCENE SMOKE FAIL: " + message)

func _run() -> void:
	var packed := load("res://scenes/main.tscn") as PackedScene
	_expect(packed != null, "main scene must load")
	if packed == null:
		quit(1)
		return

	for profile in POCTuning.VALID_PROFILE_IDS:
		await _exercise_profile(packed, StringName(profile))

	if failures == 0:
		print("HH GODOT SCENE SMOKE: PASS")
		quit(0)
	else:
		push_error("HH GODOT SCENE SMOKE: %d FAILURE(S)" % failures)
		quit(1)

func _exercise_profile(packed: PackedScene, profile: StringName) -> void:
	var main := packed.instantiate()
	root.add_child(main)
	await process_frame

	_expect(main.has_method("_start_game"), "main shell must expose the POC start flow")
	main.set("selected_tuning_profile", profile)
	main.call("_start_game")
	await process_frame
	await process_frame

	var gameplay_view := main.get("gameplay_view") as GameplayView
	var gameplay_run := main.get("gameplay_run") as GameplayRun
	var prefix := "[%s] " % str(profile).to_upper()

	_expect(gameplay_view != null, prefix + "gameplay view must instantiate")
	_expect(gameplay_run != null, prefix + "gameplay run must instantiate")

	if gameplay_view != null:
		_expect(gameplay_view.erik != null, prefix + "Erik FLP view must exist")
		_expect(gameplay_view.erik.frames.size() == 16, prefix + "all 16 Erik FLP textures must load")
		_expect((gameplay_view.erik.technique_frames.get(&"half", []) as Array).size() == 6, prefix + "Half technique frames must load")
		_expect((gameplay_view.erik.technique_frames.get(&"deep", []) as Array).size() == 8, prefix + "Deep technique frames must load")
		_expect(gameplay_view.cue_ring != null, prefix + "cue renderer must exist")
		_expect(gameplay_view.debug_label != null, prefix + "tuning telemetry HUD must exist")
		_expect(not gameplay_view._feedback_tuning.is_empty(), prefix + "feedback tuning must reach presentation")

	if gameplay_run != null:
		_expect(not gameplay_run.chart.is_empty(), prefix + "runtime chart must compile in the real start flow")
		_expect(gameplay_run.clock != null, prefix + "authoritative SongClock must exist")
		_expect(gameplay_run.resolver != null, prefix + "candidate resolver must exist")
		_expect(gameplay_run.active_tuning_profile() == profile, prefix + "selected tuning profile must reach GameplayRun")
		_expect(absf(gameplay_run.neck.stroke_duration - 0.32) < 0.001, prefix + "neck stroke duration must be identical across difficulty profiles")

		var expected_layout := &"directional" if profile in [&"hard", &"extreme"] else &"centered"
		_expect(gameplay_view.cue_ring.cue_layout_mode == expected_layout, prefix + "cue layout profile must reach renderer")
		if profile == &"normal":
			var neck_before: Dictionary = gameplay_run.neck.presentation_state()
			_expect(gameplay_view.erik.play_technique(&"half"), prefix + "Half technique animation must start")
			await create_timer(0.60).timeout
			_expect(not gameplay_view.erik.is_playing_technique(), prefix + "Half technique animation must return to normal")
			_expect(gameplay_run.neck.presentation_state() == neck_before, prefix + "technique presentation must not mutate neck state")
			_expect(gameplay_view.erik.play_technique(&"deep"), prefix + "Deep technique animation must start")
			await create_timer(0.75).timeout
			_expect(not gameplay_view.erik.is_playing_technique(), prefix + "Deep technique animation must return to normal")
			_expect(not gameplay_view.erik.play_technique(&"unknown"), prefix + "unknown technique must fail safely")
		gameplay_run.stop_run()

	main.queue_free()
	await process_frame
