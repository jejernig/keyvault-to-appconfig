# Identity Options

## Managed Identity

**Purpose**: Use when running in Azure (VM, App Service, AKS).

- Assign RBAC roles to the managed identity for Key Vault and App Configuration.
- No secrets are stored locally.

## Workload Identity

**Purpose**: Use in Kubernetes with federated identity.

- Configure workload identity federation for the cluster.
- Assign RBAC roles to the workload identity.
- Avoid storing client secrets.

## Local Development

**Purpose**: Use on developer machines.

- Authenticate with Azure CLI or developer credentials.
- Do not store secrets in source-controlled files.

## Credential Guardrails

- Never paste secrets into shell history or documentation.
- Prefer short-lived credentials and least-privilege roles.
- Use Key Vault reference mode unless copy-value is explicitly required.
