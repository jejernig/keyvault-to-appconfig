# Research Notes: Key Vault Secret Enumeration

## Decision 1: Paging strategy
- **Decision**: Use Key Vault paged listing and process results incrementally.
- **Rationale**: Supports large vaults without loading all secrets at once.
- **Alternatives considered**: Fetch-all then filter; rejected for memory and
  latency concerns.

## Decision 2: Filtering order
- **Decision**: Apply prefix and enabled-only filters first, then regex and tag
  filters to minimize processing cost.
- **Rationale**: Reduces candidate set early for performance and determinism.
- **Alternatives considered**: Apply all filters after paging; rejected for
  unnecessary processing.

## Decision 3: Version selection
- **Decision**: Default to latest-only; allow explicit version selection per
  secret when provided.
- **Rationale**: Limits data exposure and reduces enumeration load.
- **Alternatives considered**: Enumerate all versions by default; rejected for
  performance and risk.

## Decision 4: Throttling and retries
- **Decision**: Use bounded retries with exponential backoff on transient
  throttling errors.
- **Rationale**: Improves resilience while avoiding long hangs.
- **Alternatives considered**: No retries or unlimited retries; rejected for
  fragility or unpredictability.

## Decision 5: Deterministic ordering
- **Decision**: Sort output by secret name and version to ensure stable order.
- **Rationale**: Enables repeatable diffs and CI/CD comparisons.
- **Alternatives considered**: Preserve service order; rejected due to
  non-determinism.
