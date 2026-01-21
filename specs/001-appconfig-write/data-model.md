# Data Model: App Configuration Writes

## Entities

### WritePlan
- Purpose: Ordered list of create/update/skip actions derived from desired vs existing state.
- Fields:
  - PlanId
  - GeneratedAt
  - Actions (ordered list of WriteAction)
  - LabelContext
  - ManagedMetadata
- Validation rules:
  - Actions must be unique by key + label.

### WriteAction
- Purpose: Single planned write or skip action.
- Fields:
  - Key
  - Label
  - ActionType (create, update, skip)
  - DesiredValue
  - DesiredContentType
  - Reason
- Validation rules:
  - Key is required.
  - ActionType is required.

### WriteResult
- Purpose: Outcome for a write action.
- Fields:
  - Key
  - Label
  - Status (succeeded, skipped, failed, rolledBack)
  - Attempts
  - FailureReason
  - RetryCount
- Validation rules:
  - Status is required.

### WriteReport
- Purpose: Summary of a write run with totals and per-entry results.
- Fields:
  - CorrelationId
  - StartedAt
  - CompletedAt
  - Totals (create/update/skip/fail counts)
  - Results (ordered list of WriteResult)
- Validation rules:
  - Totals must match Results counts.

### LabelContext
- Purpose: Default label applied when desired entries omit a label.
- Fields:
  - EnvironmentLabel
  - UseEmptyLabelWhenMissing

### ManagedMetadata
- Purpose: Managed tags applied to entries.
- Fields:
  - Source
  - Timestamp
  - AdditionalTags (key/value pairs)

### RetryPolicy
- Purpose: Bounded retry configuration for transient failures.
- Fields:
  - MaxAttempts
  - BaseDelaySeconds
  - MaxDelaySeconds

### RollbackPlan
- Purpose: Optional rollback instructions when requested.
- Fields:
  - Enabled
  - SnapshotId
  - Actions (ordered list of WriteAction)

## Relationships

- WritePlan 1..* WriteAction
- WriteReport 1..* WriteResult
- WritePlan 0..1 RollbackPlan
- LabelContext applies to WritePlan
- ManagedMetadata applies to WritePlan

## State Transitions

### WriteResult
- Pending -> Succeeded
- Pending -> Skipped
- Pending -> Failed
- Failed -> RolledBack (when rollback requested)
