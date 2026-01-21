---

description: "Task list for Planning & Diff Engine"
---

# Tasks: Planning & Diff Engine

**Input**: Design documents from `C:\repos\keyvault-to-appconfig\specs\001-planning-diff-engine\`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Unit tests included for diff logic and dry-run safeguards.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create planning module folders in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Planning\` and `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Planning\`
- [X] T002 [P] Create CLI command folder in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\`
- [X] T003 [P] Create unit test folder in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Planning\`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Define DesiredState model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Planning\DesiredState.cs`
- [X] T005 [P] Define DesiredEntry model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Planning\DesiredEntry.cs`
- [X] T006 [P] Define ExistingStateSnapshot model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Planning\ExistingStateSnapshot.cs`
- [X] T007 [P] Define ExistingEntry model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Planning\ExistingEntry.cs`
- [X] T008 [P] Define DiffItem model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Planning\DiffItem.cs`
- [X] T009 [P] Define ConflictRecord model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Planning\ConflictRecord.cs`
- [X] T010 Define PlanOutput model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Planning\PlanOutput.cs`
- [X] T011 Define diff classification enum in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Planning\DiffClassification.cs`
- [X] T012 Define planning service interfaces in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Planning\IPlanningEngine.cs`, `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Planning\IExistingStateReader.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Plan desired changes (Priority: P1) MVP

**Goal**: Construct desired state and generate a deterministic plan without writes.

**Independent Test**: Provide a desired state and verify ordered plan output with zero writes.

### Implementation for User Story 1

- [X] T013 [P] [US1] Implement desired state builder in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Planning\DesiredStateBuilder.cs`
- [X] T014 [US1] Implement planning engine in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Planning\PlanningEngine.cs`
- [X] T015 [US1] Wire plan command in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\PlanCommand.cs`
- [X] T016 [P] [US1] Add unit tests for plan output determinism in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Planning\PlanningEngineTests.cs`
- [X] T030 [P] [US1] Extend desired state builder to construct from mapping outputs and environment labels in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Planning\DesiredStateBuilder.cs`
- [X] T031 [P] [US1] Add desired state builder tests for mapping outputs and labels in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Planning\MappingCoverageTests.cs`
- [X] T032 [US1] Add scope options and wire existing state reads for plan command in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\PlanCommand.cs`

**Checkpoint**: User Story 1 is fully functional and testable independently

---

## Phase 4: User Story 2 - Compare against existing state (Priority: P2)

**Goal**: Read existing state and compute diff classification and conflicts.

**Independent Test**: Compare known desired and existing states and verify diff classification.

### Implementation for User Story 2

- [X] T017 [P] [US2] Implement existing state reader in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Planning\ExistingStateReader.cs`
- [X] T018 [P] [US2] Implement diff classifier in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Planning\DiffClassifier.cs`
- [X] T019 [US2] Wire diff command in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\DiffCommand.cs`
- [X] T020 [P] [US2] Add diff classification tests in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Planning\DiffClassifierTests.cs`
- [X] T033 [US2] Add scope options and wire existing state reads for diff command in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\DiffCommand.cs`

**Checkpoint**: User Story 2 is fully functional and testable independently

---

## Phase 5: User Story 3 - Dry-run without writes (Priority: P3)

**Goal**: Execute dry-run and guarantee zero writes while producing plan output.

**Independent Test**: Run dry-run and verify output matches planning and no writes occur.

### Implementation for User Story 3

- [X] T021 [P] [US3] Implement dry-run handler in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Planning\DryRunHandler.cs`
- [X] T022 [US3] Wire dry-run command in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\DryRunCommand.cs`
- [X] T023 [P] [US3] Add dry-run safeguard tests in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Planning\DryRunTests.cs`
- [X] T034 [US3] Add scope options and wire existing state reads for dry-run command in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\DryRunCommand.cs`

**Checkpoint**: User Story 3 is fully functional and testable independently

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T024 [P] Update quickstart to match CLI commands in `C:\repos\keyvault-to-appconfig\specs\001-planning-diff-engine\quickstart.md`
- [X] T025 [P] Add tests for conflict detection and collision reporting in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Planning\ConflictDetectionTests.cs`
- [X] T026 Add structured, redacted logging for planning operations in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Planning\PlanningLogging.cs`
- [X] T027 [P] Add mapping coverage unit tests in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Planning\MappingCoverageTests.cs`
- [X] T028 [P] Add redaction safeguard unit tests in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Planning\RedactionSafeguardTests.cs`
- [X] T029 Add planning performance test (10k items under 5s) in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Planning\PlanningPerformanceTests.cs`
- [X] T035 [P] Add plan report output writer with counts/changes/skips/failures in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\ReportWriter.cs`
- [X] T036 [P] Add structured logging with correlation IDs for planning flows in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Planning\PlanningLogging.cs`
- [X] T037 [P] Add unit tests for report output and no-write safeguards in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Planning\DryRunTests.cs` and `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Planning\PlanningEngineTests.cs`

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
- Phase 2 tasks T005, T006, T007, T008, T009 can run in parallel
- Phase 3 tasks T013 and T016 can run in parallel
- Phase 4 tasks T017 and T018 can run in parallel
- Phase 5 tasks T021 and T023 can run in parallel

---

## Parallel Example: User Story 1

```text
Task: "Implement desired state builder in C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Planning\DesiredStateBuilder.cs"
Task: "Add unit tests for plan output determinism in C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Planning\PlanningEngineTests.cs"
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
