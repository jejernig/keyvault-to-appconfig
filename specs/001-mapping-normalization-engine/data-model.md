# Data Model: Mapping & Normalization Engine

## Entities

### MappingSpecification
- Purpose: Authoritative definition of mapping rules and defaults.
- Fields:
  - SpecificationId (unique identifier)
  - Name
  - Version
  - Description (optional)
  - Rules (ordered list of MappingRule)
  - DefaultBehavior (optional: reject or pass-through unmapped keys)
  - CollisionPolicy (error, keep-first, keep-last, report-only)
  - CreatedAt
  - CreatedBy
- Validation rules:
  - Name and Version are required.
  - Rules list is required and ordered.
  - Rule identifiers must be unique.

### MappingRule
- Purpose: Map one or more source keys to a normalized target key.
- Fields:
  - RuleId (unique within spec)
  - StrategyType (direct or regex)
  - SourceSelector (exact key or regex pattern)
  - TargetKey
  - Priority (integer, higher wins)
  - Transforms (ordered list of TransformOperation)
- Validation rules:
  - StrategyType is required.
  - SourceSelector and TargetKey are required.
  - Regex patterns must be syntactically valid.
  - Priority values must be within defined range.

### TransformOperation
- Purpose: Deterministic modification applied during mapping.
- Fields:
  - TransformType (trim, upper, lower, prefix, suffix, replace, regex-replace, capture-substitute)
  - Parameters (type-specific)
  - Order (position in pipeline)
- Validation rules:
  - TransformType must be supported.
  - Required parameters must be present and valid.

### MappingRun
- Purpose: Execution record for a mapping run.
- Fields:
  - RunId
  - SpecificationId
  - SpecificationVersion
  - SourceSetId (reference to input set)
  - StartedAt
  - CompletedAt
  - Status (succeeded, failed)
  - CollisionReportId (optional)
- Validation rules:
  - SpecificationVersion is required.
  - Status transitions must be valid.

### CollisionReport
- Purpose: Captures collisions for a run.
- Fields:
  - ReportId
  - RunId
  - Entries (list of CollisionEntry)
- Validation rules:
  - Entries required when collisions detected.

### CollisionEntry
- Purpose: Single collision record.
- Fields:
  - NormalizedKey
  - SourceKeys (list)
  - AppliedPolicy

## Relationships

- MappingSpecification 1..* MappingRule (ordered)
- MappingSpecification 1..* MappingRun
- MappingRun 0..1 CollisionReport
- CollisionReport 1..* CollisionEntry

## State Transitions

### MappingSpecification
- Draft -> Validated -> Active -> Archived
- Only Validated or Active specs can be used for MappingRun.

### MappingRun
- Started -> Succeeded
- Started -> Failed
