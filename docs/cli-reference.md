# CLI Reference

## Purpose
Provide required and optional CLI arguments with defaults and examples.

## Core Arguments

| Argument | Required | Default | Description |
|----------|----------|---------|-------------|
| --keyvault-uri | Yes | N/A | Key Vault URI (https://<vault>.vault.azure.net). |
| --appconfig-endpoint | Yes | N/A | App Configuration endpoint (https://<name>.azconfig.io). |
| --dry-run | No | false | Evaluate without writing changes. |
| --diff | No | false | Show differences without writing changes. |
| --apply | No | false | Apply changes. |
| --mode | No | kvref | Secret handling mode (kvref, reference, copyvalue). |
| --confirm-copy-value | No | false | Required to enable copy-value mode. |
| --environment | No | (empty) | Environment label context. |
| --mapping-file | No | (none) | Mapping specification file path. |
| --parallelism | No | 1 | Degree of parallelism. |
| --include-prefix | No | (none) | Filter by prefix. |
| --exclude-regex | No | (none) | Regex for exclusions. |
| --only-tag | No | (none) | Filter by tag (key=value). |
| --enabled-only | No | false | Only enabled secrets. |
| --version-mode | No | latest | Secret version mode (latest, explicit). |
| --version-map | No | (none) | Version map file path (required for explicit). |
| --page-size | No | (none) | Page size for enumeration. |
| --continuation-token | No | (none) | Continuation token for enumeration. |
| --report-json | No | (none) | Write report JSON to path. |
| --verbosity | No | normal | Console output level (minimal, normal, verbose). |
| --correlation-id | No | (generated) | Correlation identifier for logs and reports. |
| --fail-fast | No | false | Stop after the first recoverable error. |
| --disable-managed-identity | No | false | Disable Managed Identity credential source. |
| --disable-workload-identity | No | false | Disable Workload Identity credential source. |
| --disable-azure-cli | No | false | Disable Azure CLI credential source. |
| --disable-visual-studio | No | false | Disable Visual Studio credential source. |
| --disable-visual-studio-code | No | false | Disable Visual Studio Code credential source. |
| --disable-shared-token-cache | No | false | Disable shared token cache credential source. |

## Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success (no errors). |
| 1 | Recoverable failures (one or more secrets failed, run completed). |
| 2 | Fatal failure (run stopped due to fatal error). |
| 3 | Canceled (run stopped by cancellation). |

## Dry-Run Example

```text
dotnet run -- \
  --keyvault-uri https://<your-vault>.vault.azure.net \
  --appconfig-endpoint https://<your-appconfig>.azconfig.io \
  --dry-run \
  --report-json run-report.json
```

## Apply Example

```text
dotnet run -- \
  --keyvault-uri https://<your-vault>.vault.azure.net \
  --appconfig-endpoint https://<your-appconfig>.azconfig.io \
  --apply \
  --mode kvref
```

## Testing & Quality Gates

See `docs/testing.md` for required test suites, CI gate rules, and release validation expectations.
