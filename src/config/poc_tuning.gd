class_name POCTuning
extends RefCounted

## Layered POC tuning:
##   base tuning
##     + difficulty/readability profile override
##     + user/device calibration (kept outside this object by SongClock)
##
## Difficulty profiles may change timing/readability, but MUST NOT mutate neck physics.
## Chart difficulty remains authored chart metadata; these profiles are a POC tuning surface.

const BASE_PATH := "res://game/config/tuning/base.json"
const PROFILE_PATH := "res://game/config/tuning/profiles/%s.json"
const VALID_PROFILE_IDS := ["easy", "normal", "hard", "extreme"]
const DEFAULT_PROFILE := &"normal"

var data: Dictionary = {}
var base_data: Dictionary = {}
var profile_data: Dictionary = {}
var profile_id: StringName = DEFAULT_PROFILE

func load_profile(requested_profile: StringName = DEFAULT_PROFILE) -> bool:
	var normalized := StringName(str(requested_profile).strip_edges().to_lower())
	if str(normalized) not in VALID_PROFILE_IDS:
		push_warning("HH unknown tuning profile '%s'; falling back to normal" % str(requested_profile))
		normalized = DEFAULT_PROFILE

	var loaded_base := _load_json_object(BASE_PATH)
	var loaded_profile := _load_json_object(PROFILE_PATH % str(normalized))
	if loaded_base.is_empty() or loaded_profile.is_empty():
		data = {}
		base_data = {}
		profile_data = {}
		profile_id = normalized
		return false

	base_data = loaded_base
	profile_data = loaded_profile
	data = _deep_merge(base_data, profile_data)
	profile_id = normalized
	return true

func reload() -> bool:
	return load_profile(profile_id)

func section(name: String) -> Dictionary:
	var value: Variant = data.get(name, {})
	return value if value is Dictionary else {}

func number(section_name: String, key: String, fallback: float) -> float:
	var values := section(section_name)
	if not values.has(key):
		return fallback
	return float(values[key])

func integer(section_name: String, key: String, fallback: int) -> int:
	var values := section(section_name)
	if not values.has(key):
		return fallback
	return int(values[key])

func resolved_snapshot() -> Dictionary:
	return data.duplicate(true)

static func is_valid_profile(value: StringName) -> bool:
	return str(value).strip_edges().to_lower() in VALID_PROFILE_IDS

static func _load_json_object(path: String) -> Dictionary:
	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		push_warning("HH tuning file not found: %s" % path)
		return {}
	var parsed: Variant = JSON.parse_string(file.get_as_text())
	if not parsed is Dictionary:
		push_warning("HH tuning file is not a JSON object: %s" % path)
		return {}
	return (parsed as Dictionary).duplicate(true)

static func _deep_merge(base: Dictionary, override: Dictionary) -> Dictionary:
	var result := base.duplicate(true)
	for key in override.keys():
		var incoming: Variant = override[key]
		if result.has(key) and result[key] is Dictionary and incoming is Dictionary:
			result[key] = _deep_merge(result[key] as Dictionary, incoming as Dictionary)
		elif incoming is Dictionary:
			result[key] = (incoming as Dictionary).duplicate(true)
		elif incoming is Array:
			result[key] = (incoming as Array).duplicate(true)
		else:
			result[key] = incoming
	return result
