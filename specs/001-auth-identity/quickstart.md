# Quickstart: Authentication and Identity

## Prerequisites

- Managed identity enabled on the runtime environment, or workload identity
  configured for the execution context, or local developer credentials set up.

## Identity Resolution Order

1. Managed identity
2. Workload identity
3. Local developer credentials

## Common Runs

Managed identity (default):

```bash
tool --keyvault-uri https://my-vault.vault.azure.net/ --appconfig-endpoint https://my-appconfig.azconfig.io --dry-run
```

Workload identity (when configured):

```bash
tool --keyvault-uri https://my-vault.vault.azure.net/ --appconfig-endpoint https://my-appconfig.azconfig.io --dry-run
```

Local developer credentials:

```bash
tool --keyvault-uri https://my-vault.vault.azure.net/ --appconfig-endpoint https://my-appconfig.azconfig.io --dry-run
```

Local dev setup:

- Ensure `az login` has been completed for the target tenant.
- Avoid storing client secrets in configuration files.

## Failure Behavior

- If no valid credential source is found, the run exits with a fatal error.
- Transient token acquisition failures retry within bounded limits.
- RBAC failures surface a clear, actionable message.

## RBAC Assumptions

- Key Vault: secrets list/get permissions.
- App Configuration: configuration data write permissions.
