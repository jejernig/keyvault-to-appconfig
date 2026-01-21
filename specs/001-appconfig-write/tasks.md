---

description: "Task list for App Configuration Writes"
---

# Tasks: App Configuration Writes

**Input**: Design documents from `C:\repos\keyvault-to-appconfig\specs\001-appconfig-write\`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Unit tests included for diff logic and redaction safeguards.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create write module folders in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Writes\` and `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Writes\`
- [X] T002 [P] Create CLI command folder in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\`
- [X] T003 [P] Create unit test folder in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Writes\`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Define WritePlan model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Writes\WritePlan.cs`
- [X] T005 [P] Define WriteAction model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Writes\WriteAction.cs`
- [X] T006 [P] Define WriteResult model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Writes\WriteResult.cs`
- [X] T007 [P] Define WriteReport model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Writes\WriteReport.cs`
- [X] T008 [P] Define LabelContext model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Writes\LabelContext.cs`
- [X] T009 [P] Define ManagedMetadata model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Writes\ManagedMetadata.cs`
- [X] T010 [P] Define RetryPolicy model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Writes\RetryPolicy.cs`
- [X] T011 [P] Define RollbackPlan model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Writes\RollbackPlan.cs`
- [X] T012 Define write service interfaces in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Writes\IWritePlanner.cs`, `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Writes\IWriteExecutor.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Apply safe configuration writes (Priority: P1) MVP

**Goal**: Apply idempotent writes with minimal churn.

**Independent Test**: Provide desired and existing state and verify only create/update actions execute while unchanged entries are skipped.

### Implementation for User Story 1

- [X] T013 [P] [US1] Implement write planner for idempotent actions in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Writes\WritePlanner.cs`
- [X] T014 [US1] Implement write executor for apply actions in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Writes\WriteExecutor.cs`
- [X] T015 [US1] Wire apply command in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\ApplyCommand.cs`
- [X] T016 [P] [US1] Add unit tests for idempotent write planning in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Writes\WritePlannerTests.cs`
- [X] T029 [US1] Add max parallelism option and wire to executor in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\ApplyCommand.cs`

**Checkpoint**: User Story 1 is fully functional and testable independently

---

## Phase 4: User Story 2 - Apply labels and metadata consistently (Priority: P2)

**Goal**: Ensure labels and managed metadata are applied consistently while preserving unmanaged tags.

**Independent Test**: Apply a write plan with label defaults and tags, verifying expected label/tag behavior.

### Implementation for User Story 2

- [X] T017 [P] [US2] Implement label application rules in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Writes\LabelPolicy.cs`
- [X] T018 [P] [US2] Implement metadata/tag application rules in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Writes\MetadataPolicy.cs`
- [X] T019 [US2] Wire label and metadata policies into executor in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Writes\WriteExecutor.cs`
- [X] T020 [P] [US2] Add unit tests for label and metadata policies in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Writes\MetadataPolicyTests.cs`

**Checkpoint**: User Story 2 is fully functional and testable independently

---

## Phase 5: User Story 3 - Handle failures safely (Priority: P3)

**Goal**: Surface partial failures and optionally rollback when requested.

**Independent Test**: Simulate partial failures and verify results, exit status, and rollback behavior when enabled.

### Implementation for User Story 3

- [X] T021 [P] [US3] Implement retry and conflict handling in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Writes\RetryPolicyExecutor.cs`
- [X] T022 [US3] Implement rollback execution when requested in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Writes\RollbackHandler.cs`
- [X] T023 [US3] Add failure reporting and exit status mapping in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\ApplyCommand.cs`
- [X] T024 [P] [US3] Add unit tests for retries, partial failures, and rollback in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Writes\RetryPolicyExecutorTests.cs`
- [X] T030 [US3] Add retry policy options and wire to executor in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\ApplyCommand.cs`
- [X] T031 [US3] Add rollback options and snapshot ID wiring in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\ApplyCommand.cs`

**Checkpoint**: User Story 3 is fully functional and testable independently

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T025 [P] Add structured, redacted logging for write operations in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Writes\WriteLogging.cs`
- [X] T026 [P] Add write report output writer in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\ReportWriter.cs`
- [X] T027 [P] Add unit tests for write report output and redaction safeguards in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Writes\WriteReportTests.cs`
- [X] T028 [P] Update quickstart to match apply command in `C:\repos\keyvault-to-appconfig\specs\001-appconfig-write\quickstart.md`
- [X] T032 [P] Add structured logging with correlation IDs for apply flows in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Writes\WriteLogging.cs`
- [X] T033 [P] Add write report fields for counts/changes/skips/failures in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\ReportWriter.cs`
- [X] T034 [P] Add unit tests for correlation IDs and report fields in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Writes\WriteReportTests.cs`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - No dependencies on other stories

### Parallel Opportunities

- Phase 1 tasks T002 and T003 can run in parallel
- Phase 2 tasks T005 through T011 can run in parallel
- Phase 3 tasks T013 and T016 can run in parallel
- Phase 4 tasks T017 and T018 can run in parallel
- Phase 5 tasks T021 and T024 can run in parallel

---

## Parallel Example: User Story 1

```text
Task: "Implement write planner for idempotent actions in C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Writes\WritePlanner.cs"
Task: "Add unit tests for idempotent write planning in C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Writes\WritePlannerTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (blocks all stories)
3. Complete Phase 3: User Story 1
4. Validate User Story 1 independently

### Incremental Delivery

1. Complete Setup + Foundational
2. Add User Story 1 -> Test independently -> Demo
3. Add User Story 2 -> Test independently -> Demo
4. Add User Story 3 -> Test independently -> Demo
5. Apply Polish tasks
