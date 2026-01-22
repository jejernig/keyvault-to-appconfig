# Research: Testing & Quality Gates

## Decision 1: Unit Test Scope

**Decision**: Unit tests must cover mapping, diff, redaction, error handling, exit codes, and deterministic ordering.

**Rationale**: These are the highest-risk logic areas that can be validated without external dependencies.

**Alternatives considered**:
- Only cover mapping and diff: rejected because redaction and exit codes are critical to security and operations.

## Decision 2: Integration Test Strategy

**Decision**: Integration tests must validate Key Vault references and App Configuration writes in a dedicated environment before release.

**Rationale**: These paths interact with Azure services and are most likely to fail due to configuration or API changes.

**Alternatives considered**:
- Smoke-only integration tests: rejected because it misses write and reference behaviors.

## Decision 3: Security and Redaction Tests

**Decision**: Security tests must assert that secret values are never written to logs or reports; redaction must be validated with representative inputs.

**Rationale**: Preventing secret leakage is a core constitutional requirement.

**Alternatives considered**:
- Manual verification only: rejected due to inconsistency and risk.

## Decision 4: CI Gates

**Decision**: CI gates require unit tests on every merge and integration tests before release (or documented waiver).

**Rationale**: Ensures constant feedback and prevents unsafe releases.

**Alternatives considered**:
- Optional integration tests: rejected because it weakens release confidence.
