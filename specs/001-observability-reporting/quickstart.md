# Quickstart: Observability & Reporting

## Goal
Produce audit-safe logs, console summaries, and JSON reports with correlation identifiers.

## Step 1: Run with default verbosity

- Execute a standard run and confirm a console summary is printed.
- Verify the summary includes totals, changes, skips, and failures.

Example:

```text
dotnet run -- --verbosity normal
```

## Step 2: Increase verbosity for diagnostics

- Run with verbose output enabled.
- Verify that per-step events appear without exposing secret values.

Example:

```text
dotnet run -- --verbosity verbose
```

## Step 3: Provide or capture correlation identifiers

- Supply a correlation identifier if supported by the run command.
- Verify the same identifier appears in logs and the report output.

Example:

```text
dotnet run -- --correlation-id obs-001
```

## Step 4: Generate a JSON report

- Enable report output and save to a file.
- Validate required fields (correlationId, totals, items, failures).

Example:

```text
dotnet run -- --report-json observability-report.json
```

## Step 5: Audit-safe validation

- Review logs and report for redacted values and names.
- Confirm failure reasons are non-secret and machine-readable.

## Step 6: Validate exit codes

- Successful runs return exit code 0.
- Runs with failures return exit code 1.

## Expected Outcomes

- Logs are structured and include correlation identifiers.
- Console summary is bounded and includes counts.
- JSON report conforms to schema and is audit-safe.
