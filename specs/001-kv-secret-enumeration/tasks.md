---

description: "Task list for Key Vault secret enumeration"
---

# Tasks: Key Vault Secret Enumeration

**Input**: Design documents from `/specs/001-kv-secret-enumeration/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/, quickstart.md

**Tests**: Redaction safeguard coverage added to meet constitution requirements.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and dependencies

- [X] T001 Add Key Vault SDK dependencies to `src/services/KeyVaultToAppConfig.Services/KeyVaultToAppConfig.Services.csproj`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core data structures and CLI configuration used by all user stories

**CRITICAL**: No user story work can begin until this phase is complete

- [X] T002 [P] Create enumeration models in `src/lib/KeyVaultToAppConfig.Core/Enumeration/SecretDescriptor.cs`, `src/lib/KeyVaultToAppConfig.Core/Enumeration/EnumerationFilter.cs`, `src/lib/KeyVaultToAppConfig.Core/Enumeration/VersionSelection.cs`, `src/lib/KeyVaultToAppConfig.Core/Enumeration/EnumerationPage.cs`
- [X] T003 Update `src/lib/KeyVaultToAppConfig.Core/RunConfigurationInput.cs` and `src/lib/KeyVaultToAppConfig.Core/RunConfiguration.cs` to include enumeration options (enabled-only, regex, tags, version mode, version map path, page size, continuation token)
- [X] T004 Update validation rules for enumeration options in `src/services/KeyVaultToAppConfig.Services/RunConfigurationValidator.cs`
- [X] T005 Update CLI options and mappings in `src/cli/KeyVaultToAppConfig.Cli/CliOptions.cs` and `src/cli/KeyVaultToAppConfig.Cli/Program.cs`
- [X] T006 Update CLI usage text in `src/cli/KeyVaultToAppConfig.Cli/HelpText.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Enumerate secrets deterministically (Priority: P1)

**Goal**: Enumerate secrets with paging and deterministic ordering without retrieving secret values.

**Independent Test**: Run enumeration twice with the same inputs and verify identical ordered results.

### Tests for User Story 1

- [X] T007 [P] [US1] Add redaction regression test for enumeration output in `tests/unit/KeyVaultToAppConfig.UnitTests/EnumerationRedactionTests.cs`

### Implementation for User Story 1

- [X] T008 [US1] Implement Key Vault paging and bounded retry logic in `src/services/KeyVaultToAppConfig.Services/KeyVaultSecretEnumerator.cs`
- [X] T009 [P] [US1] Add deterministic ordering helper for secret descriptors in `src/lib/KeyVaultToAppConfig.Core/SecretOrdering.cs`
- [X] T010 [US1] Extend JSON report output to include enumerated secret metadata in `src/lib/KeyVaultToAppConfig.Core/RunReport.cs` and `src/cli/KeyVaultToAppConfig.Cli/ReportWriter.cs`
- [X] T011 [US1] Wire enumeration into execution flow in `src/services/KeyVaultToAppConfig.Services/ExecutionService.cs`
- [X] T012 [US1] Emit enumeration output in CLI and JSON reports via `src/cli/KeyVaultToAppConfig.Cli/OutputWriter.cs` and `src/cli/KeyVaultToAppConfig.Cli/ReportWriter.cs`
- [X] T013 [US1] Configure retry defaults in `src/services/KeyVaultToAppConfig.Services/KeyVaultSecretEnumerator.cs`

**Checkpoint**: User Story 1 is functional and deterministic

---

## Phase 4: User Story 2 - Filter secrets precisely (Priority: P2)

**Goal**: Support prefix, regex, tag, and enabled-only filters with clear validation.

**Independent Test**: Apply each filter independently and verify only matching secrets are returned.

### Implementation for User Story 2

- [X] T014 [P] [US2] Add tag filter parsing helper in `src/lib/KeyVaultToAppConfig.Core/Enumeration/TagFilter.cs`
- [X] T015 [US2] Apply prefix, regex, tag, and enabled-only filters in `src/services/KeyVaultToAppConfig.Services/KeyVaultSecretEnumerator.cs`
- [X] T016 [US2] Map CLI filter options into an EnumerationFilter in `src/services/KeyVaultToAppConfig.Services/EnumerationFilterBuilder.cs`

**Checkpoint**: User Story 2 filters work independently

---

## Phase 5: User Story 3 - Select versions safely (Priority: P3)

**Goal**: Support latest-only and explicit version selection without extra data exposure.

**Independent Test**: Run enumeration in latest-only and explicit version modes and compare outputs.

### Implementation for User Story 3

- [X] T017 [US3] Parse version selection inputs in `src/services/KeyVaultToAppConfig.Services/VersionSelectionBuilder.cs`
- [X] T018 [US3] Apply version selection strategy in `src/services/KeyVaultToAppConfig.Services/KeyVaultSecretEnumerator.cs`

**Checkpoint**: User Story 3 version selection works independently

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and documentation touchups

- [X] T019 [P] Confirm quickstart examples align with CLI flags in `specs/001-kv-secret-enumeration/quickstart.md`
- [ ] T020 Validate SC-004 performance run against 1,000+ secrets (document results in `specs/001-kv-secret-enumeration/quickstart.md`)
- [ ] T021 Validate SC-001 to SC-003 outcomes and document results in `specs/001-kv-secret-enumeration/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - blocks all user stories
- **User Stories (Phase 3+)**: Depend on Foundational phase completion
- **Polish (Final Phase)**: Depends on desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Foundational phase
- **User Story 2 (P2)**: Starts after Foundational phase; builds on US1 output
- **User Story 3 (P3)**: Starts after Foundational phase; can run after US1

### Parallel Opportunities

- T002, T007, T009, T014, T019 can run in parallel with other tasks in their phases

---

## Parallel Example: User Story 1

```bash
Task: "Add deterministic ordering helper for secret descriptors in src/lib/KeyVaultToAppConfig.Core/SecretOrdering.cs"
Task: "Implement Key Vault paging and bounded retry logic in src/services/KeyVaultToAppConfig.Services/KeyVaultSecretEnumerator.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. Validate deterministic enumeration output

### Incremental Delivery

1. Setup + Foundational
2. User Story 1 -> validate
3. User Story 2 -> validate
4. User Story 3 -> validate

---

## Notes

- Tasks are scoped to metadata-only enumeration (no secret values)
- Keep retry behavior bounded and deterministic
- Avoid logging or writing secret values
