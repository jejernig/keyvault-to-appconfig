# Research Notes: Planning & Diff Engine

## Decision 1: Desired state construction
- Decision: Build desired state from mapping specifications, input sources, and environment labels into a canonical key/label/value set.
- Rationale: A canonical model ensures deterministic diffs and consistent reviews.
- Alternatives considered: Separate models per source type; direct per-source diffs.

## Decision 2: Existing state reads
- Decision: Read existing App Configuration state for the targeted key prefix and label scope before diffing.
- Rationale: Limits scope and avoids unnecessary reads while keeping diffs accurate.
- Alternatives considered: Full namespace reads; on-demand reads per key.

## Decision 3: Diff classification rules
- Decision: Compare key, label, value, and content type to determine create, update, or unchanged.
- Rationale: Prevents false positives and aligns with App Configuration semantics.
- Alternatives considered: Value-only comparisons; label-only comparisons.

## Decision 4: Conflict and collision handling
- Decision: Detect conflicting desired entries (same key/label different values) and record conflicts without writing.
- Rationale: Surfaces ambiguity early and prevents unsafe writes.
- Alternatives considered: Last-wins resolution; silent deduplication.

## Decision 5: Dry-run behavior
- Decision: Dry-run produces the same plan output as planning mode and performs zero writes.
- Rationale: Ensures safe review and automation compliance.
- Alternatives considered: Dry-run with partial apply; write with rollback.

## Decision 6: Deterministic ordering
- Decision: Output ordering follows deterministic key + label ordering to ensure stable plans.
- Rationale: Stable output enables repeatable reviews and diffs.
- Alternatives considered: Input order only; randomized ordering.
