extends SceneTree

const TimingConfigScript = preload("res://src/domain/timing_config.gd")
const ChartCompilerScript = preload("res://src/domain/chart_compiler.gd")
const CandidateResolverScript = preload("res://src/domain/candidate_resolver.gd")
const NeckMotionScript = preload("res://src/domain/neck_motion_state.gd")
const MotionQualityScript = preload("res://src/domain/motion_quality.gd")
const RunScorerScript = preload("res://src/domain/run_scorer.gd")
const FLPFrameDriverScript = preload("res://src/presentation/flp_frame_driver.gd")
const POCTuningScript = preload("res://src/config/poc_tuning.gd")
const RunDiagnosticsScript = preload("res://src/debug/run_diagnostics.gd")
const CueLayoutScript = preload("res://src/presentation/cue_layout.gd")

var failures := 0

func _init() -> void:
	_test_timing_boundaries()
	_test_chart_compile()
	_test_candidate_resolution()
	_test_neck_and_motion_quality()
	_test_scoring()
	_test_flp_frame_driver()
	_test_poc_tuning()
	_test_run_diagnostics()
	_test_cue_layout_profiles()
	if failures == 0:
		print("HH GODOT TESTS: PASS")
		quit(0)
	else:
		push_error("HH GODOT TESTS: %d FAILURE(S)" % failures)
		quit(1)

func _expect(condition: bool, message: String) -> void:
	if condition:
		return
	failures += 1
	push_error("TEST FAIL: " + message)

func _test_timing_boundaries() -> void:
	var cfg = TimingConfigScript.new()
	_expect(cfg.classify(0.0) == &"PERFECT", "zero error must be PERFECT")
	_expect(cfg.classify(0.070) == &"PERFECT", "perfect boundary is inclusive")
	_expect(cfg.classify(0.130) == &"GREAT", "great boundary is inclusive")
	_expect(cfg.classify(0.190) == &"GOOD", "good boundary is inclusive")
	_expect(cfg.classify(0.240) == &"WELL", "well boundary is inclusive")
	_expect(cfg.classify(0.241) == &"MISS", "outside well window must MISS")

func _test_chart_compile() -> void:
	var chart: Dictionary = ChartCompilerScript.load_chart("res://game/assets/charts/lab-002-tempo-ramp.json")
	_expect(not chart.is_empty(), "tempo-ramp chart must compile")
	_expect(str(chart.get("chart_id", "")) == "lab-002-tempo-ramp", "chart identity must survive compilation")
	_expect((chart.get("events", []) as Array).size() >= 40, "tempo-ramp fixture must contain the authored event set")

func _test_candidate_resolution() -> void:
	var cfg = TimingConfigScript.new()
	var events: Array = [
		{"id":"a", "time":1.0, "direction":&"left"},
		{"id":"b", "time":1.08, "direction":&"right"}
	]
	var resolver = CandidateResolverScript.new(events)
	var match: Dictionary = resolver.resolve(&"right", 1.075, cfg)
	_expect(bool(match.get("consumed", false)), "eligible input must consume one event")
	_expect(str((match.get("event", {}) as Dictionary).get("id", "")) == "b", "compatible nearest candidate must win")
	var preview_resolver = CandidateResolverScript.new(events)
	var upcoming: Array[Dictionary] = preview_resolver.upcoming_unresolved(0.5, 1.0, 2)
	_expect(upcoming.size() == 2, "cue look-ahead must expose current + next without consuming them")
	_expect(not preview_resolver.resolved[0] and not preview_resolver.resolved[1], "cue look-ahead must be presentation-only")

	var resolver_wrong = CandidateResolverScript.new([{"id":"x", "time":2.0, "direction":&"left"}])
	var wrong: Dictionary = resolver_wrong.resolve(&"right", 2.0, cfg)
	_expect(StringName(wrong.get("judgment", "")) == &"MISS", "wrong direction in-window must consume as MISS")

func _test_neck_and_motion_quality() -> void:
	var neck = NeckMotionScript.new()
	var first: Dictionary = neck.apply_bang(&"left")
	var first_quality: Dictionary = MotionQualityScript.evaluate(first)
	_expect(bool(first_quality.get("was_setup", false)), "first bang must be setup/unprepared")
	neck.advance(0.20)
	var second: Dictionary = neck.apply_bang(&"right")
	var second_quality: Dictionary = MotionQualityScript.evaluate(second)
	_expect(not bool(second_quality.get("was_setup", true)), "second bang must have preceding travel")
	_expect(float(second_quality.get("quality", 0.0)) > 0.0, "prepared movement must produce inspectable Motion Quality")

func _test_scoring() -> void:
	var scorer = RunScorerScript.new()
	scorer.reset()
	var event := {"id":"m0000", "finisher_candidate":false}
	var outcome: Dictionary = scorer.resolve(event, &"PERFECT", 0.0, 1.0, true)
	_expect(int(outcome.get("score_contribution", 0)) > 0, "PERFECT must score")
	_expect(int(outcome.get("combo", 0)) == 1, "PERFECT must advance combo")
	scorer.resolve({"id":"m0001","finisher_candidate":false}, &"MISS", 0.0, 0.0, false)
	_expect(scorer.combo == 0, "MISS must reset combo")


func _test_flp_frame_driver() -> void:
	var driver = FLPFrameDriverScript.new(16)
	var neutral := {
		"horizontal_angle": 0.0,
		"vertical_angle": 0.0,
		"horizontal_velocity": 0.0,
		"vertical_velocity": 0.0
	}
	_expect(driver.update(neutral, 1.0 / 60.0) == 0, "settled neck must show neutral FLP frame")

	var moving := {
		"horizontal_angle": 42.0,
		"vertical_angle": 0.0,
		"horizontal_velocity": 360.0,
		"vertical_velocity": 0.0
	}
	driver.update(moving, 1.0 / 60.0)
	_expect(driver.phase_frames > 0.0, "physical angular travel must advance FLP phase")
	_expect(
		driver.phase_frames <= driver.max_visual_fps / 60.0 + 0.0001,
		"FLP phase must respect the readability speed cap"
	)

	driver.update(neutral, 1.0 / 60.0)
	_expect(driver.current_frame == 0, "physically settled neutral must return presentation to frame 0")


func _test_poc_tuning() -> void:
	var profiles := ["easy", "normal", "hard", "extreme"]
	var perfect_windows: Dictionary = {}
	var neck_impulses: Dictionary = {}
	var layouts: Dictionary = {}

	for profile in profiles:
		var tuning = POCTuningScript.new()
		_expect(tuning.load_profile(StringName(profile)), "%s tuning profile must load" % profile)
		_expect(str(tuning.profile_id) == profile, "%s profile identity must survive merge" % profile)
		perfect_windows[profile] = tuning.number("timing", "perfectWindowMs", 0.0)
		neck_impulses[profile] = tuning.number("neck", "impulse", 0.0)
		layouts[profile] = str(tuning.section("cue").get("layoutMode", ""))
		_expect(absf(tuning.number("flp", "maxVisualFps", 0.0) - 9.0) < 0.001, "%s must inherit base FLP tuning" % profile)
		_expect(absf(tuning.number("cue", "targetRadius", 0.0) - 42.0) < 0.001, "%s must inherit nested base cue values" % profile)
		_expect(not tuning.section("feedback").is_empty(), "%s must inherit base feedback tuning" % profile)

	_expect(float(perfect_windows["easy"]) > float(perfect_windows["normal"]), "Easy timing must be more generous than Normal")
	_expect(float(perfect_windows["normal"]) > float(perfect_windows["hard"]), "Hard timing must be tighter than Normal")
	_expect(float(perfect_windows["hard"]) > float(perfect_windows["extreme"]), "Extreme timing must be tighter than Hard")

	for profile in profiles:
		_expect(absf(float(neck_impulses[profile]) - 190.0) < 0.001, "%s must preserve identical neck impulse" % profile)

	_expect(str(layouts["easy"]) == "centered", "Easy cue layout must remain centered")
	_expect(str(layouts["normal"]) == "centered", "Normal cue layout must remain centered")
	_expect(str(layouts["hard"]) == "directional", "Hard cue layout must exercise directional spatialization")
	_expect(str(layouts["extreme"]) == "directional", "Extreme cue layout must exercise directional spatialization")
	_expect(not POCTuningScript.is_valid_profile(&"nightmare"), "unknown profile IDs must be rejected by validation")

	var tuned_neck = NeckMotionScript.new()
	tuned_neck.configure({"impulse": 100.0, "simulationHz": 120.0})
	tuned_neck.apply_bang(&"left")
	var state: Dictionary = tuned_neck.presentation_state()
	_expect(absf(float(state.get("horizontal_velocity", 0.0)) - 100.0) < 0.001, "neck tuning must alter launch impulse without changing algorithm")

	var driver = FLPFrameDriverScript.new(16)
	driver.configure({"maxVisualFps": 5.0})
	_expect(absf(driver.max_visual_fps - 5.0) < 0.001, "FLP tuning must alter readability cap")

func _test_run_diagnostics() -> void:
	var diag = RunDiagnosticsScript.new()
	diag.record(&"GREAT", -0.040)
	diag.record(&"PERFECT", 0.020)
	var snapshot: Dictionary = diag.snapshot()
	_expect(int(snapshot.get("samples", 0)) == 2, "timing diagnostics must count consumed player inputs")
	_expect(int(snapshot.get("early", 0)) == 1 and int(snapshot.get("late", 0)) == 1, "timing diagnostics must track early/late bias")
	_expect(absf(float(snapshot.get("mean_signed_ms", 0.0)) + 10.0) < 0.01, "timing diagnostics mean bias must preserve sign")
	_expect(absf(float(snapshot.get("mean_absolute_ms", 0.0)) - 30.0) < 0.01, "timing diagnostics must report mean absolute error")


func _test_cue_layout_profiles() -> void:
	_expect(CueLayoutScript.offset_for(&"left", &"centered", 82.0) == Vector2.ZERO, "Easy/centered cue profile must share one anchor")
	_expect(CueLayoutScript.offset_for(&"right", &"directional", 82.0) == Vector2(82.0, 0.0), "directional cue profile must spatialize RIGHT")
	_expect(CueLayoutScript.offset_for(&"up", &"directional", 82.0) == Vector2(0.0, -82.0), "directional cue profile must spatialize UP")
