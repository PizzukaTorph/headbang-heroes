class_name MotionQuality
extends RefCounted

const REFERENCE_SPEED := 220.0
const REFERENCE_TRAVEL := 24.0
const TRAVEL_WEIGHT := 0.6
const SETUP_QUALITY := 0.4

static func evaluate(snapshot: Dictionary) -> Dictionary:
	if not bool(snapshot.get("prepared", false)):
		return {"quality": SETUP_QUALITY, "travel_component": 0.0, "speed_component": 0.0, "was_setup": true}
	var speed_component := clampf(absf(float(snapshot.get("velocity", 0.0))) / REFERENCE_SPEED, 0.0, 1.0)
	var travel_component := clampf(float(snapshot.get("travel", 0.0)) / REFERENCE_TRAVEL, 0.0, 1.0)
	var quality := clampf(TRAVEL_WEIGHT * travel_component + (1.0 - TRAVEL_WEIGHT) * speed_component, 0.0, 1.0)
	return {"quality": quality, "travel_component": travel_component, "speed_component": speed_component, "was_setup": false}
