---

description: "Task list for Mapping & Normalization Engine"
---

# Tasks: Mapping & Normalization Engine

**Input**: Design documents from `C:\repos\keyvault-to-appconfig\specs\001-mapping-normalization-engine\`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Unit tests included for mapping, validation, collision logic, diff logic, and redaction safeguards.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create mapping module folders in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Mapping\` and `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Mapping\`
- [X] T002 [P] Create CLI command folders in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\`
- [X] T003 [P] Create unit test folder in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Mapping\`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Define MappingSpecification model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Mapping\MappingSpecification.cs`
- [X] T005 [P] Define MappingRule model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Mapping\MappingRule.cs`
- [X] T006 [P] Define TransformOperation model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Mapping\TransformOperation.cs`
- [X] T007 [P] Define collision models in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Mapping\CollisionReport.cs`
- [X] T008 Define validation error model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Mapping\SpecValidationError.cs`
- [X] T009 Define deterministic ordering helper in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Mapping\DeterministicOrder.cs`
- [X] T010 Define transform catalog in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Mapping\TransformCatalog.cs`
- [X] T011 Add mapping spec schema file in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Mapping\Schema\mapping-spec.schema.json`
- [X] T012 Define mapping service interfaces in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Mapping\IMappingSpecStore.cs`, `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Mapping\IMappingValidator.cs`, `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Mapping\IMappingEngine.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Author mapping specification (Priority: P1) MVP

**Goal**: Accept YAML/JSON mapping specs and persist them as the authoritative definition.

**Independent Test**: Submit a YAML or JSON spec and verify it is accepted and stored with version metadata.

### Implementation for User Story 1

- [X] T013 [P] [US1] Implement YAML/JSON loader in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Mapping\MappingSpecLoader.cs`
- [X] T014 [P] [US1] Implement file-based spec store in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Mapping\MappingSpecStore.cs`
- [X] T015 [US1] Wire spec submit command in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\MappingSpecCommand.cs`
- [X] T016 [US1] Add unit tests for loader in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Mapping\MappingSpecLoaderTests.cs`
- [X] T017 [US1] Add unit tests for spec store in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Mapping\MappingSpecStoreTests.cs`

**Checkpoint**: User Story 1 is fully functional and testable independently

---

## Phase 4: User Story 2 - Validate and explain invalid specs (Priority: P2)

**Goal**: Validate specs against schema and provide actionable error messages.

**Independent Test**: Submit invalid specs and receive precise, field-level errors.

### Implementation for User Story 2

- [X] T018 [P] [US2] Implement schema validator in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Mapping\MappingSpecValidator.cs`
- [X] T019 [US2] Add validation result model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Mapping\MappingValidationResult.cs`
- [X] T020 [US2] Wire spec validation command in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\MappingValidateCommand.cs`
- [X] T021 [P] [US2] Add validator tests in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Mapping\MappingSpecValidatorTests.cs`

**Checkpoint**: User Story 2 is fully functional and testable independently

---

## Phase 5: User Story 3 - Detect collisions and ensure deterministic outcomes (Priority: P3)

**Goal**: Execute mapping runs deterministically and produce collision reports.

**Independent Test**: Run the same source set multiple times and confirm identical output and collision reporting.

### Implementation for User Story 3

- [X] T022 [P] [US3] Implement mapping engine in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Mapping\MappingEngine.cs`
- [X] T023 [P] [US3] Implement collision reporter in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Mapping\CollisionReporter.cs`
- [X] T024 [US3] Add mapping run record in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Mapping\MappingRun.cs`
- [X] T025 [US3] Wire mapping run command in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\MappingRunCommand.cs`
- [X] T026 [P] [US3] Add engine tests for determinism and collisions in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Mapping\MappingEngineTests.cs`
- [X] T027 [P] [US3] Add collision report tests in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Mapping\CollisionReporterTests.cs`

**Checkpoint**: User Story 3 is fully functional and testable independently

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T028 [P] Update quickstart examples to match CLI commands in `C:\repos\keyvault-to-appconfig\specs\001-mapping-normalization-engine\quickstart.md`
- [X] T029 [P] Add edge case tests for unsupported transforms and overlapping regex in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Mapping\MappingEdgeCaseTests.cs`
- [X] T030 Add structured, redacted logging for mapping operations in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Mapping\MappingLogging.cs`
- [X] T031 [P] Add diff logic unit tests in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Mapping\DiffLogicTests.cs`
- [X] T032 [P] Add redaction safeguard unit tests in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Mapping\RedactionSafeguardTests.cs`

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
- Phase 2 tasks T005, T006, T007 can run in parallel
- Phase 3 tasks T013 and T014 can run in parallel
- Phase 4 tasks T018 and T021 can run in parallel
- Phase 5 tasks T022 and T023 can run in parallel

---

## Parallel Example: User Story 1

```text
Task: "Implement YAML/JSON loader in C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Mapping\MappingSpecLoader.cs"
Task: "Implement file-based spec store in C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Mapping\MappingSpecStore.cs"
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
