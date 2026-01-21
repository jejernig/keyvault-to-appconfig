---

description: "Task list for Observability & Reporting"
---

# Tasks: Observability & Reporting

**Input**: Design documents from `C:\repos\keyvault-to-appconfig\specs\001-observability-reporting\`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Unit tests included for log structure and report schema validation.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create observability module folder in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Observability\`
- [X] T002 [P] Create report schema folder in `C:\repos\keyvault-to-appconfig\specs\001-observability-reporting\schemas\`
- [X] T003 [P] Create unit test folder in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Observability\`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Define verbosity level model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Observability\VerbosityLevel.cs`
- [X] T005 [P] Define log entry model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Observability\LogEntry.cs`
- [X] T006 [P] Define console summary model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Observability\ConsoleSummary.cs`
- [X] T007 [P] Define report schema model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Observability\RunReportModel.cs`
- [X] T008 Define observability service interfaces in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Observability\IObservabilityReporter.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Trace a run end-to-end (Priority: P1) MVP

**Goal**: Emit structured logs with correlation identifiers and audit-safe redaction.

**Independent Test**: Run any mode and verify every log entry includes correlation ID and no secrets.

### Implementation for User Story 1

- [X] T009 [P] [US1] Implement correlation ID provider in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Observability\CorrelationIdProvider.cs`
- [X] T010 [US1] Implement structured log writer in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Observability\StructuredLogWriter.cs`
- [X] T011 [US1] Wire log writer into run execution flow in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\ExecutionService.cs`
- [X] T012 [P] [US1] Add unit tests for structured logging and correlation ID in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Observability\StructuredLogWriterTests.cs`

**Checkpoint**: User Story 1 is fully functional and testable independently

---

## Phase 4: User Story 2 - Control output verbosity (Priority: P2)

**Goal**: Provide verbosity levels that control console output scope.

**Independent Test**: Run with each verbosity level and verify console output scope.

### Implementation for User Story 2

- [X] T013 [P] [US2] Implement verbosity filter in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Observability\VerbosityFilter.cs`
- [X] T014 [US2] Wire verbosity handling into CLI options in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\CliOptions.cs`
- [X] T015 [US2] Apply verbosity filtering to console output in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\OutputWriter.cs`
- [X] T016 [P] [US2] Add unit tests for verbosity filtering in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Observability\VerbosityFilterTests.cs`

**Checkpoint**: User Story 2 is fully functional and testable independently

---

## Phase 5: User Story 3 - Produce audit-safe JSON reports (Priority: P3)

**Goal**: Produce audit-safe JSON report schema with per-item outcomes and aggregate counts.

**Independent Test**: Run a sample and validate report JSON matches schema and contains no secrets.

### Implementation for User Story 3

- [X] T017 [P] [US3] Implement report schema builder in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Observability\RunReportBuilder.cs`
- [X] T018 [US3] Wire JSON report generation to report writer in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\ReportWriter.cs`
- [X] T019 [US3] Add schema file and reference in `C:\repos\keyvault-to-appconfig\specs\001-observability-reporting\schemas\run-report.schema.json`
- [X] T020 [P] [US3] Add unit tests for report schema and ordering in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Observability\RunReportTests.cs`

**Checkpoint**: User Story 3 is fully functional and testable independently

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T021 [P] Add console summary timing guard (<=1s for 10,000 items) in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\OutputWriter.cs`
- [X] T022 [P] Add exit code mapping for observability reporting outcomes in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Program.cs`
- [X] T023 [P] Add unit tests for exit code mapping in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Observability\ExitCodeTests.cs`
- [X] T024 [P] Update quickstart for observability reporting steps in `C:\repos\keyvault-to-appconfig\specs\001-observability-reporting\quickstart.md`

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
- Phase 2 tasks T005 through T007 can run in parallel
- Phase 3 tasks T009 and T012 can run in parallel
- Phase 4 tasks T013 and T016 can run in parallel
- Phase 5 tasks T017 and T020 can run in parallel

---

## Parallel Example: User Story 1

```text
Task: "Implement correlation ID provider in C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Observability\CorrelationIdProvider.cs"
Task: "Add unit tests for structured logging and correlation ID in C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Observability\StructuredLogWriterTests.cs"
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
