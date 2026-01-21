# Research: App Configuration Writes

## Decision 1: Idempotent write strategy

- Decision: Skip writes when key, label, value, and content type match desired state; write only create/update actions.
- Rationale: Minimizes churn and supports safe re-runs.
- Alternatives considered: Always overwrite values; rejected due to unnecessary writes.

## Decision 2: Label handling defaults

- Decision: Preserve explicit labels; apply environment label when provided; otherwise use empty label.
- Rationale: Prevents cross-environment drift while keeping behavior predictable.
- Alternatives considered: Fail when label missing; rejected to avoid blocking safe defaults.

## Decision 3: Tag application rules

- Decision: Apply managed tags on write, preserve unmanaged tags unless explicitly overridden.
- Rationale: Maintains governance metadata without deleting user-managed tags.
- Alternatives considered: Replace all tags; rejected for causing unintended metadata loss.

## Decision 4: Concurrency and retry policy

- Decision: Use bounded concurrency with configurable max parallelism; retry transient failures with backoff and a bounded retry count.
- Rationale: Protects service limits while improving throughput and resilience.
- Alternatives considered: Unbounded parallelism; rejected for risk of throttling.

## Decision 5: Failure handling and rollback

- Decision: Surface partial failures with non-zero exit status and per-entry results; support rollback only when explicitly requested and pre-write state is available.
- Rationale: Avoids unsafe implicit rollback while enabling controlled recovery.
- Alternatives considered: Automatic rollback on any failure; rejected due to potential compounding errors.
