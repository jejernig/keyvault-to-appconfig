# Research: Developer Documentation & Onboarding

## Decision 1: Primary documentation locations

- **Decision**: Use README.md for quick start and docs/ for detailed onboarding and troubleshooting.
- **Rationale**: README provides immediate entry point; docs/ keeps detailed content organized.
- **Alternatives considered**: Single README (too long), external wiki (drifts from code).

## Decision 2: Identity configuration guidance

- **Decision**: Document Managed Identity, Workload Identity, and local dev credential flows with required environment variables.
- **Rationale**: Covers cloud and local developer workflows without storing secrets.
- **Alternatives considered**: Only local dev credentials (not secure), only managed identity (not developer-friendly).

## Decision 3: Safe vs restricted mode guidance

- **Decision**: Document Key Vault reference as default and copy-value as restricted with explicit confirmations.
- **Rationale**: Reduces risk of accidental secret exposure.
- **Alternatives considered**: Single mode guidance (insufficient safety context).

## Decision 4: Troubleshooting format

- **Decision**: Provide a table of common failures with causes and fixes plus example error output.
- **Rationale**: Quick diagnosis for onboarding.
- **Alternatives considered**: Narrative troubleshooting (harder to scan).
