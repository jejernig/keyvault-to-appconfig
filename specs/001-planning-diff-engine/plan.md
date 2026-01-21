# Implementation Plan: Planning & Diff Engine

**Branch**: `001-planning-diff-engine` | **Date**: 2026-01-20 | **Spec**: C:\repos\keyvault-to-appconfig\specs\001-planning-diff-engine\spec.md
**Input**: Feature specification from `C:\repos\keyvault-to-appconfig\specs\001-planning-diff-engine\spec.md`

## Summary

Define a planning and diff engine that constructs desired state, reads existing App Configuration state, computes deterministic diffs, and guarantees no writes in planning or dry-run modes. The approach uses consistent ordering, explicit conflict detection, and structured plan outputs to support safe reviews.

## Technical Context

**Language/Version**: C# on .NET 8.0  
**Primary Dependencies**: Azure SDK (Azure.Data.AppConfiguration), System.CommandLine, xUnit  
**Storage**: Files (plan and diff reports)  
**Testing**: xUnit with Microsoft.NET.Test.Sdk and coverlet.collector  
**Target Platform**: Cross-platform CLI (Windows, Linux, macOS)  
**Project Type**: Single project (CLI + libraries)  
**Performance Goals**: Compute diff for 10,000 items in under 5 seconds on a developer workstation  
**Constraints**: Planning and dry-run must perform zero writes; deterministic ordering; no secret values logged  
**Scale/Scope**: Up to 10,000 desired items and 10,000 existing items per run
**Logging & Reports**: Structured logs with correlation IDs; report outputs include counts, changes, skips, and failures without secret values  
**Scope Inputs**: CLI surfaces key prefix, labels list, page size, and continuation token for plan/diff/dry-run

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
C:\repos\keyvault-to-appconfig\specs\001-planning-diff-engine\
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

Research outputs captured in `C:\repos\keyvault-to-appconfig\specs\001-planning-diff-engine\research.md`.

## Phase 1: Design & Contracts

Design artifacts captured in:
- `C:\repos\keyvault-to-appconfig\specs\001-planning-diff-engine\data-model.md`
- `C:\repos\keyvault-to-appconfig\specs\001-planning-diff-engine\contracts\planning-diff.openapi.yaml`
- `C:\repos\keyvault-to-appconfig\specs\001-planning-diff-engine\quickstart.md`

## Constitution Check (Post-Design)

- No secret values logged or stored in reports; redaction rules applied.
- Dry-run and diff confirm zero writes and deterministic ordering.
- Mapping rules and naming standard documented and validated.
- Tests planned for mapping, diff logic, and redaction safeguards.
- Audit outputs and exit codes match documented contract.

Gate status: Pass (no violations).
