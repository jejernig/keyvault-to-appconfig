# Research: Error Handling & Resilience

## Decision 1: Error Classification Rules

**Decision**: Classify errors as fatal when they prevent safe processing for all secrets (configuration, authentication/authorization, startup initialization, or systemic connection failures). Classify errors as recoverable when they are isolated to a single secret operation.

**Rationale**: This preserves per-secret isolation while ensuring global failures are surfaced clearly and stop unsafe processing.

**Alternatives considered**:
- Treat all errors as recoverable: rejected because systemic failures would be misreported and could lead to partial, unsafe runs.
- Fail-fast on any error: rejected because it reduces progress and conflicts with the per-secret isolation principle.

## Decision 2: Per-Secret Isolation Defaults

**Decision**: Continue processing other secrets when a recoverable error occurs. If fail-fast is explicitly enabled, stop after the first recoverable error and mark remaining secrets as unprocessed.

**Rationale**: Default isolation maximizes progress while still surfacing failures; fail-fast remains an opt-in for strict pipelines.

**Alternatives considered**:
- Always fail-fast: rejected because it reduces throughput and contradicts resilience goals.
- Always continue, no fail-fast option: rejected because some deployments require strict fail-fast behavior.

## Decision 3: Cancellation and Shutdown Handling

**Decision**: On cancellation, stop starting new secret operations, allow in-flight work to complete where safe, and mark remaining secrets as unprocessed. Report the run outcome as canceled.

**Rationale**: Ensures predictable shutdown behavior without leaving ambiguous run outcomes.

**Alternatives considered**:
- Immediate termination of all work: rejected because it could leave partial state without clear reporting.
- Ignore cancellation until the end: rejected because it delays operator intent and can waste time.

## Decision 4: Exit Code Mapping

**Decision**: Use explicit exit codes to distinguish outcomes: 0 = success, 1 = recoverable failures, 2 = fatal failure, 3 = canceled.

**Rationale**: Clear differentiation enables reliable automation and matches the requirement for non-silent failures.

**Alternatives considered**:
- Single non-zero code for all failures: rejected because it obscures cancellation vs failure causes.
- Range-based codes for specific errors: rejected because it adds complexity without clear operator value.

## Decision 5: Deterministic Reporting

**Decision**: Record and report errors and secret outcomes in the same order as the input mapping and processing order.

**Rationale**: Ensures deterministic and reproducible outcomes for identical inputs.

**Alternatives considered**:
- Sort by error type or timestamp: rejected because it can re-order results between runs.
