# Data Model: Testing & Quality Gates

## TestScope

Represents the catalog of required test areas.

**Fields**:
- Category (Unit, Integration, Security)
- Areas (Mapping, Diff, Redaction, ErrorHandling, ExitCodes)
- Required (bool)

## QualityGate

Represents CI gating rules.

**Fields**:
- GateName
- RequiredSuites (Unit, Integration, Security)
- AppliesTo (Merge, Release)
- WaiverAllowed (bool)

## RiskPath

Represents high-risk workflow requiring coverage.

**Fields**:
- Name
- Description
- CoveredByTests (list)

## TestEvidence

Represents recorded outputs from test runs.

**Fields**:
- SuiteName
- RunId
- Timestamp
- Status (Pass, Fail, Skipped)
- Notes
