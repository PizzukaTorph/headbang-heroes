class_name ScoringConfig
extends RefCounted

var base_score: float = 1000.0
var perfect_factor: float = 1.0
var great_factor: float = 0.85
var good_factor: float = 0.60
var motion_floor: float = 0.5
var hits_per_multiplier_step: int = 10
var max_multiplier: int = 5

func timing_factor(judgment: StringName) -> float:
	match judgment:
		&"PERFECT": return perfect_factor
		&"GREAT": return great_factor
		&"GOOD": return good_factor
		_: return 0.0

func motion_factor(quality: float) -> float:
	return motion_floor + (1.0 - motion_floor) * clampf(quality, 0.0, 1.0)

func multiplier_for_hits(successful_hits: int) -> int:
	var steps := int(successful_hits / hits_per_multiplier_step)
	return mini(1 + steps, max_multiplier)
