# Implementation Plan: Mapping & Normalization Engine

**Branch**: `001-mapping-normalization-engine` | **Date**: 2026-01-20 | **Spec**: C:\repos\keyvault-to-appconfig\specs\001-mapping-normalization-engine\spec.md
**Input**: Feature specification from `C:\repos\keyvault-to-appconfig\specs\001-mapping-normalization-engine\spec.md`

## Summary

Define a deterministic mapping and normalization engine that accepts YAML/JSON specs, validates them against a strict schema, applies direct and regex strategies with ordered transforms, and reports collisions. The technical approach normalizes YAML to JSON for schema validation, uses a deterministic rule evaluation order, and produces structured outputs and reports without logging secret values.

## Technical Context

**Language/Version**: C# on .NET 8.0  
**Primary Dependencies**: Azure SDK (Azure.Core, Azure.Identity, Azure.Security.KeyVault.Secrets), System.CommandLine, xUnit  
**Storage**: Files (mapping specifications and reports)  
**Testing**: xUnit with Microsoft.NET.Test.Sdk and coverlet.collector  
**Target Platform**: Cross-platform CLI (Windows, Linux, macOS)  
**Project Type**: Single project (CLI + libraries)  
**Performance Goals**: Normalize 10,000 keys in under 2 seconds on a developer workstation  
**Constraints**: Deterministic ordering; no secret values logged; collision reporting must be complete and reproducible  
**Scale/Scope**: Up to 5,000 mapping rules and 10,000 input keys per run

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
C:\repos\keyvault-to-appconfig\specs\001-mapping-normalization-engine\
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

Research outputs captured in `C:\repos\keyvault-to-appconfig\specs\001-mapping-normalization-engine\research.md`.

## Phase 1: Design & Contracts

Design artifacts captured in:
- `C:\repos\keyvault-to-appconfig\specs\001-mapping-normalization-engine\data-model.md`
- `C:\repos\keyvault-to-appconfig\specs\001-mapping-normalization-engine\contracts\mapping-engine.openapi.yaml`
- `C:\repos\keyvault-to-appconfig\specs\001-mapping-normalization-engine\quickstart.md`

## Constitution Check (Post-Design)

- No secret values logged or stored in reports; redaction rules applied.
- Dry-run and diff confirm zero writes and deterministic ordering.
- Mapping rules and naming standard documented and validated.
- Tests planned for mapping, diff logic, and redaction safeguards.
- Audit outputs and exit codes match documented contract.

Gate status: Pass (no violations).
