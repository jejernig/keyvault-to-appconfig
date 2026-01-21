# Prerequisites

## Azure Resources

- **Key Vault**: Stores secrets; used for enumeration and reference resolution.
- **App Configuration**: Stores Key Vault references or copied values.

## Required RBAC Roles (Minimum)

- **Key Vault**: Key Vault Secrets User (read secrets) or equivalent least-privilege role.
- **App Configuration**:
  - Reader for dry-run/diff.
  - Data Owner (or equivalent) for apply/write operations.

## Additional Prerequisites

- Azure subscription with access to Key Vault and App Configuration.
- Network access to Azure endpoints (private endpoints if required by policy).

## Verification Checklist

- Key Vault exists and contains test secrets.
- App Configuration instance exists and is reachable.
- RBAC assignments are in place for the chosen identity mode.
