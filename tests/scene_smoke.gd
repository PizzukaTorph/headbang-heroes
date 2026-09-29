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

	var main := packed.instantiate()
	root.add_child(main)
	await process_frame

	_expect(main.has_method("_start_game"), "main shell must expose the POC start flow")
	main.call("_start_game")
	await process_frame
	await process_frame

	var gameplay_view := main.get("gameplay_view") as GameplayView
	var gameplay_run := main.get("gameplay_run") as GameplayRun

	_expect(gameplay_view != null, "gameplay view must instantiate")
	_expect(gameplay_run != null, "gameplay run must instantiate")

	if gameplay_view != null:
		_expect(gameplay_view.erik != null, "Erik FLP view must exist")
		_expect(gameplay_view.erik.frames.size() == 16, "all 16 Erik FLP textures must load")
		_expect(gameplay_view.cue_ring != null, "central cue renderer must exist")
		_expect(gameplay_view.debug_label != null, "tuning telemetry HUD must exist")

	if gameplay_run != null:
		_expect(not gameplay_run.chart.is_empty(), "runtime chart must compile in the real start flow")
		_expect(gameplay_run.clock != null, "authoritative SongClock must exist")
		_expect(gameplay_run.resolver != null, "candidate resolver must exist")
		_expect(gameplay_run.neck.simulation_hz >= 30.0, "neck tuning must be applied")
		gameplay_run.stop_run()

	main.queue_free()
	await process_frame

	if failures == 0:
		print("HH GODOT SCENE SMOKE: PASS")
		quit(0)
	else:
		push_error("HH GODOT SCENE SMOKE: %d FAILURE(S)" % failures)
		quit(1)
