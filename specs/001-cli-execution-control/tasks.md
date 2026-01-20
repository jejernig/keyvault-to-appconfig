---

description: "Task list template for feature implementation"
---

# Tasks: CLI and Execution Control

**Input**: Design documents from `C:\repos\keyvault-to-appconfig\specs\001-cli-execution-control\`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are REQUIRED for diff logic and redaction safeguards, plus an integration test validating write behavior before release.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `src/`, `tests/` at repository root
- Paths shown below assume single project - adjust based on plan.md structure

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create solution file at `C:\repos\keyvault-to-appconfig\KeyVaultToAppConfig.sln`
- [X] T002 [P] Create CLI project at `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli.csproj`
- [X] T003 [P] Create core library project at `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core.csproj`
- [X] T004 [P] Create services project at `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services.csproj`
- [X] T005 [P] Create unit test project at `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests.csproj`
- [X] T006 [P] Create integration test project at `C:\repos\keyvault-to-appconfig\tests\integration\KeyVaultToAppConfig.IntegrationTests.csproj`
- [X] T007 [P] Create contract test project at `C:\repos\keyvault-to-appconfig\tests\contract\KeyVaultToAppConfig.ContractTests.csproj`
- [X] T008 Add repo-wide formatting rules in `C:\repos\keyvault-to-appconfig\.editorconfig`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**CRITICAL**: No user story work can begin until this phase is complete

- [X] T009 Define CLI options schema in `C:\repos\keyvault-to-appconfig\src\cli\CliOptions.cs`
- [X] T010 Create run configuration model in `C:\repos\keyvault-to-appconfig\src\lib\RunConfiguration.cs`
- [X] T011 [P] Create execution mode enum in `C:\repos\keyvault-to-appconfig\src\lib\ExecutionMode.cs`
- [X] T012 [P] Create validation result model in `C:\repos\keyvault-to-appconfig\src\lib\ValidationResult.cs`
- [X] T013 [P] Create exit code definitions in `C:\repos\keyvault-to-appconfig\src\lib\ExitCodes.cs`
- [X] T014 [P] Create run report model in `C:\repos\keyvault-to-appconfig\src\lib\RunReport.cs`
- [X] T015 [P] Create change summary model in `C:\repos\keyvault-to-appconfig\src\lib\ChangeSummary.cs`
- [X] T016 [P] Create failure summary model in `C:\repos\keyvault-to-appconfig\src\lib\FailureSummary.cs`
- [X] T017 Add deterministic ordering helpers in `C:\repos\keyvault-to-appconfig\src\lib\DeterministicOrdering.cs`
- [X] T018 Implement input validation service in `C:\repos\keyvault-to-appconfig\src\services\RunConfigurationValidator.cs`
- [X] T019 Create execution service scaffold in `C:\repos\keyvault-to-appconfig\src\services\ExecutionService.cs`
- [X] T020 Wire CLI entrypoint skeleton in `C:\repos\keyvault-to-appconfig\src\cli\Program.cs`
- [X] T021 [P] Add output writer abstraction in `C:\repos\keyvault-to-appconfig\src\cli\OutputWriter.cs`
- [X] T022 [P] Add report writer abstraction in `C:\repos\keyvault-to-appconfig\src\cli\ReportWriter.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Run a safe dry-run or diff (Priority: P1)

**Goal**: Provide read-only dry-run and diff modes that never write and report changes deterministically.

**Independent Test**: Run with valid inputs using `--dry-run` or `--diff` and confirm no writes occur.

### Tests for User Story 1

- [X] T023 [P] [US1] Add diff ordering unit tests in `C:\repos\keyvault-to-appconfig\tests\unit\DiffOrderingTests.cs`
- [X] T024 [P] [US1] Add report redaction unit tests in `C:\repos\keyvault-to-appconfig\tests\unit\ReportRedactionTests.cs`

### Implementation for User Story 1

- [X] T025 [US1] Implement dry-run planning flow in `C:\repos\keyvault-to-appconfig\src\services\ExecutionService.cs`
- [X] T026 [US1] Implement diff output formatting in `C:\repos\keyvault-to-appconfig\src\cli\OutputWriter.cs`
- [X] T027 [US1] Serialize dry-run and diff reports in `C:\repos\keyvault-to-appconfig\src\lib\RunReport.cs`
- [X] T028 [US1] Apply deterministic ordering to change summaries in `C:\repos\keyvault-to-appconfig\src\lib\DeterministicOrdering.cs`
- [X] T029 [US1] Support `--dry-run` and `--diff` flags in `C:\repos\keyvault-to-appconfig\src\cli\Program.cs`
- [X] T030 [US1] Write JSON report output in `C:\repos\keyvault-to-appconfig\src\cli\ReportWriter.cs`
- [X] T031 [US1] Emit JSON report when requested in `C:\repos\keyvault-to-appconfig\src\cli\Program.cs`

**Checkpoint**: User Story 1 is fully functional and testable independently

---

## Phase 4: User Story 2 - Apply changes deterministically (Priority: P2)

**Goal**: Apply changes only when explicitly requested, with correct exit codes and idempotent behavior.

**Independent Test**: Run with `--apply` twice against the same inputs and confirm consistent results and exit codes.

### Implementation for User Story 2

- [X] T032 [US2] Implement apply execution path in `C:\repos\keyvault-to-appconfig\src\services\ExecutionService.cs`
- [X] T033 [US2] Enforce apply gating and copy-value confirmation in `C:\repos\keyvault-to-appconfig\src\services\RunConfigurationValidator.cs`
- [X] T034 [US2] Map apply outcomes to exit codes in `C:\repos\keyvault-to-appconfig\src\cli\Program.cs`
- [X] T035 [US2] Render apply results to console in `C:\repos\keyvault-to-appconfig\src\cli\OutputWriter.cs`

**Checkpoint**: User Stories 1 and 2 both work independently

---

## Phase 5: User Story 3 - Validate inputs and guide operators (Priority: P3)

**Goal**: Provide clear validation errors and usage guidance for invalid inputs.

**Independent Test**: Run with missing required args and verify usage output and fatal exit code.

### Implementation for User Story 3

- [X] T036 [US3] Implement usage and help text in `C:\repos\keyvault-to-appconfig\src\cli\HelpText.cs`
- [X] T037 [US3] Render validation errors in `C:\repos\keyvault-to-appconfig\src\cli\OutputWriter.cs`
- [X] T038 [US3] Validate conflicting flags in `C:\repos\keyvault-to-appconfig\src\services\RunConfigurationValidator.cs`
- [X] T039 [US3] Parse required flags in `C:\repos\keyvault-to-appconfig\src\cli\Program.cs`
- [X] T040 [US3] Add optional flag fields in `C:\repos\keyvault-to-appconfig\src\cli\CliOptions.cs`
- [X] T041 [US3] Parse optional flags in `C:\repos\keyvault-to-appconfig\src\cli\Program.cs`
- [X] T042 [US3] Validate optional flags in `C:\repos\keyvault-to-appconfig\src\services\RunConfigurationValidator.cs`

**Checkpoint**: All user stories are independently functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T043 Update CLI quickstart examples (FR-009/FR-010) in `C:\repos\keyvault-to-appconfig\specs\001-cli-execution-control\quickstart.md`
- [X] T044 Add integration test for App Configuration writes in `C:\repos\keyvault-to-appconfig\tests\integration\AppConfigWriteTests.cs`
- [X] T045 Define a basic performance check for 1,000 secrets per run in `C:\repos\keyvault-to-appconfig\specs\001-cli-execution-control\quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 -> P2 -> P3)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - May build on shared components
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - No dependencies on other stories

### Within Each User Story

- Models before services
- Services before CLI integration
- Story complete before moving to next priority

### Parallel Opportunities

- Setup tasks T002-T007 can run in parallel
- Foundational model tasks T011-T016 can run in parallel
- US1 test tasks T023-T024 can run in parallel

---

## Parallel Example: User Story 1

```bash
# Parallel tasks for User Story 1:
Task: "Add diff ordering unit tests in C:\repos\keyvault-to-appconfig\tests\unit\DiffOrderingTests.cs"
Task: "Add report redaction unit tests in C:\repos\keyvault-to-appconfig\tests\unit\ReportRedactionTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. STOP and validate: ensure dry-run and diff are read-only and deterministic

### Incremental Delivery

1. Complete Setup + Foundational -> Foundation ready
2. Add User Story 1 -> Validate independently
3. Add User Story 2 -> Validate independently
4. Add User Story 3 -> Validate independently
5. Update quickstart with final CLI examples
