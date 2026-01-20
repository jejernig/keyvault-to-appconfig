# Quickstart: CLI and Execution Control

## Prerequisites

- Access to the target Key Vault and App Configuration resources.
- Credentials configured for non-interactive execution (managed identity or
  local developer credentials).

## Common Commands

Dry-run (no writes):

```bash
tool --keyvault-uri https://my-vault.vault.azure.net/ --appconfig-endpoint https://my-appconfig.azconfig.io --dry-run
```

Diff (no writes, show changes):

```bash
tool --keyvault-uri https://my-vault.vault.azure.net/ --appconfig-endpoint https://my-appconfig.azconfig.io --diff
```

Apply changes (explicit write):

```bash
tool --keyvault-uri https://my-vault.vault.azure.net/ --appconfig-endpoint https://my-appconfig.azconfig.io --apply
```

Copy-value mode (explicit and confirmed):

```bash
tool --keyvault-uri https://my-vault.vault.azure.net/ --appconfig-endpoint https://my-appconfig.azconfig.io --apply --mode copyvalue --confirm-copy-value
```

JSON report output:

```bash
tool --keyvault-uri https://my-vault.vault.azure.net/ --appconfig-endpoint https://my-appconfig.azconfig.io --diff --report-json C:\temp\run-report.json
```

## Expected Exit Codes

- `0`: Success, no changes
- `1`: Success with changes
- `2`: Partial failures
- `3`: Fatal error

## Performance Check

Target: Handle 1,000 secrets per run with stable output ordering.

Basic check:

```bash
tool --keyvault-uri https://my-vault.vault.azure.net/ --appconfig-endpoint https://my-appconfig.azconfig.io --dry-run --parallelism 8
```
