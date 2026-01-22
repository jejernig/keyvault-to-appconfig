# Data Model: Testing & Quality Gates

## Entities

### High-Risk Path

- **Description**: A critical behavior that must be protected by explicit tests.
- **Attributes**:
  - Name
  - Description
  - Required Test Categories (unit, integration, security)

### Test Scope Definition

- **Description**: A structured description of required unit, integration, and security tests.
- **Attributes**:
  - Scope Name
  - Included Behaviors
  - Exclusions (if any, must be explicit)

### Gate Rule

- **Description**: Policy describing required test outcomes before merge or release.
- **Attributes**:
  - Stage (merge, release)
  - Required Suites
  - Waiver Conditions
  - Approval Requirements

### Test Evidence

- **Description**: Artifacts or records that prove required tests were executed.
- **Attributes**:
  - Evidence Type (log, report, link)
  - Location
  - Associated Gate Rule

## Relationships

- High-Risk Path maps to one or more Test Scope Definitions.
- Gate Rule requires Test Evidence.
