# Research Notes: CLI and Execution Control

## Decision 1: Platform and runtime
- **Decision**: Use C# on .NET 8 (LTS) for a cross-platform console tool.
- **Rationale**: Aligns with PRD and provides long-term support and tooling.
- **Alternatives considered**: Earlier .NET versions; rejected for shorter support.

## Decision 2: CLI parsing
- **Decision**: Use System.CommandLine for argument parsing, help, and validation.
- **Rationale**: Maintains consistent CLI behavior and validation in .NET.
- **Alternatives considered**: Custom parsing or third-party CLI frameworks;
  rejected to reduce dependency surface.

## Decision 3: Deterministic output and ordering
- **Decision**: Normalize and sort outputs by key, label, and action before
  emitting summaries or reports.
- **Rationale**: Ensures stable output for CI/CD comparisons.
- **Alternatives considered**: Preserve provider order; rejected due to
  non-determinism.

## Decision 4: Safety and non-interactive defaults
- **Decision**: Default to non-writing behavior; require explicit `--apply` to
  write, and a separate explicit confirmation flag for copy-value mode.
- **Rationale**: Matches constitution safety and prevents accidental writes.
- **Alternatives considered**: Interactive prompts; rejected for CI/CD safety.

## Decision 5: Logging and reporting
- **Decision**: Structured logs via Microsoft.Extensions.Logging with optional
  JSON report output that excludes secret values.
- **Rationale**: Supports auditability and machine-readable outputs.
- **Alternatives considered**: Plain text only; rejected for audit needs.

## Decision 6: Authentication defaults
- **Decision**: Default to Azure DefaultAzureCredential behavior with least
  privilege access for Key Vault and App Configuration.
- **Rationale**: Aligns with PRD and best practices.
- **Alternatives considered**: Hard-coded credentials; rejected for security.
