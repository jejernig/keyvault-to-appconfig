# Research: Secret Handling Modes

## Decision 1: Default mode selection

- Decision: Default to Key Vault reference mode when mode is not specified.
- Rationale: Least-risk behavior with no secret values written.
- Alternatives considered: Default to copy-value; rejected due to higher risk.

## Decision 2: Secret URI resolution rules

- Decision: Validate URI format, require Key Vault host, resolve missing versions to latest enabled version, and record resolved version ID in metadata.
- Rationale: Ensures deterministic references without leaking secret values.
- Alternatives considered: Require explicit version always; rejected for operational friction.

## Decision 3: Copy-value guardrails

- Decision: Require explicit flag plus secondary confirmation; restrict copy-value to explicitly allowed entries.
- Rationale: Prevents accidental leakage and enforces intent.
- Alternatives considered: Single flag only; rejected for insufficient guardrails.

## Decision 4: Redaction guarantees

- Decision: Redact secret values in logs, reports, and error messages in all modes.
- Rationale: Logging is a common exfiltration path.
- Alternatives considered: Redact only in copy-value; rejected as incomplete.
