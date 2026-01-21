# Research: Observability & Reporting

## Decision 1: Structured logging format

- **Decision**: Use structured JSON log entries with fixed fields (timestamp, level, event, correlationId, message, data).
- **Rationale**: JSON lines are machine-readable, stream-friendly, and easy to parse for audits.
- **Alternatives considered**: Plain text logs (harder to parse), mixed formats (inconsistent across tools).

## Decision 2: Verbosity levels

- **Decision**: Support three verbosity levels: minimal (summary + errors), normal (summary + key events), verbose (per-item events).
- **Rationale**: Provides bounded output for routine runs and detailed output for diagnostics.
- **Alternatives considered**: Single verbosity (too noisy or too sparse), unlimited custom levels (inconsistent).

## Decision 3: Correlation identifier handling

- **Decision**: Accept optional correlation ID input; generate a new ID when absent and reuse it across logs and reports.
- **Rationale**: Ensures every run is traceable without requiring operators to supply an ID.
- **Alternatives considered**: Require operator-supplied IDs (inconvenient), omit IDs (not audit-ready).

## Decision 4: JSON report schema

- **Decision**: Report includes correlationId, run metadata, totals (total/changes/skips/failures), per-item outcomes, and non-secret failure reasons with stable field ordering.
- **Rationale**: Aligns with audit requirements and supports automated validation.
- **Alternatives considered**: Totals-only report (insufficient detail), per-item only (missing aggregates).
