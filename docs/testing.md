# Testing & Quality Gates

## Purpose

Define the required test scope, high-risk paths, and CI gates to protect the tool against regressions and security leaks.

## CI Gate Summary

- **Merge gate**: Unit tests required, no failures.
- **Release gate**: Unit + integration + security/redaction tests required.
- **Waivers**: Allowed only with documented approval, scope, and expiry.

## High-Risk Path Catalog

| Risk Path | Description | Required Suites |
|----------|-------------|-----------------|
| Mapping/Transforms | Mapping rules and transforms produce App Configuration keys | Unit |
| Diff/Plan Accuracy | Diff classification and dry-run outputs | Unit |
| Secret Redaction | Logs and reports must never include secret values | Unit, Security |
| Copy-Value Guardrails | Copy-value mode requires explicit confirmation | Unit, Security |
| Write Path | Apply writes to App Configuration safely | Integration |
| Exit Code Contract | Exit codes reflect success/partial/fatal/canceled | Unit |

## Test Scope Matrix

| Category | Scope | Required |
|----------|-------|----------|
| Unit | mapping, diff, redaction, error handling, exit codes, deterministic ordering | Yes |
| Integration | Key Vault reference resolution, App Configuration writes | Yes (release) |
| Security | redaction coverage, copy-value guardrails | Yes (release) |

## Unit Test Coverage Checklist

- Mapping rules and transforms
- Diff classification logic
- Redaction rules (logs and reports)
- Error handling and exit code mapping
- Deterministic ordering of outputs

## Integration Test Strategy

**Prerequisites**:
- Dedicated Azure Key Vault and App Configuration instances
- Managed identity or workload identity configured
- Required RBAC roles granted

**Gate & Waiver Policy**:
- Integration tests are required for release validation.
- Waivers require documented approval, scope, and expiry and must be referenced in release notes.
- Waiver approvals must be recorded by a release approver (release manager or security reviewer) with evidence links.

**Run Instructions**:
- Use the integration test project in `tests/integration/KeyVaultToAppConfig.IntegrationTests/`
- Ensure environment variables and credentials are set per project docs

## Security & Redaction Test Scenarios

- Secrets in input mappings never appear in logs or JSON reports.
- Secret names are redacted when configured.
- Copy-value mode is blocked unless confirmation flag is provided.

## Security Verification Checklist

- Redaction tests cover logs, console output, and JSON reports.
- Copy-value guardrail tests require explicit confirmation flags.
- Secret URI resolution tests ensure no raw values are logged.
- Failure paths do not emit secret material.

## Reproducibility Criteria

- Given the same inputs, test prerequisites, and environment configuration, the required suites and gate outcomes are identical.
- Approved sources of variability (e.g., service unavailability) must be documented as waiverable exceptions.

## Unit Coverage to Risk Paths

| Risk Path | Unit Test Suites | Notes |
|----------|------------------|-------|
| Mapping/Transforms | `tests/unit/KeyVaultToAppConfig.UnitTests/Mapping/` | Covers mapping rules and transforms. |
| Diff/Plan Accuracy | `tests/unit/KeyVaultToAppConfig.UnitTests/Planning/` | Covers diff classification and dry-run logic. |
| Secret Redaction | `tests/unit/KeyVaultToAppConfig.UnitTests/Secrets/SecretRedactionTests.cs` | Verifies log/report redaction. |
| Copy-Value Guardrails | `tests/unit/KeyVaultToAppConfig.UnitTests/Secrets/CopyValueGuardrailTests.cs` | Requires explicit confirmation flags. |
| Write Path | `tests/unit/KeyVaultToAppConfig.UnitTests/Writes/` | Validates write planning and apply decisions. |
| Exit Code Contract | `tests/unit/KeyVaultToAppConfig.UnitTests/ErrorHandling/` | Ensures exit codes map to outcomes. |

## CI Gate Checklist & Enforcement

- Merge: `dotnet test` (unit + contract) must pass.
- Release: unit + contract + integration + security suites must pass.
- Any skipped integration/security suite requires a documented waiver.

## Test Evidence

Record test runs by linking CI logs or test reports in release notes.
Redaction and guardrail coverage evidence (unit tests):
- `tests/unit/KeyVaultToAppConfig.UnitTests/Secrets/SecretRedactionTests.cs`
- `tests/unit/KeyVaultToAppConfig.UnitTests/Secrets/SecretModeCommandTests.cs`
- `tests/unit/KeyVaultToAppConfig.UnitTests/Secrets/SecretUriResolverTests.cs`
- `tests/unit/KeyVaultToAppConfig.UnitTests/Secrets/CopyValueGuardrailTests.cs`
