---

description: "Task list for Secret Handling Modes"
---

# Tasks: Secret Handling Modes

**Input**: Design documents from `C:\repos\keyvault-to-appconfig\specs\001-secret-handling-modes\`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Unit tests included for guardrails and redaction safeguards.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create secret handling module folder in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Secrets\`
- [X] T002 [P] Create CLI command folder in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\`
- [X] T003 [P] Create unit test folder in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Secrets\`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Define secret handling mode model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Secrets\SecretHandlingMode.cs`
- [X] T005 [P] Define secret URI resolution model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Secrets\SecretUriResolution.cs`
- [X] T006 [P] Define copy-value guardrail model in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Secrets\CopyValueGuardrail.cs`
- [X] T007 [P] Define redaction policy model (value and optional name redaction) in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Secrets\RedactionPolicy.cs`
- [X] T008 [P] Define mode execution result model (correlation ID, guardrail status, allowed-key enforcement, resolved version metadata) in `C:\repos\keyvault-to-appconfig\src\lib\KeyVaultToAppConfig.Core\Secrets\ModeExecutionResult.cs`
- [X] T009 Define secret handling service interfaces in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Secrets\ISecretModeEvaluator.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Use Key Vault reference mode safely (Priority: P1) MVP

**Goal**: Default to reference mode and build Key Vault references without exposing secret values.

**Independent Test**: Run in reference mode with sample URIs and verify only references are produced and no secrets are logged.

### Implementation for User Story 1

- [X] T010 [P] [US1] Implement secret URI validator and resolver in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Secrets\SecretUriResolver.cs`
- [X] T011 [US1] Propagate resolved version metadata into mode results in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Secrets\SecretModeEvaluator.cs`
- [X] T012 [US1] Implement reference mode evaluator in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Secrets\SecretModeEvaluator.cs`
- [X] T013 [US1] Wire mode evaluation command in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\SecretModeCommand.cs`
- [X] T014 [P] [US1] Add unit tests for reference mode and URI resolution in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Secrets\SecretUriResolverTests.cs`

**Checkpoint**: User Story 1 is fully functional and testable independently

---

## Phase 4: User Story 2 - Use copy-value mode with guardrails (Priority: P2)

**Goal**: Require explicit confirmations and restrict copy-value scope.

**Independent Test**: Attempt copy-value without confirmations and verify it is blocked, then allow with confirmations.

### Implementation for User Story 2

- [X] T015 [P] [US2] Implement copy-value guardrail evaluator in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Secrets\CopyValueGuardrailEvaluator.cs`
- [X] T016 [US2] Wire guardrail checks into mode evaluator in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Secrets\SecretModeEvaluator.cs`
- [X] T017 [US2] Surface allowed-key enforcement results in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Secrets\SecretModeEvaluator.cs`
- [X] T018 [US2] Add copy-value command options and allowed-key reporting in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\SecretModeCommand.cs`
- [X] T019 [P] [US2] Add unit tests for guardrails and allowed keys in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Secrets\CopyValueGuardrailTests.cs`

**Checkpoint**: User Story 2 is fully functional and testable independently

---

## Phase 5: User Story 3 - Prove redaction and audit safety (Priority: P3)

**Goal**: Guarantee redaction across logs, reports, and error messages.

**Independent Test**: Generate logs/reports with secret-like input and verify redaction.

### Implementation for User Story 3

- [X] T020 [P] [US3] Implement redaction policy enforcement (value and optional name redaction) in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Secrets\SecretRedaction.cs`
- [X] T021 [US3] Wire redaction into mode evaluator outputs in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Secrets\SecretModeEvaluator.cs`
- [X] T022 [P] [US3] Add unit tests for redaction guarantees (including name redaction when enabled) in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Secrets\SecretRedactionTests.cs`

**Checkpoint**: User Story 3 is fully functional and testable independently

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T023 [P] Add correlation ID and structured logging for secret handling decisions in `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Secrets\SecretLogging.cs`
- [X] T024 [P] Add secret handling report output with correlation ID, per-item outcomes, and aggregate counts in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\ReportWriter.cs`
- [X] T025 [P] Add exit code mapping for secret handling outcomes in `C:\repos\keyvault-to-appconfig\src\cli\KeyVaultToAppConfig.Cli\Commands\SecretModeCommand.cs`
- [X] T026 [P] Add unit tests for report aggregates and exit codes in `C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Secrets\SecretModeCommandTests.cs`
- [X] T027 [P] Update quickstart to match secret mode command, exit codes, and performance validation scenario in `C:\repos\keyvault-to-appconfig\specs\001-secret-handling-modes\quickstart.md`

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
- Phase 2 tasks T005 through T008 can run in parallel
- Phase 3 tasks T010 and T014 can run in parallel
- Phase 4 tasks T015 and T019 can run in parallel
- Phase 5 tasks T020 and T022 can run in parallel

---

## Parallel Example: User Story 1

```text
Task: "Implement secret URI validator and resolver in C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services\Secrets\SecretUriResolver.cs"
Task: "Add unit tests for reference mode and URI resolution in C:\repos\keyvault-to-appconfig\tests\unit\KeyVaultToAppConfig.UnitTests\Secrets\SecretUriResolverTests.cs"
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
