# Quickstart: App Configuration Writes

## Goal
Apply safe, idempotent updates to App Configuration with consistent labels and metadata.

## Step 1: Prepare desired write plan

Provide a desired state and generate a write plan with create/update/skip actions.

## Step 2: Apply writes

- Run apply with bounded parallelism and retry settings.
- Verify unchanged entries are skipped and no secret values are printed.

Example:

```text
dotnet run -- apply --plan-path write-plan.json --max-parallelism 8 --retry-max 3
```

## Step 3: Review outcomes

- Inspect the write report for totals, results, and failures.
- Confirm unmanaged tags are preserved unless explicitly overridden.

Example:

```text
dotnet run -- apply --plan-path write-plan.json --report-json write-report.json
```

## Step 4: Optional rollback

- If rollback is requested and a pre-write snapshot is available, rerun apply with rollback enabled.

Example:

```text
dotnet run -- apply --plan-path write-plan.json --rollback --snapshot-id snapshot-001
```

## Expected Outcomes

- Idempotent writes with minimal churn.
- Consistent labels and managed metadata.
- Clear, per-entry results with non-zero exit codes on partial failures.
