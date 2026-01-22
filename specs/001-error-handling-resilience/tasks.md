# Tasks: Error Handling & Resilience

**Input**: Design documents from `/specs/001-error-handling-resilience/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Unit tests are included for error classification, exit code mapping, per-secret isolation, and cancellation handling.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `src/`, `tests/` at repository root
- Paths below assume the existing CLI + shared libraries structure

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create error handling test folder in `tests/unit/KeyVaultToAppConfig.UnitTests/ErrorHandling/`
- [X] T002 Add shared test data helpers in `tests/unit/KeyVaultToAppConfig.UnitTests/ErrorHandling/ErrorHandlingTestData.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 Add error classification enum in `src/lib/KeyVaultToAppConfig.Core/Errors/ErrorClassification.cs`
- [X] T004 Add error record model in `src/lib/KeyVaultToAppConfig.Core/Errors/ErrorRecord.cs`
- [X] T005 Add per-secret outcome model in `src/lib/KeyVaultToAppConfig.Core/Errors/SecretOperationOutcome.cs`
- [X] T006 Extend run report model for error/outcome fields in `src/lib/KeyVaultToAppConfig.Core/Observability/RunReportModel.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Deterministic Outcomes (Priority: P1) MVP

**Goal**: Deterministic error classification and exit code mapping

**Independent Test**: Execute unit tests that assert consistent classifications and exit codes for identical inputs

### Tests for User Story 1

- [X] T007 [P] [US1] Add unit tests for classification rules in `tests/unit/KeyVaultToAppConfig.UnitTests/ErrorHandling/ErrorClassificationTests.cs`
- [X] T008 [P] [US1] Add unit tests for exit code mapping in `tests/unit/KeyVaultToAppConfig.UnitTests/ErrorHandling/ExitCodeMappingTests.cs`
- [X] T009 [P] [US1] Add unit tests for retry/backoff cancellation in `tests/unit/KeyVaultToAppConfig.UnitTests/ErrorHandling/RetryHandlingTests.cs`

### Implementation for User Story 1

- [X] T010 [US1] Implement classification rules in `src/services/KeyVaultToAppConfig.Services/Errors/ErrorClassifier.cs`
- [X] T011 [US1] Configure retry policy for recoverable failures in `src/services/KeyVaultToAppConfig.Services/ExecutionService.cs`
- [X] T012 [US1] Wire classification into execution flow in `src/services/KeyVaultToAppConfig.Services/ExecutionService.cs`
- [X] T013 [US1] Update run report building for classifications in `src/services/KeyVaultToAppConfig.Services/Observability/RunReportBuilder.cs`
- [X] T014 [US1] Map outcomes to exit codes in `src/lib/KeyVaultToAppConfig.Core/Observability/ObservabilityExitCodes.cs`
- [X] T015 [US1] Enforce deterministic ordering in `src/services/KeyVaultToAppConfig.Services/Observability/RunReportBuilder.cs`

**Checkpoint**: User Story 1 functional and testable

---

## Phase 4: User Story 2 - Per-Secret Isolation (Priority: P2)

**Goal**: Recoverable failures are isolated to individual secrets

**Independent Test**: Unit test demonstrates a recoverable error does not stop other secrets

### Tests for User Story 2

- [X] T016 [P] [US2] Add isolation tests in `tests/unit/KeyVaultToAppConfig.UnitTests/ErrorHandling/PerSecretIsolationTests.cs`
- [X] T017 [P] [US2] Add fail-fast behavior tests in `tests/unit/KeyVaultToAppConfig.UnitTests/ErrorHandling/FailFastTests.cs`

### Implementation for User Story 2

- [X] T018 [US2] Continue processing on recoverable errors in `src/services/KeyVaultToAppConfig.Services/ExecutionService.cs`
- [X] T019 [US2] Record per-secret outcomes in `src/services/KeyVaultToAppConfig.Services/Observability/RunReportBuilder.cs`
- [X] T020 [US2] Add fail-fast configuration in `src/cli/KeyVaultToAppConfig.Cli/Program.cs`, `src/lib/KeyVaultToAppConfig.Core/Configuration/CliOptions.cs`, `src/lib/KeyVaultToAppConfig.Core/Configuration/RunConfigurationInput.cs`, `src/lib/KeyVaultToAppConfig.Core/Configuration/RunConfiguration.cs`, `src/lib/KeyVaultToAppConfig.Core/Validation/RunConfigurationValidator.cs`
- [X] T021 [US2] Apply fail-fast stop behavior in `src/services/KeyVaultToAppConfig.Services/ExecutionService.cs`

**Checkpoint**: User Stories 1 and 2 independently functional

---

## Phase 5: User Story 3 - Safe Cancellation and Shutdown (Priority: P3)

**Goal**: Cancellations stop work safely and report clear outcomes

**Independent Test**: Cancellation tests show unprocessed secrets are marked and exit code is cancellation

### Tests for User Story 3

- [X] T022 [P] [US3] Add cancellation tests in `tests/unit/KeyVaultToAppConfig.UnitTests/ErrorHandling/CancellationHandlingTests.cs`

### Implementation for User Story 3

- [X] T023 [US3] Handle cancellation and mark unprocessed secrets in `src/services/KeyVaultToAppConfig.Services/ExecutionService.cs`
- [X] T024 [US3] Ensure cancellation exit code mapping in `src/lib/KeyVaultToAppConfig.Core/Observability/ObservabilityExitCodes.cs`

**Checkpoint**: All user stories functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T025 [P] Update exit code documentation in `docs/cli-reference.md`
- [X] T026 Run quickstart validation and note results in `specs/001-error-handling-resilience/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - blocks all user stories
- **User Stories (Phase 3+)**: Depend on Foundational completion
- **Polish (Final Phase)**: Depends on user stories completion

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Foundational, no dependencies
- **User Story 2 (P2)**: Starts after Foundational, builds on run reporting from US1
- **User Story 3 (P3)**: Starts after Foundational, builds on execution flow

### Within Each User Story

- Tests before implementation
- Model changes before services
- Core execution changes before report updates

### Parallel Opportunities

- T007 and T008 can run in parallel
- T016 can run in parallel with documentation updates after P1 is stable
- T022 can run in parallel with T025

---

## Parallel Example: User Story 1

```bash
Task: "Add unit tests for classification rules in tests/unit/KeyVaultToAppConfig.UnitTests/ErrorHandling/ErrorClassificationTests.cs"
Task: "Add unit tests for exit code mapping in tests/unit/KeyVaultToAppConfig.UnitTests/ErrorHandling/ExitCodeMappingTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. Validate unit tests for classification and exit code mapping

### Incremental Delivery

1. Setup + Foundational
2. User Story 1 -> test independently
3. User Story 2 -> test independently
4. User Story 3 -> test independently
5. Polish updates
