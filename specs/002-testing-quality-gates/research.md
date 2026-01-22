# Research: Testing & Quality Gates

## Decision 1: Define unit test scope around high-risk paths

**Decision**: Use a high-risk path catalog to drive unit test scope (mapping, diff, redaction, error handling, exit codes).
**Rationale**: Ensures coverage aligns with the most failure-prone and safety-critical behaviors.
**Alternatives considered**: Broad unit coverage without explicit risk mapping (rejected for lack of focus and traceability).

## Decision 2: Integration tests required for release validation

**Decision**: Require integration tests for release, with documented prerequisites and waiver policy.
**Rationale**: Live-service interactions must be validated to avoid unsafe releases.
**Alternatives considered**: Optional integration tests (rejected due to operational risk).

## Decision 3: Security/redaction tests and CI gates are explicit requirements

**Decision**: Define security/redaction tests and CI gate rules as mandatory for release.
**Rationale**: Prevents secret leakage and enforces audit-ready behavior.
**Alternatives considered**: Informal or ad-hoc security checks (rejected due to compliance risk).
