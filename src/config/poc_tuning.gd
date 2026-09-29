class_name POCTuning
extends RefCounted

## Single human-editable POC tuning surface.
## This deliberately separates tunable values from gameplay/presentation algorithms.
## Missing/invalid keys fall back to the values that shipped in the first Godot port.

const DEFAULT_PATH := "res://game/config/poc_tuning.json"

var data: Dictionary = {}

func load_from_file(path: String = DEFAULT_PATH) -> bool:
	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		push_warning("HH tuning file not found, using code defaults: %s" % path)
		data = {}
		return false
	var parsed: Variant = JSON.parse_string(file.get_as_text())
	if not parsed is Dictionary:
		push_warning("HH tuning file is invalid JSON object, using code defaults: %s" % path)
		data = {}
		return false
	data = parsed
	return true

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
