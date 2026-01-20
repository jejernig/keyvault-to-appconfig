<!--
Sync Impact Report
- Version change: N/A -> 1.0.0
- Modified principles: None (initial adoption)
- Added sections: Core Principles; Security and Compliance Requirements; Development Workflow and Quality Gates; Governance
- Removed sections: None
- Templates requiring updates: ? updated - .specify/templates/plan-template.md; ? updated - .specify/templates/tasks-template.md; ? verified - .specify/templates/spec-template.md
- Follow-up TODOs: TODO(RATIFICATION_DATE): original adoption date not found in repository
-->
# Key Vault to App Configuration Standardizer Constitution

## Core Principles

### I. Secret Safety and Least Privilege
- Secret values MUST never be logged, printed, or written to reports.
- Copy-value mode MUST require an explicit flag and a secondary confirmation input.
- Access scopes MUST be least-privilege for both Key Vault and App Configuration.
Rationale: Prevents accidental leakage and reduces blast radius.

### II. Deterministic, Idempotent Runs
- Given the same inputs, runs MUST be deterministic and order-stable.
- Dry-run and diff modes MUST perform zero writes.
- Apply mode MUST be idempotent and update only when changes are detected.
Rationale: Enables safe automation and predictable CI/CD behavior.

### III. Explicit Mapping and Naming Standards
- Mapping files are the source of truth; convention fallbacks MUST be explicit.
- Keys MUST follow the agreed standard format; labels carry environment context.
- Mapping rules MUST be validated with clear, actionable errors.
Rationale: Consistent keys reduce drift and simplify operations.

### IV. Audit-Ready Observability
- Logs MUST support structured output and correlation identifiers.
- Reports MUST include counts, changes, skips, and failures without secret data.
- Exit codes MUST follow the documented contract.
Rationale: Enables auditability and operational accountability.

### V. Resilient Failure Handling
- Failures MUST be isolated per secret unless fail-fast is enabled.
- Retries MUST use Azure SDK policies with backoff and respect cancellation.
- Partial failures MUST be surfaced with a non-zero exit code.
Rationale: Maximizes progress while keeping failures visible.

## Security and Compliance Requirements

- Use DefaultAzureCredential with managed identity and workload identity support.
- Document required RBAC roles for Key Vault and App Configuration access.
- Support private endpoints and avoid hard-coded endpoints or secrets.
- Redaction rules MUST cover secret names when enabled by configuration.
- Key/value tags MUST mark managed entries with source and timestamp metadata.

## Development Workflow and Quality Gates

- Unit tests MUST cover mapping, diff logic, and redaction safeguards.
- Integration tests MUST validate Key Vault references and App Config writes
  before release.
- PRs MUST include a Constitution Check summary and any deviations.
- Release artifacts MUST be reproducible with documented build steps.

## Governance

- This constitution supersedes all other project guidance and templates.
- Amendments require a documented rationale, impact notes, and reviewer approval.
- Versioning follows semantic rules: MAJOR for breaking governance, MINOR for
  new principles or material expansions, PATCH for clarifications.
- Compliance is reviewed in PRs; deviations require a documented waiver.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): original adoption date not found in repository | **Last Amended**: 2026-01-20
