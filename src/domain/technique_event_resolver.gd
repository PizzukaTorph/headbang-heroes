class_name TechniqueEventResolver
extends RefCounted

## Resolves semantic gesture completion against authored Technique Skill windows.
## It does not score and it does not apply neck motion.

var events: Array[Dictionary] = []
var resolved: Array[bool] = []

func _init(source_events: Array = []) -> void:
	for source in source_events:
		var event: Dictionary = (source as Dictionary).duplicate(true)
		if StringName(event.get("technique", "")) in [&"half", &"deep"]:
			events.append(event)
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
	for i in events.size():
		if resolved[i]:
			continue
		var event := events[i]
		var start := float(event["time"])
		var end := start + float(event.get("duration", 0.0))
		if technique == StringName(event.get("technique", "")) and completed_at >= start and completed_at <= end:
			if not bool(intent.get("valid", false)):
				return _result(false, &"invalid_gesture", intent, event)
			resolved[i] = true
			return _result(true, &"recognized", intent, event)
	return _result(false, &"outside_window", intent, {})

func expire(now: float) -> Array[Dictionary]:
	var expired: Array[Dictionary] = []
	for i in events.size():
		if resolved[i]:
			continue
		var end := float(events[i]["time"]) + float(events[i].get("duration", 0.0))
		if now > end:
			resolved[i] = true
			expired.append(events[i])
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

func _result(valid: bool, kind: StringName, intent: Dictionary, event: Dictionary) -> Dictionary:
	return {
		"consumed": not event.is_empty(),
		"valid": valid,
		"kind": kind,
		"reason": str(intent.get("reason", kind)),
		"intent": intent,
		"event": event
	}
