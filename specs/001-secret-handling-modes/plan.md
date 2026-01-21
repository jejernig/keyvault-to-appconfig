# Implementation Plan: Secret Handling Modes

**Branch**: `001-secret-handling-modes` | **Date**: 2026-01-21 | **Spec**: C:\repos\keyvault-to-appconfig\specs\001-secret-handling-modes\spec.md
**Input**: Feature specification from `C:\repos\keyvault-to-appconfig\specs\001-secret-handling-modes\spec.md`

## Summary

Define safe secret handling modes that default to Key Vault references, enforce copy-value guardrails, validate and resolve secret URIs, and guarantee redaction in logs and reports. The plan prioritizes least-risk behavior with explicit confirmations for higher-risk actions.

## Technical Context

**Language/Version**: C# on .NET 8.0
**Primary Dependencies**: Azure SDK (Azure.Security.KeyVault.Secrets, Azure.Data.AppConfiguration, Azure.Identity), System.CommandLine, xUnit
**Storage**: Files (reports and logs)
**Testing**: xUnit with Microsoft.NET.Test.Sdk and coverlet.collector
**Target Platform**: Cross-platform CLI (Windows, Linux, macOS)
**Project Type**: Single project (CLI + libraries)
**Performance Goals**: Resolve 10,000 secret URIs in under 3 minutes on a developer workstation
**Constraints**: Default to reference mode; no secret values logged or stored; explicit copy-value confirmations; deterministic ordering
**Scale/Scope**: Up to 10,000 secrets per run

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- No secret values logged or stored in reports; redaction rules applied.
- Dry-run and diff confirm zero writes and deterministic ordering.
- Mapping rules and naming standard documented and validated.
- Tests planned for mapping, diff logic, and redaction safeguards.
- Audit outputs and exit codes match documented contract.

Gate status: Pass (no violations).

## Project Structure

### Documentation (this feature)

```text
C:\repos\keyvault-to-appconfig\specs\001-secret-handling-modes\
|-- plan.md
|-- research.md
|-- data-model.md
|-- quickstart.md
|-- contracts\
|-- tasks.md
```

### Source Code (repository root)

```text
C:\repos\keyvault-to-appconfig\
|-- src\
|   |-- cli\
|   |   |-- KeyVaultToAppConfig.Cli\
|   |-- lib\
|   |   |-- KeyVaultToAppConfig.Core\
|   |-- services\
|       |-- KeyVaultToAppConfig.Services\
|-- tests\
|   |-- contract\
|   |-- integration\
|   |-- unit\
```

**Structure Decision**: Single project with CLI entry point plus core and services libraries, matching existing `src` layout.

## Phase 0: Outline & Research

Research outputs captured in `C:\repos\keyvault-to-appconfig\specs\001-secret-handling-modes\research.md`.

## Phase 1: Design & Contracts

Design artifacts captured in:
- `C:\repos\keyvault-to-appconfig\specs\001-secret-handling-modes\data-model.md`
- `C:\repos\keyvault-to-appconfig\specs\001-secret-handling-modes\contracts\secret-handling.openapi.yaml`
- `C:\repos\keyvault-to-appconfig\specs\001-secret-handling-modes\quickstart.md`

## Constitution Check (Post-Design)

- No secret values logged or stored in reports; redaction rules applied.
- Dry-run and diff confirm zero writes and deterministic ordering.
- Mapping rules and naming standard documented and validated.
- Tests planned for mapping, diff logic, and redaction safeguards.
- Audit outputs and exit codes match documented contract.

Gate status: Pass (no violations).
