# Quickstart: Error Handling & Resilience

## Goal

Validate deterministic outcomes, per-secret isolation, cancellation handling, and exit code mapping.

## Scenario

1. Prepare a mapping with at least three secrets:
   - One valid secret.
   - One secret expected to fail recoverably (e.g., missing or invalid secret reference).
   - One secret that would be processed after the failing secret.
2. Run the tool in a mode that exercises normal processing.
3. Confirm the run report:
   - Shows one success, one recoverable failure, and one successful or unprocessed secret depending on behavior.
   - Uses a non-zero exit code for recoverable failures.
4. Trigger a cancellation during a run (if supported) and verify:
   - The run outcome is Canceled.
   - Unprocessed secrets are clearly marked.
   - Exit code matches the cancellation contract.

## Expected Results

- Outcomes are deterministic across repeated runs with identical inputs.
- No secret values appear in logs or reports.
- Exit codes match the documented contract.

## Validation Status

- Not run (requires local Azure credentials and test resources).
