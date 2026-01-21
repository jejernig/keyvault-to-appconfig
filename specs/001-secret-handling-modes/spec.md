# Feature Specification: Secret Handling Modes

**Feature Branch**: `001-secret-handling-modes`  
**Created**: 2026-01-21  
**Status**: Draft  
**Input**: User description: "Act as a Security-Focused Engineer. Specify features for Secret Handling Modes. For each feature: Define Key Vault reference mode behavior Define secret URI resolution rules Define copy-value mode behavior Define explicit guardrails for secret value handling Define logging redaction guarantees Define acceptance criteria Default to least-risk behavior."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Use Key Vault reference mode safely (Priority: P1)

As a security-focused operator, I run in Key Vault reference mode so secrets remain in Key Vault and App Configuration stores only references.

**Why this priority**: Reference mode is the least-risk default and the primary security posture.

**Independent Test**: Can be fully tested by running a plan/apply with reference mode and verifying no secret values are written or logged.

**Acceptance Scenarios**:

1. **Given** Key Vault reference mode, **When** secrets are processed, **Then** App Configuration entries contain references only and no secret values are written.
2. **Given** Key Vault reference mode and valid secret URIs, **When** resolution runs, **Then** references are built using the Key Vault reference format defined in the Reference Format section (value contains the full versioned secret URI) and stored without secret values.

---

### User Story 2 - Use copy-value mode with guardrails (Priority: P2)

As a security-focused operator, I enable copy-value mode only with explicit confirmation so I can copy values when absolutely required.

**Why this priority**: Copy-value introduces higher risk and must be tightly controlled.

**Independent Test**: Can be fully tested by attempting copy-value without confirmations and verifying the run is blocked, then enabling confirmations to allow the run.

**Acceptance Scenarios**:

1. **Given** copy-value mode without confirmation, **When** execution starts, **Then** the run is blocked with a clear error explaining required confirmations.
2. **Given** copy-value mode with required confirmations, **When** execution starts, **Then** values are written only for explicitly allowed entries and never logged.

---

### User Story 3 - Prove redaction and audit safety (Priority: P3)

As a security-focused operator, I need consistent redaction guarantees so no secret values appear in logs or reports.

**Why this priority**: Logging and reporting are common leak paths and must be protected.

**Independent Test**: Can be fully tested by generating logs/reports with secret-like inputs and verifying redaction.

**Acceptance Scenarios**:

1. **Given** any execution mode, **When** logs or reports are produced, **Then** secret values are redacted and never appear in output.
2. **Given** copy-value mode, **When** errors occur, **Then** error messages are redacted and do not expose secret values.
3. **Given** any execution mode, **When** the run completes, **Then** structured logs and reports include a correlation identifier and per-item outcomes (mode, guardrail status, allowed-key enforcement, resolved version metadata when applicable) without exposing secret values.
4. **Given** any execution mode, **When** a report is produced, **Then** it includes aggregate counts for total items, changes, skips, and failures without exposing secret values.
5. **Given** secret-name redaction is enabled, **When** logs or reports are produced, **Then** secret names are redacted consistently.
6. **Given** the run completes with failures, **When** the process exits, **Then** the exit code reflects failure as documented without exposing secret values.

---

### Edge Cases

- **Malformed or unsupported secret URI**: The system rejects the entry with a clear, non-secret error and marks the item as failed.
- **Secret URI without version**: The system resolves to the latest enabled version and records the resolved version identifier in non-secret metadata.
- **Mixed modes in one run**: The system applies the least-risk default (reference mode) unless copy-value confirmations are explicitly provided.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST default to Key Vault reference mode when no explicit mode is provided.
- **FR-002**: The system MUST build App Configuration values as Key Vault references in reference mode and MUST NOT write secret values.
- **FR-003**: The system MUST validate secret URIs and reject malformed or unsupported formats with non-secret errors.
- **FR-004**: The system MUST resolve secret URIs without versions to the latest enabled version and capture the resolved version identifier in metadata.
- **FR-005**: The system MUST require explicit flags and secondary confirmation before enabling copy-value mode.
- **FR-006**: The system MUST restrict copy-value writes to explicitly allowed entries and MUST block all others.
- **FR-007**: The system MUST redact secret values in all logs, reports, and error messages.
- **FR-008**: The system MUST surface mode selection, guardrail status, and redaction status in execution results without exposing secret values.
- **FR-009**: The system MUST emit structured logs and reports that include a correlation identifier and per-item outcomes (mode, allowed-key enforcement result, resolved version metadata when applicable) without exposing secret values.
- **FR-010**: The system MUST include aggregate counts (total items, changes, skips, failures) in reports without exposing secret values.
- **FR-011**: The system MUST support configurable redaction of secret names in logs and reports when enabled.
- **FR-012**: The system MUST follow the exit code contract defined in this specification for success and failure outcomes.

### Key Entities *(include if feature involves data)*

- **Secret Handling Mode**: Selected behavior for reference or copy-value operations.
- **Secret URI Resolution**: The process of validating and resolving a secret URI to a versioned reference.
- **Guardrail Confirmation**: Explicit flags and confirmations required to enable copy-value mode.
- **Redaction Policy**: Rules that ensure secret values are never emitted in outputs.

### Reference Format

- **Key Vault Reference Value**: A JSON object with a single `uri` field set to the full versioned secret URI (for example, `{"uri":"https://{vault}.vault.azure.net/secrets/{name}/{version}"}`).

### Exit Code Contract

- **Exit Code 0**: All items succeed and guardrails are satisfied.
- **Exit Code 1**: One or more items fail or a guardrail blocks execution.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of reference-mode runs produce only Key Vault reference values with zero secret values written.
- **SC-002**: 100% of copy-value runs are blocked unless both required confirmations are provided.
- **SC-003**: 100% of logs and reports redact secret values in test fixtures.
- **SC-004**: 100% of malformed or unsupported secret URIs are rejected with non-secret errors.
- **SC-005**: 100% of runs produce structured logs and reports that include a correlation identifier and per-item outcomes without exposing secret values.
- **SC-006**: 100% of reports include aggregate counts for total items, changes, skips, and failures without exposing secret values.
- **SC-007**: 100% of runs honor secret-name redaction when enabled in test fixtures.
- **SC-008**: 100% of runs exit with the documented success or failure code based on outcomes.
- **SC-009**: 10,000 secret URIs are processed in under 3 minutes on a developer workstation in reference mode.

## Assumptions

- App Configuration supports Key Vault reference values for the target environment.
- Operators can supply explicit confirmations when copy-value is required.
- Secret URI inputs are provided through validated configuration or upstream tooling.
