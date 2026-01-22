# Tasks: Testing & Quality Gates

**Input**: Design documents from `/specs/001-testing-quality-gates/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: This feature defines test strategy and CI gating; tasks focus on documenting scope, updating templates, and adding missing test coverage where required.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `src/`, `tests/` at repository root
- Paths below assume the existing CLI + shared libraries structure

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish test strategy scaffolding

- [X] T001 Create testing strategy doc in `docs/testing.md`
- [X] T002 Add CI gate summary section in `docs/testing.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core definitions required before user story execution

**CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 Define high-risk path catalog in `docs/testing.md`
- [X] T004 Define unit/integration/security scope matrix in `docs/testing.md`

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Reliable Unit Coverage (Priority: P1) MVP

**Goal**: Document and verify unit test scope coverage for critical logic

**Independent Test**: Review `docs/testing.md` and confirm mapping/diff/redaction/error/exit code coverage is specified.

### Tests for User Story 1

- [X] T005 [P] [US1] Add coverage checklist for mapping/diff/redaction in `docs/testing.md`
- [X] T006 [P] [US1] Add unit test run instructions in `docs/testing.md`

### Implementation for User Story 1

- [X] T007 [US1] Create unit test README in `tests/unit/KeyVaultToAppConfig.UnitTests/README.md`
- [X] T008 [US1] Map unit test coverage to risk paths in `docs/testing.md`
- [X] T009 [US1] Verify redaction test coverage in `tests/unit/KeyVaultToAppConfig.UnitTests/Secrets/` and document evidence in `docs/testing.md`

**Checkpoint**: User Story 1 functional and testable

---

## Phase 4: User Story 2 - Integration Confidence (Priority: P2)

**Goal**: Define integration strategy and release gates

**Independent Test**: Verify integration strategy, prerequisites, and gating rules are documented.

### Tests for User Story 2

- [X] T010 [P] [US2] Document integration test prerequisites in `docs/testing.md`
- [X] T011 [P] [US2] Document integration gate/waiver policy in `docs/testing.md`

### Implementation for User Story 2

- [X] T012 [US2] Add integration test run instructions in `docs/testing.md`
- [X] T013 [US2] Add CI gate checklist and enforcement steps in `docs/testing.md`

**Checkpoint**: User Stories 1 and 2 independently functional

---

## Phase 5: User Story 3 - Security & Redaction Assurance (Priority: P3)

**Goal**: Define security/redaction test scope and coverage for high-risk paths

**Independent Test**: Confirm redaction and guardrail tests are explicitly required.

### Tests for User Story 3

- [X] T014 [P] [US3] Document redaction test scenarios in `docs/testing.md`
- [X] T015 [P] [US3] Document copy-value guardrail test scenarios in `docs/testing.md`

### Implementation for User Story 3

- [X] T016 [US3] Add security verification checklist in `docs/testing.md`

**Checkpoint**: All user stories functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Ensure docs and templates are aligned

- [X] T017 [P] Link testing strategy from `docs/onboarding.md`
- [X] T018 Update `docs/cli-reference.md` to reference testing strategy (if needed)
- [X] T019 Run quickstart validation and note results in `specs/001-testing-quality-gates/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - blocks all user stories
- **User Stories (Phase 3+)**: Depend on Foundational completion
- **Polish (Final Phase)**: Depends on user stories completion

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Foundational, no dependencies
- **User Story 2 (P2)**: Starts after Foundational, no dependencies
- **User Story 3 (P3)**: Starts after Foundational, no dependencies

### Within Each User Story

- Documentation tasks before checklist tasks
- Gates before release checklist

### Parallel Opportunities

- T005 and T006 can run in parallel
- T010 and T011 can run in parallel
- T014 and T015 can run in parallel

---

## Parallel Example: User Story 1

```bash
Task: "Add coverage checklist for mapping/diff/redaction in docs/testing.md"
Task: "Add unit test run instructions in docs/testing.md"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. Validate testing doc scope

### Incremental Delivery

1. Setup + Foundational
2. User Story 1 -> validate
3. User Story 2 -> validate
4. User Story 3 -> validate
5. Polish updates
