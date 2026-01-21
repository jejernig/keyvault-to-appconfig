# Data Model: Planning & Diff Engine

## Entities

### DesiredState
- Purpose: Canonical representation of intended key/label/value entries.
- Fields:
  - DesiredStateId
  - Entries (ordered list of DesiredEntry)
  - GeneratedAt
  - GeneratedBy
- Validation rules:
  - Entries must be unique by key + label.

### DesiredEntry
- Purpose: Single intended configuration value.
- Fields:
  - Key
  - Label
  - Value
  - ContentType
  - SourceId
- Validation rules:
  - Key is required.
  - Label may be empty but must be explicit.

### ExistingStateSnapshot
- Purpose: Read-only view of App Configuration entries for the selected scope.
- Fields:
  - SnapshotId
  - Entries (ordered list of ExistingEntry)
  - RetrievedAt
  - Scope (key prefix, label filters)
- Validation rules:
  - Entries must be unique by key + label.

### ExistingEntry
- Purpose: Single existing configuration entry.
- Fields:
  - Key
  - Label
  - Value
  - ContentType
  - LastModified
- Validation rules:
  - Key is required.

### DiffItem
- Purpose: Planned action for a desired entry.
- Fields:
  - Key
  - Label
  - Classification (create, update, unchanged)
  - Reason
  - DesiredValue
  - ExistingValue (optional)

### ConflictRecord
- Purpose: Captures desired collisions or conflicts.
- Fields:
  - Key
  - Label
  - ConflictingValues (list)
  - ResolutionStatus

### PlanOutput
- Purpose: Ordered list of diff items with summary totals.
- Fields:
  - DiffItems
  - Conflicts
  - Totals (create/update/unchanged/conflict counts)
  - GeneratedAt

## Relationships

- DesiredState 1..* DesiredEntry
- ExistingStateSnapshot 1..* ExistingEntry
- PlanOutput 1..* DiffItem
- PlanOutput 0..* ConflictRecord

## State Transitions

### PlanOutput
- Draft -> Finalized
