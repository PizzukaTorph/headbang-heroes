class_name TechniqueEventResolver
extends RefCounted

## Resolves semantic gesture completion against authored Technique Skill windows.
## It does not score and it does not apply neck motion.

var events: Array[Dictionary] = []
var resolved: Array[bool] = []
var half_release_grace_seconds: float = 0.25

func _init(source_events: Array = []) -> void:
	for source in source_events:
		var event: Dictionary = (source as Dictionary).duplicate(true)
		if StringName(event.get("technique", "")) in [&"half", &"deep"]:
			events.append(event)
	resolved.resize(events.size())
	for i in resolved.size():
		resolved[i] = false

func configure(values: Dictionary) -> void:
	var half: Dictionary = values.get("half", {})
	half_release_grace_seconds = maxf(
		0.0,
		float(half.get("releaseGraceMs", half_release_grace_seconds * 1000.0)) / 1000.0
	)

func reset() -> void:
	for i in resolved.size():
		resolved[i] = false

func all_resolved() -> bool:
	for value in resolved:
		if not value:
			return false
	return true

func active_event(now: float) -> Dictionary:
	for i in events.size():
		if resolved[i]:
			continue
		var event := events[i]
		var start := float(event["time"])
		var end := start + float(event.get("duration", 0.0))
		if now >= start and now <= end:
			return event
	return {}

func upcoming_unresolved(now: float, lead_seconds: float, count: int = 2) -> Array[Dictionary]:
	var result: Array[Dictionary] = []
	if count <= 0:
		return result
	for i in events.size():
		if resolved[i]:
			continue
		var event := events[i]
		if float(event["time"]) - now > lead_seconds:
			break
		result.append({"slot": i, "event": event})
		if result.size() >= count:
			break
	return result

func resolve(intent: Dictionary) -> Dictionary:
	if intent.is_empty():
		return {"consumed": false, "valid": false, "kind": &"no_gesture", "reason": "no_gesture"}
	var technique := StringName(intent.get("technique", ""))
	var completed_at := float(intent.get("completed_at", 0.0))
	var timing_anchor := completed_at
	for i in events.size():
		if resolved[i]:
			continue
		var event := events[i]
		var start := float(event["time"])
		var end := start + float(event.get("duration", 0.0))
		if technique == &"half" and bool(intent.get("target_entered", false)):
			timing_anchor = float(intent.get("target_entered_at", completed_at))
		var release_end := end + half_release_grace_seconds if technique == &"half" else end
		if technique == StringName(event.get("technique", "")) and timing_anchor >= start and timing_anchor <= end and completed_at <= release_end:
			if not bool(intent.get("valid", false)):
				return _result(false, StringName(intent.get("reason", "invalid_gesture")), intent, event, timing_anchor - start)
			resolved[i] = true
			return _result(true, &"recognized", intent, event, timing_anchor - start)
	return _result(false, &"outside_window", intent, {}, timing_anchor - float(intent.get("started_at", completed_at)))

func expire(now: float) -> Array[Dictionary]:
	var expired: Array[Dictionary] = []
	for i in events.size():
		if resolved[i]:
			continue
		var event := events[i]
		var end := float(event["time"]) + float(event.get("duration", 0.0))
		var expiry := end + half_release_grace_seconds if StringName(event.get("technique", "")) == &"half" else end
		if now > expiry:
			resolved[i] = true
			expired.append(event)
	return expired

func debug_state(now: float) -> Dictionary:
	var current := active_event(now)
	return {
		"current": String(current.get("technique", "")).to_upper(),
		"current_id": str(current.get("id", "")),
		"active_window": not current.is_empty(),
		"resolved": resolved.count(true),
		"total": events.size()
	}

func _result(valid: bool, kind: StringName, intent: Dictionary, event: Dictionary, timing_error: float) -> Dictionary:
	return {
		"consumed": not event.is_empty(),
		"valid": valid,
		"kind": kind,
		"reason": str(intent.get("reason", kind)),
		"timing_error": timing_error,
		"timing_anchor": float(intent.get("target_entered_at", intent.get("completed_at", 0.0))) if StringName(intent.get("technique", "")) == &"half" else float(intent.get("completed_at", 0.0)),
		"intent": intent,
		"event": event
	}
