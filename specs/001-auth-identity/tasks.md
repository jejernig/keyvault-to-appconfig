---

description: "Task list template for feature implementation"
---

# Tasks: Authentication and Identity

**Input**: Design documents from `C:\repos\keyvault-to-appconfig\specs\001-auth-identity\`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are REQUIRED for mapping, diff logic, and redaction safeguards, plus integration tests when touching live Azure services or contracts.

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

- [X] T001 Add Azure.Identity package to `C:\repos\keyvault-to-appconfig\src\services\KeyVaultToAppConfig.Services.csproj`
- [X] T002 [P] Add shared auth abstractions folder at `C:\repos\keyvault-to-appconfig\src\lib\Auth\`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 Define credential source enum in `C:\repos\keyvault-to-appconfig\src\lib\Auth\CredentialSource.cs`
- [X] T004 Create credential resolution policy model in `C:\repos\keyvault-to-appconfig\src\lib\Auth\CredentialResolutionPolicy.cs`
- [X] T005 Create auth result model in `C:\repos\keyvault-to-appconfig\src\lib\Auth\AuthResult.cs`
- [X] T006 Implement auth resolver service in `C:\repos\keyvault-to-appconfig\src\services\AuthResolver.cs`
- [X] T007 Add auth diagnostics helper in `C:\repos\keyvault-to-appconfig\src\services\AuthDiagnostics.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Authenticate using preferred identity (Priority: P1)

**Goal**: Resolve credentials in deterministic order and authenticate without local secrets when managed identity is available.

**Independent Test**: Run in a managed identity environment and verify authentication succeeds without local secrets.

### Tests for User Story 1

- [X] T008 [P] [US1] Add credential resolution unit tests in `C:\repos\keyvault-to-appconfig\tests\unit\CredentialResolutionTests.cs`

### Implementation for User Story 1

- [X] T009 [US1] Implement deterministic credential selection in `C:\repos\keyvault-to-appconfig\src\services\AuthResolver.cs`
- [X] T010 [US1] Wire auth resolver into execution flow in `C:\repos\keyvault-to-appconfig\src\services\ExecutionService.cs`
- [X] T011 [US1] Document credential order in `C:\repos\keyvault-to-appconfig\specs\001-auth-identity\quickstart.md`

**Checkpoint**: User Story 1 is fully functional and testable independently

---

## Phase 4: User Story 2 - Fail safely with clear diagnostics (Priority: P2)

**Goal**: Provide explicit failure reasons and bounded retry behavior.

**Independent Test**: Run with no valid credentials and confirm fatal error with actionable diagnostics.

### Tests for User Story 2

- [X] T012 [P] [US2] Add auth failure diagnostics tests in `C:\repos\keyvault-to-appconfig\tests\unit\AuthDiagnosticsTests.cs`
- [X] T013 [P] [US2] Add no-credential failure tests in `C:\repos\keyvault-to-appconfig\tests\unit\AuthDiagnosticsTests.cs`
- [X] T014 [P] [US2] Add retry policy unit tests in `C:\repos\keyvault-to-appconfig\tests\unit\AuthRetryPolicyTests.cs`
- [X] T015 [P] [US2] Add auth redaction regression test in `C:\repos\keyvault-to-appconfig\tests\unit\ReportRedactionTests.cs`

### Implementation for User Story 2

- [X] T016 [US2] Implement bounded retry policy in `C:\repos\keyvault-to-appconfig\src\services\AuthResolver.cs`
- [X] T017 [US2] Surface auth failure messages in CLI output in `C:\repos\keyvault-to-appconfig\src\cli\OutputWriter.cs`
- [X] T018 [US2] Document RBAC assumptions in `C:\repos\keyvault-to-appconfig\specs\001-auth-identity\quickstart.md`

**Checkpoint**: User Stories 1 and 2 both work independently

---

## Phase 5: User Story 3 - Support local development (Priority: P3)

**Goal**: Allow local developer credentials without hard-coded secrets.

**Independent Test**: Run locally with developer credentials configured and verify authentication succeeds without embedded secrets.

### Tests for User Story 3

- [X] T019 [P] [US3] Add local developer auth tests in `C:\repos\keyvault-to-appconfig\tests\unit\LocalDevAuthTests.cs`
- [X] T020 [P] [US3] Add diff ordering regression test in `C:\repos\keyvault-to-appconfig\tests\unit\DiffOrderingTests.cs`
- [X] T021 [P] [US3] Add mapping test placeholder in `C:\repos\keyvault-to-appconfig\tests\unit\MappingTests.cs`

### Implementation for User Story 3

- [X] T022 [US3] Enforce rejection of hard-coded secrets in config in `C:\repos\keyvault-to-appconfig\src\services\AuthDiagnostics.cs`
- [X] T023 [US3] Document local dev auth setup in `C:\repos\keyvault-to-appconfig\specs\001-auth-identity\quickstart.md`

**Checkpoint**: All user stories are independently functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T024 Add integration test placeholder for auth with live Azure identity in `C:\repos\keyvault-to-appconfig\tests\integration\AuthIntegrationTests.cs`

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
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Builds on auth resolver core
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - No dependencies on other stories

### Within Each User Story

- Models before services
- Services before CLI integration
- Story complete before moving to next priority

### Parallel Opportunities

- Setup task T002 can run in parallel with T001
- Foundational models T003-T005 can run in parallel
- US1 tests and docs tasks can run in parallel with implementation

---

## Parallel Example: User Story 1

```bash
# Parallel tasks for User Story 1:
Task: "Add credential resolution unit tests in C:\repos\keyvault-to-appconfig\tests\unit\CredentialResolutionTests.cs"
Task: "Document credential order in C:\repos\keyvault-to-appconfig\specs\001-auth-identity\quickstart.md"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. STOP and validate: authentication uses managed identity without secrets

### Incremental Delivery

1. Complete Setup + Foundational -> Foundation ready
2. Add User Story 1 -> Validate independently
3. Add User Story 2 -> Validate independently
4. Add User Story 3 -> Validate independently
5. Add integration test placeholder for live identity
