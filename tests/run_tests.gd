extends SceneTree

const TimingConfigScript = preload("res://src/domain/timing_config.gd")
const ChartCompilerScript = preload("res://src/domain/chart_compiler.gd")
const CandidateResolverScript = preload("res://src/domain/candidate_resolver.gd")
const NeckMotionScript = preload("res://src/domain/neck_motion_state.gd")
const MotionQualityScript = preload("res://src/domain/motion_quality.gd")
const RunScorerScript = preload("res://src/domain/run_scorer.gd")
const FLPFrameDriverScript = preload("res://src/presentation/flp_frame_driver.gd")

var failures := 0

func _init() -> void:
	_test_timing_boundaries()
	_test_chart_compile()
	_test_candidate_resolution()
	_test_neck_and_motion_quality()
	_test_scoring()
	_test_flp_frame_driver()
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
	var upcoming: Array[Dictionary] = resolver.upcoming_unresolved(0.5, 1.0, 2)
	_expect(upcoming.size() == 2, "cue look-ahead must expose current + next without consuming them")
	_expect(not resolver.resolved[0] and not resolver.resolved[1], "cue look-ahead must be presentation-only")

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
		driver.phase_frames <= FLPFrameDriverScript.MAX_VISUAL_FPS / 60.0 + 0.0001,
		"FLP phase must respect the readability speed cap"
	)

	driver.update(neutral, 1.0 / 60.0)
	_expect(driver.current_frame == 0, "physically settled neutral must return presentation to frame 0")
