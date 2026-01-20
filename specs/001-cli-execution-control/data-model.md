# Data Model: CLI and Execution Control

## Entity: RunConfiguration

**Purpose**: Captures all CLI inputs and defaulted values for a run.

**Fields**:
- `keyvaultUri` (string, required): Must be a valid URI.
- `appconfigEndpoint` (string, required): Must be a valid URI/endpoint.
- `executionMode` (enum, required): `dry-run`, `diff`, `apply`.
- `environment` (string, optional): Label value; must be non-empty if provided.
- `mappingFile` (string, optional): Must exist and be readable if provided.
- `parallelism` (int, optional): Must be >= 1 when provided.
- `includePrefix` (string, optional)
- `excludeRegex` (string, optional): Must be valid regex if provided.
- `onlyTag` (string, optional): Must be `key=value` format.
- `reportJson` (string, optional): Output path for report when enabled.
- `mode` (enum, required): `kvref` (default) or `copyvalue` (explicit).
- `confirmCopyValue` (bool, optional): Required when `mode=copyvalue`.

**Validation Rules**:
- `keyvaultUri` and `appconfigEndpoint` are required before execution.
- `executionMode` is required and must not be ambiguous.
- `dry-run` and `apply` cannot be enabled together.
- `mode=copyvalue` requires `confirmCopyValue=true`.

## Entity: ExecutionMode

**Purpose**: Indicates the run behavior.

**Values**:
- `dry-run`: Read-only, no writes.
- `diff`: Read-only with change summary.
- `apply`: Writes updates when needed.

## Entity: ValidationResult

**Purpose**: Aggregates input validation outcomes before execution.

**Fields**:
- `errors` (list of strings)
- `warnings` (list of strings)

**Relationships**:
- Generated from `RunConfiguration` validation.

## Entity: RunReport

**Purpose**: Audit-safe summary of execution outcomes.

**Fields**:
- `runId` (string)
- `timestamp` (datetime)
- `executionMode` (enum)
- `totals` (object): `scanned`, `changed`, `skipped`, `failed` counts.
- `changes` (list): Per-key summaries without secret values.
- `failures` (list): Per-key error summaries without secret values.

**Relationships**:
- Produced by executing a validated `RunConfiguration`.

## Entity: ChangeSummary

**Purpose**: Represents an individual change decision.

**Fields**:
- `key` (string)
- `label` (string, optional)
- `action` (enum): `create`, `update`, `noop`, `skip`.
- `reason` (string)

## Entity: FailureSummary

**Purpose**: Captures per-item failures without sensitive data.

**Fields**:
- `key` (string)
- `errorType` (string)
- `message` (string)
