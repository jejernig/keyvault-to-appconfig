# Onboarding Guide

## Purpose
Enable a new engineer to safely install, configure, and run the tool without tribal knowledge.

## Audience
Engineers onboarding to maintain or operate the Key Vault to App Configuration standardizer.

## Required Inputs (Minimum)

- `--keyvault-uri` (Key Vault URI)
- `--appconfig-endpoint` (App Configuration endpoint)

## Build and Run Locally

```text
dotnet build
```

Dry-run (default) example:

```text
dotnet run -- \
  --keyvault-uri https://<your-vault>.vault.azure.net \
  --appconfig-endpoint https://<your-appconfig>.azconfig.io \
  --dry-run \
  --report-json run-report.json
```

Apply example (safe mode):

```text
dotnet run -- \
  --keyvault-uri https://<your-vault>.vault.azure.net \
  --appconfig-endpoint https://<your-appconfig>.azconfig.io \
  --apply \
  --mode kvref
```

## Safe vs Restricted Modes

- **Safe (default)**: Key Vault reference mode (`--mode kvref` or `--mode reference`). Values are stored as references; secret values are never written to App Configuration.
- **Restricted**: Copy-value mode (`--mode copyvalue`) copies secret values and is blocked unless confirmations are provided.

Copy-value example (requires confirmations):

```text
dotnet run -- \
  --keyvault-uri https://<your-vault>.vault.azure.net \
  --appconfig-endpoint https://<your-appconfig>.azconfig.io \
  --apply \
  --mode copyvalue \
  --confirm-copy-value
```

## Correlation Identifiers and Report Counts

- Each run emits a correlation identifier for logs and reports. Supply one with `--correlation-id` or let the tool generate it.
- Reports include counts for total, changes, skips, and failures. Use these to verify expected outcomes.

## Exit Codes

- **0**: Run completes with no failures.
- **1**: One or more failures occur.

## Secret Handling and Redaction

- Do not store secret values, passwords, certificates, or API keys in repo files.
- Logs and reports redact secret values by default.
- Secret name redaction is available when enabled by configuration.

## Manual Validation Checklist

- Build succeeds locally.
- Dry-run completes with a console summary.
- Report JSON contains correlationId, totals, and failures if present.
- Copy-value mode is blocked unless confirmations are supplied.
