# Quickstart: Secret Handling Modes

## Goal
Run with least-risk secret handling defaults and explicit guardrails for copy-value.

## Step 1: Use reference mode (default)

- Run without specifying a mode and verify references are written.
- Confirm no secret values are printed in logs or reports.

Example:

```text
dotnet run -- secret-mode --mode reference --secret-uri https://example.vault.azure.net/secrets/app-secret
```

## Step 2: Validate secret URI resolution

- Provide secret URIs with and without versions.
- Verify missing versions resolve to latest enabled and are recorded in metadata.

## Step 3: Attempt copy-value without guardrails

- Run copy-value mode without confirmations and verify the run is blocked.

Example:

```text
dotnet run -- secret-mode --mode copy-value --secret-uri https://example.vault.azure.net/secrets/app-secret
```

## Step 4: Enable copy-value with guardrails

- Provide the explicit flag and secondary confirmation.
- Limit copy-value to explicitly allowed entries.

Example:

```text
dotnet run -- secret-mode --mode copy-value --confirm-copy-value --confirm-copy-value-text "I UNDERSTAND" --allowed-key app-secret --secret-uri https://example.vault.azure.net/secrets/app-secret
```

## Step 5: Generate a redacted report

- Write a report JSON payload and verify totals, skips, and failures are present.

Example:

```text
dotnet run -- secret-mode --mode reference --secret-uri https://example.vault.azure.net/secrets/app-secret --report-json secret-mode-report.json
```

## Step 6: Validate exit codes

- Successful evaluations return exit code 0.
- Any failures return exit code 1.

## Expected Outcomes

- Reference mode writes only Key Vault references.
- Copy-value mode is blocked without both confirmations.
- Logs and reports never contain secret values.
