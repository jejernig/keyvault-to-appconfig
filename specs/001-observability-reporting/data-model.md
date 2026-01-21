# Data Model: Observability & Reporting

## Entities

### RunCorrelation

- **Fields**: id, generatedAt
- **Notes**: Unique per run; used across logs, console summary, and report.

### LogEntry

- **Fields**: timestamp, level, event, correlationId, message, data
- **Validation**: correlationId required; message must be redacted; level must be one of supported verbosity levels.

### ConsoleSummary

- **Fields**: correlationId, totals (total, changes, skips, failures), emittedAt
- **Validation**: totals are non-negative; emittedAt after run completion.

### Report

- **Fields**: correlationId, runMode, verbosity, totals (total, changes, skips, failures), items, failures, generatedAt
- **Validation**: stable field ordering; no secret values or names unless name redaction disabled.

### ReportItem

- **Fields**: key, label, outcome, reason, timestamp
- **Validation**: outcome in {changed, skipped, failed, unchanged}; reason must be non-secret.

### FailureDetail

- **Fields**: key, label, errorType, message
- **Validation**: message must be redacted; errorType required.

## Relationships

- Report aggregates many ReportItem entries and FailureDetail entries.
- LogEntry records reference the same correlationId as the report and console summary.
