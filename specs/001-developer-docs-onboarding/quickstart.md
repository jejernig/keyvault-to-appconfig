# Quickstart: Developer Documentation & Onboarding

## Goal
Enable a new engineer to safely build, configure, and run the tool without tribal knowledge.

## Step 1: Build and run locally

- Restore dependencies and build the solution.
- Run a dry-run with sample arguments.

```text
dotnet build

dotnet run -- \
  --keyvault-uri https://<your-vault>.vault.azure.net \
  --appconfig-endpoint https://<your-appconfig>.azconfig.io \
  --dry-run \
  --report-json run-report.json
```

## Step 2: Configure Azure prerequisites

- Create or identify a Key Vault and an App Configuration instance.
- Assign required RBAC roles for Key Vault and App Configuration.

## Step 3: Select identity mode

- Choose Managed Identity, Workload Identity, or local development credentials.
- Follow `docs/identity.md` for guardrails.

## Step 4: Run safe vs restricted modes

- Use Key Vault reference mode by default.
- Only use copy-value mode with explicit confirmations.

## Step 5: Troubleshoot common issues

- Use `docs/troubleshooting.md` for common errors and resolutions.

## Expected Outcomes

- Tool builds and runs locally.
- Required resources and roles are confirmed.
- Secure defaults are used for secrets.
