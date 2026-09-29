class_name RunDiagnostics
extends RefCounted

## Runtime-only tuning telemetry. Never participates in judgment, scoring or progression.
## It records only player inputs that actually resolved/consumed an authored candidate.
## Expired events and free bangs are intentionally excluded from timing-bias statistics.

var samples: int = 0
var signed_error_sum: float = 0.0
var absolute_error_sum: float = 0.0
var early_count: int = 0
var late_count: int = 0
var last_error: float = 0.0
var last_judgment: StringName = &""

func reset() -> void:
	samples = 0
	signed_error_sum = 0.0
	absolute_error_sum = 0.0
	early_count = 0
	late_count = 0
	last_error = 0.0
	last_judgment = &""

func record(judgment: StringName, signed_error_seconds: float) -> void:
	samples += 1
	signed_error_sum += signed_error_seconds
	absolute_error_sum += absf(signed_error_seconds)
	last_error = signed_error_seconds
	last_judgment = judgment
	if signed_error_seconds < 0.0:
		early_count += 1
	elif signed_error_seconds > 0.0:
		late_count += 1

func snapshot() -> Dictionary:
	var mean_signed := signed_error_sum / samples if samples > 0 else 0.0
	var mean_absolute := absolute_error_sum / samples if samples > 0 else 0.0
	return {
		"samples": samples,
		"mean_signed_ms": mean_signed * 1000.0,
		"mean_absolute_ms": mean_absolute * 1000.0,
		"early": early_count,
		"late": late_count,
		"last_error_ms": last_error * 1000.0,
		"last_judgment": last_judgment
	}
