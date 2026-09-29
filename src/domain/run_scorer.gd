class_name RunScorer
extends RefCounted

var config := ScoringConfig.new()
var hype := HypeSystem.new()

var score: int = 0
var combo: int = 0
var longest_combo: int = 0
var completed_combos: int = 0
var multiplier_hits: int = 0
var perfect_count: int = 0
var great_count: int = 0
var good_count: int = 0
var well_count: int = 0
var miss_count: int = 0
var total_hype_earned: int = 0

func reset() -> void:
	score = 0
	combo = 0
	longest_combo = 0
	completed_combos = 0
	multiplier_hits = 0
	perfect_count = 0
	great_count = 0
	good_count = 0
	well_count = 0
	miss_count = 0
	total_hype_earned = 0
	hype.reset()

func advance(song_time: float) -> void:
	hype.advance(song_time)

func try_activate_the_bang(song_time: float) -> bool:
	return hype.try_activate(song_time)

func resolve(event: Dictionary, judgment: StringName, signed_error: float, motion_quality: float, was_setup: bool) -> Dictionary:
	var context := hype.reward_context()
	var is_hit := judgment != &"MISS"
	var finisher_candidate := bool(event.get("finisher_candidate", false))
	var finisher := is_hit and finisher_candidate and bool(context["finisher_available"]) and judgment in [&"PERFECT", &"GREAT"]

	_apply_judgment(judgment)
	var multiplier := config.multiplier_for_hits(multiplier_hits)
	var contribution := 0
	if judgment == &"WELL":
		contribution = 1
	elif is_hit:
		var raw := config.base_score * config.timing_factor(judgment) * config.motion_factor(motion_quality) * multiplier
		if bool(context["during_the_bang"]):
			raw *= float(context["score_multiplier"])
			raw *= float(context["motion_reward_multiplier"])
		if finisher:
			raw *= float(context["finisher_multiplier"])
		contribution = maxi(1, int(floor(raw)))
	score += contribution

	var hype_added := hype.add_hype(judgment)
	total_hype_earned += hype_added
	if finisher:
		hype.mark_finisher_executed()

	return {
		"event_id": str(event.get("id", "")),
		"judgment": judgment,
		"error": signed_error,
		"motion_quality": motion_quality,
		"was_setup": was_setup,
		"score_contribution": contribution,
		"score": score,
		"combo": combo,
		"multiplier": multiplier,
		"hype": hype.hype,
		"the_bang": hype.the_bang_active,
		"finisher": finisher
	}

func resolve_expired(event: Dictionary) -> Dictionary:
	return resolve(event, &"MISS", 0.0, 0.0, false)

func result(chart: Dictionary) -> Dictionary:
	return {
		"song_id": str(chart.get("song_id", "")),
		"chart_id": str(chart.get("chart_id", "")),
		"chart_version": int(chart.get("chart_version", 1)),
		"rules_version": int(chart.get("rules_version", 1)),
		"score": score,
		"perfect": perfect_count,
		"great": great_count,
		"good": good_count,
		"well": well_count,
		"miss": miss_count,
		"longest_combo": longest_combo,
		"completed_combos": completed_combos,
		"total_hype_earned": total_hype_earned,
		"the_bang_activations": hype.the_bang_activations,
		"finishers": hype.finishers_executed
	}

func _apply_judgment(judgment: StringName) -> void:
	match judgment:
		&"PERFECT":
			perfect_count += 1
			combo += 1
			multiplier_hits += 1
			longest_combo = maxi(longest_combo, combo)
		&"GREAT":
			great_count += 1
			combo += 1
			multiplier_hits += 1
			longest_combo = maxi(longest_combo, combo)
		&"GOOD":
			good_count += 1
			_end_combo()
			multiplier_hits += 1
		&"WELL":
			well_count += 1
			_end_combo()
		_:
			miss_count += 1
			combo = 0
			multiplier_hits = 0

func _end_combo() -> void:
	if combo > 0:
		completed_combos += 1
	longest_combo = maxi(longest_combo, combo)
	combo = 0
