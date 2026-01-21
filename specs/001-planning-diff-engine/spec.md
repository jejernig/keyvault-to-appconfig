# Feature Specification: Planning & Diff Engine

**Feature Branch**: `001-planning-diff-engine`  
**Created**: 2026-01-20  
**Status**: Draft  
**Input**: User description: "Act as a Staff Software Engineer. Specify features for the Planning & Diff Engine. For each feature: Define how desired state is constructed Define how existing App Configuration state is read Define diff rules (create/update/unchanged) Define collision and conflict handling Define dry-run behavior Define acceptance criteria Ensure no writes occur during planning or dry-run modes."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Plan desired changes (Priority: P1)

As a configuration steward, I generate a plan that describes the desired App Configuration state and the actions needed to reach it so I can review changes before execution.

**Why this priority**: Planning is the core safeguard for preventing accidental configuration changes.

**Independent Test**: Can be fully tested by submitting a desired-state definition and verifying a complete, ordered plan with no writes.

**Acceptance Scenarios**:

1. **Given** a desired-state definition and a source mapping spec, **When** planning runs, **Then** the plan lists creates, updates, and unchanged items with reasons.
2. **Given** planning mode, **When** the plan is generated, **Then** no writes are performed against App Configuration.
3. **Given** planning mode, **When** planning executes, **Then** no write-capable clients are invoked and no write operations are attempted.

---

### User Story 2 - Compare against existing state (Priority: P2)

As a configuration steward, I see how the desired state differs from the existing App Configuration state so I can validate the impact.

**Why this priority**: Accurate diffs are required for safe and deterministic updates.

**Independent Test**: Can be fully tested by comparing a known desired state to a mocked existing state and verifying diff results.

**Acceptance Scenarios**:

1. **Given** an existing state snapshot, **When** the diff is computed, **Then** each key is classified as create, update, or unchanged.
2. **Given** conflicting keys with multiple sources, **When** diffing runs, **Then** conflicts are detected and reported without applying changes.

---

### User Story 3 - Dry-run without writes (Priority: P3)

As a configuration steward, I run a dry-run that produces the same plan output as apply mode but guarantees no writes.

**Why this priority**: Dry-run is required by the constitution for safe automation and reviews.

**Independent Test**: Can be fully tested by running dry-run with valid inputs and verifying that output matches planning and no writes occur.

**Acceptance Scenarios**:

1. **Given** dry-run mode, **When** the engine executes, **Then** the output plan is identical to planning mode.
2. **Given** dry-run mode, **When** the run completes, **Then** no write operations are recorded or attempted.

---

### Edge Cases

- **Desired key missing in App Configuration**: The plan marks the entry as `create` with reason "Missing in existing state" and includes it in the ordered output.
- **Stale existing-state read or missing label**: The plan proceeds using the snapshot provided, marks entries as `create` when no matching key/label exists, and includes a warning in the report/logs indicating snapshot scope may be incomplete.
- **Multiple desired inputs resolve to the same key/label with different values**: The plan records a conflict for that key/label, excludes conflicting entries from diff actions, and reports the conflict reason.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST construct a desired state from mapping specifications, input source enumeration, and environment labels.
- **FR-002**: The system MUST read existing App Configuration state for the targeted key/label scope before diffing.
- **FR-003**: The system MUST classify each desired item as create, update, or unchanged based on key, label, and value comparisons.
- **FR-004**: The system MUST consider value, content type, and label when determining whether an update is required.
- **FR-005**: The system MUST produce a deterministic, ordered plan output for the same inputs.
- **FR-006**: The system MUST detect collisions when multiple desired entries target the same key/label.
- **FR-007**: The system MUST surface conflicts with explicit reasons and prevent write execution in planning and dry-run modes.
- **FR-008**: Planning and dry-run modes MUST perform zero writes to App Configuration.
- **FR-009**: Dry-run mode MUST produce the same plan output as planning mode.
- **FR-010**: The system MUST allow filtering of existing state reads by key prefix, label list, page size, and continuation token across plan, diff, and dry-run modes.

### Non-Functional Requirements

- **NFR-001**: Planning, diff, and dry-run logs MUST be structured and include a correlation identifier per run.
- **NFR-002**: Reports MUST include counts, changes, skips, and failures without secret values.
- **NFR-003**: Planning and dry-run outputs MUST be deterministic and order-stable for the same inputs.
- **NFR-004**: Planning and diff for 10,000 items MUST complete in under 5 seconds on a developer workstation.

### Key Entities *(include if feature involves data)*

- **Desired State**: The complete set of keys/labels/values that represent the intended configuration.
- **Existing State Snapshot**: A read-only view of current App Configuration entries for the selected scope.
- **Diff Item**: A single planned action with classification (create/update/unchanged) and rationale.
- **Plan Output**: An ordered list of Diff Items with summary totals.
- **Conflict Record**: A record of colliding or conflicting desired entries and the resolution status.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of planning and dry-run executions perform zero writes.
- **SC-002**: Diff classification is correct for at least 99% of items across regression fixtures.
- **SC-003**: Plan output is identical across repeated runs with identical inputs.
- **SC-004**: Users can complete a plan review in under 5 minutes for 1,000 items.
- **SC-005**: Conflict detection reports 100% of key/label collisions.

## Assumptions

- Desired state is derived from mapping specs and source inputs already validated by the mapping engine.
- Existing state reads are bounded to the targeted environment labels and key prefixes.
- Planning and dry-run outputs are persisted as reports without secret values.
