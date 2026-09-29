class_name CandidateResolver
extends RefCounted

var events: Array[Dictionary] = []
var resolved: Array[bool] = []

func _init(source_events: Array = []) -> void:
	for event in source_events:
		events.append((event as Dictionary).duplicate(true))
	resolved.resize(events.size())
	for i in resolved.size():
		resolved[i] = false

func reset() -> void:
	for i in resolved.size():
		resolved[i] = false

func all_resolved() -> bool:
	for value in resolved:
		if not value:
			return false
	return true

func next_unresolved_time() -> float:
	for i in events.size():
		if not resolved[i]:
			return float(events[i]["time"])
	return -1.0

func earliest_unresolved_within(now: float, lead_seconds: float) -> Dictionary:
	for i in events.size():
		if resolved[i]:
			continue
		var event := events[i]
		if float(event["time"]) - now <= lead_seconds:
			return {"slot": i, "event": event}
		return {}
	return {}

func resolve(direction: StringName, song_time: float, config: TimingConfig) -> Dictionary:
	var have_compatible := false
	var compatible_slot := -1
	var compatible_abs := INF
	var have_any := false
	var any_slot := -1
	var any_abs := INF

	for i in events.size():
		if resolved[i]:
			continue
		var event := events[i]
		var error := song_time - float(event["time"])
		if error < -config.well_window or error > config.late_expiry:
			continue
		var distance := absf(error)
		if not have_any or _is_better(i, distance, any_slot, any_abs):
			have_any = true
			any_slot = i
			any_abs = distance
		if StringName(event["direction"]) == direction:
			if not have_compatible or _is_better(i, distance, compatible_slot, compatible_abs):
				have_compatible = true
				compatible_slot = i
				compatible_abs = distance

	if have_compatible:
		resolved[compatible_slot] = true
		var event := events[compatible_slot]
		var error := song_time - float(event["time"])
		return {
			"consumed": true,
			"kind": &"hit",
			"slot": compatible_slot,
			"event": event,
			"error": error,
			"judgment": config.classify(error)
		}
	if have_any:
		resolved[any_slot] = true
		var event := events[any_slot]
		return {
			"consumed": true,
			"kind": &"wrong",
			"slot": any_slot,
			"event": event,
			"error": song_time - float(event["time"]),
			"judgment": &"MISS"
		}
	return {"consumed": false, "kind": &"too_early", "slot": -1, "judgment": &"MISS", "error": 0.0}

func expire(now: float, config: TimingConfig) -> Array[Dictionary]:
	var expired: Array[Dictionary] = []
	for i in events.size():
		if resolved[i]:
			continue
		if now - float(events[i]["time"]) > config.late_expiry:
			resolved[i] = true
			expired.append(events[i])
	return expired

func _is_better(slot: int, distance: float, best_slot: int, best_distance: float) -> bool:
	if best_slot < 0:
		return true
	if distance < best_distance - 0.000000000001:
		return true
	if distance > best_distance + 0.000000000001:
		return false
	var event_time := float(events[slot]["time"])
	var best_time := float(events[best_slot]["time"])
	if event_time < best_time - 0.000000000001:
		return true
	if event_time > best_time + 0.000000000001:
		return false
	return str(events[slot]["id"]) < str(events[best_slot]["id"])
