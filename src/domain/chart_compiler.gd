class_name HHChartCompiler
extends RefCounted

const SUPPORTED_SCHEMA_VERSION := 1
const SUPPORTED_RULES_VERSION := 1
const VALID_TECHNIQUES := ["classic", "half", "deep", "whiplash", "windmill"]
const VALID_TRAJECTORIES := ["horizontal", "vertical", "circular", "centeredge"]
const VALID_MODIFIERS := ["none", "double", "hold", "accent", "burst"]

static func load_chart(path: String) -> Dictionary:
	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		push_error("HH chart not found: %s" % path)
		return {}
	var parsed: Variant = JSON.parse_string(file.get_as_text())
	if not parsed is Dictionary:
		push_error("HH chart is not a JSON object: %s" % path)
		return {}
	return compile(parsed)

static func compile(data: Dictionary) -> Dictionary:
	var chart_id := str(data.get("chartId", "")).strip_edges()
	if chart_id.is_empty():
		push_error("HH chart is missing chartId")
		return {}

	var schema := int(data.get("schemaVersion", SUPPORTED_SCHEMA_VERSION))
	var rules := int(data.get("rulesVersion", SUPPORTED_RULES_VERSION))
	if schema == 0:
		schema = SUPPORTED_SCHEMA_VERSION
	if rules == 0:
		rules = SUPPORTED_RULES_VERSION
	if schema > SUPPORTED_SCHEMA_VERSION or rules > SUPPORTED_RULES_VERSION:
		push_error("HH chart requires unsupported schema/rules version")
		return {}

	var default_technique := _normalize_technique(str(data.get("technique", "classic")))
	if default_technique.is_empty():
		push_error("HH chart has unknown default technique")
		return {}

	var source_events: Array = data.get("events", [])
	var runtime_events: Array[Dictionary] = []
	var seen := {}
	for i in source_events.size():
		var source: Variant = source_events[i]
		if not source is Dictionary:
			push_error("HH chart event %d is not an object" % i)
			return {}
		var raw: Dictionary = source
		var event_time := float(raw.get("time", -1.0))
		if event_time < 0.0 or is_nan(event_time) or is_inf(event_time):
			push_error("HH chart event %d has invalid time" % i)
			return {}
		var event_id := str(raw.get("id", "m%04d" % i)).strip_edges()
		if event_id.is_empty():
			event_id = "m%04d" % i
		if seen.has(event_id):
			push_error("HH chart has duplicate event id: %s" % event_id)
			return {}
		seen[event_id] = true

		var direction := _parse_direction(raw)
		if direction == &"":
			push_error("HH chart event %d has an invalid direction" % i)
			return {}
		var technique := default_technique
		if raw.has("technique") and not str(raw["technique"]).strip_edges().is_empty():
			technique = _normalize_technique(str(raw["technique"]))
			if technique.is_empty():
				push_error("HH chart event %d has an unknown technique" % i)
				return {}
		var authored_duration := maxf(0.0, float(raw.get("duration", 0.0)))
		if technique in [&"half", &"deep"] and authored_duration <= 0.0:
			push_error("HH chart event %d technique window must have a positive duration" % i)
			return {}
		var default_trajectory := &"horizontal" if direction in [&"left", &"right"] else &"vertical"
		var trajectory := default_trajectory
		if raw.has("trajectory") and not str(raw["trajectory"]).strip_edges().is_empty():
			trajectory = _normalize_trajectory(str(raw["trajectory"]))
			if trajectory.is_empty():
				push_error("HH chart event %d has an unknown trajectory" % i)
				return {}
		var modifier := &"none"
		if raw.has("modifier") and not str(raw["modifier"]).strip_edges().is_empty():
			modifier = _normalize_modifier(str(raw["modifier"]))
			if modifier.is_empty():
				push_error("HH chart event %d has an unknown modifier" % i)
				return {}

		var intensity := clampf(float(raw.get("intensity", 1.0)), 0.0, 1.0)
		if intensity <= 0.0:
			intensity = 1.0
		runtime_events.append({
			"id": event_id,
			"time": event_time,
			"direction": direction,
			"technique": technique,
			"trajectory": trajectory,
			"modifier": modifier,
			"duration": authored_duration,
			"intensity": intensity,
			"finisher_candidate": bool(raw.get("finisherCandidate", false))
		})

	runtime_events.sort_custom(_event_before)

	var source_rests: Array = data.get("rests", [])
	var runtime_rests: Array[Dictionary] = []
	for i in source_rests.size():
		var raw_rest: Variant = source_rests[i]
		if not raw_rest is Dictionary:
			continue
		var rest: Dictionary = raw_rest
		var start := float(rest.get("time", -1.0))
		var duration := float(rest.get("duration", 0.0))
		var settling := float(rest.get("settlingDuration", 0.0))
		if start < 0.0 or duration <= 0.0 or settling < 0.0 or settling >= duration:
			push_error("HH chart rest %d is invalid" % i)
			return {}
		runtime_rests.append({
			"id": str(rest.get("id", "r%04d" % i)),
			"time": start,
			"duration": duration,
			"settling_duration": settling
		})

	return {
		"song_id": str(data.get("songId", "")),
		"chart_id": chart_id,
		"schema_version": schema,
		"chart_version": int(data.get("version", 1)),
		"rules_version": rules,
		"difficulty": str(data.get("difficulty", "")),
		"approach_time": maxf(0.2, float(data.get("approachTime", 1.0))),
		"events": runtime_events,
		"rests": runtime_rests
	}

static func _event_before(a: Dictionary, b: Dictionary) -> bool:
	var at := float(a["time"])
	var bt := float(b["time"])
	if not is_equal_approx(at, bt):
		return at < bt
	return str(a["id"]) < str(b["id"])

static func _parse_direction(raw: Dictionary) -> StringName:
	var explicit_name := str(raw.get("directionName", "")).strip_edges().to_lower()
	if not explicit_name.is_empty():
		match explicit_name:
			"left": return &"left"
			"right": return &"right"
			"up": return &"up"
			"down": return &"down"
			_: return &""
	match int(raw.get("direction", 0)):
		-1: return &"left"
		1: return &"right"
		2: return &"up"
		3: return &"down"
		_: return &""

static func _normalize(value: String) -> String:
	return value.strip_edges().to_lower().replace("_", "").replace("-", "").replace(" ", "")

static func _normalize_technique(value: String) -> StringName:
	var v := _normalize(value)
	if v == "classicbang":
		v = "classic"
	return StringName(v) if v in VALID_TECHNIQUES else &""

static func _normalize_trajectory(value: String) -> StringName:
	var v := _normalize(value)
	return StringName(v) if v in VALID_TRAJECTORIES else &""

static func _normalize_modifier(value: String) -> StringName:
	var v := _normalize(value)
	return StringName(v) if v in VALID_MODIFIERS else &""
