class_name TimingConfig
extends RefCounted

var perfect_window: float = 0.070
var great_window: float = 0.130
var good_window: float = 0.190
var well_window: float = 0.240
var candidate_lead: float = 1.0
var late_expiry: float = 0.240

func classify(signed_error: float) -> StringName:
	var error := absf(signed_error)
	if error <= perfect_window:
		return &"PERFECT"
	if error <= great_window:
		return &"GREAT"
	if error <= good_window:
		return &"GOOD"
	if error <= well_window:
		return &"WELL"
	return &"MISS"
